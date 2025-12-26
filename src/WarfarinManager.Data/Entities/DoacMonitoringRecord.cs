using System.ComponentModel.DataAnnotations.Schema;

namespace WarfarinManager.Data.Entities;

/// <summary>
/// Record di monitoraggio DOAC per un paziente
/// Ogni rilevazione è un nuovo record (storico temporale)
/// </summary>
public class DoacMonitoringRecord : BaseEntity
{
    /// <summary>
    /// ID del paziente (FK)
    /// </summary>
    public int PatientId { get; set; }

    /// <summary>
    /// Data e ora della rilevazione
    /// </summary>
    public DateTime DataRilevazione { get; set; } = DateTime.Now;

    // ====== PARAMETRI LABORATORIO ======

    /// <summary>
    /// Peso corporeo in kg (1 decimale)
    /// </summary>
    public decimal? Peso { get; set; }

    /// <summary>
    /// Creatinina sierica in mg/dL (1 decimale)
    /// </summary>
    public decimal? Creatinina { get; set; }

    /// <summary>
    /// Emoglobina in g/dL (1 decimale)
    /// </summary>
    public decimal? Emoglobina { get; set; }

    /// <summary>
    /// Ematocrito in % (1 decimale)
    /// </summary>
    public decimal? Ematocrito { get; set; }

    /// <summary>
    /// Piastrine (/μL) - intero
    /// </summary>
    public int? Piastrine { get; set; }

    /// <summary>
    /// AST (Aspartato aminotransferasi) in U/L - intero
    /// </summary>
    public int? AST { get; set; }

    /// <summary>
    /// ALT (Alanina aminotransferasi) in U/L - intero
    /// </summary>
    public int? ALT { get; set; }

    /// <summary>
    /// Bilirubina totale in mg/dL (1 decimale)
    /// </summary>
    public decimal? Bilirubina { get; set; }

    /// <summary>
    /// Gamma-GT in U/L (1 decimale)
    /// </summary>
    public decimal? GGT { get; set; }

    // ====== CALCOLO CLEARANCE CREATININA ======

    /// <summary>
    /// Clearance della creatinina calcolata con Cockcroft-Gault (mL/min)
    /// </summary>
    public int? CrCl_Cockroft { get; set; }

    /// <summary>
    /// Se true, CrCl viene calcolato automaticamente; se false, è inserito manualmente
    /// </summary>
    public bool CrCl_Calcolato { get; set; } = true;

    // ====== SELEZIONE DOAC E INDICAZIONE ======

    /// <summary>
    /// DOAC selezionato (Apixaban, Rivaroxaban, Edoxaban, Dabigatran)
    /// </summary>
    public string? DoacSelezionato { get; set; }

    /// <summary>
    /// Indicazione per la terapia anticoagulante
    /// </summary>
    public string? Indicazione { get; set; }

    // ====== FATTORI DI RISCHIO HAS-BLED ======

    /// <summary>
    /// H: Ipertensione arteriosa non controllata (PAS >160 mmHg)
    /// </summary>
    public bool Ipertensione { get; set; }

    /// <summary>
    /// A: Disfunzione renale (dialisi, trapianto, Cr >200 μmol/L o CrCl <30 mL/min)
    /// </summary>
    public bool DisfunzioneRenale { get; set; }

    /// <summary>
    /// A: Disfunzione epatica (cirrosi, bilirubina >2x, AST/ALT/FA >3x)
    /// </summary>
    public bool DisfunzioneEpatica { get; set; }

    /// <summary>
    /// A: Cirrosi epatica documentata
    /// </summary>
    public bool Cirrosi { get; set; }

    /// <summary>
    /// A: Ipertensione portale (varici esofagee)
    /// </summary>
    public bool IpertensPortale { get; set; }

    /// <summary>
    /// S: Storia di stroke o TIA
    /// </summary>
    public bool StoriaStroke { get; set; }

    /// <summary>
    /// B: Storia di sanguinamento maggiore o predisposizione
    /// </summary>
    public bool StoriaSanguinamento { get; set; }

    /// <summary>
    /// L: INR labile (TTR <60%) - applicabile se switch da warfarin
    /// </summary>
    public bool INRLabile { get; set; }

    /// <summary>
    /// E: Età >65 anni (calcolato automaticamente)
    /// </summary>
    [NotMapped]
    public bool EtaSuperiore65 => Patient != null && Patient.Age > 65;

    /// <summary>
    /// D: Uso di antiaggreganti piastrinici
    /// </summary>
    public bool Antiaggreganti { get; set; }

    /// <summary>
    /// D: Uso di FANS
    /// </summary>
    public bool FANS { get; set; }

    /// <summary>
    /// D: Abuso di alcol (≥8 unità/settimana)
    /// </summary>
    public bool AbusoDiAlcol { get; set; }

    // ====== SCORE E DOSAGGIO CALCOLATI ======

    /// <summary>
    /// Punteggio HAS-BLED calcolato (0-9)
    /// </summary>
    public int HasBledScore { get; set; }

    /// <summary>
    /// Dosaggio DOAC suggerito dall'algoritmo
    /// </summary>
    public string? DosaggioSuggerito { get; set; }

    /// <summary>
    /// Razionale completo della scelta del dosaggio
    /// </summary>
    public string? RazionaleDosaggio { get; set; }

    // ====== PROGRAMMAZIONE CONTROLLI ======

    /// <summary>
    /// Intervallo controllo in mesi (3, 6, 12)
    /// </summary>
    public int? IntervalloControlloMesi { get; set; }

    /// <summary>
    /// Data prevista per il prossimo controllo
    /// </summary>
    public DateTime? DataProssimoControllo { get; set; }

    // ====== NAVIGATION PROPERTIES ======

    /// <summary>
    /// Paziente di riferimento
    /// </summary>
    public Patient? Patient { get; set; }

    // ====== COMPUTED PROPERTIES ======

    /// <summary>
    /// Livello rischio emorragico basato su HAS-BLED
    /// </summary>
    [NotMapped]
    public string RischioEmorragico
    {
        get
        {
            if (HasBledScore >= 5) return "Alto";
            if (HasBledScore >= 3) return "Moderato";
            return "Basso";
        }
    }

    /// <summary>
    /// Verifica se emoglobina è sotto soglia critica
    /// </summary>
    [NotMapped]
    public bool EmoglobinaAbnorme
    {
        get
        {
            if (!Emoglobina.HasValue || Patient?.Gender == null) return false;

            // Soglie differenziate per sesso
            if (Patient.Gender == Shared.Enums.Gender.Male)
                return Emoglobina.Value < 13.0m;
            else
                return Emoglobina.Value < 12.0m;
        }
    }

    /// <summary>
    /// Verifica piastrinopenia (<100.000/μL)
    /// </summary>
    [NotMapped]
    public bool Piastrinopenia => Piastrine.HasValue && Piastrine.Value < 100000;

    /// <summary>
    /// Verifica transaminasi elevate (>3x upper normal limit)
    /// </summary>
    [NotMapped]
    public bool TransaminasiElevate => (AST.HasValue && AST.Value > 120) || (ALT.HasValue && ALT.Value > 120);

    // ====== METODI ======

    /// <summary>
    /// Calcola automaticamente la data del prossimo controllo
    /// </summary>
    public void CalcolaDataProssimoControllo()
    {
        if (IntervalloControlloMesi.HasValue && IntervalloControlloMesi.Value > 0)
        {
            DataProssimoControllo = DataRilevazione.AddMonths(IntervalloControlloMesi.Value);
        }
    }
}
