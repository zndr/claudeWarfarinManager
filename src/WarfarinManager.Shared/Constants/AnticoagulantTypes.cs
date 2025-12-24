namespace WarfarinManager.Shared.Constants;

/// <summary>
/// Costanti per i tipi di anticoagulanti supportati dall'applicazione
/// </summary>
public static class AnticoagulantTypes
{
    // Costanti per i tipi di anticoagulanti
    public const string Warfarin = "warfarin";
    public const string Dabigatran = "dabigatran";
    public const string Rivaroxaban = "rivaroxaban";
    public const string Apixaban = "apixaban";
    public const string Edoxaban = "edoxaban";
    public const string NotSpecified = "non specificato";
    public const string Other = "altro";

    /// <summary>
    /// Array di tutti i DOACs (Direct Oral Anticoagulants)
    /// </summary>
    public static readonly string[] AllDOACs = new[]
    {
        Dabigatran,
        Rivaroxaban,
        Apixaban,
        Edoxaban
    };

    /// <summary>
    /// Array di tutti i tipi validi
    /// </summary>
    public static readonly string[] AllTypes = new[]
    {
        Warfarin,
        Dabigatran,
        Rivaroxaban,
        Apixaban,
        Edoxaban,
        NotSpecified,
        Other
    };

    /// <summary>
    /// Verifica se un tipo è un DOAC (Direct Oral Anticoagulant)
    /// </summary>
    /// <param name="anticoagulantType">Il tipo di anticoagulante da verificare</param>
    /// <returns>True se è un DOAC, False altrimenti</returns>
    public static bool IsDoac(string? anticoagulantType)
    {
        if (string.IsNullOrWhiteSpace(anticoagulantType))
            return false;

        return AllDOACs.Contains(anticoagulantType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifica se un tipo è Warfarin
    /// </summary>
    /// <param name="anticoagulantType">Il tipo di anticoagulante da verificare</param>
    /// <returns>True se è Warfarin o se non è specificato (default Warfarin), False altrimenti</returns>
    public static bool IsWarfarin(string? anticoagulantType)
    {
        // Se null o vuoto, consideriamo il paziente come Warfarin (default per retrocompatibilità)
        if (string.IsNullOrWhiteSpace(anticoagulantType))
            return true;

        return string.Equals(anticoagulantType, Warfarin, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(anticoagulantType, NotSpecified, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Ottiene l'abbreviazione per la visualizzazione nella colonna "Tipo"
    /// </summary>
    /// <param name="anticoagulantType">Il tipo di anticoagulante</param>
    /// <returns>Abbreviazione: W/A/D/E/R/- o ?</returns>
    public static string GetAbbreviation(string? anticoagulantType)
    {
        // Se null o vuoto, consideriamo Warfarin (default per retrocompatibilità)
        if (string.IsNullOrWhiteSpace(anticoagulantType))
            return "W";

        return anticoagulantType.ToLowerInvariant() switch
        {
            Warfarin => "W",
            Apixaban => "A",
            Dabigatran => "D",
            Edoxaban => "E",
            Rivaroxaban => "R",
            NotSpecified => "W", // Default Warfarin
            Other => "?",
            _ => "?"
        };
    }

    /// <summary>
    /// Ottiene il nome completo del farmaco con il nome commerciale
    /// </summary>
    /// <param name="anticoagulantType">Il tipo di anticoagulante</param>
    /// <returns>Nome completo formattato (es. "Apixaban (Eliquis)")</returns>
    public static string GetDisplayName(string? anticoagulantType)
    {
        // Se null o vuoto, consideriamo Warfarin (default per retrocompatibilità)
        if (string.IsNullOrWhiteSpace(anticoagulantType))
            return "Warfarin (Coumadin)";

        return anticoagulantType.ToLowerInvariant() switch
        {
            Warfarin => "Warfarin (Coumadin)",
            Apixaban => "Apixaban (Eliquis)",
            Dabigatran => "Dabigatran (Pradaxa)",
            Edoxaban => "Edoxaban (Lixiana)",
            Rivaroxaban => "Rivaroxaban (Xarelto)",
            NotSpecified => "Warfarin (Coumadin)", // Default Warfarin
            Other => "Altro anticoagulante",
            _ => anticoagulantType // Ritorna il valore raw se non riconosciuto
        };
    }
}
