using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WarfarinManager.UI.Helpers;
using WarfarinManager.UI.Models;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels.Tools;

/// <summary>
/// ViewModel per il calcolatore di dose settimanale warfarin standalone
/// </summary>
public partial class WeeklyDoseCalculatorViewModel : ObservableObject
{
    private readonly WeeklySchedulePdfService _pdfService;

    // Flag per prevenire loop infiniti durante aggiornamento programmatico
    private bool _isApplyingSchedule;

    #region Observable Properties

    // Arrays di opzioni per i ComboBox (popolati in base a ExcludeQuarters)
    [ObservableProperty]
    private DoseOption[] _mondayDoses = Array.Empty<DoseOption>();

    [ObservableProperty]
    private DoseOption[] _tuesdayDoses = Array.Empty<DoseOption>();

    [ObservableProperty]
    private DoseOption[] _wednesdayDoses = Array.Empty<DoseOption>();

    [ObservableProperty]
    private DoseOption[] _thursdayDoses = Array.Empty<DoseOption>();

    [ObservableProperty]
    private DoseOption[] _fridayDoses = Array.Empty<DoseOption>();

    [ObservableProperty]
    private DoseOption[] _saturdayDoses = Array.Empty<DoseOption>();

    [ObservableProperty]
    private DoseOption[] _sundayDoses = Array.Empty<DoseOption>();

    // Selezioni correnti per ciascun giorno
    [ObservableProperty]
    private DoseOption? _selectedMondayDose;

    [ObservableProperty]
    private DoseOption? _selectedTuesdayDose;

    [ObservableProperty]
    private DoseOption? _selectedWednesdayDose;

    [ObservableProperty]
    private DoseOption? _selectedThursdayDose;

    [ObservableProperty]
    private DoseOption? _selectedFridayDose;

    [ObservableProperty]
    private DoseOption? _selectedSaturdayDose;

    [ObservableProperty]
    private DoseOption? _selectedSundayDose;

    // Checkbox per escludere quarti di compressa
    [ObservableProperty]
    private bool _excludeQuarters = true;

    // Schema breve (es: "Lun ½, Mar 1, Mer ½...")
    [ObservableProperty]
    private string _shortSchedule = "";

    #endregion

