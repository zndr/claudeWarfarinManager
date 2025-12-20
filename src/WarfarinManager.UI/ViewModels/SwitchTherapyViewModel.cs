using System.IO;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using WarfarinManager.Core.Models;
using WarfarinManager.Core.Services;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per la gestione dello Switch Terapeutico tra Warfarin e DOAC
/// </summary>
public partial class SwitchTherapyViewModel : ObservableObject
{
    private readonly ISwitchCalculatorService _switchCalculatorService;
    private readonly WarfarinDbContext _dbContext;
    private readonly ILogger<SwitchTherapyViewModel> _logger;

    [ObservableProperty]
    private string _guidePath = string.Empty;

    [ObservableProperty]
    private string _title = "Switch Terapia Anticoagulante";

    [ObservableProperty]
    private int? _currentPatientId;

    [ObservableProperty]
    private bool _hasActiveSwitches = false;

    [ObservableProperty]
    private string _tabHeader = "🔄 Switch Terapia";

    private CoreWebView2? _webView;

    public SwitchTherapyViewModel(
        ISwitchCalculatorService switchCalculatorService,
        WarfarinDbContext dbContext,
        ILogger<SwitchTherapyViewModel> logger)
    {
        _switchCalculatorService = switchCalculatorService ?? throw new ArgumentNullException(nameof(switchCalculatorService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InitializeGuidePath();
    }

    private void InitializeGuidePath()
    {
        try
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var guidesPath = Path.Combine(baseDirectory, "Resources", "Guides");
            var fullPath = Path.Combine(guidesPath, "switch-therapy.html");

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning($"Switch therapy guide not found: {fullPath}");
                throw new FileNotFoundException("Switch therapy guide HTML file not found", fullPath);
            }

            GuidePath = new Uri(fullPath).AbsoluteUri;
            _logger.LogInformation($"Switch therapy guide loaded: {GuidePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing switch therapy guide path");
            throw;
        }
    }

    /// <summary>
    /// Inizializza il WebView2 e registra gli handler JavaScript
    /// </summary>
    public async Task InitializeWebViewAsync(CoreWebView2 webView)
    {
        _webView = webView;

        try
        {
            // Crea un oggetto host per esporre metodi C# al JavaScript
            var switchService = new SwitchServiceBridge(this);
            _webView.AddHostObjectToScript("switchService", switchService);

            _logger.LogInformation("WebView2 initialized for Switch Therapy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing WebView2");
            throw;
        }
    }

    /// <summary>
    /// Imposta il paziente corrente per cui eseguire lo switch
    /// </summary>
    public void SetCurrentPatient(int? patientId)
    {
        CurrentPatientId = patientId;
    }

    /// <summary>
    /// Aggiorna i dati del paziente nel form (chiamato dopo il caricamento della pagina)
    /// </summary>
    public async void RefreshPatientData()
    {
        // Se c'è un paziente selezionato, pre-compila i dati nel form HTML
        if (CurrentPatientId.HasValue && _webView != null)
        {
            var patient = _dbContext.Patients.Find(CurrentPatientId.Value);
            if (patient != null)
            {
                await PreFillPatientDataAsync(patient);
                // Aspetta che il pre-fill sia completato prima di caricare lo storico
                await Task.Delay(500);
                LoadSwitchHistory();
            }
        }
    }

