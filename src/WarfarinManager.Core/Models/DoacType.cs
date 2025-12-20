namespace WarfarinManager.Core.Models;

/// <summary>
/// Tipi di DOAC (Direct Oral Anticoagulants)
/// </summary>
public enum DoacType
{
    /// <summary>
    /// Apixaban (Eliquis) - Inibitore del fattore Xa
    /// </summary>
    Apixaban,

    /// <summary>
    /// Rivaroxaban (Xarelto) - Inibitore del fattore Xa
    /// </summary>
    Rivaroxaban,

    /// <summary>
    /// Dabigatran (Pradaxa) - Inibitore diretto della trombina (IIa)
    /// </summary>
    Dabigatran,

    /// <summary>
    /// Edoxaban (Lixiana) - Inibitore del fattore Xa
    /// </summary>
    Edoxaban
}
