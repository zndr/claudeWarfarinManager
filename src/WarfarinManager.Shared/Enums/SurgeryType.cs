namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Tipo di chirurgia programmata per bridge therapy
/// </summary>
public enum SurgeryType
{
    /// <summary>
    /// Chirurgia maggiore (rischio emorragico alto)
    /// </summary>
    Major,
    
    /// <summary>
    /// Chirurgia minore (rischio emorragico basso)
    /// </summary>
    Minor,
    
    /// <summary>
    /// Procedure odontoiatriche (<3 estrazioni)
    /// </summary>
    DentalMinor,
    
    /// <summary>
    /// Procedure odontoiatriche (≥3 estrazioni)
    /// </summary>
    DentalMajor,
    
    /// <summary>
    /// Procedure diagnostiche invasive
    /// </summary>
    DiagnosticInvasive
}
