namespace WarfarinManager.Shared.Constants;

/// <summary>
/// Costanti cliniche per la gestione della terapia anticoagulante
/// </summary>
public static class ClinicalConstants
{
    /// <summary>
    /// Soglia dose settimanale per identificare metabolizzatori lenti (mg)
    /// </summary>
    public const decimal SlowMetabolizerThreshold = 15.0m;
    
    /// <summary>
    /// Soglia età per paziente anziano (anni)
    /// </summary>
    public const int ElderlyAgeThreshold = 80;
    
    /// <summary>
    /// Soglia età minima per pazienti adulti (anni)
    /// </summary>
    public const int MinimumAdultAge = 18;
    
    /// <summary>
    /// Soglia dose settimanale alta per alert (mg)
    /// </summary>
    public const decimal HighDoseThreshold = 40.0m;
    
    /// <summary>
    /// Valore minimo INR valido
    /// </summary>
    public const decimal MinINRValue = 0.5m;
    
    /// <summary>
    /// Valore massimo INR valido
    /// </summary>
    public const decimal MaxINRValue = 10.0m;
    
    /// <summary>
    /// Dose compressa standard warfarin (mg)
    /// </summary>
    public const decimal StandardTabletDose = 5.0m;
    
    /// <summary>
    /// Dose mezza compressa warfarin (mg)
    /// </summary>
    public const decimal HalfTabletDose = 2.5m;
}

/// <summary>
/// Costanti per calcolo TTR
/// </summary>
public static class TTRConstants
{
    /// <summary>
    /// Soglia TTR eccellente (%)
    /// </summary>
    public const decimal ExcellentThreshold = 70.0m;
    
    /// <summary>
    /// Soglia TTR accettabile (%)
    /// </summary>
    public const decimal AcceptableThreshold = 60.0m;
    
    /// <summary>
    /// Soglia TTR subottimale (%)
    /// </summary>
    public const decimal SuboptimalThreshold = 50.0m;
    
    /// <summary>
    /// Finestra rolling per trend TTR (mesi)
    /// </summary>
    public const int RollingWindowMonths = 3;
}

/// <summary>
/// Costanti per gestione dosaggio secondo linee guida
/// </summary>
public static class DosageConstants
{
    /// <summary>
    /// Soglia INR per Vitamina K secondo FCSA (asintomatico)
    /// </summary>
    public const decimal VitaminKThreshold_FCSA = 6.0m;
    
    /// <summary>
    /// Soglia INR per Vitamina K secondo ACCP (asintomatico)
    /// </summary>
    public const decimal VitaminKThreshold_ACCP = 10.0m;
    
    /// <summary>
    /// Soglia INR critico basso per valutare EBPM
    /// </summary>
    public const decimal CriticalLowINR = 1.5m;
    
    /// <summary>
    /// Dose minima settimanale warfarin (mg)
    /// </summary>
    public const decimal MinWeeklyDose = 15.0m;
    
    /// <summary>
    /// Dose massima settimanale warfarin (mg)
    /// </summary>
    public const decimal MaxWeeklyDose = 70.0m;
}

/// <summary>
/// Costanti per intervalli di controllo INR
/// </summary>
public static class ControlIntervals
{
    /// <summary>
    /// Intervallo controllo minimo (giorni) - situazioni critiche
    /// </summary>
    public const int MinControlDays = 1;
    
    /// <summary>
    /// Intervallo controllo breve (giorni) - post-aggiustamento
    /// </summary>
    public const int ShortControlDays = 3;
    
    /// <summary>
    /// Intervallo controllo medio (giorni) - induzione
    /// </summary>
    public const int MediumControlDays = 7;
    
    /// <summary>
    /// Intervallo controllo lungo (giorni) - mantenimento
    /// </summary>
    public const int LongControlDays = 14;
    
    /// <summary>
    /// Intervallo controllo massimo (giorni) - stabile
    /// </summary>
    public const int MaxControlDays = 42; // 6 settimane
}
