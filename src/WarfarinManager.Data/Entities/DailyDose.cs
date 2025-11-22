namespace WarfarinManager.Data.Entities;

/// <summary>
/// Entità dose giornaliera (parte dello schema settimanale)
/// </summary>
public class DailyDose : BaseEntity
{
    /// <summary>
    /// ID controllo INR (Foreign Key)
    /// </summary>
    public int INRControlId { get; set; }
    
    /// <summary>
    /// Giorno della settimana (1=Lunedì, 7=Domenica)
    /// </summary>
    public int DayOfWeek { get; set; }
    
    /// <summary>
    /// Dose in mg per questo giorno
    /// </summary>
    public decimal DoseMg { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Controllo INR associato
    /// </summary>
    public INRControl INRControl { get; set; } = null!;
}
