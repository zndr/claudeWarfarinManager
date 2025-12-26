using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Data.Services;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per la gestione completa del modulo DoacGest
/// </summary>
public partial class DoacGestViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDialogService _dialogService;
    private readonly ILogger<DoacGestViewModel> _logger;

    [ObservableProperty]
    private int _patientId;

    [ObservableProperty]
    private Patient? _patient;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    // ====== RILEVAZIONE CORRENTE ======

    [ObservableProperty]
    private DoacMonitoringRecord _currentRecord = new();

    [ObservableProperty]
    private bool _isEditMode;

    // ====== STORICO RILEVAZIONI ======

    [ObservableProperty]
    private ObservableCollection<DoacMonitoringRecord> _storico = new();

    [ObservableProperty]
    private DoacMonitoringRecord? _selectedRecord;

    // ====== PARAMETRI LABORATORIO ======

    [ObservableProperty]
    private decimal? _peso;

    [ObservableProperty]
    private decimal? _creatinina;

    [ObservableProperty]
    private decimal? _emoglobina;

    [ObservableProperty]
    private decimal? _ematocrito;

    [ObservableProperty]
    private int? _piastrine;

    [ObservableProperty]
    private int? _ast;

    [ObservableProperty]
    private int? _alt;

    [ObservableProperty]
    private decimal? _bilirubina;

    [ObservableProperty]
    private decimal? _ggt;

    // ====== CLEARANCE CREATININA ======

    [ObservableProperty]
    private int? _crClCockroft;

    [ObservableProperty]
    private bool _crClCalcolato = true;

    /// <summary>
    /// Ricalcola automaticamente CrCl quando cambia il Peso
    /// </summary>
    partial void OnPesoChanged(decimal? value)
    {
        RicalcolaCrClSeNecessario();
    }

    /// <summary>
    /// Ricalcola automaticamente CrCl quando cambia la Creatinina
    /// </summary>
    partial void OnCreatininaChanged(decimal? value)
    {
        RicalcolaCrClSeNecessario();
    }

    /// <summary>
    /// Ricalcola la Clearance della Creatinina se tutti i dati sono disponibili
    /// </summary>
    private void RicalcolaCrClSeNecessario()
    {
        // Calcola solo se abbiamo tutti i dati necessari
        if (Patient != null && Peso.HasValue && Peso.Value > 0 && Creatinina.HasValue && Creatinina.Value > 0)
        {
            var crCl = DoacCalculationService.CalcolaCockcroftGault(Patient.Age, Peso, Creatinina, Patient.Gender);
            if (crCl.HasValue)
            {
                CrClCockroft = crCl.Value;
                CrClCalcolato = true;
            }
        }
    }

    // ====== DOAC E INDICAZIONE ======

    [ObservableProperty]
    private string _doacSelezionato = "Apixaban";

    [ObservableProperty]
    private string _indicazione = "Fibrillazione Atriale Non Valvolare";

    public List<string> DoacOptions { get; } = new()
    {
        "Apixaban",
        "Rivaroxaban",
        "Edoxaban",
        "Dabigatran"
    };

    public List<string> IndicazioniOptions { get; } = new()
    {
        "Fibrillazione Atriale Non Valvolare",
        "Trattamento TEV (Trombosi Venosa Profonda / Embolia Polmonare)",
        "Prevenzione Secondaria TEV a lungo termine",
        "Altro"
    };

    // ====== FATTORI RISCHIO HAS-BLED ======

    [ObservableProperty]
    private bool _ipertensione;

    [ObservableProperty]
    private bool _disfunzioneRenale;

    [ObservableProperty]
    private bool _disfunzioneEpatica;

    [ObservableProperty]
    private bool _cirrosi;

    [ObservableProperty]
    private bool _ipertensPortale;

    [ObservableProperty]
    private bool _storiaStroke;

    [ObservableProperty]
    private bool _storiaSanguinamento;

    [ObservableProperty]
    private bool _inrLabile;

    [ObservableProperty]
    private bool _antiaggreganti;

    [ObservableProperty]
    private bool _fans;

    [ObservableProperty]
    private bool _abusoDiAlcol;

    // ====== SCORE E DOSAGGIO ======

    [ObservableProperty]
    private int _hasBledScore;

    [ObservableProperty]
    private string? _dosaggioSuggerito;

    [ObservableProperty]
    private string? _razionaleDosaggio;

    [ObservableProperty]
    private string? _rischioEmorragico;

    [ObservableProperty]
    private List<string> _alertClinici = new();

    // ====== PROGRAMMAZIONE CONTROLLI ======

    [ObservableProperty]
    private int _intervalloControlloMesi = 6;

    [ObservableProperty]
    private DateTime? _dataProssimoControllo;

    public List<int> IntervalliControlloOptions { get; } = new() { 3, 6, 12 };

    public DoacGestViewModel(
        IUnitOfWork unitOfWork,
        IDialogService dialogService,
        ILogger<DoacGestViewModel> logger)
    {
        _unitOfWork = unitOfWork;
        _dialogService = dialogService;
        _logger = logger;

        CurrentRecord = new DoacMonitoringRecord
        {
            DataRilevazione = DateTime.Now
        };
    }

    /// <summary>
    /// Inizializza il ViewModel con il paziente specificato
    /// </summary>
    public async Task InitializeAsync(int patientId)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            PatientId = patientId;

            // Carica paziente
            Patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
            if (Patient == null)
            {
                ErrorMessage = "Paziente non trovato";
                return;
            }

            // Carica storico rilevazioni
            var records = await _unitOfWork.DoacMonitoring.GetAllByPatientIdAsync(patientId);
            Storico = new ObservableCollection<DoacMonitoringRecord>(records);

            // Carica ultima rilevazione se esiste
            var ultimaRilevazione = await _unitOfWork.DoacMonitoring.GetUltimaRilevazioneAsync(patientId);
            if (ultimaRilevazione != null)
            {
                LoadRecordIntoForm(ultimaRilevazione);
            }
            else
            {
                // Inizializza con dati paziente
                InitializeFromPatient();
            }

            // Rileva farmaci a rischio
            await RilevaDosaggioFarmaciRischioAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'inizializzazione DoacGest per paziente {PatientId}", patientId);
            ErrorMessage = $"Errore: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Inizializza form con dati del paziente
    /// </summary>
    private void InitializeFromPatient()
    {
        if (Patient == null) return;

        // Precompila fattori di rischio da anagrafica paziente
        Ipertensione = Patient.HasHypertension;
        DisfunzioneRenale = Patient.HasRenalDisease;
        DisfunzioneEpatica = Patient.HasLiverDisease;
        StoriaStroke = Patient.HasStroke;
        StoriaSanguinamento = Patient.HasBleedingHistory;
        InrLabile = Patient.HasLabileINR;
        AbusoDiAlcol = Patient.UsesAlcohol;

        // Se il paziente ha già un DOAC in corso, preimpostalo
        if (!string.IsNullOrEmpty(Patient.AnticoagulantType))
        {
            var doacType = Patient.AnticoagulantType.ToLower();
            if (doacType.Contains("apixaban")) DoacSelezionato = "Apixaban";
            else if (doacType.Contains("rivaroxaban")) DoacSelezionato = "Rivaroxaban";
            else if (doacType.Contains("edoxaban")) DoacSelezionato = "Edoxaban";
            else if (doacType.Contains("dabigatran")) DoacSelezionato = "Dabigatran";
        }
    }

    /// <summary>
    /// Carica un record storico nel form
    /// </summary>
    private void LoadRecordIntoForm(DoacMonitoringRecord record)
    {
        Peso = record.Peso;
        Creatinina = record.Creatinina;
        Emoglobina = record.Emoglobina;
        Ematocrito = record.Ematocrito;
        Piastrine = record.Piastrine;
        Ast = record.AST;
        Alt = record.ALT;
        Bilirubina = record.Bilirubina;
        Ggt = record.GGT;

        CrClCockroft = record.CrCl_Cockroft;
        CrClCalcolato = record.CrCl_Calcolato;

        DoacSelezionato = record.DoacSelezionato ?? "Apixaban";
        Indicazione = record.Indicazione ?? "Fibrillazione Atriale Non Valvolare";

        Ipertensione = record.Ipertensione;
        DisfunzioneRenale = record.DisfunzioneRenale;
        DisfunzioneEpatica = record.DisfunzioneEpatica;
        Cirrosi = record.Cirrosi;
        IpertensPortale = record.IpertensPortale;
        StoriaStroke = record.StoriaStroke;
        StoriaSanguinamento = record.StoriaSanguinamento;
        InrLabile = record.INRLabile;
        Antiaggreganti = record.Antiaggreganti;
        Fans = record.FANS;
        AbusoDiAlcol = record.AbusoDiAlcol;

        HasBledScore = record.HasBledScore;
        DosaggioSuggerito = record.DosaggioSuggerito;
        RazionaleDosaggio = record.RazionaleDosaggio;

        IntervalloControlloMesi = record.IntervalloControlloMesi ?? 6;
        DataProssimoControllo = record.DataProssimoControllo;
    }

    /// <summary>
    /// Rileva automaticamente uso di antiaggreganti e FANS dal database
    /// </summary>
    private async Task RilevaDosaggioFarmaciRischioAsync()
    {
        try
        {
            var terapieAttive = await _unitOfWork.TerapieContinuative.GetTerapieAttiveAsync(PatientId);

            Antiaggreganti = terapieAttive.Any(t => t.IsAntiaggregante);
            Fans = terapieAttive.Any(t => t.IsFANS);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Errore durante il rilevamento farmaci a rischio");
        }
    }

    /// <summary>
    /// Calcola automaticamente la Clearance della Creatinina (Cockcroft-Gault)
    /// </summary>
    [RelayCommand]
    private void CalcolaClearanceCreatinina()
    {
        if (Patient == null || !Peso.HasValue || !Creatinina.HasValue)
        {
            _dialogService.ShowWarning("Inserire Peso e Creatinina per calcolare la CrCl");
            return;
        }

        var crCl = DoacCalculationService.CalcolaCockcroftGault(Patient.Age, Peso, Creatinina, Patient.Gender);

        if (crCl.HasValue)
        {
            CrClCockroft = crCl.Value;
            CrClCalcolato = true;
            _dialogService.ShowInformation($"Clearance Creatinina calcolata: {crCl.Value} mL/min", "CrCl Calcolata");
        }
        else
        {
            _dialogService.ShowWarning("Impossibile calcolare CrCl. Verifica i dati inseriti.");
        }
    }

    /// <summary>
    /// Calcola HAS-BLED score e determina dosaggio DOAC
    /// </summary>
    [RelayCommand]
    private async Task CalcolaDosaggioAsync()
    {
        if (Patient == null)
        {
            _dialogService.ShowWarning("Paziente non caricato");
            return;
        }

        try
        {
            // Crea record temporaneo per calcoli
            var tempRecord = BuildRecordFromForm();

            // Calcola HAS-BLED
            HasBledScore = DoacCalculationService.CalcolaHasBledScore(tempRecord, Patient);
            RischioEmorragico = DoacCalculationService.GetRischioEmorragico(HasBledScore);

            // Determina dosaggio
            var dosaggio = DoacCalculationService.DeterminaDosaggio(
                tempRecord,
                Patient,
                DoacSelezionato,
                Indicazione
            );

            DosaggioSuggerito = dosaggio.Dose;
            RazionaleDosaggio = dosaggio.RazionaleCompleto;

            // Genera alert clinici
            AlertClinici = DoacCalculationService.GeneraAlert(tempRecord, Patient);

            _dialogService.ShowInformation($"HAS-BLED Score: {HasBledScore}\nRischio: {RischioEmorragico}\nDosaggio: {DosaggioSuggerito}", "Calcolo Dosaggio");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il calcolo del dosaggio");
            _dialogService.ShowError($"Errore: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida i campi obbligatori prima del salvataggio
    /// </summary>
    private bool ValidaCampiObbligatori()
    {
        var campiMancanti = new List<string>();

        if (!Creatinina.HasValue || Creatinina.Value <= 0)
            campiMancanti.Add("Creatinina");

        if (!Emoglobina.HasValue || Emoglobina.Value <= 0)
            campiMancanti.Add("Emoglobina");

        if (!Ast.HasValue || Ast.Value <= 0)
            campiMancanti.Add("AST");

        if (!Alt.HasValue || Alt.Value <= 0)
            campiMancanti.Add("ALT");

        if (campiMancanti.Count > 0)
        {
            _dialogService.ShowWarning(
                $"Compilare i seguenti campi obbligatori:\n• {string.Join("\n• ", campiMancanti)}",
                "Campi Obbligatori Mancanti"
            );
            return false;
        }

        return true;
    }

    /// <summary>
    /// Salva la rilevazione corrente
    /// </summary>
    [RelayCommand]
    private async Task SalvaRilevazioneAsync()
    {
        if (Patient == null)
        {
            _dialogService.ShowWarning("Paziente non caricato");
            return;
        }

        // Validazione campi obbligatori
        if (!ValidaCampiObbligatori())
            return;

        try
        {
            IsLoading = true;

            // Calcola automaticamente CrCl se non presente e abbiamo i dati necessari
            if (!CrClCockroft.HasValue && Peso.HasValue && Creatinina.HasValue)
            {
                var crCl = DoacCalculationService.CalcolaCockcroftGault(Patient.Age, Peso, Creatinina, Patient.Gender);
                if (crCl.HasValue)
                {
                    CrClCockroft = crCl.Value;
                    CrClCalcolato = true;
                }
            }

            // Costruisci record da form
            var record = BuildRecordFromForm();
            record.PatientId = PatientId;
            record.Patient = Patient;

            // Salva con calcoli automatici
            var savedRecord = await _unitOfWork.DoacMonitoring.CreateWithCalculationsAsync(record);

            // Ricarica storico
            var records = await _unitOfWork.DoacMonitoring.GetAllByPatientIdAsync(PatientId);
            Storico = new ObservableCollection<DoacMonitoringRecord>(records);

            _dialogService.ShowInformation("Rilevazione salvata con successo", "Salvataggio Completato");

            // Reset form per nuova rilevazione
            ResetForm();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il salvataggio della rilevazione");
            _dialogService.ShowError($"Errore durante il salvataggio: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Costruisce un DoacMonitoringRecord dai campi del form
    /// </summary>
    private DoacMonitoringRecord BuildRecordFromForm()
    {
        return new DoacMonitoringRecord
        {
            DataRilevazione = DateTime.Now,

            // Parametri laboratorio
            Peso = Peso,
            Creatinina = Creatinina,
            Emoglobina = Emoglobina,
            Ematocrito = Ematocrito,
            Piastrine = Piastrine,
            AST = Ast,
            ALT = Alt,
            Bilirubina = Bilirubina,
            GGT = Ggt,

            // Clearance creatinina
            CrCl_Cockroft = CrClCockroft,
            CrCl_Calcolato = CrClCalcolato,

            // DOAC e indicazione
            DoacSelezionato = DoacSelezionato,
            Indicazione = Indicazione,

            // Fattori rischio HAS-BLED
            Ipertensione = Ipertensione,
            DisfunzioneRenale = DisfunzioneRenale,
            DisfunzioneEpatica = DisfunzioneEpatica,
            Cirrosi = Cirrosi,
            IpertensPortale = IpertensPortale,
            StoriaStroke = StoriaStroke,
            StoriaSanguinamento = StoriaSanguinamento,
            INRLabile = InrLabile,
            Antiaggreganti = Antiaggreganti,
            FANS = Fans,
            AbusoDiAlcol = AbusoDiAlcol,

            // Programmazione controlli
            IntervalloControlloMesi = IntervalloControlloMesi
        };
    }

    /// <summary>
    /// Reset del form per nuova rilevazione
    /// </summary>
    private void ResetForm()
    {
        CurrentRecord = new DoacMonitoringRecord
        {
            DataRilevazione = DateTime.Now
        };

        // Mantieni solo i dati stabili del paziente
        InitializeFromPatient();

        // Reset parametri laboratorio
        Peso = null;
        Creatinina = null;
        Emoglobina = null;
        Ematocrito = null;
        Piastrine = null;
        Ast = null;
        Alt = null;
        Bilirubina = null;
        Ggt = null;

        CrClCockroft = null;
        CrClCalcolato = true;

        HasBledScore = 0;
        DosaggioSuggerito = null;
        RazionaleDosaggio = null;
        RischioEmorragico = null;
        AlertClinici = new();

        IntervalloControlloMesi = 6;
        DataProssimoControllo = null;
    }

    /// <summary>
    /// Visualizza dettagli di una rilevazione storica
    /// </summary>
    [RelayCommand]
    private void MostraDettaglioStorico(DoacMonitoringRecord? record)
    {
        if (record == null) return;

        LoadRecordIntoForm(record);
        IsEditMode = true;
    }

    /// <summary>
    /// Elimina una rilevazione storica
    /// </summary>
    [RelayCommand]
    private async Task EliminaRilevazioneAsync(DoacMonitoringRecord? record)
    {
        if (record == null) return;

        var conferma = _dialogService.ShowConfirmation(
            $"Eliminare la rilevazione del {record.DataRilevazione:dd/MM/yyyy}?",
            "Conferma eliminazione"
        );

        if (!conferma) return;

        try
        {
            IsLoading = true;

            await _unitOfWork.DoacMonitoring.DeleteAsync(record);
            await _unitOfWork.SaveChangesAsync();

            Storico.Remove(record);

            _dialogService.ShowInformation("Rilevazione eliminata", "Eliminazione Completata");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'eliminazione della rilevazione");
            _dialogService.ShowError($"Errore: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Esporta rilevazione in PDF
    /// </summary>
    [RelayCommand]
    private void EsportaPdf()
    {
        _dialogService.ShowInformation("Funzionalità di esportazione PDF in fase di sviluppo", "Export PDF");
    }

    /// <summary>
    /// Stampa rilevazione
    /// </summary>
    [RelayCommand]
    private void Stampa()
    {
        _dialogService.ShowInformation("Funzionalità di stampa in fase di sviluppo", "Stampa");
    }
}
