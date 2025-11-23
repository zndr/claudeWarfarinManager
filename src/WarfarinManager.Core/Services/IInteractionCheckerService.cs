using WarfarinManager.Core.Models;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per la verifica delle interazioni farmacologiche con warfarin
/// </summary>
public interface IInteractionCheckerService
{
    /// <summary>
    /// Verifica se un farmaco ha interazioni con warfarin
    /// </summary>
    /// <param name="medicationName">Nome del farmaco da verificare</param>
    /// <returns>Risultato della verifica interazione</returns>
    Task<InteractionCheckResult> CheckInteractionAsync(string medicationName);

    /// <summary>
    /// Ottiene raccomandazioni per aggiustamento dose in presenza di interazione
    /// </summary>
    /// <param name="medicationName">Nome del farmaco</param>
    /// <param name="currentWeeklyDoseMg">Dose settimanale attuale di warfarin (mg)</param>
    /// <param name="guideline">Linea guida di riferimento (FCSA o ACCP)</param>
    /// <returns>Raccomandazione per aggiustamento dose</returns>
    Task<DoseAdjustmentRecommendation> GetDoseAdjustmentRecommendationAsync(
        string medicationName, 
        decimal currentWeeklyDoseMg, 
        GuidelineType guideline);

    /// <summary>
    /// Ottiene il numero di giorni raccomandato per il prossimo controllo INR
    /// in presenza di interazione farmacologica
    /// </summary>
    /// <param name="medicationName">Nome del farmaco</param>
    /// <returns>Numero di giorni consigliati per il controllo INR</returns>
    Task<int?> GetRecommendedINRCheckDaysAsync(string medicationName);

    /// <summary>
    /// Cerca farmaci nel database per nome parziale (per autocomplete)
    /// </summary>
    /// <param name="partialName">Parte del nome del farmaco</param>
    /// <param name="maxResults">Numero massimo di risultati (default 10)</param>
    /// <returns>Lista di nomi farmaci che matchiano</returns>
    Task<List<string>> SearchMedicationsAsync(string partialName, int maxResults = 10);
}

/// <summary>
/// Raccomandazione per aggiustamento dose warfarin
/// </summary>
public class DoseAdjustmentRecommendation
{
    /// <summary>
    /// Percentuale di riduzione suggerita (es. -25 per riduzione del 25%)
    /// Valore positivo = aumento, negativo = riduzione
    /// </summary>
    public decimal? PercentageAdjustment { get; set; }

    /// <summary>
    /// Nuova dose settimanale calcolata (mg)
    /// </summary>
    public decimal? SuggestedWeeklyDoseMg { get; set; }

    /// <summary>
    /// Descrizione testuale della raccomandazione
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Timing controllo INR (giorni)
    /// </summary>
    public int? RecommendedINRCheckDays { get; set; }

    /// <summary>
    /// Note aggiuntive specifiche per la linea guida
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Linea guida applicata
    /// </summary>
    public GuidelineType GuidelineUsed { get; set; }
}
