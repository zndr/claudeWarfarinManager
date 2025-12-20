using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Core.Models;

/// <summary>
/// Risultato del calcolo suggerimento dosaggio
/// </summary>
public class DosageSuggestionResult
{
    public GuidelineType GuidelineUsed { get; set; }
    public decimal CurrentINR { get; set; }
    public decimal TargetINRMin { get; set; }
    public decimal TargetINRMax { get; set; }
    public decimal CurrentWeeklyDoseMg { get; set; }
    public bool IsInRange { get; set; }

    /// <summary>
    /// Stato INR semplificato (deprecato, usare FasciaINR)
    /// </summary>
    [Obsolete("Usare FasciaINR per classificazione più dettagliata")]
    public INRStatus INRStatus { get; set; }

    /// <summary>
    /// Classificazione dettagliata della fascia INR
    /// </summary>
    public FasciaINR FasciaINR { get; set; }

    public string? LoadingDoseAction { get; set; }
    public decimal PercentageAdjustment { get; set; }
    public decimal SuggestedWeeklyDoseMg { get; set; }

    /// <summary>
    /// Dose supplementare da somministrare il primo giorno (per INR basso)
    /// </summary>
    public decimal? DoseSupplementarePrimoGiorno { get; set; }

    /// <summary>
    /// Numero dosi da sospendere (per INR alto)
    /// Null = sospendere fino a INR rientrato
    /// </summary>
    public int? SospensioneDosi { get; set; }

    /// <summary>
    /// Schema settimanale dettagliato
    /// </summary>
    public WeeklyDoseSchedule WeeklySchedule { get; set; } = new();

    public int NextControlDays { get; set; }
    public bool RequiresEBPM { get; set; }
    public bool RequiresVitaminK { get; set; }
    public decimal? VitaminKDoseMg { get; set; }
    public string? VitaminKRoute { get; set; }

    /// <summary>
    /// Richiede PCC (Concentrato Complesso Protrombinico)
    /// </summary>
    public bool RequiresPCC { get; set; }

    /// <summary>
    /// Dosaggio PCC raccomandato
    /// </summary>
    public string? DosePCC { get; set; }

    /// <summary>
    /// Richiede plasma fresco congelato (alternativa a PCC)
    /// </summary>
    public bool RequiresPlasma { get; set; }

    /// <summary>
    /// Dosaggio plasma raccomandato
    /// </summary>
    public string? DosePlasma { get; set; }

    /// <summary>
    /// Richiede ospedalizzazione
    /// </summary>
    public bool RequiresHospitalization { get; set; }

    /// <summary>
    /// Livello di urgenza della situazione
    /// </summary>
    public UrgencyLevel UrgencyLevel { get; set; }

    /// <summary>
    /// Tipo di emorragia presente (se applicabile)
    /// </summary>
    public TipoEmorragia TipoEmorragia { get; set; }

    /// <summary>
    /// Fonte della raccomandazione (FCSA, ACCP, Sintesi)
    /// </summary>
    public string FonteRaccomandazione { get; set; } = "FCSA";

    public string ClinicalNotes { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Raccomandazione Vitamina K
/// </summary>
public class VitaminKRecommendation
{
    public bool IsRecommended { get; set; }
    public decimal DoseMg { get; set; }
    public string Route { get; set; } = string.Empty;
    public string Urgency { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
