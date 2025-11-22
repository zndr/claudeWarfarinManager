namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Livello di rischio tromboembolico del paziente
/// </summary>
public enum ThromboembolicRisk
{
    /// <summary>
    /// Basso rischio (FA con CHAD2DS2-VASc ≤2, TEV >12 mesi)
    /// </summary>
    Low,
    
    /// <summary>
    /// Rischio moderato (valvola aortica senza FdR, FA con CHAD2DS2-VASc 3-4, TEV <3 mesi)
    /// </summary>
    Moderate,
    
    /// <summary>
    /// Alto rischio (valvola mitralica meccanica, valvola aortica con FdR, ictus/TIA <3 mesi)
    /// </summary>
    High
}
