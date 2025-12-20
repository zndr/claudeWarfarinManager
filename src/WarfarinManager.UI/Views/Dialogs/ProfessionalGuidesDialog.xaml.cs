using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Dialogs;

/// <summary>
/// Interaction logic for ProfessionalGuidesDialog.xaml
/// </summary>
public partial class ProfessionalGuidesDialog : Window
{
    private readonly IServiceProvider _serviceProvider;

    public ProfessionalGuidesDialog(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    private void OpenInteractions_Click(object sender, RoutedEventArgs e)
    {
        OpenGuide("interactions.html", "Interazioni Farmacologiche Warfarin");
        Close();
    }

    private void OpenFlowchart_Click(object sender, RoutedEventArgs e)
    {
        OpenGuide("algoritmo-gestione-inr.html", "Flowchart Gestione INR");
        Close();
    }

    private void OpenInfographic_Click(object sender, RoutedEventArgs e)
    {
        OpenGuide("infografica-tao.html", "Infografica Gestione TAO");
        Close();
    }

    private void OpenGuideText_Click(object sender, RoutedEventArgs e)
    {
        OpenGuide("linee-guida-tao.html", "Guida alla TAO con Warfarin per MMG");
        Close();
    }

    private void OpenGuidePdf_Click(object sender, RoutedEventArgs e)
    {
        OpenPdfGuide("LineeGuida.pdf");
        Close();
    }

    private void OpenPatientGuide_Click(object sender, RoutedEventArgs e)
    {
        OpenPdfGuide("Guida Warfarin per pazienti.pdf");
        Close();
    }

    private void OpenSwitchGuidePdf_Click(object sender, RoutedEventArgs e)
    {
        OpenPdfGuide("Switch_Doac_Warf.pdf");
        Close();
    }

    private void OpenSwitchInfographic_Click(object sender, RoutedEventArgs e)
    {
        OpenGuide("guida-switch-infografica.html", "Infografica Switch DOAC ↔ Warfarin");
        Close();
    }

    private void OpenGuide(string fileName, string title)
    {
        try
        {
            var guideViewModel = _serviceProvider.GetRequiredService<GuideViewModel>();
            guideViewModel.Initialize(fileName, title);

            var dialog = new GuideDialog(guideViewModel);
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore nell'apertura della guida:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OpenPdfGuide(string pdfFileName)
    {
        try
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var pdfPath = System.IO.Path.Combine(baseDirectory, "Resources", "Guides", pdfFileName);

            if (!System.IO.File.Exists(pdfPath))
            {
                MessageBox.Show(
                    $"File PDF non trovato:\n{pdfPath}",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore nell'apertura del PDF:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
