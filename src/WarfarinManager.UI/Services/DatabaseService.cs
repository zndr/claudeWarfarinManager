using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Repositories.Interfaces;

namespace WarfarinManager.UI.Services;

/// <summary>
/// Implementazione del servizio di gestione database
/// </summary>
public class DatabaseService : IDatabaseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DatabaseService> _logger;
    private readonly string _databasePath;

    public DatabaseService(IUnitOfWork unitOfWork, ILogger<DatabaseService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Estrai il percorso del database dalla connection string
        _databasePath = GetDatabasePath();
    }

    public string DatabasePath => _databasePath;

    public async Task<double> GetDatabaseSizeAsync()
    {
        try
        {
            if (!File.Exists(_databasePath))
                return 0;

            var fileInfo = new FileInfo(_databasePath);
            return fileInfo.Length / (1024.0 * 1024.0); // Converti in MB
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel calcolo dimensione database");
            return 0;
        }
    }

    public async Task<DatabaseStatistics> GetStatisticsAsync()
    {
        try
        {
            var totalPatients = await _unitOfWork.Database.Patients.CountAsync();
            var totalINRControls = await _unitOfWork.Database.INRControls.CountAsync();
            var totalIndications = await _unitOfWork.Database.Indications.CountAsync();
            var totalAdverseEvents = await _unitOfWork.Database.AdverseEvents.CountAsync();
            var dbSize = await GetDatabaseSizeAsync();

            // Cerca la data dell'ultimo backup (se esiste un file di log o preferenze)
            DateTime? lastBackup = GetLastBackupDate();

            return new DatabaseStatistics
            {
                TotalPatients = totalPatients,
                TotalINRControls = totalINRControls,
                TotalIndications = totalIndications,
                TotalAdverseEvents = totalAdverseEvents,
                DatabaseSizeMB = dbSize,
                LastBackupDate = lastBackup
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero statistiche database");
            throw;
        }
    }

    public async Task<bool> BackupDatabaseAsync(string destinationPath)
    {
        try
        {
            _logger.LogInformation("Avvio backup database verso {Destination}", destinationPath);

            if (!File.Exists(_databasePath))
            {
                _logger.LogError("Database non trovato: {Path}", _databasePath);
                return false;
            }

            // Assicurati che la directory di destinazione esista
            var destinationDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            // Chiudi eventuali connessioni aperte
            await _unitOfWork.Database.Database.CloseConnectionAsync();

            // Copia il file database
            File.Copy(_databasePath, destinationPath, overwrite: true);

            // Salva la data del backup
            SaveLastBackupDate(DateTime.Now);

            _logger.LogInformation("Backup completato con successo");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il backup del database");
            return false;
        }
    }

    public async Task<bool> RestoreDatabaseAsync(string backupPath)
    {
        try
        {
            _logger.LogInformation("Avvio ripristino database da {Source}", backupPath);

            if (!File.Exists(backupPath))
            {
                _logger.LogError("File di backup non trovato: {Path}", backupPath);
                return false;
            }

            // Verifica che il backup sia valido
            var integrityCheck = await CheckBackupIntegrityAsync(backupPath);
            if (!integrityCheck.IsValid)
            {
                _logger.LogError("File di backup non valido: {Message}", integrityCheck.Message);
                return false;
            }

            // Chiudi tutte le connessioni
            await _unitOfWork.Database.Database.CloseConnectionAsync();

            // Crea un backup di sicurezza del database corrente
            var emergencyBackup = _databasePath + ".emergency_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            if (File.Exists(_databasePath))
            {
                File.Copy(_databasePath, emergencyBackup, overwrite: true);
                _logger.LogInformation("Backup di emergenza creato: {Path}", emergencyBackup);
            }

            // Ripristina il database
            File.Copy(backupPath, _databasePath, overwrite: true);

            // Riapri la connessione
            await _unitOfWork.Database.Database.OpenConnectionAsync();

            _logger.LogInformation("Ripristino completato con successo");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il ripristino del database");
            return false;
        }
    }

    public async Task<bool> CompactDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Avvio compattazione database");

            await _unitOfWork.Database.Database.ExecuteSqlRawAsync("VACUUM;");
            await _unitOfWork.Database.Database.ExecuteSqlRawAsync("ANALYZE;");

            _logger.LogInformation("Compattazione completata con successo");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la compattazione del database");
            return false;
        }
    }

    public async Task<DatabaseIntegrityResult> CheckIntegrityAsync()
    {
        try
        {
            _logger.LogInformation("Verifica integrità database");

            var errors = new List<string>();

            // Esegui PRAGMA integrity_check
            var connection = _unitOfWork.Database.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;

            if (!wasOpen)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA integrity_check;";
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var result = reader.GetString(0);
                        if (result != "ok")
                        {
                            errors.Add(result);
                        }
                    }
                }
            }

            if (!wasOpen)
                await connection.CloseAsync();

            var isValid = errors.Count == 0;
            var message = isValid ? "Database integro" : $"Trovati {errors.Count} errori";

            _logger.LogInformation("Verifica integrità completata: {IsValid}", isValid);

            return new DatabaseIntegrityResult
            {
                IsValid = isValid,
                Message = message,
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la verifica di integrità");
            return new DatabaseIntegrityResult
            {
                IsValid = false,
                Message = $"Errore: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private string GetDatabasePath()
    {
        try
        {
            var connection = _unitOfWork.Database.Database.GetDbConnection();
            if (connection is SqliteConnection sqliteConnection)
            {
                var connectionString = sqliteConnection.ConnectionString;
                var builder = new SqliteConnectionStringBuilder(connectionString);
                return builder.DataSource;
            }

            // Fallback: cerca nella connection string
            var connStr = connection.ConnectionString;
            var match = System.Text.RegularExpressions.Regex.Match(connStr, @"Data Source=([^;]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            _logger.LogWarning("Impossibile determinare il percorso del database dalla connection string");
            return "Unknown";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero del percorso database");
            return "Unknown";
        }
    }

    private async Task<DatabaseIntegrityResult> CheckBackupIntegrityAsync(string backupPath)
    {
        try
        {
            // Verifica che il file sia un database SQLite valido
            var connectionString = $"Data Source={backupPath};Mode=ReadOnly";
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA integrity_check;";

            var result = await command.ExecuteScalarAsync();
            var isValid = result?.ToString() == "ok";

            await connection.CloseAsync();

            return new DatabaseIntegrityResult
            {
                IsValid = isValid,
                Message = isValid ? "Backup valido" : "Backup corrotto",
                Errors = isValid ? new List<string>() : new List<string> { "Database corrotto" }
            };
        }
        catch (Exception ex)
        {
            return new DatabaseIntegrityResult
            {
                IsValid = false,
                Message = $"File non valido: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private DateTime? GetLastBackupDate()
    {
        try
        {
            var backupLogPath = Path.Combine(
                Path.GetDirectoryName(_databasePath) ?? "",
                "last_backup.txt");

            if (File.Exists(backupLogPath))
            {
                var dateStr = File.ReadAllText(backupLogPath).Trim();
                if (DateTime.TryParse(dateStr, out var date))
                {
                    return date;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Errore nel recupero data ultimo backup");
        }

        return null;
    }

    private void SaveLastBackupDate(DateTime date)
    {
        try
        {
            var backupLogPath = Path.Combine(
                Path.GetDirectoryName(_databasePath) ?? "",
                "last_backup.txt");

            File.WriteAllText(backupLogPath, date.ToString("O"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Errore nel salvataggio data backup");
        }
    }
}
