using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private void ShowProfessionalGuide(string fileName)
    {
        try
        {
            // Determina il titolo della guida in base al file
            var title = fileName switch
            {
                "index.html" => "Indice Guide Professionali",
                "interactions.html" => "Interazioni Farmacologiche Warfarin",
                "algoritmo-gestione-inr.html" => "Flowchart Gestione INR",
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
    private void ShowGuideline(string guidelineType)
    {
        var message = guidelineType switch
        {
            "FCSA2016" => "Linee guida FCSA 2016\n\nFunzionalità in fase di sviluppo",
            "ESC2020" => "Linee guida ESC 2020\n\nFunzionalità in fase di sviluppo",
            "CHEST" => "Linee guida CHEST\n\nFunzionalità in fase di sviluppo",
            "Perioperative" => "Gestione perioperatoria\n\nFunzionalità in fase di sviluppo",
            _ => "Linee guida professionali\n\nFunzionalità in fase di sviluppo"
        };
        _dialogService.ShowInformation(message, "Linee guida");
    }

    [RelayCommand]
    private void CheckUpdates()
    {
        _dialogService.ShowInformation("Nessun aggiornamento disponibile.\n\nVersione corrente: 1.0.0.0", "Verifica aggiornamenti");
    }

    [RelayCommand]
    private void ShowAbout()
    {
        var message = "TaoGEST - Gestione Terapia Anticoagulante Orale\n\n" +
                     "Versione 1.0.0.0 - Beta\n\n" +
                     "Sistema di gestione per la terapia anticoagulante orale con Warfarin\n\n" +
                     "© 2024 - Tutti i diritti riservati";
        _dialogService.ShowInformation(message, "About TaoGEST");
    }

    #endregion
}
