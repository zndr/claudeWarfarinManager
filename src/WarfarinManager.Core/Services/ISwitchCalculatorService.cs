using WarfarinManager.Core.Models;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per il calcolo dei protocolli di switch tra Warfarin e DOAC
/// </summary>
public interface ISwitchCalculatorService
{
    /// <summary>
    /// Calcola il protocollo di switch completo
    /// </summary>
    SwitchProtocol CalculateProtocol(
        SwitchDirection direction,
        DoacType doacType,
        WarfarinType warfarinType,
        SwitchPatientParameters patientParameters);

    /// <summary>
    /// Valida i parametri del paziente
    /// </summary>
    (bool IsValid, List<string> Errors) ValidateParameters(SwitchPatientParameters parameters);

    /// <summary>
    /// Calcola il dosaggio DOAC raccomandato
    /// </summary>
    (string Dosage, string Rationale) CalculateDoacDosage(DoacType doacType, SwitchPatientParameters parameters);

    /// <summary>
    /// Verifica le controindicazioni all'uso di DOAC
    /// </summary>
    List<string> CheckContraindications(DoacType doacType, SwitchPatientParameters parameters);

    /// <summary>
    /// Verifica warnings e precauzioni
    /// </summary>
    List<string> CheckWarnings(DoacType doacType, WarfarinType warfarinType, SwitchPatientParameters parameters);

    /// <summary>
    /// Calcola la soglia INR per l'avvio del DOAC
    /// </summary>
    decimal GetInrThresholdForDoac(DoacType doacType);
}
