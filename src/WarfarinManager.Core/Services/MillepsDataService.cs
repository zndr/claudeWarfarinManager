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

    #region SQL Query Snippets

    /// <summary>
    /// Join primario tra pazienti e nos_002 per associazione medico-paziente.
    /// Deve essere seguito da PaziIdCondition per completare le clausole obbligatorie.
    /// </summary>
    private const string JoinPrimario = @"
        FROM pazienti p, nos_002 nos
        WHERE p.codice = nos.codice";

    /// <summary>
    /// Condizioni obbligatorie per identificare pazienti validi del medico:
    /// - In convenzione SSN (pa_convenzione = 'S')
    /// - Non deceduto (decesso IS NULL)
    /// - Non revocato o revoca futura (pa_drevoca)
    /// - Appartenente al medico specificato (@pa_medi)
    /// </summary>
    private const string PaziIdCondition = @"
        AND p.pa_convenzione = 'S'
        AND p.decesso IS NULL
        AND (nos.pa_drevoca IS NULL OR nos.pa_drevoca > CURRENT_DATE)
        AND nos.pa_medi = @pa_medi";

    #endregion

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

            // Step 1: Query per terapie continuative attive con codice ATC
            var medicationsQuery = @"
                SELECT DISTINCT
                    ct.co_atc,
                    ct.co_des,
                    ct.data_open
                FROM cart_terap ct
                WHERE ct.codice = @PatientCode
                  AND ct.te_c_flag = 'C'
                  AND ct.co_atc IS NOT NULL
                  AND ct.co_atc <> ''
                ORDER BY ct.co_des";

            var tempMedications = new List<(string AtcCode, string DrugName, DateTime? StartDate)>();

            await using (var cmd = new NpgsqlCommand(medicationsQuery, connection))
            {
                cmd.Parameters.AddWithValue("@PatientCode", patientCode);

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var atcCode = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                    var drugName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    var startDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2);

                    if (!string.IsNullOrEmpty(atcCode))
                    {
                        tempMedications.Add((atcCode, drugName, startDate));
                    }
                }
            }

            _logger.LogInformation("Trovate {Count} terapie continuative, recupero principi attivi...",
                tempMedications.Count);

            // Step 2: Per ogni codice ATC, recupera il principio attivo dalla tabella tab_atc
            foreach (var med in tempMedications)
            {
                string? activeIngredient = await GetActiveIngredientAsync(connection, med.AtcCode);
                medications.Add(new MillepsMedication(med.AtcCode, med.DrugName, activeIngredient, med.StartDate));
            }

            _logger.LogInformation("Recuperate {Count} terapie continuative con principi attivi per paziente {PatientCode}",
                medications.Count, patientCode);

            return medications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero delle terapie per CF: {CodiceFiscale}", codiceFiscale);
            throw;
        }
    }

    /// <summary>
    /// Recupera il principio attivo (atc_des) per un dato codice ATC
    /// </summary>
    private async Task<string?> GetActiveIngredientAsync(NpgsqlConnection connection, string atcCode)
    {
        try
        {
            var query = @"
                SELECT DISTINCT a.atc_des
                FROM mn_v_prodotti m, tab_atc a
                WHERE a.atc_cod = m.codice_atc
                  AND m.codice_atc = @AtcCode
                LIMIT 1";

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@AtcCode", atcCode);

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossibile recuperare principio attivo per ATC: {AtcCode}", atcCode);
            return null;
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

    /// <inheritdoc />
    public async Task<string?> GetMillepsDoctorCodeAsync(string codiceFiscaleMedico)
    {
        if (string.IsNullOrWhiteSpace(codiceFiscaleMedico))
            return null;

        try
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            // Query per ottenere l'userid del medico dalla tabella users
            var query = "SELECT userid FROM users WHERE UPPER(ut_cf) = UPPER(@CodiceFiscaleMedico) LIMIT 1";

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@CodiceFiscaleMedico", codiceFiscaleMedico);

            var result = await cmd.ExecuteScalarAsync();
            var doctorCode = result?.ToString();

            if (!string.IsNullOrEmpty(doctorCode))
            {
                _logger.LogInformation("Trovato codice medico Milleps (pa_medi): {DoctorCode} per CF: {CF}",
                    doctorCode, codiceFiscaleMedico);
            }
            else
            {
                _logger.LogWarning("Medico non trovato in Milleps per CF: {CodiceFiscaleMedico}", codiceFiscaleMedico);
            }

            return doctorCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero del codice medico Milleps per CF: {CodiceFiscaleMedico}",
                codiceFiscaleMedico);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<MillepsPatientData?> GetPatientDataAsync(string codiceFiscalePaziente, string codiceFiscaleMedico)
    {
        if (string.IsNullOrWhiteSpace(codiceFiscalePaziente))
            return null;

        var result = new MillepsPatientData
        {
            CodiceFiscalePaziente = codiceFiscalePaziente
        };

        try
        {
            // Step 1: Ottieni il codice medico Milleps (pa_medi) - serve per la query paziente
            if (!string.IsNullOrWhiteSpace(codiceFiscaleMedico))
            {
                result.CodiceMillepsMedico = await GetMillepsDoctorCodeAsync(codiceFiscaleMedico);
            }

            if (string.IsNullOrEmpty(result.CodiceMillepsMedico))
            {
                _logger.LogWarning("Impossibile recuperare pa_medi per CF medico: {CF}", codiceFiscaleMedico);
                return result;
            }

            // Step 2: Ottieni il codice paziente Milleps usando pa_medi e CF paziente
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            // Query con join su nos_002 per verificare che il paziente appartenga al medico
            // Usa JoinPrimario + PaziIdCondition per le clausole obbligatorie
            var patientQuery = $@"
                SELECT p.codice
                {JoinPrimario}
                {PaziIdCondition}
                  AND UPPER(p.codice_fiscale) = UPPER(@CFpazi)
                LIMIT 1";

            await using var patientCmd = new NpgsqlCommand(patientQuery, connection);
            patientCmd.Parameters.AddWithValue("@pa_medi", result.CodiceMillepsMedico);
            patientCmd.Parameters.AddWithValue("@CFpazi", codiceFiscalePaziente);

            var patientResult = await patientCmd.ExecuteScalarAsync();
            result.CodiceMillepsPaziente = patientResult?.ToString();

            if (string.IsNullOrEmpty(result.CodiceMillepsPaziente))
            {
                _logger.LogWarning(
                    "Paziente non trovato in Milleps per CF: {CodiceFiscale} con medico pa_medi: {PaMedi}",
                    codiceFiscalePaziente, result.CodiceMillepsMedico);
                return result;
            }

            _logger.LogInformation(
                "Recupero dati paziente Milleps - Paziente: {PatientCode}, Medico: {DoctorCode}",
                result.CodiceMillepsPaziente,
                result.CodiceMillepsMedico);

            // Step 3: Query per dati antropometrici e di laboratorio
            // Riutilizza la connessione già aperta
            // Costruisce una query che usa entrambi i codici per maggiore precisione
            var query = @"
                SELECT ac_des, ac_val, data_open
                FROM cart_accert
                WHERE codice = @PatientCode
                  AND (
                    -- Dati biometrici
                    UPPER(ac_des) LIKE '%PESO%'
                    OR UPPER(ac_des) LIKE '%ALTEZZA%'
                    OR UPPER(ac_des) LIKE '%STATURA%'
                    -- Esami di laboratorio
                    OR UPPER(ac_des) = 'CREATININA'
                    OR UPPER(ac_des) LIKE 'HGB EMOGLO%'
                    OR UPPER(ac_des) = 'PLT PIASTRINE'
                    OR UPPER(ac_des) LIKE 'ASPARTATO AMINO%'
                    OR UPPER(ac_des) LIKE 'ALANINA AMINO%'
                    OR UPPER(ac_des) = 'INR'
                  )
                ORDER BY data_open DESC";

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@PatientCode", result.CodiceMillepsPaziente);

            // Variabili per dati biometrici
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

                if (!examType.HasValue || !numericValue.HasValue)
                    continue;

                switch (examType.Value)
                {
                    // Dati biometrici
                    case LabExamType.Weight when !weight.HasValue:
                        weight = numericValue;
                        weightDate = examDate;
                        break;

                    case LabExamType.Height when !height.HasValue:
                        height = numericValue;
                        heightDate = examDate;
                        break;

                    // Esami di laboratorio
                    case LabExamType.Creatinine when !result.LabResults.Creatinine.HasValue:
                        result.LabResults.Creatinine = numericValue;
                        result.LabResults.CreatinineDate = examDate;
                        break;

                    case LabExamType.Hemoglobin when !result.LabResults.Hemoglobin.HasValue:
                        result.LabResults.Hemoglobin = numericValue;
                        result.LabResults.HemoglobinDate = examDate;
                        break;

                    case LabExamType.Platelets when !result.LabResults.Platelets.HasValue:
                        var platelets = numericValue.Value;
                        if (platelets < 1000) platelets *= 1000;
                        result.LabResults.Platelets = (int)platelets;
                        result.LabResults.PlateletsDate = examDate;
                        break;

                    case LabExamType.AST when !result.LabResults.AST.HasValue:
                        result.LabResults.AST = (int)numericValue.Value;
                        result.LabResults.ASTDate = examDate;
                        break;

                    case LabExamType.ALT when !result.LabResults.ALT.HasValue:
                        result.LabResults.ALT = (int)numericValue.Value;
                        result.LabResults.ALTDate = examDate;
                        break;

                    case LabExamType.INR when !result.LabResults.INR.HasValue:
                        result.LabResults.INR = numericValue;
                        result.LabResults.INRDate = examDate;
                        break;
                }
            }

            // Imposta i dati biometrici se trovati
            if (weight.HasValue || height.HasValue)
            {
                result.BiometricData = new MillepsBiometricData(weight, height, weightDate, heightDate);
            }

            _logger.LogInformation(
                "Dati paziente Milleps recuperati - Peso: {Weight}, Altezza: {Height}, Creat: {Creat}, Hb: {Hb}",
                weight, height, result.LabResults.Creatinine, result.LabResults.Hemoglobin);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero dei dati paziente Milleps per CF: {CodiceFiscale}",
                codiceFiscalePaziente);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidatePatientAsync(string millewinPatientCode, string millewinDoctorCode)
    {
        if (string.IsNullOrWhiteSpace(millewinPatientCode) || string.IsNullOrWhiteSpace(millewinDoctorCode))
            return false;

        try
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            // Verifica che il paziente esista ed sia valido per il medico
            // Usa JoinPrimario + PaziIdCondition per le clausole obbligatorie
            var query = $@"
                SELECT 1
                {JoinPrimario}
                {PaziIdCondition}
                  AND p.codice = @patientCode
                LIMIT 1";

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@pa_medi", millewinDoctorCode);
            cmd.Parameters.AddWithValue("@patientCode", millewinPatientCode);

            var result = await cmd.ExecuteScalarAsync();
            var isValid = result != null;

            _logger.LogInformation(
                "Validazione paziente Millewin: {PatientCode} per medico {DoctorCode} = {IsValid}",
                millewinPatientCode, millewinDoctorCode, isValid);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la validazione del paziente Millewin: {PatientCode}",
                millewinPatientCode);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<MillepsPatientData?> GetPatientDataByCodeAsync(string millewinPatientCode, string millewinDoctorCode)
    {
        if (string.IsNullOrWhiteSpace(millewinPatientCode) || string.IsNullOrWhiteSpace(millewinDoctorCode))
            return null;

        var result = new MillepsPatientData
        {
            CodiceMillepsPaziente = millewinPatientCode,
            CodiceMillepsMedico = millewinDoctorCode
        };

        try
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            // Prima valida che il paziente sia associato al medico
            var isValid = await ValidatePatientAsync(millewinPatientCode, millewinDoctorCode);
            if (!isValid)
            {
                _logger.LogWarning(
                    "Paziente {PatientCode} non valido o non associato al medico {DoctorCode}",
                    millewinPatientCode, millewinDoctorCode);
                return null;
            }

            _logger.LogInformation(
                "Recupero dati paziente Milleps tramite codice diretto - Paziente: {PatientCode}, Medico: {DoctorCode}",
                millewinPatientCode, millewinDoctorCode);

            // Query per dati antropometrici e di laboratorio
            var query = @"
                SELECT ac_des, ac_val, data_open
                FROM cart_accert
                WHERE codice = @PatientCode
                  AND (
                    -- Dati biometrici
                    UPPER(ac_des) LIKE '%PESO%'
                    OR UPPER(ac_des) LIKE '%ALTEZZA%'
                    OR UPPER(ac_des) LIKE '%STATURA%'
                    -- Esami di laboratorio
                    OR UPPER(ac_des) = 'CREATININA'
                    OR UPPER(ac_des) LIKE 'HGB EMOGLO%'
                    OR UPPER(ac_des) = 'PLT PIASTRINE'
                    OR UPPER(ac_des) LIKE 'ASPARTATO AMINO%'
                    OR UPPER(ac_des) LIKE 'ALANINA AMINO%'
                    OR UPPER(ac_des) = 'INR'
                  )
                ORDER BY data_open DESC";

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@PatientCode", millewinPatientCode);

            // Variabili per dati biometrici
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

                if (!examType.HasValue || !numericValue.HasValue)
                    continue;

                switch (examType.Value)
                {
                    // Dati biometrici
                    case LabExamType.Weight when !weight.HasValue:
                        weight = numericValue;
                        weightDate = examDate;
                        break;

                    case LabExamType.Height when !height.HasValue:
                        height = numericValue;
                        heightDate = examDate;
                        break;

                    // Esami di laboratorio
                    case LabExamType.Creatinine when !result.LabResults.Creatinine.HasValue:
                        result.LabResults.Creatinine = numericValue;
                        result.LabResults.CreatinineDate = examDate;
                        break;

                    case LabExamType.Hemoglobin when !result.LabResults.Hemoglobin.HasValue:
                        result.LabResults.Hemoglobin = numericValue;
                        result.LabResults.HemoglobinDate = examDate;
                        break;

                    case LabExamType.Platelets when !result.LabResults.Platelets.HasValue:
                        var platelets = numericValue.Value;
                        if (platelets < 1000) platelets *= 1000;
                        result.LabResults.Platelets = (int)platelets;
                        result.LabResults.PlateletsDate = examDate;
                        break;

                    case LabExamType.AST when !result.LabResults.AST.HasValue:
                        result.LabResults.AST = (int)numericValue.Value;
                        result.LabResults.ASTDate = examDate;
                        break;

                    case LabExamType.ALT when !result.LabResults.ALT.HasValue:
                        result.LabResults.ALT = (int)numericValue.Value;
                        result.LabResults.ALTDate = examDate;
                        break;

                    case LabExamType.INR when !result.LabResults.INR.HasValue:
                        result.LabResults.INR = numericValue;
                        result.LabResults.INRDate = examDate;
                        break;
                }
            }

            // Imposta i dati biometrici se trovati
            if (weight.HasValue || height.HasValue)
            {
                result.BiometricData = new MillepsBiometricData(weight, height, weightDate, heightDate);
            }

            _logger.LogInformation(
                "Dati paziente Milleps recuperati (via codice) - Peso: {Weight}, Altezza: {Height}, Creat: {Creat}, Hb: {Hb}",
                weight, height, result.LabResults.Creatinine, result.LabResults.Hemoglobin);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero dati paziente Milleps per codice: {PatientCode}",
                millewinPatientCode);
            throw;
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
