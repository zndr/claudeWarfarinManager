using System;

namespace WarfarinManager.UI.Services;

/// <summary>
/// Servizio di navigazione per gestire il routing tra le view dell'applicazione
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// View corrente visualizzata
    /// </summary>
    object? CurrentView { get; }

    /// <summary>
    /// Evento scatenato quando la view corrente cambia
    /// </summary>
    event EventHandler? CurrentViewChanged;

    /// <summary>
    /// Naviga verso una view specificata dal ViewModel type
    /// </summary>
    /// <typeparam name="TViewModel">Tipo del ViewModel di destinazione</typeparam>
    void NavigateTo<TViewModel>() where TViewModel : class;

    /// <summary>
    /// Naviga verso una view specificata dal ViewModel type con parametro
    /// </summary>
    /// <typeparam name="TViewModel">Tipo del ViewModel di destinazione</typeparam>
    /// <param name="parameter">Parametro da passare al ViewModel (es. ID paziente)</param>
    void NavigateTo<TViewModel>(object? parameter) where TViewModel : class;

    /// <summary>
    /// Torna alla view precedente
    /// </summary>
    void GoBack();

    /// <summary>
    /// Indica se è possibile tornare indietro
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Naviga alla lista pazienti
    /// </summary>
    void NavigateToPatientList();

    /// <summary>
    /// Naviga al form nuovo paziente
    /// </summary>
    void NavigateToNewPatient();
}
