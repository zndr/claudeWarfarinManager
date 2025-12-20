namespace WarfarinManager.UI.Models;

/// <summary>
/// Data Transfer Object per tipo di indicazione disponibile
/// </summary>
public class IndicationTypeDto
{
    public int Id { get; set; }
    
    /// <summary>
    /// Codice univoco (es: "FA", "TVP", "PROTESI_MECCANICA")
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Categoria (es: "Fibrillazione Atriale", "Tromboembolismo Venoso")
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrizione completa dell'indicazione
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Target INR minimo
    /// </summary>
    public decimal TargetINRMin { get; set; }
    
    /// <summary>
    /// Target INR massimo
    /// </summary>
    public decimal TargetINRMax { get; set; }
    
    /// <summary>
    /// Range INR formattato (es: "2.0 - 3.0")
    /// </summary>
    public string TargetINRRange => $"{TargetINRMin:F1} - {TargetINRMax:F1}";
    
    /// <summary>
    /// Durata tipica dell'indicazione
    /// </summary>
    public string? TypicalDuration { get; set; }
    
    /// <summary>
    /// Display text per ComboBox (Categoria - Descrizione)
    /// </summary>
    public string DisplayText => $"{Category} - {Description}";
}
