using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Npgsql;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Shared.Models;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per l'estrazione di dati clinici dal database PostgreSQL Milleps
/// </summary>
public class MillepsDataService : IMillepsDataService
{
    private readonly ILogger<MillepsDataService> _logger;
    private const string ConnectionString = "Host=127.0.0.1;Port=5432;Database=milleps;Username=dba;Password=sql;";

    public MillepsDataService(ILogger<MillepsDataService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string?> GetMillepsPatientCodeAsync(string codiceFiscale)
    {
        if (string.IsNullOrWhiteSpace(codiceFiscale))
            return null;

        try
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            var query = "SELECT codice FROM pazienti WHERE UPPER(codice_fiscale) = UPPER(@CodiceFiscale) LIMIT 1";

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@CodiceFiscale", codiceFiscale);

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero del codice paziente Milleps per CF: {CodiceFiscale}", codiceFiscale);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<MillepsBiometricData?> GetBiometricDataAsync(string codiceFiscale)
    {
        var patientCode = await GetMillepsPatientCodeAsync(codiceFiscale);
        if (string.IsNullOrEmpty(patientCode))
        {
            _logger.LogWarning("Paziente non trovato in Milleps per CF: {CodiceFiscale}", codiceFiscale);
            return null;
        }

        try
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            _logger.LogInformation("Recupero dati biometrici per paziente {PatientCode}", patientCode);

            // Query per peso e altezza più recenti
            // Usa i pattern definiti in MillepsExamPatterns (versione semplificata)
            var query = @"
                SELECT ac_des, ac_val, data_open
                FROM cart_accert
                WHERE codice = @PatientCode
                  AND (
                    UPPER(ac_des) LIKE '%PESO%'
                    OR UPPER(ac_des) LIKE '%ALTEZZA%'
                    OR UPPER(ac_des) LIKE '%STATURA%'
                  )
                ORDER BY data_open DESC";

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@PatientCode", patientCode);

            decimal? weight = null;
            decimal? height = null;
            DateTime? weightDate = null;
            DateTime? heightDate = null;

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var examName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                var rawValue = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                var examDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2);

                var examType = MillepsExamPatterns.GetExamType(examName);
                var numericValue = ParseNumericValue(rawValue);

                if (examType == LabExamType.Weight && !weight.HasValue && numericValue.HasValue)
                {
                    weight = numericValue;
                    weightDate = examDate;
                }
                else if (examType == LabExamType.Height && !height.HasValue && numericValue.HasValue)
                {
                    height = numericValue;
                    heightDate = examDate;
                }

                // Se abbiamo entrambi, possiamo uscire
                if (weight.HasValue && height.HasValue)
                    break;
            }

            if (!weight.HasValue && !height.HasValue)
            {
                _logger.LogInformation("Nessun dato biometrico trovato per paziente {PatientCode}", patientCode);
                return null;
            }

            return new MillepsBiometricData(weight, height, weightDate, heightDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero dei dati biometrici per CF: {CodiceFiscale}", codiceFiscale);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<MillepsLabResultsCollection> GetLabResultsAsync(string codiceFiscale, DateTime? fromDate = null)
    {
        var results = new MillepsLabResultsCollection();

        var patientCode = await GetMillepsPatientCodeAsync(codiceFiscale);
        if (string.IsNullOrEmpty(patientCode))
        {
            _logger.LogWarning("Paziente non trovato in Milleps per CF: {CodiceFiscale}", codiceFiscale);
            return results;
        }

        var cutoffDate = fromDate ?? DateTime.Today.AddDays(-30);

        try
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            _logger.LogInformation("Recupero esami laboratorio per paziente {PatientCode} dal {FromDate}",
                patientCode, cutoffDate.ToString("yyyy-MM-dd"));

            // Query per esami pertinenti - pattern esatti da milleps
            var query = @"
                SELECT ac_des, ac_val, data_open
                FROM cart_accert
                WHERE codice = @PatientCode
                  AND data_open >= @FromDate
                  AND (
                    UPPER(ac_des) = 'CREATININA'
                    OR UPPER(ac_des) LIKE 'HGB EMOGLO%'
                    OR UPPER(ac_des) = 'PLT PIASTRINE'
                    OR UPPER(ac_des) LIKE 'ASPARTATO AMINO%'
                    OR UPPER(ac_des) LIKE 'ALANINA AMINO%'
                    OR UPPER(ac_des) = 'INR'
                  )
                ORDER BY data_open DESC";

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@PatientCode", patientCode);
            cmd.Parameters.AddWithValue("@FromDate", cutoffDate);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var examName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                var rawValue = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                var examDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2);

                var examType = MillepsExamPatterns.GetExamType(examName);
                var numericValue = ParseNumericValue(rawValue);

                if (!examType.HasValue || !numericValue.HasValue || !examDate.HasValue)
                    continue;

                // Assegna al campo appropriato solo se non già valorizzato (prendiamo il più recente)
                switch (examType.Value)
                {
                    case LabExamType.Creatinine when !results.Creatinine.HasValue:
                        results.Creatinine = numericValue;
                        results.CreatinineDate = examDate;
                        break;

                    case LabExamType.Hemoglobin when !results.Hemoglobin.HasValue:
                        results.Hemoglobin = numericValue;
                        results.HemoglobinDate = examDate;
                        break;

                    case LabExamType.Platelets when !results.Platelets.HasValue:
                        // Le piastrine potrebbero essere in migliaia (x10^3)
                        var platelets = numericValue.Value;
                        if (platelets < 1000) // Probabilmente in migliaia
                            platelets *= 1000;
                        results.Platelets = (int)platelets;
                        results.PlateletsDate = examDate;
                        break;

                    case LabExamType.AST when !results.AST.HasValue:
                        results.AST = (int)numericValue.Value;
                        results.ASTDate = examDate;
                        break;

                    case LabExamType.ALT when !results.ALT.HasValue:
                        results.ALT = (int)numericValue.Value;
                        results.ALTDate = examDate;
                        break;

                    case LabExamType.INR when !results.INR.HasValue:
                        results.INR = numericValue;
                        results.INRDate = examDate;
                        break;
                }
            }

            _logger.LogInformation("Recuperati esami laboratorio: Creat={Creat}, Hb={Hb}, PLT={Plt}, AST={AST}, ALT={ALT}, INR={INR}",
                results.Creatinine, results.Hemoglobin, results.Platelets, results.AST, results.ALT, results.INR);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero degli esami laboratorio per CF: {CodiceFiscale}", codiceFiscale);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<MillepsMedication>> GetActiveMedicationsAsync(string codiceFiscale)
    {
        var medications = new List<MillepsMedication>();

        var patientCode = await GetMillepsPatientCodeAsync(codiceFiscale);
        if (string.IsNullOrEmpty(patientCode))
        {
            _logger.LogWarning("Paziente non trovato in Milleps per CF: {CodiceFiscale}", codiceFiscale);
            return medications;
        }

        try
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            _logger.LogInformation("Recupero terapie continuative per paziente {PatientCode}", patientCode);

            // Query per terapie continuative attive
            var query = @"
                SELECT DISTINCT
                    ct.co_atc,
                    ct.te_des,
                    ct.te_dose,
                    ct.data_open
                FROM cart_terap ct
                WHERE ct.codice = @PatientCode
                  AND ct.te_c_flag = 'C'
                  AND ct.co_atc IS NOT NULL
                  AND ct.co_atc <> ''
                ORDER BY ct.te_des";

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@PatientCode", patientCode);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var atcCode = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                var drugName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                var dosage = reader.IsDBNull(2) ? null : reader.GetString(2);
                var startDate = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3);

                if (!string.IsNullOrEmpty(atcCode))
                {
                    medications.Add(new MillepsMedication(atcCode, drugName, dosage, startDate));
                }
            }

