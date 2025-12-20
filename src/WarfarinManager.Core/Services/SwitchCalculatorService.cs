using WarfarinManager.Core.Models;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Implementazione del servizio di calcolo protocolli switch Warfarin ↔ DOAC
/// Basato su linee guida ESC/EHRA 2021 e Nota AIFA 97
/// </summary>
public class SwitchCalculatorService : ISwitchCalculatorService
{
    public SwitchProtocol CalculateProtocol(
        SwitchDirection direction,
        DoacType doacType,
        WarfarinType warfarinType,
        SwitchPatientParameters patientParameters)
    {
        var protocol = new SwitchProtocol
        {
            Direction = direction,
            DoacType = doacType,
            WarfarinType = warfarinType,
            CalculatedAt = DateTime.Now
        };

        // Validazione parametri
        var (isValid, errors) = ValidateParameters(patientParameters);
        if (!isValid)
        {
            protocol.IsSafeToSwitch = false;
            protocol.Contraindications.AddRange(errors);
            return protocol;
        }

        // Verifica controindicazioni
        var contraindications = CheckContraindications(doacType, patientParameters);
        protocol.Contraindications.AddRange(contraindications);

        // Verifica warnings
        var warnings = CheckWarnings(doacType, warfarinType, patientParameters);
        protocol.Warnings.AddRange(warnings);

        // Lo switch è sicuro se non ci sono controindicazioni assolute
        protocol.IsSafeToSwitch = contraindications.Count == 0;

        if (!protocol.IsSafeToSwitch)
        {
            return protocol;
        }

        // Calcola dosaggio DOAC
        var (dosage, rationale) = CalculateDoacDosage(doacType, patientParameters);
        protocol.RecommendedDoacDosage = dosage;
        protocol.DosageRationale = rationale;

        // Genera protocollo specifico per direzione
        if (direction == SwitchDirection.WarfarinToDoac)
        {
            GenerateWarfarinToDoacProtocol(protocol, doacType, warfarinType, patientParameters);
        }
        else
        {
            GenerateDoacToWarfarinProtocol(protocol, doacType, warfarinType, patientParameters);
        }

        // Genera piano di monitoraggio
        protocol.MonitoringPlan = GenerateMonitoringPlan(direction, doacType);

        return protocol;
    }

    public (bool IsValid, List<string> Errors) ValidateParameters(SwitchPatientParameters parameters)
    {
        var errors = new List<string>();

        if (parameters.Age <= 0 || parameters.Age > 120)
            errors.Add("Età non valida");

        if (parameters.Weight <= 0 || parameters.Weight > 300)
            errors.Add("Peso non valido");

        if (parameters.CreatinineClearance < 0)
            errors.Add("Clearance creatinina non valida");

        if (string.IsNullOrEmpty(parameters.Gender))
            errors.Add("Sesso non specificato");

        return (errors.Count == 0, errors);
    }

    public (string Dosage, string Rationale) CalculateDoacDosage(DoacType doacType, SwitchPatientParameters parameters)
    {
        var clcr = parameters.CreatinineClearance;
        var age = parameters.Age;
        var weight = parameters.Weight;

        return doacType switch
        {
            DoacType.Apixaban => CalculateApixabanDosage(age, weight, clcr, parameters),
            DoacType.Rivaroxaban => CalculateRivaroxabanDosage(clcr),
            DoacType.Dabigatran => CalculateDabigatranDosage(age, clcr),
            DoacType.Edoxaban => CalculateEdoxabanDosage(weight, clcr),
            _ => ("Non determinato", "Tipo DOAC non riconosciuto")
        };
    }

    private (string Dosage, string Rationale) CalculateApixabanDosage(int age, decimal weight, decimal clcr, SwitchPatientParameters parameters)
    {
        // Criteri ABC per riduzione dose Apixaban:
        // Almeno 2 di: Age ≥80, Body weight ≤60kg, Creatinine ≥1.5 mg/dL
        int criteriaCount = 0;
        var reasons = new List<string>();

        if (age >= 80)
        {
            criteriaCount++;
            reasons.Add("età ≥80 anni");
        }

        if (weight <= 60)
        {
            criteriaCount++;
            reasons.Add("peso ≤60 kg");
        }

        if (parameters.SerumCreatinine.HasValue && parameters.SerumCreatinine.Value >= 1.5m)
        {
            criteriaCount++;
            reasons.Add("creatinina ≥1.5 mg/dL");
        }

        if (criteriaCount >= 2)
        {
            return ("2.5 mg BID (due volte al giorno)",
                    $"Dose ridotta per criteri ABC: {string.Join(", ", reasons)}");
        }

        return ("5 mg BID (due volte al giorno)", "Dose standard");
    }