    /// <summary>
    /// Carica lo storico degli switch del paziente corrente
    /// </summary>
    public async void LoadSwitchHistory()
    {
        try
        {
            if (!CurrentPatientId.HasValue || _webView == null)
            {
                _logger.LogWarning("Cannot load switch history: no patient or webview not ready");
                return;
            }

            _logger.LogInformation($"Loading switch history for patient {CurrentPatientId}");

            var switches = _dbContext.TherapySwitches
                .Where(s => s.PatientId == CurrentPatientId.Value)
                .OrderByDescending(s => s.SwitchDate)
                .Select(s => new
                {
                    s.Id,
                    SwitchDate = s.SwitchDate.ToString("dd/MM/yyyy"),
                    s.Direction,
                    s.DoacType,
                    s.WarfarinType,
                    s.RecommendedDosage,
                    s.DosageRationale,
                    s.InrAtSwitch,
                    s.CreatinineClearance,
                    s.AgeAtSwitch,
                    s.WeightAtSwitch,
                    s.ProtocolTimeline,
                    s.Contraindications,
                    s.Warnings,
                    s.ClinicalNotes,
                    s.MonitoringPlan,
                    FirstFollowUpDate = s.FirstFollowUpDate.HasValue ? s.FirstFollowUpDate.Value.ToString("dd/MM/yyyy") : null,
                    s.FollowUpCompleted,
                    s.FollowUpNotes,
                    s.SwitchCompleted,
                    CompletionDate = s.CompletionDate.HasValue ? s.CompletionDate.Value.ToString("dd/MM/yyyy") : null,
                    s.Outcome
                })
                .ToList();

            _logger.LogInformation($"Found {switches.Count} switch records in database");

            // Verifica se ci sono switch attivi (non completati)
            var hasActive = switches.Any(s => !s.SwitchCompleted);
            HasActiveSwitches = hasActive;
            TabHeader = hasActive ? "⚠️ Switch Terapia" : "🔄 Switch Terapia";
            _logger.LogInformation($"Active switches: {hasActive}, TabHeader updated to: {TabHeader}");

            var switchesJson = JsonSerializer.Serialize(switches, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Aspetta che il DOM sia completamente caricato
            await Task.Delay(1000);

            var script = $@"
                console.log('Attempting to load switch history...');
                if (typeof loadSwitchHistory === 'function') {{
                    console.log('loadSwitchHistory function found, calling with data');
                    loadSwitchHistory({switchesJson});
                }} else {{
                    console.error('loadSwitchHistory function not found!');
                }}
            ";

            // Esegui lo script sul thread UI
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await _webView.ExecuteScriptAsync(script);
            });

            _logger.LogInformation($"Switch history script executed: {switches.Count} records sent to UI");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading switch history");
        }
    }

    /// <summary>
    /// Aggiorna le note di follow-up per uno switch
    /// </summary>
    public async Task<bool> UpdateFollowUpAsync(int switchId, string followUpNotes, bool completed, string? outcome)
    {
        try
        {
            var therapySwitch = await _dbContext.TherapySwitches.FindAsync(switchId);
            if (therapySwitch == null)
            {
                _logger.LogWarning($"Switch {switchId} not found");
                return false;
            }

            therapySwitch.FollowUpNotes = followUpNotes;
            therapySwitch.FollowUpCompleted = completed;

            if (completed && !therapySwitch.SwitchCompleted)
            {
                therapySwitch.SwitchCompleted = true;
                therapySwitch.CompletionDate = DateTime.Now;
                therapySwitch.Outcome = outcome ?? "Completato";
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Follow-up updated for switch {switchId}");

            // Ricarica lo storico
            LoadSwitchHistory();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating follow-up");
            return false;
        }
    }

    /// <summary>
    /// Elimina un protocollo di switch
    /// </summary>
    public async Task<bool> DeleteSwitchAsync(int switchId)
    {
        try
        {
            var therapySwitch = await _dbContext.TherapySwitches.FindAsync(switchId);
            if (therapySwitch == null)
            {
                _logger.LogWarning($"Switch {switchId} not found");
                return false;
            }

            _dbContext.TherapySwitches.Remove(therapySwitch);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Switch {switchId} deleted successfully");

            // Ricarica lo storico
            LoadSwitchHistory();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting switch {switchId}");
            return false;
        }
    }

    private async Task PreFillPatientDataAsync(Patient patient)
    {
        try
        {
            // Pre-compila età e sesso dall'anagrafica
            // Converti l'enum Gender in "M" o "F" per il select HTML
            var genderValue = "";
            if (patient.Gender.HasValue)
            {
                genderValue = patient.Gender.Value.ToString().ToUpper().Substring(0, 1); // "Male" -> "M", "Female" -> "F"
            }

            _logger.LogInformation($"Pre-filling patient data: Age={patient.Age}, Gender={patient.Gender}, GenderValue={genderValue}");

            // Ritarda l'esecuzione per dare tempo al WebView2 di caricare completamente
            await Task.Delay(500);

            var script = $@"
                (function() {{
                    console.log('PreFilling patient data: Age={patient.Age}, Gender={genderValue}');

                    // Funzione per aspettare che un elemento sia disponibile
                    function waitForElement(selector, callback, maxAttempts = 20) {{
                        let attempts = 0;
                        const interval = setInterval(() => {{
                            const element = document.getElementById(selector);
                            attempts++;

                            if (element) {{
                                clearInterval(interval);
                                callback(element);
                            }} else if (attempts >= maxAttempts) {{
                                clearInterval(interval);
                                console.error('Element not found after ' + maxAttempts + ' attempts: ' + selector);
                            }}
                        }}, 100);
                    }}

                    // Pre-compila età
                    waitForElement('age', (el) => {{
                        el.value = '{patient.Age}';
                        el.readOnly = true;
                        el.style.backgroundColor = '#fef3c7';
                        el.style.borderColor = '#fbbf24';
                        el.style.color = '#92400e';
                        el.style.fontWeight = '600';
                        console.log('Age field populated: ' + el.value);
                    }});

                    // Pre-compila sesso
                    waitForElement('gender', (el) => {{
                        el.value = '{genderValue}';
                        el.disabled = true;
                        el.style.backgroundColor = '#fef3c7';
                        el.style.borderColor = '#fbbf24';
                        el.style.color = '#92400e';
                        el.style.fontWeight = '600';
                        console.log('Gender field populated: ' + el.value);
                    }});
                }})();
            ";

            await _webView!.ExecuteScriptAsync(script);
            _logger.LogInformation($"Patient data pre-fill script executed: Age={patient.Age}, Gender={genderValue}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pre-filling patient data");
        }
    }

    /// <summary>
    /// Calcola il protocollo di switch
    /// </summary>
    public string CalculateProtocol(string parametersJson)
    {
        try
        {
            _logger.LogInformation("Calculating switch protocol with parameters: {Parameters}", parametersJson);

            // Deserializza i parametri dal JSON con opzioni case-insensitive
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var request = JsonSerializer.Deserialize<SwitchProtocolRequest>(parametersJson, options);
            if (request == null)
            {
                _logger.LogError("Failed to deserialize request JSON");
                throw new ArgumentException("Invalid parameters JSON");
            }

            _logger.LogInformation("Deserialized: Direction={Direction}, DoacType={DoacType}, WarfarinType={WarfarinType}",
                request.Direction ?? "NULL", request.DoacType ?? "NULL", request.WarfarinType ?? "NULL");

            // Converti le stringhe in enum (gestendo i vari formati da JavaScript)
            var direction = ParseDirection(request.Direction);
            var doacType = ParseDoacType(request.DoacType);
            var warfarinType = ParseWarfarinType(request.WarfarinType);

            _logger.LogInformation("Parsed enums: Direction={Direction}, DoacType={DoacType}, WarfarinType={WarfarinType}",
                direction, doacType, warfarinType);

            // Calcola il protocollo usando il servizio
            var protocol = _switchCalculatorService.CalculateProtocol(
                direction,
                doacType,
                warfarinType,
                request.PatientParameters);

            // Serializza il risultato in JSON per JavaScript
            var resultJson = JsonSerializer.Serialize(protocol, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger.LogInformation("Protocol calculated successfully");
            return resultJson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating switch protocol");
            throw;
        }
    }

    /// <summary>
    /// Salva il protocollo nel database
    /// </summary>
    public async Task<bool> SaveProtocolAsync(string saveDataJson)
    {
        try
        {
            if (!CurrentPatientId.HasValue)
            {
                _logger.LogWarning("Cannot save protocol: no patient selected");
                return false;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var saveData = JsonSerializer.Deserialize<SaveProtocolData>(saveDataJson, options);
            if (saveData?.Protocol == null || saveData.PatientParams == null)
            {
                throw new ArgumentException("Invalid save data JSON");
            }

            var protocol = saveData.Protocol;
            var patientParams = saveData.PatientParams;

            // Crea l'entità TherapySwitch
            var therapySwitch = new TherapySwitch
            {
                PatientId = CurrentPatientId.Value,
                SwitchDate = DateTime.Now,
                Direction = protocol.Direction.ToString(),
                DoacType = protocol.DoacType.ToString(),
                WarfarinType = protocol.WarfarinType.ToString(),
                InrAtSwitch = protocol.InrThreshold,
                CreatinineClearance = patientParams.CreatinineClearance,
                AgeAtSwitch = patientParams.Age,
                WeightAtSwitch = patientParams.Weight,
                RecommendedDosage = protocol.RecommendedDoacDosage,
                DosageRationale = protocol.DosageRationale,
                ProtocolTimeline = JsonSerializer.Serialize(protocol.Timeline),
                Contraindications = protocol.Contraindications.Count > 0
                    ? JsonSerializer.Serialize(protocol.Contraindications)
                    : null,
                Warnings = protocol.Warnings.Count > 0
                    ? JsonSerializer.Serialize(protocol.Warnings)
                    : null,
                ClinicalNotes = protocol.ClinicalNotes.Count > 0
                    ? string.Join("\n", protocol.ClinicalNotes)
                    : null,
                MonitoringPlan = protocol.MonitoringPlan,
                FirstFollowUpDate = DateTime.Now.AddDays(30), // Follow-up a 1 mese
                FollowUpCompleted = false,
                SwitchCompleted = false
            };

            _dbContext.TherapySwitches.Add(therapySwitch);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Protocol saved successfully for patient {CurrentPatientId}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving protocol");
            return false;
        }
    }

    /// <summary>
    /// Classe bridge per esporre metodi al JavaScript
    /// Questa classe deve essere COM-visible per WebView2
    /// </summary>
    [System.Runtime.InteropServices.ClassInterface(System.Runtime.InteropServices.ClassInterfaceType.AutoDual)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public class SwitchServiceBridge
    {
        private readonly SwitchTherapyViewModel _viewModel;

        public SwitchServiceBridge(SwitchTherapyViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public string CalculateProtocol(string parametersJson)
        {
            return _viewModel.CalculateProtocol(parametersJson);
        }

        public void SaveProtocol(string protocolJson)
        {
            // Nota: questo viene chiamato da JavaScript in modo sincrono,
            // quindi non possiamo usare async qui. Usiamo Task.Run per non bloccare.
            Task.Run(async () =>
            {
                await _viewModel.SaveProtocolAsync(protocolJson);
                // Dopo il salvataggio, ricarica lo storico
                await Task.Delay(200); // Piccolo delay per assicurarsi che il DB sia aggiornato
                _viewModel.LoadSwitchHistory();
            });
        }

        public void UpdateFollowUp(int switchId, string followUpNotes, bool completed, string outcome)
        {
            Task.Run(async () => await _viewModel.UpdateFollowUpAsync(switchId, followUpNotes, completed, outcome));
        }

        public void DeleteSwitch(int switchId)
        {
            Task.Run(async () => await _viewModel.DeleteSwitchAsync(switchId));
        }

        public void RefreshHistory()
        {
            _viewModel.LoadSwitchHistory();
        }
    }

    /// <summary>
    /// Classe per deserializzare la richiesta dal JavaScript
    /// </summary>
    #region Enum Parsing Helpers

    private SwitchDirection ParseDirection(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "warfarin-to-doac" => SwitchDirection.WarfarinToDoac,
            "doac-to-warfarin" => SwitchDirection.DoacToWarfarin,
            "warfarintodoac" => SwitchDirection.WarfarinToDoac,
            "doactowarfarin" => SwitchDirection.DoacToWarfarin,
            _ => Enum.Parse<SwitchDirection>(value, ignoreCase: true)
        };
    }

    private DoacType ParseDoacType(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "apixaban" => DoacType.Apixaban,
            "rivaroxaban" => DoacType.Rivaroxaban,
            "dabigatran" => DoacType.Dabigatran,
            "edoxaban" => DoacType.Edoxaban,
            _ => Enum.Parse<DoacType>(value, ignoreCase: true)
        };
    }

    private WarfarinType ParseWarfarinType(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "warfarin" => WarfarinType.Warfarin,
            "acenocumarolo" => WarfarinType.Acenocumarolo,
            "acenocoumarol" => WarfarinType.Acenocumarolo,
            _ => Enum.Parse<WarfarinType>(value, ignoreCase: true)
        };
    }

    #endregion

    private class SwitchProtocolRequest
    {
        public string Direction { get; set; } = string.Empty;
        public string DoacType { get; set; } = string.Empty;
        public string WarfarinType { get; set; } = string.Empty;
        public SwitchPatientParameters PatientParameters { get; set; } = new();
    }

    private class SaveProtocolData
    {
        public SwitchProtocol Protocol { get; set; } = null!;
        public SwitchPatientParameters PatientParams { get; set; } = null!;
    }
}
