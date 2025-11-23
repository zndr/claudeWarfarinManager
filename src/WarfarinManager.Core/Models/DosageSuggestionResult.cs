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
    public INRStatus INRStatus { get; set; }
    public string? LoadingDoseAction { get; set; }
    public decimal PercentageAdjustment { get; set; }
    public decimal SuggestedWeeklyDoseMg { get; set; }
    
    /// <summary>
    /// Schema settimanale dettagliato
    /// </summary>
    public WeeklyDoseSchedule WeeklySchedule { get; set; } = new();
    
    public int NextControlDays { get; set; }
    public bool RequiresEBPM { get; set; }
    public bool RequiresVitaminK { get; set; }
    public decimal? VitaminKDoseMg { get; set; }
    public string? VitaminKRoute { get; set; }
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
