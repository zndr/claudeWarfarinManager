using WarfarinManager.Shared.Enums;
using WarfarinManager.Shared.Models;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Interfaccia per il servizio di gestione indicazioni e controindicazioni DOAC
/// </summary>
public interface IDOACClinicalService
{
    /// <summary>
    /// Ottiene tutte le indicazioni autorizzate EMA per un DOAC
    /// </summary>
    List<DOACIndication> GetApprovedIndications(DOACType doacType);

    /// <summary>
    /// Verifica se un'indicazione è autorizzata per un DOAC
    /// </summary>
    bool IsIndicationApproved(DOACType doacType, string indicationCode);

    /// <summary>
    /// Ottiene tutte le controindicazioni specifiche per un DOAC
    /// </summary>
    List<DOACContraindication> GetContraindications(DOACType doacType);

    /// <summary>
    /// Ottiene le controindicazioni assolute
    /// </summary>
    List<DOACContraindication> GetAbsoluteContraindications(DOACType doacType);

    /// <summary>
    /// Ottiene le precauzioni/limiti d'uso per un DOAC
    /// </summary>
    List<DOACContraindication> GetPrecautions(DOACType doacType);

    /// <summary>
    /// Determina la funzione renale in base a eGFR
    /// </summary>
    RenalFunction DetermineRenalFunction(double egfr);

    /// <summary>
    /// Verifica se un DOAC è controindicato con una specifica funzione renale
    /// </summary>
    bool IsContraindicatedWithRenalFunction(DOACType doacType, RenalFunction renalFunction);

    /// <summary>
    /// Verifica se è necessaria riduzione dose in base a funzione renale
    /// </summary>
    bool RequiresDoseReductionForRenalFunction(DOACType doacType, RenalFunction renalFunction);
}
