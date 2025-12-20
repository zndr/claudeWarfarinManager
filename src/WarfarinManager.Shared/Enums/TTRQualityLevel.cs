namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Livello qualità TTR (Time in Therapeutic Range)
/// </summary>
public enum TTRQualityLevel
{
    /// <summary>
    /// Dati insufficienti per valutazione
    /// </summary>
    Insufficient,
    
    /// <summary>
    /// TTR critico (<40%)
    /// </summary>
    Poor,
    
    /// <summary>
    /// TTR subottimale (40-49%)
    /// </summary>
    Suboptimal,
    
    /// <summary>
    /// TTR accettabile (50-59%)
    /// </summary>
    Acceptable,
    
    /// <summary>
    /// TTR buono (60-69%)
    /// </summary>
    Good,
    
    /// <summary>
    /// TTR eccellente (≥70%)
    /// </summary>
    Excellent
}