    private (string Dosage, string Rationale) CalculateRivaroxabanDosage(decimal clcr)
    {
        if (clcr < 15)
            return ("Controindicato", "ClCr <15 mL/min");

        if (clcr >= 15 && clcr < 50)
            return ("15 mg una volta al giorno (con il pasto)",
                    $"Dose ridotta per ClCr {clcr:F1} mL/min (15-49 mL/min)");

        return ("20 mg una volta al giorno (con il pasto)", "Dose standard");
    }

    private (string Dosage, string Rationale) CalculateDabigatranDosage(int age, decimal clcr)
    {
        if (clcr < 30)
            return ("Controindicato", "ClCr <30 mL/min (controindicazione assoluta per Dabigatran)");

        if (age >= 80)
            return ("110 mg BID (due volte al giorno)",
                    "Dose ridotta per età ≥80 anni");

        if (clcr >= 30 && clcr <= 50)
            return ("110 mg BID (due volte al giorno)",
                    $"Dose ridotta per ClCr {clcr:F1} mL/min (30-50 mL/min)");

        // Età 75-79 con alto rischio emorragico: considerare 110mg
        if (age >= 75 && age < 80)
            return ("150 mg BID (due volte al giorno) - valutare 110 mg se alto rischio emorragico",
                    "Dose standard, ma considerare riduzione per età 75-79 anni");

        return ("150 mg BID (due volte al giorno)", "Dose standard");
    }

    private (string Dosage, string Rationale) CalculateEdoxabanDosage(decimal weight, decimal clcr)
    {
        if (clcr < 15)
            return ("Controindicato", "ClCr <15 mL/min");

        if (clcr > 95)
            return ("Non raccomandato in FA",
                    "ClCr >95 mL/min: efficacia ridotta nella fibrillazione atriale. Considerare altro DOAC.");

        var reasons = new List<string>();
        bool reduceDose = false;

        if (clcr >= 30 && clcr <= 50)
        {
            reduceDose = true;
            reasons.Add($"ClCr {clcr:F1} mL/min (30-50 mL/min)");
        }

        if (weight <= 60)
        {
            reduceDose = true;
            reasons.Add("peso ≤60 kg");
        }

        if (reduceDose)
            return ("30 mg una volta al giorno",
                    $"Dose ridotta per: {string.Join(", ", reasons)}");

        return ("60 mg una volta al giorno", "Dose standard");
    }

    public List<string> CheckContraindications(DoacType doacType, SwitchPatientParameters parameters)
    {
        var contraindications = new List<string>();

        // Controindicazioni assolute comuni a tutti i DOAC
        if (parameters.HasMechanicalValves)
            contraindications.Add("❌ CONTROINDICAZIONE ASSOLUTA: Presenza di valvole meccaniche. I DOAC sono controindicati.");

        if (parameters.HasMitralStenosis)
            contraindications.Add("❌ CONTROINDICAZIONE ASSOLUTA: Stenosi mitralica moderata/severa. I DOAC sono controindicati.");

        if (parameters.IsPregnantOrBreastfeeding)
            contraindications.Add("❌ CONTROINDICAZIONE ASSOLUTA: Gravidanza o allattamento. I DOAC sono controindicati.");

        // Controindicazioni specifiche per funzione renale
        var clcr = parameters.CreatinineClearance;

        if (doacType == DoacType.Dabigatran && clcr < 30)
            contraindications.Add($"❌ CONTROINDICAZIONE ASSOLUTA: ClCr {clcr:F1} mL/min. Dabigatran controindicato se ClCr <30 mL/min.");

        if (clcr < 15)
            contraindications.Add($"❌ CONTROINDICAZIONE ASSOLUTA: ClCr {clcr:F1} mL/min. Tutti i DOAC sono controindicati se ClCr <15 mL/min.");

        // Sindrome antifosfolipidi
        if (parameters.HasAntiphospholipidSyndrome)
            contraindications.Add("⚠️ CONTROINDICAZIONE RELATIVA: Sindrome da antifosfolipidi. I DOAC hanno mostrato risultati inferiori a Warfarin in APS ad alto rischio (tripla positività).");

        // Edoxaban e ClCr >95 in FA
        if (doacType == DoacType.Edoxaban && clcr > 95)
            contraindications.Add($"⚠️ NON RACCOMANDATO: ClCr {clcr:F1} mL/min >95. Edoxaban ha efficacia ridotta in FA con ClCr molto elevata. Scegliere altro DOAC.");

        return contraindications;
    }

