using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per la gestione dei dati del medico
/// </summary>
public partial class DoctorDataViewModel : ObservableObject
{
    private readonly WarfarinDbContext _context;
    private readonly IDialogService _dialogService;
    private readonly IMillepsDataService _millepsDataService;
    private readonly IMillewinIntegrationService _millewinIntegrationService;
    private DoctorData? _doctorData;
    private bool _isLoadingData;

    /// <summary>
    /// Evento che viene scatenato quando il salvataggio è completato con successo
    /// </summary>
    public event EventHandler? SaveCompleted;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _fiscalCode = string.Empty;

    [ObservableProperty]
    private string _street = string.Empty;

    [ObservableProperty]
    private string _postalCode = string.Empty;

    [ObservableProperty]
    private string _city = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string? _millewinCode;

    [ObservableProperty]
    private bool _isLookingUpMillewinCode;

    public DoctorDataViewModel(
        WarfarinDbContext context,
        IDialogService dialogService,
        IMillepsDataService millepsDataService,
        IMillewinIntegrationService millewinIntegrationService)
    {
        _context = context;
        _dialogService = dialogService;
        _millepsDataService = millepsDataService;
        _millewinIntegrationService = millewinIntegrationService;
    }

    /// <summary>
    /// Quando il codice fiscale cambia, cerca il codice Millewin corrispondente
    /// </summary>
    partial void OnFiscalCodeChanged(string value)
    {
        // Non cercare durante il caricamento iniziale
        if (_isLoadingData) return;

        // Cerca il codice Millewin solo se il CF ha 16 caratteri
        if (!string.IsNullOrWhiteSpace(value) && value.Length == 16)
        {
            _ = LookupMillewinCodeAsync(value);
        }
        else
        {
            MillewinCode = null;
        }
    }

    /// <summary>
    /// Cerca il codice Millewin dal codice fiscale
    /// </summary>
    private async Task LookupMillewinCodeAsync(string fiscalCode)
    {
        // Verifica se l'integrazione è abilitata
        if (!_millewinIntegrationService.IsIntegrationEnabled)
        {
            MillewinCode = null;
            return;
        }

        try
        {
            IsLookingUpMillewinCode = true;
            var code = await _millepsDataService.GetMillepsDoctorCodeAsync(fiscalCode);
            MillewinCode = code;

            if (string.IsNullOrEmpty(code))
            {
                _dialogService.ShowWarning(
                    $"Il Codice Fiscale '{fiscalCode}' non corrisponde a nessun medico registrato in Millewin.\n\n" +
                    "Verificare che:\n" +
                    "- Il codice fiscale sia corretto\n" +
                    "- Il medico sia registrato nel database Millewin",
                    "Medico non trovato");
            }
        }
        catch (Exception ex)
        {
            MillewinCode = null;
            _dialogService.ShowError(
                $"Errore durante la ricerca del codice Millewin:\n{ex.Message}",
                "Errore connessione");
        }
        finally
        {
            IsLookingUpMillewinCode = false;
        }
    }

    /// <summary>
    /// Carica i dati del medico dal database
    /// </summary>
    public async Task LoadDataAsync()
    {
        try
        {
            _isLoadingData = true;

            // Ci dovrebbe essere solo un record, prendiamo il primo
            _doctorData = await _context.DoctorData.FirstOrDefaultAsync();

            if (_doctorData != null)
            {
                FullName = _doctorData.FullName;
                FiscalCode = _doctorData.FiscalCode;
                Street = _doctorData.Street;
                PostalCode = _doctorData.PostalCode;
                City = _doctorData.City;
                Phone = _doctorData.Phone;
                Email = _doctorData.Email;
                MillewinCode = _doctorData.MillewinCode;
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Errore durante il caricamento dei dati: {ex.Message}", "Errore");
        }
        finally
        {
            _isLoadingData = false;
        }
    }

    /// <summary>
    /// Salva i dati del medico
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            // Validazione base
            if (string.IsNullOrWhiteSpace(FullName))
            {
                _dialogService.ShowWarning("Il campo 'Cognome e nome' è obbligatorio", "Validazione");
                return;
            }

            if (string.IsNullOrWhiteSpace(FiscalCode))
            {
                _dialogService.ShowWarning("Il campo 'Codice Fiscale' è obbligatorio", "Validazione");
                return;
            }

            if (FiscalCode.Length != 16)
            {
                _dialogService.ShowWarning("Il Codice Fiscale deve essere di 16 caratteri", "Validazione");
                return;
            }

            if (_doctorData == null)
            {
                // Crea nuovo record
                _doctorData = new DoctorData();
                _context.DoctorData.Add(_doctorData);
            }

            // Aggiorna i valori
            _doctorData.FullName = FullName;
            _doctorData.FiscalCode = FiscalCode.ToUpperInvariant();
            _doctorData.Street = Street;
            _doctorData.PostalCode = PostalCode;
            _doctorData.City = City;
            _doctorData.Phone = Phone;
            _doctorData.Email = Email;
            _doctorData.MillewinCode = MillewinCode;

            await _context.SaveChangesAsync();

            _dialogService.ShowInformation("Dati del medico salvati correttamente", "Successo");

            // Notifica che il salvataggio è completato con successo
            SaveCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Errore durante il salvataggio: {ex.Message}", "Errore");
        }
    }
}
