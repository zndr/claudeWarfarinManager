namespace WarfarinManager.Core.Models
    {
    /// <summary>
    /// Risultato del calcolo TTR (Time in Therapeutic Range)
    /// </summary>
    public class TTRResult
        {
        /// <summary>
        /// Percentuale TTR globale (0-100)
        /// </summary>
        public decimal TTRPercentage { get; set; }

        /// <summary>
        /// Totale giorni analizzati
        /// </summary>
        public int TotalDays { get; set; }

        /// <summary>
        /// Giorni nel range terapeutico
        /// </summary>
        public int DaysInRange { get; set; }

        /// <summary>
        /// Giorni sotto range
        /// </summary>
        public int DaysBelowRange { get; set; }

        /// <summary>
        /// Giorni sopra range
        /// </summary>
        public int DaysAboveRange { get; set; }

        // Alias per compatibilità con test
        public int DaysBelow => DaysBelowRange;
        public int DaysAbove => DaysAboveRange;

        /// <summary>
        /// Data inizio periodo analizzato
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Data fine periodo analizzato
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Qualità del controllo basata su TTR
        /// </summary>
        public TTRQuality Quality { get; set; }

        /// <summary>
        /// Descrizione testuale della qualità
        /// </summary>
        public string QualityDescription => Quality switch
            {
                TTRQuality.Excellent => "Eccellente (≥70%)",
                TTRQuality.Good => "Buono (65-69%)",
                TTRQuality.Acceptable => "Accettabile (60-64%)",
                TTRQuality.Suboptimal => "Subottimale (50-59%)",
                TTRQuality.Poor => "Scarso (<50%)",
                _ => "Non determinato"
                };

        // Alias per compatibilità con test
        public string Message => QualityDescription;

        /// <summary>
        /// Indica se il TTR è insufficiente (alert)
        /// </summary>
        public bool IsInsufficient => TTRPercentage < 60m;

        /// <summary>
        /// Indica se il TTR è critico (richiede intervento)
        /// </summary>
        public bool IsCritical => TTRPercentage < 50m;

        /// <summary>
        /// Numero di controlli INR inclusi nel calcolo
        /// </summary>
        public int NumberOfControls { get; set; }

        /// <summary>
        /// Valore INR medio nel periodo
        /// </summary>
        public decimal AverageINR { get; set; }

        /// <summary>
        /// Deviazione standard INR
        /// </summary>
        public decimal INRStandardDeviation { get; set; }

        /// <summary>
        /// Target INR minimo utilizzato
        /// </summary>
        public decimal TargetINRMin { get; set; }

        /// <summary>
        /// Target INR massimo utilizzato
        /// </summary>
        public decimal TargetINRMax { get; set; }
        }

    /// <summary>
    /// Classificazione qualità TTR secondo linee guida
    /// </summary>
    public enum TTRQuality
        {
        /// <summary>
        /// TTR ≥70% - Controllo eccellente
        /// </summary>
        Excellent,

        /// <summary>
        /// TTR 65-69% - Controllo buono
        /// </summary>
        Good,

        /// <summary>
        /// TTR 60-64% - Controllo accettabile
        /// </summary>
        Acceptable,

        /// <summary>
        /// TTR 50-59% - Controllo subottimale (richiede attenzione)
        /// </summary>
        Suboptimal,

        /// <summary>
        /// TTR <50% - Controllo scarso (richiede intervento)
        /// </summary>
        Poor
        }
    }