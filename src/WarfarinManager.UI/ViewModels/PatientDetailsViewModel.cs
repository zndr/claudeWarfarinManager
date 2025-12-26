using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Shared.Constants;
using WarfarinManager.UI.Models;
using WarfarinManager.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using WarfarinManager.UI.Views.INR;
using WarfarinManager.UI.Views.Patient;

namespace WarfarinManager.UI.ViewModels
{
    /// <summary>
    /// ViewModel per la vista dettagli paziente con tab multiple
    /// </summary>
    public partial class PatientDetailsViewModel : ObservableObject, INavigationAware
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<PatientDetailsViewModel> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMillepsDataService _millepsDataService;
        private readonly IMillewinIntegrationService _millewinIntegrationService;

        [ObservableProperty]
        private int _patientId;

        [ObservableProperty]
        private PatientDto? _patient;

        [ObservableProperty]
        private ObservableCollection<IndicationDto> _indications = new();

        [ObservableProperty]
        private IndicationDto? _selectedIndication;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private int _selectedTabIndex = 0;

        /// <summary>
        /// ViewModel per la gestione farmaci (esposto per binding nel tab Farmaci)
        /// </summary>
        [ObservableProperty]
        private MedicationsViewModel? _medicationsViewModel;

        /// <summary>
        /// ViewModel per la gestione bridge therapy (esposto per binding nel tab Bridge Therapy)
        /// </summary>
        [ObservableProperty]
        private BridgeTherapyViewModel? _bridgeTherapyViewModel;

        /// <summary>
        /// ViewModel per lo storico INR (esposto per binding nel tab Storico INR)
        /// </summary>
        [ObservableProperty]
        private INRHistoryViewModel? _inrHistoryViewModel;

        /// <summary>
        /// ViewModel per la valutazione pre-TAO (esposto per binding nel tab Valutazione Pre-TAO)
        /// </summary>
        [ObservableProperty]
        private PreTaoAssessmentViewModel? _preTaoAssessmentViewModel;

        /// <summary>
        /// ViewModel per la gestione eventi avversi (esposto per binding nel tab Eventi Avversi)
        /// </summary>
        [ObservableProperty]
        private AdverseEventsViewModel? _adverseEventsViewModel;

        /// <summary>
        /// ViewModel per lo switch terapia (esposto per binding nel tab Switch Terapia)
        /// </summary>
        [ObservableProperty]
        private SwitchTherapyViewModel? _switchTherapyViewModel;

        /// <summary>
        /// ViewModel per la gestione DOAC (esposto per binding nel tab DoacGest)
        /// </summary>
        [ObservableProperty]
        private DoacGestViewModel? _doacGestViewModel;

        /// <summary>
        /// Indica se il paziente corrente assume Warfarin (per visibilità tab INR)
        /// </summary>
        public bool IsWarfarinPatient => Patient?.IsWarfarinPatient ?? true; // Default true per backward compatibility

        /// <summary>
        /// Indica se il paziente corrente assume un DOAC
        /// </summary>
        public bool IsDoacPatient => Patient?.IsDoacPatient ?? false;

        /// <summary>
        /// Indica se non ci sono dati biometrici disponibili
        /// </summary>
        public bool HasNoBiometricData => Patient?.Weight == null && Patient?.Height == null;

        /// <summary>
        /// Chiamato quando Patient cambia - notifica le proprietà computed dipendenti
        /// </summary>
        partial void OnPatientChanged(PatientDto? value)
        {
            OnPropertyChanged(nameof(IsWarfarinPatient));
            OnPropertyChanged(nameof(IsDoacPatient));
            OnPropertyChanged(nameof(HasNoBiometricData));
            _logger.LogInformation("Patient changed: IsWarfarinPatient={IsWarfarin}, IsDoacPatient={IsDoac}",
                IsWarfarinPatient, IsDoacPatient);
        }

