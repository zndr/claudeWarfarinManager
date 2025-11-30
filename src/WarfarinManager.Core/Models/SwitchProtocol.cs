namespace WarfarinManager.Core.Models;

/// <summary>
/// Protocollo di switch terapeutico con istruzioni dettagliate
/// </summary>
public class SwitchProtocol
{
    /// <summary>
    /// Direzione dello switch
    /// </summary>
    public SwitchDirection Direction { get; set; }

    /// <summary>
    /// Tipo di DOAC coinvolto
    /// </summary>
    public DoacType DoacType { get; set; }

    /// <summary>
    /// Tipo di Warfarin (Warfarin o Acenocumarolo)
    /// </summary>
    public WarfarinType WarfarinType { get; set; }

    /// <summary>
    /// Dosaggio raccomandato del DOAC
    /// </summary>
    public string RecommendedDoacDosage { get; set; } = string.Empty;

    /// <summary>
    /// Motivazione del dosaggio (es: ridotto per età/peso/ClCr)
    /// </summary>
    public string DosageRationale { get; set; } = string.Empty;

    /// <summary>
    /// Timeline step-by-step del protocollo
    /// </summary>
    public List<SwitchTimelineStep> Timeline { get; set; } = new();

    /// <summary>
    /// Controindicazioni identificate
    /// </summary>
    public List<string> Contraindications { get; set; } = new();

    /// <summary>
    /// Warnings e precauzioni
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Note cliniche aggiuntive
    /// </summary>
    public List<string> ClinicalNotes { get; set; } = new();

    /// <summary>
    /// Piano di monitoraggio post-switch
    /// </summary>
    public string MonitoringPlan { get; set; } = string.Empty;

    /// <summary>
    /// Soglia INR per lo switch (solo per Warfarin → DOAC)
    /// </summary>
    public decimal? InrThreshold { get; set; }

    /// <summary>
    /// Indica se lo switch è sicuro da procedere
    /// </summary>
    public bool IsSafeToSwitch { get; set; }

    /// <summary>
    /// Data di calcolo del protocollo
    /// </summary>
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// Step della timeline del protocollo
/// </summary>
public class SwitchTimelineStep
{
    /// <summary>
    /// Giorno relativo (0 = oggi)
    /// </summary>
    public int Day { get; set; }

    /// <summary>
    /// Descrizione dell'azione da eseguire
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Dettagli aggiuntivi
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Tipo di step (info, action, monitoring, warning)
    /// </summary>
    public string StepType { get; set; } = "action";
}
