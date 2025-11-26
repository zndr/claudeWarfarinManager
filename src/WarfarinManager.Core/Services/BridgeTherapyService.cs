using System.Text;
using WarfarinManager.Data.Entities;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per la gestione della bridge therapy perioperatoria
/// Implementa le linee guida FCSA-SIMG e ACCP per la gestione
/// della terapia anticoagulante in pazienti che devono sottoporsi a chirurgia
/// </summary>
public class BridgeTherapyService : IBridgeTherapyService
{
    /// <summary>
    /// Calcola il punteggio CHA₂DS₂-VASc per pazienti con fibrillazione atriale
    /// </summary>
    public int CalculateCHA2DS2VAScScore(Patient patient, bool? hadStrokeTIA = null)
    {
        int score = 0;
        int age = CalculateAge(patient.BirthDate);

        // C - Congestive heart failure/LV dysfunction: +1
        // (da verificare nelle note del paziente)
        
        // H - Hypertension: +1
        // (da verificare nelle note del paziente)
        
        // A₂ - Age ≥75: +2
        if (age >= 75)
            score += 2;
        // A - Age 65-74: +1
        else if (age >= 65)
            score += 1;

        // D - Diabetes mellitus: +1
        // (da verificare nelle note del paziente)
        
        // S₂ - Stroke/TIA/thromboembolism: +2
        if (hadStrokeTIA == true)
            score += 2;

        // V - Vascular disease: +1
        // (da verificare nelle note del paziente)
        
        // Sc - Sex category (female): +1
        if (patient.Gender == Gender.Female)
            score += 1;

        return score;
    }

    /// <summary>
    /// Determina il rischio tromboembolico del paziente in base all'indicazione
    /// </summary>
    public ThromboembolicRisk DetermineThromboembolicRisk(
        Patient patient,
        Indication? activeIndication,
        int? cha2ds2vascOverride = null)
    {
        if (activeIndication == null)
            return ThromboembolicRisk.Moderate;

        var indicationCode = activeIndication.IndicationType?.Code ?? "";

        // Protesi valvolari meccaniche - sempre alto rischio
        if (indicationCode.Contains("MECH_VALVE"))
        {
            // Valvola mitralica meccanica = ALTO rischio
            if (indicationCode.Contains("MITRAL"))
                return ThromboembolicRisk.High;
            
            // Valvola aortica con fattori di rischio aggiuntivi = ALTO
            // Valvola aortica senza fattori di rischio = MODERATO
            return ThromboembolicRisk.High; // Default alto per protesi meccaniche
        }

        // Fibrillazione atriale - usa CHA2DS2-VASc
        if (indicationCode.Contains("FA") || indicationCode.Contains("AFIB"))
        {
            int cha2ds2vasc = cha2ds2vascOverride ?? CalculateCHA2DS2VAScScore(patient);
            
            if (cha2ds2vasc >= 5)
                return ThromboembolicRisk.High;
            else if (cha2ds2vasc >= 3)
                return ThromboembolicRisk.Moderate;
            else
                return ThromboembolicRisk.Low;
        }

        // TEV - dipende dalla tempistica
        if (indicationCode.Contains("TEV") || indicationCode.Contains("TVP") || indicationCode.Contains("PE"))
        {
            // TEV negli ultimi 3 mesi = ALTO rischio
            var monthsSinceStart = (DateTime.Today - activeIndication.StartDate).TotalDays / 30;
            if (monthsSinceStart < 3)
                return ThromboembolicRisk.High;
            else if (monthsSinceStart < 12)
                return ThromboembolicRisk.Moderate;
            else
                return ThromboembolicRisk.Low;
        }

        // Default moderato per altre indicazioni
        return ThromboembolicRisk.Moderate;
    }

