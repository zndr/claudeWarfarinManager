using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
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
    private static Mutex? _instanceMutex;

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
        services.AddScoped<PengoNomogramService>();
        services.AddScoped<PostgreSqlImportService>();
        services.AddScoped<IMillepsDataService, MillepsDataService>();

        // DOAC Services
        services.AddScoped<IDOACInteractionService, DOACInteractionService>();
        services.AddScoped<IDOACClinicalService, DOACClinicalService>();
        services.AddScoped<IDOACPerioperativeService, DOACPerioperativeService>();

        // Document Library Service
        services.AddSingleton<IDocumentLibraryService, DocumentLibraryService>();

        // HttpClient per Update Checker
        services.AddHttpClient();

        // Update Checker Service
        services.AddSingleton<IUpdateCheckerService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<UpdateCheckerService>>();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();

            var timeoutSeconds = configuration.GetValue<int>("UpdateChecker:TimeoutSeconds", 30);
            httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

            var versionFileUrl = configuration.GetValue<string>("UpdateChecker:VersionFileUrl")
                ?? "https://raw.githubusercontent.com/zndr/claudeWarfarinManager/master/version.json";

            return new UpdateCheckerService(logger, httpClient, versionFileUrl);
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
        services.AddTransient<ImportPatientsViewModel>();
        services.AddTransient<DoacGestViewModel>();

        // Tools ViewModels
        services.AddTransient<ViewModels.Tools.WeeklyDoseCalculatorViewModel>();
        services.AddTransient<ViewModels.Tools.InitialDosageEstimatorViewModel>();

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
        services.AddTransient<ImportPatientsDialog>();
        services.AddTransient<DoacGestView>();
        services.AddTransient<DoacGestWindow>();

        // Tools Views
        services.AddTransient<Views.Tools.WeeklyDoseCalculatorDialog>();
        services.AddTransient<Views.Tools.InitialDosageEstimatorDialog>();
        services.AddTransient<Views.Tools.DoacGestWebViewWindow>();

        // Dialog Views
        services.AddTransient<Views.Dialogs.DocumentBrowserDialog>();

        // Main Window
        services.AddSingleton<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Controlla se un'altra istanza dell'applicazione è già in esecuzione
        const string mutexName = "Global\\TaoGEST_SingleInstance_Mutex_5E7F8A9B";
        bool createdNew;

        try
        {
            _instanceMutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                // Un'altra istanza è già in esecuzione
                Log.Information("Tentativo di avviare una seconda istanza dell'applicazione - bloccato");
                MessageBox.Show(
                    "TaoGEST è già in esecuzione.\n\nÈ possibile avere una sola istanza dell'applicazione alla volta.",
                    "TaoGEST già in esecuzione",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Shutdown();
                return;
            }

            Log.Information("Istanza singola acquisita correttamente");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Errore durante il controllo dell'istanza singola");
            // Continua comunque l'avvio in caso di errore nel mutex
        }

        try
        {
            await _host.StartAsync();

            // Inizializza il sistema di temi
            ThemeManager.Instance.Initialize();

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

            // Prima di applicare le migrazioni, correggi eventuali problemi di schema
            // per database provenienti da versioni precedenti (es. 1.0.0)
            await FixLegacyDatabaseSchemaAsync(context);

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

    /// <summary>
    /// Corregge problemi di schema in database provenienti da versioni precedenti.
    /// Aggiunge colonne mancanti che potrebbero causare errori durante le operazioni.
    /// IMPORTANTE: Questo metodo viene eseguito PRIMA di MigrateAsync() per prevenire errori
    /// durante l'applicazione delle migrazioni su database di versioni precedenti.
    /// </summary>
    private async System.Threading.Tasks.Task FixLegacyDatabaseSchemaAsync(WarfarinDbContext context)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            Log.Information("Inizio verifica e correzione schema database legacy");

            // =========================================================================
            // TABELLA: AdverseEvents
            // =========================================================================
            await EnsureColumnExistsAsync(connection, "AdverseEvents", "CertaintyLevel",
                "TEXT NOT NULL DEFAULT 'Possible'");

            // =========================================================================
            // TABELLA: DoctorData
            // =========================================================================
            // FiscalCode: Aggiunto nella versione 1.X.X per supportare l'importazione pazienti
            await EnsureColumnExistsAsync(connection, "DoctorData", "FiscalCode",
                "TEXT NOT NULL DEFAULT ''");

            // =========================================================================
            // TABELLA: Patients - Campi biometrici (v1.4.0)
            // =========================================================================
            await EnsureColumnExistsAsync(connection, "Patients", "Weight",
                "TEXT NULL");
            await EnsureColumnExistsAsync(connection, "Patients", "Height",
                "TEXT NULL");
            await EnsureColumnExistsAsync(connection, "Patients", "WeightLastUpdated",
                "TEXT NULL");
            await EnsureColumnExistsAsync(connection, "Patients", "HeightLastUpdated",
                "TEXT NULL");

            // =========================================================================
            // TABELLA: DoacMonitoring (nuova per modulo DOAC)
            // =========================================================================
            await EnsureDoacMonitoringTableExistsAsync(connection);

            // =========================================================================
            // TABELLA: TerapieContinuative (nuova per modulo DOAC)
            // =========================================================================
            await EnsureTerapieContinuativeTableExistsAsync(connection);

            await connection.CloseAsync();
            Log.Information("Verifica e correzione schema database legacy completata");
        }
        catch (Exception ex)
        {
            // Non bloccare l'avvio dell'app se questa correzione fallisce
            // Le migrazioni standard potrebbero gestirlo comunque
            Log.Warning(ex, "Impossibile verificare/correggere completamente lo schema legacy del database. Le migrazioni standard potrebbero gestirlo.");
        }
    }

    /// <summary>
    /// Verifica se una colonna esiste in una tabella e la aggiunge se mancante.
    /// Metodo generico riutilizzabile per tutte le correzioni di schema.
    /// </summary>
    /// <param name="connection">Connessione al database aperta</param>
    /// <param name="tableName">Nome della tabella</param>
    /// <param name="columnName">Nome della colonna</param>
    /// <param name="columnDefinition">Definizione SQL della colonna (es. "TEXT NOT NULL DEFAULT ''")</param>
    private async System.Threading.Tasks.Task EnsureColumnExistsAsync(
        System.Data.Common.DbConnection connection,
        string tableName,
        string columnName,
        string columnDefinition)
    {
        try
        {
            // Verifica se la tabella esiste
            using var checkTableCommand = connection.CreateCommand();
            checkTableCommand.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
            var tableExists = await checkTableCommand.ExecuteScalarAsync() != null;

            if (!tableExists)
            {
                Log.Debug("Tabella {TableName} non esiste ancora - verrà creata dalle migrazioni", tableName);
                return;
            }

            // Verifica se la colonna esiste
            using var checkColumnCommand = connection.CreateCommand();
            checkColumnCommand.CommandText = $"PRAGMA table_info({tableName});";
            var hasColumn = false;

            using (var reader = await checkColumnCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var existingColumnName = reader.GetString(1); // La colonna 'name' è all'indice 1
                    if (existingColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        hasColumn = true;
                        break;
                    }
                }
            }

            if (!hasColumn)
            {
                Log.Warning("Colonna {ColumnName} mancante nella tabella {TableName}. Aggiunta in corso...",
                    columnName, tableName);

                // Aggiungi la colonna mancante
                using var addColumnCommand = connection.CreateCommand();
                addColumnCommand.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};";
                await addColumnCommand.ExecuteNonQueryAsync();

                Log.Information("Colonna {ColumnName} aggiunta con successo alla tabella {TableName}",
                    columnName, tableName);
            }
            else
            {
                Log.Debug("Colonna {ColumnName} già presente nella tabella {TableName}",
                    columnName, tableName);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Errore durante la verifica/aggiunta della colonna {ColumnName} nella tabella {TableName}",
                columnName, tableName);
            // Non rilanciare l'eccezione - lascia che le migrazioni standard tentino di gestirla
        }
    }

    /// <summary>
    /// Crea la tabella DoacMonitoring se non esiste
    /// </summary>
    private async System.Threading.Tasks.Task EnsureDoacMonitoringTableExistsAsync(
        System.Data.Common.DbConnection connection)
    {
        try
        {
            using var checkTableCommand = connection.CreateCommand();
            checkTableCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='DoacMonitoring';";
            var tableExists = await checkTableCommand.ExecuteScalarAsync() != null;

            if (!tableExists)
            {
                Log.Information("Creazione tabella DoacMonitoring...");

                using var createCommand = connection.CreateCommand();
                createCommand.CommandText = @"
                    CREATE TABLE DoacMonitoring (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PatientId INTEGER NOT NULL,
                        DataRilevazione TEXT NOT NULL,
                        Peso TEXT,
                        Creatinina TEXT,
                        Emoglobina TEXT,
                        Ematocrito TEXT,
                        Piastrine INTEGER,
                        AST INTEGER,
                        ALT INTEGER,
                        Bilirubina TEXT,
                        GGT TEXT,
                        CrCl_Cockroft INTEGER,
                        CrCl_Calcolato INTEGER NOT NULL DEFAULT 1,
                        DoacSelezionato TEXT,
                        Indicazione TEXT,
                        Ipertensione INTEGER NOT NULL DEFAULT 0,
                        DisfunzioneRenale INTEGER NOT NULL DEFAULT 0,
                        DisfunzioneEpatica INTEGER NOT NULL DEFAULT 0,
                        Cirrosi INTEGER NOT NULL DEFAULT 0,
                        IpertensPortale INTEGER NOT NULL DEFAULT 0,
                        StoriaStroke INTEGER NOT NULL DEFAULT 0,
                        StoriaSanguinamento INTEGER NOT NULL DEFAULT 0,
                        INRLabile INTEGER NOT NULL DEFAULT 0,
                        Antiaggreganti INTEGER NOT NULL DEFAULT 0,
                        FANS INTEGER NOT NULL DEFAULT 0,
                        AbusoDiAlcol INTEGER NOT NULL DEFAULT 0,
                        HasBledScore INTEGER NOT NULL DEFAULT 0,
                        DosaggioSuggerito TEXT,
                        RazionaleDosaggio TEXT,
                        IntervalloControlloMesi INTEGER,
                        DataProssimoControllo TEXT,
                        CreatedAt TEXT NOT NULL,
                        UpdatedAt TEXT NOT NULL,
                        FOREIGN KEY (PatientId) REFERENCES Patients(Id) ON DELETE CASCADE
                    );
                    CREATE INDEX IX_DoacMonitoring_PatientId_DataRilevazione ON DoacMonitoring(PatientId, DataRilevazione DESC);
                    CREATE INDEX IX_DoacMonitoring_DataProssimoControllo ON DoacMonitoring(DataProssimoControllo) WHERE DataProssimoControllo IS NOT NULL;
                ";
                await createCommand.ExecuteNonQueryAsync();

                Log.Information("Tabella DoacMonitoring creata con successo");
            }
            else
            {
                Log.Debug("Tabella DoacMonitoring già esistente");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Errore durante la creazione della tabella DoacMonitoring");
        }
    }

    /// <summary>
    /// Crea la tabella TerapieContinuative se non esiste
    /// </summary>
    private async System.Threading.Tasks.Task EnsureTerapieContinuativeTableExistsAsync(
        System.Data.Common.DbConnection connection)
    {
        try
        {
            using var checkTableCommand = connection.CreateCommand();
            checkTableCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='TerapieContinuative';";
            var tableExists = await checkTableCommand.ExecuteScalarAsync() != null;

            if (!tableExists)
            {
                Log.Information("Creazione tabella TerapieContinuative...");

                using var createCommand = connection.CreateCommand();
                createCommand.CommandText = @"
                    CREATE TABLE TerapieContinuative (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PatientId INTEGER NOT NULL,
                        Classe TEXT NOT NULL,
                        PrincipioAttivo TEXT NOT NULL,
                        NomeCommerciale TEXT,
                        Dosaggio TEXT,
                        FrequenzaGiornaliera TEXT,
                        ViaAssunzione TEXT,
                        DataInizio TEXT,
                        DataFine TEXT,
                        Attiva INTEGER NOT NULL DEFAULT 1,
                        Indicazione TEXT,
                        Note TEXT,
                        MotivoSospensione TEXT,
                        Importato INTEGER NOT NULL DEFAULT 0,
                        FonteDati TEXT,
                        CreatedAt TEXT NOT NULL,
                        UpdatedAt TEXT NOT NULL,
                        FOREIGN KEY (PatientId) REFERENCES Patients(Id) ON DELETE CASCADE
                    );
                    CREATE INDEX IX_TerapieContinuative_PatientId ON TerapieContinuative(PatientId);
                    CREATE INDEX IX_TerapieContinuative_Attiva_PatientId ON TerapieContinuative(Attiva, PatientId);
                    CREATE INDEX IX_TerapieContinuative_Classe_PatientId_Attiva ON TerapieContinuative(Classe, PatientId, Attiva);
                ";
                await createCommand.ExecuteNonQueryAsync();

                Log.Information("Tabella TerapieContinuative creata con successo");
            }
            else
            {
                Log.Debug("Tabella TerapieContinuative già esistente");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Errore durante la creazione della tabella TerapieContinuative");
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

            // Rilascia il mutex dell'istanza singola
            _instanceMutex?.ReleaseMutex();
            _instanceMutex?.Dispose();

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
