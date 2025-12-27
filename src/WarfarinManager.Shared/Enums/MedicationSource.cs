namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Origine del farmaco nel sistema
/// </summary>
public enum MedicationSource
{
    /// <summary>
    /// Farmaco inserito manualmente dall'utente
    /// </summary>
    Manual,

    /// <summary>
    /// Farmaco importato da Millewin/Milleps
    /// </summary>
    Milleps
}
