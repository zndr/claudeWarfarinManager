using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WarfarinManager.Core.Services;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Data.Services;
using WarfarinManager.Shared.Enums;
using WarfarinManager.Shared.Models;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// Rappresenta un farmaco inserito dall'utente per la verifica delle interazioni
/// </summary>
public partial class DrugEntryViewModel : ObservableObject
{
    /// <summary>
    /// Nome del farmaco inserito
    /// </summary>
    [ObservableProperty]
    private string _drugName = string.Empty;

    /// <summary>
    /// Indica se il farmaco è stato mappato a una categoria di interazione nota
    /// </summary>
    [ObservableProperty]
    private bool _isMapped;

    /// <summary>
    /// Categoria di interazione se mappato
    /// </summary>
    [ObservableProperty]
    private string? _mappedCategory;

    /// <summary>
    /// Livello di interazione se trovato
    /// </summary>
    [ObservableProperty]
    private DOACInteractionLevel? _interactionLevel;

    /// <summary>
    /// Dettagli interazione se presente
    /// </summary>
    [ObservableProperty]
    private DOACInteraction? _interaction;
}

/// <summary>
/// ViewModel per la gestione completa del modulo DoacGest
/// </summary>
public partial class DoacGestViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDialogService _dialogService;
    private readonly ILogger<DoacGestViewModel> _logger;
    private readonly IDOACInteractionService _interactionService;

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

    /// <summary>
    /// Ricalcola il HAS-BLED Score in tempo reale quando cambiano i checkbox
    /// </summary>
    private void RicalcolaHasBledScoreInTempoReale()
    {
        if (Patient == null) return;

        // Crea un record temporaneo con i valori correnti del form
        var tempRecord = BuildRecordFromForm();

        // Calcola HAS-BLED score
        HasBledScore = DoacCalculationService.CalcolaHasBledScore(tempRecord, Patient);
        RischioEmorragico = DoacCalculationService.GetRischioEmorragico(HasBledScore);
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

    partial void OnIpertensioneChanged(bool value) => RicalcolaHasBledScoreInTempoReale();

    [ObservableProperty]
    private bool _disfunzioneRenale;

    partial void OnDisfunzioneRenaleChanged(bool value) => RicalcolaHasBledScoreInTempoReale();

    [ObservableProperty]
    private bool _disfunzioneEpatica;

    partial void OnDisfunzioneEpaticaChanged(bool value) => RicalcolaHasBledScoreInTempoReale();

    [ObservableProperty]
    private bool _cirrosi;

    partial void OnCirrosiChanged(bool value) => RicalcolaHasBledScoreInTempoReale();

    [ObservableProperty]
    private bool _ipertensPortale;

    partial void OnIpertensPortaleChanged(bool value) => RicalcolaHasBledScoreInTempoReale();

    [ObservableProperty]
    private bool _storiaStroke;

    partial void OnStoriaStrokeChanged(bool value) => RicalcolaHasBledScoreInTempoReale();

    [ObservableProperty]
    private bool _storiaSanguinamento;

    partial void OnStoriaSanguinamentoChanged(bool value) => RicalcolaHasBledScoreInTempoReale();

    [ObservableProperty]
    private bool _inrLabile;

    partial void OnInrLabileChanged(bool value) => RicalcolaHasBledScoreInTempoReale();

    [ObservableProperty]
    private bool _antiaggreganti;

    partial void OnAntiaggregantiChanged(bool value) => RicalcolaHasBledScoreInTempoReale();

    [ObservableProperty]
    private bool _fans;

    partial void OnFansChanged(bool value) => RicalcolaHasBledScoreInTempoReale();

    [ObservableProperty]
    private bool _abusoDiAlcol;

    partial void OnAbusoDiAlcolChanged(bool value) => RicalcolaHasBledScoreInTempoReale();

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

    [ObservableProperty]
    private string? _motivazioneIntervallo;

    [ObservableProperty]
    private List<string> _motivazioniIntervallo = new();

    public List<int> IntervalliControlloOptions { get; } = new() { 3, 6, 12 };

    // ====== VALUTAZIONE APPROPRIATEZZA DOAC ======

    [ObservableProperty]
    private bool _isDoacAppropriato = true;

    [ObservableProperty]
    private bool _isDoacContraindicato;

    [ObservableProperty]
    private bool _isDoacSconsigliato;

    [ObservableProperty]
    private string? _motivoInappropriatezza;

    [ObservableProperty]
    private string? _doacConsigliato;

    [ObservableProperty]
    private string? _motivoConsiglioCambio;

    [ObservableProperty]
    private string? _raccomandazioneDoac;

    [ObservableProperty]
    private List<DoacAlternativo> _doacAlternativi = new();

    [ObservableProperty]
    private bool _suggerisciSwitchWarfarin;

    [ObservableProperty]
    private string? _motivoSwitchWarfarin;

    // ====== INTERAZIONI FARMACOLOGICHE ======

    /// <summary>
    /// Testo del farmaco da aggiungere
    /// </summary>
    [ObservableProperty]
    private string _nuovoFarmaco = string.Empty;

    /// <summary>
    /// Lista dei farmaci inseriti dall'utente per verifica interazioni
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DrugEntryViewModel> _farmaciInseriti = new();

    /// <summary>
    /// Lista delle interazioni rilevate
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DOACInteraction> _interazioniRilevate = new();

    /// <summary>
    /// Numero di interazioni controindicanti
    /// </summary>
    [ObservableProperty]
    private int _countContraindicazioni;

    /// <summary>
    /// Numero di interazioni pericolose
    /// </summary>
    [ObservableProperty]
    private int _countPericolose;

    /// <summary>
    /// Numero di interazioni moderate
    /// </summary>
    [ObservableProperty]
    private int _countModerate;

    /// <summary>
    /// Numero di interazioni minori
    /// </summary>
    [ObservableProperty]
    private int _countMinori;

    /// <summary>
    /// Indica se ci sono farmaci non mappati (non riconosciuti)
    /// </summary>
    [ObservableProperty]
    private bool _hasFarmaciNonMappati;

    /// <summary>
    /// Suggerimenti farmaci per autocompletamento
    /// </summary>
    public List<string> SuggerimentiFarmaci { get; } = new()
    {
        // Azoli antifungini
        "Ketoconazolo", "Itraconazolo", "Voriconazolo", "Posaconazolo", "Fluconazolo",
        // Inibitori proteasi HIV
        "Ritonavir", "Cobicistat",
        // Induttori
        "Rifampicina", "Carbamazepina", "Fenitoina", "Fenobarbital", "Erba di San Giovanni", "Iperico",
        // Antiaritmici
        "Dronedarone", "Amiodarone", "Chinidina", "Verapamil", "Diltiazem",
        // Immunosoppressori
        "Ciclosporina", "Tacrolimus",
        // Macrolidi
        "Claritromicina", "Eritromicina",
        // Antipiastrinici
        "Acido acetilsalicilico", "ASA", "Aspirina", "Clopidogrel", "Ticagrelor", "Prasugrel",
        // FANS
        "Ibuprofene", "Naprossene", "Diclofenac", "Ketoprofene", "Indometacina", "Celecoxib", "Etoricoxib",
        // Antidepressivi
        "Sertralina", "Paroxetina", "Fluoxetina", "Citalopram", "Escitalopram", "Venlafaxina", "Duloxetina",
        // HCV
        "Glecaprevir", "Pibrentasvir",
        // Altri
        "Digossina", "Pantoprazolo", "Omeprazolo"
    };

    public DoacGestViewModel(
        IUnitOfWork unitOfWork,
        IDialogService dialogService,
        ILogger<DoacGestViewModel> logger,
        IDOACInteractionService interactionService)
    {
        _unitOfWork = unitOfWork;
        _dialogService = dialogService;
        _logger = logger;
        _interactionService = interactionService;

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
    /// Calcola HAS-BLED score, determina dosaggio DOAC, valuta appropriatezza e intervallo controllo
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

            // Crea record temporaneo per calcoli
            var tempRecord = BuildRecordFromForm();

            // 1. Calcola HAS-BLED
            HasBledScore = DoacCalculationService.CalcolaHasBledScore(tempRecord, Patient);
            RischioEmorragico = DoacCalculationService.GetRischioEmorragico(HasBledScore);

            // 2. Valuta appropriatezza del DOAC attuale
            var valutazione = DoacCalculationService.ValutaAppropriatezzaDoac(tempRecord, Patient, DoacSelezionato);
            IsDoacAppropriato = valutazione.IsAppropriato;
            IsDoacContraindicato = valutazione.IsContraindicato;
            IsDoacSconsigliato = valutazione.IsSconsigliato;
            MotivoInappropriatezza = valutazione.MotivoInappropriatezza;
            RaccomandazioneDoac = valutazione.Raccomandazione;
            DoacConsigliato = valutazione.DoacConsigliato;
            MotivoConsiglioCambio = valutazione.MotivoConsiglio;
            DoacAlternativi = valutazione.DoacAlternativi;
            SuggerisciSwitchWarfarin = valutazione.SuggerisciSwitchWarfarin;
            MotivoSwitchWarfarin = valutazione.MotivoSwitchWarfarin;

            // 3. Determina dosaggio (se DOAC appropriato)
            var dosaggio = DoacCalculationService.DeterminaDosaggio(
                tempRecord,
                Patient,
                DoacSelezionato,
                Indicazione
            );

            DosaggioSuggerito = dosaggio.Dose;
            RazionaleDosaggio = dosaggio.RazionaleCompleto;

            // 4. Determina intervallo di controllo basato su HAS-BLED e altri fattori
            var controlloRaccomandato = DoacCalculationService.DeterminaIntervalloControllo(
                tempRecord,
                Patient,
                HasBledScore
            );

            IntervalloControlloMesi = controlloRaccomandato.IntervalloMesi;
            DataProssimoControllo = controlloRaccomandato.DataProssimoControllo;
            MotivazioneIntervallo = controlloRaccomandato.Descrizione;
            MotivazioniIntervallo = controlloRaccomandato.Motivazioni;

            // 5. Genera alert clinici
            AlertClinici = DoacCalculationService.GeneraAlert(tempRecord, Patient);

            // Mostra risultato
            var messaggioRisultato = $"HAS-BLED Score: {HasBledScore} ({RischioEmorragico} rischio)\n\n";

            if (!IsDoacAppropriato)
            {
                messaggioRisultato += $"ATTENZIONE: {RaccomandazioneDoac}\n";
                if (!string.IsNullOrEmpty(DoacConsigliato))
                {
                    messaggioRisultato += $"Alternativa consigliata: {DoacConsigliato}\n";
                }
                messaggioRisultato += "\n";
            }

            messaggioRisultato += $"Dosaggio: {DosaggioSuggerito}\n";
            messaggioRisultato += $"Prossimo controllo: {DataProssimoControllo:dd/MM/yyyy} ({IntervalloControlloMesi} mesi)";

            _dialogService.ShowInformation(messaggioRisultato, "Valutazione Completa");
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

        // Reset programmazione controlli
        IntervalloControlloMesi = 6;
        DataProssimoControllo = null;
        MotivazioneIntervallo = null;
        MotivazioniIntervallo = new();

        // Reset valutazione appropriatezza DOAC
        IsDoacAppropriato = true;
        IsDoacContraindicato = false;
        IsDoacSconsigliato = false;
        MotivoInappropriatezza = null;
        DoacConsigliato = null;
        MotivoConsiglioCambio = null;
        RaccomandazioneDoac = null;
        DoacAlternativi = new();
        SuggerisciSwitchWarfarin = false;
        MotivoSwitchWarfarin = null;
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

    // ====== METODI INTERAZIONI FARMACOLOGICHE ======

    /// <summary>
    /// Converte il nome del DOAC selezionato in enum DOACType
    /// </summary>
    private DOACType GetDoacType()
    {
        return DoacSelezionato?.ToLower() switch
        {
            "apixaban" => DOACType.Apixaban,
            "rivaroxaban" => DOACType.Rivaroxaban,
            "dabigatran" => DOACType.Dabigatran,
            "edoxaban" => DOACType.Edoxaban,
            _ => DOACType.Apixaban
        };
    }

    /// <summary>
    /// Aggiunge uno o più farmaci alla lista per verifica interazioni
    /// </summary>
    [RelayCommand]
    private void AggiungiFarmaco()
    {
        if (string.IsNullOrWhiteSpace(NuovoFarmaco))
            return;

        // Supporta input multipli separati da virgola, punto e virgola o a capo
        var farmaci = NuovoFarmaco
            .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .Where(f => !string.IsNullOrEmpty(f))
            .ToList();

        var doacType = GetDoacType();

        foreach (var farmaco in farmaci)
        {
            // Verifica se il farmaco è già presente
            if (FarmaciInseriti.Any(f => f.DrugName.Equals(farmaco, StringComparison.OrdinalIgnoreCase)))
                continue;

            var entry = new DrugEntryViewModel { DrugName = farmaco };

            // Cerca interazione nel servizio
            var interaction = _interactionService.CheckInteraction(doacType, farmaco);
            if (interaction != null)
            {
                entry.IsMapped = true;
                entry.MappedCategory = interaction.DrugClass;
                entry.InteractionLevel = interaction.InteractionLevel;
                entry.Interaction = interaction;
            }
            else
            {
                entry.IsMapped = false;
            }

            FarmaciInseriti.Add(entry);
        }

        NuovoFarmaco = string.Empty;
        AggiornaInterazioni();
    }

    /// <summary>
    /// Rimuove un farmaco dalla lista
    /// </summary>
    [RelayCommand]
    private void RimuoviFarmaco(DrugEntryViewModel? farmaco)
    {
        if (farmaco == null) return;

        FarmaciInseriti.Remove(farmaco);
        AggiornaInterazioni();
    }

    /// <summary>
    /// Svuota la lista dei farmaci inseriti
    /// </summary>
    [RelayCommand]
    private void SvuotaListaFarmaci()
    {
        FarmaciInseriti.Clear();
        AggiornaInterazioni();
    }

    /// <summary>
    /// Aggiorna la lista delle interazioni in base ai farmaci inseriti e al DOAC selezionato
    /// </summary>
    private void AggiornaInterazioni()
    {
        var doacType = GetDoacType();

        // Raccogli tutte le interazioni dai farmaci inseriti
        var interazioni = new List<DOACInteraction>();

        foreach (var farmaco in FarmaciInseriti)
        {
            // Aggiorna l'interazione per il DOAC corrente
            var interaction = _interactionService.CheckInteraction(doacType, farmaco.DrugName);
            if (interaction != null)
            {
                farmaco.IsMapped = true;
                farmaco.MappedCategory = interaction.DrugClass;
                farmaco.InteractionLevel = interaction.InteractionLevel;
                farmaco.Interaction = interaction;
                interazioni.Add(interaction);
            }
            else
            {
                farmaco.IsMapped = false;
                farmaco.MappedCategory = null;
                farmaco.InteractionLevel = null;
                farmaco.Interaction = null;
            }
        }

        // Ordina per gravità
        var interazioniOrdinate = interazioni
            .OrderByDescending(i => i.InteractionLevel)
            .ThenBy(i => i.DrugClass)
            .ToList();

        InterazioniRilevate = new ObservableCollection<DOACInteraction>(interazioniOrdinate);

        // Aggiorna contatori
        CountContraindicazioni = interazioni.Count(i => i.InteractionLevel == DOACInteractionLevel.Contraindicated);
        CountPericolose = interazioni.Count(i => i.InteractionLevel == DOACInteractionLevel.Dangerous);
        CountModerate = interazioni.Count(i => i.InteractionLevel == DOACInteractionLevel.Moderate);
        CountMinori = interazioni.Count(i => i.InteractionLevel == DOACInteractionLevel.Minor);

        // Verifica farmaci non mappati
        HasFarmaciNonMappati = FarmaciInseriti.Any(f => !f.IsMapped);
    }

    /// <summary>
    /// Quando cambia il DOAC selezionato, ricalcola le interazioni
    /// </summary>
    partial void OnDoacSelezionatoChanged(string value)
    {
        if (FarmaciInseriti.Count > 0)
        {
            AggiornaInterazioni();
        }
    }

    /// <summary>
    /// Carica i farmaci dalla terapia continuativa del paziente
    /// </summary>
    [RelayCommand]
    private async Task CaricaFarmaciDaTerapiaAsync()
    {
        if (Patient == null) return;

        try
        {
            var terapie = await _unitOfWork.TerapieContinuative.GetTerapieAttiveAsync(PatientId);

            foreach (var terapia in terapie)
            {
                if (!string.IsNullOrEmpty(terapia.PrincipioAttivo))
                {
                    // Aggiungi solo se non già presente
                    if (!FarmaciInseriti.Any(f => f.DrugName.Equals(terapia.PrincipioAttivo, StringComparison.OrdinalIgnoreCase)))
                    {
                        NuovoFarmaco = terapia.PrincipioAttivo;
                        AggiungiFarmaco();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Errore durante il caricamento farmaci dalla terapia");
        }
    }

    /// <summary>
    /// Ottiene la descrizione del livello di interazione
    /// </summary>
    public static string GetInteractionLevelDescription(DOACInteractionLevel level)
    {
        return level switch
        {
            DOACInteractionLevel.Contraindicated => "CONTROINDICATO",
            DOACInteractionLevel.Dangerous => "EVITARE / NON RACCOMANDATO",
            DOACInteractionLevel.Moderate => "CAUTELA",
            DOACInteractionLevel.Minor => "MONITORARE",
            DOACInteractionLevel.None => "NESSUNA INTERAZIONE",
            _ => level.ToString()
        };
    }

    /// <summary>
    /// Ottiene il colore del tag per il livello di interazione
    /// </summary>
    public static string GetInteractionLevelColor(DOACInteractionLevel level)
    {
        return level switch
        {
            DOACInteractionLevel.Contraindicated => "DangerBrush",
            DOACInteractionLevel.Dangerous => "WarningBrush",
            DOACInteractionLevel.Moderate => "InfoBrush",
            DOACInteractionLevel.Minor => "SuccessBrush",
            DOACInteractionLevel.None => "SuccessBrush",
            _ => "TextSecondaryBrush"
        };
    }
}
