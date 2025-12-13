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

                    // Verifica valutazione 4D anche se non c'è dosaggio
                    _ = CheckAndShowFourDEvaluationAsync();

                    // Calcola suggerimenti solo se c'è un dosaggio
                    if (CurrentWeeklyDose > 0)
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
        /// Copia testo negli appunti con retry per gestire il clipboard occupato
        /// </summary>
        private static bool TrySetClipboardText(string text, int maxRetries = 10, int retryDelayMs = 100)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    Clipboard.Clear();
                    Clipboard.SetDataObject(text, true);
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

                // Carica dati paziente
                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null) return;

                PatientName = $"{patient.LastName} {patient.FirstName}";
                PatientFiscalCode = patient.FiscalCode;

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
                    Notes = Notes
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
                bool success = TrySetClipboardText(exportText);
                
                if (success)
                    _dialogService.ShowInformation("Suggerimento copiato negli appunti!");
                else
                    _dialogService.ShowWarning("Impossibile accedere agli appunti. Riprovare.");
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
                bool success = TrySetClipboardText(shortText);
                
                if (success)
                    _dialogService.ShowInformation("Testo breve copiato negli appunti!");
                else
                    _dialogService.ShowWarning("Impossibile accedere agli appunti. Riprovare.");
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
                        AccpSuggestion);

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
                }
            }
        }

        #endregion

        #region Helper Methods

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
Schema corrente:
  Lunedì:    {MondayDose?.DoseMg ?? 0:F1} mg ({MondayDose?.TabletDescription ?? "—"})
  Martedì:   {TuesdayDose?.DoseMg ?? 0:F1} mg ({TuesdayDose?.TabletDescription ?? "—"})
  Mercoledì: {WednesdayDose?.DoseMg ?? 0:F1} mg ({WednesdayDose?.TabletDescription ?? "—"})
  Giovedì:   {ThursdayDose?.DoseMg ?? 0:F1} mg ({ThursdayDose?.TabletDescription ?? "—"})
  Venerdì:   {FridayDose?.DoseMg ?? 0:F1} mg ({FridayDose?.TabletDescription ?? "—"})
  Sabato:    {SaturdayDose?.DoseMg ?? 0:F1} mg ({SaturdayDose?.TabletDescription ?? "—"})
  Domenica:  {SundayDose?.DoseMg ?? 0:F1} mg ({SundayDose?.TabletDescription ?? "—"})

───────────────────────────────────────────────────────────────
SUGGERIMENTO DOSAGGIO (Linee Guida {SelectedGuideline})
───────────────────────────────────────────────────────────────

";

            if (!string.IsNullOrEmpty(ActiveSuggestion.LoadingDoseAction))
            {
                text += $@"AZIONE IMMEDIATA:
{ActiveSuggestion.LoadingDoseAction}

";
            }

            text += $@"NUOVA DOSE SETTIMANALE: {ActiveSuggestion.SuggestedWeeklyDoseMg:F1} mg ({ActiveSuggestion.PercentageAdjustment:+0.0;-0.0}%)

SCHEMA SETTIMANALE CONSIGLIATO (distribuzione equilibrata):
{SuggestedScheduleText}

───────────────────────────────────────────────────────────────
PROSSIMO CONTROLLO INR
───────────────────────────────────────────────────────────────

Data consigliata: {ControlDate.AddDays(ActiveSuggestion.NextControlDays):dd/MM/yyyy} (tra {ActiveSuggestion.NextControlDays} giorni)

───────────────────────────────────────────────────────────────
NOTE CLINICHE
───────────────────────────────────────────────────────────────

{ActiveSuggestion.ClinicalNotes}

";

            if (ActiveSuggestion.Warnings.Any())
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

FCSA-SIMG (Italia):
  • Nuova dose: {FcsaSuggestion.SuggestedWeeklyDoseMg:F1} mg
  • Prossimo controllo: {FcsaSuggestion.NextControlDays} giorni

ACCP/ACC (USA):
  • Nuova dose: {AccpSuggestion.SuggestedWeeklyDoseMg:F1} mg
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
        /// Verifica se l'INR è significativamente fuori range e mostra il dialog di valutazione 4D
        /// </summary>
        public async Task CheckAndShowFourDEvaluationAsync()
        {
            // Verifica che ci sia un target INR valido
            if (TargetINRMin <= 0 || TargetINRMax <= 0)
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
