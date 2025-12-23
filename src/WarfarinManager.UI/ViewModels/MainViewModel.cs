using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Reflection;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WarfarinManager.UI.Services;
using WarfarinManager.UI.Views.Dialogs;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel principale per MainWindow
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _title = "TaoGEST";

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private bool _isInHomePage = true;

    public MainViewModel(INavigationService navigationService, IDialogService dialogService, IServiceProvider serviceProvider)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;

        // Sottoscrivi agli eventi di navigazione
        _navigationService.CurrentViewChanged += OnCurrentViewChanged;
    }

    private void OnCurrentViewChanged(object? sender, EventArgs e)
    {
        // Aggiorna lo stato in base alla view corrente
        IsInHomePage = _navigationService.CurrentView is Views.Dashboard.PatientListView;

        // Notifica i comandi che dipendono dallo stato della navigazione
        ShowInteractionCheckerCommand.NotifyCanExecuteChanged();
        ShowBridgeTherapyCommand.NotifyCanExecuteChanged();
        ShowDrugDatabaseCommand.NotifyCanExecuteChanged();
        ShowSwitchTherapyCommand.NotifyCanExecuteChanged();
    }

    #region File Menu Commands

    [RelayCommand]
    private void NewPatient()
    {
        _navigationService.NavigateToNewPatient();
    }

    [RelayCommand]
    private void ExportData()
    {
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Esporta dati");
    }

    [RelayCommand]
    private void ImportData()
    {
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Importa dati");
    }

    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        ThemeManager.Instance.ToggleTheme();
    }

    #endregion

    #region Pazienti Menu Commands

    [RelayCommand]
    private void ShowPatientList()
    {
        _navigationService.NavigateToPatientList();
    }

    [RelayCommand]
    private void SearchPatient()
    {
        // Se siamo già nella homepage, sposta il focus sul campo ricerca
        if (IsInHomePage)
        {
            // Invia un messaggio al ViewModel della PatientList per settare il focus
            if (_navigationService.CurrentView is Views.Dashboard.PatientListView view &&
                view.DataContext is PatientListViewModel vm)
            {
                vm.FocusSearchBox();
            }
        }
        else
        {
            // Altrimenti naviga alla homepage
            _navigationService.NavigateToPatientList();
        }
    }

    [RelayCommand]
    private void ShowPatientStatistics()
    {
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Statistiche pazienti");
    }

    #endregion

    #region Strumenti Menu Commands

    [RelayCommand(CanExecute = nameof(CanExecutePatientSpecificCommand))]
    private void ShowInteractionChecker()
    {
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Verifica interazioni");
    }

    [RelayCommand(CanExecute = nameof(CanExecutePatientSpecificCommand))]
    private void ShowBridgeTherapy()
    {
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Bridge Therapy");
    }

    [RelayCommand(CanExecute = nameof(CanExecutePatientSpecificCommand))]
    private void ShowDrugDatabase()
    {
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Database farmaci");
    }

    [RelayCommand]
    private void ShowSwitchTherapy()
    {
        try
        {
            var switchViewModel = _serviceProvider.GetRequiredService<SwitchTherapyViewModel>();
            var switchView = new Views.Switch.SwitchTherapyView(switchViewModel);

            // Se c'è un paziente selezionato, pre-compila i dati
            // Qui potresti passare l'ID del paziente corrente se disponibile
            // switchView.SetPatient(currentPatientId);

            switchView.Owner = Application.Current.MainWindow;
            switchView.ShowDialog();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Errore nell'apertura dello strumento Switch:\n{ex.Message}", "Errore");
        }
    }

    private bool CanExecutePatientSpecificCommand() => !IsInHomePage;

    #endregion

    #region Opzioni Menu Commands

    [RelayCommand]
    private void ShowDoctorData()
    {
        var dialog = _serviceProvider.GetRequiredService<DoctorDataDialog>();
        dialog.ShowDialog();
    }

    [RelayCommand]
    private void ShowPreferences()
    {
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Preferenze");
    }

    [RelayCommand]
    private void ShowDatabaseSettings()
    {
        var dialog = _serviceProvider.GetRequiredService<DatabaseManagementDialog>();
        dialog.ShowDialog();
    }

    #endregion

    #region Aiuto Menu Commands

    [RelayCommand]
    private void ShowUserGuide()
    {
        try
        {
            var guideViewModel = _serviceProvider.GetRequiredService<GuideViewModel>();
            guideViewModel.Initialize("user-guide.html", "Guida Utente - TaoGEST");

            var dialog = new GuideDialog(guideViewModel);
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Errore nell'apertura della guida:\n{ex.Message}", "Errore");
        }
    }

    [RelayCommand]
    private void ShowProfessionalGuidesDialog()
    {
        try
        {
            var dialog = new ProfessionalGuidesDialog(_serviceProvider);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Errore nell'apertura del dialog guide professionali:\n{ex.Message}", "Errore");
        }
    }

    [RelayCommand]
    private void ShowProfessionalGuide(string fileName)
    {
        try
        {
            // Determina il titolo della guida in base al file
            var title = fileName switch
            {
                "interactions.html" => "Interazioni Farmacologiche Warfarin",
                "algoritmo-gestione-inr.html" => "Flowchart Gestione INR",
                "infografica-tao.html" => "Infografica Gestione TAO",
                "linee-guida-tao.html" => "Guida alla TAO con Warfarin per MMG",
                _ => "Guida Professionale"
            };

            // Crea il ViewModel e il Dialog
            var guideViewModel = _serviceProvider.GetRequiredService<GuideViewModel>();
            guideViewModel.Initialize(fileName, title);

            var dialog = new GuideDialog(guideViewModel);
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Errore nell'apertura della guida:\n{ex.Message}", "Errore");
        }
    }

    [RelayCommand]
    private void OpenPdfGuide(string pdfFileName)
    {
        try
        {
            // Costruisce il percorso completo del PDF
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var pdfPath = System.IO.Path.Combine(baseDirectory, "Resources", "Guides", pdfFileName);

            // Verifica che il file esista
            if (!System.IO.File.Exists(pdfPath))
            {
                _dialogService.ShowError($"File PDF non trovato:\n{pdfPath}", "Errore");
                return;
            }

            // Apre il PDF con l'applicazione predefinita
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Errore nell'apertura del PDF:\n{ex.Message}", "Errore");
        }
    }

    [RelayCommand]
    private async Task CheckUpdates()
    {
        try
        {
            var updateService = _serviceProvider.GetRequiredService<UpdateNotificationService>();
            await updateService.CheckForUpdatesAsync(showNoUpdateMessage: true);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Errore durante il controllo degli aggiornamenti:\n{ex.Message}", "Errore");
        }
    }

    [RelayCommand]
    private void ShowAbout()
    {
        var aboutDialog = new AboutDialog
        {
            Owner = Application.Current.MainWindow
        };
        aboutDialog.ShowDialog();
    }

    #endregion
}
