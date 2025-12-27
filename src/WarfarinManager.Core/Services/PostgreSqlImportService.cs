using Microsoft.Extensions.Logging;
using Npgsql;
using WarfarinManager.Shared.Enums;
using WarfarinManager.Shared.Models;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per l'importazione di pazienti dal database PostgreSQL Milleps
/// </summary>
public class PostgreSqlImportService
{
    private readonly ILogger<PostgreSqlImportService> _logger;

    public PostgreSqlImportService(ILogger<PostgreSqlImportService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Importa i pazienti in terapia anticoagulante dal database Milleps
    /// </summary>
    /// <param name="codiceFiscaleMedico">Codice fiscale del medico</param>
    /// <param name="therapyType">Tipo di terapia da filtrare</param>
    /// <returns>Lista dei pazienti trovati</returns>
    public async Task<List<MillepsPatientDto>> ImportPatientsAsync(string codiceFiscaleMedico, TherapyType therapyType)
    {
        var patients = new List<MillepsPatientDto>();

        var connectionString = "Host=127.0.0.1;Port=5432;Database=milleps;Username=dba;Password=sql;";

        try
        {
            _logger.LogInformation("Connessione al database PostgreSQL Milleps...");

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            _logger.LogInformation("Connessione stabilita. Esecuzione query importazione pazienti (Tipo: {TherapyType})...", therapyType);

            // Costruisci la condizione ATC in base al tipo di terapia
            string atcCondition = therapyType switch
            {
                TherapyType.Warfarin => "ct.co_atc = 'B01AA03'",
                TherapyType.DOAC => "(ct.co_atc = 'B01AE07' OR ct.co_atc = 'B01AF01' OR ct.co_atc = 'B01AF02' OR ct.co_atc = 'B01AF03')",
                TherapyType.Both => "(ct.co_atc = 'B01AE07' OR ct.co_atc = 'B01AF01' OR ct.co_atc = 'B01AF02' OR ct.co_atc = 'B01AF03' OR ct.co_atc = 'B01AA03')",
                _ => "ct.co_atc = 'B01AA03'"
            };

            var query = $@"
                SELECT DISTINCT
                    p.cognome,
                    p.nome,
                    p.nascita,
                    p.codice_fiscale,
                    nos.tel_cell,
                    nos.email,
                    CASE
                    WHEN ct.co_atc IS NULL THEN 'non specificato'
                    WHEN ct.co_atc = 'B01AE07' THEN 'dabigatran'
                    WHEN ct.co_atc = 'B01AF01' THEN 'rivaroxaban'
                    WHEN ct.co_atc = 'B01AF02' THEN 'apixaban'
                    WHEN ct.co_atc = 'B01AF03' THEN 'edoxaban'
                    WHEN ct.co_atc = 'B01AA03' THEN 'warfarin'
                    ELSE 'altro'
                    END AS anticoagulante,
                    ct.data_open AS data_inizio,
                    p.sesso,
                    p.codice AS codice_millewin
                FROM pazienti p, cart_pazpbl cp, nos_002 nos, cart_terap ct
                WHERE p.codice = nos.codice
                AND p.codice = cp.codice
                AND p.codice = ct.codice
                AND p.pa_convenzione = 'S'
                AND p.decesso IS NULL
                AND (nos.pa_drevoca IS NULL OR nos.pa_drevoca > CURRENT_DATE)
                AND nos.pa_medi = (SELECT userid FROM users u2 WHERE ut_cf = @CodiceFiscaleMedico)
                AND cp.cp_code = 'V58.61'
                AND {atcCondition}
                AND ct.te_c_flag = 'C'";

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@CodiceFiscaleMedico", codiceFiscaleMedico);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var patient = new MillepsPatientDto
                {
                    Cognome = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                    Nome = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Nascita = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                    CodiceFiscale = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    TelCell = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Anticoagulante = reader.IsDBNull(6) ? null : reader.GetString(6),
                    DataInizio = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    Sesso = reader.IsDBNull(8) ? null : reader.GetString(8),
                    MillewinCode = reader.IsDBNull(9) ? null : reader.GetString(9)
                };

                patients.Add(patient);
            }

            _logger.LogInformation("Importati {Count} pazienti dal database Milleps", patients.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'importazione dei pazienti da PostgreSQL");
            throw;
        }

        return patients;
    }

    /// <summary>
    /// Testa la connessione al database PostgreSQL Milleps
    /// </summary>
    /// <returns>True se la connessione ha successo, False altrimenti</returns>
    public async Task<bool> TestConnectionAsync()
    {
        var connectionString = "Host=127.0.0.1;Port=5432;Database=milleps;Username=dba;Password=sql;";

        try
        {
            _logger.LogInformation("Test connessione al database PostgreSQL Milleps...");

            await using var connection = new NpgsqlConnection(connectionString);
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
}
