namespace WarfarinManager.Core.Models;

/// <summary>
/// Parametri del paziente per il calcolo dello switch
/// </summary>
public class SwitchPatientParameters
{
    /// <summary>
    /// INR attuale del paziente
    /// </summary>
    public decimal? CurrentINR { get; set; }

    /// <summary>
    /// Età del paziente in anni
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Peso del paziente in kg
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// Sesso del paziente (M/F) - necessario per Cockcroft-Gault
    /// </summary>
    public string Gender { get; set; } = string.Empty;

    /// <summary>
    /// Creatinina sierica in mg/dL
    /// </summary>
    public decimal? SerumCreatinine { get; set; }

    /// <summary>
    /// Clearance della creatinina in mL/min (calcolata o inserita manualmente)
    /// </summary>
    public decimal CreatinineClearance { get; set; }

    /// <summary>
    /// Indica se la clearance è stata calcolata automaticamente o inserita manualmente
    /// </summary>
    public bool IsCreatinineClearanceCalculated { get; set; }

    /// <summary>
    /// Presenza di valvole meccaniche (controindicazione DOAC)
    /// </summary>
    public bool HasMechanicalValves { get; set; }

    /// <summary>
    /// Presenza di stenosi mitralica moderata/severa (controindicazione DOAC)
    /// </summary>
    public bool HasMitralStenosis { get; set; }

    /// <summary>
    /// Sindrome da antifosfolipidi (controindicazione relativa DOAC)
    /// </summary>
    public bool HasAntiphospholipidSyndrome { get; set; }

    /// <summary>
    /// Gravidanza o allattamento (controindicazione assoluta)
    /// </summary>
    public bool IsPregnantOrBreastfeeding { get; set; }

    /// <summary>
    /// Calcola la Clearance della Creatinina usando la formula di Cockcroft-Gault
    /// </summary>
    /// <returns>ClCr in mL/min</returns>
    public decimal CalculateCreatinineClearance()
    {
        if (!SerumCreatinine.HasValue || SerumCreatinine.Value <= 0)
            return 0;

        // Formula Cockcroft-Gault:
        // ClCr (mL/min) = [(140 - età) × peso (kg) × (0.85 se femmina)] / (72 × creatinina sierica mg/dL)

        decimal factor = Gender.Equals("F", StringComparison.OrdinalIgnoreCase) ? 0.85m : 1.0m;
        decimal clcr = ((140 - Age) * Weight * factor) / (72 * SerumCreatinine.Value);

        return Math.Round(clcr, 1);
    }
}