            _logger.LogInformation("Recuperate {Count} terapie continuative per paziente {PatientCode}",
                medications.Count, patientCode);

            return medications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero delle terapie per CF: {CodiceFiscale}", codiceFiscale);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Test connessione al database PostgreSQL Milleps...");

            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            _logger.LogInformation("Connessione al database Milleps riuscita");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il test di connessione al database Milleps");
            return false;
        }
    }

    /// <summary>
    /// Estrae il valore numerico da una stringa che può contenere unità di misura
    /// Esempi: "1.2", "1,2", "1.2 mg/dL", "120 U/L", "150.000", "150000"
    /// </summary>
    private static decimal? ParseNumericValue(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        // Rimuovi spazi e caratteri non numerici all'inizio/fine
        var cleaned = rawValue.Trim();

        // Estrai solo la parte numerica (inclusi . , e -)
        var match = Regex.Match(cleaned, @"^[\-]?[\d]+([.,][\d]+)?");
        if (!match.Success)
            return null;

        var numericPart = match.Value;

        // Gestisci separatori decimali (sia . che ,)
        // Se c'è un punto seguito da 3 cifre, è probabilmente un separatore delle migliaia italiano
        if (Regex.IsMatch(numericPart, @"^\d+\.\d{3}$"))
        {
            // Es: "150.000" -> 150000
            numericPart = numericPart.Replace(".", "");
        }
        else
        {
            // Sostituisci virgola con punto per parsing standard
            numericPart = numericPart.Replace(",", ".");
        }

        if (decimal.TryParse(numericPart, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return null;
    }
}