    /// <summary>
    /// Determina il rischio emorragico in base al tipo di chirurgia
    /// </summary>
    public BleedingRisk DetermineBleedingRisk(SurgeryType surgeryType)
    {
        return surgeryType switch
        {
            // Alto rischio emorragico (>5%)
            SurgeryType.Neurosurgery => BleedingRisk.High,
            SurgeryType.CardiacSurgery => BleedingRisk.High,
            SurgeryType.VascularSurgery => BleedingRisk.High,
            SurgeryType.ThoracicSurgery => BleedingRisk.High,
            SurgeryType.AbdominalSurgery => BleedingRisk.High,
            SurgeryType.HepaticSurgery => BleedingRisk.High,
            SurgeryType.PancreaticSurgery => BleedingRisk.High,
            SurgeryType.ProstateSurgery => BleedingRisk.High,
            SurgeryType.RenalSurgery => BleedingRisk.High,
            SurgeryType.MajorOrthopedic => BleedingRisk.High,
            SurgeryType.EpiduralAnesthesia => BleedingRisk.High,
            SurgeryType.OphthalmologySurgery => BleedingRisk.High,
            SurgeryType.DentalMajor => BleedingRisk.High,
            
            // Rischio moderato (2-5%)
            SurgeryType.EndoscopyWithBiopsy => BleedingRisk.Moderate,
            SurgeryType.Polypectomy => BleedingRisk.Moderate,
            SurgeryType.Cardioversion => BleedingRisk.Moderate,
            SurgeryType.PacemakerImplant => BleedingRisk.Moderate,
            SurgeryType.ArthroscopyMinor => BleedingRisk.Moderate,
            SurgeryType.LaparoscopicCholecystectomy => BleedingRisk.Moderate,
            SurgeryType.TURP => BleedingRisk.Moderate,
            
            // Basso rischio (0-2%)
            SurgeryType.DiagnosticEndoscopy => BleedingRisk.Low,
            SurgeryType.DiagnosticColonoscopy => BleedingRisk.Low,
            SurgeryType.CardiacCatheterization => BleedingRisk.Low,
            SurgeryType.Transesophageal => BleedingRisk.Low,
            SurgeryType.DermatologySingleExcision => BleedingRisk.Low,
            SurgeryType.Ophthalmology => BleedingRisk.Low,
            SurgeryType.DentalSingleExtraction => BleedingRisk.Low,
            
            _ => BleedingRisk.Moderate
        };
    }

    /// <summary>
    /// Raccomandazione FCSA-SIMG per bridge therapy
    /// </summary>
    public BridgeRecommendation GetFCSARecommendation(
        ThromboembolicRisk teRisk,
        BleedingRisk bleedingRisk,
        bool hasMechanicalValve)
    {
        // Protesi valvolare meccanica: SEMPRE bridge terapeutico
        if (hasMechanicalValve)
        {
            return new BridgeRecommendation
            {
                BridgeRecommended = true,
                DosageType = BridgeDosageType.Therapeutic,
                Rationale = "Protesi valvolare meccanica: bridge therapy terapeutico obbligatorio",
                Warnings = "Alto rischio trombotico in assenza di anticoagulazione",
                Guideline = GuidelineType.FCSA
            };
        }

        // Alto rischio TE
        if (teRisk == ThromboembolicRisk.High)
        {
            return new BridgeRecommendation
            {
                BridgeRecommended = true,
                DosageType = bleedingRisk == BleedingRisk.High 
                    ? BridgeDosageType.Prophylactic 
                    : BridgeDosageType.Therapeutic,
                Rationale = "Alto rischio tromboembolico: bridge therapy raccomandato",
                Warnings = bleedingRisk == BleedingRisk.High 
                    ? "Bilanciare rischio TE elevato con rischio emorragico chirurgico" 
                    : "",
                Guideline = GuidelineType.FCSA
            };
        }

        // Rischio moderato
        if (teRisk == ThromboembolicRisk.Moderate)
        {
            if (bleedingRisk == BleedingRisk.High)
            {
                return new BridgeRecommendation
                {
                    BridgeRecommended = false,
                    DosageType = BridgeDosageType.None,
                    Rationale = "Rischio TE moderato + alto rischio emorragico: no bridge (Trial BRIDGE)",
                    Warnings = "Valutare caso per caso considerando tipo di chirurgia",
                    Guideline = GuidelineType.FCSA
                };
            }
            
            return new BridgeRecommendation
            {
                BridgeRecommended = true,
                DosageType = BridgeDosageType.Prophylactic,
                Rationale = "Rischio TE moderato: bridge profilattico può essere considerato",
                Warnings = "Decisione da individualizzare",
                Guideline = GuidelineType.FCSA
            };
        }

        // Basso rischio TE
        return new BridgeRecommendation
        {
            BridgeRecommended = false,
            DosageType = BridgeDosageType.None,
            Rationale = "Basso rischio tromboembolico: bridge therapy NON raccomandato",
            Warnings = "",
            Guideline = GuidelineType.FCSA
        };
    }

