using Microsoft.Extensions.Logging;
using WarfarinManager.Core.Models;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Implementazione algoritmi calcolo dosaggio warfarin
/// </summary>
public class DosageCalculatorService : IDosageCalculatorService
{
    private readonly ILogger<DosageCalculatorService> _logger;

    public DosageCalculatorService(ILogger<DosageCalculatorService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public DosageSuggestionResult CalculateFCSA(
        decimal currentINR,
        decimal targetINRMin,
        decimal targetINRMax,
        decimal currentWeeklyDoseMg,
        TherapyPhase phase,
        bool isCompliant,
        bool isSlowMetabolizer,
        ThromboembolicRisk thromboembolicRisk = ThromboembolicRisk.Moderate,
        TipoEmorragia tipoEmorragia = TipoEmorragia.Nessuna,
        SedeEmorragia sedeEmorragia = SedeEmorragia.Nessuna,
        bool hasProtesiMeccanica = false,
        DateTime? dataUltimoTEV = null,
        string indicazioneTAO = "",
        int cha2ds2vasc = 0)
    {
        ValidateInputs(currentINR, targetINRMin, targetINRMax, currentWeeklyDoseMg);

        var result = new DosageSuggestionResult
        {
            GuidelineUsed = GuidelineType.FCSA,
            CurrentINR = currentINR,
            TargetINRMin = targetINRMin,
            TargetINRMax = targetINRMax,
            CurrentWeeklyDoseMg = currentWeeklyDoseMg,
            IsInRange = IsINRInRange(currentINR, targetINRMin, targetINRMax),
            FonteRaccomandazione = "FCSA"
        };

        // Determina fascia INR dettagliata
        FasciaINR fasciaINR;
        if (currentINR >= targetINRMin && currentINR <= targetINRMax)
        {
            fasciaINR = FasciaINR.InRange;
        }
        else if (currentINR < targetINRMin)
        {
            fasciaINR = DeterminaFasciaINRBasso(currentINR, targetINRMin, targetINRMax);
        }
        else
        {
            fasciaINR = DeterminaFasciaINRAlto(currentINR, targetINRMin, targetINRMax);
        }

        result.FasciaINR = fasciaINR;

        // Mantieni compatibilità con INRStatus (deprecato)
#pragma warning disable CS0618 // Type or member is obsolete
        result.INRStatus = EvaluateINRStatus(currentINR, targetINRMin, targetINRMax);
#pragma warning restore CS0618

        // Alert se non compliance
        if (!isCompliant)
        {
            result.Warnings.Add("⚠️ ATTENZIONE: Verificata scarsa compliance. Verificare le 4D prima di modificare dose.");
        }

        // Alert metabolizzatore lento
        if (isSlowMetabolizer)
        {
            result.Warnings.Add("⚠️ METABOLIZZATORE LENTO: Piccole variazioni hanno grande impatto sul INR.");
        }

        // Calcola rischio tromboembolico (per INR basso)
        bool rischioTromboticoElevato = false;
        if (fasciaINR is FasciaINR.SubCritico or FasciaINR.SubModerato or FasciaINR.SubLieve)
        {
            rischioTromboticoElevato = CalcolaRischioTromboticoElevato(
                hasProtesiMeccanica, dataUltimoTEV, indicazioneTAO, cha2ds2vasc);
        }

        // Calcolo basato su fascia INR
        switch (fasciaINR)
        {
            case FasciaINR.InRange:
                HandleInRangeFCSA(result, phase);
                break;

            case FasciaINR.SubCritico:
            case FasciaINR.SubModerato:
            case FasciaINR.SubLieve:
                // INR sottoterapeutico
                HandleBelowRangeNuovoFCSA(result, fasciaINR, currentWeeklyDoseMg,
                    isSlowMetabolizer, rischioTromboticoElevato);
                break;

            case FasciaINR.SovraLieve:
            case FasciaINR.SovraModerato:
            case FasciaINR.SovraAlto:
            case FasciaINR.SovraMoltoAlto:
            case FasciaINR.SovraCritico:
            case FasciaINR.SovraEstremo:
                // INR sovraterapeutico - switch su presenza emorragia
                if (tipoEmorragia != TipoEmorragia.Nessuna)
                {
                    HandleAboveRangeConEmorragiaFCSA(result, tipoEmorragia, sedeEmorragia,
                        currentWeeklyDoseMg, currentINR);
                }
                else
                {
                    HandleAboveRangeSenzaEmorragiaFCSA(result, fasciaINR, currentWeeklyDoseMg);
                }
                break;
        }

        // Genera schema settimanale (solo se dose > 0)
        if (result.SuggestedWeeklyDoseMg > 0)
        {
            result.WeeklySchedule = GenerateWeeklySchedule(
                result.SuggestedWeeklyDoseMg,
                result.DoseSupplementarePrimoGiorno);
        }

        _logger.LogInformation(
            "Calcolo FCSA: INR {INR} (target {Min}-{Max}), Fascia {Fascia} → Nuova dose {NewDose}mg, controllo tra {Days} giorni, Urgenza {Urgency}",
            currentINR, targetINRMin, targetINRMax, fasciaINR, result.SuggestedWeeklyDoseMg, result.NextControlDays, result.UrgencyLevel);

        return result;
    }

    public DosageSuggestionResult CalculateACCP(
        decimal currentINR,
        decimal targetINRMin,
        decimal targetINRMax,
        decimal currentWeeklyDoseMg,
        TherapyPhase phase,
        bool isCompliant,
        bool isSlowMetabolizer,
        ThromboembolicRisk thromboembolicRisk = ThromboembolicRisk.Moderate,
        TipoEmorragia tipoEmorragia = TipoEmorragia.Nessuna,
        SedeEmorragia sedeEmorragia = SedeEmorragia.Nessuna,
        bool hasProtesiMeccanica = false,
        DateTime? dataUltimoTEV = null,
        string indicazioneTAO = "",
        int cha2ds2vasc = 0)
    {
        ValidateInputs(currentINR, targetINRMin, targetINRMax, currentWeeklyDoseMg);

        var result = new DosageSuggestionResult
        {
            GuidelineUsed = GuidelineType.ACCP,
            CurrentINR = currentINR,
            TargetINRMin = targetINRMin,
            TargetINRMax = targetINRMax,
            CurrentWeeklyDoseMg = currentWeeklyDoseMg,
            IsInRange = IsINRInRange(currentINR, targetINRMin, targetINRMax),
            FonteRaccomandazione = "ACCP"
        };

        // Determina fascia INR dettagliata
        FasciaINR fasciaINR;
        if (currentINR >= targetINRMin && currentINR <= targetINRMax)
        {
            fasciaINR = FasciaINR.InRange;
        }
        else if (currentINR < targetINRMin)
        {
            fasciaINR = DeterminaFasciaINRBasso(currentINR, targetINRMin, targetINRMax);
        }
        else
        {
            fasciaINR = DeterminaFasciaINRAlto(currentINR, targetINRMin, targetINRMax);
        }

        result.FasciaINR = fasciaINR;

        // Mantieni compatibilità con INRStatus (deprecato)
#pragma warning disable CS0618 // Type or member is obsolete
        result.INRStatus = EvaluateINRStatus(currentINR, targetINRMin, targetINRMax);
#pragma warning restore CS0618

        if (!isCompliant)
        {
            result.Warnings.Add("⚠️ WARNING: Poor compliance verified. Assess causes before dose adjustment.");
        }

        if (isSlowMetabolizer)
        {
            result.Warnings.Add("⚠️ SLOW METABOLIZER: Small dose changes have large impact on INR.");
        }

        // Calcola rischio tromboembolico (per INR basso)
        // ACCP: solo rischio alto per EBPM (più conservativo di FCSA)
        bool rischioTromboticoElevato = false;
        if (fasciaINR is FasciaINR.SubCritico or FasciaINR.SubModerato or FasciaINR.SubLieve)
        {
            rischioTromboticoElevato = CalcolaRischioTromboticoElevato(
                hasProtesiMeccanica, dataUltimoTEV, indicazioneTAO, cha2ds2vasc);
        }

        // Calcolo basato su fascia INR
        switch (fasciaINR)
        {
            case FasciaINR.InRange:
                HandleInRangeACCP(result, phase);
                break;

            case FasciaINR.SubCritico:
            case FasciaINR.SubModerato:
            case FasciaINR.SubLieve:
                // INR sottoterapeutico - usa stesso algoritmo di FCSA ma EBPM solo per rischio alto
                HandleBelowRangeNuovoFCSA(result, fasciaINR, currentWeeklyDoseMg,
                    isSlowMetabolizer, rischioTromboticoElevato);
                // Nota: ACCP è più conservativo, EBPM solo se rischio VERAMENTE alto
                break;

            case FasciaINR.SovraLieve:
            case FasciaINR.SovraModerato:
            case FasciaINR.SovraAlto:
            case FasciaINR.SovraMoltoAlto:
            case FasciaINR.SovraCritico:
            case FasciaINR.SovraEstremo:
                // INR sovraterapeutico
                if (tipoEmorragia != TipoEmorragia.Nessuna)
                {
                    // Con emorragia: stessa gestione FCSA (linee guida convergono)
                    HandleAboveRangeConEmorragiaFCSA(result, tipoEmorragia, sedeEmorragia,
                        currentWeeklyDoseMg, currentINR);
                    result.FonteRaccomandazione = "ACCP/FCSA"; // Convergenti
                }
                else
                {
                    // Senza emorragia: ACCP ha soglie Vit K diverse
                    HandleAboveRangeSenzaEmorragiaACCP(result, fasciaINR, currentWeeklyDoseMg, currentINR);
                }
                break;
        }

        // Genera schema settimanale (solo se dose > 0)
        if (result.SuggestedWeeklyDoseMg > 0)
        {
            result.WeeklySchedule = GenerateWeeklySchedule(
                result.SuggestedWeeklyDoseMg,
                result.DoseSupplementarePrimoGiorno);
        }

        _logger.LogInformation(
            "Calcolo ACCP: INR {INR} (target {Min}-{Max}), Fascia {Fascia} → Nuova dose {NewDose}mg, controllo tra {Days} giorni, Urgenza {Urgency}",
            currentINR, targetINRMin, targetINRMax, fasciaINR, result.SuggestedWeeklyDoseMg, result.NextControlDays, result.UrgencyLevel);

        return result;
    }

    #region FCSA Logic

    private void HandleInRangeFCSA(DosageSuggestionResult result, TherapyPhase phase)
    {
        result.SuggestedWeeklyDoseMg = result.CurrentWeeklyDoseMg;
        result.PercentageAdjustment = 0;
        result.SospensioneDosi = 0; // Nessuna sospensione necessaria
        result.LoadingDoseAction = "Nessuna dose carico necessaria";
        result.ClinicalNotes = "✅ INR nel range terapeutico. Mantenere dose attuale.";

        // Intervallo controllo in base a fase
        result.NextControlDays = phase switch
        {
            TherapyPhase.Induction => 7,
            TherapyPhase.PostAdjustment => 14,
            TherapyPhase.Maintenance => 28, // 4 settimane
            _ => 14
        };

        // Se in mantenimento stabile, si può estendere
        if (phase == TherapyPhase.Maintenance)
        {
            result.ClinicalNotes += " Se INR stabilmente in range, controllo estendibile a 6 settimane.";
        }
    }

    private void HandleBelowRangeFCSA(
        DosageSuggestionResult result,
        decimal targetINRMin,
        decimal targetINRMax,
        ThromboembolicRisk risk,
        bool isSlowMetabolizer)
    {
        var inr = result.CurrentINR;
        var currentDose = result.CurrentWeeklyDoseMg;

        // Determina target range type (2-3 vs 2.5-3.5)
        bool isHighTarget = targetINRMax >= 3.5m;

        // Logica FCSA - Tabella INR BASSO
        if (!isHighTarget) // Target 2.0-3.0
        {
            if (inr >= 1.8m && inr < 2.0m)
            {
                // INR 1.8-1.9
                result.LoadingDoseAction = "Aumentare dose del 25% oggi";
                result.PercentageAdjustment = 0; // No cambio dose settimanale
                result.SuggestedWeeklyDoseMg = currentDose;
                result.NextControlDays = 14;
                result.RequiresEBPM = false;
                result.ClinicalNotes = "INR lievemente sotto range. Dose carico singola, poi mantenere schema abituale.";
            }
            else if (inr >= 1.5m && inr < 1.8m)
            {
                // INR 1.5-1.7
                result.LoadingDoseAction = "Aumentare dose del 50% oggi";
                result.PercentageAdjustment = isSlowMetabolizer ? 5 : 10m;
                result.SuggestedWeeklyDoseMg = currentDose * (1 + result.PercentageAdjustment / 100);
                result.NextControlDays = 7;
                result.RequiresEBPM = risk == ThromboembolicRisk.High;
                result.ClinicalNotes = "INR moderatamente basso. Dose carico + aumento dose settimanale.";
                
                if (result.RequiresEBPM)
                {
                    result.Warnings.Add("⚠️ Considerare EBPM a dose terapeutica (rischio TE alto)");
                }
            }
            else if (inr < 1.5m)
            {
                // INR <1.5
                result.LoadingDoseAction = "Raddoppiare dose abituale oggi";
                result.PercentageAdjustment = 10m;
                result.SuggestedWeeklyDoseMg = currentDose * (1 + result.PercentageAdjustment / 100);
                result.NextControlDays = 5;
                result.RequiresEBPM = risk == ThromboembolicRisk.High;
                result.ClinicalNotes = "⚠️ INR significativamente basso. Dose carico immediata + aumento dose.";
                result.Warnings.Add("⚠️ URGENTE: INR <1.5 - Valutare cause (4D)");
                
                if (result.RequiresEBPM)
                {
                    result.Warnings.Add("🔴 EBPM A DOSE TERAPEUTICA RACCOMANDATA (rischio TE alto)");
                }
            }
        }
        else // Target 2.5-3.5
        {
            if (inr >= 2.3m && inr < 2.5m)
            {
                // INR 2.3-2.4
                result.LoadingDoseAction = "Aumentare dose del 25% oggi";
                result.PercentageAdjustment = 0;
                result.SuggestedWeeklyDoseMg = currentDose;
                result.NextControlDays = 14;
                result.RequiresEBPM = false;
            }
            else if (inr >= 1.8m && inr < 2.3m)
            {
                // INR 1.8-2.2
                result.LoadingDoseAction = "Aumentare dose del 50% oggi";
                result.PercentageAdjustment = isSlowMetabolizer ? 5 : 10m;
                result.SuggestedWeeklyDoseMg = currentDose * (1 + result.PercentageAdjustment / 100);
                result.NextControlDays = 7;
                result.RequiresEBPM = false;
            }
            else if (inr >= 1.5m && inr < 1.8m)
            {
                // INR 1.5-1.7
                result.LoadingDoseAction = "Aumentare dose del 50% oggi";
                result.PercentageAdjustment = isSlowMetabolizer ? 5 : 10m;
                result.SuggestedWeeklyDoseMg = currentDose * (1 + result.PercentageAdjustment / 100);
                result.NextControlDays = 7;
                result.RequiresEBPM = risk >= ThromboembolicRisk.Moderate; // Dose intermedia
                
                if (result.RequiresEBPM)
                {
                    result.Warnings.Add("⚠️ Considerare EBPM a dose intermedia");
                }
            }
            else if (inr < 1.5m)
            {
                // INR <1.5
                result.LoadingDoseAction = "Raddoppiare dose abituale oggi";
                result.PercentageAdjustment = 10m;
                result.SuggestedWeeklyDoseMg = currentDose * (1 + result.PercentageAdjustment / 100);
                result.NextControlDays = 5;
                result.RequiresEBPM = true; // Dose terapeutica
                result.Warnings.Add("🔴 EBPM A DOSE TERAPEUTICA RACCOMANDATA");
            }
        }

        // Round to sensible dose
        result.SuggestedWeeklyDoseMg = RoundToSensibleDose(result.SuggestedWeeklyDoseMg);
    }

    private void HandleAboveRangeFCSA(DosageSuggestionResult result, decimal targetINRMax)
    {
        var inr = result.CurrentINR;
        var currentDose = result.CurrentWeeklyDoseMg;

        // Logica FCSA - Tabella INR ALTO
        if (inr > targetINRMax && inr <= 5.0m)
        {
            // INR 3-5 sopra range (ma ≤5)
            result.LoadingDoseAction = "Ridurre dose del 50% oggi o saltare 1 giorno";
            result.PercentageAdjustment = -10m;
            result.SuggestedWeeklyDoseMg = currentDose * 0.90m;
            result.NextControlDays = 7;
            result.RequiresVitaminK = false;
            result.ClinicalNotes = "INR moderatamente sopra range. Riduzione dose temporanea.";
        }
        else if (inr > 5.0m && inr <= 6.0m)
        {
            // INR 5-6
            result.LoadingDoseAction = "Saltare 1 giorno di terapia";
            result.PercentageAdjustment = -10m;
            result.SuggestedWeeklyDoseMg = currentDose * 0.90m;
            result.NextControlDays = 5; // Controllo più ravvicinato
            result.RequiresVitaminK = false;
            result.ClinicalNotes = "INR elevato. Sospensione temporanea raccomandata.";
            result.Warnings.Add("⚠️ INR 5-6: Valutare rischio emorragico");
        }
        else if (inr > 6.0m)
        {
            // INR >6
            result.LoadingDoseAction = "STOP warfarin + Vitamina K 1-2mg PO";
            result.PercentageAdjustment = -10m;
            result.SuggestedWeeklyDoseMg = currentDose * 0.90m;
            result.NextControlDays = 1; // Controllo urgente
            result.RequiresVitaminK = true;
            result.VitaminKDoseMg = 2;
            result.VitaminKRoute = "Orale";
            result.ClinicalNotes = "🔴 INR CRITICO: Sospensione immediata + Vitamina K";
            result.Warnings.Add("🔴 URGENTE: INR >6 - Valutare ricovero se sanguinamento");
            result.Warnings.Add("Controllo INR dopo 24h, poi ripetere a 7 giorni");
        }

        result.SuggestedWeeklyDoseMg = RoundToSensibleDose(result.SuggestedWeeklyDoseMg);
    }

    /// <summary>
    /// Gestione INR sottoterapeutico con nuovo algoritmo dettagliato
    /// </summary>
    private void HandleBelowRangeNuovoFCSA(
        DosageSuggestionResult result,
        FasciaINR fascia,
        decimal currentDose,
        bool isSlowMetabolizer,
        bool rischioTromboticoElevato)
    {
        switch (fascia)
        {
            case FasciaINR.SubCritico:
                // INR <1.5 (target 2-3) o <2.0 (target 2.5-3.5)
                result.PercentageAdjustment = 17.5m; // Media 15-20%
                result.DoseSupplementarePrimoGiorno = currentDose * 0.075m; // 7.5% primo giorno
                result.SuggestedWeeklyDoseMg = currentDose * 1.175m;
                result.NextControlDays = 6; // 5-7 giorni

                var loadingDoseRounded = RoundToSensibleDose(result.DoseSupplementarePrimoGiorno.Value);
                result.LoadingDoseAction = $"DOSE CARICO OGGI: +{loadingDoseRounded:F1}mg (7.5% dose settimanale)";

                if (rischioTromboticoElevato)
                {
                    result.RequiresEBPM = true;
                    result.Warnings.Add("🔴 EBPM RACCOMANDATA: Enoxaparina 70 UI/kg x 2/die fino a INR ≥2.0 per 24h");
                }
                result.ClinicalNotes = $"⚠️ INR CRITICO: Incremento dose settimanale +17.5% + dose carico OGGI (+{loadingDoseRounded:F1}mg).";
                result.UrgencyLevel = UrgencyLevel.Urgente;
                break;

            case FasciaINR.SubModerato:
                // INR 1.5-1.79 (target 2-3) o 2.0-2.29 (target 2.5-3.5)
                result.PercentageAdjustment = isSlowMetabolizer ? 7.5m : 11.25m; // 7.5-15%
                result.DoseSupplementarePrimoGiorno = currentDose * 0.05m; // Opzionale
                result.SuggestedWeeklyDoseMg = currentDose * (1 + result.PercentageAdjustment / 100);
                result.NextControlDays = 8; // 7-10 giorni
                result.LoadingDoseAction = "Dose carico opzionale (5% dose settimanale oggi)";
                result.ClinicalNotes = "INR moderatamente basso. Incremento dose.";
                result.UrgencyLevel = UrgencyLevel.Routine;
                break;

            case FasciaINR.SubLieve:
                // INR 1.8-1.99 (target 2-3) o 2.3-2.49 (target 2.5-3.5)
                result.PercentageAdjustment = 7.5m; // 5-10%
                result.DoseSupplementarePrimoGiorno = null;
                result.SuggestedWeeklyDoseMg = currentDose * 1.075m;
                result.NextControlDays = 12; // 10-14 giorni
                result.LoadingDoseAction = "Nessuna dose carico";
                result.ClinicalNotes = "INR lievemente basso. Valutare se ultimi 2 INR erano in range (possibile non modificare).";
                result.UrgencyLevel = UrgencyLevel.Routine;
                break;
        }

        // Round
        result.SuggestedWeeklyDoseMg = RoundToSensibleDose(result.SuggestedWeeklyDoseMg);
    }

    /// <summary>
    /// Gestione INR sovraterapeutico SENZA emorragia
    /// </summary>
    private void HandleAboveRangeSenzaEmorragiaFCSA(
        DosageSuggestionResult result,
        FasciaINR fascia,
        decimal currentDose)
    {
        switch (fascia)
        {
            case FasciaINR.SovraLieve:
                result.SospensioneDosi = 0;
                result.PercentageAdjustment = -7.5m; // -5 a -10%
                result.SuggestedWeeklyDoseMg = currentDose * 0.925m;
                result.RequiresVitaminK = false;
                result.NextControlDays = 10; // 7-14
                result.LoadingDoseAction = "Nessuna sospensione";
                result.ClinicalNotes = "INR lievemente alto. Riduzione modesta. Se ultimi 2 INR in range e causa transitoria, considerare non modificare.";
                result.UrgencyLevel = UrgencyLevel.Routine;
                break;

            case FasciaINR.SovraModerato:
                result.SospensioneDosi = 1; // Considerare
                result.PercentageAdjustment = -12.5m; // -10 a -15%
                result.SuggestedWeeklyDoseMg = currentDose * 0.875m;
                result.RequiresVitaminK = false;
                result.NextControlDays = 6; // 5-8
                result.LoadingDoseAction = "Considerare saltare 1 dose";
                result.ClinicalNotes = "INR moderatamente alto. Riduzione dose.";
                result.UrgencyLevel = UrgencyLevel.Routine;
                break;

            case FasciaINR.SovraAlto:
                result.SospensioneDosi = 1;
                result.PercentageAdjustment = -12.5m;
                result.SuggestedWeeklyDoseMg = currentDose * 0.875m;
                result.RequiresVitaminK = false; // FCSA: no Vit K sotto INR 5
                result.NextControlDays = 6; // 4-8
                result.LoadingDoseAction = "Saltare 1 dose";
                result.ClinicalNotes = "INR alto. Sospensione temporanea + riduzione dose.";
                result.UrgencyLevel = UrgencyLevel.Urgente;
                break;

            case FasciaINR.SovraMoltoAlto:
                result.SospensioneDosi = 2; // 1-2 dosi
                result.PercentageAdjustment = -17.5m; // -15 a -20%
                result.SuggestedWeeklyDoseMg = currentDose * 0.80m;
                result.RequiresVitaminK = true; // Opzionale, ma raccomandato
                result.VitaminKDoseMg = 2;
                result.VitaminKRoute = "Orale";
                result.NextControlDays = 5; // 4-7, controllo a 24h
                result.LoadingDoseAction = "Saltare 1-2 dosi";
                result.ClinicalNotes = "INR molto alto. Vitamina K 2mg PO se fattori rischio emorragico.";
                result.Warnings.Add("⚠️ Controllo INR a 24h raccomandato");
                result.UrgencyLevel = UrgencyLevel.Urgente;
                break;

            case FasciaINR.SovraCritico:
                result.SospensioneDosi = 3; // 2-3 dosi
                result.PercentageAdjustment = -20m;
                result.SuggestedWeeklyDoseMg = currentDose * 0.80m;
                result.RequiresVitaminK = true; // FCSA raccomanda
                result.VitaminKDoseMg = 2.5m; // 2-3mg
                result.VitaminKRoute = "Orale";
                result.NextControlDays = 1; // Controllo a 24h
                result.LoadingDoseAction = "Saltare 2-3 dosi";
                result.ClinicalNotes = "🔴 INR CRITICO: Vitamina K + sospensione.";
                result.Warnings.Add("🔴 URGENTE: Controllo INR a 24-48h obbligatorio");
                result.UrgencyLevel = UrgencyLevel.Emergenza;
                break;

            case FasciaINR.SovraEstremo:
                result.SospensioneDosi = null; // "Fino a INR < limite superiore range"
                result.PercentageAdjustment = -35m; // -20 a -50%, media
                result.SuggestedWeeklyDoseMg = currentDose * 0.65m;
                result.RequiresVitaminK = true; // OBBLIGATORIA
                result.VitaminKDoseMg = 4; // 3-5mg (FCSA)
                result.VitaminKRoute = "Orale";
                result.NextControlDays = 1;
                result.LoadingDoseAction = "STOP warfarin fino a INR < limite superiore range";
                result.ClinicalNotes = "🔴 EMERGENZA: INR ESTREMO. Vitamina K obbligatoria. Possibile seconda dose se INR ancora elevato.";
                result.Warnings.Add("🔴 EMERGENZA: Controllo INR a 24h e 48h");
                result.Warnings.Add("Valutare ricovero e monitoraggio intensivo");
                result.UrgencyLevel = UrgencyLevel.Emergenza;
                break;
        }

        result.SuggestedWeeklyDoseMg = RoundToSensibleDose(result.SuggestedWeeklyDoseMg);
    }

    /// <summary>
    /// Gestione INR sovraterapeutico CON emorragia attiva
    /// </summary>
    private void HandleAboveRangeConEmorragiaFCSA(
        DosageSuggestionResult result,
        TipoEmorragia tipoEmorragia,
        SedeEmorragia sedeEmorragia,
        decimal currentDose,
        decimal currentINR)
    {
        result.TipoEmorragia = tipoEmorragia;
        result.LoadingDoseAction = "STOP warfarin";
        result.UrgencyLevel = UrgencyLevel.Emergenza;

        switch (tipoEmorragia)
        {
            case TipoEmorragia.Minore:
                result.RequiresVitaminK = true;
                result.VitaminKRoute = "Orale";

                if (currentINR >= 5 && currentINR < 8)
                {
                    result.VitaminKDoseMg = 2;
                }
                else if (currentINR >= 8)
                {
                    result.VitaminKDoseMg = 4; // 3-5mg
                }

                result.NextControlDays = 1; // Controllo 24h
                result.PercentageAdjustment = -20m;
                result.SuggestedWeeklyDoseMg = currentDose * 0.80m;
                result.RequiresHospitalization = false; // Valutare
                result.ClinicalNotes = "Emorragia MINORE: STOP warfarin + Vitamina K PO. Ricerca causa locale.";
                result.Warnings.Add("⚠️ Valutare ospedalizzazione in base a contesto clinico");
                break;

            case TipoEmorragia.Maggiore:
                result.RequiresVitaminK = true;
                result.VitaminKDoseMg = 10;
                result.VitaminKRoute = "EV lenta (10-20 min)";

                // Fattori procoagulanti
                result.RequiresPCC = true;
                result.DosePCC = "20-50 UI/kg (PREFERIBILE)";
                result.RequiresPlasma = true; // Alternativa
                result.DosePlasma = "15 mL/kg (se PCC non disponibile)";

                result.RequiresHospitalization = true; // OBBLIGATORIA
                result.NextControlDays = 1;
                result.PercentageAdjustment = -50m; // Riduzione drastica
                result.SuggestedWeeklyDoseMg = currentDose * 0.50m;
                result.ClinicalNotes = "🔴 EMORRAGIA MAGGIORE: EMERGENZA - STOP warfarin + Vit K 10mg EV + PCC (preferito) o Plasma.";
                result.Warnings.Add("🔴 RICOVERO OBBLIGATORIO");
                result.Warnings.Add("Controllo INR post-PCC e seriato a 24-48h");
                result.Warnings.Add("Vitamina K ripetibile ogni 12h se necessario");
                break;

            case TipoEmorragia.RischioVitale:
                result.RequiresVitaminK = true;
                result.VitaminKDoseMg = 10;
                result.VitaminKRoute = "EV lenta";

                result.RequiresPCC = true;
                result.DosePCC = "20-50 UI/kg (PRIMA SCELTA)";

                result.RequiresHospitalization = true;
                result.NextControlDays = 0; // Monitoraggio continuo
                result.PercentageAdjustment = -100m; // STOP indefinito
                result.SuggestedWeeklyDoseMg = 0;
                result.ClinicalNotes = "🔴🔴 EMORRAGIA RISCHIO VITALE: EMERGENZA ASSOLUTA - STOP warfarin + Vit K 10mg EV + PCC immediato.";
                result.Warnings.Add("🔴 RICOVERO TERAPIA INTENSIVA IMMEDIATO");

                if (sedeEmorragia == SedeEmorragia.Intracranica)
                {
                    result.Warnings.Add("🔴 Imaging cerebrale URGENTE");
                }

                result.Warnings.Add("Valutazione chirurgica immediata se indicata");
                result.Warnings.Add("NOTA RIPRESA: Se necessario proseguire anticoagulazione, usare eparina per 7-10 giorni fino a scomparsa effetto vitamina K");
                break;
        }

        result.SuggestedWeeklyDoseMg = RoundToSensibleDose(result.SuggestedWeeklyDoseMg);
        result.FonteRaccomandazione = "FCSA";
    }

    #endregion

    #region ACCP Logic

    private void HandleInRangeACCP(DosageSuggestionResult result, TherapyPhase phase)
    {
        result.SuggestedWeeklyDoseMg = result.CurrentWeeklyDoseMg;
        result.PercentageAdjustment = 0;
        result.SospensioneDosi = 0; // No dose suspension needed
        result.LoadingDoseAction = "No loading dose required";
        result.ClinicalNotes = "✅ INR within therapeutic range. Continue current dose.";

        // ACCP permette intervalli più lunghi in mantenimento
        result.NextControlDays = phase switch
        {
            TherapyPhase.Induction => 7,
            TherapyPhase.PostAdjustment => 14,
            TherapyPhase.Maintenance => 42, // Fino a 12 settimane se stabile
            _ => 14
        };

        if (phase == TherapyPhase.Maintenance)
        {
            result.ClinicalNotes += " ACCP allows up to 12 weeks between checks if stable.";
        }
    }

    private void HandleBelowRangeACCP(
        DosageSuggestionResult result,
        decimal targetINRMin,
        decimal targetINRMax,
        ThromboembolicRisk risk)
    {
        // ACCP è generalmente allineato a FCSA per INR basso
        // Logica simile ma leggermente più conservativa
        var inr = result.CurrentINR;
        var currentDose = result.CurrentWeeklyDoseMg;

        if (inr >= 1.8m && inr < 2.0m)
        {
            result.LoadingDoseAction = "Increase today's dose by 25%";
            result.PercentageAdjustment = 0;
            result.SuggestedWeeklyDoseMg = currentDose;
            result.NextControlDays = 14;
            result.RequiresEBPM = false;
        }
        else if (inr >= 1.5m && inr < 1.8m)
        {
            result.LoadingDoseAction = "Increase today's dose by 50%";
            result.PercentageAdjustment = 5; // Più conservativo di FCSA
            result.SuggestedWeeklyDoseMg = currentDose * 1.05m;
            result.NextControlDays = 7;
            result.RequiresEBPM = risk == ThromboembolicRisk.High;
            
            if (result.RequiresEBPM)
            {
                result.Warnings.Add("⚠️ Consider therapeutic EBPM bridging (high TE risk)");
            }
        }
        else if (inr < 1.5m)
        {
            result.LoadingDoseAction = "Double usual dose today";
            result.PercentageAdjustment = 7.5m;
            result.SuggestedWeeklyDoseMg = currentDose * 1.075m;
            result.NextControlDays = 5;
            result.RequiresEBPM = risk == ThromboembolicRisk.High;
            result.Warnings.Add("⚠️ CRITICAL: INR <1.5 - Assess causes");
        }

        result.SuggestedWeeklyDoseMg = RoundToSensibleDose(result.SuggestedWeeklyDoseMg);
    }

    private void HandleAboveRangeACCP(DosageSuggestionResult result, decimal targetINRMax)
    {
        var inr = result.CurrentINR;
        var currentDose = result.CurrentWeeklyDoseMg;

        // DIFFERENZA CHIAVE ACCP: soglia Vitamina K più alta (INR >10)
        if (inr > targetINRMax && inr <= 10.0m)
        {
            if (inr <= 5.0m)
            {
                result.LoadingDoseAction = "Reduce dose by 50% today or skip 1 day";
                result.PercentageAdjustment = -5m; // Più conservativo
                result.SuggestedWeeklyDoseMg = currentDose * 0.95m;
                result.NextControlDays = 7;
                result.RequiresVitaminK = false;
            }
            else if (inr <= 10.0m)
            {
                result.LoadingDoseAction = "Hold warfarin, monitor closely";
                result.PercentageAdjustment = -10m;
                result.SuggestedWeeklyDoseMg = currentDose * 0.90m;
                result.NextControlDays = 3;
                result.RequiresVitaminK = false; // NO Vit K fino a 10
                result.ClinicalNotes = "ACCP: No routine Vitamin K for INR 5-10 without bleeding";
                result.Warnings.Add("⚠️ Monitor for bleeding signs");
            }
        }
        else if (inr > 10.0m)
        {
            // INR >10: Vitamina K anche per ACCP
            result.LoadingDoseAction = "HOLD warfarin + Vitamin K 2.5-5mg PO";
            result.PercentageAdjustment = -15m;
            result.SuggestedWeeklyDoseMg = currentDose * 0.85m;
            result.NextControlDays = 1;
            result.RequiresVitaminK = true;
            result.VitaminKDoseMg = 5;
            result.VitaminKRoute = "Oral";
            result.Warnings.Add("🔴 CRITICAL: INR >10 - High bleeding risk");
        }

        result.SuggestedWeeklyDoseMg = RoundToSensibleDose(result.SuggestedWeeklyDoseMg);
    }

    /// <summary>
    /// Gestione INR sovraterapeutico SENZA emorragia - Algoritmo ACCP
    /// DIFFERENZE: Vitamina K solo se INR >10, aggiustamenti più conservativi
    /// </summary>
    private void HandleAboveRangeSenzaEmorragiaACCP(
        DosageSuggestionResult result,
        FasciaINR fascia,
        decimal currentDose,
        decimal currentINR)
    {
        switch (fascia)
        {
            case FasciaINR.SovraLieve:
                result.SospensioneDosi = 0;
                result.PercentageAdjustment = -5m; // ACCP più conservativo
                result.SuggestedWeeklyDoseMg = currentDose * 0.95m;
                result.RequiresVitaminK = false;
                result.NextControlDays = 10; // 7-14
                result.LoadingDoseAction = "No dose adjustment or reduce by 5%";
                result.ClinicalNotes = "Slightly elevated INR. Minor dose reduction or observe.";
                result.UrgencyLevel = UrgencyLevel.Routine;
                break;

            case FasciaINR.SovraModerato:
                result.SospensioneDosi = 1;
                result.PercentageAdjustment = -7.5m; // Più conservativo di FCSA
                result.SuggestedWeeklyDoseMg = currentDose * 0.925m;
                result.RequiresVitaminK = false;
                result.NextControlDays = 7;
                result.LoadingDoseAction = "Consider holding 1 dose";
                result.ClinicalNotes = "Moderately elevated INR. Dose reduction recommended.";
                result.UrgencyLevel = UrgencyLevel.Routine;
                break;

            case FasciaINR.SovraAlto:
                result.SospensioneDosi = 1;
                result.PercentageAdjustment = -10m;
                result.SuggestedWeeklyDoseMg = currentDose * 0.90m;
                result.RequiresVitaminK = false; // ACCP: NO Vit K sotto 10
                result.NextControlDays = 5;
                result.LoadingDoseAction = "Hold 1 dose";
                result.ClinicalNotes = "High INR. Hold warfarin, reduce dose. ACCP: No routine Vitamin K below INR 10.";
                result.UrgencyLevel = UrgencyLevel.Urgente;
                break;

            case FasciaINR.SovraMoltoAlto:
                result.SospensioneDosi = 2;
                result.PercentageAdjustment = -15m;
                result.SuggestedWeeklyDoseMg = currentDose * 0.85m;
                result.RequiresVitaminK = false; // ACCP: ancora no Vit K
                result.NextControlDays = 3;
                result.LoadingDoseAction = "Hold 1-2 doses";
                result.ClinicalNotes = "Very high INR. Monitor closely for bleeding signs. ACCP: No routine Vitamin K.";
                result.Warnings.Add("⚠️ Monitor for bleeding signs");
                result.UrgencyLevel = UrgencyLevel.Urgente;
                break;

            case FasciaINR.SovraCritico:
                // INR 6-7.9 (target 2-3) o 6.5-8.4 (target 2.5-3.5)
                if (currentINR < 10)
                {
                    result.SospensioneDosi = 2;
                    result.PercentageAdjustment = -15m;
                    result.SuggestedWeeklyDoseMg = currentDose * 0.85m;
                    result.RequiresVitaminK = false; // ACCP: NO Vit K sotto 10
                    result.NextControlDays = 2;
                    result.LoadingDoseAction = "Hold warfarin, monitor closely";
                    result.ClinicalNotes = "🔴 CRITICAL INR but <10. ACCP: NO routine Vitamin K without bleeding. Close monitoring.";
                    result.Warnings.Add("🔴 URGENT: Monitor for bleeding signs");
                    result.UrgencyLevel = UrgencyLevel.Emergenza;
                }
                else // INR ≥10
                {
                    result.SospensioneDosi = 3;
                    result.PercentageAdjustment = -20m;
                    result.SuggestedWeeklyDoseMg = currentDose * 0.80m;
                    result.RequiresVitaminK = true; // ACCP: Vit K se ≥10
                    result.VitaminKDoseMg = 5; // 2.5-5mg
                    result.VitaminKRoute = "Oral";
                    result.NextControlDays = 1;
                    result.LoadingDoseAction = "HOLD warfarin + Vitamin K";
                    result.ClinicalNotes = "🔴 CRITICAL: INR ≥10. ACCP recommends Vitamin K 2.5-5mg PO.";
                    result.Warnings.Add("🔴 URGENT: Check INR at 24h");
                    result.UrgencyLevel = UrgencyLevel.Emergenza;
                }
                break;

            case FasciaINR.SovraEstremo:
                // INR ≥8 (target 2-3) o ≥8.5 (target 2.5-3.5) - SEMPRE ≥10 per ACCP
                result.SospensioneDosi = null;
                result.PercentageAdjustment = -30m;
                result.SuggestedWeeklyDoseMg = currentDose * 0.70m;
                result.RequiresVitaminK = true; // OBBLIGATORIA
                result.VitaminKDoseMg = 5; // 2.5-5mg (ACCP)
                result.VitaminKRoute = "Oral";
                result.NextControlDays = 1;
                result.LoadingDoseAction = "STOP warfarin + Vitamin K";
                result.ClinicalNotes = "🔴 EXTREME INR. Vitamin K mandatory. Hold until INR normalizes.";
                result.Warnings.Add("🔴 EMERGENCY: Check INR at 24h and 48h");
                result.Warnings.Add("Consider hospitalization for monitoring");
                result.UrgencyLevel = UrgencyLevel.Emergenza;
                break;
        }

        result.SuggestedWeeklyDoseMg = RoundToSensibleDose(result.SuggestedWeeklyDoseMg);
    }

    #endregion

    #region Weekly Schedule Generation

    public WeeklyDoseSchedule GenerateWeeklySchedule(decimal weeklyDoseMg, decimal? loadingDoseDay1 = null)
        {
        if (weeklyDoseMg <= 0)
            {
            throw new ArgumentException("Dose settimanale deve essere maggiore di zero", nameof(weeklyDoseMg));
            }

        // Arrotonda a dose gestibile (multipli di 2.5mg)
        var roundedDose = RoundToSensibleDose(weeklyDoseMg);

        var schedule = new WeeklyDoseSchedule
            {
            TotalWeeklyDose = roundedDose
            };

        // Numero compresse da 5mg al giorno (base)
        int wholePills = (int) (roundedDose / 5m) / 7;
        decimal remainder = roundedDose - (wholePills * 5m * 7);

        // Distribuzione remainder
        int halfPillDays = (int) (remainder / 2.5m);

        // Assegna dose base a tutti i giorni
        decimal baseDaily = wholePills * 5m;
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
            schedule.DailyDoses[day] = baseDaily;
            schedule.DailyDescriptions[day] = FormatDose(baseDaily);
            }

        // Distribuisci mezze compresse uniformemente
        if (halfPillDays >= 7)
            {
            // Aggiungi 2.5mg a tutti i giorni
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                schedule.DailyDoses[day] += 2.5m;
                schedule.DailyDescriptions[day] = FormatDose(schedule.DailyDoses[day]);
                }
            halfPillDays -= 7;
            }

