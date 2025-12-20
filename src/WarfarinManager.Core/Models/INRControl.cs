using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Core.Models
{
    /// <summary>
    /// Rappresenta un singolo controllo INR di un paziente
    /// </summary>
    public class INRControl
    {
        /// <summary>
        /// Identificativo univoco
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID paziente associato
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// Data del controllo/prelievo
        /// </summary>
        public DateTime ControlDate { get; set; }

        /// <summary>
        /// Valore INR misurato
        /// </summary>
        public decimal INRValue { get; set; }

        /// <summary>
        /// Dose settimanale corrente (mg)
        /// </summary>
        public decimal CurrentWeeklyDose { get; set; }

        /// <summary>
        /// Fase della terapia
        /// </summary>
        public TherapyPhase PhaseOfTherapy { get; set; }

        /// <summary>
        /// Il paziente assume regolarmente la terapia
        /// </summary>
        public bool IsCompliant { get; set; }

        /// <summary>
        /// Note cliniche del controllo
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Data creazione record
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // Proprietà calcolate

        /// <summary>
        /// Verifica se INR è nel range terapeutico
        /// </summary>
        public bool IsInRange(decimal targetMin, decimal targetMax)
        {
            return INRValue >= targetMin && INRValue <= targetMax;
        }

        /// <summary>
        /// Verifica se INR è sotto range
        /// </summary>
        public bool IsBelowRange(decimal targetMin)
        {
            return INRValue < targetMin;
        }

        /// <summary>
        /// Verifica se INR è sopra range
        /// </summary>
        public bool IsAboveRange(decimal targetMax)
        {
            return INRValue > targetMax;
        }

        /// <summary>
        /// Calcola scostamento percentuale dal target medio
        /// </summary>
        public decimal CalculateDeviationFromTarget(decimal targetMin, decimal targetMax)
        {
            decimal targetMid = (targetMin + targetMax) / 2m;
            return ((INRValue - targetMid) / targetMid) * 100m;
        }
    }
}
