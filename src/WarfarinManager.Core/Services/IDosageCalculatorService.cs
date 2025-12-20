using WarfarinManager.Core.Models;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per il calcolo del dosaggio warfarin secondo linee guida FCSA-SIMG e ACCP
/// </summary>
public interface IDosageCalculatorService
{
    /// <summary>
    /// Calcola suggerimento dosaggio secondo linee guida FCSA-SIMG (Italia)
    /// </summary>
    /// <param name="currentINR">Valore INR attuale misurato</param>
    /// <param name="targetINRMin">Limite inferiore range terapeutico</param>
    /// <param name="targetINRMax">Limite superiore range terapeutico</param>
    /// <param name="currentWeeklyDoseMg">Dose settimanale corrente (mg)</param>
    /// <param name="phase">Fase della terapia (Induction/Maintenance/PostAdjustment)</param>
    /// <param name="isCompliant">Flag compliance paziente</param>
    /// <param name="isSlowMetabolizer">Flag metabolizzatore lento (<15mg/settimana)</param>
    /// <param name="thromboembolicRisk">Rischio tromboembolico paziente</param>
    /// <param name="tipoEmorragia">Tipo di emorragia presente (se applicabile)</param>
    /// <param name="sedeEmorragia">Sede anatomica emorragia</param>
    /// <param name="hasProtesiMeccanica">Paziente con protesi valvolare meccanica</param>
    /// <param name="dataUltimoTEV">Data ultimo evento tromboembolico venoso</param>
    /// <param name="indicazioneTAO">Indicazione alla TAO (es. FA, TEV)</param>
    /// <param name="cha2ds2vasc">Score CHA2DS2-VASc (per FA)</param>
    DosageSuggestionResult CalculateFCSA(
        decimal currentINR,
        decimal targetINRMin,
        decimal targetINRMax,
        decimal currentWeeklyDoseMg,
        TherapyPhase phase,
        bool isCompliant,
        bool isSlowMetabolizer,
        ThromboembolicRisk thromboembolicRisk = ThromboembolicRisk.Moderate,
        TipoEmorragia tipoEmorragia = TipoEmorragia.Nessuna,
        SedeEmorragia sedeEmorragia = SedeEmorragia.Nessuna,
        bool hasProtesiMeccanica = false,
        DateTime? dataUltimoTEV = null,
        string indicazioneTAO = "",
        int cha2ds2vasc = 0);

    /// <summary>
    /// Calcola suggerimento dosaggio secondo linee guida ACCP/ACC (USA)
    /// </summary>
    /// <param name="currentINR">Valore INR attuale misurato</param>
    /// <param name="targetINRMin">Limite inferiore range terapeutico</param>
    /// <param name="targetINRMax">Limite superiore range terapeutico</param>
    /// <param name="currentWeeklyDoseMg">Dose settimanale corrente (mg)</param>
    /// <param name="phase">Fase della terapia (Induction/Maintenance/PostAdjustment)</param>
    /// <param name="isCompliant">Flag compliance paziente</param>
    /// <param name="isSlowMetabolizer">Flag metabolizzatore lento (<15mg/settimana)</param>
    /// <param name="thromboembolicRisk">Rischio tromboembolico paziente</param>
    /// <param name="tipoEmorragia">Tipo di emorragia presente (se applicabile)</param>
    /// <param name="sedeEmorragia">Sede anatomica emorragia</param>
    /// <param name="hasProtesiMeccanica">Paziente con protesi valvolare meccanica</param>
    /// <param name="dataUltimoTEV">Data ultimo evento tromboembolico venoso</param>
    /// <param name="indicazioneTAO">Indicazione alla TAO (es. FA, TEV)</param>
    /// <param name="cha2ds2vasc">Score CHA2DS2-VASc (per FA)</param>
    DosageSuggestionResult CalculateACCP(
        decimal currentINR,
        decimal targetINRMin,
        decimal targetINRMax,
        decimal currentWeeklyDoseMg,
        TherapyPhase phase,
        bool isCompliant,
        bool isSlowMetabolizer,
        ThromboembolicRisk thromboembolicRisk = ThromboembolicRisk.Moderate,
        TipoEmorragia tipoEmorragia = TipoEmorragia.Nessuna,
        SedeEmorragia sedeEmorragia = SedeEmorragia.Nessuna,
        bool hasProtesiMeccanica = false,
        DateTime? dataUltimoTEV = null,
        string indicazioneTAO = "",
        int cha2ds2vasc = 0);

    /// <summary>
    /// Genera schema posologico settimanale ottimizzato da dose totale
    /// Preferisce compresse intere (5mg) e mezze (2.5mg), evita 1/4 o 3/4
    /// </summary>
    /// <param name="weeklyDoseMg">Dose settimanale totale (mg)</param>
    /// <param name="loadingDoseDay1">Dose supplementare da aggiungere al primo giorno (opzionale)</param>
    WeeklyDoseSchedule GenerateWeeklySchedule(decimal weeklyDoseMg, decimal? loadingDoseDay1 = null);

    /// <summary>
    /// Valuta se è necessario bridge con EBPM
    /// </summary>
    bool RequiresEBPM(decimal inr, ThromboembolicRisk risk, GuidelineType guideline);

    /// <summary>
    /// Valuta se è necessaria somministrazione Vitamina K
    /// </summary>
    VitaminKRecommendation EvaluateVitaminK(decimal inr, GuidelineType guideline, bool hasBleeding = false);
}

/// <summary>
/// Raccomandazione per uso Vitamina K
/// </summary>
public class VitaminKRecommendation
{
    public bool IsRecommended { get; set; }
    public decimal? DoseMg { get; set; }
    public string Route { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Urgency { get; set; } = string.Empty;
}