        /// <summary>
        /// Nome completo del farmaco anticoagulante (per TextBox indicatore)
        /// </summary>
        public string AnticoagulantDisplayName => Patient?.AnticoagulantDisplayName ?? "Non specificato";

        /// <summary>
        /// Indica se l'integrazione con Millewin è attiva (abilitata + connessione disponibile)
        /// </summary>
        public bool IsMillewinIntegrationActive => _millewinIntegrationService.IsIntegrationActive;

        public PatientDetailsViewModel(
            IUnitOfWork unitOfWork,
            INavigationService navigationService,
            IDialogService dialogService,
            ILogger<PatientDetailsViewModel> logger,
            IServiceProvider serviceProvider,
            IMillepsDataService millepsDataService,
            IMillewinIntegrationService millewinIntegrationService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _millepsDataService = millepsDataService ?? throw new ArgumentNullException(nameof(millepsDataService));
            _millewinIntegrationService = millewinIntegrationService ?? throw new ArgumentNullException(nameof(millewinIntegrationService));

            // Inizializza il MedicationsViewModel dal DI
            MedicationsViewModel = _serviceProvider.GetRequiredService<MedicationsViewModel>();

            // Inizializza il BridgeTherapyViewModel dal DI
            BridgeTherapyViewModel = _serviceProvider.GetRequiredService<BridgeTherapyViewModel>();

            // Inizializza il INRHistoryViewModel dal DI
            InrHistoryViewModel = _serviceProvider.GetRequiredService<INRHistoryViewModel>();

            // Inizializza il PreTaoAssessmentViewModel dal DI
            PreTaoAssessmentViewModel = _serviceProvider.GetRequiredService<PreTaoAssessmentViewModel>();

            // Inizializza il AdverseEventsViewModel dal DI
            AdverseEventsViewModel = _serviceProvider.GetRequiredService<AdverseEventsViewModel>();

            // Inizializza il SwitchTherapyViewModel dal DI
            SwitchTherapyViewModel = _serviceProvider.GetRequiredService<SwitchTherapyViewModel>();

            // Inizializza il DoacGestViewModel dal DI
            DoacGestViewModel = _serviceProvider.GetRequiredService<DoacGestViewModel>();
        }

        /// <summary>
        /// Chiamato quando si naviga verso questa view
        /// </summary>
        public void OnNavigatedTo(object parameter)
        {
            if (parameter is int patientId)
            {
                PatientId = patientId;
                _ = LoadPatientDataAsync(PatientId);
            }
            else if (parameter is PatientNavigationParameter navParam)
            {
                PatientId = navParam.PatientId;
                SelectedTabIndex = navParam.TabIndex;
                _ = LoadPatientDataAsync(PatientId);
            }
        }

