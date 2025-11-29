using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WarfarinManager.UI.Views.Dialogs;

/// <summary>
/// Dialog per la valutazione 4D quando l'INR è significativamente fuori range
/// </summary>
public partial class FourDEvaluationDialog : Window
{
    public string? EvaluationText { get; private set; }

    public FourDEvaluationDialog()
    {
        InitializeComponent();

        // Aggiungi handler per conteggio caratteri
        DietTextBox.TextChanged += (s, e) => UpdateCharCount(DietTextBox, DietCharCount);
        DosageTextBox.TextChanged += (s, e) => UpdateCharCount(DosageTextBox, DosageCharCount);
        DrugsTextBox.TextChanged += (s, e) => UpdateCharCount(DrugsTextBox, DrugsCharCount);
        DiseasesTextBox.TextChanged += (s, e) => UpdateCharCount(DiseasesTextBox, DiseasesCharCount);
    }

    /// <summary>
    /// Aggiorna il conteggio dei caratteri
    /// </summary>
    private void UpdateCharCount(TextBox textBox, TextBlock charCount)
    {
        int count = textBox.Text?.Length ?? 0;
        charCount.Text = $"{count}/100 caratteri";
    }

    /// <summary>
    /// Gestisce il cambio di stato delle checkbox
    /// </summary>
    private void OnCheckBoxChanged(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox)
        {
            // Trova il TextBox e il TextBlock associati
            var parent = checkBox.Parent as StackPanel;
            if (parent == null) return;

            TextBox? textBox = null;
            TextBlock? charCount = null;

            foreach (var child in parent.Children)
            {
                if (child is TextBox tb)
                    textBox = tb;
                else if (child is TextBlock tc)
                    charCount = tc;
            }

            if (textBox != null && charCount != null)
            {
                if (checkBox.IsChecked == true)
                {
                    textBox.Visibility = Visibility.Visible;
                    charCount.Visibility = Visibility.Visible;
                    textBox.Focus();
                }
                else
                {
                    textBox.Visibility = Visibility.Collapsed;
                    charCount.Visibility = Visibility.Collapsed;
                    textBox.Clear();
                }
            }
        }
    }

    /// <summary>
    /// Genera il testo della valutazione 4D
    /// </summary>
    private string GenerateEvaluationText()
    {
        var sb = new StringBuilder();
        bool hasAnyEntry = false;

        sb.AppendLine("4D:");

        if (DietCheckBox.IsChecked == true && !string.IsNullOrWhiteSpace(DietTextBox.Text))
        {
            sb.AppendLine($"Diet: {DietTextBox.Text.Trim()}");
            hasAnyEntry = true;
        }

        if (DosageCheckBox.IsChecked == true && !string.IsNullOrWhiteSpace(DosageTextBox.Text))
        {
            sb.AppendLine($"Dosage: {DosageTextBox.Text.Trim()}");
            hasAnyEntry = true;
        }

        if (DrugsCheckBox.IsChecked == true && !string.IsNullOrWhiteSpace(DrugsTextBox.Text))
        {
            sb.AppendLine($"Drugs: {DrugsTextBox.Text.Trim()}");
            hasAnyEntry = true;
        }

        if (DiseasesCheckBox.IsChecked == true && !string.IsNullOrWhiteSpace(DiseasesTextBox.Text))
        {
            sb.AppendLine($"Diseases: {DiseasesTextBox.Text.Trim()}");
            hasAnyEntry = true;
        }

        return hasAnyEntry ? sb.ToString().TrimEnd() : string.Empty;
    }

    /// <summary>
    /// Valida che almeno una checkbox sia selezionata con testo
    /// </summary>
    private bool ValidateInput()
    {
        if (DietCheckBox.IsChecked == true && !string.IsNullOrWhiteSpace(DietTextBox.Text))
            return true;

        if (DosageCheckBox.IsChecked == true && !string.IsNullOrWhiteSpace(DosageTextBox.Text))
            return true;

        if (DrugsCheckBox.IsChecked == true && !string.IsNullOrWhiteSpace(DrugsTextBox.Text))
            return true;

        if (DiseasesCheckBox.IsChecked == true && !string.IsNullOrWhiteSpace(DiseasesTextBox.Text))
            return true;

        return false;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateInput())
        {
            MessageBox.Show(
                "Selezionare almeno un fattore e inserire una spiegazione.",
                "Validazione",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        EvaluationText = GenerateEvaluationText();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
