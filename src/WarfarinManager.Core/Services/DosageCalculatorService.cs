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
        ThromboembolicRisk thromboembolicRisk = ThromboembolicRisk.Moderate)
    {
        ValidateInputs(currentINR, targetINRMin, targetINRMax, currentWeeklyDoseMg);

        var result = new DosageSuggestionResult
        {
            GuidelineUsed = GuidelineType.FCSA,
            CurrentINR = currentINR,
            TargetINRMin = targetINRMin,
            TargetINRMax = targetINRMax,
            CurrentWeeklyDoseMg = currentWeeklyDoseMg,
            IsInRange = IsINRInRange(currentINR, targetINRMin, targetINRMax)
        };

        // Valutazione posizione INR
        var inrStatus = EvaluateINRStatus(currentINR, targetINRMin, targetINRMax);
        result.INRStatus = inrStatus;

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

        // Calcolo basato su stato INR
        switch (inrStatus)
        {
            case INRStatus.InRange:
                HandleInRangeFCSA(result, phase);
                break;

            case INRStatus.BelowRange:
                HandleBelowRangeFCSA(result, targetINRMin, targetINRMax, thromboembolicRisk, isSlowMetabolizer);
                break;

            case INRStatus.AboveRange:
                HandleAboveRangeFCSA(result, targetINRMax);
                break;
        }

        // Genera schema settimanale
        result.WeeklySchedule = GenerateWeeklySchedule(result.SuggestedWeeklyDoseMg);

        _logger.LogInformation(
            "Calcolo FCSA: INR {INR} (target {Min}-{Max}) → Nuova dose {NewDose}mg, controllo tra {Days} giorni",
            currentINR, targetINRMin, targetINRMax, result.SuggestedWeeklyDoseMg, result.NextControlDays);

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
        ThromboembolicRisk thromboembolicRisk = ThromboembolicRisk.Moderate)
    {
        ValidateInputs(currentINR, targetINRMin, targetINRMax, currentWeeklyDoseMg);

        var result = new DosageSuggestionResult
        {
            GuidelineUsed = GuidelineType.ACCP,
            CurrentINR = currentINR,
            TargetINRMin = targetINRMin,
            TargetINRMax = targetINRMax,
            CurrentWeeklyDoseMg = currentWeeklyDoseMg,
            IsInRange = IsINRInRange(currentINR, targetINRMin, targetINRMax)
        };

        var inrStatus = EvaluateINRStatus(currentINR, targetINRMin, targetINRMax);
        result.INRStatus = inrStatus;

        if (!isCompliant)
        {
            result.Warnings.Add("⚠️ WARNING: Poor compliance verified. Assess causes before dose adjustment.");
        }

        // Calcolo basato su stato INR - logica ACCP
        switch (inrStatus)
        {
            case INRStatus.InRange:
                HandleInRangeACCP(result, phase);
                break;

            case INRStatus.BelowRange:
                HandleBelowRangeACCP(result, targetINRMin, targetINRMax, thromboembolicRisk);
                break;

            case INRStatus.AboveRange:
                HandleAboveRangeACCP(result, targetINRMax);
                break;
        }

        result.WeeklySchedule = GenerateWeeklySchedule(result.SuggestedWeeklyDoseMg);

        _logger.LogInformation(
            "Calcolo ACCP: INR {INR} (target {Min}-{Max}) → Nuova dose {NewDose}mg, controllo tra {Days} giorni",
            currentINR, targetINRMin, targetINRMax, result.SuggestedWeeklyDoseMg, result.NextControlDays);

        return result;
    }

    #region FCSA Logic

    private void HandleInRangeFCSA(DosageSuggestionResult result, TherapyPhase phase)
    {
        result.SuggestedWeeklyDoseMg = result.CurrentWeeklyDoseMg;
        result.PercentageAdjustment = 0;
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

    #endregion

    #region ACCP Logic

    private void HandleInRangeACCP(DosageSuggestionResult result, TherapyPhase phase)
    {
        result.SuggestedWeeklyDoseMg = result.CurrentWeeklyDoseMg;
        result.PercentageAdjustment = 0;
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

    #endregion

    #region Weekly Schedule Generation

    public WeeklyDoseSchedule GenerateWeeklySchedule(decimal weeklyDoseMg)
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
}
