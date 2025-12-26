using WarfarinManager.Data.Entities;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Data.Services;

/// <summary>
/// Servizio per calcoli clinici DOAC
/// - Cockcroft-Gault (CrCl)
/// - HAS-BLED Score
/// - Determinazione dosaggio DOAC
/// </summary>
public class DoacCalculationService
{
    #region Cockcroft-Gault Formula

    /// <summary>
    /// Calcola la Clearance della Creatinina usando la formula Cockcroft-Gault
    ///
    /// FORMULA:
    /// Maschi:   CrCl = ((140 - età) × peso) / (72 × creatinina)
    /// Femmine:  CrCl = ((140 - età) × peso × 0.85) / (72 × creatinina)
    ///
    /// </summary>
    /// <param name="eta">Età in anni</param>
    /// <param name="peso">Peso in kg</param>
    /// <param name="creatinina">Creatinina in mg/dL</param>
    /// <param name="gender">Genere del paziente</param>
    /// <returns>CrCl in mL/min (arrotondato a intero)</returns>
    public static int? CalcolaCockcroftGault(int? eta, decimal? peso, decimal? creatinina, Gender? gender)
    {
        // Validazione input
        if (!eta.HasValue || !peso.HasValue || !creatinina.HasValue || !gender.HasValue)
            return null;

        if (creatinina.Value == 0)
            return null; // Evita divisione per zero

        // Calcolo base
        decimal risultato = ((140m - eta.Value) * peso.Value) / (72m * creatinina.Value);

        // Fattore correttivo per femmine
        if (gender.Value == Gender.Female)
        {
            risultato *= 0.85m;
        }

        // Arrotonda a intero
        return (int)Math.Round(risultato, 0);
    }

    /// <summary>
    /// Calcola CrCl da un record di monitoraggio DOAC
    /// </summary>
    public static int? CalcolaCockcroftGault(DoacMonitoringRecord record, Patient patient)
    {
        if (record == null || patient == null)
            return null;

        return CalcolaCockcroftGault(patient.Age, record.Peso, record.Creatinina, patient.Gender);
    }

    #endregion

    #region HAS-BLED Score

    /// <summary>
    /// Calcola il HAS-BLED Score per valutare il rischio emorragico
    ///
    /// PUNTEGGIO (max 9):
    /// H - Hypertension (ipertensione non controllata): 1
    /// A - Abnormal renal function (CrCl < 30): 1
    /// A - Abnormal liver function (cirrosi: 2, alterazione lieve: 1): 1-2
    /// S - Stroke (storia di stroke/TIA): 1
    /// B - Bleeding (storia di sanguinamenti): 1
    /// L - Labile INR (non applicabile per DOAC): 0
    /// E - Elderly (età > 65): 1
    /// D - Drugs (antiaggreganti o FANS): 1
    /// D - Alcohol (abuso di alcol): 1
    ///
    /// INTERPRETAZIONE:
    /// 0-1: Basso rischio
    /// 2: Moderato rischio
    /// ≥3: Alto rischio
    /// </summary>
    public static int CalcolaHasBledScore(DoacMonitoringRecord record, Patient patient)
    {
        if (record == null || patient == null)
            return 0;

        int score = 0;

        // H - Hypertension
        if (record.Ipertensione)
            score += 1;

        // A - Abnormal renal function (CrCl < 30)
        if (record.CrCl_Cockroft.HasValue && record.CrCl_Cockroft.Value < 30)
            score += 1;

        // A - Abnormal liver function
        if (record.Cirrosi)
            score += 2; // Cirrosi severa
        else if (record.DisfunzioneEpatica || record.IpertensPortale)
            score += 1; // Alterazione lieve

        // S - Stroke
        if (record.StoriaStroke)
            score += 1;

        // B - Bleeding
        if (record.StoriaSanguinamento)
            score += 1;

        // L - Labile INR (non applicabile per DOAC, skip)

        // E - Elderly (> 65 anni)
        if (patient.Age > 65)
            score += 1;

        // D - Drugs (antiaggreganti o FANS)
        if (record.Antiaggreganti || record.FANS)
            score += 1;

        // D - Alcohol
        if (record.AbusoDiAlcol)
            score += 1;

        return score;
    }

