using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WarfarinManager.Core.Models;
using WarfarinManager.Core.Services;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Shared.Enums;
using WarfarinManager.UI.Helpers;
using WarfarinManager.UI.Models;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels
{
    /// <summary>
    /// ViewModel per la gestione dei controlli INR con calcolo automatico dosaggio
    /// </summary>
    public partial class INRControlViewModel : ObservableObject
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDosageCalculatorService _dosageCalculator;
        private readonly ITTRCalculatorService _ttrCalculator;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly WeeklySchedulePdfService _pdfService;
        private readonly Core.Services.PengoNomogramService _pengoNomogramService;

        #region Properties - Dati Paziente

        [ObservableProperty]
        private int _patientId;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private string _patientFiscalCode = string.Empty;

        [ObservableProperty]
        private string _activeIndication = string.Empty;

        [ObservableProperty]
        private decimal _targetINRMin;

        [ObservableProperty]
        private decimal _targetINRMax;

        [ObservableProperty]
        private bool _isSlowMetabolizer;

        [ObservableProperty]
        private bool _isNaivePatient;

        [ObservableProperty]
        private int _patientAge;

        [ObservableProperty]
        private int _patientHasBledScore;

        #endregion

        #region Properties - Form Inserimento

        [ObservableProperty]
        private DateTime _controlDate = DateTime.Today;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSelectBleeding))]
        private decimal _inrValue;

        /// <summary>
        /// Il campo emorragia è abilitato solo per INR > 3.2 (sovraterapeutico)
        /// </summary>
        public bool CanSelectBleeding => InrValue > 3.2m;

        [ObservableProperty]
        private bool _isCompliant = true;

        [ObservableProperty]
        private TherapyPhase _selectedPhase = TherapyPhase.Maintenance;

        [ObservableProperty]
        private string _notes = string.Empty;

        // Gestione emorragie
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasBleeding))]
        private TipoEmorragia _selectedBleedingType = TipoEmorragia.Nessuna;

        [ObservableProperty]
        private SedeEmorragia _selectedBleedingSite = SedeEmorragia.Nessuna;

        /// <summary>
        /// Indica se è presente un sanguinamento (visibilità condizionale UI)
        /// </summary>
        public bool HasBleeding => SelectedBleedingType != TipoEmorragia.Nessuna;

        /// <summary>
        /// Quando INR cambia, resetta l'emorragia se INR <= 3.2
        /// </summary>
        partial void OnInrValueChanged(decimal value)
        {
            if (value <= 3.2m && SelectedBleedingType != TipoEmorragia.Nessuna)
            {
                SelectedBleedingType = TipoEmorragia.Nessuna;
                SelectedBleedingSite = SedeEmorragia.Nessuna;
            }
        }

        // Dosi giornaliere usando DoseOption per visualizzare mg + compresse
        [ObservableProperty]
        private DoseOption _mondayDose;

        [ObservableProperty]
        private DoseOption _tuesdayDose;

        [ObservableProperty]
        private DoseOption _wednesdayDose;

        [ObservableProperty]
        private DoseOption _thursdayDose;

        [ObservableProperty]
        private DoseOption _fridayDose;

        [ObservableProperty]
        private DoseOption _saturdayDose;

        [ObservableProperty]
        private DoseOption _sundayDose;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentWeeklyTablets))]
        private decimal _currentWeeklyDose;

        /// <summary>
        /// Numero di compresse settimanali formattato (1 cp = 5 mg)
        /// </summary>
        public string CurrentWeeklyTablets => DoseDistributionHelper.FormatAsTablets(CurrentWeeklyDose);

        // Opzione esclusione quarti di compressa
        [ObservableProperty]
        private bool _excludeQuarterTablets = true; // Default: solo mezze cp (step 2.5 mg)

        // Lista valori dose disponibili (con visualizzazione mg + compresse)
        [ObservableProperty]
        private ObservableCollection<DoseOption> _availableDoses = new();

        // Flag per bloccare il ricalcolo durante l'applicazione dello schema
        private bool _isApplyingSchedule = false;

        // Flag per evitare doppia applicazione del nomogramma di Pengo
        private bool _pengoNomogramAlreadyApplied = false;

        #endregion

        #region Properties - Suggerimenti

        [ObservableProperty]
        private GuidelineType _selectedGuideline = GuidelineType.FCSA;

        [ObservableProperty]
        private DosageSuggestionResult? _fcsaSuggestion;

        [ObservableProperty]
        private DosageSuggestionResult? _accpSuggestion;

        [ObservableProperty]
        private DosageSuggestionResult? _activeSuggestion;

        [ObservableProperty]
        private bool _hasSuggestions;

        [ObservableProperty]
        private string _inrStatusText = string.Empty;

        [ObservableProperty]
        private string _inrStatusColor = "#666666";

        // Schema suggerito distribuito equilibratamente
        [ObservableProperty]
        private decimal[] _suggestedDistributedSchedule = new decimal[7];

        [ObservableProperty]
        private string _suggestedScheduleText = string.Empty;

        // Dosi giornaliere SUGGERITE (visualizzate nei dropdown a destra, read-only)
        [ObservableProperty]
        private DoseOption? _suggestedMondayDose;

        [ObservableProperty]
        private DoseOption? _suggestedTuesdayDose;

        [ObservableProperty]
        private DoseOption? _suggestedWednesdayDose;

        [ObservableProperty]
        private DoseOption? _suggestedThursdayDose;

        [ObservableProperty]
        private DoseOption? _suggestedFridayDose;

        [ObservableProperty]
        private DoseOption? _suggestedSaturdayDose;

        [ObservableProperty]
        private DoseOption? _suggestedSundayDose;

        #endregion

        #region Properties - Modalità Modifica Manuale

        /// <summary>
        /// Indica se la modalità modifica è attiva
        /// </summary>
        [ObservableProperty]
        private bool _isEditMode = false;

        /// <summary>
        /// Note cliniche modificabili (in modalità edit)
        /// </summary>
        [ObservableProperty]
        private string _editableClinicalNotes = string.Empty;

        /// <summary>
        /// Prossimo controllo modificabile (giorni)
        /// </summary>
        [ObservableProperty]
        private int _editableNextControlDays;

        /// <summary>
        /// Indica se il dosaggio è stato modificato manualmente
        /// </summary>
        [ObservableProperty]
        private bool _isManuallyModified = false;

        /// <summary>
        /// Indica se i warning delle linee guida devono essere ignorati
        /// </summary>
        [ObservableProperty]
        private bool _isWarningIgnored = false;

        /// <summary>
        /// Dose settimanale corrente calcolata dai dropdown (aggiornata in tempo reale)
        /// </summary>
        public decimal CurrentSuggestedWeeklyDose =>
            (SuggestedMondayDose?.DoseMg ?? 0) +
            (SuggestedTuesdayDose?.DoseMg ?? 0) +
            (SuggestedWednesdayDose?.DoseMg ?? 0) +
            (SuggestedThursdayDose?.DoseMg ?? 0) +
            (SuggestedFridayDose?.DoseMg ?? 0) +
            (SuggestedSaturdayDose?.DoseMg ?? 0) +
            (SuggestedSundayDose?.DoseMg ?? 0);

        /// <summary>
        /// Percentuale di aggiustamento corrente rispetto alla dose attuale
        /// </summary>
        public decimal CurrentPercentageAdjustment =>
            CurrentWeeklyDose > 0
                ? ((CurrentSuggestedWeeklyDose - CurrentWeeklyDose) / CurrentWeeklyDose * 100)
                : 0;

        /// <summary>
        /// Indica se è richiesta una sospensione di dosi
        /// null = sospendere fino a rientro, >0 = numero dosi da saltare
        /// </summary>
        public bool HasDoseSuspension => ActiveSuggestion?.SospensioneDosi == null || (ActiveSuggestion?.SospensioneDosi > 0);

        /// <summary>
        /// Testo dell'avviso di sospensione dose
        /// </summary>
        public string DoseSuspensionText
        {
            get
            {
                if (ActiveSuggestion == null)
                    return string.Empty;

                // null significa "sospendere fino a INR rientrato" (valore sentinella)
                if (ActiveSuggestion.SospensioneDosi == null)
                    return "⚠️ AZIONE IMMEDIATA: Sospendere warfarin fino a INR rientrato nel range";

                // 0 significa nessuna sospensione
                if (ActiveSuggestion.SospensioneDosi == 0)
                    return string.Empty;

                var dosi = ActiveSuggestion.SospensioneDosi.Value;
                return dosi == 1
                    ? "⚠️ AZIONE IMMEDIATA: Considerare saltare 1 dose"
                    : $"⚠️ AZIONE IMMEDIATA: Saltare {dosi} dosi";
            }
        }

        /// <summary>
        /// Indica se ci sono warning attivi (sospensione dose, dose carico, ecc.)
        /// </summary>
        public bool HasActiveWarnings
        {
            get
            {
                if (ActiveSuggestion == null) return false;

                // Verifica sospensione dosi
                bool hasSuspension = ActiveSuggestion.SospensioneDosi == null || ActiveSuggestion.SospensioneDosi > 0;

                // Verifica dose di carico
                bool hasLoadingDose = !string.IsNullOrEmpty(ActiveSuggestion.LoadingDoseAction);

                // Verifica altri warning
                bool hasOtherWarnings = ActiveSuggestion.Warnings?.Any() ?? false;

                return hasSuspension || hasLoadingDose || hasOtherWarnings;
            }
        }

        // Backup dei valori originali per annullamento
        private DosageSuggestionResult? _originalSuggestion;
        private decimal[]? _originalSuggestedSchedule;
        private DoseOption? _originalMondayDose;
        private DoseOption? _originalTuesdayDose;
        private DoseOption? _originalWednesdayDose;
        private DoseOption? _originalThursdayDose;
        private DoseOption? _originalFridayDose;
        private DoseOption? _originalSaturdayDose;
        private DoseOption? _originalSundayDose;

        // Backup dosaggi originali per confronto linee guida
        private decimal? _originalFcsaDose;
        private decimal? _originalAccpDose;

        // Notifica cambio dose settimanale quando cambiano i dropdown
        partial void OnSuggestedMondayDoseChanged(DoseOption? value) => NotifyWeeklyDoseChanged();
        partial void OnSuggestedTuesdayDoseChanged(DoseOption? value) => NotifyWeeklyDoseChanged();
        partial void OnSuggestedWednesdayDoseChanged(DoseOption? value) => NotifyWeeklyDoseChanged();
        partial void OnSuggestedThursdayDoseChanged(DoseOption? value) => NotifyWeeklyDoseChanged();
        partial void OnSuggestedFridayDoseChanged(DoseOption? value) => NotifyWeeklyDoseChanged();
        partial void OnSuggestedSaturdayDoseChanged(DoseOption? value) => NotifyWeeklyDoseChanged();
        partial void OnSuggestedSundayDoseChanged(DoseOption? value) => NotifyWeeklyDoseChanged();

        private void NotifyWeeklyDoseChanged()
        {
            OnPropertyChanged(nameof(CurrentSuggestedWeeklyDose));
            OnPropertyChanged(nameof(CurrentPercentageAdjustment));
        }

        // Notifica cambio proprietà sospensione dose quando ActiveSuggestion cambia
        partial void OnActiveSuggestionChanged(DosageSuggestionResult? value)
        {
            OnPropertyChanged(nameof(HasDoseSuspension));
            OnPropertyChanged(nameof(DoseSuspensionText));
            OnPropertyChanged(nameof(HasActiveWarnings));
        }

        #endregion

        #region Properties - Storico

        [ObservableProperty]
        private ObservableCollection<INRControlDto> _inrHistory = new();

        [ObservableProperty]
        private INRControlDto? _selectedHistoryItem;

        [ObservableProperty]
        private decimal _currentTTR;

        #endregion

        #region Properties - Grafico INR

        /// <summary>
        /// ViewModel per il grafico andamento INR
        /// </summary>
        [ObservableProperty]
        private INRChartViewModel _chartViewModel = new();

        /// <summary>
        /// Visibilità del pannello grafico (toggle mostra/nascondi)
        /// </summary>
        [ObservableProperty]
        private bool _isChartVisible = true;

        #endregion

        #region Collections

        public ObservableCollection<TherapyPhase> TherapyPhases { get; } = new()
        {
            TherapyPhase.Induction,
            TherapyPhase.Maintenance,
            TherapyPhase.PostAdjustment
        };

        public ObservableCollection<GuidelineType> Guidelines { get; } = new()
        {
            GuidelineType.FCSA,
            GuidelineType.ACCP
        };

        public ObservableCollection<TipoEmorragia> BleedingTypes { get; } = new()
        {
            TipoEmorragia.Nessuna,
            TipoEmorragia.Minore,
            TipoEmorragia.Maggiore,
            TipoEmorragia.RischioVitale
        };

        public ObservableCollection<SedeEmorragia> BleedingSites { get; } = new()
        {
            SedeEmorragia.Nessuna,
            SedeEmorragia.Cutanea,
            SedeEmorragia.Nasale,
            SedeEmorragia.Gengivale,
            SedeEmorragia.Gastrointestinale,
            SedeEmorragia.Urinaria,
            SedeEmorragia.Intracranica,
            SedeEmorragia.Retroperitoneale,
            SedeEmorragia.Altra
        };

        #endregion

        public INRControlViewModel(
            IUnitOfWork unitOfWork,
            IDosageCalculatorService dosageCalculator,
            ITTRCalculatorService ttrCalculator,
            IDialogService dialogService,
            INavigationService navigationService,
            WeeklySchedulePdfService pdfService)
        {
            _unitOfWork = unitOfWork;
            _dosageCalculator = dosageCalculator;
            _ttrCalculator = ttrCalculator;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _pdfService = pdfService;
            _pengoNomogramService = new Core.Services.PengoNomogramService();

            // Inizializza lista dosi disponibili e valori default
            UpdateAvailableDoses();

            // Subscribe to property changes
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[PropertyChanged] Property={e.PropertyName}, InrValue={InrValue}, _isApplyingSchedule={_isApplyingSchedule}");

            // Ricalcola dose settimanale quando cambiano le dosi giornaliere
            if (e.PropertyName == nameof(MondayDose) ||
                e.PropertyName == nameof(TuesdayDose) ||
                e.PropertyName == nameof(WednesdayDose) ||
                e.PropertyName == nameof(ThursdayDose) ||
                e.PropertyName == nameof(FridayDose) ||
                e.PropertyName == nameof(SaturdayDose) ||
                e.PropertyName == nameof(SundayDose))
            {
                RecalculateWeeklyDose();
            }

            // Aggiorna lista dosi disponibili quando cambia il checkbox
            if (e.PropertyName == nameof(ExcludeQuarterTablets))
            {
                UpdateAvailableDoses();
            }

            // Ricalcola suggerimenti quando cambia INR o fase terapia
            // NON ricalcolare quando cambia CurrentWeeklyDose (altrimenti loop infinito con ApplySuggestedSchedule)
            // NON ricalcolare quando cambia SelectedGuideline (gestito separatamente in OnSelectedGuidelineChanged)
            if (!_isApplyingSchedule &&
                (e.PropertyName == nameof(InrValue) ||
                 e.PropertyName == nameof(SelectedPhase)))
            {
                System.Diagnostics.Debug.WriteLine($"[PropertyChanged] INR or Phase changed. InrValue={InrValue}");

                if (InrValue > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[PropertyChanged] Calling CheckAndShowFourDEvaluationAsync()");

                    // Verifica se usare il nomogramma di Pengo per paziente naive
                    _ = CheckAndApplyPengoNomogramAsync();

                    // Verifica valutazione 4D anche se non c'è dosaggio
                    _ = CheckAndShowFourDEvaluationAsync();

                    // NON calcolare suggerimenti se:
                    // - È appena stato applicato il nomogramma di Pengo
                    // - Siamo in fase di induzione (il dosaggio è gestito dal nomogramma)
                    bool skipSuggestions = _pengoNomogramAlreadyApplied || SelectedPhase == TherapyPhase.Induction;

                    // Calcola suggerimenti solo se c'è un dosaggio e non siamo in condizioni di skip
                    if (CurrentWeeklyDose > 0 && !skipSuggestions)
                    {
                        _ = CalculateSuggestionsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Ricalcola la dose settimanale totale
        /// </summary>
        private void RecalculateWeeklyDose()
        {
            CurrentWeeklyDose = 
                (MondayDose?.DoseMg ?? 0) + 
                (TuesdayDose?.DoseMg ?? 0) + 
                (WednesdayDose?.DoseMg ?? 0) +
                (ThursdayDose?.DoseMg ?? 0) + 
                (FridayDose?.DoseMg ?? 0) + 
                (SaturdayDose?.DoseMg ?? 0) + 
                (SundayDose?.DoseMg ?? 0);
        }

        /// <summary>
        /// Aggiorna la lista delle dosi disponibili in base all'opzione quarti di compressa
        /// </summary>
        private void UpdateAvailableDoses()
        {
            // Salva i valori correnti prima di aggiornare la lista
            decimal mondayMg = MondayDose?.DoseMg ?? 5.0m;
            decimal tuesdayMg = TuesdayDose?.DoseMg ?? 5.0m;
            decimal wednesdayMg = WednesdayDose?.DoseMg ?? 5.0m;
            decimal thursdayMg = ThursdayDose?.DoseMg ?? 5.0m;
            decimal fridayMg = FridayDose?.DoseMg ?? 5.0m;
            decimal saturdayMg = SaturdayDose?.DoseMg ?? 5.0m;
            decimal sundayMg = SundayDose?.DoseMg ?? 5.0m;

            // Crea nuove opzioni
            var options = DoseOption.CreateOptions(ExcludeQuarterTablets);
            
            AvailableDoses.Clear();
            foreach (var option in options)
            {
                AvailableDoses.Add(option);
            }

            // Ripristina i valori arrotondati al più vicino disponibile
            MondayDose = DoseOption.FindNearest(options, mondayMg);
            TuesdayDose = DoseOption.FindNearest(options, tuesdayMg);
            WednesdayDose = DoseOption.FindNearest(options, wednesdayMg);
            ThursdayDose = DoseOption.FindNearest(options, thursdayMg);
            FridayDose = DoseOption.FindNearest(options, fridayMg);
            SaturdayDose = DoseOption.FindNearest(options, saturdayMg);
            SundayDose = DoseOption.FindNearest(options, sundayMg);
        }

        /// <summary>
        /// Trova il DoseOption corrispondente a un valore in mg
        /// </summary>
        private DoseOption FindDoseOption(decimal doseMg)
        {
            return AvailableDoses.FirstOrDefault(d => d.DoseMg == doseMg) 
                ?? DoseOption.FindNearest(AvailableDoses.ToArray(), doseMg);
        }

        /// <summary>
        /// Ottiene lo schema corrente come array di dosi
        /// </summary>
        private decimal[] GetCurrentScheduleArray()
        {
            return new[]
            {
                MondayDose?.DoseMg ?? 0,
                TuesdayDose?.DoseMg ?? 0,
                WednesdayDose?.DoseMg ?? 0,
                ThursdayDose?.DoseMg ?? 0,
                FridayDose?.DoseMg ?? 0,
                SaturdayDose?.DoseMg ?? 0,
                SundayDose?.DoseMg ?? 0
            };
        }

        #region Clipboard Helper

        /// <summary>
        /// Copia testo negli appunti con retry e gestione corretta del thread UI
        /// </summary>
        private static bool TrySetClipboardText(string text, int maxRetries = 5, int retryDelayMs = 50)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // Assicura che l'operazione avvenga sul thread UI
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Clipboard.Clear();
                        Thread.Sleep(10); // Piccolo delay dopo Clear
                        Clipboard.SetText(text);
                    });
                    return true;
                }
                catch (COMException)
                {
                    if (i < maxRetries - 1)
                        Thread.Sleep(retryDelayMs);
                }
                catch (ExternalException)
                {
                    if (i < maxRetries - 1)
                        Thread.Sleep(retryDelayMs);
                }
                catch (Exception)
                {
                    if (i < maxRetries - 1)
                        Thread.Sleep(retryDelayMs);
                }
            }
            return false;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Torna all'elenco pazienti (chiude la finestra)
        /// </summary>
        [RelayCommand]
        private void GoBack()
        {
            // Chiude la finestra corrente per tornare alla MainWindow
            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)?
                .Close();
        }

        [RelayCommand]
        public async Task LoadPatientDataAsync(int patientId)
        {
            try
            {
                PatientId = patientId;

                // Reset flag nomogramma per nuovo caricamento
                _pengoNomogramAlreadyApplied = false;

                // Carica dati paziente
                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null) return;

                PatientName = $"{patient.LastName} {patient.FirstName}";
                PatientFiscalCode = patient.FiscalCode;
                IsNaivePatient = patient.IsNaive;
                PatientAge = patient.Age;
                PatientHasBledScore = patient.HasBledScore;

                // Verifica se il wizard iniziale è stato completato
                if (!patient.IsInitialWizardCompleted)
                {
                    _dialogService.ShowWarning(
                        "Non è possibile inserire controlli INR per questo paziente.\n\n" +
                        "È necessario completare prima la configurazione iniziale obbligatoria:\n" +
                        "• Indicazione alla TAO\n" +
                        "• Valutazione Pre-TAO\n" +
                        "• Score CHA2DS2-VASc\n" +
                        "• Score HAS-BLED\n\n" +
                        "Accedere ai dettagli del paziente per completare la configurazione.",
                        "Configurazione Iniziale Richiesta");

                    // Chiudi la finestra
                    Application.Current.Windows
                        .OfType<Window>()
                        .FirstOrDefault(w => w.DataContext == this)?
                        .Close();
                    return;
                }

                // Carica indicazione attiva tramite DbContext
                var indication = await _unitOfWork.Database.Indications
                    .Include(i => i.IndicationType)
                    .Where(i => i.PatientId == patientId && i.IsActive)
                    .FirstOrDefaultAsync();

                if (indication != null)
                {
                    ActiveIndication = indication.IndicationType.Description;
                    TargetINRMin = indication.TargetINRMin;
                    TargetINRMax = indication.TargetINRMax;
                }

                // Carica storico INR
                await LoadINRHistoryAsync();

                // Carica ultimo dosaggio
                LoadLastDosage();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Errore nel caricamento dati paziente: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task LoadINRHistoryAsync()
        {
            try
            {
                // Usa GetByPatientIdWithDetailsAsync per includere DailyDoses e DosageSuggestions
                var controls = await _unitOfWork.INRControls.GetByPatientIdWithDetailsAsync(PatientId);
                
                // Converti da Data.Entities.INRControl a Core.Models.INRControl
                var coreControls = controls.Select(c => new Core.Models.INRControl
                {
                    Id = c.Id,
                    ControlDate = c.ControlDate,
                    INRValue = c.INRValue,
                    CurrentWeeklyDose = c.CurrentWeeklyDose
                }).ToList();

                var controlDtos = controls
                    .OrderByDescending(c => c.ControlDate)
                    .Select(MapToDto)
                    .ToList();

                InrHistory = new ObservableCollection<INRControlDto>(controlDtos);

                // Calcola TTR
                if (coreControls.Any())
                {
                    var ttrResult = _ttrCalculator.CalculateTTR(
                        coreControls,
                        TargetINRMin,
                        TargetINRMax);
                    CurrentTTR = (decimal)ttrResult.TTRPercentage;
                }

                // Aggiorna il grafico INR
                UpdateChart();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Errore nel caricamento storico: {ex.Message}");
            }
        }

        /// <summary>
        /// Aggiorna il grafico INR con i dati correnti
        /// </summary>
        private void UpdateChart()
        {
            if (InrHistory.Any())
            {
                ChartViewModel.LoadData(InrHistory, TargetINRMin, TargetINRMax);
                ChartViewModel.UpdateTTR(CurrentTTR);
            }
        }

        /// <summary>
        /// Toggle visibilità del grafico
        /// </summary>
        [RelayCommand]
        private void ToggleChart()
        {
            IsChartVisible = !IsChartVisible;
        }

        [RelayCommand]
        private void LoadLastDosage()
        {
            if (!InrHistory.Any()) return;

            try
            {
                // Blocca il ricalcolo durante il caricamento
                _isApplyingSchedule = true;

                var lastControl = InrHistory.First();
                MondayDose = FindDoseOption(lastControl.MondayDose);
                TuesdayDose = FindDoseOption(lastControl.TuesdayDose);
                WednesdayDose = FindDoseOption(lastControl.WednesdayDose);
                ThursdayDose = FindDoseOption(lastControl.ThursdayDose);
                FridayDose = FindDoseOption(lastControl.FridayDose);
                SaturdayDose = FindDoseOption(lastControl.SaturdayDose);
                SundayDose = FindDoseOption(lastControl.SundayDose);
            }
            finally
            {
                _isApplyingSchedule = false;
                
                // Ricalcola solo se abbiamo anche un INR
                if (InrValue > 0 && CurrentWeeklyDose > 0)
                {
                    _ = CalculateSuggestionsAsync();
                }
            }
        }

        [RelayCommand]
        private async Task CalculateSuggestionsAsync()
        {
            try
            {
                if (InrValue <= 0 || CurrentWeeklyDose <= 0)
                {
                    HasSuggestions = false;
                    return;
                }

                // Calcola suggerimento FCSA
                FcsaSuggestion = _dosageCalculator.CalculateFCSA(
                    InrValue,
                    TargetINRMin,
                    TargetINRMax,
                    CurrentWeeklyDose,
                    SelectedPhase,
                    IsCompliant,
                    IsSlowMetabolizer,
                    ThromboembolicRisk.Moderate, // TODO: calcolare dal paziente
                    SelectedBleedingType,
                    SelectedBleedingSite);

                // Calcola suggerimento ACCP
                AccpSuggestion = _dosageCalculator.CalculateACCP(
                    InrValue,
                    TargetINRMin,
                    TargetINRMax,
                    CurrentWeeklyDose,
                    SelectedPhase,
                    IsCompliant,
                    IsSlowMetabolizer,
                    ThromboembolicRisk.Moderate, // TODO: calcolare dal paziente
                    SelectedBleedingType,
                    SelectedBleedingSite);

                // Imposta suggerimento attivo
                ActiveSuggestion = SelectedGuideline == GuidelineType.FCSA 
                    ? FcsaSuggestion 
                    : AccpSuggestion;

                HasSuggestions = true;

                // Aggiorna status INR
                UpdateINRStatus();

                // Genera schema distribuito equilibratamente
                if (ActiveSuggestion != null)
                {
                    SuggestedDistributedSchedule = DoseDistributionHelper.DistributeWeeklyDose(
                        ActiveSuggestion.SuggestedWeeklyDoseMg,
                        ExcludeQuarterTablets);
                    SuggestedScheduleText = DoseDistributionHelper.GenerateShortSchedule(SuggestedDistributedSchedule);

                    // Popola i dropdown delle dosi suggerite
                    UpdateSuggestedDoseDropdowns();
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Errore nel calcolo suggerimenti: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SaveControlAsync()
        {
            try
            {
                // Validazione INR
                if (InrValue <= 0 || InrValue < 0.5m || InrValue > 15.0m)
                {
                    _dialogService.ShowWarning("Inserire un valore INR valido (tra 0.5 e 15.0).");
                    return;
                }

                // Validazione dosaggio settimanale
                if (CurrentWeeklyDose <= 0)
                {
                    _dialogService.ShowWarning("Inserire un dosaggio settimanale valido.\n\nAlmeno un giorno deve avere una dose > 0 mg.");
                    return;
                }

                // Crea entità INRControl
                var control = new Data.Entities.INRControl
                {
                    PatientId = PatientId,
                    ControlDate = ControlDate,
                    INRValue = InrValue,
                    CurrentWeeklyDose = CurrentWeeklyDose,
                    PhaseOfTherapy = SelectedPhase,
                    IsCompliant = IsCompliant,
                    Notes = Notes,
                    IsManuallyModified = IsManuallyModified
                };

                // Aggiungi dosi giornaliere
                control.DailyDoses = new List<Data.Entities.DailyDose>
                {
                    new() { DayOfWeek = 1, DoseMg = MondayDose?.DoseMg ?? 0 },
                    new() { DayOfWeek = 2, DoseMg = TuesdayDose?.DoseMg ?? 0 },
                    new() { DayOfWeek = 3, DoseMg = WednesdayDose?.DoseMg ?? 0 },
                    new() { DayOfWeek = 4, DoseMg = ThursdayDose?.DoseMg ?? 0 },
                    new() { DayOfWeek = 5, DoseMg = FridayDose?.DoseMg ?? 0 },
                    new() { DayOfWeek = 6, DoseMg = SaturdayDose?.DoseMg ?? 0 },
                    new() { DayOfWeek = 7, DoseMg = SundayDose?.DoseMg ?? 0 }
                };

                // Salva suggerimenti se disponibili
                if (FcsaSuggestion != null)
                {
                    control.DosageSuggestions.Add(new Data.Entities.DosageSuggestion
                    {
                        GuidelineUsed = Guideline.FCSA_SIMG,
                        SuggestedWeeklyDose = FcsaSuggestion.SuggestedWeeklyDoseMg,
                        LoadingDoseAction = FcsaSuggestion.LoadingDoseAction,
                        PercentageAdjustment = FcsaSuggestion.PercentageAdjustment,
                        NextControlDays = FcsaSuggestion.NextControlDays,
                        RequiresEBPM = FcsaSuggestion.RequiresEBPM,
                        RequiresVitaminK = FcsaSuggestion.RequiresVitaminK,
                        WeeklySchedule = FcsaSuggestion.WeeklySchedule.Description,
                        ClinicalNotes = FcsaSuggestion.ClinicalNotes,
                        ExportedText = GenerateExportText()
                    });
                }

                if (AccpSuggestion != null)
                {
                    control.DosageSuggestions.Add(new Data.Entities.DosageSuggestion
                    {
                        GuidelineUsed = Guideline.ACCP_ACC,
                        SuggestedWeeklyDose = AccpSuggestion.SuggestedWeeklyDoseMg,
                        LoadingDoseAction = AccpSuggestion.LoadingDoseAction,
                        PercentageAdjustment = AccpSuggestion.PercentageAdjustment,
                        NextControlDays = AccpSuggestion.NextControlDays,
                        RequiresEBPM = AccpSuggestion.RequiresEBPM,
                        RequiresVitaminK = AccpSuggestion.RequiresVitaminK,
                        WeeklySchedule = AccpSuggestion.WeeklySchedule.Description,
                        ClinicalNotes = AccpSuggestion.ClinicalNotes
                    });
                }

                await _unitOfWork.INRControls.AddAsync(control);
                await _unitOfWork.SaveChangesAsync();

                _dialogService.ShowInformation("Controllo INR salvato con successo!");

                // Ricarica storico (aggiorna anche il grafico)
                await LoadINRHistoryAsync();

                // Reset form
                ResetForm();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Errore nel salvataggio: {ex.Message}");
            }
        }

        [RelayCommand]
        private void CopyToClipboard()
        {
            if (ActiveSuggestion == null) return;

            try
            {
                var exportText = GenerateExportText();

                // Usa Task.Run per evitare problemi di threading
                Task.Run(async () =>
                {
                    await Task.Delay(100); // Piccolo delay

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            Clipboard.SetDataObject(exportText, true);
                        }
                        catch
                        {
                            // Retry con metodo alternativo
                            Clipboard.SetText(exportText);
                        }
                    });
                }).Wait(2000); // Timeout 2 secondi

                _dialogService.ShowInformation("Suggerimento copiato negli appunti!");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Errore nella copia: {ex.Message}");
            }
        }

        /// <summary>
        /// Copia negli appunti una versione concisa del suggerimento
        /// </summary>
        [RelayCommand]
        private void CopyShortToClipboard()
        {
            if (ActiveSuggestion == null) return;

            try
            {
                var shortText = GenerateShortExportText();

                // Usa Task.Run per evitare problemi di threading
                Task.Run(async () =>
                {
                    await Task.Delay(100); // Piccolo delay

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            Clipboard.SetDataObject(shortText, true);
                        }
                        catch
                        {
                            // Retry con metodo alternativo
                            Clipboard.SetText(shortText);
                        }
                    });
                }).Wait(2000); // Timeout 2 secondi

                _dialogService.ShowInformation("Testo breve copiato negli appunti!");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Errore nella copia: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ExportToFileAsync()
        {
            if (ActiveSuggestion == null) return;

            try
            {
                var exportText = GenerateExportText();
                var fileName = $"Warfarin_{PatientFiscalCode}_{ControlDate:yyyyMMdd}.txt";

                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = fileName,
                    DefaultExt = ".txt",
                    Filter = "File di testo|*.txt"
                };

                if (dialog.ShowDialog() == true)
                {
                    await System.IO.File.WriteAllTextAsync(dialog.FileName, exportText);
                    _dialogService.ShowInformation($"File esportato: {dialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Errore nell'esportazione: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ExportToPdfAsync()
        {
            if (ActiveSuggestion == null) return;

            try
            {
                var fileName = $"SchemaSettimanale_{PatientFiscalCode}_{ControlDate:yyyyMMdd}.pdf";

                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = fileName,
                    DefaultExt = ".pdf",
                    Filter = "PDF files (*.pdf)|*.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    // Carica il paziente
                    var patient = await _unitOfWork.Patients.GetByIdAsync(PatientId);
                    if (patient == null)
                    {
                        _dialogService.ShowError("Impossibile caricare i dati del paziente");
                        return;
                    }

                    // Prepara i dati per il PDF
                    var currentSchedule = GetCurrentScheduleArray();
                    var currentScheduleDescriptions = new[]
                    {
                        MondayDose?.TabletDescription ?? "—",
                        TuesdayDose?.TabletDescription ?? "—",
                        WednesdayDose?.TabletDescription ?? "—",
                        ThursdayDose?.TabletDescription ?? "—",
                        FridayDose?.TabletDescription ?? "—",
                        SaturdayDose?.TabletDescription ?? "—",
                        SundayDose?.TabletDescription ?? "—"
                    };

                    // Genera il PDF
                    await _pdfService.GeneratePdfAsync(
                        dialog.FileName,
                        patient,
                        PatientFiscalCode,
                        ActiveIndication,
                        TargetINRMin,
                        TargetINRMax,
                        ControlDate,
                        InrValue,
                        InrStatusText,
                        CurrentWeeklyDose,
                        currentSchedule,
                        currentScheduleDescriptions,
                        ActiveSuggestion,
                        SuggestedDistributedSchedule ?? new decimal[7],
                        SuggestedScheduleText,
                        SelectedGuideline.ToString(),
                        FcsaSuggestion,
                        AccpSuggestion,
                        IsWarningIgnored);

                    _dialogService.ShowInformation($"PDF generato: {dialog.FileName}");

                    // Chiedi se aprire il file
                    var result = _dialogService.ShowQuestion(
                        $"PDF generato con successo!\n\nVuoi aprire il file?",
                        "PDF Generato");

                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        var process = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = dialog.FileName,
                                UseShellExecute = true
                            }
                        };
                        process.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Errore nella generazione del PDF: {ex.Message}");
            }
        }

        [RelayCommand]
        private void LoadHistoryItem()
        {
            if (SelectedHistoryItem == null) return;

            try
            {
                // Blocca il ricalcolo durante il caricamento
                _isApplyingSchedule = true;

                InrValue = SelectedHistoryItem.INRValue;
                MondayDose = FindDoseOption(SelectedHistoryItem.MondayDose);
                TuesdayDose = FindDoseOption(SelectedHistoryItem.TuesdayDose);
                WednesdayDose = FindDoseOption(SelectedHistoryItem.WednesdayDose);
                ThursdayDose = FindDoseOption(SelectedHistoryItem.ThursdayDose);
                FridayDose = FindDoseOption(SelectedHistoryItem.FridayDose);
                SaturdayDose = FindDoseOption(SelectedHistoryItem.SaturdayDose);
                SundayDose = FindDoseOption(SelectedHistoryItem.SundayDose);
                SelectedPhase = SelectedHistoryItem.Phase;
                IsCompliant = SelectedHistoryItem.IsCompliant;
                Notes = SelectedHistoryItem.Notes ?? string.Empty;
            }
            finally
            {
                _isApplyingSchedule = false;
                
                // Ora ricalcola i suggerimenti con i dati caricati
                if (InrValue > 0 && CurrentWeeklyDose > 0)
                {
                    _ = CalculateSuggestionsAsync();
                }
            }
        }

        /// <summary>
        /// Applica lo schema suggerito ai campi di input giornalieri
        /// NON deve triggerare un ricalcolo dei suggerimenti
        /// </summary>
        [RelayCommand]
        private void ApplySuggestedSchedule()
        {
            if (SuggestedDistributedSchedule == null || SuggestedDistributedSchedule.Length != 7) return;

            try
            {
                // Blocca il ricalcolo durante l'applicazione
                _isApplyingSchedule = true;

                MondayDose = FindDoseOption(SuggestedDistributedSchedule[0]);
                TuesdayDose = FindDoseOption(SuggestedDistributedSchedule[1]);
                WednesdayDose = FindDoseOption(SuggestedDistributedSchedule[2]);
                ThursdayDose = FindDoseOption(SuggestedDistributedSchedule[3]);
                FridayDose = FindDoseOption(SuggestedDistributedSchedule[4]);
                SaturdayDose = FindDoseOption(SuggestedDistributedSchedule[5]);
                SundayDose = FindDoseOption(SuggestedDistributedSchedule[6]);

                _dialogService.ShowInformation("Schema suggerito applicato!");
            }
            finally
            {
                // Ripristina la possibilità di ricalcolo
                _isApplyingSchedule = false;
            }
        }

        /// <summary>
        /// Ricalcola lo schema settimanale distribuendo equilibratamente il dosaggio corrente
        /// </summary>
        [RelayCommand]
        private void RecalculateSchedule()
        {
            if (CurrentWeeklyDose <= 0)
            {
                _dialogService.ShowWarning("Inserire prima un dosaggio settimanale valido.");
                return;
            }

            try
            {
                // Blocca il ricalcolo durante l'applicazione
                _isApplyingSchedule = true;

                // Genera nuovo schema distribuito equilibratamente basato sul dosaggio corrente
                var newSchedule = DoseDistributionHelper.DistributeWeeklyDose(
                    CurrentWeeklyDose,
                    ExcludeQuarterTablets);

                // Applica il nuovo schema
                MondayDose = FindDoseOption(newSchedule[0]);
                TuesdayDose = FindDoseOption(newSchedule[1]);
                WednesdayDose = FindDoseOption(newSchedule[2]);
                ThursdayDose = FindDoseOption(newSchedule[3]);
                FridayDose = FindDoseOption(newSchedule[4]);
                SaturdayDose = FindDoseOption(newSchedule[5]);
                SundayDose = FindDoseOption(newSchedule[6]);

                // Aggiorna anche il testo descrittivo
                var scheduleText = DoseDistributionHelper.GenerateShortSchedule(newSchedule);

                _dialogService.ShowInformation($"Schema ricalcolato per {CurrentWeeklyDose:F1} mg/settimana:\n\n{scheduleText}");
            }
            finally
            {
                // Ripristina la possibilità di ricalcolo
                _isApplyingSchedule = false;
            }
        }

        #region Comandi Modifica Manuale

        /// <summary>
        /// Attiva la modalità modifica manuale del dosaggio suggerito
        /// </summary>
        [RelayCommand]
        private void EditSchema()
        {
            if (ActiveSuggestion == null) return;

            // Salva i valori originali per eventuale annullamento
            _originalSuggestion = ActiveSuggestion;
            _originalSuggestedSchedule = SuggestedDistributedSchedule?.ToArray();
            _originalMondayDose = SuggestedMondayDose;
            _originalTuesdayDose = SuggestedTuesdayDose;
            _originalWednesdayDose = SuggestedWednesdayDose;
            _originalThursdayDose = SuggestedThursdayDose;
            _originalFridayDose = SuggestedFridayDose;
            _originalSaturdayDose = SuggestedSaturdayDose;
            _originalSundayDose = SuggestedSundayDose;

            // Salva dosaggi originali delle linee guida per confronto
            _originalFcsaDose = FcsaSuggestion?.SuggestedWeeklyDoseMg;
            _originalAccpDose = AccpSuggestion?.SuggestedWeeklyDoseMg;

            // Inizializza i campi editabili con i valori correnti
            EditableClinicalNotes = ActiveSuggestion.ClinicalNotes ?? string.Empty;
            EditableNextControlDays = ActiveSuggestion.NextControlDays;

            // Attiva modalità modifica
            IsEditMode = true;
        }

        /// <summary>
        /// Salva le modifiche manuali al dosaggio
        /// </summary>
        [RelayCommand]
        private void SaveSchema()
        {
            if (ActiveSuggestion == null) return;

            try
            {
                // Validazione: somma dosi giornaliere
                var totalWeeklyDose = (SuggestedMondayDose?.DoseMg ?? 0) +
                                     (SuggestedTuesdayDose?.DoseMg ?? 0) +
                                     (SuggestedWednesdayDose?.DoseMg ?? 0) +
                                     (SuggestedThursdayDose?.DoseMg ?? 0) +
                                     (SuggestedFridayDose?.DoseMg ?? 0) +
                                     (SuggestedSaturdayDose?.DoseMg ?? 0) +
                                     (SuggestedSundayDose?.DoseMg ?? 0);

                // Validazione range (5-70mg settimanali)
                if (totalWeeklyDose < 5 || totalWeeklyDose > 70)
                {
                    _dialogService.ShowWarning($"Dose settimanale totale ({totalWeeklyDose:F1} mg) fuori range consentito (5-70 mg).\n\nModificare i valori giornalieri.");
                    return;
                }

                // Validazione prossimo controllo
                if (EditableNextControlDays < 1 || EditableNextControlDays > 90)
                {
                    _dialogService.ShowWarning("Prossimo controllo deve essere tra 1 e 90 giorni.");
                    return;
                }

                // Avviso se l'utente sta ignorando raccomandazioni critiche
                bool hasWarnings = ActiveSuggestion.Warnings?.Any() ?? false;
                bool hasEBPMRequired = ActiveSuggestion.RequiresEBPM;
                bool hasVitaminK = ActiveSuggestion.RequiresVitaminK;

                if (hasWarnings || hasEBPMRequired || hasVitaminK)
                {
                    var confirmMessage = "⚠️ ATTENZIONE: Stai modificando manualmente un dosaggio con raccomandazioni critiche:\n\n";

                    if (hasEBPMRequired)
                        confirmMessage += "• EBPM raccomandata\n";
                    if (hasVitaminK)
                        confirmMessage += $"• Vitamina K raccomandata ({ActiveSuggestion.VitaminKDoseMg ?? 0}mg {ActiveSuggestion.VitaminKRoute})\n";
                    if (hasWarnings)
                        confirmMessage += $"• {ActiveSuggestion.Warnings?.Count ?? 0} alert speciali presenti\n";

                    confirmMessage += "\nConfermi di voler procedere con le modifiche manuali?";

                    if (!_dialogService.ShowConfirmation(confirmMessage, "Conferma modifica manuale"))
                    {
                        return;
                    }
                }

                // Aggiorna il suggerimento attivo con i nuovi valori
                ActiveSuggestion.SuggestedWeeklyDoseMg = totalWeeklyDose;
                ActiveSuggestion.ClinicalNotes = EditableClinicalNotes;
                ActiveSuggestion.NextControlDays = EditableNextControlDays;

                // Aggiorna lo schema distribuito
                SuggestedDistributedSchedule = new[]
                {
                    SuggestedMondayDose?.DoseMg ?? 0,
                    SuggestedTuesdayDose?.DoseMg ?? 0,
                    SuggestedWednesdayDose?.DoseMg ?? 0,
                    SuggestedThursdayDose?.DoseMg ?? 0,
                    SuggestedFridayDose?.DoseMg ?? 0,
                    SuggestedSaturdayDose?.DoseMg ?? 0,
                    SuggestedSundayDose?.DoseMg ?? 0
                };

                // Rigenera testo schema
                SuggestedScheduleText = DoseDistributionHelper.GenerateShortSchedule(SuggestedDistributedSchedule);

                // Segna come modificato manualmente
                IsManuallyModified = true;

                // Disattiva modalità modifica
                IsEditMode = false;

                _dialogService.ShowInformation($"Schema modificato correttamente.\n\nNuova dose settimanale: {totalWeeklyDose:F1} mg\nProssimo controllo: tra {EditableNextControlDays} giorni");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Errore nel salvataggio delle modifiche: {ex.Message}");
            }
        }

        /// <summary>
        /// Annulla le modifiche e ripristina i valori originali
        /// </summary>
        [RelayCommand]
        private void CancelEdit()
        {
            if (_originalSuggestion == null) return;

            // Ripristina valori originali
            ActiveSuggestion = _originalSuggestion;
            SuggestedDistributedSchedule = _originalSuggestedSchedule ?? new decimal[7];
            SuggestedMondayDose = _originalMondayDose;
            SuggestedTuesdayDose = _originalTuesdayDose;
            SuggestedWednesdayDose = _originalWednesdayDose;
            SuggestedThursdayDose = _originalThursdayDose;
            SuggestedFridayDose = _originalFridayDose;
            SuggestedSaturdayDose = _originalSaturdayDose;
            SuggestedSundayDose = _originalSundayDose;

            // Disattiva modalità modifica
            IsEditMode = false;
            IsManuallyModified = false;
        }

        #endregion

        /// <summary>
        /// Elimina il controllo INR selezionato dallo storico
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanDeleteHistoryItem))]
        private async Task DeleteHistoryItemAsync()
        {
            if (SelectedHistoryItem == null) return;

            try
            {
                // Chiedi conferma
                var result = _dialogService.ShowConfirmation(
                    $"Eliminare il controllo INR del {SelectedHistoryItem.FormattedDate}?\n\n" +
                    $"INR: {SelectedHistoryItem.INRValue:F1}\n" +
                    $"Dose: {SelectedHistoryItem.CurrentWeeklyDose:F1} mg/sett\n\n" +
                    "Questa operazione non può essere annullata.",
                    "Conferma eliminazione");

                if (!result) return;

                // Elimina dal database
                var control = await _unitOfWork.INRControls.GetByIdAsync(SelectedHistoryItem.Id);
                if (control != null)
                {
                    await _unitOfWork.INRControls.DeleteAsync(control);
                    await _unitOfWork.SaveChangesAsync();

                    _dialogService.ShowInformation("Controllo INR eliminato con successo.");

                    // Ricarica storico (aggiorna anche il grafico)
                    await LoadINRHistoryAsync();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Errore durante l'eliminazione: {ex.Message}");
            }
        }

        private bool CanDeleteHistoryItem() => SelectedHistoryItem != null;

        /// <summary>
        /// Aggiorna CanExecute quando cambia la selezione nello storico
        /// </summary>
        partial void OnSelectedHistoryItemChanged(INRControlDto? value)
        {
            DeleteHistoryItemCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Cambia il suggerimento attivo quando l'utente seleziona una diversa linea guida
        /// </summary>
        partial void OnSelectedGuidelineChanged(GuidelineType value)
        {
            // Switch tra suggerimenti già calcolati (non ricalcola)
            if (FcsaSuggestion != null && AccpSuggestion != null)
            {
                ActiveSuggestion = value == GuidelineType.FCSA ? FcsaSuggestion : AccpSuggestion;
                
                // Aggiorna status e schema distribuito
                UpdateINRStatus();
                
                if (ActiveSuggestion != null)
                {
                    SuggestedDistributedSchedule = DoseDistributionHelper.DistributeWeeklyDose(
                        ActiveSuggestion.SuggestedWeeklyDoseMg,
                        ExcludeQuarterTablets);
                    SuggestedScheduleText = DoseDistributionHelper.GenerateShortSchedule(SuggestedDistributedSchedule);

                    // Popola i dropdown delle dosi suggerite
                    UpdateSuggestedDoseDropdowns();
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Aggiorna i dropdown delle dosi suggerite in base allo schema distribuito
        /// </summary>
        private void UpdateSuggestedDoseDropdowns()
        {
            if (SuggestedDistributedSchedule == null || SuggestedDistributedSchedule.Length != 7)
            {
                // Reset tutti i dropdown se non c'è uno schema valido
                SuggestedMondayDose = null;
                SuggestedTuesdayDose = null;
                SuggestedWednesdayDose = null;
                SuggestedThursdayDose = null;
                SuggestedFridayDose = null;
                SuggestedSaturdayDose = null;
                SuggestedSundayDose = null;
                return;
            }

            // Trova i DoseOption corrispondenti alle dosi suggerite
            SuggestedMondayDose = FindDoseOption(SuggestedDistributedSchedule[0]);
            SuggestedTuesdayDose = FindDoseOption(SuggestedDistributedSchedule[1]);
            SuggestedWednesdayDose = FindDoseOption(SuggestedDistributedSchedule[2]);
            SuggestedThursdayDose = FindDoseOption(SuggestedDistributedSchedule[3]);
            SuggestedFridayDose = FindDoseOption(SuggestedDistributedSchedule[4]);
            SuggestedSaturdayDose = FindDoseOption(SuggestedDistributedSchedule[5]);
            SuggestedSundayDose = FindDoseOption(SuggestedDistributedSchedule[6]);
        }

        private void UpdateINRStatus()
        {
            if (ActiveSuggestion == null) return;

            switch (ActiveSuggestion.INRStatus)
            {
                case INRStatus.InRange:
                    InrStatusText = "✓ IN RANGE TERAPEUTICO";
                    InrStatusColor = "#107C10"; // Verde
                    break;
                case INRStatus.BelowRange:
                    InrStatusText = "⚠ SOTTO-ANTICOAGULAZIONE";
                    InrStatusColor = "#FFB900"; // Giallo
                    break;
                case INRStatus.AboveRange:
                    InrStatusText = "⚠ SOVRA-ANTICOAGULAZIONE";
                    InrStatusColor = "#E81123"; // Rosso
                    break;
            }
        }

        /// <summary>
        /// Genera il testo breve/conciso per export negli appunti
        /// </summary>
        private string GenerateShortExportText()
        {
            if (ActiveSuggestion == null) return string.Empty;

            var currentSchedule = GetCurrentScheduleArray();
            var currentScheduleText = DoseDistributionHelper.GenerateShortSchedule(currentSchedule);
            var suggestedScheduleText = DoseDistributionHelper.GenerateShortSchedule(SuggestedDistributedSchedule);

            bool isDoseChanged = Math.Abs(ActiveSuggestion.SuggestedWeeklyDoseMg - CurrentWeeklyDose) > 0.1m;

            string doseSection;
            if (isDoseChanged)
            {
                doseSection = $"NUOVO DOSAGGIO ({ActiveSuggestion.SuggestedWeeklyDoseMg:F1} mg/sett): {suggestedScheduleText}";
            }
            else
            {
                doseSection = $"Proseguire con dosaggio attuale ({CurrentWeeklyDose:F1} mg/sett): {currentScheduleText}";
            }

            // Calcola intervallo prossimo controllo
            string controlInterval = ActiveSuggestion.NextControlDays switch
            {
                <= 3 => $"{ActiveSuggestion.NextControlDays} giorni",
                <= 7 => "1 settimana",
                <= 14 => "2 settimane",
                <= 21 => "3 settimane",
                <= 28 => "4 settimane",
                <= 42 => "6 settimane",
                _ => $"{ActiveSuggestion.NextControlDays} giorni"
            };

            var text = $@"INR attuale: {InrValue:F1}
Dosaggio corrente: {CurrentWeeklyDose:F1} mg/sett
{doseSection}
Prossimo controllo: {controlInterval}";

            // Aggiungi note cliniche se presenti (includono dose carico)
            if (!string.IsNullOrEmpty(ActiveSuggestion.ClinicalNotes))
            {
                text += $"\n\nNOTE: {ActiveSuggestion.ClinicalNotes}";
            }

            return text;
        }

        private string GenerateExportText()
        {
            if (ActiveSuggestion == null) return string.Empty;

            // Carica i dati del medico (sincrono perché questo metodo deve rimanere sincrono)
            var doctorData = _unitOfWork.Database.DoctorData.FirstOrDefault();

            var text = $@"═══════════════════════════════════════════════════════════════
  WARFARIN MANAGER PRO - SUGGERIMENTO DOSAGGIO
═══════════════════════════════════════════════════════════════

PAZIENTE: {PatientName}
CF: {PatientFiscalCode}

INDICAZIONE ATTIVA: {ActiveIndication}
Target INR: {TargetINRMin:F1}-{TargetINRMax:F1}

───────────────────────────────────────────────────────────────
CONTROLLO INR - {ControlDate:dd/MM/yyyy}
───────────────────────────────────────────────────────────────

INR RILEVATO: {InrValue:F1} ({InrStatusText})
Scostamento: {(InrValue - (TargetINRMin + TargetINRMax) / 2):+0.0;-0.0}

DOSAGGIO ATTUALE: {CurrentWeeklyDose:F1} mg/settimana

───────────────────────────────────────────────────────────────
SUGGERIMENTO DOSAGGIO{(IsManuallyModified ? "" : $" (Linee Guida {SelectedGuideline})")}
───────────────────────────────────────────────────────────────

";

            // Includi azione immediata solo se NON sono stati ignorati i warning
            if (!IsWarningIgnored && !string.IsNullOrEmpty(ActiveSuggestion.LoadingDoseAction))
            {
                text += $@"AZIONE IMMEDIATA:
{ActiveSuggestion.LoadingDoseAction}

";
            }

            // Calcola dose settimanale attuale (da dropdown se modificata, altrimenti dal suggerimento)
            decimal currentWeeklyDose = (SuggestedMondayDose?.DoseMg ?? 0) +
                                       (SuggestedTuesdayDose?.DoseMg ?? 0) +
                                       (SuggestedWednesdayDose?.DoseMg ?? 0) +
                                       (SuggestedThursdayDose?.DoseMg ?? 0) +
                                       (SuggestedFridayDose?.DoseMg ?? 0) +
                                       (SuggestedSaturdayDose?.DoseMg ?? 0) +
                                       (SuggestedSundayDose?.DoseMg ?? 0);

            // Calcola percentuale di aggiustamento
            decimal percentageAdjustment = CurrentWeeklyDose > 0
                ? ((currentWeeklyDose - CurrentWeeklyDose) / CurrentWeeklyDose * 100)
                : 0;

            // Aggiungi schema dettagliato del nuovo dosaggio (usa i dropdown)
            if (SuggestedMondayDose != null || SuggestedTuesdayDose != null ||
                SuggestedWednesdayDose != null || SuggestedThursdayDose != null ||
                SuggestedFridayDose != null || SuggestedSaturdayDose != null || SuggestedSundayDose != null)
            {
                text += $@"NUOVA DOSE SETTIMANALE: {currentWeeklyDose:F1} mg ({percentageAdjustment:+0.0;-0.0}%)

NUOVO SCHEMA SETTIMANALE DETTAGLIATO:
  Lunedì:    {SuggestedMondayDose?.DisplayText ?? "—"}
  Martedì:   {SuggestedTuesdayDose?.DisplayText ?? "—"}
  Mercoledì: {SuggestedWednesdayDose?.DisplayText ?? "—"}
  Giovedì:   {SuggestedThursdayDose?.DisplayText ?? "—"}
  Venerdì:   {SuggestedFridayDose?.DisplayText ?? "—"}
  Sabato:    {SuggestedSaturdayDose?.DisplayText ?? "—"}
  Domenica:  {SuggestedSundayDose?.DisplayText ?? "—"}";
            }

            // Aggiungi dose di carico se presente (per sottocoagulazione) e se NON ignorati i warning
            if (!IsWarningIgnored && ActiveSuggestion.DoseSupplementarePrimoGiorno.HasValue && ActiveSuggestion.DoseSupplementarePrimoGiorno.Value > 0)
            {
                decimal loadingDose = ActiveSuggestion.DoseSupplementarePrimoGiorno.Value;
                decimal percentageOfWeekly = CurrentWeeklyDose > 0 ? (loadingDose / CurrentWeeklyDose * 100) : 0;

                text += $@"

DOSE DI CARICO:
  • Somministrare {loadingDose:F1} mg oggi
  • Equivale al {percentageOfWeekly:F1}% della dose settimanale corrente
  • Poi proseguire con il nuovo schema settimanale da domani";
            }

            text += $@"

───────────────────────────────────────────────────────────────
PROSSIMO CONTROLLO INR
───────────────────────────────────────────────────────────────

Data consigliata: {ControlDate.AddDays(EditableNextControlDays > 0 ? EditableNextControlDays : ActiveSuggestion.NextControlDays):dd/MM/yyyy} (tra {(EditableNextControlDays > 0 ? EditableNextControlDays : ActiveSuggestion.NextControlDays)} giorni)

───────────────────────────────────────────────────────────────
NOTE CLINICHE
───────────────────────────────────────────────────────────────

{(string.IsNullOrEmpty(EditableClinicalNotes) ? ActiveSuggestion.ClinicalNotes : EditableClinicalNotes)}";

            text += @"

";

            // Includi gli alert solo se NON sono stati ignorati dall'utente
            if (!IsWarningIgnored && ActiveSuggestion.Warnings.Any())
            {
                text += "⚠ ALERT SPECIALI:\n";
                foreach (var warning in ActiveSuggestion.Warnings)
                {
                    text += $"  • {warning}\n";
                }
            }

            if (FcsaSuggestion != null && AccpSuggestion != null)
            {
                text += $@"
───────────────────────────────────────────────────────────────
CONFRONTO LINEE GUIDA
───────────────────────────────────────────────────────────────
";

                // Usa dosaggi originali se disponibili (modifica manuale), altrimenti usa quelli correnti
                decimal fcsaDose = IsManuallyModified && _originalFcsaDose.HasValue
                    ? _originalFcsaDose.Value
                    : FcsaSuggestion.SuggestedWeeklyDoseMg;

                decimal accpDose = IsManuallyModified && _originalAccpDose.HasValue
                    ? _originalAccpDose.Value
                    : AccpSuggestion.SuggestedWeeklyDoseMg;

                // Se lo schema è stato modificato manualmente, aggiungi warning
                if (IsManuallyModified)
                {
                    text += $@"
⚠️ ATTENZIONE: Dosaggio modificato manualmente dal medico
   Nuovo dosaggio manuale: {currentWeeklyDose:F1} mg/settimana
   Le dosi seguenti sono quelle suggerite PRIMA della modifica manuale.

";
                }

                text += $@"
FCSA-SIMG (Italia):
  • Nuova dose: {fcsaDose:F1} mg
  • Prossimo controllo: {FcsaSuggestion.NextControlDays} giorni

ACCP/ACC (USA):
  • Nuova dose: {accpDose:F1} mg
  • Prossimo controllo: {AccpSuggestion.NextControlDays} giorni
";
            }

            text += $@"

═══════════════════════════════════════════════════════════════";

            // Aggiunge i dati del medico se presenti
            if (doctorData != null)
            {
                text += $@"
MEDICO: dr. {doctorData.FullName}";

                if (!string.IsNullOrWhiteSpace(doctorData.Street) ||
                    !string.IsNullOrWhiteSpace(doctorData.City))
                {
                    text += "\nIndirizzo: ";
                    if (!string.IsNullOrWhiteSpace(doctorData.Street))
                        text += doctorData.Street;
                    if (!string.IsNullOrWhiteSpace(doctorData.City))
                    {
                        if (!string.IsNullOrWhiteSpace(doctorData.Street))
                            text += ", ";
                        if (!string.IsNullOrWhiteSpace(doctorData.PostalCode))
                            text += $"{doctorData.PostalCode} ";
                        text += doctorData.City;
                    }
                }

                if (!string.IsNullOrWhiteSpace(doctorData.Phone))
                    text += $"\nTel: {doctorData.Phone}";

                if (!string.IsNullOrWhiteSpace(doctorData.Email))
                    text += $"\nEmail: {doctorData.Email}";

                text += "\n───────────────────────────────────────────────────────────────";
            }

            text += $@"
Generato da: WarfarinManager Pro v1.0
Data/Ora: {DateTime.Now:dd/MM/yyyy HH:mm}
═══════════════════════════════════════════════════════════════

DISCLAIMER: Questo suggerimento è uno strumento di supporto
decisionale. Il medico prescrittore è sempre responsabile della
valutazione clinica finale e della decisione terapeutica.
═══════════════════════════════════════════════════════════════";

            return text;
        }

        private void ResetForm()
        {
            ControlDate = DateTime.Today;
            InrValue = 0;
            Notes = string.Empty;
            HasSuggestions = false;
            FcsaSuggestion = null;
            AccpSuggestion = null;
            ActiveSuggestion = null;
            SuggestedScheduleText = string.Empty;
            IsManuallyModified = false;
            IsEditMode = false;
            _pengoNomogramAlreadyApplied = false; // Reset flag nomogramma
        }

        private INRControlDto MapToDto(Data.Entities.INRControl control)
        {
            var dailyDoses = control.DailyDoses?.OrderBy(d => d.DayOfWeek).ToList() ?? new List<Data.Entities.DailyDose>();
            
            return new INRControlDto
            {
                Id = control.Id,
                PatientId = control.PatientId,
                ControlDate = control.ControlDate,
                INRValue = control.INRValue,
                TargetINRMin = TargetINRMin,
                TargetINRMax = TargetINRMax,
                // Dose settimanale salvata direttamente (fallback)
                SavedWeeklyDose = control.CurrentWeeklyDose,
                // Dosi giornaliere (se disponibili)
                MondayDose = dailyDoses.FirstOrDefault(d => d.DayOfWeek == 1)?.DoseMg ?? 0,
                TuesdayDose = dailyDoses.FirstOrDefault(d => d.DayOfWeek == 2)?.DoseMg ?? 0,
                WednesdayDose = dailyDoses.FirstOrDefault(d => d.DayOfWeek == 3)?.DoseMg ?? 0,
                ThursdayDose = dailyDoses.FirstOrDefault(d => d.DayOfWeek == 4)?.DoseMg ?? 0,
                FridayDose = dailyDoses.FirstOrDefault(d => d.DayOfWeek == 5)?.DoseMg ?? 0,
                SaturdayDose = dailyDoses.FirstOrDefault(d => d.DayOfWeek == 6)?.DoseMg ?? 0,
                SundayDose = dailyDoses.FirstOrDefault(d => d.DayOfWeek == 7)?.DoseMg ?? 0,
                Phase = control.PhaseOfTherapy,
                IsCompliant = control.IsCompliant,
                Notes = control.Notes,
                SuggestedWeeklyDose = control.DosageSuggestions
                    .FirstOrDefault()?.SuggestedWeeklyDose,
                SuggestedSchedule = control.DosageSuggestions
                    .FirstOrDefault()?.WeeklySchedule,
                NextControlDays_FCSA = control.DosageSuggestions
                    .FirstOrDefault(s => s.GuidelineUsed == Guideline.FCSA_SIMG)?.NextControlDays,
                NextControlDays_ACCP = control.DosageSuggestions
                    .FirstOrDefault(s => s.GuidelineUsed == Guideline.ACCP_ACC)?.NextControlDays
            };
        }

        /// <summary>
        /// Verifica se applicare il nomogramma di Pengo per paziente naive al primo INR
        /// </summary>
        private async Task CheckAndApplyPengoNomogramAsync()
        {
            try
            {
                // Verifica solo per pazienti naive
                if (!IsNaivePatient)
                    return;

                // Evita doppia applicazione del nomogramma
                if (_pengoNomogramAlreadyApplied)
                    return;

                // Verifica che non ci siano già controlli INR (primo INR)
                var hasExistingControls = InrHistory != null && InrHistory.Any();
                if (hasExistingControls)
                    return;

                // IMPORTANTE: Verifica che ci sia un'indicazione attiva con target INR
                if (TargetINRMin <= 0 || TargetINRMax <= 0)
                {
                    var confirm = _dialogService.ShowConfirmation(
                        "Per utilizzare il nomogramma di Pengo è necessario specificare prima l'indicazione terapeutica " +
                        "(es. Fibrillazione Atriale, TEV, ecc.) con il relativo target INR.\n\n" +
                        "Vuoi aggiungere l'indicazione ora?\n\n" +
                        "Dopo aver aggiunto l'indicazione, potrai inserire nuovamente il valore INR.",
                        "Indicazione Terapeutica Mancante");

                    if (confirm)
                    {
                        // Naviga alla form di creazione indicazione
                        _navigationService.NavigateTo<IndicationFormViewModel>(PatientId);
                    }
                    return;
                }

                // Verifica che l'INR sia nel range del nomogramma
                if (!_pengoNomogramService.IsInrInNomogramRange(InrValue))
                {
                    _dialogService.ShowWarning(
                        $"L'INR {InrValue:F1} è fuori dal range del nomogramma di Pengo (1.0 - 4.4).\n\n" +
                        "Sarà necessario impostare il dosaggio manualmente.",
                        "INR Fuori Range Nomogramma");
                    return;
                }

                // Chiedi conferma per usare il nomogramma di Pengo
                bool usePengo = _dialogService.ShowPengoNomogramConfirmation(InrValue);

                if (!usePengo)
                    return;

                // Imposta il flag per evitare doppia applicazione
                _pengoNomogramAlreadyApplied = true;

                // Imposta la fase come "Induction"
                SelectedPhase = TherapyPhase.Induction;

                // Calcola il fabbisogno stimato
                var estimatedDose = _pengoNomogramService.GetEstimatedWeeklyDose(InrValue);

                // Applica l'arrotondamento clinico
                var roundedDose = _pengoNomogramService.ApplyClinicalRounding(
                    estimatedDose,
                    PatientAge,
                    PatientHasBledScore,
                    InrValue);

                // Determina tipo di arrotondamento per informazione
                string roundingType = (PatientAge > 75 || PatientHasBledScore >= 3 || InrValue > 2.5m)
                    ? "per difetto"
                    : "per eccesso";

                // Genera e applica lo schema settimanale (dichiarato fuori dal try per usarlo nel finally)
                decimal[] weeklySchedule = WarfarinManager.UI.Helpers.DoseDistributionHelper.DistributeWeeklyDose(
                    roundedDose,
                    ExcludeQuarterTablets);

                // Blocca il ricalcolo durante l'applicazione
                _isApplyingSchedule = true;

                try
                {

                    MondayDose = FindDoseOption(weeklySchedule[0]);
                    TuesdayDose = FindDoseOption(weeklySchedule[1]);
                    WednesdayDose = FindDoseOption(weeklySchedule[2]);
                    ThursdayDose = FindDoseOption(weeklySchedule[3]);
                    FridayDose = FindDoseOption(weeklySchedule[4]);
                    SaturdayDose = FindDoseOption(weeklySchedule[5]);
                    SundayDose = FindDoseOption(weeklySchedule[6]);

                    // Aggiorna le note cliniche DOPO aver impostato lo schema
                    // così CurrentWeeklyDose è già aggiornato
                    Notes = $"Stima fabbisogno effettuata seguendo il nomogramma di Pengo (2001)\n" +
                            $"Dose stimata: {estimatedDose:F1} mg/sett → Dose arrotondata: {roundedDose:F1} mg/sett\n" +
                            $"Arrotondamento {roundingType} (Età: {PatientAge}, HAS-BLED: {PatientHasBledScore}, INR: {InrValue:F1})";

                    // Mostra il nomogramma HTML interattivo
                    _dialogService.ShowPengoNomogramHtml(InrValue);

                    // Notifica all'utente che lo schema è stato applicato
                    _dialogService.ShowInformation(
                        $"Schema settimanale calcolato secondo il nomogramma di Pengo:\n\n" +
                        $"• Fabbisogno stimato: {estimatedDose:F1} mg/sett\n" +
                        $"• Dose arrotondata {roundingType}: {roundedDose:F1} mg/sett\n\n" +
                        $"Lo schema settimanale è stato impostato automaticamente.",
                        "Schema Applicato");
                }
                finally
                {
                    _isApplyingSchedule = false;

                    // NON chiamare CalculateSuggestionsAsync() qui perché sovrascrive la dose del nomogramma!
                    // Invece, crea manualmente i suggerimenti che mostrano la dose del nomogramma senza modifiche
                    CreatePengoNomogramSuggestions(roundedDose, weeklySchedule);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Errore nell'applicazione del nomogramma di Pengo:\n{ex.Message}");
            }
        }

        /// <summary>
        /// Crea suggerimenti personalizzati per il nomogramma di Pengo (senza modifiche alla dose)
        /// </summary>
        private void CreatePengoNomogramSuggestions(decimal roundedDose, decimal[] weeklySchedule)
        {
            // Determina se l'INR è effettivamente nel range (anche se in fase di induzione)
            bool isActuallyInRange = InrValue >= TargetINRMin && InrValue <= TargetINRMax;

            // Crea un suggerimento "fittizio" che mostra la dose del nomogramma senza modificarla
            var pengoSuggestion = new DosageSuggestionResult
            {
                GuidelineUsed = GuidelineType.FCSA,
                CurrentINR = InrValue,
                TargetINRMin = TargetINRMin,
                TargetINRMax = TargetINRMax,
                CurrentWeeklyDoseMg = CurrentWeeklyDose,
                SuggestedWeeklyDoseMg = roundedDose, // Dose del nomogramma, NON modificata
                PercentageAdjustment = 0, // Nessun aggiustamento
                IsInRange = isActuallyInRange, // Verifica reale se è in range
                NextControlDays = 7, // Controllo settimanale durante induzione
                LoadingDoseAction = string.Empty, // Nessuna azione speciale (evita warning arancione)
                SospensioneDosi = 0, // IMPORTANTE: Nessuna sospensione (evita warning arancione)
                DoseSupplementarePrimoGiorno = null, // Nessuna dose supplementare
                ClinicalNotes = "Fase di induzione con nomogramma di Pengo. Mantenere questa dose per almeno 7 giorni.",
                FonteRaccomandazione = "Nomogramma di Pengo (2001)",
                UrgencyLevel = UrgencyLevel.Routine,
                Warnings = new List<string>() // IMPORTANTE: Nessun warning durante fase di induzione
            };

            // Imposta lo stesso suggerimento per entrambe le linee guida
            FcsaSuggestion = pengoSuggestion;
            AccpSuggestion = pengoSuggestion;
            ActiveSuggestion = pengoSuggestion;

            HasSuggestions = true;

            // Genera schema distribuito
            SuggestedDistributedSchedule = weeklySchedule;
            SuggestedScheduleText = DoseDistributionHelper.GenerateShortSchedule(weeklySchedule);

            // Popola i dropdown delle dosi suggerite
            UpdateSuggestedDoseDropdowns();

            // Aggiorna status INR - ma con messaggio personalizzato per fase di induzione
            UpdateINRStatusForInduction(isActuallyInRange);
        }

        /// <summary>
        /// Aggiorna lo status INR durante la fase di induzione (messaggio diverso)
        /// </summary>
        private void UpdateINRStatusForInduction(bool isInRange)
        {
            if (isInRange)
            {
                InrStatusText = "✓ INR IN RANGE (Fase induzione)";
                InrStatusColor = "#107C10"; // Verde
            }
            else
            {
                InrStatusText = "⏳ FASE INDUZIONE (Titolazione in corso)";
                InrStatusColor = "#0078D4"; // Blu - neutro, non warning
            }
        }

        /// <summary>
        /// Verifica se l'INR è significativamente fuori range e mostra il dialog di valutazione 4D
        /// </summary>
        public async Task CheckAndShowFourDEvaluationAsync()
        {
            // Verifica che ci sia un target INR valido
            if (TargetINRMin <= 0 || TargetINRMax <= 0)
                return;

            // NON mostrare valutazione 4D per pazienti naive al primo INR
            if (IsNaivePatient && (InrHistory == null || !InrHistory.Any()))
                return;

            // NON mostrare valutazione 4D durante la fase di induzione
            if (SelectedPhase == TherapyPhase.Induction)
                return;

            // Soglia: INR molto fuori range (± 0.5 dal target)
            const decimal THRESHOLD = 0.5m;

            bool isSignificantlyOutOfRange =
                InrValue < (TargetINRMin - THRESHOLD) ||
                InrValue > (TargetINRMax + THRESHOLD);

            System.Diagnostics.Debug.WriteLine($"[4D Check] INR={InrValue}, Target={TargetINRMin}-{TargetINRMax}, Threshold={THRESHOLD}, OutOfRange={isSignificantlyOutOfRange}");

            if (!isSignificantlyOutOfRange)
                return;

            // Mostra dialog di valutazione 4D
            var evaluationText = await _dialogService.ShowFourDEvaluationDialogAsync();

            if (!string.IsNullOrWhiteSpace(evaluationText))
            {
                // Aggiungi il testo della valutazione 4D alle note cliniche
                if (!string.IsNullOrWhiteSpace(Notes))
                {
                    // Aggiungi in coda alle note esistenti
                    Notes += "\n\n" + evaluationText;
                }
                else
                {
                    // Nessuna nota esistente, imposta direttamente
                    Notes = evaluationText;
                }
            }
        }

        #endregion
    }
}
