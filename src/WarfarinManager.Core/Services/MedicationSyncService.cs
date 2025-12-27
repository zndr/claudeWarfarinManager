using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per la sincronizzazione dei farmaci concomitanti con Milleps
/// </summary>
public class MedicationSyncService : IMedicationSyncService
{
    private readonly IMillepsDataService _millepsDataService;
    private readonly IMillewinIntegrationService _integrationService;
    private readonly IInteractionCheckerService _interactionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MedicationSyncService> _logger;

    public MedicationSyncService(
        IMillepsDataService millepsDataService,
        IMillewinIntegrationService integrationService,
        IInteractionCheckerService interactionService,
        IUnitOfWork unitOfWork,
        ILogger<MedicationSyncService> logger)
    {
        _millepsDataService = millepsDataService;
        _integrationService = integrationService;
        _interactionService = interactionService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsSyncAvailable => _integrationService.IsIntegrationActive;

    /// <inheritdoc />
    public async Task<MedicationSyncResult> SyncMedicationsAsync(int patientId, string codiceFiscale)
    {
        if (string.IsNullOrWhiteSpace(codiceFiscale))
        {
            return MedicationSyncResult.Error("Codice fiscale non valido");
        }

        if (!IsSyncAvailable)
        {
            return MedicationSyncResult.Error("Integrazione Millewin non attiva");
        }

        try
        {
            _logger.LogInformation(
                "Avvio sincronizzazione farmaci per paziente {PatientId}, CF: {CF}",
                patientId, codiceFiscale);

            // 1. Recupera farmaci da Milleps
            var millepseMedications = await _millepsDataService.GetActiveMedicationsAsync(codiceFiscale);

            if (millepseMedications.Count == 0)
            {
                _logger.LogInformation("Nessun farmaco trovato in Milleps per CF: {CF}", codiceFiscale);

                // Disattiva eventuali farmaci Milleps esistenti che non sono piu in terapia
                var deactivated = await DeactivateAllMillepsMedicationsAsync(patientId);

                return new MedicationSyncResult
                {
                    Success = true,
                    Added = 0,
                    Updated = 0,
                    Deactivated = deactivated,
                    NewInteractions = 0
                };
            }

            // 2. Recupera farmaci esistenti dal database locale (solo quelli da Milleps)
            var existingMedications = await _unitOfWork.Database.Medications
                .Where(m => m.PatientId == patientId && m.Source == MedicationSource.Milleps)
                .ToListAsync();

            // 3. Crea dizionario per lookup veloce per codice ATC
            var existingByAtc = existingMedications
                .Where(m => !string.IsNullOrEmpty(m.AtcCode))
                .ToDictionary(m => m.AtcCode!, StringComparer.OrdinalIgnoreCase);

            // 4. Processa i farmaci da Milleps
            int added = 0;
            int updated = 0;
            int newInteractions = 0;
            var processedAtcCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var millepseMed in millepseMedications)
            {
                if (string.IsNullOrEmpty(millepseMed.AtcCode))
                    continue;

                processedAtcCodes.Add(millepseMed.AtcCode);

                if (existingByAtc.TryGetValue(millepseMed.AtcCode, out var existingMed))
                {
                    // Farmaco esistente - aggiorna se necessario
                    var wasUpdated = await UpdateMedicationIfNeededAsync(existingMed, millepseMed);
                    if (wasUpdated) updated++;
                }
                else
                {
                    // Nuovo farmaco - aggiungi
                    var (medication, hasInteraction) = await CreateMedicationAsync(patientId, millepseMed);
                    await _unitOfWork.Database.Medications.AddAsync(medication);
                    added++;
                    if (hasInteraction) newInteractions++;
                }
            }

            // 5. Disattiva farmaci Milleps non piu presenti
            int deactivatedCount = 0;
            foreach (var existingMed in existingMedications)
            {
                if (!string.IsNullOrEmpty(existingMed.AtcCode) &&
                    !processedAtcCodes.Contains(existingMed.AtcCode) &&
                    existingMed.IsActive)
                {
                    existingMed.IsActive = false;
                    existingMed.EndDate = DateTime.Today;
                    deactivatedCount++;

                    _logger.LogInformation(
                        "Farmaco {Name} (ATC: {ATC}) disattivato - non piu in Milleps",
                        existingMed.MedicationName, existingMed.AtcCode);
                }
            }

            // 6. Salva modifiche
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Sincronizzazione completata per paziente {PatientId}: +{Added} ~{Updated} -{Deactivated}",
                patientId, added, updated, deactivatedCount);

            return new MedicationSyncResult
            {
                Success = true,
                Added = added,
                Updated = updated,
                Deactivated = deactivatedCount,
                NewInteractions = newInteractions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante sincronizzazione farmaci per paziente {PatientId}", patientId);
            return MedicationSyncResult.Error($"Errore sincronizzazione: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea una nuova entity Medication da un farmaco Milleps
    /// </summary>
    private async Task<(Medication medication, bool hasInteraction)> CreateMedicationAsync(
        int patientId,
        Shared.Models.MillepsMedication millepseMed)
    {
        // Verifica interazione
        var interactionResult = await _interactionService.CheckInteractionAsync(
            millepseMed.DrugName,
            millepseMed.ActiveIngredient);

        var medication = new Medication
        {
            PatientId = patientId,
            MedicationName = millepseMed.DrugName,
            AtcCode = millepseMed.AtcCode,
            ActiveIngredient = millepseMed.ActiveIngredient,
            Source = MedicationSource.Milleps,
            StartDate = millepseMed.StartDate ?? DateTime.Today,
            IsActive = true,
            InteractionLevel = interactionResult.InteractionLevel,
            InteractionDetails = interactionResult.HasInteraction
                ? JsonSerializer.Serialize(new
                {
                    interactionResult.InteractionEffect,
                    interactionResult.Mechanism,
                    interactionResult.FCSAManagement,
                    interactionResult.ACCPManagement,
                    interactionResult.RecommendedINRCheckDays,
                    interactionResult.OddsRatio
                })
                : null
        };

        _logger.LogInformation(
            "Nuovo farmaco da Milleps: {Name} (ATC: {ATC}), Interazione: {Level}",
            medication.MedicationName, medication.AtcCode, medication.InteractionLevel);

        return (medication, interactionResult.HasInteraction);
    }

    /// <summary>
    /// Aggiorna un farmaco esistente se ci sono differenze
    /// </summary>
    private async Task<bool> UpdateMedicationIfNeededAsync(
        Medication existing,
        Shared.Models.MillepsMedication millepseMed)
    {
        bool updated = false;

        // Riattiva se era stato disattivato
        if (!existing.IsActive)
        {
            existing.IsActive = true;
            existing.EndDate = null;
            updated = true;
            _logger.LogInformation("Farmaco {Name} riattivato da Milleps", existing.MedicationName);
        }

        // Aggiorna nome commerciale se cambiato
        if (!string.Equals(existing.MedicationName, millepseMed.DrugName, StringComparison.OrdinalIgnoreCase))
        {
            existing.MedicationName = millepseMed.DrugName;
            updated = true;
        }

        // Aggiorna principio attivo se cambiato o mancante
        if (string.IsNullOrEmpty(existing.ActiveIngredient) && !string.IsNullOrEmpty(millepseMed.ActiveIngredient))
        {
            existing.ActiveIngredient = millepseMed.ActiveIngredient;
            updated = true;
        }

        // Ricalcola interazione se non presente
        if (existing.InteractionLevel == InteractionLevel.None && !string.IsNullOrEmpty(existing.ActiveIngredient))
        {
            var interactionResult = await _interactionService.CheckInteractionAsync(
                existing.MedicationName,
                existing.ActiveIngredient);

            if (interactionResult.HasInteraction)
            {
                existing.InteractionLevel = interactionResult.InteractionLevel;
                existing.InteractionDetails = JsonSerializer.Serialize(new
                {
                    interactionResult.InteractionEffect,
                    interactionResult.Mechanism,
                    interactionResult.FCSAManagement,
                    interactionResult.ACCPManagement,
                    interactionResult.RecommendedINRCheckDays,
                    interactionResult.OddsRatio
                });
                updated = true;

                _logger.LogInformation(
                    "Aggiornata interazione per farmaco {Name}: {Level}",
                    existing.MedicationName, existing.InteractionLevel);
            }
        }

        return updated;
    }

    /// <summary>
    /// Disattiva tutti i farmaci Milleps di un paziente
    /// </summary>
    private async Task<int> DeactivateAllMillepsMedicationsAsync(int patientId)
    {
        var millepseMedications = await _unitOfWork.Database.Medications
            .Where(m => m.PatientId == patientId &&
                       m.Source == MedicationSource.Milleps &&
                       m.IsActive)
            .ToListAsync();

        foreach (var med in millepseMedications)
        {
            med.IsActive = false;
            med.EndDate = DateTime.Today;
        }

        if (millepseMedications.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation(
                "Disattivati {Count} farmaci Milleps per paziente {PatientId}",
                millepseMedications.Count, patientId);
        }

        return millepseMedications.Count;
    }
}