    /// <summary>
    /// Raccomandazione ACCP per bridge therapy
    /// </summary>
    public BridgeRecommendation GetACCPRecommendation(
        ThromboembolicRisk teRisk,
        BleedingRisk bleedingRisk,
        bool hasMechanicalValve)
    {
        // Protesi valvolare meccanica
        if (hasMechanicalValve)
        {
            return new BridgeRecommendation
            {
                BridgeRecommended = true,
                DosageType = BridgeDosageType.Therapeutic,
                Rationale = "Protesi valvolare meccanica: bridge therapy raccomandato (Grade 2C)",
                Warnings = "Considerare rischio emorragico specifico della procedura",
                Guideline = GuidelineType.ACCP
            };
        }

        // ACCP è più conservativo dopo Trial BRIDGE
        if (teRisk == ThromboembolicRisk.High)
        {
            return new BridgeRecommendation
            {
                BridgeRecommended = true,
                DosageType = BridgeDosageType.Therapeutic,
                Rationale = "Alto rischio TE: bridge therapy suggerito (Grade 2C)",
                Warnings = bleedingRisk == BleedingRisk.High 
                    ? "Valutare attentamente il rapporto rischio/beneficio" 
                    : "",
                Guideline = GuidelineType.ACCP
            };
        }

        // Rischio moderato/basso - ACCP più conservativo dopo BRIDGE trial
        return new BridgeRecommendation
        {
            BridgeRecommended = false,
            DosageType = BridgeDosageType.None,
            Rationale = "Rischio TE non elevato: NO bridge (BRIDGE Trial 2015)",
            Warnings = "Il Trial BRIDGE ha dimostrato non inferiorità del no-bridging in FA a rischio moderato",
            Guideline = GuidelineType.ACCP
        };
    }

    /// <summary>
    /// Genera il protocollo completo di bridge therapy
    /// </summary>
    public BridgeProtocol GenerateProtocol(
        Patient patient,
        DateTime surgeryDate,
        SurgeryType surgeryType,
        ThromboembolicRisk teRisk,
        BleedingRisk bleedingRisk,
        GuidelineType guideline,
        bool bridgeRecommended,
        BridgeDosageType? dosageType = null)
    {
        var protocol = new BridgeProtocol
        {
            SurgeryDate = surgeryDate,
            SurgeryType = surgeryType,
            TERisk = teRisk,
            BleedingRisk = bleedingRisk,
            CHA2DS2VAScScore = CalculateCHA2DS2VAScScore(patient),
            BridgeRecommended = bridgeRecommended,
            DosageType = dosageType,
            Guideline = guideline
        };

        // Timeline standard FCSA-SIMG
        // Giorno -5: Stop warfarin
        protocol.WarfarinStopDate = surgeryDate.AddDays(-5);
        
        // Giorno -1: Controllo INR (deve essere <1.5)
        protocol.INRCheckDate1 = surgeryDate.AddDays(-1);

        if (bridgeRecommended && dosageType != BridgeDosageType.None)
        {
            // Giorno -3: Inizio EBPM
            protocol.EBPMStartDate = surgeryDate.AddDays(-3);
            
            // Giorno -1 (24h prima): Ultima dose EBPM
            protocol.EBPMLastDoseDate = surgeryDate.AddDays(-1);
            
            // Dosaggio EBPM basato su tipo
            SetEBPMDosage(protocol, dosageType ?? BridgeDosageType.Prophylactic, patient);
        }

        // Post-operatorio
        // Giorno 0 (12-24h post): Ripresa warfarin dose abituale
        protocol.WarfarinResumeDate = surgeryDate;

        if (bridgeRecommended && dosageType != BridgeDosageType.None && bleedingRisk != BleedingRisk.High)
        {
            // Giorno +1-2: Ripresa EBPM (se emostasi adeguata)
            protocol.EBPMResumeDate = surgeryDate.AddDays(1);
        }

        // Controllo INR post-ripresa
        protocol.INRCheckDate2 = surgeryDate.AddDays(5);

        // Genera raccomandazioni testuali
        GenerateRecommendationText(protocol, guideline, bleedingRisk);
        
        // Note cliniche
        AddClinicalNotes(protocol, patient, surgeryType, bleedingRisk);

        return protocol;
    }

    /// <summary>
    /// Formatta il protocollo per l'esportazione in testo
    /// </summary>
    public string FormatProtocolForExport(BridgeProtocol protocol, Patient patient)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine("  WARFARIN MANAGER PRO - PROTOCOLLO BRIDGE THERAPY");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine();
        
