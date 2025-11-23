using WarfarinManager.Data.Entities;

namespace WarfarinManager.Core.Interfaces
{
    /// <summary>
    /// Repository per la gestione dei dati dei farmaci con interazioni
    /// </summary>
    public interface IInteractionDrugRepository
    {
        /// <summary>
        /// Recupera un farmaco dal database delle interazioni per nome
        /// </summary>
        /// <param name="drugName">Nome del farmaco (case-insensitive)</param>
        /// <returns>Dati del farmaco o null se non trovato</returns>
        Task<InteractionDrug?> FindByNameAsync(string drugName);

        /// <summary>
        /// Cerca farmaci per nome parziale (autocomplete)
        /// </summary>
        /// <param name="partialName">Nome parziale del farmaco</param>
        /// <param name="maxResults">Numero massimo di risultati (default: 10)</param>
        /// <returns>Lista di farmaci corrispondenti</returns>
        Task<IEnumerable<InteractionDrug>> SearchByNameAsync(string partialName, int maxResults = 10);

        /// <summary>
        /// Recupera tutti i farmaci di una categoria specifica
        /// </summary>
        /// <param name="category">Categoria farmacologica (es. "Antibiotic", "Antiarrhythmic")</param>
        /// <returns>Lista di farmaci della categoria</returns>
        Task<IEnumerable<InteractionDrug>> GetByCategoryAsync(string category);

        /// <summary>
        /// Recupera tutti i farmaci con effetto specifico su INR
        /// </summary>
        /// <param name="effect">Effetto su INR ("Increases", "Decreases", "Variable")</param>
        /// <returns>Lista di farmaci con l'effetto specificato</returns>
        Task<IEnumerable<InteractionDrug>> GetByEffectAsync(string effect);

        /// <summary>
        /// Recupera tutti i farmaci ad alto rischio (OR ≥ 2.0)
        /// </summary>
        /// <returns>Lista di farmaci ad alto rischio emorragico</returns>
        Task<IEnumerable<InteractionDrug>> GetHighRiskDrugsAsync();

        /// <summary>
        /// Recupera tutti i farmaci nel database
        /// </summary>
        /// <returns>Lista completa di tutti i farmaci</returns>
        Task<IEnumerable<InteractionDrug>> GetAllAsync();

        /// <summary>
        /// Verifica se un farmaco esiste nel database
        /// </summary>
        Task<bool> ExistsAsync(string drugName);
    }
}
