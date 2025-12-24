using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Shared.Models;

/// <summary>
/// Rappresenta un'interazione farmacologica con un DOAC
/// </summary>
public class DOACInteraction
{
    /// <summary>
    /// Tipo di DOAC
    /// </summary>
    public DOACType DOACType { get; set; }

    /// <summary>
    /// Nome del farmaco interagente
    /// </summary>
    public string DrugName { get; set; } = string.Empty;

    /// <summary>
    /// Classe farmacologica
    /// </summary>
    public string DrugClass { get; set; } = string.Empty;

    /// <summary>
    /// Livello di pericolosità
    /// </summary>
    public DOACInteractionLevel InteractionLevel { get; set; }

    /// <summary>
    /// Effetto sull'esposizione al DOAC (es. "↑ AUC 2.4x")
    /// </summary>
    public string Effect { get; set; } = string.Empty;

    /// <summary>
    /// Raccomandazione clinica
    /// </summary>
    public string ClinicalRecommendation { get; set; } = string.Empty;

    /// <summary>
    /// Note aggiuntive
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Se true, l'interazione è pericolosa secondo i criteri clinici
    /// </summary>
    public bool IsDangerous => InteractionLevel >= DOACInteractionLevel.Dangerous;
}