    public List<string> CheckWarnings(DoacType doacType, WarfarinType warfarinType, SwitchPatientParameters parameters)
    {
        var warnings = new List<string>();

        // Warning per peso
        if (parameters.Weight > 120)
            warnings.Add($"⚠️ ATTENZIONE: Peso {parameters.Weight} kg >120 kg. Evidenza limitata per DOAC in pazienti con peso molto elevato. Considerare monitoraggio più stretto.");

        if (parameters.Weight < 50)
            warnings.Add($"⚠️ ATTENZIONE: Peso {parameters.Weight} kg <50 kg. Rischio di concentrazioni plasmatiche più elevate. Considerare dosaggio ridotto e monitoraggio.");

        // Warning per età avanzata
        if (parameters.Age >= 85)
            warnings.Add($"⚠️ ATTENZIONE: Età {parameters.Age} anni. Paziente molto anziano: valutare rischio emorragico, aderenza terapeutica e rischio cadute.");

        // Warning per funzione renale borderline
        var clcr = parameters.CreatinineClearance;
        if (clcr >= 15 && clcr < 30 && doacType != DoacType.Dabigatran)
            warnings.Add($"⚠️ ATTENZIONE: ClCr {clcr:F1} mL/min. Funzione renale severamente ridotta. Monitorare frequentemente la funzione renale (ogni 3 mesi).");

        if (clcr >= 30 && clcr < 50)
            warnings.Add($"⚠️ ATTENZIONE: ClCr {clcr:F1} mL/min. Insufficienza renale moderata. Richiesto aggiustamento dose e monitoraggio funzione renale ogni 6 mesi.");

        // Warning Rivaroxaban - assunzione con cibo
        if (doacType == DoacType.Rivaroxaban)
            warnings.Add("ℹ️ IMPORTANTE: Rivaroxaban 15-20 mg deve essere assunto CON IL CIBO per garantire assorbimento ottimale.");

        return warnings;
    }

    public decimal GetInrThresholdForDoac(DoacType doacType)
    {
        return doacType switch
        {
            DoacType.Rivaroxaban => 3.0m,
            DoacType.Edoxaban => 2.5m,
            DoacType.Apixaban => 2.0m,
            DoacType.Dabigatran => 2.0m,
            _ => 2.0m
        };
    }

