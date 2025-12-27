namespace WarfarinManager.Shared.Models;

/// <summary>
/// Dati biometrici estratti dal database Milleps (tabella cart_accert)
/// </summary>
public record MillepsBiometricData(
    decimal? Weight,           // kg
    decimal? Height,           // cm
    DateTime? WeightDate,      // Data rilevazione peso
    DateTime? HeightDate       // Data rilevazione altezza
)
{
    /// <summary>
    /// Calcola il BMI se peso e altezza sono disponibili
    /// </summary>
    public decimal? BMI => (Weight.HasValue && Height.HasValue && Height.Value > 0)
        ? Math.Round(Weight.Value / ((Height.Value / 100m) * (Height.Value / 100m)), 1)
        : null;
}

/// <summary>
/// Risultato singolo esame di laboratorio da Milleps
/// </summary>
public record MillepsLabResult(
    string ExamName,           // AC_DES - nome esame originale
    string RawValue,           // AC_VAL - valore grezzo (può contenere unità)
    DateTime ExamDate          // data_open
)
{
    /// <summary>
    /// Tipo di esame normalizzato
    /// </summary>
    public LabExamType? ExamType { get; init; }

    /// <summary>
    /// Valore numerico parsato (null se non parsabile)
    /// </summary>
    public decimal? NumericValue { get; init; }

    /// <summary>
    /// Unità di misura estratta o predefinita
    /// </summary>
    public string? Unit { get; init; }
}

/// <summary>
/// Tipi di esami di laboratorio supportati
/// </summary>
public enum LabExamType
{
    Creatinine,      // Creatinina (mg/dL)
    Hemoglobin,      // Emoglobina (g/dL)
    Platelets,       // Piastrine (/μL o x10^3/μL)
    AST,             // Aspartato aminotransferasi (U/L)
    ALT,             // Alanina aminotransferasi (U/L)
    INR,             // International Normalized Ratio
    Weight,          // Peso corporeo (kg)
    Height           // Altezza/Statura (cm)
}

/// <summary>
/// Raccolta di esami di laboratorio recenti per un paziente
/// </summary>
public class MillepsLabResultsCollection
{
    public decimal? Creatinine { get; set; }
    public DateTime? CreatinineDate { get; set; }

    public decimal? Hemoglobin { get; set; }
    public DateTime? HemoglobinDate { get; set; }

    public int? Platelets { get; set; }
    public DateTime? PlateletsDate { get; set; }

    public int? AST { get; set; }
    public DateTime? ASTDate { get; set; }

    public int? ALT { get; set; }
    public DateTime? ALTDate { get; set; }

    public decimal? INR { get; set; }
    public DateTime? INRDate { get; set; }

    /// <summary>
    /// Data più recente tra tutti gli esami
    /// </summary>
    public DateTime? MostRecentDate => new[] { CreatinineDate, HemoglobinDate, PlateletsDate, ASTDate, ALTDate, INRDate }
        .Where(d => d.HasValue)
        .OrderByDescending(d => d)
        .FirstOrDefault();

    /// <summary>
    /// Indica se ci sono esami disponibili
    /// </summary>
    public bool HasAnyResults => Creatinine.HasValue || Hemoglobin.HasValue || Platelets.HasValue ||
                                  AST.HasValue || ALT.HasValue || INR.HasValue;

    // Flags per valori anomali
    public bool IsHemoglobinLow(bool isMale) => Hemoglobin.HasValue && Hemoglobin.Value < (isMale ? 13.0m : 12.0m);
    public bool IsPlateletsLow => Platelets.HasValue && Platelets.Value < 100000;
    public bool IsASTHigh => AST.HasValue && AST.Value > 120; // >3x UNL (40)
    public bool IsALTHigh => ALT.HasValue && ALT.Value > 120; // >3x UNL (40)
    public bool IsCreatinineHigh => Creatinine.HasValue && Creatinine.Value > 1.5m;
}

/// <summary>
/// Farmaco in terapia continuativa estratto da Milleps
/// </summary>
public record MillepsMedication(
    string AtcCode,            // co_atc - codice ATC
    string DrugName,           // te_des - nome farmaco
    string? Dosage,            // dosaggio (se disponibile)
    DateTime? StartDate        // data_open - inizio terapia
);

/// <summary>
/// Dati completi del paziente estratti da Milleps usando i codici medico/paziente
/// </summary>
public class MillepsPatientData
{
    /// <summary>
    /// Codice fiscale del paziente (CFpazi)
    /// </summary>
    public string CodiceFiscalePaziente { get; set; } = string.Empty;

