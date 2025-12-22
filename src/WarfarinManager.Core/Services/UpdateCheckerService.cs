using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using WarfarinManager.Core.Models;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Implementazione del servizio per il controllo automatico degli aggiornamenti via FTP
/// </summary>
public class UpdateCheckerService : IUpdateCheckerService
{
    private readonly ILogger<UpdateCheckerService> _logger;
    private readonly string _ftpHost;
    private readonly string _ftpUsername;
    private readonly string _ftpPassword;
    private readonly string _versionFileName;
    private readonly TimeSpan _timeout;

    public UpdateCheckerService(
        ILogger<UpdateCheckerService> logger,
        string ftpHost,
        string ftpUsername,
        string ftpPassword,
        string versionFileName = "version.json",
        int timeoutSeconds = 30)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ftpHost = ftpHost ?? throw new ArgumentNullException(nameof(ftpHost));
        _ftpUsername = ftpUsername ?? throw new ArgumentNullException(nameof(ftpUsername));
        _ftpPassword = ftpPassword ?? throw new ArgumentNullException(nameof(ftpPassword));
        _versionFileName = versionFileName;
        _timeout = TimeSpan.FromSeconds(timeoutSeconds);
    }

    public async Task<UpdateInfo?> CheckForUpdateAsync(string currentVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Controllo aggiornamenti disponibili. Versione corrente: {CurrentVersion}", currentVersion);

            // Scarica il file version.json dal server FTP
            var versionJson = await DownloadVersionFileAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(versionJson))
            {
                _logger.LogWarning("File versione vuoto o non trovato");
                return null;
            }

            // Deserializza le informazioni sulla versione
            var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(versionJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (updateInfo == null)
            {
                _logger.LogWarning("Impossibile deserializzare le informazioni sulla versione");
                return null;
            }

            // Verifica se la versione remota è più recente
            if (IsNewerVersion(updateInfo.Version, currentVersion))
            {
                _logger.LogInformation(
                    "Nuova versione disponibile: {NewVersion} (corrente: {CurrentVersion})",
                    updateInfo.Version,
                    currentVersion);
                return updateInfo;
            }

            _logger.LogInformation("Nessun aggiornamento disponibile. Versione corrente è aggiornata.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il controllo degli aggiornamenti");
            return null;
        }
    }

    public bool IsNewerVersion(string version, string currentVersion)
    {
        try
        {
            // Rimuovi eventuali caratteri non numerici e confronta usando Version
            var cleanVersion = CleanVersionString(version);
            var cleanCurrentVersion = CleanVersionString(currentVersion);

            var remoteVersion = new Version(cleanVersion);
            var localVersion = new Version(cleanCurrentVersion);

            return remoteVersion > localVersion;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Errore nel confronto versioni: {Version} vs {CurrentVersion}", version, currentVersion);
            return false;
        }
    }

    private async Task<string> DownloadVersionFileAsync(CancellationToken cancellationToken)
    {
        var ftpUrl = $"{_ftpHost.TrimEnd('/')}/{_versionFileName}";

        _logger.LogDebug("Download file versione da: {FtpUrl}", ftpUrl);

        var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
        request.Method = WebRequestMethods.Ftp.DownloadFile;
        request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
        request.Timeout = (int)_timeout.TotalMilliseconds;
        request.UseBinary = true;
        request.UsePassive = true;
        request.KeepAlive = false;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);

        try
        {
            using var response = (FtpWebResponse)await request.GetResponseAsync();
            using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream);

            var content = await reader.ReadToEndAsync(cts.Token);

            _logger.LogDebug("File versione scaricato con successo. Dimensione: {Size} bytes", content.Length);

            return content;
        }
        catch (WebException ex) when (ex.Response is FtpWebResponse ftpResponse)
        {
            _logger.LogError(ex, "Errore FTP durante il download. Status: {Status}", ftpResponse.StatusDescription);
            throw;
        }
    }

    private static string CleanVersionString(string version)
    {
        // Rimuovi 'v' iniziale se presente (es. "v1.2.0" -> "1.2.0")
        if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            version = version[1..];
        }

        // Se la versione ha meno di 4 componenti, aggiungi zeri
        // es. "1.2" -> "1.2.0.0"
        var parts = version.Split('.');
        while (parts.Length < 4)
        {
            version += ".0";
            parts = version.Split('.');
        }

        return version;
    }

    /// <summary>
    /// Calcola l'hash SHA256 di un file
    /// </summary>
    /// <param name="filePath">Percorso completo del file</param>
    /// <returns>Hash SHA256 in formato esadecimale (lowercase)</returns>
    public static string CalculateFileSha256(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File non trovato", filePath);
        }

        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Verifica se l'hash di un file corrisponde all'hash atteso
    /// </summary>
    /// <param name="filePath">Percorso del file da verificare</param>
    /// <param name="expectedHash">Hash SHA256 atteso (case-insensitive)</param>
    /// <returns>True se l'hash corrisponde, false altrimenti</returns>
    public static bool VerifyFileHash(string filePath, string expectedHash)
    {
        if (string.IsNullOrWhiteSpace(expectedHash))
        {
            // Se non è specificato un hash, salta la verifica
            return true;
        }

        try
        {
            var actualHash = CalculateFileSha256(filePath);
            return actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