        // Distribuisci rimanenti mezze compresse su giorni alternati
        var daysToAddHalf = new[] {
        DayOfWeek.Sunday, DayOfWeek.Wednesday, DayOfWeek.Friday,
        DayOfWeek.Monday, DayOfWeek.Thursday, DayOfWeek.Tuesday, DayOfWeek.Saturday
    };

        for (int i = 0; i < halfPillDays && i < 7; i++)
            {
            var day = daysToAddHalf[i];
            schedule.DailyDoses[day] += 2.5m;
            schedule.DailyDescriptions[day] = FormatDose(schedule.DailyDoses[day]);
            }

        // Aggiungi dose carico al primo giorno (oggi - assumiamo sia il giorno corrente)
        if (loadingDoseDay1.HasValue && loadingDoseDay1.Value > 0)
        {
            var today = DateTime.Today.DayOfWeek;
            var roundedLoadingDose = RoundToSensibleDose(loadingDoseDay1.Value);
            schedule.DailyDoses[today] += roundedLoadingDose;
            schedule.DailyDescriptions[today] = FormatDose(schedule.DailyDoses[today]);

            // Aggiorna totale settimanale effettivo
            schedule.TotalWeeklyDose += roundedLoadingDose;
        }

        // Genera descrizione formattata
        schedule.Description = FormatScheduleDescription(schedule);