    /// <summary>
    /// Ottiene il livello di rischio emorragico basato su HAS-BLED
    /// </summary>
    public static string GetRischioEmorragico(int hasBledScore)
    {
        return hasBledScore switch
        {
            0 or 1 => "Basso",
            2 => "Moderato",
            >= 3 => "Alto",
            _ => "Non calcolato"
        };
    }

    #endregion

    #region Determinazione Dosaggio DOAC

    /// <summary>
    /// Schema dosaggio per ciascun DOAC
    /// </summary>
    private static readonly Dictionary<string, DosageScheme> DOSAGE_SCHEMES = new()
    {
        ["Apixaban"] = new DosageScheme
        {
            Standard = "5 mg BID",
            Reduced = "2.5 mg BID",
            Loading = "10 mg BID (7gg)",
            Prevention = "2.5 mg BID",
            RenalThreshold = 15,
            CriteriRiduzione = "Almeno 2 tra: Età ≥ 80, Peso ≤ 60, Creatinina ≥ 1.5 mg/dL. O CrCl 15-29 mL/min."
        },
        ["Rivaroxaban"] = new DosageScheme
        {
            Standard = "20 mg QD",
            Reduced = "15 mg QD",
            Loading = "15 mg BID (21gg)",
            Prevention = "10 mg QD",
            RenalThreshold = 15,
            CriteriRiduzione = "CrCl 15-49 mL/min."
        },
        ["Edoxaban"] = new DosageScheme
        {
            Standard = "60 mg QD",
            Reduced = "30 mg QD",
            Loading = null,
            Prevention = null,
            RenalThreshold = 15,
            CriteriRiduzione = "CrCl 15-50 mL/min, Peso ≤ 60 kg, o uso inibitori P-gp."
        },
        ["Dabigatran"] = new DosageScheme
        {
            Standard = "150 mg BID",
            Reduced = "110 mg BID",
            Loading = null,
            Prevention = null,
            RenalThreshold = 30, // Soglia più alta!
            CriteriRiduzione = "Età ≥ 80 anni, Uso Verapamil, o elevato rischio emorragico gastrico."
        }
    };