        sb.AppendLine($"PAZIENTE: {patient.LastName} {patient.FirstName}");
        sb.AppendLine($"CF: {patient.FiscalCode}");
        sb.AppendLine($"Data di nascita: {patient.BirthDate:dd/MM/yyyy} ({CalculateAge(patient.BirthDate)} anni)");
        sb.AppendLine();
        
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine("INFORMAZIONI INTERVENTO");
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine();
        sb.AppendLine($"Data intervento: {protocol.SurgeryDate:dddd dd MMMM yyyy}");
        sb.AppendLine($"Tipo chirurgia: {GetSurgeryTypeDescription(protocol.SurgeryType)}");
        sb.AppendLine($"Rischio emorragico: {GetRiskDescription(protocol.BleedingRisk)}");
        sb.AppendLine();
        
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine("VALUTAZIONE RISCHIO TROMBOEMBOLICO");
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine();
        sb.AppendLine($"Rischio TE: {GetRiskDescription(protocol.TERisk)}");
        if (protocol.CHA2DS2VAScScore > 0)
            sb.AppendLine($"CHA₂DS₂-VASc Score: {protocol.CHA2DS2VAScScore}");
        sb.AppendLine();
        
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine($"RACCOMANDAZIONE ({protocol.Guideline})");
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine();
        
        if (protocol.BridgeRecommended)
        {
            sb.AppendLine("★ BRIDGE THERAPY: RACCOMANDATO");
            sb.AppendLine($"  Tipo dosaggio: {GetDosageTypeDescription(protocol.DosageType)}");
        }
        else
        {
            sb.AppendLine("☆ BRIDGE THERAPY: NON RACCOMANDATO");
        }
        sb.AppendLine();
        sb.AppendLine(protocol.FCSARecommendation);
        
        if (!string.IsNullOrEmpty(protocol.ACCPRecommendation))
        {
            sb.AppendLine();
            sb.AppendLine("Confronto ACCP:");
            sb.AppendLine(protocol.ACCPRecommendation);
        }
        sb.AppendLine();
        
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine("TIMELINE PRE-OPERATORIA");
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine();
        sb.AppendLine($"  Giorno -5 ({protocol.WarfarinStopDate:dd/MM}): STOP WARFARIN");
        
        if (protocol.EBPMStartDate.HasValue)
        {
            sb.AppendLine($"  Giorno -3 ({protocol.EBPMStartDate:dd/MM}): Inizio EBPM");
            sb.AppendLine($"      → {protocol.EBPMDrug} {protocol.EBPMDosage} {protocol.EBPMFrequency}");
        }
        
        sb.AppendLine($"  Giorno -1 ({protocol.INRCheckDate1:dd/MM}): Controllo INR (target <1.5)");
        
        if (protocol.EBPMLastDoseDate.HasValue)
        {
            sb.AppendLine($"  Giorno -1 ({protocol.EBPMLastDoseDate:dd/MM}): Ultima dose EBPM (almeno 24h prima)");
        }
        
        sb.AppendLine($"  Giorno  0 ({protocol.SurgeryDate:dd/MM}): *** INTERVENTO ***");
        sb.AppendLine();
        
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine("TIMELINE POST-OPERATORIA");
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine();
        sb.AppendLine($"  Giorno  0 ({protocol.WarfarinResumeDate:dd/MM}): Ripresa WARFARIN (12-24h post)");
        sb.AppendLine("      → Dose abituale se emostasi adeguata");
        
        if (protocol.EBPMResumeDate.HasValue)
        {
            sb.AppendLine($"  Giorno +1-2 ({protocol.EBPMResumeDate:dd/MM}): Ripresa EBPM");
            sb.AppendLine("      → Se emostasi adeguata, stesso dosaggio pre-operatorio");
            sb.AppendLine("      → Stop EBPM quando INR ≥2.0 per 24h");
        }
        
        if (protocol.INRCheckDate2.HasValue)
        {
            sb.AppendLine($"  Giorno +5 ({protocol.INRCheckDate2:dd/MM}): Controllo INR");
        }
        sb.AppendLine();
        
        if (protocol.Warnings.Any())
        {
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine("⚠ AVVERTENZE");
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine();
            foreach (var warning in protocol.Warnings)
            {
                sb.AppendLine($"  • {warning}");
            }
            sb.AppendLine();
        }
        
