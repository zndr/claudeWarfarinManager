using System;
using WarfarinManager.Shared.Constants;

namespace WarfarinManager.UI.Models;

/// <summary>
/// Data Transfer Object per visualizzazione paziente nella UI
/// </summary>
public class PatientDto
{
    public int Id { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string FullName => $"{LastName} {FirstName}";
    
    public DateTime BirthDate { get; set; }
    
    public int Age => CalculateAge();
    
    public string FiscalCode { get; set; } = string.Empty;
    
    public string? Gender { get; set; }
    
    public string? Phone { get; set; }
    
    public string? Email { get; set; }
    
    /// <summary>
    /// Indicazione terapeutica attiva
    /// </summary>
    public string? ActiveIndication { get; set; }
    
    /// <summary>
    /// Ultimo valore INR registrato
    /// </summary>
    public decimal? LastINR { get; set; }
    
    /// <summary>
    /// Data ultimo controllo INR
    /// </summary>
    public DateTime? LastINRDate { get; set; }
    
    /// <summary>
    /// Dosaggio settimanale corrente (mg) - dall'ultimo controllo
    /// </summary>
    public decimal? CurrentWeeklyDose { get; set; }
    
    /// <summary>
    /// Time in Therapeutic Range (percentuale)
    /// </summary>
    public decimal? TTRPercentage { get; set; }
    
    /// <summary>
    /// Data prossimo controllo previsto
    /// </summary>
    public DateTime? NextControlDate { get; set; }
    
    /// <summary>
    /// Flag metabolizzatore lento (dose < 15mg/sett)
    /// </summary>
    public bool IsSlowMetabolizer { get; set; }

    /// <summary>
    /// Tipo di anticoagulante in uso (es. "warfarin", "apixaban", ecc.)
    /// </summary>
    public string? AnticoagulantType { get; set; }

    /// <summary>
    /// Data di inizio della terapia anticoagulante
    /// </summary>
    public DateTime? TherapyStartDate { get; set; }
    
    /// <summary>
    /// Indica se il controllo è scaduto
    /// </summary>
    public bool IsControlOverdue => NextControlDate.HasValue && NextControlDate.Value < DateTime.Today;
    
    /// <summary>
    /// Indica se il controllo è prossimo (entro 3 giorni)
    /// </summary>
    public bool IsControlSoon => NextControlDate.HasValue && 
                                   NextControlDate.Value >= DateTime.Today && 
                                   NextControlDate.Value <= DateTime.Today.AddDays(3);
    
    /// <summary>
    /// Indica se il TTR è critico (<60%)
    /// </summary>
    public bool IsTTRCritical => TTRPercentage.HasValue && TTRPercentage.Value < 60;

    // === Computed Properties per Anticoagulanti ===

    /// <summary>
    /// Abbreviazione del tipo di anticoagulante (W/A/D/E/R/-)
    /// </summary>
    public string AnticoagulantAbbreviation =>
        AnticoagulantTypes.GetAbbreviation(AnticoagulantType);

    /// <summary>
    /// Nome completo del farmaco per visualizzazione (es. "Apixaban (Eliquis)")
    /// </summary>
    public string AnticoagulantDisplayName =>
        AnticoagulantTypes.GetDisplayName(AnticoagulantType);

    /// <summary>
    /// Indica se il paziente assume Warfarin
    /// </summary>
    public bool IsWarfarinPatient =>
        AnticoagulantTypes.IsWarfarin(AnticoagulantType);

    /// <summary>
    /// Indica se il paziente assume un DOAC (Direct Oral Anticoagulant)
    /// </summary>
    public bool IsDoacPatient =>
        AnticoagulantTypes.IsDoac(AnticoagulantType);

    // === Dati Biometrici (da Millewin) ===

    /// <summary>
    /// Peso corporeo in kg
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Altezza in cm
    /// </summary>
    public decimal? Height { get; set; }

    /// <summary>
    /// Data ultimo aggiornamento peso
    /// </summary>
    public DateTime? WeightLastUpdated { get; set; }

    /// <summary>
    /// Data ultimo aggiornamento altezza
    /// </summary>
    public DateTime? HeightLastUpdated { get; set; }

    /// <summary>
    /// Body Mass Index calcolato (kg/m²)
    /// </summary>
    public decimal? BMI => (Weight.HasValue && Height.HasValue && Height.Value > 0)
        ? Math.Round(Weight.Value / ((Height.Value / 100m) * (Height.Value / 100m)), 1)
        : null;

    /// <summary>
    /// Categoria BMI per visualizzazione
    /// </summary>
    public string? BMICategory => BMI switch
    {
        null => null,
        < 18.5m => "Sottopeso",
        < 25m => "Normopeso",
        < 30m => "Sovrappeso",
        _ => "Obesità"
    };

    /// <summary>
    /// Durata della terapia in mesi (arrotondata)
    /// </summary>
    public int? TherapyDurationMonths
    {
        get
        {
            if (!TherapyStartDate.HasValue)
                return null;

            var duration = DateTime.Today - TherapyStartDate.Value;
            return (int)Math.Round(duration.TotalDays / 30.0);
        }
    }
    
    private int CalculateAge()
    {
        var today = DateTime.Today;
        var age = today.Year - BirthDate.Year;
        
        // Sottrai 1 anno se il compleanno non è ancora arrivato quest'anno
        if (BirthDate.Date > today.AddYears(-age))
            age--;
        
        return age;
    }
}