    /// <summary>
    /// Determina il dosaggio DOAC appropriato
    /// </summary>
    public static DosageRecommendation DeterminaDosaggio(
        DoacMonitoringRecord record,
        Patient patient,
        string doacSelezionato,
        string indicazione)
    {
        var result = new DosageRecommendation();

        // Validazione
        if (string.IsNullOrEmpty(doacSelezionato) || !DOSAGE_SCHEMES.ContainsKey(doacSelezionato))
        {
            result.Dose = "DOAC non specificato";
            result.Reasons.Add("Selezionare un DOAC valido");
            return result;
        }

        var scheme = DOSAGE_SCHEMES[doacSelezionato];
        var eta = patient.Age;
        var crCl = record.CrCl_Cockroft ?? 0;

        // ======== CONTROINDICAZIONI ASSOLUTE ========

        // Insufficienza renale severa
        if (crCl < scheme.RenalThreshold)
        {
            result.Dose = "CONTROINDICATO";
            result.Reasons.Add($"Insufficienza renale severa (CrCl < {scheme.RenalThreshold} mL/min)");
            result.IsControindicato = true;
            return result;
        }

        // Cirrosi per Rivaroxaban
        if (doacSelezionato == "Rivaroxaban" && record.Cirrosi)
        {
            result.Dose = "SCONSIGLIATO";
            result.Reasons.Add("Cirrosi epatica (Child-Pugh B/C)");
            result.IsControindicato = true;
            return result;
        }

        // ======== FASE DI TRATTAMENTO ========

        // Fase d'attacco TEV
        if (indicazione?.Contains("Trattamento") == true && !string.IsNullOrEmpty(scheme.Loading))
        {
            result.Dose = scheme.Loading;
            result.Reasons.Add("Fase d'attacco TEV");
            return result;
        }

        // Prevenzione secondaria a lungo termine
        if (indicazione?.Contains("Prevenzione") == true && !string.IsNullOrEmpty(scheme.Prevention))
        {
            result.Dose = scheme.Prevention;
            result.Reasons.Add("Prevenzione secondaria a lungo termine");
            return result;
        }

        // ======== CRITERI DI RIDUZIONE DOSE ========

        bool needsReduction = false;
        var reductionReasons = new List<string>();

        // APIXABAN: almeno 2 tra età ≥80, peso ≤60, creatinina ≥1.5
        if (doacSelezionato == "Apixaban")
        {
            int criteriaCount = 0;
            if (eta >= 80) { criteriaCount++; reductionReasons.Add("Età ≥ 80 anni"); }
            if (record.Peso.HasValue && record.Peso.Value <= 60) { criteriaCount++; reductionReasons.Add("Peso ≤ 60 kg"); }
            if (record.Creatinina.HasValue && record.Creatinina.Value >= 1.5m) { criteriaCount++; reductionReasons.Add("Creatinina ≥ 1.5 mg/dL"); }

            needsReduction = criteriaCount >= 2 || (crCl >= 15 && crCl <= 29);
        }

        // RIVAROXABAN: CrCl 15-49
        if (doacSelezionato == "Rivaroxaban")
        {
            if (crCl >= 15 && crCl < 50)
            {
                needsReduction = true;
                reductionReasons.Add($"CrCl {crCl} mL/min (15-49)");
            }
        }

        // EDOXABAN: CrCl 15-50, peso ≤60
        if (doacSelezionato == "Edoxaban")
        {
            if (crCl >= 15 && crCl <= 50)
            {
                needsReduction = true;
                reductionReasons.Add($"CrCl {crCl} mL/min (15-50)");
            }
            if (record.Peso.HasValue && record.Peso.Value <= 60)
            {
                needsReduction = true;
                reductionReasons.Add("Peso ≤ 60 kg");
            }
        }

        // DABIGATRAN: età ≥80
        if (doacSelezionato == "Dabigatran")
        {
            if (eta >= 80)
            {
                needsReduction = true;
                reductionReasons.Add("Età ≥ 80 anni");
            }
        }

        // ======== ASSEGNA DOSE ========

        if (needsReduction)
        {
            result.Dose = scheme.Reduced;
            result.Reasons.AddRange(reductionReasons);
        }
        else
        {
            result.Dose = scheme.Standard;
            result.Reasons.Add("Posologia Standard");
        }

        return result;
    }

    #endregion

    #region Intervallo Controllo

