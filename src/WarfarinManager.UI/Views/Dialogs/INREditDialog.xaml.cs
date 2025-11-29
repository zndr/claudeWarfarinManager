using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using WarfarinManager.Data.Entities;
using WarfarinManager.Shared.Enums;
using WarfarinManager.UI.Helpers;

namespace WarfarinManager.UI.Views.Dialogs;

/// <summary>
/// Dialog per la modifica di un record INR
/// </summary>
public partial class INREditDialog : Window
{
    private readonly INRControl _originalControl;

    public INRControl? EditedControl { get; private set; }

    public INREditDialog(INRControl control)
    {
        InitializeComponent();

        _originalControl = control ?? throw new ArgumentNullException(nameof(control));

        LoadPhases();
        LoadControlData();
    }

    /// <summary>
    /// Carica le fasi della terapia nella ComboBox
    /// </summary>
    private void LoadPhases()
    {
        var phases = new List<PhaseItem>
        {
            new PhaseItem { Phase = TherapyPhase.Induction, Description = "Induzione" },
            new PhaseItem { Phase = TherapyPhase.Maintenance, Description = "Mantenimento" },
            new PhaseItem { Phase = TherapyPhase.PostAdjustment, Description = "Post-Aggiustamento" }
        };

        PhaseComboBox.ItemsSource = phases;
    }

