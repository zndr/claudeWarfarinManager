using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
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
    private DoctorData? _doctorData;

    [ObservableProperty]
    private string _fullName = string.Empty;

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

    public DoctorDataViewModel(WarfarinDbContext context, IDialogService dialogService)
    {
        _context = context;
        _dialogService = dialogService;
    }

    /// <summary>
    /// Carica i dati del medico dal database
    /// </summary>
    public async Task LoadDataAsync()
    {
        try
        {
            // Ci dovrebbe essere solo un record, prendiamo il primo
            _doctorData = await _context.DoctorData.FirstOrDefaultAsync();

            if (_doctorData != null)
            {
                FullName = _doctorData.FullName;
                Street = _doctorData.Street;
                PostalCode = _doctorData.PostalCode;
                City = _doctorData.City;
                Phone = _doctorData.Phone;
                Email = _doctorData.Email;
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Errore durante il caricamento dei dati: {ex.Message}", "Errore");
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

            if (_doctorData == null)
            {
                // Crea nuovo record
                _doctorData = new DoctorData();
                _context.DoctorData.Add(_doctorData);
            }

            // Aggiorna i valori
            _doctorData.FullName = FullName;
            _doctorData.Street = Street;
            _doctorData.PostalCode = PostalCode;
            _doctorData.City = City;
            _doctorData.Phone = Phone;
            _doctorData.Email = Email;

            await _context.SaveChangesAsync();

            _dialogService.ShowInformation("Dati del medico salvati correttamente", "Successo");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Errore durante il salvataggio: {ex.Message}", "Errore");
        }
    }
}