    /// <summary>
    /// Determina l'intervallo di controllo consigliato basato su HAS-BLED, CrCl e altri fattori
    ///
    /// LINEE GUIDA:
    /// - HAS-BLED >= 3 (Alto rischio): controllo ogni 3 mesi
    /// - HAS-BLED 2 (Moderato) o CrCl 30-50: controllo ogni 6 mesi
    /// - HAS-BLED 0-1 (Basso) e CrCl > 50: controllo ogni 12 mesi
    /// - CrCl < 30: controllo ogni 3 mesi (se DOAC ancora appropriato)
    /// - Età > 80: ridurre intervallo di un livello
    /// </summary>
    public static ControlloRecommendation DeterminaIntervalloControllo(
        DoacMonitoringRecord record,
        Patient patient,
        int hasBledScore)
    {
        var result = new ControlloRecommendation();
        var motivazioni = new List<string>();
        int intervallo = 12; // Default: annuale

        var crCl = record.CrCl_Cockroft ?? 0;
        var eta = patient.Age;

        // Criterio HAS-BLED
        if (hasBledScore >= 3)
        {
            intervallo = Math.Min(intervallo, 3);
            motivazioni.Add($"HAS-BLED elevato ({hasBledScore}): rischio emorragico alto");
        }
        else if (hasBledScore == 2)
        {
            intervallo = Math.Min(intervallo, 6);
            motivazioni.Add($"HAS-BLED moderato ({hasBledScore})");
        }

        // Criterio funzione renale
        if (crCl > 0 && crCl < 30)
        {
            intervallo = Math.Min(intervallo, 3);
            motivazioni.Add($"CrCl critica ({crCl} mL/min): monitoraggio stretto funzione renale");
        }
        else if (crCl >= 30 && crCl < 50)
        {
            intervallo = Math.Min(intervallo, 6);
            motivazioni.Add($"CrCl ridotta ({crCl} mL/min): monitoraggio semestrale funzione renale");
        }

        // Criterio età avanzata
        if (eta > 80)
        {
            if (intervallo > 6) intervallo = 6;
            motivazioni.Add("Età > 80 anni: monitoraggio più frequente consigliato");
        }

        // Criterio disfunzione epatica
        if (record.DisfunzioneEpatica || record.Cirrosi)
        {
            intervallo = Math.Min(intervallo, 6);
            motivazioni.Add("Disfunzione epatica: monitoraggio funzione epatica");
        }

        // Criterio farmaci concomitanti a rischio
        if (record.Antiaggreganti || record.FANS)
        {
            intervallo = Math.Min(intervallo, 6);
            motivazioni.Add("Farmaci a rischio emorragico concomitanti");
        }

        // Criterio anemia/piastrinopenia
        if (record.Emoglobina.HasValue && record.Emoglobina.Value < 10)
        {
            intervallo = Math.Min(intervallo, 3);
            motivazioni.Add($"Anemia significativa (Hb {record.Emoglobina.Value} g/dL)");
        }

        if (record.Piastrine.HasValue && record.Piastrine.Value < 100000)
        {
            intervallo = Math.Min(intervallo, 3);
            motivazioni.Add($"Piastrinopenia ({record.Piastrine.Value}/μL)");
        }

        // Se nessun criterio specifico
        if (motivazioni.Count == 0)
        {
            motivazioni.Add("Paziente stabile: follow-up standard");
        }

        result.IntervalloMesi = intervallo;
        result.Motivazioni = motivazioni;
        result.DataProssimoControllo = DateTime.Now.AddMonths(intervallo);
        result.Descrizione = intervallo switch
        {
            3 => "Controllo ravvicinato (3 mesi)",
            6 => "Controllo semestrale (6 mesi)",
            12 => "Controllo annuale (12 mesi)",
            _ => $"Controllo a {intervallo} mesi"
        };

        return result;
    }

    #endregion

    #region Suggerimento DOAC Alternativi

