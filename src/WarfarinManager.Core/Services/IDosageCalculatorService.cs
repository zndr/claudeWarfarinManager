using WarfarinManager.Core.Models;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per il calcolo del dosaggio warfarin secondo linee guida FCSA-SIMG e ACCP
/// </summary>
public interface IDosageCalculatorService
{
    /// <summary>
    /// Calcola suggerimento dosaggio secondo linee guida FCSA-SIMG (Italia)
    /// </summary>
    DosageSuggestionResult CalculateFCSA(
        decimal currentINR,
        decimal targetINRMin,
        decimal targetINRMax,
        decimal currentWeeklyDoseMg,
        TherapyPhase phase,
        bool isCompliant,
        bool isSlowMetabolizer,
        ThromboembolicRisk thromboembolicRisk = ThromboembolicRisk.Moderate);

    /// <summary>
    /// Calcola suggerimento dosaggio secondo linee guida ACCP/ACC (USA)
    /// </summary>
    DosageSuggestionResult CalculateACCP(
        decimal currentINR,
        decimal targetINRMin,
        decimal targetINRMax,
        decimal currentWeeklyDoseMg,
        TherapyPhase phase,
        bool isCompliant,
        bool isSlowMetabolizer,
        ThromboembolicRisk thromboembolicRisk = ThromboembolicRisk.Moderate);

    /// <summary>
    /// Genera schema posologico settimanale ottimizzato da dose totale
    /// Preferisce compresse intere (5mg) e mezze (2.5mg), evita 1/4 o 3/4
    /// </summary>
    WeeklyDoseSchedule GenerateWeeklySchedule(decimal weeklyDoseMg);

    /// <summary>
    /// Valuta se è necessario bridge con EBPM
    /// </summary>
    bool RequiresEBPM(decimal inr, ThromboembolicRisk risk, GuidelineType guideline);

    /// <summary>
    /// Valuta se è necessaria somministrazione Vitamina K
    /// </summary>
    VitaminKRecommendation EvaluateVitaminK(decimal inr, GuidelineType guideline, bool hasBleeding = false);
}

/// <summary>
/// Raccomandazione per uso Vitamina K
/// </summary>
public class VitaminKRecommendation
{
    public bool IsRecommended { get; set; }
    public decimal? DoseMg { get; set; }
    public string Route { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Urgency { get; set; } = string.Empty;
}