        /// <summary>
        /// Carica i dati del paziente
        /// </summary>
        [RelayCommand]
        public async Task LoadPatientDataAsync(int patientId)
        {
            try
            {
                IsLoading = true;
                _logger.LogInformation("Caricamento dettagli paziente ID: {PatientId}", PatientId);

                var patient = await _unitOfWork.Patients.GetByIdAsync(PatientId);
                if (patient == null)
                {
                    _dialogService.ShowError("Paziente non trovato", "Errore");
                    _navigationService.GoBack();
                    return;
                }

                // IMPORTANTE: Reset del TabControl prima di cambiare paziente
                // Questo forza WPF a ri-valutare le visibilità dei tab
                SelectedTabIndex = 0;

                Patient = MapPatientToDto(patient);

                // Forza aggiornamento sincronizzato dei binding prima di qualsiasi altra operazione
                // Questo assicura che IsDoacPatient sia corretto quando viene letto da OnSelectedTabIndexChanged
                System.Windows.Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.DataBind);

                _logger.LogInformation("Post-binding: IsWarfarinPatient={IsWarfarin}, IsDoacPatient={IsDoac}, SelectedTabIndex={TabIndex}",
                    IsWarfarinPatient, IsDoacPatient, SelectedTabIndex);

                // PRIMO: Carica le indicazioni PRIMA del check wizard
                // Questo è necessario per determinare se il paziente DOAC ha bisogno del wizard
                await LoadIndicationsAsync();

                // SECONDO: Verifica se è un paziente che necessita del wizard iniziale
                // Per pazienti DOAC senza indicazione → wizard
                // Per pazienti Warfarin senza INR → dialog naive + wizard
                await CheckAndShowNaivePatientDialogAsync(patient);

                // TERZO: Dopo il wizard (o se non necessario), imposta il tab corretto in base al tipo di paziente
                // - Paziente DOAC con indicazioni → tab "Gestione DOAC" (indice 6)
                // - Paziente Warfarin non-naive (wizard completato) → tab "Storico INR" (indice 4)
                if (IsDoacPatient && Indications.Any())
                {
                    // Usa Dispatcher con doppio cambio tab per forzare WPF a ri-renderizzare il contenuto
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Loaded,
                        new Action(() =>
                        {
                            _logger.LogInformation("FORZATURA tab Gestione DOAC per paziente DOAC con indicazione - step 1: vai a tab Anagrafica");
                            SelectedTabIndex = 0; // Prima vai al tab Anagrafica

                            // Poi con un secondo dispatcher, vai al tab Gestione DOAC
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Loaded,
                                new Action(() =>
                                {
                                    _logger.LogInformation("FORZATURA tab Gestione DOAC per paziente DOAC - step 2: vai a indice 6");
                                    SelectedTabIndex = 6; // Tab "Gestione DOAC" (indice fisso: 0=Anagrafica, 1=PreTAO, 2=Indicazione, 3=Farmaci, 4=StoricoINR, 5=Bridge, 6=DOAC)
                                })
                            );
                        })
                    );
                }
                else if (IsWarfarinPatient && patient.IsInitialWizardCompleted)
                {
                    // Paziente Warfarin non-naive (wizard già completato) → vai direttamente a Storico INR
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Loaded,
                        new Action(() =>
                        {
                            _logger.LogInformation("Paziente Warfarin non-naive: impostazione tab Storico INR (indice 4)");
                            SelectedTabIndex = 4; // Tab "Storico INR" (indice fisso)
                        })
                    );
                }

                // Carica i farmaci tramite MedicationsViewModel
                if (MedicationsViewModel != null)
                {
                    await MedicationsViewModel.LoadMedicationsAsync(PatientId);
                }

                // Inizializza il BridgeTherapyViewModel (solo per Warfarin, ma lo carichiamo comunque)
                if (BridgeTherapyViewModel != null)
                {
                    await BridgeTherapyViewModel.InitializeAsync(PatientId);
                }

                // Inizializza lo storico INR (solo per Warfarin)
                if (InrHistoryViewModel != null && IsWarfarinPatient)
                {
                    // Ottieni target INR dall'indicazione attiva
                    var activeIndication = Indications.FirstOrDefault(i => i.IsActive);
                    if (activeIndication != null)
                    {
                        await InrHistoryViewModel.InitializeAsync(PatientId, activeIndication.TargetINRMin, activeIndication.TargetINRMax);
                    }
                    else
                    {
                        await InrHistoryViewModel.InitializeAsync(PatientId);
                    }
                }

                // Inizializza la valutazione pre-TAO
                if (PreTaoAssessmentViewModel != null)
                {
                    _logger.LogInformation("Inizializzazione PreTaoAssessmentViewModel per paziente {PatientId}", PatientId);
                    await PreTaoAssessmentViewModel.InitializeAsync(PatientId);
                }
                else
                {
                    _logger.LogWarning("PreTaoAssessmentViewModel è NULL!");
                }

                // Inizializza gli eventi avversi
                if (AdverseEventsViewModel != null)
                {
                    _logger.LogInformation("Caricamento eventi avversi per paziente {PatientId}", PatientId);
                    await AdverseEventsViewModel.LoadAdverseEventsAsync(PatientId);
                }

                // Inizializza lo Switch Terapia
                if (SwitchTherapyViewModel != null)
                {
                    _logger.LogInformation("Inizializzazione Switch Terapia per paziente {PatientId}", PatientId);
                    SwitchTherapyViewModel.SetCurrentPatient(PatientId);
                }

                // Inizializza DoacGest solo se il paziente assume un DOAC
                if (DoacGestViewModel != null && IsDoacPatient)
                {
                    _logger.LogInformation("Inizializzazione DoacGest per paziente DOAC {PatientId}", PatientId);
                    await DoacGestViewModel.InitializeAsync(PatientId);
                }

                _logger.LogInformation("Paziente caricato: {FullName}", Patient.FullName);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Carica le indicazioni del paziente
        /// </summary>
        private async Task LoadIndicationsAsync()
        {
            try
            {
                var indications = await _unitOfWork.Database.Indications
                    .Where(i => i.PatientId == PatientId)
                    .Include(i => i.IndicationType)
                    .OrderByDescending(i => i.IsActive)
                    .ThenByDescending(i => i.StartDate)
                    .ToListAsync();

                var indicationDtos = indications
                    .Select(MapIndicationToDto)
                    .ToList();

                Indications = new ObservableCollection<IndicationDto>(indicationDtos);

                _logger.LogInformation("Caricate {Count} indicazioni per paziente {PatientId}",
                    Indications.Count, PatientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento delle indicazioni");
                throw;
            }
        }

        /// <summary>
        /// Torna alla lista pazienti
        /// </summary>
        [RelayCommand]
        private void GoBack()
        {
            _navigationService.NavigateTo<PatientListViewModel>();
        }

        /// <summary>
        /// Apre il form per modificare l'anagrafica
        /// </summary>
        [RelayCommand]
        private void EditPatient()
        {
            try
            {
                _logger.LogInformation("Apertura form modifica anagrafica per paziente {PatientId}", PatientId);

                // Naviga al form paziente passando l'ID per la modifica
                _navigationService.NavigateTo<PatientFormViewModel>(PatientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore apertura form modifica paziente");
                _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
            }
        }

        /// <summary>
        /// Apre il form per aggiungere una nuova indicazione
        /// </summary>
        [RelayCommand]
        private void AddIndication()
        {
            try
            {
                _logger.LogInformation("Apertura form nuova indicazione per paziente {PatientId}", PatientId);

                // Naviga al form indicazione passando l'ID del paziente
                _navigationService.NavigateTo<IndicationFormViewModel>(PatientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore apertura form indicazione");
                _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
            }
        }

        /// <summary>
        /// Apre la vista per gestire i controlli INR del paziente
        /// </summary>
        [RelayCommand]
        private async Task OpenINRControlAsync()
        {
            try
            {
                if (Patient == null) return;

                _logger.LogInformation("Apertura gestione INR per paziente {PatientId}", PatientId);

                // Risolvi INRControlView e ViewModel dal DI
                var inrView = _serviceProvider.GetRequiredService<INRControlView>();
                var inrViewModel = _serviceProvider.GetRequiredService<INRControlViewModel>();

                // Carica i dati del paziente nel ViewModel
                await inrViewModel.LoadPatientDataAsync(PatientId);

                // Assegna il DataContext
                inrView.DataContext = inrViewModel;

                // Mostra la finestra
                inrView.ShowDialog();

                // Dopo la chiusura, ricarica i dati del paziente (potrebbero essere cambiati)
                await LoadPatientDataAsync(PatientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore apertura gestione INR");
                _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
            }
        }

        /// <summary>
        /// Aggiorna i dati biometrici del paziente dal database Millewin
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRefreshBiometricData))]
        private async Task RefreshBiometricDataAsync()
        {
            if (Patient == null) return;

            try
            {
                _logger.LogInformation("Aggiornamento dati biometrici da Millewin per paziente {FiscalCode}", Patient.FiscalCode);

                // Verifica connessione Millewin
                if (!await _millepsDataService.TestConnectionAsync())
                {
                    _dialogService.ShowError("Impossibile connettersi al database Millewin.\nVerificare che Millewin sia in esecuzione.", "Errore Connessione");
                    return;
                }

                // Recupera dati biometrici da Millewin
                var biometricData = await _millepsDataService.GetBiometricDataAsync(Patient.FiscalCode);

                if (biometricData == null || (!biometricData.Weight.HasValue && !biometricData.Height.HasValue))
                {
                    _dialogService.ShowInformation("Nessun dato biometrico trovato in Millewin per questo paziente.", "Nessun Dato");
                    return;
                }

                // Aggiorna il paziente nel database TaoGEST
                var patient = await _unitOfWork.Patients.GetByIdAsync(PatientId);
                if (patient == null) return;

                bool updated = false;

                if (biometricData.Weight.HasValue)
                {
                    patient.Weight = biometricData.Weight;
                    patient.WeightLastUpdated = biometricData.WeightDate ?? DateTime.Now;
                    updated = true;
                }

                if (biometricData.Height.HasValue)
                {
                    patient.Height = biometricData.Height;
                    patient.HeightLastUpdated = biometricData.HeightDate ?? DateTime.Now;
                    updated = true;
                }

                if (updated)
                {
                    await _unitOfWork.SaveChangesAsync();

                    // Aggiorna il DTO locale
                    Patient.Weight = patient.Weight;
                    Patient.Height = patient.Height;
                    Patient.WeightLastUpdated = patient.WeightLastUpdated;
                    Patient.HeightLastUpdated = patient.HeightLastUpdated;

                    // Notifica cambio proprietà
                    OnPropertyChanged(nameof(Patient));
                    OnPropertyChanged(nameof(HasNoBiometricData));

                    _logger.LogInformation("Dati biometrici aggiornati: Peso={Weight}kg, Altezza={Height}cm",
                        patient.Weight, patient.Height);

                    _dialogService.ShowInformation(
                        $"Dati biometrici aggiornati da Millewin:\n\n" +
                        $"Peso: {patient.Weight:F1} kg\n" +
                        $"Altezza: {patient.Height:F0} cm\n" +
                        $"BMI: {patient.BMI:F1}",
                        "Aggiornamento Completato");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore aggiornamento dati biometrici da Millewin");
                _dialogService.ShowError($"Errore durante l'aggiornamento: {ex.Message}", "Errore");
            }
        }

        private bool CanRefreshBiometricData() => IsMillewinIntegrationActive;

        /// <summary>
        /// Termina l'indicazione selezionata
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEndIndication))]
        private async Task EndIndicationAsync()
        {
            if (SelectedIndication == null)
                return;

            try
            {
                var confirm = _dialogService.ShowQuestion(
                    $"Vuoi terminare l'indicazione '{SelectedIndication.Description}'?\n\n" +
                    $"Data inizio: {SelectedIndication.StartDate:dd/MM/yyyy}\n" +
                    $"Questa azione non può essere annullata.",
                    "Conferma Termine Indicazione");

                if (confirm != System.Windows.MessageBoxResult.Yes)
                    return;

                var indication = await _unitOfWork.Database.Indications
                    .FirstOrDefaultAsync(i => i.Id == SelectedIndication.Id);

                if (indication == null)
                {
                    _dialogService.ShowError("Indicazione non trovata", "Errore");
                    return;
                }

                indication.EndDate = DateTime.Today;
                indication.IsActive = false;

                await _unitOfWork.SaveChangesAsync();

                _dialogService.ShowInformation("Indicazione terminata con successo", "Successo");
                await LoadIndicationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il termine dell'indicazione");
                _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
            }
        }

        private bool CanEndIndication() => SelectedIndication != null && SelectedIndication.IsActive;

        partial void OnSelectedIndicationChanged(IndicationDto? value)
        {
            EndIndicationCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Verifica se mostrare il wizard iniziale basandosi sul tipo di paziente e sulle indicazioni registrate
        /// - Paziente Warfarin senza INR e wizard non completato → dialog naive + wizard completo
        /// - Paziente DOAC senza indicazione → wizard per configurazione iniziale
        /// NOTA: Richiede che LoadIndicationsAsync() sia già stato chiamato per popolare la collezione Indications
        /// </summary>
        private async Task CheckAndShowNaivePatientDialogAsync(Data.Entities.Patient patient)
        {
            try
            {
                // Determina il tipo di paziente
                var isDoacPatient = AnticoagulantTypes.IsDoac(patient.AnticoagulantType);
                var isWarfarinPatient = AnticoagulantTypes.IsWarfarin(patient.AnticoagulantType);

                // Usa la collezione Indications già caricata (evita query DB aggiuntiva)
                var hasIndications = Indications.Any();

                _logger.LogInformation("Verifica wizard iniziale per paziente {PatientId}: IsDoac={IsDoac}, IsWarfarin={IsWarfarin}, WizardCompleted={WizardCompleted}, HasIndications={HasInd}",
                    patient.Id, isDoacPatient, isWarfarinPatient, patient.IsInitialWizardCompleted, hasIndications);

                // Se il wizard è già stato completato, non mostrare nulla
                if (patient.IsInitialWizardCompleted)
                {
                    _logger.LogInformation("Paziente {PatientId}: wizard già completato, skip", patient.Id);
                    return;
                }

                if (isDoacPatient)
                {
                    // === PAZIENTE DOAC ===
                    // Se NON ha indicazioni → mostra wizard per configurazione iniziale
                    if (!hasIndications)
                    {
                        _logger.LogInformation("Paziente DOAC {PatientId} senza indicazioni: avvio wizard configurazione", patient.Id);
                        await OpenInitialConfigurationWizardAsync(patient.Id);
                    }
                    else
                    {
                        _logger.LogInformation("Paziente DOAC {PatientId} ha già indicazioni registrate, nessun wizard necessario", patient.Id);
                        // Marca il wizard come completato per evitare richieste future
                        patient.IsInitialWizardCompleted = true;
                        await _unitOfWork.Patients.UpdateAsync(patient);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
                else if (isWarfarinPatient)
                {
                    // === PAZIENTE WARFARIN ===
                    // Controlla se il paziente ha già controlli INR
                    var hasINRControls = await _unitOfWork.Database.INRControls
                        .AnyAsync(c => c.PatientId == patient.Id);

                    // Mostra il dialog solo se:
                    // 1. Il paziente è in terapia con Warfarin
                    // 2. Il paziente non ha ancora controlli INR
                    // 3. Il wizard iniziale non è già stato completato
                    if (!hasINRControls)
                    {
                        var isNaive = _dialogService.ShowNaivePatientDialog(patient.FullName);

                        if (isNaive.HasValue)
                        {
                            // Aggiorna il flag nel database
                            patient.IsNaive = isNaive.Value;
                            await _unitOfWork.Patients.UpdateAsync(patient);
                            await _unitOfWork.SaveChangesAsync();

                            _logger.LogInformation("Paziente {PatientId} marcato come {Status}",
                                patient.Id, isNaive.Value ? "Naive" : "Non-Naive");

                            // Se è naive, mostra le informazioni sulla fase di induzione
                            if (isNaive.Value)
                            {
                                _dialogService.ShowInductionPhaseInfo();
                            }

                            // SEMPRE apri il wizard di configurazione iniziale
                            // (sia per pazienti naive che non-naive appena importati)
                            await OpenInitialConfigurationWizardAsync(patient.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica paziente naive");
                // Non mostrare errore all'utente, è una funzionalità opzionale
            }
        }

        /// <summary>
        /// Gestisce il cambio di tab - ricarica i dati se necessario
        /// Indici tab (fissi, indipendenti dalla visibility):
        /// 0=Anagrafica, 1=PreTAO, 2=Indicazione, 3=Farmaci, 4=StoricoINR, 5=Bridge, 6=DOAC, 7=EventiAvversi, 8=Switch
        /// </summary>
        partial void OnSelectedTabIndexChanged(int value)
        {
            _logger.LogInformation("Tab selezionato: indice {TabIndex}, IsWarfarinPatient: {IsWarfarin}, IsDoacPatient: {IsDoac}",
                value, IsWarfarinPatient, IsDoacPatient);

            // Tab 3 = Farmaci
            if (value == 3 && PatientId > 0 && MedicationsViewModel != null)
            {
                _ = MedicationsViewModel.LoadMedicationsAsync(PatientId);
            }
            // Tab 4 = Storico INR (solo Warfarin)
            else if (value == 4 && PatientId > 0 && IsWarfarinPatient && InrHistoryViewModel != null)
            {
                // Ottieni target INR dall'indicazione attiva
                var activeIndication = Indications.FirstOrDefault(i => i.IsActive);
                if (activeIndication != null)
                {
                    _ = InrHistoryViewModel.InitializeAsync(PatientId, activeIndication.TargetINRMin, activeIndication.TargetINRMax);
                }
                else
                {
                    _ = InrHistoryViewModel.InitializeAsync(PatientId);
                }
            }
            // Tab 5 = Bridge Therapy (solo Warfarin)
            else if (value == 5 && PatientId > 0 && IsWarfarinPatient && BridgeTherapyViewModel != null)
            {
                _ = BridgeTherapyViewModel.InitializeAsync(PatientId);
            }
            // Tab 6 = Gestione DOAC (solo DOAC)
            else if (value == 6 && PatientId > 0 && IsDoacPatient && DoacGestViewModel != null)
            {
                _logger.LogInformation("Tab Gestione DOAC selezionato per paziente DOAC");
                // DoacGestViewModel è già inizializzato in LoadPatientDataAsync
            }
        }

        /// <summary>
        /// Mappa Patient entity a DTO
        /// </summary>
        private PatientDto MapPatientToDto(Data.Entities.Patient patient)
        {
            return new PatientDto
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                BirthDate = patient.BirthDate,
                FiscalCode = patient.FiscalCode,
                Gender = patient.Gender?.ToString(),
                Phone = patient.Phone,
                Email = patient.Email,
                IsSlowMetabolizer = patient.IsSlowMetabolizer,
                AnticoagulantType = patient.AnticoagulantType,
                TherapyStartDate = patient.TherapyStartDate,
                // Dati biometrici
                Weight = patient.Weight,
                Height = patient.Height,
                WeightLastUpdated = patient.WeightLastUpdated,
                HeightLastUpdated = patient.HeightLastUpdated,
                // TODO: Caricare indicazione attiva, ultimo INR, TTR
                ActiveIndication = null,
                LastINR = null,
                LastINRDate = null,
                TTRPercentage = null,
                NextControlDate = null
            };
        }

        /// <summary>
        /// Mappa Indication entity a DTO
        /// </summary>
        private IndicationDto MapIndicationToDto(Data.Entities.Indication indication)
        {
            return new IndicationDto
            {
                Id = indication.Id,
                PatientId = indication.PatientId,
                IndicationTypeCode = indication.IndicationType?.Code ?? string.Empty,
                Description = indication.IndicationType?.Description ?? "N/D",
                Category = indication.IndicationType?.Category ?? string.Empty,
                TargetINRMin = indication.TargetINRMin,
                TargetINRMax = indication.TargetINRMax,
                StartDate = indication.StartDate,
                EndDate = indication.EndDate,
                IsActive = indication.IsActive,
                ChangeReason = indication.ChangeReason,
                Notes = indication.Notes,
                TypicalDuration = indication.IndicationType?.TypicalDuration
            };
        }

        /// <summary>
        /// Apre il wizard di configurazione iniziale per il paziente
        /// </summary>
        private async Task OpenInitialConfigurationWizardAsync(int patientId)
        {
            try
            {
                _logger.LogInformation("Apertura wizard configurazione iniziale per paziente {PatientId}", patientId);

                // Risolvi il wizard dal DI
                var wizardView = _serviceProvider.GetRequiredService<NewPatientWizardView>();
                var wizardViewModel = _serviceProvider.GetRequiredService<NewPatientWizardViewModel>();

                // Inizializza il wizard con i dati del paziente
                await wizardViewModel.InitializeAsync(patientId);

                // Assegna il DataContext
                wizardView.DataContext = wizardViewModel;

                // Mostra il wizard in modalità dialog
                var result = wizardView.ShowDialog();

                // Se il wizard è stato completato con successo, aggiorna solo le proprietà necessarie
                if (result == true)
                {
                    _logger.LogInformation("Wizard completato per paziente {PatientId}, aggiorno dati", patientId);

                    // Ricarica solo le indicazioni e i dati del paziente senza ri-trigger del dialog naive
                    var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                    if (patient != null)
                    {
                        Patient = MapPatientToDto(patient);
                        await LoadIndicationsAsync();

                        // Notifica cambio proprietà
                        OnPropertyChanged(nameof(IsWarfarinPatient));
                        OnPropertyChanged(nameof(IsDoacPatient));
                        OnPropertyChanged(nameof(AnticoagulantDisplayName));

                        // Inizializza DoacGest se il paziente è DOAC
                        if (IsDoacPatient && DoacGestViewModel != null)
                        {
                            _logger.LogInformation("Post-wizard: inizializzazione DoacGest per paziente DOAC {PatientId}", patientId);
                            await DoacGestViewModel.InitializeAsync(patientId);
                        }
                    }

                    // Per pazienti Warfarin: verifica se l'utente vuole inserire il primo INR
                    if (IsWarfarinPatient && wizardViewModel.ShouldOpenINRForm)
                    {
                        _logger.LogInformation("Apertura dialog Controllo INR per inserimento primo valore");

                        // Apri il dialog di Controllo INR
                        var inrView = _serviceProvider.GetRequiredService<INRControlView>();
                        var inrViewModel = _serviceProvider.GetRequiredService<INRControlViewModel>();

                        await inrViewModel.LoadPatientDataAsync(patientId);
                        inrView.DataContext = inrViewModel;
                        inrView.ShowDialog();

                        // Ricarica i dati dopo l'inserimento
                        await LoadPatientDataAsync(patientId);
                    }
                    else if (IsDoacPatient)
                    {
                        // Per pazienti DOAC: mostra messaggio di completamento e imposta il tab Gestione DOAC
                        _dialogService.ShowInformation(
                            "Configurazione iniziale completata con successo.\n\n" +
                            "Ora puoi gestire la terapia DOAC nella sezione 'Gestione DOAC'.",
                            "Configurazione Completata");

                        // Imposta il tab Gestione DOAC
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Loaded,
                            new Action(() =>
                            {
                                _logger.LogInformation("Post-wizard DOAC: impostazione tab Gestione DOAC");
                                SelectedTabIndex = 6; // Tab "Gestione DOAC" (indice fisso: 0=Anagrafica, 1=PreTAO, 2=Indicazione, 3=Farmaci, 4=StoricoINR, 5=Bridge, 6=DOAC)
                            })
                        );
                    }
                    else
                    {
                        _dialogService.ShowInformation(
                            "Configurazione iniziale completata con successo.\n\n" +
                            "Ora puoi gestire i controlli INR e tutte le altre funzionalità del paziente.",
                            "Configurazione Completata");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore apertura wizard configurazione iniziale");
                _dialogService.ShowError($"Errore: {ex.Message}", "Errore");
            }
        }
    }

    /// <summary>
    /// Parametro per la navigazione ai dettagli paziente con selezione tab
    /// </summary>
    public class PatientNavigationParameter
    {
        public int PatientId { get; set; }
        public int TabIndex { get; set; } = 0; // Default: tab Anagrafica
    }
}
