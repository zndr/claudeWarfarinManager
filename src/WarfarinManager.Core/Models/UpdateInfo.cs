namespace WarfarinManager.Core.Models;

/// <summary>
/// Informazioni sulla versione disponibile per l'aggiornamento
/// </summary>
public class UpdateInfo
{
    /// <summary>
    /// Versione disponibile (es. "1.2.0")
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// URL per scaricare l'installer
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Data di rilascio
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// Note di rilascio (Markdown supportato)
    /// </summary>
    public string ReleaseNotes { get; set; } = string.Empty;

    /// <summary>
    /// Indica se è un aggiornamento critico/obbligatorio
    /// </summary>
    public bool IsCritical { get; set; }

    /// <summary>
    /// Dimensione del file in byte
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Hash SHA256 del file per verifica integrità (opzionale)
    /// </summary>
    public string? Sha256Hash { get; set; }
}
