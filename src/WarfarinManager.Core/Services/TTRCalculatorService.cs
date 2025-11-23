using Microsoft.Extensions.Logging;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Core.Models;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Implementazione calcolo TTR con metodo Rosendaal
/// </summary>
public class TTRCalculatorService : ITTRCalculatorService
{
    private readonly ILogger<TTRCalculatorService> _logger;

    public TTRCalculatorService(ILogger<TTRCalculatorService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public TTRResult CalculateTTR(
        IEnumerable<INRControl> controls,
        decimal targetMin,
        decimal targetMax)
    {
        if (controls == null || !controls.Any())
        {
            return CreateEmptyResult(targetMin, targetMax, "Dati insufficienti per calcolo TTR (minimo 2 controlli richiesti)");
        }

        var orderedControls = controls.OrderBy(c => c.ControlDate).ToList();

        if (orderedControls.Count < 2)
        {
            return CreateEmptyResult(targetMin, targetMax, "Dati insufficienti per calcolo TTR (minimo 2 controlli richiesti)");
        }

        try
        {
            // Interpolazione INR giornaliero (metodo Rosendaal)
            var dailyINR = InterpolateINR(orderedControls);

            // Conta giorni in/sotto/sopra range
            int daysInRange = 0;
            int daysBelowRange = 0;
            int daysAboveRange = 0;
            int totalDays = dailyINR.Count;

            foreach (var inr in dailyINR.Values)
            {
                if (inr >= targetMin && inr <= targetMax)
                {
                    daysInRange++;
                }
                else if (inr < targetMin)
                {
                    daysBelowRange++;
                }
                else
                {
                    daysAboveRange++;
                }
            }

            decimal ttrPercentage = totalDays > 0 
                ? Math.Round((decimal)daysInRange / totalDays * 100, 1)
                : 0;

            var quality = EvaluateQuality(ttrPercentage);

            // Calcola statistiche INR
            var inrValues = orderedControls.Select(c => c.INRValue).ToList();
            var inrStats = CalculateINRStatistics(orderedControls);

            var result = new TTRResult
            {
                TTRPercentage = ttrPercentage,
                TotalDays = totalDays,
                DaysInRange = daysInRange,
                DaysBelowRange = daysBelowRange,
                DaysAboveRange = daysAboveRange,
                Quality = quality,
                TargetINRMin = targetMin,
                TargetINRMax = targetMax,
                StartDate = orderedControls.First().ControlDate,
                EndDate = orderedControls.Last().ControlDate,
                NumberOfControls = orderedControls.Count,
                AverageINR = inrStats.Mean,
                INRStandardDeviation = inrStats.StandardDeviation
            };

            _logger.LogInformation(
                "Calcolo TTR completato: {TTR}% su {Days} giorni ({Controls} controlli)",
                ttrPercentage, totalDays, orderedControls.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante calcolo TTR");
            throw;
        }
    }

    public TTRResult CalculateTTR(
        IEnumerable<INRControl> controls,
        decimal targetMin,
        decimal targetMax,
        DateTime startDate,
        DateTime endDate)
    {
        if (startDate >= endDate)
        {
            throw new ArgumentException("Data inizio deve essere precedente a data fine");
        }

        // Filtra controlli nel periodo
        var controlsInPeriod = controls
            .Where(c => c.ControlDate >= startDate && c.ControlDate <= endDate)
            .ToList();

        // Aggiungi controllo immediatamente precedente (se esiste) per interpolazione corretta
        var previousControl = controls
            .Where(c => c.ControlDate < startDate)
            .OrderByDescending(c => c.ControlDate)
            .FirstOrDefault();

        if (previousControl != null)
        {
            controlsInPeriod.Insert(0, previousControl);
        }

        var result = CalculateTTR(controlsInPeriod, targetMin, targetMax);

        // Aggiusta per periodo effettivo se necessario
        if (previousControl != null && controlsInPeriod.Count > 1)
        {
            result.StartDate = startDate;
        }

        return result;
    }

    public Dictionary<DateTime, decimal> CalculateTTRTrend(
        IEnumerable<INRControl> controls,
        decimal targetMin,
        decimal targetMax,
        int rollingWindowMonths = 3)
    {
        if (rollingWindowMonths < 1 || rollingWindowMonths > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(rollingWindowMonths), 
                "Finestra rolling deve essere tra 1 e 12 mesi");
        }

        var orderedControls = controls.OrderBy(c => c.ControlDate).ToList();

        if (orderedControls.Count < 2)
        {
            return new Dictionary<DateTime, decimal>();
        }

        var result = new Dictionary<DateTime, decimal>();

        var firstDate = orderedControls.First().ControlDate;
        var lastDate = orderedControls.Last().ControlDate;

        // Calcola TTR per ogni finestra mobile
        var currentDate = firstDate.AddMonths(rollingWindowMonths);

        while (currentDate <= lastDate)
        {
            var windowStart = currentDate.AddMonths(-rollingWindowMonths);
            var windowEnd = currentDate;

            var ttrForWindow = CalculateTTR(
                orderedControls,
                targetMin,
                targetMax,
                windowStart,
                windowEnd);

            if (ttrForWindow.TotalDays > 0)
            {
                result[currentDate] = ttrForWindow.TTRPercentage;
            }

            currentDate = currentDate.AddMonths(1);
        }

        return result;
    }

    public Dictionary<DateTime, decimal> InterpolateINR(IEnumerable<INRControl> controls)
    {
        var result = new Dictionary<DateTime, decimal>();

        var orderedControls = controls.OrderBy(c => c.ControlDate).ToList();

        if (orderedControls.Count < 2)
        {
            return result;
        }

        // Interpolazione lineare tra ogni coppia di controlli consecutivi
        for (int i = 0; i < orderedControls.Count - 1; i++)
        {
            var control1 = orderedControls[i];
            var control2 = orderedControls[i + 1];

            var date1 = control1.ControlDate.Date;
            var date2 = control2.ControlDate.Date;
            var inr1 = control1.INRValue;
            var inr2 = control2.INRValue;

            int totalDays = (date2 - date1).Days;

            if (totalDays == 0)
            {
                // Stesso giorno: usa secondo valore
                result[date2] = inr2;
                continue;
            }

            // Interpolazione giorno per giorno
            for (int day = 0; day <= totalDays; day++)
            {
                var currentDate = date1.AddDays(day);
                
                // Formula interpolazione lineare
                decimal interpolatedINR = inr1 + (inr2 - inr1) * (day / (decimal)totalDays);
                
                result[currentDate] = Math.Round(interpolatedINR, 2);
            }
        }

        _logger.LogDebug("Interpolazione completata: {Days} giorni da {Controls} controlli",
            result.Count, orderedControls.Count);

        return result;
    }

    public TTRQuality EvaluateQuality(decimal ttrPercentage)
    {
        // Classificazione secondo linee guida FCSA
        if (ttrPercentage >= 70)
            return TTRQuality.Excellent;
        if (ttrPercentage >= 65)
            return TTRQuality.Good;
        if (ttrPercentage >= 60)
            return TTRQuality.Acceptable;
        if (ttrPercentage >= 50)
            return TTRQuality.Suboptimal;
        
        return TTRQuality.Poor;
    }

    public INRStatistics CalculateINRStatistics(IEnumerable<INRControl> controls)
    {
        var controlsList = controls.ToList();
        
        if (!controlsList.Any())
        {
            return new INRStatistics { Count = 0 };
        }

        var inrValues = controlsList.Select(c => c.INRValue).OrderBy(v => v).ToList();

        return new INRStatistics
        {
            Count = inrValues.Count,
            Mean = Math.Round(inrValues.Average(), 2),
            StandardDeviation = CalculateStandardDeviation(inrValues),
            Min = inrValues.Min(),
            Max = inrValues.Max(),
            Median = CalculateMedian(inrValues)
        };
    }

    #region Private Methods

    private TTRResult CreateEmptyResult(decimal targetMin, decimal targetMax, string message)
    {
        return new TTRResult
        {
            TTRPercentage = 0,
            TotalDays = 0,
            DaysInRange = 0,
            DaysBelowRange = 0,
            DaysAboveRange = 0,
            Quality = TTRQuality.Poor,
            TargetINRMin = targetMin,
            TargetINRMax = targetMax,
            NumberOfControls = 0
        };
    }

    private static decimal CalculateStandardDeviation(List<decimal> values)
    {
        if (values.Count < 2)
            return 0;

        decimal average = values.Average();
        decimal sumOfSquares = values.Sum(val => (val - average) * (val - average));
        decimal variance = sumOfSquares / (values.Count - 1);
        
        return Math.Round((decimal)Math.Sqrt((double)variance), 2);
    }

    private static decimal CalculateMedian(List<decimal> sortedValues)
    {
        int count = sortedValues.Count;
        if (count == 0) return 0;

        if (count % 2 == 1)
        {
            return sortedValues[count / 2];
        }
        else
        {
            return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2m;
        }
    }

    #endregion
}