        return schedule;
        }

    private string FormatScheduleDescription(WeeklyDoseSchedule schedule)
        {
        var allDoses = schedule.DailyDoses.Values.ToList();

        // Trova pattern comuni
        if (allDoses.All(d => d == allDoses[0]))
            {
            return $"Tutti i giorni: {FormatDose(allDoses[0])}";
            }

        // Conta occorrenze
        var groups = allDoses.GroupBy(d => d)
            .OrderByDescending(g => g.Count())
            .ToList();

        if (groups.Count == 1)
            {
            return $"Tutti i giorni: {FormatDose(groups[0].Key)}";
            }

        if (groups.Count == 2)
            {
            var main = groups[0];
            var alt = groups[1];
            return $"{main.Count()} giorni: {FormatDose(main.Key)}, {alt.Count()} giorni: {FormatDose(alt.Key)}";
            }

        // Descrizione dettagliata
        return "Schema variabile (vedi dettaglio giornaliero)";
        }

    private string FormatDose(decimal doseMg)
        {
        if (doseMg == 0) return "Nessuna dose";
        if (doseMg == 2.5m) return "1/2 cp (2.5 mg)";
        if (doseMg == 5.0m) return "1 cp (5 mg)";
        if (doseMg == 7.5m) return "1 cp + 1/2 cp (7.5 mg)";
        if (doseMg == 10.0m) return "2 cp (10 mg)";
        return $"{doseMg} mg";
        }

    #endregion

    #region Vitamin K & EBPM Evaluation

    public bool RequiresEBPM(decimal inr, ThromboembolicRisk risk, GuidelineType guideline)
    {
        // EBPM raccomandato se INR molto basso E rischio TE alto/moderato
        if (guideline == GuidelineType.FCSA)
        {
            if (inr < 1.5m && risk >= ThromboembolicRisk.Moderate)
                return true;
            if (inr < 1.7m && risk == ThromboembolicRisk.High)
                return true;
        }
        else // ACCP
        {
            // ACCP più conservativo: solo se rischio alto
            if (inr < 1.5m && risk == ThromboembolicRisk.High)
                return true;
        }

        return false;
    }

    public VitaminKRecommendation EvaluateVitaminK(decimal inr, GuidelineType guideline, bool hasBleeding = false)
    {
        var recommendation = new VitaminKRecommendation();

        if (hasBleeding)
        {
            // Con sanguinamento: Vit K sempre raccomandata se INR >4
            if (inr > 4.0m)
            {
                recommendation.IsRecommended = true;
                recommendation.DoseMg = inr > 10 ? 10 : 5;
                recommendation.Route = "EV lenta (o PO se sanguinamento minore)";
                recommendation.Urgency = "ALTA";
                recommendation.Notes = "Sanguinamento attivo: considerare PCC se emorragia maggiore";
            }
        }
        else
        {
            // Senza sanguinamento
            if (guideline == GuidelineType.FCSA)
            {
                // FCSA: Vit K se INR >6
                if (inr > 6.0m)
                {
                    recommendation.IsRecommended = true;
                    recommendation.DoseMg = inr > 10 ? 5 : 2;
                    recommendation.Route = "Orale";
                    recommendation.Urgency = inr > 10 ? "ALTA" : "MODERATA";
                    recommendation.Notes = "FCSA: Vitamina K per INR >6 anche senza sanguinamento";
                }
            }
            else // ACCP
            {
                // ACCP: Vit K solo se INR >10
                if (inr > 10.0m)
                {
                    recommendation.IsRecommended = true;
                    recommendation.DoseMg = 5;
                    recommendation.Route = "Orale";
                    recommendation.Urgency = "ALTA";
                    recommendation.Notes = "ACCP: Vitamina K per INR >10 senza sanguinamento";
                }
                else if (inr > 6.0m)
                {
                    recommendation.Notes = "ACCP: NO Vitamina K routinaria per INR 6-10 senza sanguinamento. Monitorare.";
                }
            }
        }

        return recommendation;
    }

    #endregion

    #region Helper Methods

    private static void ValidateInputs(decimal inr, decimal targetMin, decimal targetMax, decimal currentDose)
    {
        if (inr <= 0 || inr > 20)
            throw new ArgumentOutOfRangeException(nameof(inr), "INR deve essere tra 0 e 20");

        if (targetMin <= 0 || targetMin > 5)
            throw new ArgumentOutOfRangeException(nameof(targetMin), "Target INR min deve essere tra 0 e 5");

        if (targetMax <= targetMin || targetMax > 5)
            throw new ArgumentException("Target INR max deve essere maggiore di min e ≤5", nameof(targetMax));

        if (currentDose <= 0 || currentDose > 100)
            throw new ArgumentOutOfRangeException(nameof(currentDose), "Dose settimanale deve essere tra 0 e 100mg");
    }

    private static bool IsINRInRange(decimal inr, decimal min, decimal max)
    {
        return inr >= min && inr <= max;
    }

    private static INRStatus EvaluateINRStatus(decimal inr, decimal min, decimal max)
    {
        if (inr >= min && inr <= max)
            return INRStatus.InRange;
        if (inr < min)
            return INRStatus.BelowRange;
        return INRStatus.AboveRange;
    }

    private static decimal RoundToSensibleDose(decimal dose)
    {
        // Arrotonda a multipli di 2.5mg (mezza compressa)
        return Math.Round(dose / 2.5m) * 2.5m;
    }

    #endregion

    #region New Algorithm Helpers

    /// <summary>
    /// Determina la fascia INR per valori sottoterapeutici
    /// </summary>
    private static FasciaINR DeterminaFasciaINRBasso(decimal inr, decimal targetMin, decimal targetMax)
    {
        bool isHighTarget = targetMax >= 3.5m;

        if (!isHighTarget) // Target 2.0-3.0
        {
            if (inr >= 1.8m && inr < 2.0m) return FasciaINR.SubLieve;
            if (inr >= 1.5m && inr < 1.8m) return FasciaINR.SubModerato;
            if (inr < 1.5m) return FasciaINR.SubCritico;
        }
        else // Target 2.5-3.5
        {
            if (inr >= 2.3m && inr < 2.5m) return FasciaINR.SubLieve;
            if (inr >= 2.0m && inr < 2.3m) return FasciaINR.SubModerato;
            if (inr < 2.0m) return FasciaINR.SubCritico;
        }

        return FasciaINR.InRange; // Fallback
    }

    /// <summary>
    /// Determina la fascia INR per valori sovraterapeutici
    /// </summary>
    private static FasciaINR DeterminaFasciaINRAlto(decimal inr, decimal targetMin, decimal targetMax)
    {
        bool isHighTarget = targetMax >= 3.5m;

        if (!isHighTarget) // Target 2.0-3.0
        {
            if (inr >= 3.1m && inr <= 3.4m) return FasciaINR.SovraLieve;
            if (inr >= 3.5m && inr <= 3.9m) return FasciaINR.SovraModerato;
            if (inr >= 4.0m && inr <= 4.9m) return FasciaINR.SovraAlto;
            if (inr >= 5.0m && inr <= 5.9m) return FasciaINR.SovraMoltoAlto;
            if (inr >= 6.0m && inr <= 7.9m) return FasciaINR.SovraCritico;
            if (inr >= 8.0m) return FasciaINR.SovraEstremo;
        }
        else // Target 2.5-3.5
        {
            if (inr >= 3.6m && inr <= 3.9m) return FasciaINR.SovraLieve;
            if (inr >= 4.0m && inr <= 4.4m) return FasciaINR.SovraModerato;
            if (inr >= 4.5m && inr <= 5.4m) return FasciaINR.SovraAlto;
            if (inr >= 5.5m && inr <= 6.4m) return FasciaINR.SovraMoltoAlto;
            if (inr >= 6.5m && inr <= 8.4m) return FasciaINR.SovraCritico;
            if (inr >= 8.5m) return FasciaINR.SovraEstremo;
        }

        return FasciaINR.InRange; // Fallback
    }

    /// <summary>
    /// Calcola se il paziente ha un rischio tromboembolico elevato
    /// Basato su: protesi meccanica, TEV recente, CHA2DS2-VASc elevato
    /// </summary>
    private static bool CalcolaRischioTromboticoElevato(
        bool hasProtesiMeccanica,
        DateTime? dataUltimoTEV,
        string indicazioneTAO,
        int cha2ds2vasc)
    {
        // Protesi meccanica = sempre alto rischio
        if (hasProtesiMeccanica) return true;

        // TEV recente (<3 mesi)
        if (dataUltimoTEV.HasValue)
        {
            var giorniDaUltimoTEV = (DateTime.Now - dataUltimoTEV.Value).Days;
            if (giorniDaUltimoTEV < 90) return true;
        }

        // FA con CHA2DS2-VASc ≥4
        if (indicazioneTAO.Equals("FA", StringComparison.OrdinalIgnoreCase) && cha2ds2vasc >= 4)
            return true;

        // TEV molto recente (<30 giorni)
        if (indicazioneTAO.Equals("TEV", StringComparison.OrdinalIgnoreCase) && dataUltimoTEV.HasValue)
        {
            var giorniDaUltimoTEV = (DateTime.Now - dataUltimoTEV.Value).Days;
            if (giorniDaUltimoTEV < 30) return true;
        }

        return false;
    }

    #endregion
}
