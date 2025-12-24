using WarfarinManager.Shared.Enums;
using WarfarinManager.Shared.Models;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Interfaccia per il servizio di gestione delle interazioni farmacologiche dei DOAC
/// </summary>
public interface IDOACInteractionService
{
    /// <summary>
    /// Ottiene tutte le interazioni pericolose per un DOAC specifico
    /// </summary>
    List<DOACInteraction> GetDangerousInteractions(DOACType doacType);

    /// <summary>
    /// Ottiene tutte le interazioni (pericolose e non) per un DOAC
    /// </summary>
    List<DOACInteraction> GetAllInteractions(DOACType doacType);

    /// <summary>
    /// Verifica se un farmaco interagisce pericolosamente con un DOAC
    /// </summary>
    DOACInteraction? CheckInteraction(DOACType doacType, string drugName);

    /// <summary>
    /// Ottiene le interazioni per una lista di farmaci
    /// </summary>
    List<DOACInteraction> CheckMultipleInteractions(DOACType doacType, List<string> drugNames);
}
