namespace WarfarinManager.Data.Entities;

/// <summary>
/// Terapia continuativa del paziente
/// Traccia farmaci assunti cronicamente che possono interagire con DOAC
/// </summary>
public class TerapiaContinuativa : BaseEntity
{
    /// <summary>
    /// ID del paziente (FK)
    /// </summary>
    public int PatientId { get; set; }

    /// <summary>
    /// Classe farmacologica (es: "Antiaggreganti", "FANS", "Antipertensivi")
    /// </summary>
    public string Classe { get; set; } = string.Empty;

    /// <summary>
    /// Principio attivo (es: "Acido Acetilsalicilico", "Ibuprofene")
    /// </summary>
    public string PrincipioAttivo { get; set; } = string.Empty;

    /// <summary>
    /// Nome commerciale (opzionale)
    /// </summary>
    public string? NomeCommerciale { get; set; }

    /// <summary>
    /// Dosaggio prescritto (es: "100 mg", "5 mg/2.5 mg")
    /// </summary>
    public string? Dosaggio { get; set; }

    /// <summary>
    /// Frequenza di assunzione giornaliera (es: "1x/die", "2x/die", "al bisogno")
    /// </summary>
    public string? FrequenzaGiornaliera { get; set; }

    /// <summary>
    /// Via di assunzione (Orale, Endovenosa, Topica, etc.)
    /// </summary>
    public string? ViaAssunzione { get; set; }

    /// <summary>
    /// Data di inizio della terapia
    /// </summary>
    public DateTime? DataInizio { get; set; }

    /// <summary>
    /// Data di fine della terapia (null se ancora in corso)
    /// </summary>
    public DateTime? DataFine { get; set; }

    /// <summary>
    /// Terapia attualmente attiva
    /// </summary>
    public bool Attiva { get; set; } = true;

    /// <summary>
    /// Indicazione terapeutica
    /// </summary>
    public string? Indicazione { get; set; }

    /// <summary>
    /// Note libere
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Motivo di sospensione (se sospesa)
    /// </summary>
    public string? MotivoSospensione { get; set; }

    /// <summary>
    /// Flag che indica se il dato è stato importato automaticamente (es: da Milleps)
    /// </summary>
    public bool Importato { get; set; }

    /// <summary>
    /// Fonte dei dati (Manuale, Milleps, HL7, etc.)
    /// </summary>
    public string? FonteDati { get; set; }

    // ====== NAVIGATION PROPERTIES ======

    /// <summary>
    /// Paziente di riferimento
    /// </summary>
    public Patient? Patient { get; set; }

    // ====== HELPER PROPERTIES ======

    /// <summary>
    /// Verifica se il farmaco è un antiaggregante piastrinico
    /// </summary>
    public bool IsAntiaggregante
    {
        get
        {
            if (string.IsNullOrEmpty(Classe) && string.IsNullOrEmpty(PrincipioAttivo))
                return false;

            var classe = Classe?.ToLower() ?? "";
            var principio = PrincipioAttivo?.ToLower() ?? "";

            return classe.Contains("antiaggregant") ||
                   principio.Contains("aspirina") ||
                   principio.Contains("acido acetilsalicilico") ||
                   principio.Contains("clopidogrel") ||
                   principio.Contains("ticagrelor") ||
                   principio.Contains("prasugrel") ||
                   principio.Contains("ticlopidina") ||
                   principio.Contains("dipiridamolo");
        }
    }

    /// <summary>
    /// Verifica se il farmaco è un FANS
    /// </summary>
    public bool IsFANS
    {
        get
        {
            if (string.IsNullOrEmpty(Classe) && string.IsNullOrEmpty(PrincipioAttivo))
                return false;

            var classe = Classe?.ToLower() ?? "";
            var principio = PrincipioAttivo?.ToLower() ?? "";

            return classe.Contains("fans") ||
                   classe.Contains("nsaid") ||
                   classe.Contains("antinfiammatori non steroidei") ||
                   principio.Contains("ibuprofene") ||
                   principio.Contains("naprossene") ||
                   principio.Contains("ketoprofene") ||
                   principio.Contains("diclofenac") ||
                   principio.Contains("indometacina") ||
                   principio.Contains("celecoxib") ||
                   principio.Contains("etoricoxib") ||
                   principio.Contains("nimesulide") ||
                   principio.Contains("piroxicam");
        }
    }

    // ====== METODI ======

    /// <summary>
    /// Sospende la terapia
    /// </summary>
    public void Sospendi(string? motivazione = null)
    {
        Attiva = false;
        DataFine = DateTime.Now;
        MotivoSospensione = motivazione;
    }

    /// <summary>
    /// Riattiva la terapia sospesa
    /// </summary>
    public void Riattiva()
    {
        Attiva = true;
        DataFine = null;
        MotivoSospensione = null;
    }
}
