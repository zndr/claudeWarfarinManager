using WarfarinManager.Core.Models;

namespace WarfarinManager.Core.Interfaces
    {
    /// <summary>
    /// Servizio per il calcolo del TTR (Time in Therapeutic Range)
    /// </summary>
    public interface ITTRCalculatorService
        {
        /// <summary>
        /// Calcola il TTR utilizzando il metodo Rosendaal (interpolazione lineare)
        /// </summary>
        /// <param name="controls">Lista di controlli INR ordinati cronologicamente</param>
        /// <param name="targetMin">Valore minimo target INR</param>
        /// <param name="targetMax">Valore massimo target INR</param>
        /// <returns>Risultato del calcolo TTR con statistiche complete</returns>
        TTRResult CalculateTTR(
            IEnumerable<INRControl> controls,
            decimal targetMin,
            decimal targetMax);

        /// <summary>
        /// Calcola il TTR per un periodo specifico
        /// </summary>
        /// <param name="controls">Lista di controlli INR ordinati cronologicamente</param>
        /// <param name="targetMin">Valore minimo target INR</param>
        /// <param name="targetMax">Valore massimo target INR</param>
        /// <param name="startDate">Data inizio periodo</param>
        /// <param name="endDate">Data fine periodo</param>
        /// <returns>Risultato del calcolo TTR per il periodo specificato</returns>
        TTRResult CalculateTTR(
            IEnumerable<INRControl> controls,
            decimal targetMin,
            decimal targetMax,
            DateTime startDate,
            DateTime endDate);

        /// <summary>
        /// Calcola trend TTR con finestra mobile (rolling window)
        /// </summary>
        /// <param name="controls">Lista di controlli INR ordinati cronologicamente</param>
        /// <param name="targetMin">Valore minimo target INR</param>
        /// <param name="targetMax">Valore massimo target INR</param>
        /// <param name="rollingWindowMonths">Dimensione finestra in mesi (default: 3)</param>
        /// <returns>Dizionario con data e TTR per ogni punto della finestra mobile</returns>
        Dictionary<DateTime, decimal> CalculateTTRTrend(
            IEnumerable<INRControl> controls,
            decimal targetMin,
            decimal targetMax,
            int rollingWindowMonths = 3);

        /// <summary>
        /// Interpola valori INR giornalieri tra controlli consecutivi
        /// </summary>
        /// <param name="controls">Lista di controlli INR ordinati cronologicamente</param>
        /// <returns>Dizionario con data e valore INR interpolato per ogni giorno</returns>
        Dictionary<DateTime, decimal> InterpolateINR(IEnumerable<INRControl> controls);

        /// <summary>
        /// Valuta la qualità del controllo TAO basandosi sul TTR
        /// </summary>
        /// <param name="ttrPercentage">Percentuale TTR (0-100)</param>
        /// <returns>Livello di qualità secondo classificazione FCSA</returns>
        TTRQuality EvaluateQuality(decimal ttrPercentage);

        /// <summary>
        /// Calcola statistiche aggregate INR
        /// </summary>
        /// <param name="controls">Lista di controlli INR</param>
        /// <returns>Statistiche descrittive dei valori INR</returns>
        INRStatistics CalculateINRStatistics(IEnumerable<INRControl> controls);
        }

    /// <summary>
    /// Statistiche descrittive dei valori INR
    /// </summary>
    public class INRStatistics
        {
        /// <summary>
        /// Numero totale di controlli
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Valore INR medio
        /// </summary>
        public decimal Mean { get; set; }

        /// <summary>
        /// Deviazione standard
        /// </summary>
        public decimal StandardDeviation { get; set; }

        /// <summary>
        /// Valore INR minimo
        /// </summary>
        public decimal Min { get; set; }

        /// <summary>
        /// Valore INR massimo
        /// </summary>
        public decimal Max { get; set; }

        /// <summary>
        /// Mediana
        /// </summary>
        public decimal Median { get; set; }

        /// <summary>
        /// Percentuale di controlli in range
        /// </summary>
        public decimal PercentageInRange { get; set; }

        /// <summary>
        /// Percentuale di controlli sotto range
        /// </summary>
        public decimal PercentageBelowRange { get; set; }

        /// <summary>
        /// Percentuale di controlli sopra range
        /// </summary>
        public decimal PercentageAboveRange { get; set; }
        }
    }