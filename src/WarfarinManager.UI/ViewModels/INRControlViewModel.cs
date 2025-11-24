using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WarfarinManager.Core.Models;
using WarfarinManager.Core.Services;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Shared.Enums;
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
        private decimal _inrValue;

        [ObservableProperty]
        private bool _isCompliant = true;

        [ObservableProperty]
        private TherapyPhase _selectedPhase = TherapyPhase.Maintenance;

        [ObservableProperty]
        private string _notes = string.Empty;

        // Dosi giornaliere (mg)
        [ObservableProperty]
        private decimal _mondayDose = 5.0m;

        [ObservableProperty]
        private decimal _tuesdayDose = 5.0m;

        [ObservableProperty]
        private decimal _wednesdayDose = 5.0m;

        [ObservableProperty]
        private decimal _thursdayDose = 5.0m;

        [ObservableProperty]
        private decimal _fridayDose = 5.0m;

        [ObservableProperty]
        private decimal _saturdayDose = 5.0m;

        [ObservableProperty]
        private decimal _sundayDose = 5.0m;

        [ObservableProperty]
        private decimal _currentWeeklyDose;

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

        #endregion

        #region Properties - Storico

        [ObservableProperty]
        private ObservableCollection<INRControlDto> _inrHistory = new();

        [ObservableProperty]
        private INRControlDto? _selectedHistoryItem;

        [ObservableProperty]
        private decimal _currentTTR;

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

        #endregion

        public INRControlViewModel(
            IUnitOfWork unitOfWork,
            IDosageCalculatorService dosageCalculator,
            ITTRCalculatorService ttrCalculator,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _dosageCalculator = dosageCalculator;
            _ttrCalculator = ttrCalculator;
            _dialogService = dialogService;
            _navigationService = navigationService;

            // Subscribe to property changes
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Ricalcola dose settimanale quando cambiano le dosi giornaliere
            if (e.PropertyName == nameof(MondayDose) ||
                e.PropertyName == nameof(TuesdayDose) ||
                e.PropertyName == nameof(WednesdayDose) ||
                e.PropertyName == nameof(ThursdayDose) ||
                e.PropertyName == nameof(FridayDose) ||
                e.PropertyName == nameof(SaturdayDose) ||
                e.PropertyName == nameof(SundayDose))
            {
                CurrentWeeklyDose = MondayDose + TuesdayDose + WednesdayDose +
                                   ThursdayDose + FridayDose + SaturdayDose + SundayDose;
            }

            // Ricalcola suggerimenti quando cambia INR o linea guida
            if (e.PropertyName == nameof(InrValue) ||
                e.PropertyName == nameof(SelectedGuideline) ||
                e.PropertyName == nameof(CurrentWeeklyDose) ||
                e.PropertyName == nameof(SelectedPhase))
            {
                if (InrValue > 0 && CurrentWeeklyDose > 0)
                {
                    _ = CalculateSuggestionsAsync();
                }
            }
        }

        #region Commands

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
                    ActiveIndication = indication.IndicationType.Description; TargetINRMin = indication.TargetINRMin;
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
                var controls = await _unitOfWork.INRControls.GetByPatientIdAsync(PatientId);
                
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
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Errore nel caricamento storico: {ex.Message}");
            }
        }

        [RelayCommand]
        private void LoadLastDosage()
        {
            if (InrHistory.Any())
            {
                var lastControl = InrHistory.First();
                MondayDose = lastControl.MondayDose;
                TuesdayDose = lastControl.TuesdayDose;
                WednesdayDose = lastControl.WednesdayDose;
                ThursdayDose = lastControl.ThursdayDose;
                FridayDose = lastControl.FridayDose;
                SaturdayDose = lastControl.SaturdayDose;
                SundayDose = lastControl.SundayDose;
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
                    IsSlowMetabolizer);

                // Calcola suggerimento ACCP
                AccpSuggestion = _dosageCalculator.CalculateACCP(
                    InrValue,
                    TargetINRMin,
                    TargetINRMax,
                    CurrentWeeklyDose,
                    SelectedPhase,
                    IsCompliant,
                    IsSlowMetabolizer);

                // Imposta suggerimento attivo
                ActiveSuggestion = SelectedGuideline == GuidelineType.FCSA 
                    ? FcsaSuggestion 
                    : AccpSuggestion;

                HasSuggestions = true;

                // Aggiorna status INR
                UpdateINRStatus();

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
                // Validazione
                if (InrValue <= 0)
                {
                    _dialogService.ShowWarning("Inserire un valore INR valido.");
                    return;
                }

                if (CurrentWeeklyDose <= 0)
                {
                    _dialogService.ShowWarning("Inserire un dosaggio settimanale valido.");
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
                    new() { DayOfWeek = 1, DoseMg = MondayDose },
                    new() { DayOfWeek = 2, DoseMg = TuesdayDose },
                    new() { DayOfWeek = 3, DoseMg = WednesdayDose },
                    new() { DayOfWeek = 4, DoseMg = ThursdayDose },
                    new() { DayOfWeek = 5, DoseMg = FridayDose },
                    new() { DayOfWeek = 6, DoseMg = SaturdayDose },
                    new() { DayOfWeek = 7, DoseMg = SundayDose }
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

                // Ricarica storico
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
                Clipboard.SetText(exportText);
                _dialogService.ShowInformation("Suggerimento copiato negli appunti!");
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
        private void LoadHistoryItem()
        {
            if (SelectedHistoryItem == null) return;

            InrValue = SelectedHistoryItem.INRValue;
            MondayDose = SelectedHistoryItem.MondayDose;
            TuesdayDose = SelectedHistoryItem.TuesdayDose;
            WednesdayDose = SelectedHistoryItem.WednesdayDose;
            ThursdayDose = SelectedHistoryItem.ThursdayDose;
            FridayDose = SelectedHistoryItem.FridayDose;
            SaturdayDose = SelectedHistoryItem.SaturdayDose;
            SundayDose = SelectedHistoryItem.SundayDose;
            SelectedPhase = SelectedHistoryItem.Phase;
            IsCompliant = SelectedHistoryItem.IsCompliant;
            Notes = SelectedHistoryItem.Notes ?? string.Empty;
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

        private string GenerateExportText()
        {
            if (ActiveSuggestion == null) return string.Empty;

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
  Lunedì:    {MondayDose:F1} mg
  Martedì:   {TuesdayDose:F1} mg
  Mercoledì: {WednesdayDose:F1} mg
  Giovedì:   {ThursdayDose:F1} mg
  Venerdì:   {FridayDose:F1} mg
  Sabato:    {SaturdayDose:F1} mg
  Domenica:  {SundayDose:F1} mg

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

SCHEMA SETTIMANALE CONSIGLIATO:
{ActiveSuggestion.WeeklySchedule.GetFullDescription()}

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

═══════════════════════════════════════════════════════════════
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
        }

        private INRControlDto MapToDto(Data.Entities.INRControl control)
        {
            // Carica le dosi giornaliere dalla collezione
            var dailyDoses = control.DailyDoses.OrderBy(d => d.DayOfWeek).ToList();
            
            return new INRControlDto
            {
                Id = control.Id,
                PatientId = control.PatientId,
                ControlDate = control.ControlDate,
                INRValue = control.INRValue,
                TargetINRMin = TargetINRMin, // Usa i valori dal paziente
                TargetINRMax = TargetINRMax,
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

        #endregion
    }
}
