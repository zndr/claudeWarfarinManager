using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace WarfarinManager.UI.Services;

/// <summary>
/// Implementazione del servizio di navigazione basato su Dependency Injection
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<object> _navigationHistory = new();
    private object? _currentView;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object? CurrentView
    {
        get => _currentView;
        private set
        {
            _currentView = value;
            CurrentViewChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? CurrentViewChanged;

    public bool CanGoBack => _navigationHistory.Count > 0;

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        NavigateTo<TViewModel>(null);
    }

    public void NavigateTo<TViewModel>(object? parameter) where TViewModel : class
    {
        // Salva la view corrente nella history (se esiste)
        if (CurrentView != null)
        {
            _navigationHistory.Push(CurrentView);
        }

        // Risolvi il ViewModel dal DI container
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        // Se il ViewModel implementa INavigationAware, passa il parametro
        if (viewModel is INavigationAware navigationAware && parameter != null)
        {
            navigationAware.OnNavigatedTo(parameter);
        }

        CurrentView = viewModel;
    }

    public void GoBack()
    {
        if (!CanGoBack)
            return;

        CurrentView = _navigationHistory.Pop();
    }
}

/// <summary>
/// Interfaccia opzionale per ViewModel che necessitano di gestire parametri di navigazione
/// </summary>
public interface INavigationAware
{
    /// <summary>
    /// Chiamato quando si naviga verso questo ViewModel
    /// </summary>
    /// <param name="parameter">Parametro passato durante la navigazione</param>
    void OnNavigatedTo(object parameter);
}
