using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Core.Services;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Repositories;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.UI.Services;
using WarfarinManager.UI.ViewModels;
using WarfarinManager.UI.Views.Dashboard;
using WarfarinManager.UI.Views.Patient;
using WarfarinManager.UI.Views.INR;

namespace WarfarinManager.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        // Configura Serilog
        ConfigureLogging();

        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            })
            .Build();
    }

    private void ConfigureLogging()
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WarfarinManager",
            "Logs",
            "app.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("WarfarinManager Pro avviato");
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Database
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WarfarinManager",
            "warfarin.db");

        services.AddDbContext<WarfarinDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Repositories & Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Core Business Services
        services.AddScoped<IDosageCalculatorService, DosageCalculatorService>();
        services.AddScoped<ITTRCalculatorService, TTRCalculatorService>();
        services.AddScoped<IInteractionCheckerService, InteractionCheckerService>();

        // UI Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<PatientListViewModel>();
        services.AddTransient<PatientFormViewModel>();
        services.AddTransient<PatientDetailsViewModel>();
        services.AddTransient<IndicationFormViewModel>();
        services.AddTransient<INRControlViewModel>();

        // Views
        services.AddTransient<PatientListView>();
        services.AddTransient<PatientFormView>();
        services.AddTransient<PatientDetailsView>();
        services.AddTransient<IndicationFormView>();
        services.AddTransient<INRControlView>();

        // Main Window
        services.AddSingleton<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            await _host.StartAsync();

            // Assicura che il database esista e sia aggiornato
            await EnsureDatabaseAsync();

            // Ottieni MainWindow dal DI container
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Errore critico durante l'avvio dell'applicazione");
            MessageBox.Show(
                $"Errore durante l'avvio dell'applicazione:\n{ex.Message}",
                "Errore Critico",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    private async System.Threading.Tasks.Task EnsureDatabaseAsync()
    {
        try
        {
            using var scope = _host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WarfarinDbContext>();

            // Crea il database se non esiste e applica migrations
            await context.Database.MigrateAsync();

            Log.Information("Database inizializzato correttamente");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Errore durante l'inizializzazione del database");
            throw;
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            using (_host)
            {
                await _host.StopAsync();
            }

            Log.Information("WarfarinManager Pro chiuso");
            Log.CloseAndFlush();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Errore durante la chiusura dell'applicazione");
        }

        base.OnExit(e);
    }
}
