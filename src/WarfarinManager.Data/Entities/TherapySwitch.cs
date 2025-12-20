namespace WarfarinManager.Data.Entities;

/// <summary>
/// Entità che rappresenta uno switch terapeutico tra Warfarin e DOAC
/// </summary>
public class TherapySwitch : BaseEntity
{
    /// <summary>
    /// ID del paziente
    /// </summary>
    public int PatientId { get; set; }

    /// <summary>
    /// Navigazione al paziente
    /// </summary>
    public Patient Patient { get; set; } = null!;

    /// <summary>
    /// Data dello switch
    /// </summary>
    public DateTime SwitchDate { get; set; }

    /// <summary>
    /// Direzione dello switch (WarfarinToDoac o DoacToWarfarin)
    /// </summary>
    public string Direction { get; set; } = string.Empty;

    /// <summary>
    /// Tipo di DOAC coinvolto (Apixaban, Rivaroxaban, Dabigatran, Edoxaban)
    /// </summary>
    public string DoacType { get; set; } = string.Empty;

    /// <summary>
    /// Tipo di Warfarin (Warfarin o Acenocumarolo)
    /// </summary>
    public string WarfarinType { get; set; } = string.Empty;

    /// <summary>
    /// INR al momento dello switch
    /// </summary>
    public decimal? InrAtSwitch { get; set; }

    /// <summary>
    /// Clearance creatinina al momento dello switch (mL/min)
    /// </summary>
    public decimal CreatinineClearance { get; set; }

    /// <summary>
    /// Età del paziente al momento dello switch
    /// </summary>
    public int AgeAtSwitch { get; set; }

    /// <summary>
    /// Peso del paziente al momento dello switch (kg)
    /// </summary>
    public decimal WeightAtSwitch { get; set; }

    /// <summary>
    /// Dosaggio DOAC raccomandato
    /// </summary>
    public string RecommendedDosage { get; set; } = string.Empty;

    /// <summary>
    /// Motivazione del dosaggio
    /// </summary>
    public string DosageRationale { get; set; } = string.Empty;

    /// <summary>
    /// Protocollo seguito (JSON serializzato della timeline)
    /// </summary>
    public string ProtocolTimeline { get; set; } = string.Empty;

    /// <summary>
    /// Controindicazioni rilevate (JSON)
    /// </summary>
    public string? Contraindications { get; set; }

    /// <summary>
    /// Warnings rilevati (JSON)
    /// </summary>
    public string? Warnings { get; set; }

    /// <summary>
    /// Note cliniche
    /// </summary>
    public string? ClinicalNotes { get; set; }

    /// <summary>
    /// Piano di monitoraggio
    /// </summary>
    public string? MonitoringPlan { get; set; }

    /// <summary>
    /// Data prevista per il primo follow-up
    /// </summary>
    public DateTime? FirstFollowUpDate { get; set; }

    /// <summary>
    /// Indica se il follow-up è stato completato
    /// </summary>
    public bool FollowUpCompleted { get; set; }

    /// <summary>
    /// Note del follow-up
    /// </summary>
    public string? FollowUpNotes { get; set; }

    /// <summary>
    /// Indica se lo switch è stato completato con successo
    /// </summary>
    public bool SwitchCompleted { get; set; }

    /// <summary>
    /// Data di completamento dello switch
    /// </summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Esito dello switch (es: "Successo", "Complicazioni", "Sospeso")
    /// </summary>
    public string? Outcome { get; set; }
}