    /// <summary>
    /// Carica i dati del controllo nel form
    /// </summary>
    private void LoadControlData()
    {
        ControlDatePicker.SelectedDate = _originalControl.ControlDate;
        INRValueTextBox.Text = _originalControl.INRValue.ToString("F2", CultureInfo.InvariantCulture);
        WeeklyDoseTextBox.Text = _originalControl.CurrentWeeklyDose.ToString("F1", CultureInfo.InvariantCulture);
        IsCompliantCheckBox.IsChecked = _originalControl.IsCompliant;
        NotesTextBox.Text = _originalControl.Notes ?? string.Empty;

        // Seleziona la fase
        PhaseComboBox.SelectedItem = ((List<PhaseItem>)PhaseComboBox.ItemsSource)
            .FirstOrDefault(p => p.Phase == _originalControl.PhaseOfTherapy);

        // Carica dosi giornaliere se presenti
        if (_originalControl.DailyDoses != null && _originalControl.DailyDoses.Any())
        {
            foreach (var dose in _originalControl.DailyDoses)
            {
                switch (dose.DayOfWeek)
                {
                    case 1:
                        MondayDoseTextBox.Text = dose.DoseMg.ToString("F1", CultureInfo.InvariantCulture);
                        break;
                    case 2:
                        TuesdayDoseTextBox.Text = dose.DoseMg.ToString("F1", CultureInfo.InvariantCulture);
                        break;
                    case 3:
                        WednesdayDoseTextBox.Text = dose.DoseMg.ToString("F1", CultureInfo.InvariantCulture);
                        break;
                    case 4:
                        ThursdayDoseTextBox.Text = dose.DoseMg.ToString("F1", CultureInfo.InvariantCulture);
                        break;
                    case 5:
                        FridayDoseTextBox.Text = dose.DoseMg.ToString("F1", CultureInfo.InvariantCulture);
                        break;
                    case 6:
                        SaturdayDoseTextBox.Text = dose.DoseMg.ToString("F1", CultureInfo.InvariantCulture);
                        break;
                    case 7:
                        SundayDoseTextBox.Text = dose.DoseMg.ToString("F1", CultureInfo.InvariantCulture);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Valida i dati del form
    /// </summary>
    private bool ValidateData(out string errorMessage)
    {
        errorMessage = string.Empty;

        // Valida data
        if (!ControlDatePicker.SelectedDate.HasValue)
        {
            errorMessage = "Selezionare una data per il controllo";
            return false;
        }

        if (ControlDatePicker.SelectedDate.Value > DateTime.Today)
        {
            errorMessage = "La data del controllo non può essere futura";
            return false;
        }

        // Valida INR
        if (!decimal.TryParse(INRValueTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal inrValue))
        {
            errorMessage = "Valore INR non valido";
            return false;
        }

        if (inrValue <= 0 || inrValue > 20)
        {
            errorMessage = "Il valore INR deve essere compreso tra 0 e 20";
            return false;
        }

        // Valida dose settimanale
        if (!decimal.TryParse(WeeklyDoseTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal weeklyDose))
        {
            errorMessage = "Dose settimanale non valida";
            return false;
        }

        if (weeklyDose < 0 || weeklyDose > 200)
        {
            errorMessage = "La dose settimanale deve essere compresa tra 0 e 200 mg";
            return false;
        }

        // Valida fase
        if (PhaseComboBox.SelectedItem == null)
        {
            errorMessage = "Selezionare una fase della terapia";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Crea il controllo modificato dai dati del form
    /// </summary>
    private INRControl CreateEditedControl()
    {
        var control = new INRControl
        {
            Id = _originalControl.Id,
            PatientId = _originalControl.PatientId,
            ControlDate = ControlDatePicker.SelectedDate!.Value,
            INRValue = decimal.Parse(INRValueTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
            CurrentWeeklyDose = decimal.Parse(WeeklyDoseTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
            PhaseOfTherapy = ((PhaseItem)PhaseComboBox.SelectedItem).Phase,
            IsCompliant = IsCompliantCheckBox.IsChecked ?? true,
            Notes = string.IsNullOrWhiteSpace(NotesTextBox.Text) ? null : NotesTextBox.Text.Trim(),
            CreatedAt = _originalControl.CreatedAt
        };

        // Crea dosi giornaliere se specificate
        var dailyDoses = new List<DailyDose>();

        if (TryParseDose(MondayDoseTextBox.Text, out decimal mondayDose))
            dailyDoses.Add(new DailyDose { INRControlId = control.Id, DayOfWeek = 1, DoseMg = mondayDose });

        if (TryParseDose(TuesdayDoseTextBox.Text, out decimal tuesdayDose))
            dailyDoses.Add(new DailyDose { INRControlId = control.Id, DayOfWeek = 2, DoseMg = tuesdayDose });

        if (TryParseDose(WednesdayDoseTextBox.Text, out decimal wednesdayDose))
            dailyDoses.Add(new DailyDose { INRControlId = control.Id, DayOfWeek = 3, DoseMg = wednesdayDose });

        if (TryParseDose(ThursdayDoseTextBox.Text, out decimal thursdayDose))
            dailyDoses.Add(new DailyDose { INRControlId = control.Id, DayOfWeek = 4, DoseMg = thursdayDose });

        if (TryParseDose(FridayDoseTextBox.Text, out decimal fridayDose))
            dailyDoses.Add(new DailyDose { INRControlId = control.Id, DayOfWeek = 5, DoseMg = fridayDose });

        if (TryParseDose(SaturdayDoseTextBox.Text, out decimal saturdayDose))
            dailyDoses.Add(new DailyDose { INRControlId = control.Id, DayOfWeek = 6, DoseMg = saturdayDose });

        if (TryParseDose(SundayDoseTextBox.Text, out decimal sundayDose))
            dailyDoses.Add(new DailyDose { INRControlId = control.Id, DayOfWeek = 7, DoseMg = sundayDose });

        if (dailyDoses.Any())
        {
            control.DailyDoses = dailyDoses;
        }

        return control;
    }

    /// <summary>
    /// Prova a parsare una dose giornaliera
    /// </summary>
    private bool TryParseDose(string text, out decimal dose)
    {
        dose = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out dose) && dose >= 0;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateData(out string errorMessage))
        {
            MessageBox.Show(errorMessage, "Dati non validi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        EditedControl = CreateEditedControl();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>
    /// Ricalcola le dosi giornaliere distribuendole equilibratamente dalla dose settimanale
    /// </summary>
    private void RecalculateDosesButton_Click(object sender, RoutedEventArgs e)
    {
        // Valida dose settimanale
        if (!decimal.TryParse(WeeklyDoseTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal weeklyDose))
        {
            MessageBox.Show("Inserire prima una dose settimanale valida", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (weeklyDose <= 0 || weeklyDose > 200)
        {
            MessageBox.Show("La dose settimanale deve essere compresa tra 0 e 200 mg", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Distribuisci dosi equilibratamente (senza quarti di compressa per semplicità)
        var distributedDoses = DoseDistributionHelper.DistributeWeeklyDose(weeklyDose, excludeQuarters: true);

        // Aggiorna i campi delle dosi giornaliere
        MondayDoseTextBox.Text = distributedDoses[0].ToString("F1", CultureInfo.InvariantCulture);
        TuesdayDoseTextBox.Text = distributedDoses[1].ToString("F1", CultureInfo.InvariantCulture);
        WednesdayDoseTextBox.Text = distributedDoses[2].ToString("F1", CultureInfo.InvariantCulture);
        ThursdayDoseTextBox.Text = distributedDoses[3].ToString("F1", CultureInfo.InvariantCulture);
        FridayDoseTextBox.Text = distributedDoses[4].ToString("F1", CultureInfo.InvariantCulture);
        SaturdayDoseTextBox.Text = distributedDoses[5].ToString("F1", CultureInfo.InvariantCulture);
        SundayDoseTextBox.Text = distributedDoses[6].ToString("F1", CultureInfo.InvariantCulture);

        // Mostra schema in formato leggibile
        string schedule = DoseDistributionHelper.GenerateShortSchedule(distributedDoses);
        MessageBox.Show(
            $"Dosi giornaliere ricalcolate:\n\n{schedule}\n\nTotale: {weeklyDose:F1} mg/settimana",
            "Ricalcolo completato",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /// <summary>
    /// Helper class per ComboBox fasi
    /// </summary>
    private class PhaseItem
    {
        public TherapyPhase Phase { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
