using System;
using WarfarinManager.Shared.Enums;

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
    /// Target INR minimo del paziente
    /// </summary>
    public decimal TargetINRMin { get; set; }
    
    /// <summary>
    /// Target INR massimo del paziente
    /// </summary>
    public decimal TargetINRMax { get; set; }
    
    // Dosi giornaliere individuali (mg)
    public decimal MondayDose { get; set; }
    public decimal TuesdayDose { get; set; }
    public decimal WednesdayDose { get; set; }
    public decimal ThursdayDose { get; set; }
    public decimal FridayDose { get; set; }
    public decimal SaturdayDose { get; set; }
    public decimal SundayDose { get; set; }
    
    /// <summary>
    /// Dose settimanale totale salvata direttamente (mg) - usata come fallback
    /// </summary>
    public decimal SavedWeeklyDose { get; set; }
    
    /// <summary>
    /// Dose settimanale totale (mg) - calcolata dalle dosi giornaliere o fallback al valore salvato
    /// </summary>
    public decimal CurrentWeeklyDose
    {
        get
        {
            var calculated = MondayDose + TuesdayDose + WednesdayDose + ThursdayDose + 
                            FridayDose + SaturdayDose + SundayDose;
            // Se le dosi giornaliere sono tutte 0 ma c'è un valore salvato, usa quello
            return calculated > 0 ? calculated : SavedWeeklyDose;
        }
    }
    
    /// <summary>
    /// Fase della terapia
    /// </summary>
    public TherapyPhase Phase { get; set; }
    
    /// <summary>
    /// Paziente assume regolarmente
    /// </summary>
    public bool IsCompliant { get; set; }
    
    /// <summary>
    /// Note libere
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Nuova dose settimanale suggerita (mg)
    /// </summary>
    public decimal? SuggestedWeeklyDose { get; set; }
    
    /// <summary>
    /// Schema posologico suggerito
    /// </summary>
    public string? SuggestedSchedule { get; set; }
    
    /// <summary>
    /// Giorni al prossimo controllo (FCSA)
    /// </summary>
    public int? NextControlDays_FCSA { get; set; }
    
    /// <summary>
    /// Giorni al prossimo controllo (ACCP)
    /// </summary>
    public int? NextControlDays_ACCP { get; set; }
    
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
            if (IsInRange) return "✓ In Range";
            if (INRValue < TargetINRMin) return "⚠ Sotto";
            return "⚠ Sopra";
        }
    }
    
    /// <summary>
    /// Colore status INR
    /// </summary>
    public string StatusColor
    {
        get
        {
            if (IsInRange) return "#107C10"; // Verde
            if (INRValue < TargetINRMin) return "#FFB900"; // Giallo
            return "#E81123"; // Rosso
        }
    }
    
    /// <summary>
    /// Formattazione data per UI
    /// </summary>
    public string FormattedDate => ControlDate.ToString("dd/MM/yyyy");
    
    /// <summary>
    /// Fase terapia in italiano
    /// </summary>
    public string PhaseDescription => Phase switch
    {
        TherapyPhase.Induction => "Induzione",
        TherapyPhase.Maintenance => "Mantenimento",
        TherapyPhase.PostAdjustment => "Post-aggiustamento",
        _ => "Sconosciuta"
    };
}
