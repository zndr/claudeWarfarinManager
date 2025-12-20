namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Livello di rischio emorragico della procedura chirurgica
/// </summary>
public enum BleedingRisk
{
    /// <summary>
    /// Basso rischio emorragico (0-2% major bleeding)
    /// Procedure diagnostiche, interventi minori
    /// </summary>
    Low,
    
    /// <summary>
    /// Moderato rischio emorragico (2-5% major bleeding)
    /// Endoscopia con biopsia, impianto PM/ICD, chirurgia minore
    /// </summary>
    Moderate,
    
    /// <summary>
    /// Alto rischio emorragico (>5% major bleeding)
    /// Neurochirurgia, chirurgia cardiaca, vascolare maggiore
    /// </summary>
    High
}
