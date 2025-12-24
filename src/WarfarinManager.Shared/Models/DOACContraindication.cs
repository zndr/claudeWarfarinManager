using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Shared.Models;

/// <summary>
/// Controindicazione specifica per un DOAC
/// </summary>
public class DOACContraindication
{
    /// <summary>
    /// Tipo di DOAC
    /// </summary>
    public DOACType DOACType { get; set; }

    /// <summary>
    /// Descrizione controindicazione
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Livello (assoluta o relativa/precauzione)
    /// </summary>
    public string Level { get; set; } = "Assoluta";

    /// <summary>
    /// Dettagli clinici
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Se true, è controindicazione assoluta
    /// </summary>
    public bool IsAbsolute => Level.Equals("Assoluta", StringComparison.OrdinalIgnoreCase);
}