    /// <summary>
    /// Valuta se il DOAC attuale è appropriato e suggerisce alternative se necessario
    /// </summary>
    public static DoacEvaluationResult ValutaAppropriatezzaDoac(
        DoacMonitoringRecord record,
        Patient patient,
        string doacAttuale)
    {
        var result = new DoacEvaluationResult
        {
            DoacAttuale = doacAttuale,
            IsAppropriato = true
        };

        var crCl = record.CrCl_Cockroft ?? 0;
        var eta = patient.Age;
        var peso = record.Peso ?? 0;

        // Lista di tutti i DOAC disponibili
        var tuttiDoac = new List<string> { "Apixaban", "Rivaroxaban", "Edoxaban", "Dabigatran" };

        // Valutazione per ogni DOAC
        var doacContraindicati = new Dictionary<string, string>();
        var doacSconsigliati = new Dictionary<string, string>();
        var doacIdonei = new Dictionary<string, List<string>>();

        foreach (var doac in tuttiDoac)
        {
            var idoneita = ValutaIdoneitaDoac(doac, record, patient);

            if (idoneita.IsContraindicato)
            {
                doacContraindicati[doac] = idoneita.Motivo;
            }
            else if (idoneita.IsSconsigliato)
            {
                doacSconsigliati[doac] = idoneita.Motivo;
            }
            else
            {
                doacIdonei[doac] = idoneita.Vantaggi;
            }
        }

        // Valuta DOAC attuale
        if (doacContraindicati.ContainsKey(doacAttuale))
        {
            result.IsAppropriato = false;
            result.IsContraindicato = true;
            result.MotivoInappropriatezza = doacContraindicati[doacAttuale];
            result.Raccomandazione = $"⛔ {doacAttuale} CONTROINDICATO: {doacContraindicati[doacAttuale]}";
        }
        else if (doacSconsigliati.ContainsKey(doacAttuale))
        {
            result.IsAppropriato = false;
            result.IsSconsigliato = true;
            result.MotivoInappropriatezza = doacSconsigliati[doacAttuale];
            result.Raccomandazione = $"⚠️ {doacAttuale} SCONSIGLIATO: {doacSconsigliati[doacAttuale]}";
        }

        // Suggerisci alternative
        if (!result.IsAppropriato)
        {
            foreach (var doac in doacIdonei.Keys)
            {
                if (doac != doacAttuale)
                {
                    result.DoacAlternativi.Add(new DoacAlternativo
                    {
                        Nome = doac,
                        Vantaggi = doacIdonei[doac],
                        Priorita = CalcolaPrioritaDoac(doac, record, patient)
                    });
                }
            }

            // Ordina per priorità
            result.DoacAlternativi = result.DoacAlternativi
                .OrderByDescending(d => d.Priorita)
                .ToList();

            if (result.DoacAlternativi.Any())
            {
                // Esiste almeno un DOAC alternativo idoneo
                var migliore = result.DoacAlternativi.First();
                result.DoacConsigliato = migliore.Nome;
                result.MotivoConsiglio = string.Join(", ", migliore.Vantaggi);
            }
            else
            {
                // Nessun DOAC alternativo idoneo - suggerire Switch a Warfarin
                result.SuggerisciSwitchWarfarin = true;

                // Costruisci il motivo dello switch
                var motiviContraindicazione = new List<string>();
                foreach (var kvp in doacContraindicati)
                {
                    motiviContraindicazione.Add($"{kvp.Key}: {kvp.Value}");
                }
                foreach (var kvp in doacSconsigliati)
                {
                    motiviContraindicazione.Add($"{kvp.Key}: {kvp.Value}");
                }

                result.MotivoSwitchWarfarin = $"Tutti i DOAC sono controindicati o sconsigliati per questo paziente. " +
                    $"Motivazioni: {string.Join("; ", motiviContraindicazione)}. " +
                    "Warfarin rappresenta l'alternativa terapeutica raccomandata con monitoraggio INR regolare.";
            }
        }

        // Verifica anche se il DOAC attuale è appropriato ma tutti gli altri sono controindicati
        // In questo caso non suggeriamo switch ma informiamo dell'assenza di alternative
        if (result.IsAppropriato && doacIdonei.Count == 1 && doacIdonei.ContainsKey(doacAttuale))
        {
            // Solo il DOAC attuale è idoneo, nessuna alternativa disponibile
            // Non suggeriamo switch perché il DOAC attuale va bene
        }

        return result;
    }

