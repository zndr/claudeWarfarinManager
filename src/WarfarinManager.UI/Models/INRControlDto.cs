using System;
using System.Collections.Generic;

namespace WarfarinManager.UI.Models;

/// <summary>
/// Data Transfer Object per controllo INR
/// </summary>
public class INRControlDto
{
    public int Id { get; set; }
    
    public int PatientId { get; set; }
    
    /// <summary>
    /// Data del controllo INR
    /// </summary>
    public DateTime ControlDate { get; set; }
    
    /// <summary>
    /// Valore INR rilevato
    /// </summary>
    public decimal INRValue { get; set; }
    
    /// <summary>
    /// Dose settimanale corrente (mg)
    /// </summary>
    public decimal CurrentWeeklyDose { get; set; }
    
    /// <summary>
    /// Fase della terapia
    /// </summary>
    public string PhaseOfTherapy { get; set; } = string.Empty;
    
    /// <summary>
    /// Paziente assume regolarmente
    /// </summary>
    public bool IsCompliant { get; set; }
    
    /// <summary>
    /// Note libere
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Dosaggi giornalieri (Lunedì = index 0, Domenica = index 6)
    /// </summary>
    public List<decimal> DailyDoses { get; set; } = new List<decimal>(7);
    
    /// <summary>
    /// Target INR minimo del paziente
    /// </summary>
    public decimal TargetINRMin { get; set; }
    
    /// <summary>
    /// Target INR massimo del paziente
    /// </summary>
    public decimal TargetINRMax { get; set; }
    
    /// <summary>
    /// Indica se l'INR è nel range terapeutico
    /// </summary>
    public bool IsInRange => INRValue >= TargetINRMin && INRValue <= TargetINRMax;
    
    /// <summary>
    /// Stato INR formattato
    /// </summary>
    public string Status
    {
        get
        {
            if (IsInRange) return "In Range";
            if (INRValue < TargetINRMin) return "Sotto-anticoagulazione";
            return "Sovra-anticoagulazione";
        }
    }
}
