using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Shared.Enums;
using WarfarinManager.UI.Services;

namespace WarfarinManager.UI.ViewModels
{
    /// <summary>
    /// ViewModel per la gestione degli eventi avversi del paziente
    /// </summary>
    public partial class AdverseEventsViewModel : ObservableObject
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDialogService _dialogService;
        private readonly ILogger<AdverseEventsViewModel> _logger;

        [ObservableProperty]
        private int _patientId;

        [ObservableProperty]
        private ObservableCollection<AdverseEvent> _adverseEvents = new();

        [ObservableProperty]
        private AdverseEvent? _selectedEvent;

        [ObservableProperty]
        private bool _isLoading;

        // Form fields per nuovo/modifica evento
        [ObservableProperty]
        private DateTime _onsetDate = DateTime.Today;

        [ObservableProperty]
        private AdverseReactionType? _selectedReactionType;

        [ObservableProperty]
        private CertaintyLevel _selectedCertaintyLevel = CertaintyLevel.Certain;

        [ObservableProperty]
        private string? _measuresTaken;

        [ObservableProperty]
        private decimal? _inrAtEvent;

        [ObservableProperty]
        private string? _notes;

        [ObservableProperty]
        private bool _isEditMode;

        public AdverseEventsViewModel(
            IUnitOfWork unitOfWork,
            IDialogService dialogService,
            ILogger<AdverseEventsViewModel> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Carica gli eventi avversi del paziente
        /// </summary>
        public async Task LoadAdverseEventsAsync(int patientId)
        {
            try
            {
                IsLoading = true;
                PatientId = patientId;

                _logger.LogInformation("Caricamento eventi avversi per paziente {PatientId}", patientId);

                var events = await _unitOfWork.AdverseEvents.GetAllAsync();
                var patientEvents = (events ?? Enumerable.Empty<AdverseEvent>())
                    .Where(e => e.PatientId == patientId)
                    .OrderByDescending(e => e.OnsetDate)
                    .ToList();

                AdverseEvents.Clear();
                foreach (var evt in patientEvents)
                {
                    AdverseEvents.Add(evt);
                }

                _logger.LogInformation("Caricati {Count} eventi avversi", patientEvents.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento degli eventi avversi");
                // Non mostrare errore all'utente se non ci sono eventi (è normale)
                _logger.LogWarning("Nessun evento avverso trovato per il paziente {PatientId}", patientId);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Comando per aprire il dialog di selezione del tipo di reazione
        /// </summary>
        [RelayCommand]
        private void SelectReactionType()
        {
            var dialog = new Views.Dialogs.ReactionSelectionDialog
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true && dialog.SelectedReaction.HasValue)
            {
                SelectedReactionType = dialog.SelectedReaction.Value;
                _logger.LogInformation("Selezionata reazione: {Reaction}", SelectedReactionType);
            }
        }

        /// <summary>
        /// Comando per salvare un nuovo evento o modificare uno esistente
        /// </summary>
        [RelayCommand]
        private async Task SaveEvent()
        {
            try
            {
                if (PatientId <= 0)
                {
                    _dialogService.ShowWarning("Nessun paziente selezionato", "Attenzione");
                    return;
                }

                if (!SelectedReactionType.HasValue)
                {
                    _dialogService.ShowWarning("Seleziona il tipo di reazione avversa", "Attenzione");
                    return;
                }

                IsLoading = true;

                AdverseEvent adverseEvent;
                if (IsEditMode && SelectedEvent != null)
                {
                    // Modifica evento esistente
                    adverseEvent = SelectedEvent;
                    adverseEvent.OnsetDate = OnsetDate;
                    adverseEvent.ReactionType = SelectedReactionType.Value;
                    adverseEvent.Severity = GetSeverityForReaction(SelectedReactionType.Value);
                    adverseEvent.CertaintyLevel = SelectedCertaintyLevel;
                    adverseEvent.MeasuresTaken = MeasuresTaken;
                    adverseEvent.INRAtEvent = InrAtEvent;
                    adverseEvent.Notes = Notes;
                    adverseEvent.UpdatedAt = DateTime.Now;

                    await _unitOfWork.AdverseEvents.UpdateAsync(adverseEvent);
                    _logger.LogInformation("Evento avverso {Id} aggiornato", adverseEvent.Id);
                }
                else
                {
                    // Nuovo evento
                    adverseEvent = new AdverseEvent
                    {
                        PatientId = PatientId,
                        OnsetDate = OnsetDate,
                        ReactionType = SelectedReactionType.Value,
                        Severity = GetSeverityForReaction(SelectedReactionType.Value),
                        CertaintyLevel = SelectedCertaintyLevel,
                        MeasuresTaken = MeasuresTaken,
                        INRAtEvent = InrAtEvent,
                        Notes = Notes,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    await _unitOfWork.AdverseEvents.AddAsync(adverseEvent);
                    _logger.LogInformation("Nuovo evento avverso creato per paziente {PatientId}", PatientId);
                }

                await _unitOfWork.SaveChangesAsync();

                // Ricarica la lista
                await LoadAdverseEventsAsync(PatientId);

                // Reset form
                ResetForm();

                _dialogService.ShowInformation(
                    IsEditMode ? "Evento avverso aggiornato con successo" : "Evento avverso registrato con successo",
                    "Successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il salvataggio dell'evento avverso");
                _dialogService.ShowError("Errore durante il salvataggio dell'evento avverso", "Errore");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Comando per modificare un evento esistente
        /// </summary>
        [RelayCommand]
        private void EditEvent(AdverseEvent? adverseEvent)
        {
            if (adverseEvent == null) return;

            IsEditMode = true;
            SelectedEvent = adverseEvent;
            OnsetDate = adverseEvent.OnsetDate;
            SelectedReactionType = adverseEvent.ReactionType;
            SelectedCertaintyLevel = adverseEvent.CertaintyLevel;
            MeasuresTaken = adverseEvent.MeasuresTaken;
            InrAtEvent = adverseEvent.INRAtEvent;
            Notes = adverseEvent.Notes;
        }

        /// <summary>
        /// Comando per eliminare un evento
        /// </summary>
        [RelayCommand]
        private async Task DeleteEvent(AdverseEvent? adverseEvent)
        {
            if (adverseEvent == null) return;

            var result = _dialogService.ShowQuestion(
                "Sei sicuro di voler eliminare questo evento avverso?",
                "Conferma Eliminazione");

            if (result != System.Windows.MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;

                await _unitOfWork.AdverseEvents.DeleteAsync(adverseEvent);
                await _unitOfWork.SaveChangesAsync();

                AdverseEvents.Remove(adverseEvent);

                _logger.LogInformation("Evento avverso {Id} eliminato", adverseEvent.Id);
                _dialogService.ShowInformation("Evento avverso eliminato con successo", "Successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'evento avverso");
                _dialogService.ShowError("Errore durante l'eliminazione dell'evento avverso", "Errore");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Comando per annullare la modifica
        /// </summary>
        [RelayCommand]
        private void CancelEdit()
        {
            ResetForm();
        }

        /// <summary>
        /// Reset del form
        /// </summary>
        private void ResetForm()
        {
            IsEditMode = false;
            SelectedEvent = null;
            OnsetDate = DateTime.Today;
            SelectedReactionType = null;
            SelectedCertaintyLevel = CertaintyLevel.Certain;
            MeasuresTaken = null;
            InrAtEvent = null;
            Notes = null;
        }

        /// <summary>
        /// Ottiene la gravità associata a un tipo di reazione
        /// </summary>
        private AdverseReactionSeverity GetSeverityForReaction(AdverseReactionType reactionType)
        {
            var field = reactionType.GetType().GetField(reactionType.ToString());
            var attribute = field?.GetCustomAttribute<AdverseReactionInfoAttribute>();
            return attribute?.Severity ?? AdverseReactionSeverity.Common;
        }

        /// <summary>
        /// Ottiene la descrizione di un tipo di reazione
        /// </summary>
        public static string GetReactionDescription(AdverseReactionType reactionType)
        {
            var field = reactionType.GetType().GetField(reactionType.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? reactionType.ToString();
        }

        /// <summary>
        /// Ottiene tutte le reazioni avverse disponibili, raggruppate per gravità
        /// </summary>
        public static IEnumerable<IGrouping<AdverseReactionSeverity, AdverseReactionType>> GetReactionsGroupedBySeverity()
        {
            var allReactions = Enum.GetValues<AdverseReactionType>();
            return allReactions
                .Select(r => new
                {
                    Reaction = r,
                    Severity = GetSeverityForReactionStatic(r)
                })
                .GroupBy(x => x.Severity, x => x.Reaction)
                .OrderBy(g => g.Key);
        }

        private static AdverseReactionSeverity GetSeverityForReactionStatic(AdverseReactionType reactionType)
        {
            var field = reactionType.GetType().GetField(reactionType.ToString());
            var attribute = field?.GetCustomAttribute<AdverseReactionInfoAttribute>();
            return attribute?.Severity ?? AdverseReactionSeverity.Common;
        }

        /// <summary>
        /// Verifica se una reazione può verificarsi all'inizio della terapia
        /// </summary>
        public static bool CanOccurAtStart(AdverseReactionType reactionType)
        {
            var field = reactionType.GetType().GetField(reactionType.ToString());
            var attribute = field?.GetCustomAttribute<AdverseReactionInfoAttribute>();
            return attribute?.CanOccurAtStart ?? false;
        }
    }
}
