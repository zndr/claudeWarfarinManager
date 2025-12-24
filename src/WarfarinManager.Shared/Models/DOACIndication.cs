using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Shared.Models;

/// <summary>
/// Indicazione terapeutica autorizzata per un DOAC
/// </summary>
public class DOACIndication
{
    /// <summary>
    /// Tipo di DOAC
    /// </summary>
    public DOACType DOACType { get; set; }

    /// <summary>
    /// Codice indicazione
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Descrizione indicazione
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Popolazione (adulta/pediatrica)
    /// </summary>
    public string Population { get; set; } = "Adulta";

    /// <summary>
    /// Note specifiche sul dosaggio o particolarità
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Se true, l'indicazione è autorizzata EMA
    /// </summary>
    public bool IsEMAApproved { get; set; } = true;
}