    /// <summary>
    /// Codice interno Milleps del paziente (campo 'codice' in pazienti)
    /// </summary>
    public string? CodiceMillepsPaziente { get; set; }

    /// <summary>
    /// Codice medico Milleps (pa_medi) - userid dalla tabella users
    /// </summary>
    public string? CodiceMillepsMedico { get; set; }

    /// <summary>
    /// Dati biometrici (peso, altezza)
    /// </summary>
    public MillepsBiometricData? BiometricData { get; set; }

    /// <summary>
    /// Esami di laboratorio
    /// </summary>
    public MillepsLabResultsCollection LabResults { get; set; } = new();

    /// <summary>
    /// Indica se i dati sono stati recuperati correttamente
    /// </summary>
    public bool IsValid => !string.IsNullOrEmpty(CodiceMillepsPaziente);
}

/// <summary>
/// Configurazione dei pattern per il matching dei nomi esami in Milleps
/// Pattern basati sui dati reali estratti dal database milleps
/// </summary>
public static class MillepsExamPatterns
{
    // Pattern esatti trovati in milleps (case-insensitive)
    // Fonte: SELECT DISTINCT UPPER(AC_DES) FROM cart_accert

    // CREATININA
    public static readonly string[] CreatininePatterns = new[]
    {
        "CREATININA"
    };

    // HGB EMOGLOBINA
    public static readonly string[] HemoglobinPatterns = new[]
    {
        "HGB EMOGLOBINA", "EMOGLOBINA", "HGB"
    };

    // PLT PIASTRINE
    public static readonly string[] PlateletsPatterns = new[]
    {
        "PLT PIASTRINE", "PIASTRINE", "PLT"
    };

    // ASPARTATO AMINOTRANSFERASI (AST) (GOT)
    public static readonly string[] ASTPatterns = new[]
    {
        "ASPARTATO AMINOTRANSFERASI", "AST", "GOT"
    };

    // ALANINA AMINOTRANSFERASI (ALT) (GPT)
    public static readonly string[] ALTPatterns = new[]
    {
        "ALANINA AMINOTRANSFERASI", "ALT", "GPT"
    };

    // INR
    public static readonly string[] INRPatterns = new[]
    {
        "INR"
    };

    // PESO
    public static readonly string[] WeightPatterns = new[]
    {
        "PESO"
    };

    // ALTEZZA, STATURA
    public static readonly string[] HeightPatterns = new[]
    {
        "ALTEZZA", "STATURA"
    };

    // BMI<BODY MASS INDEX> - per riferimento, non usato direttamente
    public static readonly string[] BMIPatterns = new[]
    {
        "BMI"
    };

    /// <summary>
    /// Determina il tipo di esame basandosi sul nome (AC_DES)
    /// </summary>
    public static LabExamType? GetExamType(string examName)
    {
        if (string.IsNullOrWhiteSpace(examName))
            return null;

        var upper = examName.ToUpperInvariant().Trim();

        // Match esatto o contenuto per pattern specifici di Milleps
        if (upper == "CREATININA")
            return LabExamType.Creatinine;

        if (upper.StartsWith("HGB") || upper.Contains("EMOGLOBINA"))
            return LabExamType.Hemoglobin;

        if (upper.StartsWith("PLT") || upper.Contains("PIASTRINE"))
            return LabExamType.Platelets;

        // AST: "ASPARTATO AMINOTRANSFERASI (AST) (GOT)"
        if (upper.Contains("ASPARTATO AMINOTRANSFERASI") ||
            (upper.Contains("AST") && !upper.Contains("CONTRAST")) ||
            (upper.Contains("GOT") && upper.Contains("AST")))
            return LabExamType.AST;

        // ALT: "ALANINA AMINOTRANSFERASI (ALT) (GPT)"
        if (upper.Contains("ALANINA AMINOTRANSFERASI") ||
            (upper.Contains("ALT") && !upper.Contains("SALT")) ||
            (upper.Contains("GPT") && upper.Contains("ALT")))
            return LabExamType.ALT;

        if (upper == "INR" || upper.StartsWith("INR "))
            return LabExamType.INR;

        if (upper == "PESO" || upper.StartsWith("PESO "))
            return LabExamType.Weight;

        if (upper == "ALTEZZA" || upper.Contains("STATURA"))
            return LabExamType.Height;

        return null;
    }
}
