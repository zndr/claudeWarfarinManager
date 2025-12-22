using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
using WarfarinManager.UI.Views.Dialogs;

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
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services, context.Configuration);
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

    private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
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
        services.AddScoped<IInteractionDrugRepository, InteractionDrugRepository>();

        // Core Business Services
        services.AddScoped<IInteractionCheckerService, InteractionCheckerService>();
        services.AddScoped<IDosageCalculatorService, DosageCalculatorService>();
        services.AddScoped<ITTRCalculatorService, TTRCalculatorService>();
        services.AddScoped<IBridgeTherapyService, BridgeTherapyService>();
        services.AddScoped<ISwitchCalculatorService, SwitchCalculatorService>();

        // Update Checker Service
        services.AddSingleton<IUpdateCheckerService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<UpdateCheckerService>>();
            var ftpHost = configuration.GetValue<string>("UpdateChecker:FtpHost") ?? "";
            var ftpUsername = configuration.GetValue<string>("UpdateChecker:FtpUsername") ?? "";
            var ftpPassword = configuration.GetValue<string>("UpdateChecker:FtpPassword") ?? "";
            var versionFileName = configuration.GetValue<string>("UpdateChecker:VersionFileName") ?? "version.json";
            var timeoutSeconds = configuration.GetValue<int>("UpdateChecker:TimeoutSeconds", 30);

            return new UpdateCheckerService(logger, ftpHost, ftpUsername, ftpPassword, versionFileName, timeoutSeconds);
        });

        // UI Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddScoped<IDatabaseService, DatabaseService>();
        services.AddScoped<PatientSummaryPdfService>();
        services.AddScoped<BridgeTherapyPdfService>();
        services.AddScoped<WeeklySchedulePdfService>();
        services.AddSingleton<UpdateNotificationService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<PatientListViewModel>();
        services.AddTransient<PatientFormViewModel>();
        services.AddTransient<PatientDetailsViewModel>();
        services.AddTransient<IndicationFormViewModel>();
        services.AddTransient<INRControlViewModel>();
        services.AddTransient<MedicationsViewModel>();
        services.AddTransient<BridgeTherapyViewModel>();
        services.AddTransient<INRHistoryViewModel>();
        services.AddTransient<PreTaoAssessmentViewModel>();
        services.AddTransient<PatientSummaryViewModel>();
        services.AddTransient<AdverseEventsViewModel>();
        services.AddTransient<DoctorDataViewModel>();
        services.AddTransient<DatabaseManagementViewModel>();
        services.AddTransient<GuideViewModel>();
        services.AddTransient<SwitchTherapyViewModel>();
        services.AddTransient<NewPatientWizardViewModel>();

        // Views
        services.AddTransient<PatientListView>();
        services.AddTransient<PatientFormView>();
        services.AddTransient<PatientDetailsView>();
        services.AddTransient<PatientSummaryView>();
        services.AddTransient<IndicationFormView>();
        services.AddTransient<INRControlView>();
        services.AddTransient<MedicationsView>();
        services.AddTransient<INRHistoryView>();
        services.AddTransient<PreTaoAssessmentView>();
        services.AddTransient<PreTaoAssessmentSummary>();
        services.AddTransient<PreTaoAssessmentDialog>();
        services.AddTransient<AdverseEventsView>();
        services.AddTransient<DoctorDataDialog>();
        services.AddTransient<DatabaseManagementDialog>();
        services.AddTransient<NewPatientWizardView>();

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

            // Avvia il servizio di controllo aggiornamenti
            var updateService = _host.Services.GetRequiredService<UpdateNotificationService>();
            updateService.Start();

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