    /// <summary>
    /// Valuta l'idoneità di un singolo DOAC per il paziente
    /// </summary>
    private static (bool IsContraindicato, bool IsSconsigliato, string Motivo, List<string> Vantaggi)
        ValutaIdoneitaDoac(string doac, DoacMonitoringRecord record, Patient patient)
    {
        var vantaggi = new List<string>();
        var crCl = record.CrCl_Cockroft ?? 0;
        var eta = patient.Age;
        var peso = record.Peso ?? 0;

        switch (doac)
        {
            case "Apixaban":
                // Controindicazioni
                if (crCl < 15)
                    return (true, false, "Insufficienza renale severa (CrCl < 15 mL/min)", vantaggi);

                // Vantaggi
                vantaggi.Add("Migliore profilo in IRC (25% eliminazione renale)");
                if (eta >= 80) vantaggi.Add("Sicuro nell'anziano (studio ARISTOTLE)");
                if (peso <= 60) vantaggi.Add("Riduzione dose ben definita per basso peso");
                if (record.StoriaSanguinamento) vantaggi.Add("Minor rischio sanguinamento GI vs altri DOAC");
                break;

            case "Rivaroxaban":
                // Controindicazioni
                if (crCl < 15)
                    return (true, false, "Insufficienza renale severa (CrCl < 15 mL/min)", vantaggi);
                if (record.Cirrosi)
                    return (false, true, "Cirrosi Child-Pugh B/C: metabolismo epatico compromesso", vantaggi);

                // Vantaggi
                vantaggi.Add("Monosomministrazione giornaliera (migliore aderenza)");
                if (crCl >= 50) vantaggi.Add("Efficace con funzione renale normale");
                break;

            case "Edoxaban":
                // Controindicazioni
                if (crCl < 15)
                    return (true, false, "Insufficienza renale severa (CrCl < 15 mL/min)", vantaggi);
                if (crCl > 95)
                    return (false, true, "CrCl > 95 mL/min: efficacia ridotta in FA", vantaggi);

                // Vantaggi
                vantaggi.Add("Monosomministrazione giornaliera");
                if (crCl >= 30 && crCl <= 50) vantaggi.Add("Schema di riduzione dose semplice");
                if (peso <= 60) vantaggi.Add("Riduzione dose per basso peso");
                break;

            case "Dabigatran":
                // Controindicazioni - soglia più alta!
                if (crCl < 30)
                    return (true, false, "Insufficienza renale (CrCl < 30 mL/min) - 80% eliminazione renale", vantaggi);

                // Vantaggi
                vantaggi.Add("Unico con antidoto specifico (Idarucizumab)");
                vantaggi.Add("Non metabolizzato dal fegato (sicuro in epatopatia)");
                if (!record.Cirrosi && !record.DisfunzioneEpatica)
                    vantaggi.Add("Ideale se funzione epatica compromessa");
                break;
        }

        return (false, false, string.Empty, vantaggi);
    }

    /// <summary>
    /// Calcola la priorità di un DOAC come alternativa (0-100)
    /// </summary>
    private static int CalcolaPrioritaDoac(string doac, DoacMonitoringRecord record, Patient patient)
    {
        int priorita = 50; // Base
        var crCl = record.CrCl_Cockroft ?? 0;
        var eta = patient.Age;

        switch (doac)
        {
            case "Apixaban":
                // Migliore in IRC, anziani, basso peso
                if (crCl < 50) priorita += 20;
                if (eta >= 80) priorita += 15;
                if (record.Peso <= 60) priorita += 10;
                if (record.StoriaSanguinamento) priorita += 15;
                break;

            case "Rivaroxaban":
                // Migliore aderenza (QD)
                priorita += 5; // Bonus monosomministrazione
                if (crCl >= 50) priorita += 10;
                break;

            case "Edoxaban":
                // Buon profilo generale
                if (crCl >= 30 && crCl <= 50) priorita += 15;
                priorita += 5; // Bonus monosomministrazione
                break;

            case "Dabigatran":
                // Migliore se problema epatico
                if (record.DisfunzioneEpatica || record.Cirrosi) priorita += 25;
                if (crCl >= 50) priorita += 10;
                // Penalità per IRC
                if (crCl < 50) priorita -= 20;
                break;
        }

        return Math.Max(0, Math.Min(100, priorita));
    }

    #endregion

    #region Alert Clinici

