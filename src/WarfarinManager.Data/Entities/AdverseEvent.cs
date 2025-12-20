using System;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Data.Entities;

/// <summary>
/// Rappresenta un evento avverso occorso durante la terapia con Warfarin
/// </summary>
public class AdverseEvent : BaseEntity
{
    /// <summary>
    /// ID del paziente
    /// </summary>
    public int PatientId { get; set; }

    /// <summary>
    /// Data di comparsa della reazione avversa
    /// </summary>
    public DateTime OnsetDate { get; set; }

    /// <summary>
    /// Tipo di reazione avversa (classificata secondo gravità e frequenza)
    /// </summary>
    public AdverseReactionType ReactionType { get; set; }

    /// <summary>
    /// Gravità della reazione (automaticamente derivata dal tipo)
    /// </summary>
    public AdverseReactionSeverity Severity { get; set; }

    /// <summary>
    /// Grado di certezza della correlazione con il Warfarin
    /// </summary>
    public CertaintyLevel CertaintyLevel { get; set; }

    /// <summary>
    /// Provvedimenti adottati (testo libero)
    /// </summary>
    public string? MeasuresTaken { get; set; }

    /// <summary>
    /// INR al momento dell'evento (se disponibile)
    /// </summary>
    public decimal? INRAtEvent { get; set; }

    /// <summary>
    /// Note aggiuntive
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// ID controllo INR collegato (opzionale)
    /// </summary>
    public int? LinkedINRControlId { get; set; }

    // Navigation Properties

    /// <summary>
    /// Paziente associato
    /// </summary>
    public Patient Patient { get; set; } = null!;

    /// <summary>
    /// Controllo INR collegato (se presente)
    /// </summary>
    public INRControl? LinkedINRControl { get; set; }
}