        if (protocol.ClinicalNotes.Any())
        {
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine("NOTE CLINICHE");
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine();
            foreach (var note in protocol.ClinicalNotes)
            {
                sb.AppendLine($"  • {note}");
            }
            sb.AppendLine();
        }
        
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine($"Generato da: WarfarinManager Pro v1.0");
        sb.AppendLine($"Data/Ora: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine();
        sb.AppendLine("DISCLAIMER: Questo protocollo è uno strumento di supporto");
        sb.AppendLine("decisionale. Il medico prescrittore è responsabile della");
        sb.AppendLine("valutazione clinica finale e della decisione terapeutica.");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        
        return sb.ToString();
    }

    #region Private Helpers

    private int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;
        return age;
    }

    private void SetEBPMDosage(BridgeProtocol protocol, BridgeDosageType dosageType, Patient patient)
    {
        protocol.EBPMDrug = "Enoxaparina";
        
        if (dosageType == BridgeDosageType.Therapeutic)
        {
            // Dose terapeutica: 1 mg/kg × 2/die
            protocol.EBPMDosage = "1 mg/kg";
            protocol.EBPMFrequency = "ogni 12 ore (bid)";
        }
        else // Prophylactic
        {
            // Dose profilattica: 40 mg/die o 4000 UI/die
            protocol.EBPMDosage = "4000 UI (40 mg)";
            protocol.EBPMFrequency = "una volta al giorno";
        }
    }

    private void GenerateRecommendationText(BridgeProtocol protocol, GuidelineType guideline, BleedingRisk bleedingRisk)
    {
        var sb = new StringBuilder();
        
        if (guideline == GuidelineType.FCSA)
        {
            if (protocol.BridgeRecommended)
            {
                sb.AppendLine("Secondo le linee guida FCSA-SIMG:");
                if (protocol.DosageType == BridgeDosageType.Therapeutic)
                {
                    sb.AppendLine("- Bridge therapy con dosaggio TERAPEUTICO raccomandato");
                    sb.AppendLine("- Enoxaparina 1 mg/kg × 2/die o equivalente");
                }
                else
                {
                    sb.AppendLine("- Bridge therapy con dosaggio PROFILATTICO raccomandato");
                    sb.AppendLine("- Enoxaparina 4000 UI/die o equivalente");
                }
            }
            else
            {
                sb.AppendLine("Secondo le linee guida FCSA-SIMG:");
                sb.AppendLine("- Bridge therapy NON raccomandato");
                sb.AppendLine("- Sospendere warfarin 5 giorni prima e riprendere dopo l'intervento");
            }
        }
        
        protocol.FCSARecommendation = sb.ToString();
        
        // Genera anche raccomandazione ACCP per confronto
        var accpRec = GetACCPRecommendation(protocol.TERisk, bleedingRisk, 
            protocol.TERisk == ThromboembolicRisk.High);
        protocol.ACCPRecommendation = accpRec.Rationale;
    }

    private void AddClinicalNotes(BridgeProtocol protocol, Patient patient, SurgeryType surgeryType, BleedingRisk bleedingRisk)
    {
        // Note generali
        protocol.ClinicalNotes.Add("Verificare assenza di controindicazioni a EBPM");
        protocol.ClinicalNotes.Add("Controllare funzionalità renale prima di iniziare EBPM");
        
        // Note specifiche per età
        int age = CalculateAge(patient.BirthDate);
        if (age > 75)
        {
            protocol.ClinicalNotes.Add("Paziente anziano: considerare riduzione dosaggio EBPM");
            protocol.Warnings.Add("Rischio emorragico aumentato per età >75 anni");
        }
        
        // Note per chirurgia ad alto rischio
        if (bleedingRisk == BleedingRisk.High)
        {
            protocol.Warnings.Add("Chirurgia ad alto rischio emorragico: coordinare con team chirurgico");
            protocol.ClinicalNotes.Add("Valutare emostasi prima di riprendere anticoagulazione");
        }
        
        // Note specifiche per tipo di chirurgia
        switch (surgeryType)
        {
            case SurgeryType.Neurosurgery:
            case SurgeryType.EpiduralAnesthesia:
                protocol.Warnings.Add("NEUROCHIRURGIA/ANESTESIA NEUROASSIALE: Massima cautela, consultare specialista");
                protocol.ClinicalNotes.Add("Valutare ritardo ripresa anticoagulazione");
                protocol.ClinicalNotes.Add("Rischio ematoma epidurale: attendere almeno 24h post-procedura");
                break;
                
            case SurgeryType.DentalSingleExtraction:
            case SurgeryType.DentalMajor:
                protocol.ClinicalNotes.Add("Procedure dentali: considerare continuazione warfarin con INR 2.0-2.5");
                protocol.ClinicalNotes.Add("Emostasi locale con acido tranexamico collutorio");
                break;
                
            case SurgeryType.DiagnosticEndoscopy:
            case SurgeryType.DiagnosticColonoscopy:
            case SurgeryType.EndoscopyWithBiopsy:
            case SurgeryType.Polypectomy:
                protocol.ClinicalNotes.Add("Endoscopia: il bridge dipende dal tipo di procedura (biopsia, polipectomia)");
                break;
                
            case SurgeryType.CardiacSurgery:
            case SurgeryType.VascularSurgery:
                protocol.ClinicalNotes.Add("Coordinare gestione anticoagulazione con team cardiochirurgico/vascolare");
                break;
        }
    }

    private string GetSurgeryTypeDescription(SurgeryType type)
    {
        return type switch
        {
            // Basso rischio
            SurgeryType.DiagnosticEndoscopy => "Endoscopia diagnostica",
            SurgeryType.DiagnosticColonoscopy => "Colonscopia diagnostica",
            SurgeryType.CardiacCatheterization => "Cateterismo cardiaco diagnostico",
            SurgeryType.Transesophageal => "Ecocardiografia transesofagea",
            SurgeryType.DermatologySingleExcision => "Escissione cutanea minore",
            SurgeryType.Ophthalmology => "Chirurgia oculistica minore (cataratta)",
            SurgeryType.DentalSingleExtraction => "Estrazione dentaria singola",
            
            // Rischio moderato
            SurgeryType.EndoscopyWithBiopsy => "Endoscopia con biopsia",
            SurgeryType.Polypectomy => "Polipectomia colonscopica",
            SurgeryType.Cardioversion => "Cardioversione elettrica",
            SurgeryType.PacemakerImplant => "Impianto pacemaker/ICD",
            SurgeryType.ArthroscopyMinor => "Artroscopia minore",
            SurgeryType.LaparoscopicCholecystectomy => "Colecistectomia laparoscopica",
            SurgeryType.TURP => "TURP / Cistoscopia con biopsia",
            
            // Alto rischio
            SurgeryType.Neurosurgery => "Neurochirurgia",
            SurgeryType.CardiacSurgery => "Cardiochirurgia",
            SurgeryType.VascularSurgery => "Chirurgia vascolare maggiore",
            SurgeryType.ThoracicSurgery => "Chirurgia toracica",
            SurgeryType.AbdominalSurgery => "Chirurgia addominale maggiore",
            SurgeryType.HepaticSurgery => "Chirurgia epatica",
            SurgeryType.PancreaticSurgery => "Chirurgia pancreatica",
            SurgeryType.ProstateSurgery => "Chirurgia prostatica",
            SurgeryType.RenalSurgery => "Chirurgia renale",
            SurgeryType.MajorOrthopedic => "Chirurgia ortopedica maggiore",
            SurgeryType.EpiduralAnesthesia => "Anestesia neuroassiale",
            SurgeryType.OphthalmologySurgery => "Chirurgia oculistica maggiore",
            SurgeryType.DentalMajor => "Chirurgia odontoiatrica maggiore (≥3 estrazioni)",
            
            SurgeryType.Other => "Altra procedura",
            _ => type.ToString()
        };
    }

    private string GetRiskDescription(BleedingRisk risk)
    {
        return risk switch
        {
            BleedingRisk.Low => "BASSO",
            BleedingRisk.Moderate => "MODERATO",
            BleedingRisk.High => "ALTO",
            _ => risk.ToString()
        };
    }

    private string GetRiskDescription(ThromboembolicRisk risk)
    {
        return risk switch
        {
            ThromboembolicRisk.Low => "BASSO",
            ThromboembolicRisk.Moderate => "MODERATO",
            ThromboembolicRisk.High => "ALTO",
            _ => risk.ToString()
        };
    }

    private string GetDosageTypeDescription(BridgeDosageType? type)
    {
        return type switch
        {
            BridgeDosageType.Therapeutic => "TERAPEUTICO (Enoxaparina 1 mg/kg × 2/die)",
            BridgeDosageType.Prophylactic => "PROFILATTICO (Enoxaparina 4000 UI/die)",
            BridgeDosageType.None => "Nessuno",
            _ => "Non specificato"
        };
    }

    #endregion
}
