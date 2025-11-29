using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Core.Models;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.UI.Models;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per la gestione dello storico controlli INR con TTR e statistiche
/// </summary>
public partial class INRHistoryViewModel : ObservableObject
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITTRCalculatorService _ttrCalculatorService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<INRHistoryViewModel> _logger;

    private int _patientId;
    private decimal _targetINRMin = 2.0m;
    private decimal _targetINRMax = 3.0m;
    private List<INRControlDto> _allControls = new();

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<INRHistoryRowDto> _inrHistoryRows = new();

    [ObservableProperty]
    private INRHistoryRowDto? _selectedRow;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private bool _hasNoData = true;

    // Filtri temporali
    [ObservableProperty]
    private int _selectedMonths = 12;

    [ObservableProperty]
    private bool _isThreeMonthsSelected;

    [ObservableProperty]
    private bool _isSixMonthsSelected;

    [ObservableProperty]
    private bool _isTwelveMonthsSelected = true;

    [ObservableProperty]
    private bool _isAllTimeSelected;

    // TTR
    [ObservableProperty]
    private decimal _ttrPercentage;

    [ObservableProperty]
    private string _ttrQualityText = "N/D";

    [ObservableProperty]
    private string _ttrBackgroundColor = "#666666";

    [ObservableProperty]
    private string _ttrTextColor = "White";

    // Statistiche
    [ObservableProperty]
    private int _controlCount;

    [ObservableProperty]
    private decimal _averageINR;

    [ObservableProperty]
    private decimal _standardDeviation;

    [ObservableProperty]
    private decimal _inRangePercentage;

    [ObservableProperty]
    private string _lastINRDisplay = "N/D";

    [ObservableProperty]
    private string _lastINRDateDisplay = "";

    [ObservableProperty]
    private string _lastINRColor = "#666666";

    // ViewModel per il grafico (integrato)
    [ObservableProperty]
    private INRChartViewModel? _chartViewModel;

    #endregion

    public INRHistoryViewModel(
        IUnitOfWork unitOfWork,
        ITTRCalculatorService ttrCalculatorService,
        IDialogService dialogService,
        ILogger<INRHistoryViewModel> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _ttrCalculatorService = ttrCalculatorService ?? throw new ArgumentNullException(nameof(ttrCalculatorService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Inizializza il ViewModel del grafico
        ChartViewModel = new INRChartViewModel();
    }

    /// <summary>
    /// Inizializza lo storico INR per un paziente specifico
    /// </summary>
    public async Task InitializeAsync(int patientId, decimal? targetMin = null, decimal? targetMax = null)
    {
        _patientId = patientId;

        try
        {
            IsLoading = true;
            _logger.LogInformation("Caricamento storico INR per paziente {PatientId}", patientId);

            // Recupera target INR dall'indicazione attiva se non specificato
            if (targetMin.HasValue && targetMax.HasValue)
            {
                _targetINRMin = targetMin.Value;
                _targetINRMax = targetMax.Value;
            }
            else
            {
                await LoadTargetINRAsync();
            }

            // Carica tutti i controlli INR
            await LoadAllControlsAsync();

            // Applica filtro temporale e aggiorna tutto
            UpdateFilteredData();

            _logger.LogInformation("Storico INR caricato: {Count} controlli", _allControls.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore caricamento storico INR");
            _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Carica il target INR dall'indicazione attiva del paziente
    /// </summary>
    private async Task LoadTargetINRAsync()
    {
        try
        {
            var activeIndication = await _unitOfWork.Database.Indications
                .Where(i => i.PatientId == _patientId && i.IsActive)
                .FirstOrDefaultAsync();

            if (activeIndication != null)
            {
                _targetINRMin = activeIndication.TargetINRMin;
                _targetINRMax = activeIndication.TargetINRMax;
            }
            else
            {
                // Default se nessuna indicazione attiva
                _targetINRMin = 2.0m;
                _targetINRMax = 3.0m;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossibile caricare target INR, usando default 2.0-3.0");
            _targetINRMin = 2.0m;
            _targetINRMax = 3.0m;
        }
    }

    /// <summary>
    /// Carica tutti i controlli INR dal database
    /// </summary>
    private async Task LoadAllControlsAsync()
    {
        var controls = await _unitOfWork.Database.INRControls
            .Where(c => c.PatientId == _patientId)
            .Include(c => c.DailyDoses)
            .OrderByDescending(c => c.ControlDate)
            .ToListAsync();

        _allControls = controls.Select(c => MapToDto(c)).ToList();
    }

    /// <summary>
    /// Aggiorna dati filtrati, statistiche, TTR e grafico
    /// </summary>
    private void UpdateFilteredData()
    {
        var filteredControls = FilterByTimeRange(_allControls);

        HasData = filteredControls.Any();
        HasNoData = !HasData;

        if (!HasData)
        {
            ClearStatistics();
            InrHistoryRows.Clear();
            ChartViewModel?.LoadData(Enumerable.Empty<INRControlDto>(), _targetINRMin, _targetINRMax);
            return;
        }

        // Aggiorna tabella storico con calcoli variazione
        UpdateHistoryTable(filteredControls);

        // Calcola statistiche
        CalculateStatistics(filteredControls);

        // Calcola TTR
        CalculateTTR(filteredControls);

        // Aggiorna grafico
        ChartViewModel?.LoadData(filteredControls, _targetINRMin, _targetINRMax);
        ChartViewModel?.UpdateTTR(TtrPercentage);
    }

    /// <summary>
    /// Aggiorna la tabella storico con righe e calcoli variazione dose
    /// </summary>
    private void UpdateHistoryTable(List<INRControlDto> controls)
    {
        var rows = new List<INRHistoryRowDto>();

        // Ordina per data decrescente per UI, ma calcola variazioni con ordine crescente
        var orderedAsc = controls.OrderBy(c => c.ControlDate).ToList();

        for (int i = 0; i < orderedAsc.Count; i++)
        {
            var control = orderedAsc[i];
            var row = new INRHistoryRowDto
            {
                Id = control.Id,
                ControlDate = control.ControlDate,
                FormattedDate = control.FormattedDate,
                INRValue = control.INRValue,
                FormattedINR = control.INRValue.ToString("F2"),
                StatusText = GetStatusText(control),
                StatusColor = control.StatusColor,
                IsInRange = control.IsInRange,
                CurrentWeeklyDose = control.CurrentWeeklyDose,
                FormattedDose = $"{control.CurrentWeeklyDose:F1} mg",
                Phase = control.Phase,
                PhaseDescription = control.PhaseDescription,
                IsCompliant = control.IsCompliant,
                Notes = control.Notes,
                TargetINRMin = _targetINRMin,
                TargetINRMax = _targetINRMax
            };

            // Calcola variazione dose rispetto al controllo precedente
            if (i > 0)
            {
                var previousControl = orderedAsc[i - 1];

                // Imposta dosaggio precedente
                row.PreviousWeeklyDose = previousControl.CurrentWeeklyDose;
                row.FormattedPreviousDose = $"{previousControl.CurrentWeeklyDose:F1} mg";

                if (previousControl.CurrentWeeklyDose > 0)
                {
                    decimal variation = ((control.CurrentWeeklyDose - previousControl.CurrentWeeklyDose)
                        / previousControl.CurrentWeeklyDose) * 100;
                    row.DoseVariationPercent = variation;
                    row.DoseVariationDisplay = variation >= 0 ? $"+{variation:F1}%" : $"{variation:F1}%";
                    row.VariationColor = variation == 0 ? "#666666" :
                                         variation > 0 ? "#107C10" : "#E81123";
                }

                // Calcola giorni dal controllo precedente
                row.DaysSincePrevious = (control.ControlDate - previousControl.ControlDate).Days;
                row.DaysSincePreviousDisplay = $"{row.DaysSincePrevious} gg";
            }
            else
            {
                row.DoseVariationDisplay = "-";
                row.VariationColor = "#666666";
                row.DaysSincePreviousDisplay = "-";
                row.FormattedPreviousDose = "-";
            }

            rows.Add(row);
        }

        // Ordina per data decrescente per visualizzazione
        InrHistoryRows = new ObservableCollection<INRHistoryRowDto>(
            rows.OrderByDescending(r => r.ControlDate));
    }

    private string GetStatusText(INRControlDto control)
    {
        if (control.IsInRange) return "In range";
        return control.INRValue < _targetINRMin ? "Sotto" : "Sopra";
    }

    /// <summary>
    /// Calcola statistiche aggregate
    /// </summary>
    private void CalculateStatistics(List<INRControlDto> controls)
    {
        ControlCount = controls.Count;

        if (ControlCount == 0)
        {
            ClearStatistics();
            return;
        }

        // Media INR
        AverageINR = Math.Round(controls.Average(c => c.INRValue), 2);

        // Deviazione standard
        if (ControlCount > 1)
        {
            var mean = controls.Average(c => (double)c.INRValue);
            var sumSquares = controls.Sum(c => Math.Pow((double)c.INRValue - mean, 2));
            StandardDeviation = Math.Round((decimal)Math.Sqrt(sumSquares / (ControlCount - 1)), 2);
        }
        else
        {
            StandardDeviation = 0;
        }

        // Percentuale in range
        int inRangeCount = controls.Count(c => c.IsInRange);
        InRangePercentage = Math.Round((decimal)inRangeCount / ControlCount * 100, 0);

        // Ultimo controllo
        var lastControl = controls.OrderByDescending(c => c.ControlDate).First();
        LastINRDisplay = $"{lastControl.INRValue:F2} ({lastControl.FormattedDate})";
        LastINRColor = lastControl.StatusColor;
    }

    /// <summary>
    /// Calcola TTR con metodo Rosendaal
    /// </summary>
    private void CalculateTTR(List<INRControlDto> controls)
    {
        if (controls.Count < 2)
        {
            TtrPercentage = 0;
            TtrQualityText = "Dati insufficienti";
            TtrBackgroundColor = "#666666";
            TtrTextColor = "White";
            return;
        }

        try
        {
            // Converti a modello Core per calcolo TTR
            var coreControls = controls.Select(c => new INRControl
            {
                Id = c.Id,
                PatientId = c.PatientId,
                ControlDate = c.ControlDate,
                INRValue = c.INRValue,
                CurrentWeeklyDose = c.CurrentWeeklyDose,
                PhaseOfTherapy = c.Phase,
                IsCompliant = c.IsCompliant,
                Notes = c.Notes
            }).ToList();

            var ttrResult = _ttrCalculatorService.CalculateTTR(
                coreControls, _targetINRMin, _targetINRMax);

            TtrPercentage = ttrResult.TTRPercentage;

            // Imposta colore e testo qualità
            UpdateTTRDisplay(ttrResult.TTRPercentage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore calcolo TTR");
            TtrPercentage = 0;
            TtrQualityText = "Errore";
            TtrBackgroundColor = "#666666";
            TtrTextColor = "White";
        }
    }

    /// <summary>
    /// Aggiorna visualizzazione TTR con colori e testo qualità
    /// </summary>
    private void UpdateTTRDisplay(decimal ttr)
    {
        if (ttr >= 70)
        {
            TtrBackgroundColor = "#107C10"; // Verde
            TtrQualityText = "Eccellente";
            TtrTextColor = "White";
        }
        else if (ttr >= 60)
        {
            TtrBackgroundColor = "#FFB900"; // Giallo
            TtrQualityText = "Accettabile";
            TtrTextColor = "#333333"; // Testo scuro su sfondo giallo
        }
        else if (ttr >= 50)
        {
            TtrBackgroundColor = "#FF8C00"; // Arancione
            TtrQualityText = "Subottimale";
            TtrTextColor = "White";
        }
        else
        {
            TtrBackgroundColor = "#E81123"; // Rosso
            TtrQualityText = "Critico";
            TtrTextColor = "White";
        }
    }

    /// <summary>
    /// Pulisce statistiche quando non ci sono dati
    /// </summary>
    private void ClearStatistics()
    {
        ControlCount = 0;
        AverageINR = 0;
        StandardDeviation = 0;
        InRangePercentage = 0;
        LastINRDisplay = "N/D";
        LastINRDateDisplay = "";
        LastINRColor = "#666666";
        TtrPercentage = 0;
        TtrQualityText = "N/D";
        TtrBackgroundColor = "#666666";
        TtrTextColor = "White";
    }

    /// <summary>
    /// Filtra controlli per range temporale
    /// </summary>
    private List<INRControlDto> FilterByTimeRange(List<INRControlDto> controls)
    {
        if (SelectedMonths <= 0) return controls;
        var cutoffDate = DateTime.Today.AddMonths(-SelectedMonths);
        return controls.Where(c => c.ControlDate >= cutoffDate).ToList();
    }

    #region Commands

    /// <summary>
    /// Cambia filtro temporale
    /// </summary>
    [RelayCommand]
    private void SetTimeRange(string monthsStr)
    {
        if (!int.TryParse(monthsStr, out int months)) return;

        SelectedMonths = months;
        IsThreeMonthsSelected = months == 3;
        IsSixMonthsSelected = months == 6;
        IsTwelveMonthsSelected = months == 12;
        IsAllTimeSelected = months == 0;

        // Sincronizza con grafico
        ChartViewModel?.SetTimeRangeCommand.Execute(monthsStr);

        // Aggiorna dati filtrati
        UpdateFilteredData();
    }

    /// <summary>
    /// Modifica un record INR esistente
    /// </summary>
    [RelayCommand]
    private async Task EditINRRecord(INRHistoryRowDto? row)
    {
        if (row == null) return;

        try
        {
            _logger.LogInformation("Modifica record INR {Id}", row.Id);

            // Carica il record completo con tutte le relazioni
            var control = await _unitOfWork.Database.INRControls
                .Include(c => c.DailyDoses)
                .FirstOrDefaultAsync(c => c.Id == row.Id);

            if (control == null)
            {
                _dialogService.ShowWarning("Record non trovato", "Modifica");
                return;
            }

            // Apri dialog di modifica
            var result = await _dialogService.ShowINREditDialogAsync(control);

            if (result != null)
            {
                // Aggiorna il record nel database
                control.ControlDate = result.ControlDate;
                control.INRValue = result.INRValue;
                control.CurrentWeeklyDose = result.CurrentWeeklyDose;
                control.PhaseOfTherapy = result.PhaseOfTherapy;
                control.IsCompliant = result.IsCompliant;
                control.Notes = result.Notes;

                // Aggiorna dosi giornaliere
                if (result.DailyDoses != null && result.DailyDoses.Any())
                {
                    // Rimuovi dosi esistenti
                    _unitOfWork.Database.DailyDoses.RemoveRange(control.DailyDoses);

                    // Aggiungi nuove dosi
                    control.DailyDoses = result.DailyDoses;
                }

                await _unitOfWork.SaveChangesAsync();

                _dialogService.ShowInformation("Record INR modificato con successo", "Modifica Completata");

                // Ricarica i dati
                await LoadAllControlsAsync();
                UpdateFilteredData();

                _logger.LogInformation("Record INR {Id} modificato con successo", row.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la modifica del record INR {Id}", row?.Id);
            _dialogService.ShowError($"Errore durante la modifica: {ex.Message}", "Errore");
        }
    }

    /// <summary>
    /// Cancella un record INR
    /// </summary>
    [RelayCommand]
    private async Task DeleteINRRecord(INRHistoryRowDto? row)
    {
        if (row == null) return;

        try
        {
            _logger.LogInformation("Richiesta cancellazione record INR {Id}", row.Id);

            // Conferma cancellazione
            var confirmed = _dialogService.ShowConfirmation(
                $"Sei sicuro di voler eliminare il controllo INR del {row.FormattedDate}?\n\n" +
                $"INR: {row.FormattedINR}\n" +
                $"Dose: {row.FormattedDose}\n\n" +
                "Questa operazione non può essere annullata.",
                "Conferma Cancellazione");

            if (!confirmed) return;

            // Carica il record con tutte le relazioni
            var control = await _unitOfWork.Database.INRControls
                .Include(c => c.DailyDoses)
                .Include(c => c.DosageSuggestions)
                .FirstOrDefaultAsync(c => c.Id == row.Id);

            if (control == null)
            {
                _dialogService.ShowWarning("Record non trovato", "Cancellazione");
                return;
            }

            // Rimuovi dosi giornaliere associate
            if (control.DailyDoses.Any())
            {
                _unitOfWork.Database.DailyDoses.RemoveRange(control.DailyDoses);
            }

            // Rimuovi suggerimenti dosaggio associati
            if (control.DosageSuggestions.Any())
            {
                _unitOfWork.Database.DosageSuggestions.RemoveRange(control.DosageSuggestions);
            }

            // Rimuovi il controllo
            _unitOfWork.Database.INRControls.Remove(control);

            await _unitOfWork.SaveChangesAsync();

            _dialogService.ShowInformation("Record INR eliminato con successo", "Cancellazione Completata");

            // Ricarica i dati
            await LoadAllControlsAsync();
            UpdateFilteredData();

            _logger.LogInformation("Record INR {Id} eliminato con successo", row.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la cancellazione del record INR {Id}", row?.Id);
            _dialogService.ShowError($"Errore durante la cancellazione: {ex.Message}", "Errore");
        }
    }

    /// <summary>
    /// Esporta storico in CSV
    /// </summary>
    [RelayCommand]
    private void ExportToCsv()
    {
        if (!InrHistoryRows.Any())
        {
            _dialogService.ShowWarning("Nessun dato da esportare", "Export CSV");
            return;
        }

        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "File CSV (*.csv)|*.csv",
                FileName = $"StoricoINR_{DateTime.Now:yyyyMMdd}",
                Title = "Esporta Storico INR"
            };

            if (saveDialog.ShowDialog() != true) return;

            var sb = new StringBuilder();

            // Header
            sb.AppendLine("Data;INR;Stato;Dosaggio precedente (mg);Nuovo dosaggio (mg);Variazione %;Giorni;Note");

            // Righe
            foreach (var row in InrHistoryRows)
            {
                sb.AppendLine(string.Join(";",
                    row.ControlDate.ToString("dd/MM/yyyy"),
                    row.INRValue.ToString("F2").Replace(".", ","),
                    row.StatusText,
                    row.PreviousWeeklyDose?.ToString("F1").Replace(".", ",") ?? "-",
                    row.CurrentWeeklyDose.ToString("F1").Replace(".", ","),
                    row.DoseVariationPercent?.ToString("F1").Replace(".", ",") ?? "",
                    row.DaysSincePrevious?.ToString() ?? "",
                    (row.Notes ?? "").Replace(";", ",").Replace("\n", " ")
                ));
            }

            File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);

            _dialogService.ShowInformation(
                $"Esportati {InrHistoryRows.Count} controlli in:\n{saveDialog.FileName}",
                "Export Completato");

            _logger.LogInformation("Export CSV completato: {Path}", saveDialog.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore export CSV");
            _dialogService.ShowError($"Errore export: {ex.Message}", "Errore");
        }
    }

    /// <summary>
    /// Gestisce selezione riga nella tabella - sincronizza con grafico
    /// </summary>
    partial void OnSelectedRowChanged(INRHistoryRowDto? value)
    {
        if (value != null && ChartViewModel != null)
        {
            // Notifica il grafico del punto selezionato
            ChartViewModel.OnChartPointClicked(value.ControlDate);
        }
    }

    #endregion

    #region Mapping

    /// <summary>
    /// Mappa entity INRControl a DTO
    /// </summary>
    private INRControlDto MapToDto(Data.Entities.INRControl entity)
    {
        // Calcola dose settimanale dalle dosi giornaliere
        decimal weeklyDose = entity.CurrentWeeklyDose;
        decimal mondayDose = 0, tuesdayDose = 0, wednesdayDose = 0, thursdayDose = 0;
        decimal fridayDose = 0, saturdayDose = 0, sundayDose = 0;

        if (entity.DailyDoses != null && entity.DailyDoses.Any())
        {
            foreach (var daily in entity.DailyDoses)
            {
                switch (daily.DayOfWeek)
                {
                    case 1: mondayDose = daily.DoseMg; break;
                    case 2: tuesdayDose = daily.DoseMg; break;
                    case 3: wednesdayDose = daily.DoseMg; break;
                    case 4: thursdayDose = daily.DoseMg; break;
                    case 5: fridayDose = daily.DoseMg; break;
                    case 6: saturdayDose = daily.DoseMg; break;
                    case 7: sundayDose = daily.DoseMg; break;
                }
            }
            var calculatedWeekly = mondayDose + tuesdayDose + wednesdayDose + thursdayDose +
                                   fridayDose + saturdayDose + sundayDose;
            if (calculatedWeekly > 0)
            {
                weeklyDose = calculatedWeekly;
            }
        }

        return new INRControlDto
        {
            Id = entity.Id,
            PatientId = entity.PatientId,
            ControlDate = entity.ControlDate,
            INRValue = entity.INRValue,
            TargetINRMin = _targetINRMin,
            TargetINRMax = _targetINRMax,
            SavedWeeklyDose = entity.CurrentWeeklyDose,
            MondayDose = mondayDose,
            TuesdayDose = tuesdayDose,
            WednesdayDose = wednesdayDose,
            ThursdayDose = thursdayDose,
            FridayDose = fridayDose,
            SaturdayDose = saturdayDose,
            SundayDose = sundayDose,
            Phase = entity.PhaseOfTherapy,
            IsCompliant = entity.IsCompliant,
            Notes = entity.Notes
        };
    }

    #endregion
}

/// <summary>
/// DTO per riga tabella storico INR con dati calcolati
/// </summary>
public class INRHistoryRowDto
{
    public int Id { get; set; }
    public DateTime ControlDate { get; set; }
    public string FormattedDate { get; set; } = string.Empty;
    public decimal INRValue { get; set; }
    public string FormattedINR { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public string StatusColor { get; set; } = "#666666";
    public bool IsInRange { get; set; }
    public decimal CurrentWeeklyDose { get; set; }
    public string FormattedDose { get; set; } = string.Empty;
    public decimal? PreviousWeeklyDose { get; set; }
    public string FormattedPreviousDose { get; set; } = "-";
    public decimal? DoseVariationPercent { get; set; }
    public string DoseVariationDisplay { get; set; } = "-";
    public string VariationColor { get; set; } = "#666666";
    public int? DaysSincePrevious { get; set; }
    public string DaysSincePreviousDisplay { get; set; } = "-";
    public Shared.Enums.TherapyPhase Phase { get; set; }
    public string PhaseDescription { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public string? Notes { get; set; }
    public decimal TargetINRMin { get; set; }
    public decimal TargetINRMax { get; set; }

    /// <summary>
    /// Colore sfondo riga basato su stato INR
    /// </summary>
    public string RowBackgroundColor => IsInRange ? "White" :
        (INRValue < TargetINRMin ? "#FFF8E1" : "#FFEBEE");
}
