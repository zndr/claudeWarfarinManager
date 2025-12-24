using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WarfarinManager.Core.Services;
using WarfarinManager.UI.Helpers;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels.Tools;

/// <summary>
/// ViewModel per lo stimatore di dosaggio iniziale warfarin usando il Nomogramma di Pengo
/// </summary>
public partial class InitialDosageEstimatorViewModel : ObservableObject
{
    private readonly PengoNomogramService _nomogramService;
    private readonly WeeklySchedulePdfService _pdfService;

    // Array delle dosi distribuite (per PDF export)
    private decimal[]? _calculatedDoses;

    #region Input Properties

    [ObservableProperty]
    private string _patientAge = "";

    [ObservableProperty]
    private int _hasBleedScore = 0;

    [ObservableProperty]
    private string _targetInrRange = "2.0-3.0";

    [ObservableProperty]
    private string _measuredInr = "";

    [ObservableProperty]
    private bool _excludeQuarters = true;

    #endregion

    #region Output Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanExport))]
    private bool _hasResult;

    [ObservableProperty]
    private string _rawWeeklyDose = "";

    [ObservableProperty]
    private string _adjustedWeeklyDose = "";

    [ObservableProperty]
    private string _adjustmentReason = "";

    [ObservableProperty]
    private string _distributedSchedule = "";

    [ObservableProperty]
    private string _validationError = "";

    #endregion

    public bool CanExport => HasResult;

    public InitialDosageEstimatorViewModel(
        PengoNomogramService nomogramService,
        WeeklySchedulePdfService pdfService)
    {
        _nomogramService = nomogramService ?? throw new ArgumentNullException(nameof(nomogramService));
        _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
    }

    /// <summary>
    /// Calcola il dosaggio iniziale usando il Nomogramma di Pengo
    /// </summary>
    [RelayCommand]
    private void Calculate()
    {
        // Reset
        ValidationError = "";
        HasResult = false;
        _calculatedDoses = null;

        // Validazione età
        if (!int.TryParse(PatientAge, out int age) || age < 18 || age > 120)
        {
            ValidationError = "Età non valida (18-120 anni)";
            return;
        }

        // Validazione INR
        if (!decimal.TryParse(MeasuredInr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal inr))
        {
            ValidationError = "INR non valido - inserire numero decimale (es. 2.5)";
            return;
        }

        // Verifica range nomogramma
        if (!_nomogramService.IsInrInNomogramRange(inr))
        {
            ValidationError = $"INR fuori range nomogramma (1.0-4.4). Valore inserito: {inr:F1}";
            return;
        }

        try
        {
            // Step 1: Ottieni dose dal nomogramma
            decimal rawDose = _nomogramService.GetEstimatedWeeklyDose(inr);
            RawWeeklyDose = $"{rawDose:F1} mg";

            // Step 2: Applica rounding clinico
            decimal adjustedDose = _nomogramService.ApplyClinicalRounding(rawDose, age, HasBleedScore, inr);
            AdjustedWeeklyDose = $"{adjustedDose:F1} mg";

            // Step 3: Genera motivazione aggiustamento
            if (adjustedDose < rawDose)
            {
                List<string> reasons = new();
                if (age > 75) reasons.Add("età > 75 anni");
                if (HasBleedScore >= 3) reasons.Add("HAS-BLED ≥ 3");
                if (inr > 2.5m) reasons.Add("INR > 2.5");

                AdjustmentReason = reasons.Count > 0
                    ? $"Dose ridotta per: {string.Join(", ", reasons)}"
                    : "Nessun aggiustamento necessario";
            }
            else if (adjustedDose > rawDose)
            {
                AdjustmentReason = "Dose arrotondata per eccesso";
            }
            else
            {
                AdjustmentReason = "Nessun aggiustamento necessario";
            }

            // Step 4: Distribuisci dose settimanale
            _calculatedDoses = DoseDistributionHelper.DistributeWeeklyDose(adjustedDose, ExcludeQuarters);
            DistributedSchedule = DoseDistributionHelper.GenerateShortSchedule(_calculatedDoses);

            HasResult = true;
        }
        catch (Exception ex)
        {
            ValidationError = $"Errore durante il calcolo: {ex.Message}";
        }
    }

    /// <summary>
    /// Copia i risultati negli appunti
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExport))]
    private void CopyToClipboard()
    {
        try
        {
            string textToCopy = $"STIMA DOSAGGIO INIZIALE WARFARIN (Nomogramma di Pengo)\n" +
                              $"===============================================\n\n" +
                              $"PARAMETRI PAZIENTE:\n" +
                              $"Età: {PatientAge} anni\n" +
                              $"HAS-BLED score: {HasBleedScore}\n" +
                              $"INR giorno 5 (dopo 4×5mg): {MeasuredInr}\n" +
                              $"Target INR: {TargetInrRange}\n\n" +
                              $"RISULTATI:\n" +
                              $"Dose settimanale (nomogramma): {RawWeeklyDose}\n" +
                              $"Dose aggiustata: {AdjustedWeeklyDose}\n" +
                              $"{AdjustmentReason}\n\n" +
                              $"SCHEMA SETTIMANALE:\n" +
                              $"{DistributedSchedule}\n\n" +
                              $"Generato il: {DateTime.Now:dd/MM/yyyy HH:mm}";

            Clipboard.SetText(textToCopy);

            MessageBox.Show("Risultati copiati negli appunti!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore nella copia negli appunti:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Esporta i risultati in PDF
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportPdfAsync()
    {
        try
        {
            if (_calculatedDoses == null)
            {
                MessageBox.Show("Errore: dosi non calcolate correttamente.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string metadata = $"PARAMETRI PAZIENTE:\n" +
                            $"Età: {PatientAge} anni\n" +
                            $"HAS-BLED score: {HasBleedScore}\n" +
                            $"INR misurato al giorno 5 (dopo 4 dosi da 5mg): {MeasuredInr}\n" +
                            $"Target INR: {TargetInrRange}\n\n" +
                            $"RISULTATI:\n" +
                            $"Dose settimanale stimata (nomogramma): {RawWeeklyDose}\n" +
                            $"Dose aggiustata: {AdjustedWeeklyDose}\n" +
                            $"{AdjustmentReason}";

            // Mostra dialog per salvare il file
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"TaoGEST_StimaDosaggio_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                DefaultExt = ".pdf",
                Title = "Salva stima dosaggio"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await _pdfService.GenerateStandalonePdfAsync(
                    saveDialog.FileName,
                    "Stima dosaggio iniziale warfarin (Nomogramma di Pengo)",
                    _calculatedDoses,
                    metadata);

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
