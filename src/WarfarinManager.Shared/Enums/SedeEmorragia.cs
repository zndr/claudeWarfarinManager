namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Sede anatomica dell'emorragia
/// Rilevante per la stratificazione del rischio e la decisione terapeutica
/// </summary>
public enum SedeEmorragia
{
    /// <summary>
    /// Nessuna sede specificata (no emorragia)
    /// </summary>
    Nessuna = 0,

    /// <summary>
    /// Emorragia cutanea (ecchimosi, petecchie, porpora)
    /// Generalmente di gravità minore
    /// </summary>
    Cutanea = 1,

    /// <summary>
    /// Epistassi (sanguinamento nasale)
    /// Può essere controllato con misure locali
    /// </summary>
    Nasale = 2,

    /// <summary>
    /// Sanguinamento gengivale
    /// Generalmente minore
    /// </summary>
    Gengivale = 3,

    /// <summary>
    /// Emorragia gastrointestinale
    /// Può essere maggiore se sanguinamento attivo (melena, ematochezia)
    /// </summary>
    Gastrointestinale = 4,

    /// <summary>
    /// Ematuria (sangue nelle urine)
    /// Classificazione dipende da entità
    /// </summary>
    Urinaria = 5,

    /// <summary>
    /// Emorragia intracranica (cerebrale, subaracnoidea, subdurale, epidurale)
    /// SEMPRE considerata rischio vitale
    /// Richiede imaging urgente e intervento neurochirurgico
    /// </summary>
    Intracranica = 6,

    /// <summary>
    /// Emorragia retroperitoneale
    /// Alto rischio, difficile da controllare
    /// Generalmente classificata come rischio vitale
    /// </summary>
    Retroperitoneale = 7,

    /// <summary>
    /// Altra sede non specificata
    /// Es: emoftoe, emottisi, ematoma muscolare, emorragia oculare, ecc.
    /// </summary>
    Altra = 8
}
