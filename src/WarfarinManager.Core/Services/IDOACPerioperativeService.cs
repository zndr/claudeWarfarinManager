using WarfarinManager.Data.Entities;
using WarfarinManager.Shared.Enums;
using WarfarinManager.Shared.Models;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per la gestione perioperatoria dei DOAC
/// Basato su linee guida FCSA 2021, Studio PAUSE, ASRA 2025
/// </summary>
public interface IDOACPerioperativeService
{
    /// <summary>
    /// Genera protocollo perioperatorio completo per un DOAC
    /// </summary>
    DOACPerioperativeProtocol GenerateProtocol(
        DOACType doacType,
        double egfr,
        BleedingRisk bleedingRisk,
        ThromboembolicRisk teRisk,
        DateTime surgeryDate,
        SurgeryType surgeryType,
        int patientAge,
        double? patientWeight = null);

    /// <summary>
    /// Calcola ore di sospensione pre-operatoria
    /// </summary>
    int CalculatePreOpSuspensionHours(
        DOACType doacType,
        RenalFunction renalFunction,
        BleedingRisk bleedingRisk);

    /// <summary>
    /// Calcola ore per ripresa post-operatoria
    /// </summary>
    (int minHours, int maxHours) CalculatePostOpResumeHours(
        BleedingRisk bleedingRisk,
        ThromboembolicRisk teRisk);

    /// <summary>
    /// Verifica se è raccomandato test livelli plasmatici
    /// </summary>
    bool ShouldTestPlasmaLevels(
        int patientAge,
        RenalFunction renalFunction,
        double? patientWeight,
        BleedingRisk bleedingRisk);

    /// <summary>
    /// Formatta il protocollo per esportazione
    /// </summary>
    string FormatProtocolForExport(DOACPerioperativeProtocol protocol, Patient patient);
}
