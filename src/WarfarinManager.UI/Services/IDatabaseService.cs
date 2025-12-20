using System.Threading.Tasks;

namespace WarfarinManager.UI.Services;

/// <summary>
/// Servizio per la gestione del database (backup, ripristino, manutenzione)
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// Ottiene il percorso del file database
    /// </summary>
    string DatabasePath { get; }

    /// <summary>
    /// Ottiene la dimensione del database in MB
    /// </summary>
    Task<double> GetDatabaseSizeAsync();

    /// <summary>
    /// Ottiene statistiche sul database
    /// </summary>
    Task<DatabaseStatistics> GetStatisticsAsync();

    /// <summary>
    /// Esegue un backup del database
    /// </summary>
    /// <param name="destinationPath">Percorso dove salvare il backup</param>
    Task<bool> BackupDatabaseAsync(string destinationPath);

    /// <summary>
    /// Ripristina il database da un backup
    /// </summary>
    /// <param name="backupPath">Percorso del file di backup</param>
    Task<bool> RestoreDatabaseAsync(string backupPath);

    /// <summary>
    /// Compatta il database (VACUUM)
    /// </summary>
    Task<bool> CompactDatabaseAsync();

    /// <summary>
    /// Verifica l'integrità del database
    /// </summary>
    Task<DatabaseIntegrityResult> CheckIntegrityAsync();
}

/// <summary>
/// Statistiche del database
/// </summary>
public record DatabaseStatistics
{
    public int TotalPatients { get; init; }
    public int TotalINRControls { get; init; }
    public int TotalIndications { get; init; }
    public int TotalAdverseEvents { get; init; }
    public double DatabaseSizeMB { get; init; }
    public DateTime? LastBackupDate { get; init; }
}

/// <summary>
/// Risultato della verifica di integrità
/// </summary>
public record DatabaseIntegrityResult
{
    public bool IsValid { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string> Errors { get; init; } = new();
}
