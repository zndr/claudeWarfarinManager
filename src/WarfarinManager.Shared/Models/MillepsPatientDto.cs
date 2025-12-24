using System.ComponentModel;

namespace WarfarinManager.Shared.Models;

/// <summary>
/// DTO per i dati paziente importati dal database PostgreSQL Milleps
/// </summary>
public class MillepsPatientDto : INotifyPropertyChanged
{
    private bool _isSelected;

    /// <summary>
    /// Indica se il paziente è selezionato per l'importazione
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    /// <summary>
    /// Cognome del paziente
    /// </summary>
    public string Cognome { get; set; } = string.Empty;

    /// <summary>
    /// Nome del paziente
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Data di nascita
    /// </summary>
    public DateTime? Nascita { get; set; }

    /// <summary>
    /// Codice fiscale del paziente
    /// </summary>
    public string CodiceFiscale { get; set; } = string.Empty;

    /// <summary>
    /// Numero di telefono cellulare
    /// </summary>
    public string? TelCell { get; set; }

    /// <summary>
    /// Indirizzo email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Tipo di anticoagulante (warfarin, dabigatran, rivaroxaban, apixaban, edoxaban)
    /// </summary>
    public string? Anticoagulante { get; set; }

    /// <summary>
    /// Data inizio terapia
    /// </summary>
    public DateTime? DataInizio { get; set; }

    /// <summary>
    /// Sesso del paziente ('M' o 'F')
    /// </summary>
    public string? Sesso { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
}