    private void GenerateWarfarinToDoacProtocol(
        SwitchProtocol protocol,
        DoacType doacType,
        WarfarinType warfarinType,
        SwitchPatientParameters parameters)
    {
        var threshold = GetInrThresholdForDoac(doacType);
        protocol.InrThreshold = threshold;

        var warfarinName = warfarinType == WarfarinType.Warfarin ? "Warfarin (Coumadin)" : "Acenocumarolo (Sintrom)";
        var doacName = GetDoacName(doacType);

        // Giorno 0: Sospendere Warfarin
        protocol.Timeline.Add(new SwitchTimelineStep
        {
            Day = 0,
            Action = $"Sospendere {warfarinName}",
            Details = $"Ultima dose di {warfarinName} oggi. Non assumere più il farmaco.",
            StepType = "action"
        });

        // Monitoraggio INR
        int dayToCheck = warfarinType == WarfarinType.Warfarin ? 2 : 1;

        protocol.Timeline.Add(new SwitchTimelineStep
        {
            Day = dayToCheck,
            Action = "Controllare INR",
            Details = $"Eseguire prelievo per INR. Obiettivo: INR ≤{threshold:F1}",
            StepType = "monitoring"
        });

        // Avvio DOAC
        int estimatedDays = warfarinType == WarfarinType.Warfarin ? 3 : 2;

        protocol.Timeline.Add(new SwitchTimelineStep
        {
            Day = estimatedDays,
            Action = $"Iniziare {doacName} se INR ≤{threshold:F1}",
            Details = $"Dosaggio: {protocol.RecommendedDoacDosage}. Assumere solo se l'INR è sceso a ≤{threshold:F1}. Se INR ancora elevato, ripetere controllo dopo 24h.",
            StepType = "action"
        });

        // Note cliniche
        protocol.ClinicalNotes.Add($"📌 PRINCIPIO 'Stop and Wait': Il {warfarinName} ha un effetto prolungato (emivita {(warfarinType == WarfarinType.Warfarin ? "36-42 ore" : "8-11 ore")}). È necessario attendere che l'INR scenda sotto la soglia di {threshold:F1} prima di iniziare {doacName}.");
        protocol.ClinicalNotes.Add($"📌 L'attesa media è di {estimatedDays}-{estimatedDays + 1} giorni, ma può variare in base al metabolismo individuale.");
        protocol.ClinicalNotes.Add("📌 Non è necessario bridging con eparina in questo passaggio, poiché il Warfarin mantiene copertura anticoagulante fino a quando l'INR è terapeutico.");

        if (parameters.CurrentINR.HasValue)
        {
            protocol.ClinicalNotes.Add($"📌 INR attuale del paziente: {parameters.CurrentINR.Value:F1}");
        }
    }

    private void GenerateDoacToWarfarinProtocol(
        SwitchProtocol protocol,
        DoacType doacType,
        WarfarinType warfarinType,
        SwitchPatientParameters parameters)
    {
        var warfarinName = warfarinType == WarfarinType.Warfarin ? "Warfarin (Coumadin)" : "Acenocumarolo (Sintrom)";
        var doacName = GetDoacName(doacType);

        switch (doacType)
        {
            case DoacType.Dabigatran:
                GenerateDabigatranToWarfarinProtocol(protocol, warfarinName, doacName, parameters);
                break;

            case DoacType.Edoxaban:
                GenerateEdoxabanToWarfarinProtocol(protocol, warfarinName, doacName, parameters);
                break;

            case DoacType.Apixaban:
            case DoacType.Rivaroxaban:
                GenerateXaInhibitorToWarfarinProtocol(protocol, warfarinName, doacName, doacType, parameters);
                break;
        }
    }

    private void GenerateDabigatranToWarfarinProtocol(
        SwitchProtocol protocol,
        string warfarinName,
        string doacName,
        SwitchPatientParameters parameters)
    {
        var clcr = parameters.CreatinineClearance;
        int overlapDays;
        string overlapReason;

        if (clcr >= 50)
        {
            overlapDays = 3;
            overlapReason = $"ClCr {clcr:F1} mL/min ≥50";
        }
        else if (clcr >= 30 && clcr < 50)
        {
            overlapDays = 2;
            overlapReason = $"ClCr {clcr:F1} mL/min tra 30-50";
        }
        else
        {
            overlapDays = 1;
            overlapReason = $"ClCr {clcr:F1} mL/min tra 15-30";
        }

        // Giorno 0: Iniziare Warfarin continuando Dabigatran
        protocol.Timeline.Add(new SwitchTimelineStep
        {
            Day = 0,
            Action = $"Iniziare {warfarinName} mantenendo {doacName}",
            Details = $"Assumere {warfarinName} (dose iniziale 5 mg/die o 2-3 mg in anziani) E continuare {doacName} alla dose abituale.",
            StepType = "action"
        });

        // Giorni di overlap
        for (int day = 1; day < overlapDays; day++)
        {
            protocol.Timeline.Add(new SwitchTimelineStep
            {
                Day = day,
                Action = $"Continuare {warfarinName} + {doacName}",
                Details = $"Assumere entrambi i farmaci. Controllare INR prima della dose di {doacName}.",
                StepType = "action"
            });
        }

        // Ultimo giorno: sospendere Dabigatran
        protocol.Timeline.Add(new SwitchTimelineStep
        {
            Day = overlapDays,
            Action = $"Sospendere {doacName}",
            Details = $"Non assumere più {doacName}. Continuare solo {warfarinName}. Controllare INR.",
            StepType = "action"
        });

        // Follow-up INR
        protocol.Timeline.Add(new SwitchTimelineStep
        {
            Day = overlapDays + 1,
            Action = "Monitoraggio INR",
            Details = "Controllo INR quotidiano fino a stabilizzazione nel range 2-3. Aggiustare dose Warfarin secondo INR.",
            StepType = "monitoring"
        });

        protocol.ClinicalNotes.Add($"📌 OVERLAP GRADUATO: Il Dabigatran viene continuato per {overlapDays} giorni ({overlapReason}) mentre il Warfarin inizia a fare effetto.");
        protocol.ClinicalNotes.Add("📌 L'INR va misurato PRIMA della dose di Dabigatran (a valle) per evitare interferenze.");
        protocol.ClinicalNotes.Add("📌 Questo metodo evita periodi di scopertura anticoagulante sfruttando l'emivita breve del Dabigatran.");
    }