    /// <summary>
    /// Genera alert basati sui risultati di laboratorio
    /// </summary>
    public static List<string> GeneraAlert(DoacMonitoringRecord record, Patient patient)
    {
        var alerts = new List<string>();

        if (record == null || patient == null)
            return alerts;

        // Alert Emoglobina bassa
        if (record.Emoglobina.HasValue)
        {
            var sogliaHb = patient.Gender == Gender.Male ? 13.0m : 12.0m;
            if (record.Emoglobina.Value < sogliaHb)
            {
                alerts.Add($"⚠️ Emoglobina bassa ({record.Emoglobina.Value} g/dL) - sospetto sanguinamento occulto");
            }
        }

        // Alert Piastrinopenia
        if (record.Piastrine.HasValue && record.Piastrine.Value < 100000)
        {
            alerts.Add($"⚠️ Piastrinopenia ({record.Piastrine.Value}/μL) - aumentato rischio emorragico");
        }

        // Alert Transaminasi elevate
        if ((record.ALT.HasValue && record.ALT.Value > 120) ||
            (record.AST.HasValue && record.AST.Value > 120))
        {
            alerts.Add($"⚠️ Transaminasi elevate (AST {record.AST} U/L, ALT {record.ALT} U/L) - valutare funzione epatica");
        }

        // Alert CrCl molto basso
        if (record.CrCl_Cockroft.HasValue)
        {
            if (record.CrCl_Cockroft.Value < 15)
            {
                alerts.Add($"🚫 Clearance creatinina critica ({record.CrCl_Cockroft.Value} mL/min) - DOAC controindicati");
            }
            else if (record.CrCl_Cockroft.Value < 30)
            {
                alerts.Add($"⚠️ Clearance creatinina molto bassa ({record.CrCl_Cockroft.Value} mL/min) - controindicazione relativa DOAC");
            }
            else if (record.CrCl_Cockroft.Value < 50)
            {
                alerts.Add($"⚠️ Clearance creatinina ridotta ({record.CrCl_Cockroft.Value} mL/min) - considerare riduzione dose");
            }
        }

        // Alert HAS-BLED alto
        if (record.HasBledScore >= 5)
        {
            alerts.Add($"🔴 HAS-BLED molto alto ({record.HasBledScore} punti) - rischio emorragico severo, valutare attentamente beneficio/rischio");
        }
        else if (record.HasBledScore >= 3)
        {
            alerts.Add($"⚠️ HAS-BLED elevato ({record.HasBledScore} punti) - rischio emorragico aumentato, monitoraggio stretto");
        }

        return alerts;
    }

    #endregion
}

#region Helper Classes

/// <summary>
/// Schema dosaggio per un DOAC
/// </summary>
public class DosageScheme
{
    public string Standard { get; set; } = string.Empty;
    public string Reduced { get; set; } = string.Empty;
    public string? Loading { get; set; }
    public string? Prevention { get; set; }
    public int RenalThreshold { get; set; }
    public string CriteriRiduzione { get; set; } = string.Empty;
}

/// <summary>
/// Risultato della raccomandazione dosaggio
/// </summary>
public class DosageRecommendation
{
    public string Dose { get; set; } = string.Empty;
    public List<string> Reasons { get; set; } = new();
    public bool IsControindicato { get; set; }
    public bool IsSconsigliato { get; set; }
    public List<string> DoacAlternativi { get; set; } = new();
    public string? MotivoSuggerimentoCambio { get; set; }

    public string RazionaleCompleto => string.Join("; ", Reasons);
}

/// <summary>
/// Risultato del calcolo dell'intervallo di controllo
/// </summary>
public class ControlloRecommendation
{
    public int IntervalloMesi { get; set; }
    public string Descrizione { get; set; } = string.Empty;
    public List<string> Motivazioni { get; set; } = new();
    public DateTime DataProssimoControllo { get; set; }
}

/// <summary>
/// Risultato della valutazione di appropriatezza DOAC
/// </summary>
public class DoacEvaluationResult
{
    public string DoacAttuale { get; set; } = string.Empty;
    public bool IsAppropriato { get; set; } = true;
    public bool IsContraindicato { get; set; }
    public bool IsSconsigliato { get; set; }
    public string? MotivoInappropriatezza { get; set; }
    public string? Raccomandazione { get; set; }
    public string? DoacConsigliato { get; set; }
    public string? MotivoConsiglio { get; set; }
    public List<DoacAlternativo> DoacAlternativi { get; set; } = new();
    public bool SuggerisciSwitchWarfarin { get; set; }
    public string? MotivoSwitchWarfarin { get; set; }
}

/// <summary>
/// DOAC alternativo suggerito
/// </summary>
public class DoacAlternativo
{
    public string Nome { get; set; } = string.Empty;
    public List<string> Vantaggi { get; set; } = new();
    public int Priorita { get; set; }
}

#endregion
