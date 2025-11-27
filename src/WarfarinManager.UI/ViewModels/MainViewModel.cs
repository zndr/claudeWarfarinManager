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

    public MainViewModel(INavigationService navigationService, IDialogService dialogService, IServiceProvider serviceProvider)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
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
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Cerca paziente");
    }

    [RelayCommand]
    private void ShowPatientStatistics()
    {
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Statistiche pazienti");
    }

    #endregion

    #region Strumenti Menu Commands

    [RelayCommand]
    private void ShowTTRCalculator()
    {
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Calcolatore TTR");
    }

    [RelayCommand]
    private void ShowInteractionChecker()
    {
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Verifica interazioni");
    }

    [RelayCommand]
    private void ShowBridgeTherapy()
    {
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Bridge Therapy");
    }

    [RelayCommand]
    private void ShowDrugDatabase()
    {
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Database farmaci");
    }

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
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Impostazioni database");
    }

    #endregion

    #region Aiuto Menu Commands

    [RelayCommand]
    private void ShowUserGuide()
    {
        _dialogService.ShowInformation("Funzionalità in fase di sviluppo", "Guida utente");
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
