using System;

namespace WarfarinManager.UI.Models;

/// <summary>
/// Data Transfer Object per visualizzazione indicazione terapeutica nella UI
/// </summary>
public class IndicationDto
{
    public int Id { get; set; }
    
    public int PatientId { get; set; }
    
    /// <summary>
    /// Codice del tipo di indicazione
    /// </summary>
    public string IndicationTypeCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrizione completa dell'indicazione
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Categoria (es: "Fibrillazione Atriale", "Tromboembolismo Venoso")
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
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
    /// Data inizio terapia
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Data fine terapia (null se ancora attiva)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Indica se è l'indicazione attualmente attiva
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Motivo del cambio indicazione
    /// </summary>
    public string? ChangeReason { get; set; }
    
    /// <summary>
    /// Note aggiuntive
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Durata tipica dell'indicazione
    /// </summary>
    public string? TypicalDuration { get; set; }
    
    /// <summary>
    /// Stato formattato
    /// </summary>
    public string Status => IsActive ? "ATTIVA" : "Storico";
    
    /// <summary>
    /// Periodo formattato
    /// </summary>
    public string Period
    {
        get
        {
            if (EndDate.HasValue)
                return $"Dal {StartDate:dd/MM/yyyy} al {EndDate.Value:dd/MM/yyyy}";
            else
                return $"Dal {StartDate:dd/MM/yyyy} - In corso";
        }
    }
}
