using System;

namespace WarfarinManager.UI.Models
{
    /// <summary>
    /// Modello personalizzato per i punti del grafico INR con tooltip personalizzati
    /// </summary>
    public class INRChartPoint
    {
        public DateTime Date { get; set; }
        public double INRValue { get; set; }
        public decimal WeeklyDose { get; set; }
        public bool IsInRange { get; set; }
        public string Phase { get; set; } = string.Empty;

        /// <summary>
        /// Tooltip formattato per il grafico - LiveCharts2 usa ToString() per i tooltip
        /// </summary>
        public override string ToString()
        {
            return $"Data: {Date:dd/MM/yyyy}\nINR: {INRValue:F2}\nDose: {WeeklyDose:F1} mg/sett";
        }
    }
}
