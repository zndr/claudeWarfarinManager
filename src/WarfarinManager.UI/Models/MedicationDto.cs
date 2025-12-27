using System;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.UI.Models;

/// <summary>
/// DTO per la visualizzazione dei farmaci concomitanti
/// </summary>
public class MedicationDto
{
    public int Id { get; set; }
    
    public int PatientId { get; set; }

    /// <summary>
    /// Nome farmaco (nome commerciale)
    /// </summary>
    public string MedicationName { get; set; } = string.Empty;

    /// <summary>
    /// Codice ATC del farmaco
    /// </summary>
    public string? AtcCode { get; set; }

    /// <summary>
    /// Principio attivo
    /// </summary>
    public string? ActiveIngredient { get; set; }

    /// <summary>
    /// Origine del farmaco (Manual o Milleps)
    /// </summary>
    public MedicationSource Source { get; set; }

    /// <summary>
    /// Dosaggio (es. "500 mg")
    /// </summary>
    public string? Dosage { get; set; }
    
    /// <summary>
    /// Frequenza assunzione (es. "2 volte/die")
    /// </summary>
    public string? Frequency { get; set; }
    
    /// <summary>
    /// Data inizio terapia
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Data fine terapia
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Terapia attiva
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Livello interazione con warfarin
    /// </summary>
    public InteractionLevel InteractionLevel { get; set; }
    
    /// <summary>
    /// Dettagli interazione
    /// </summary>
    public string? InteractionDetails { get; set; }
    
    // Proprietà calcolate per UI
    
    /// <summary>
    /// Descrizione dosaggio completa
    /// </summary>
    public string DosageDescription => string.IsNullOrEmpty(Dosage) && string.IsNullOrEmpty(Frequency)
        ? "-"
        : $"{Dosage ?? ""} {Frequency ?? ""}".Trim();
    
    /// <summary>
    /// Stato testuale
    /// </summary>
    public string Status => IsActive ? "Attivo" : "Sospeso";
    
    /// <summary>
    /// Periodo terapia
    /// </summary>
    public string Period => EndDate.HasValue
        ? $"{StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}"
        : $"Dal {StartDate:dd/MM/yyyy}";
    
    /// <summary>
    /// Colore indicatore interazione
    /// </summary>
    public string InteractionColor => InteractionLevel switch
    {
        InteractionLevel.High => "#E81123",     // Rosso
        InteractionLevel.Moderate => "#FFB900", // Giallo
        InteractionLevel.Low => "#107C10",      // Verde
        _ => "#A0A0A0"                          // Grigio
    };
    
    /// <summary>
    /// Icona livello interazione
    /// </summary>
    public string InteractionIcon => InteractionLevel switch
    {
        InteractionLevel.High => "⚠️",
        InteractionLevel.Moderate => "⚡",
        InteractionLevel.Low => "ℹ️",
        _ => "✓"
    };
    
    /// <summary>
    /// Descrizione livello interazione
    /// </summary>
    public string InteractionLevelDescription => InteractionLevel switch
    {
        InteractionLevel.High => "ALTO RISCHIO",
        InteractionLevel.Moderate => "Rischio Moderato",
        InteractionLevel.Low => "Basso Rischio",
        _ => "Nessuna interazione nota"
    };
    
    /// <summary>
    /// Ha interazione con warfarin
    /// </summary>
    public bool HasInteraction => InteractionLevel != InteractionLevel.None;

    /// <summary>
    /// Indica se il farmaco proviene da Milleps
    /// </summary>
    public bool IsFromMilleps => Source == MedicationSource.Milleps;

    /// <summary>
    /// Descrizione origine del farmaco
    /// </summary>
    public string SourceDescription => Source switch
    {
        MedicationSource.Milleps => "Importato da Millewin",
        _ => "Inserito manualmente"
    };

    /// <summary>
    /// Nome completo farmaco (nome commerciale + principio attivo)
    /// </summary>
    public string FullName => string.IsNullOrEmpty(ActiveIngredient)
        ? MedicationName
        : $"{MedicationName} ({ActiveIngredient})";
}