    private void GenerateEdoxabanToWarfarinProtocol(
        SwitchProtocol protocol,
        string warfarinName,
        string doacName,
        SwitchPatientParameters parameters)
    {
        var currentDose = protocol.RecommendedDoacDosage.Contains("60") ? "60 mg" : "30 mg";
        var reducedDose = protocol.RecommendedDoacDosage.Contains("60") ? "30 mg" : "15 mg";

        // Giorno 0: Ridurre Edoxaban e iniziare Warfarin
        protocol.Timeline.Add(new SwitchTimelineStep
        {
            Day = 0,
            Action = $"Ridurre {doacName} a metà dose e iniziare {warfarinName}",
            Details = $"Ridurre {doacName} da {currentDose} a {reducedDose}. Iniziare {warfarinName} (dose 5 mg/die o 2-3 mg in anziani).",
            StepType = "action"
        });

        // Giorni 1-3: Overlap con dose ridotta
        for (int day = 1; day <= 3; day++)
        {
            protocol.Timeline.Add(new SwitchTimelineStep
            {
                Day = day,
                Action = $"Continuare {doacName} {reducedDose} + {warfarinName}",
                Details = $"Assumere {doacName} a dose ridotta + {warfarinName}. Controllare INR prima della dose di {doacName}.",
                StepType = "action"
            });
        }

        // Giorno 4: Verifica INR e possibile sospensione Edoxaban
        protocol.Timeline.Add(new SwitchTimelineStep
        {
            Day = 4,
            Action = "Controllo INR decisionale",
            Details = $"Se INR ≥2.0 (misurato prima dose {doacName}): sospendere {doacName} e continuare solo {warfarinName}. Se INR <2.0: continuare overlap un altro giorno.",
            StepType = "monitoring"
        });

        // Follow-up
        protocol.Timeline.Add(new SwitchTimelineStep
        {
            Day = 5,
            Action = "Monitoraggio INR",
            Details = "Controllo INR frequente (ogni 2-3 giorni) fino a stabilizzazione nel range 2-3.",
            StepType = "monitoring"
        });

        protocol.ClinicalNotes.Add("📌 METODO A METÀ DOSE: Edoxaban viene ridotto a metà dose durante l'introduzione di Warfarin, mantenendo copertura anticoagulante senza sovra-elevare l'INR.");
        protocol.ClinicalNotes.Add("📌 Questo schema è stato validato nello studio ENGAGE AF-TIMI 48.");
        protocol.ClinicalNotes.Add("📌 L'INR va misurato immediatamente prima della dose di Edoxaban per evitare interferenze analitiche.");
    }

