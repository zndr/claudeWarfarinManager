using WarfarinManager.Core.Models;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Core.Interfaces;

/// <summary>
/// Servizio per la verifica delle interazioni farmacologiche con Warfarin
/// </summary>
public interface IInteractionCheckerService
{
    /// <summary>
    /// Verifica l'interazione di un farmaco con Warfarin
    /// </summary>
    /// <param name="medicationName">Nome del farmaco da verificare</param>
    /// <returns>Risultato della verifica con livello di rischio e raccomandazioni</returns>
    Task<InteractionCheckResult> CheckInteractionAsync(string medicationName);

    /// <summary>
    /// Verifica l'interazione di un farmaco con Warfarin cercando prima per nome,
    /// poi per principio attivo se il nome non viene trovato
    /// </summary>
    /// <param name="medicationName">Nome commerciale del farmaco</param>
    /// <param name="activeIngredient">Principio attivo (opzionale)</param>
    /// <returns>Risultato della verifica con livello di rischio e raccomandazioni</returns>
    Task<InteractionCheckResult> CheckInteractionAsync(string medicationName, string? activeIngredient);

    /// <summary>
    /// Ottiene la raccomandazione per l'aggiustamento della dose
    /// </summary>
    /// <param name="medicationName">Nome del farmaco</param>
    /// <param name="currentWeeklyDoseMg">Dose settimanale attuale in mg</param>
    /// <param name="guideline">Linea guida da utilizzare (FCSA o ACCP)</param>
    /// <returns>Raccomandazione per l'aggiustamento della dose</returns>
    Task<DoseAdjustmentRecommendation> GetDoseAdjustmentRecommendationAsync(
        string medicationName,
        decimal currentWeeklyDoseMg,
        GuidelineType guideline);

    /// <summary>
    /// Ottiene i giorni raccomandati per il controllo INR dopo l'inizio di un farmaco
    /// </summary>
    /// <param name="medicationName">Nome del farmaco</param>
    /// <returns>Numero di giorni raccomandati per il controllo, null se non disponibile</returns>
    Task<int?> GetRecommendedINRCheckDaysAsync(string medicationName);

    /// <summary>
    /// Cerca farmaci nel database delle interazioni (per autocomplete)
    /// </summary>
    /// <param name="partialName">Nome parziale del farmaco</param>
    /// <param name="maxResults">Numero massimo di risultati</param>
    /// <returns>Lista di nomi farmaci corrispondenti</returns>
    Task<List<string>> SearchMedicationsAsync(string partialName, int maxResults = 10);
}