    public WeeklyDoseCalculatorViewModel(WeeklySchedulePdfService pdfService)
    {
        _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));

        // Inizializza con opzioni default (escludi quarti)
        InitializeDoseOptions();
    }

    /// <summary>
    /// Inizializza gli array di DoseOption per tutti i ComboBox
    /// </summary>
    private void InitializeDoseOptions()
    {
        var options = DoseOption.CreateOptions(ExcludeQuarters, maxDose: 10.0m);

        MondayDoses = options;
        TuesdayDoses = options;
        WednesdayDoses = options;
        ThursdayDoses = options;
        FridayDoses = options;
        SaturdayDoses = options;
        SundayDoses = options;

        // Imposta dose iniziale a 0 per tutti i giorni
        SelectedMondayDose = options[0];
        SelectedTuesdayDose = options[0];
        SelectedWednesdayDose = options[0];
        SelectedThursdayDose = options[0];
        SelectedFridayDose = options[0];
        SelectedSaturdayDose = options[0];
        SelectedSundayDose = options[0];
    }

    /// <summary>
    /// Chiamato quando cambia la selezione di qualsiasi giorno
    /// </summary>
    partial void OnSelectedMondayDoseChanged(DoseOption? value) => RecalculateWeeklyTotal();
    partial void OnSelectedTuesdayDoseChanged(DoseOption? value) => RecalculateWeeklyTotal();
    partial void OnSelectedWednesdayDoseChanged(DoseOption? value) => RecalculateWeeklyTotal();
    partial void OnSelectedThursdayDoseChanged(DoseOption? value) => RecalculateWeeklyTotal();
    partial void OnSelectedFridayDoseChanged(DoseOption? value) => RecalculateWeeklyTotal();
    partial void OnSelectedSaturdayDoseChanged(DoseOption? value) => RecalculateWeeklyTotal();
    partial void OnSelectedSundayDoseChanged(DoseOption? value) => RecalculateWeeklyTotal();

    /// <summary>
    /// Chiamato quando cambia il checkbox ExcludeQuarters
    /// </summary>
    partial void OnExcludeQuartersChanged(bool value)
    {
        if (_isApplyingSchedule) return;

        // Salva le dosi correnti
        var currentDoses = new[]
        {
            SelectedMondayDose?.DoseMg ?? 0,
            SelectedTuesdayDose?.DoseMg ?? 0,
            SelectedWednesdayDose?.DoseMg ?? 0,
            SelectedThursdayDose?.DoseMg ?? 0,
            SelectedFridayDose?.DoseMg ?? 0,
            SelectedSaturdayDose?.DoseMg ?? 0,
            SelectedSundayDose?.DoseMg ?? 0
        };

        _isApplyingSchedule = true;

        // Rigenera opzioni con nuovo step
        var newOptions = DoseOption.CreateOptions(value, maxDose: 10.0m);

        MondayDoses = newOptions;
        TuesdayDoses = newOptions;
        WednesdayDoses = newOptions;
        ThursdayDoses = newOptions;
        FridayDoses = newOptions;
        SaturdayDoses = newOptions;
        SundayDoses = newOptions;

        // Riapplica selezioni con FindNearest
        SelectedMondayDose = DoseOption.FindNearest(newOptions, currentDoses[0]);
        SelectedTuesdayDose = DoseOption.FindNearest(newOptions, currentDoses[1]);
        SelectedWednesdayDose = DoseOption.FindNearest(newOptions, currentDoses[2]);
        SelectedThursdayDose = DoseOption.FindNearest(newOptions, currentDoses[3]);
        SelectedFridayDose = DoseOption.FindNearest(newOptions, currentDoses[4]);
        SelectedSaturdayDose = DoseOption.FindNearest(newOptions, currentDoses[5]);
        SelectedSundayDose = DoseOption.FindNearest(newOptions, currentDoses[6]);

        _isApplyingSchedule = false;

        RecalculateWeeklyTotal();
    }

    /// <summary>
    /// Ricalcola il totale settimanale e lo schema breve
    /// </summary>
    private void RecalculateWeeklyTotal()
    {
        if (_isApplyingSchedule) return;

        CurrentWeeklyDose = (SelectedMondayDose?.DoseMg ?? 0) +
                           (SelectedTuesdayDose?.DoseMg ?? 0) +
                           (SelectedWednesdayDose?.DoseMg ?? 0) +
                           (SelectedThursdayDose?.DoseMg ?? 0) +
                           (SelectedFridayDose?.DoseMg ?? 0) +
                           (SelectedSaturdayDose?.DoseMg ?? 0) +
                           (SelectedSundayDose?.DoseMg ?? 0);

        UpdateShortSchedule();

        // Notifica il cambiamento del CanExecute del comando
        RecalculateScheduleCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Aggiorna lo schema breve (es: "Lun ½, Mar 1...")
    /// </summary>
    private void UpdateShortSchedule()
    {
        var doses = new[]
        {
            SelectedMondayDose?.DoseMg ?? 0,
            SelectedTuesdayDose?.DoseMg ?? 0,
            SelectedWednesdayDose?.DoseMg ?? 0,
            SelectedThursdayDose?.DoseMg ?? 0,
            SelectedFridayDose?.DoseMg ?? 0,
            SelectedSaturdayDose?.DoseMg ?? 0,
            SelectedSundayDose?.DoseMg ?? 0
        };

        ShortSchedule = DoseDistributionHelper.GenerateShortSchedule(doses);
    }

    // Dose settimanale corrente calcolata
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentWeeklyTablets))]
    private decimal _currentWeeklyDose;

    // Dose settimanale editabile dall'utente (stringa per gestire input parziali)
    [ObservableProperty]
    private string _targetWeeklyDoseText = "";

    /// <summary>
    /// Numero di compresse settimanali formattato (1 cp = 5 mg)
    /// </summary>
    public string CurrentWeeklyTablets => DoseDistributionHelper.FormatAsTablets(CurrentWeeklyDose);

    /// <summary>
    /// Chiamato quando l'utente modifica manualmente il dosaggio settimanale target
    /// </summary>
    partial void OnTargetWeeklyDoseTextChanged(string value)
    {
        // Ignora se stiamo applicando uno schema programmaticamente
        if (_isApplyingSchedule) return;

        // Prova a parsare il valore
        if (string.IsNullOrWhiteSpace(value))
        {
            // Campo vuoto, nessuna azione
            return;
        }

        // Sostituisci virgola con punto per il parsing
        string normalizedValue = value.Replace(',', '.');

        if (decimal.TryParse(normalizedValue, System.Globalization.NumberStyles.AllowDecimalPoint,
            System.Globalization.CultureInfo.InvariantCulture, out decimal targetDose))
        {
            // Validazione base
            if (targetDose < 0)
            {
                MessageBox.Show("Il dosaggio settimanale non può essere negativo.",
                    "Valore non valido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (targetDose == 0)
            {
                // Azzera tutti i giorni
                ApplyWeeklyDoseDistribution(0);
                return;
            }

            // Verifica se il dosaggio è ragionevole (< 70 mg)
            if (targetDose >= 70)
            {
                var result = MessageBox.Show(
                    $"Il dosaggio settimanale inserito ({targetDose:F1} mg) è molto elevato.\n\n" +
                    "Un dosaggio tipico per warfarin è tra 17.5 e 52.5 mg/settimana.\n" +
                    "Dosaggi superiori a 70 mg sono estremamente rari.\n\n" +
                    "Continuare comunque?",
                    "Dosaggio elevato",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    // Ripristina il valore precedente
                    _isApplyingSchedule = true;
                    TargetWeeklyDoseText = CurrentWeeklyDose > 0 ? CurrentWeeklyDose.ToString("F1") : "";
                    _isApplyingSchedule = false;
                    return;
                }
            }

            // Applica la distribuzione
            ApplyWeeklyDoseDistribution(targetDose);
        }
    }

    /// <summary>
    /// Applica una distribuzione equilibrata per il dosaggio settimanale target
    /// </summary>
    private void ApplyWeeklyDoseDistribution(decimal targetWeeklyDose)
    {
        try
        {
            _isApplyingSchedule = true;

            if (targetWeeklyDose == 0)
            {
                // Azzera tutti i giorni
                SelectedMondayDose = FindDoseOption(0);
                SelectedTuesdayDose = FindDoseOption(0);
                SelectedWednesdayDose = FindDoseOption(0);
                SelectedThursdayDose = FindDoseOption(0);
                SelectedFridayDose = FindDoseOption(0);
                SelectedSaturdayDose = FindDoseOption(0);
                SelectedSundayDose = FindDoseOption(0);
            }
            else
            {
                // Genera distribuzione equilibrata
                var newSchedule = DoseDistributionHelper.DistributeWeeklyDose(
                    targetWeeklyDose,
                    ExcludeQuarters);

                // Applica il nuovo schema
                SelectedMondayDose = FindDoseOption(newSchedule[0]);
                SelectedTuesdayDose = FindDoseOption(newSchedule[1]);
                SelectedWednesdayDose = FindDoseOption(newSchedule[2]);
                SelectedThursdayDose = FindDoseOption(newSchedule[3]);
                SelectedFridayDose = FindDoseOption(newSchedule[4]);
                SelectedSaturdayDose = FindDoseOption(newSchedule[5]);
                SelectedSundayDose = FindDoseOption(newSchedule[6]);
            }
        }
        finally
        {
            _isApplyingSchedule = false;
        }

        // Aggiorna il totale e lo schema breve
        RecalculateWeeklyTotal();
        UpdateShortSchedule();
    }

    /// <summary>
    /// Verifica se è possibile ricalcolare lo schema (dose totale > 0)
    /// </summary>
    public bool CanRecalculateSchedule => CurrentWeeklyDose > 0;

    /// <summary>
    /// Ricalcola e distribuisce il dosaggio settimanale in modo equilibrato
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRecalculateSchedule))]
    private void RecalculateSchedule()
    {
        if (CurrentWeeklyDose <= 0)
        {
            MessageBox.Show("Inserire prima un dosaggio settimanale valido.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Blocca il ricalcolo durante l'applicazione
            _isApplyingSchedule = true;

            // Genera nuovo schema distribuito equilibratamente basato sul dosaggio corrente
            var newSchedule = DoseDistributionHelper.DistributeWeeklyDose(
                CurrentWeeklyDose,
                ExcludeQuarters);

            // Applica il nuovo schema usando FindDoseOption
            SelectedMondayDose = FindDoseOption(newSchedule[0]);
            SelectedTuesdayDose = FindDoseOption(newSchedule[1]);
            SelectedWednesdayDose = FindDoseOption(newSchedule[2]);
            SelectedThursdayDose = FindDoseOption(newSchedule[3]);
            SelectedFridayDose = FindDoseOption(newSchedule[4]);
            SelectedSaturdayDose = FindDoseOption(newSchedule[5]);
            SelectedSundayDose = FindDoseOption(newSchedule[6]);
        }
        finally
        {
            // Ripristina la possibilità di ricalcolo
            _isApplyingSchedule = false;
        }

        // Aggiorna lo schema breve dopo aver applicato le nuove selezioni
        UpdateShortSchedule();

        // Mostra conferma all'utente
        MessageBox.Show($"Schema ricalcolato per {CurrentWeeklyDose:F1} mg/settimana:\n\n{ShortSchedule}",
            "Ricalcolo Completato", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Trova il DoseOption corrispondente a un valore in mg
    /// </summary>
    private DoseOption FindDoseOption(decimal doseMg)
    {
        return MondayDoses.FirstOrDefault(d => d.DoseMg == doseMg)
            ?? DoseOption.FindNearest(MondayDoses, doseMg);
    }

    /// <summary>
    /// Copia lo schema negli appunti
    /// </summary>
    [RelayCommand]
    private void CopyToClipboard()
    {
        try
        {
            string textToCopy = $"Dose settimanale warfarin: {CurrentWeeklyDose:F1} mg ({CurrentWeeklyTablets})\n\n{ShortSchedule}";
            Clipboard.SetText(textToCopy);

            MessageBox.Show("Schema copiato negli appunti!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore nella copia negli appunti:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Esporta lo schema in PDF
    /// </summary>
    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        try
        {
            var doses = new[]
            {
                SelectedMondayDose?.DoseMg ?? 0,
                SelectedTuesdayDose?.DoseMg ?? 0,
                SelectedWednesdayDose?.DoseMg ?? 0,
                SelectedThursdayDose?.DoseMg ?? 0,
                SelectedFridayDose?.DoseMg ?? 0,
                SelectedSaturdayDose?.DoseMg ?? 0,
                SelectedSundayDose?.DoseMg ?? 0
            };

            // Mostra dialog per salvare il file
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"TaoGEST_DoseSettimanale_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                DefaultExt = ".pdf",
                Title = "Salva schema settimanale"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await _pdfService.GenerateStandalonePdfAsync(
                    saveDialog.FileName,
                    "Calcolo dose settimanale warfarin",
                    doses,
                    null);

                MessageBox.Show($"PDF generato con successo:\n{saveDialog.FileName}", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Opzionalmente apri il file
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch
                {
                    // Ignora errori nell'apertura
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore nella generazione del PDF:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