    private void GenerateXaInhibitorToWarfarinProtocol(
        SwitchProtocol protocol,
        string warfarinName,
        string doacName,
        DoacType doacType,
        SwitchPatientParameters parameters)
    {
        // Giorno 0: Sospendere DOAC e iniziare EBPM + Warfarin
        protocol.Timeline.Add(new SwitchTimelineStep
        {
            Day = 0,
            Action = $"Sospendere {doacName} e iniziare EBPM + {warfarinName}",
            Details = $"Non assumere più {doacName}. Iniziare Eparina a Basso Peso Molecolare (es. Enoxaparina 1 mg/kg ogni 12h o 1.5 mg/kg/die) + {warfarinName} (dose 5 mg/die o 2-3 mg in anziani).",
            StepType = "action"
        });

        // Giorni 1-5: Bridging con EBPM
        for (int day = 1; day <= 5; day++)
        {
            protocol.Timeline.Add(new SwitchTimelineStep
            {
                Day = day,
                Action = $"Continuare EBPM + {warfarinName}",
                Details = $"Mantenere sovrapposizione EBPM + {warfarinName}. Controllo INR quotidiano.",
                StepType = "action"
            });
        }

        // Giorno 6: Valutazione INR
        protocol.Timeline.Add(new SwitchTimelineStep
        {
            Day = 6,
            Action = "Valutazione INR per sospensione EBPM",
            Details = "Se INR ≥2.0 per due controlli consecutivi: sospendere EBPM e continuare solo Warfarin. Se INR <2.0: continuare EBPM + Warfarin.",
            StepType = "monitoring"
        });

        // Follow-up
        protocol.Timeline.Add(new SwitchTimelineStep
        {
            Day = 7,
            Action = "Monitoraggio INR",
            Details = "Controllo INR ogni 2-3 giorni fino a stabilizzazione nel range 2-3.",
            StepType = "monitoring"
        });

        protocol.ClinicalNotes.Add($"📌 BRIDGING CON EPARINA: {doacName} ha emivita breve (~12 ore). È necessario bridging con EBPM per coprire il paziente durante i 5-7 giorni necessari affinché il Warfarin raggiunga effetto terapeutico.");
        protocol.ClinicalNotes.Add("📌 L'EBPM garantisce anticoagulazione continua e permette misurazioni INR 'pulite' (non influenzate dal DOAC residuo).");
        protocol.ClinicalNotes.Add("📌 Sospendere EBPM solo quando INR è stabilmente ≥2.0 per almeno 24-48h.");

        if (doacType == DoacType.Rivaroxaban)
        {
            protocol.ClinicalNotes.Add("ℹ️ Alternativa (basso rischio): In pazienti selezionati a basso rischio trombotico, è teoricamente possibile sovrapporre direttamente Rivaroxaban + Warfarin senza EBPM, ma l'approccio con bridging è più sicuro e raccomandato.");
        }
    }

    private string GenerateMonitoringPlan(SwitchDirection direction, DoacType doacType)
    {
        if (direction == SwitchDirection.WarfarinToDoac)
        {
            return @"📋 PIANO DI MONITORAGGIO POST-SWITCH:
• Controllo emocromo e funzione renale a 1 mese dallo switch
• Controllo funzione renale ogni 3-6 mesi (più frequente se ClCr <50 mL/min)
• Controllo funzione epatica se indicato clinicamente
• NON è necessario monitoraggio INR dopo l'avvio del DOAC (i DOAC non richiedono monitoraggio routinario)
• Educare il paziente sull'importanza dell'aderenza: i DOAC hanno emivita breve, saltare una dose significa scoprire il paziente
• Follow-up clinico a 1 mese per verificare tollerabilità e assenza di eventi avversi";
        }
        else
        {
            return @"📋 PIANO DI MONITORAGGIO POST-SWITCH:
• Controllo INR frequente nei primi 7-14 giorni (ogni 2-3 giorni) fino a stabilizzazione
• Target INR: 2.0-3.0 per la maggior parte delle indicazioni
• Dopo stabilizzazione: controllo INR ogni 2-4 settimane
• Calcolare Time in Therapeutic Range (TTR) dopo 3 mesi
• Controllo emocromo e funzione renale/epatica secondo necessità clinica
• Educare il paziente su interazioni farmacologiche e alimentari del Warfarin
• Follow-up clinico a 1 mese per verificare stabilità INR e assenza di eventi avversi";
        }
    }

    private string GetDoacName(DoacType doacType)
    {
        return doacType switch
        {
            DoacType.Apixaban => "Apixaban (Eliquis)",
            DoacType.Rivaroxaban => "Rivaroxaban (Xarelto)",
            DoacType.Dabigatran => "Dabigatran (Pradaxa)",
            DoacType.Edoxaban => "Edoxaban (Lixiana)",
            _ => "DOAC"
        };
    }
}
