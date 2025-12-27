using Microsoft.Extensions.Logging;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Core.Models;
using WarfarinManager.Shared.Enums;
using InteractionDrug = WarfarinManager.Data.Entities.InteractionDrug;
using IInteractionDrugRepository = WarfarinManager.Data.Repositories.Interfaces.IInteractionDrugRepository;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Implementazione servizio verifica interazioni farmacologiche
/// </summary>
public class InteractionCheckerService : IInteractionCheckerService
{
    private readonly IInteractionDrugRepository _drugRepository;
    private readonly ILogger<InteractionCheckerService> _logger;

    public InteractionCheckerService(
        IInteractionDrugRepository drugRepository,
        ILogger<InteractionCheckerService> logger)
    {
        _drugRepository = drugRepository ?? throw new ArgumentNullException(nameof(drugRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<InteractionCheckResult> CheckInteractionAsync(string medicationName)
    {
        return await CheckInteractionAsync(medicationName, null);
    }

    public async Task<InteractionCheckResult> CheckInteractionAsync(string medicationName, string? activeIngredient)
    {
        if (string.IsNullOrWhiteSpace(medicationName) && string.IsNullOrWhiteSpace(activeIngredient))
        {
            return new InteractionCheckResult
            {
                HasInteraction = false,
                InteractionLevel = InteractionLevel.None,
                Message = "Nome farmaco non specificato"
            };
        }

        try
        {
            InteractionDrug? drug = null;

            // Prima cerca per nome commerciale
            if (!string.IsNullOrWhiteSpace(medicationName))
            {
                drug = await _drugRepository.FindByNameAsync(medicationName.Trim());
            }

            // Se non trovato, cerca per principio attivo
            if (drug == null && !string.IsNullOrWhiteSpace(activeIngredient))
            {
                drug = await _drugRepository.FindByNameAsync(activeIngredient.Trim());
                if (drug != null)
                {
                    _logger.LogInformation(
                        "Farmaco '{MedicationName}' trovato tramite principio attivo '{ActiveIngredient}'",
                        medicationName, activeIngredient);
                }
            }

            if (drug == null)
            {
                _logger.LogInformation(
                    "Farmaco '{MedicationName}' (principio attivo: '{ActiveIngredient}') non trovato nel database interazioni",
                    medicationName, activeIngredient ?? "N/A");

                return new InteractionCheckResult
                {
                    HasInteraction = false,
                    InteractionLevel = InteractionLevel.None,
                    MedicationName = medicationName ?? activeIngredient ?? string.Empty,
                    Message = "Farmaco non presente nel database delle interazioni note"
                };
            }

            var interactionLevel = DetermineInteractionLevel(drug);

            return new InteractionCheckResult
            {
                HasInteraction = true,
                InteractionLevel = interactionLevel,
                MedicationName = drug.DrugName,
                InteractionEffect = drug.InteractionEffect.ToString(),
                Mechanism = drug.Mechanism,
                OddsRatio = drug.OddsRatio,
                FCSAManagement = drug.FCSAManagement,
                ACCPManagement = drug.ACCPManagement,
                RecommendedINRCheckDays = drug.RecommendedINRCheckDays,
                Message = BuildInteractionMessage(drug, interactionLevel)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante verifica interazione per farmaco '{MedicationName}'", medicationName);
            throw;
        }
    }

    public async Task<DoseAdjustmentRecommendation> GetDoseAdjustmentRecommendationAsync(
        string medicationName,
        decimal currentWeeklyDoseMg,
        GuidelineType guideline)
    {
        if (string.IsNullOrWhiteSpace(medicationName))
        {
            throw new ArgumentException("Nome farmaco obbligatorio", nameof(medicationName));
        }

        if (currentWeeklyDoseMg <= 0)
        {
            throw new ArgumentException("Dose settimanale deve essere maggiore di zero", nameof(currentWeeklyDoseMg));
        }

        var drug = await _drugRepository.FindByNameAsync(medicationName.Trim());

        if (drug == null)
        {
            return new DoseAdjustmentRecommendation
            {
                Recommendation = "Farmaco non presente nel database delle interazioni note",
                GuidelineUsed = guideline
            };
        }

        return guideline == GuidelineType.FCSA
            ? BuildFCSARecommendation(drug, currentWeeklyDoseMg)
            : BuildACCPRecommendation(drug, currentWeeklyDoseMg);
    }

    public async Task<int?> GetRecommendedINRCheckDaysAsync(string medicationName)
    {
        if (string.IsNullOrWhiteSpace(medicationName))
        {
            return null;
        }

        var drug = await _drugRepository.FindByNameAsync(medicationName.Trim());
        return drug?.RecommendedINRCheckDays;
    }

    public async Task<List<string>> SearchMedicationsAsync(string partialName, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(partialName) || partialName.Length < 2)
        {
            return new List<string>();
        }

        try
        {
            var drugs = await _drugRepository.SearchByNameAsync(partialName.Trim(), maxResults);
            return drugs.Select(d => d.DrugName).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante ricerca farmaci con pattern '{Pattern}'", partialName);
            return new List<string>();
        }
    }

    #region Private Methods

    private static InteractionLevel DetermineInteractionLevel(InteractionDrug drug)
    {
        // Logica per determinare livello rischio basata su:
        // 1. OddsRatio (se disponibile)
        // 2. Meccanismo interazione
        // 3. Management raccomandato

        // Alto rischio: OR > 2.5 o richiesta riduzione dose >30%
        if (drug.OddsRatio >= 2.5m)
        {
            return InteractionLevel.High;
        }

        // Farmaci ad alto rischio per meccanismo noto
        var highRiskDrugs = new[]
        {
            "Cotrimoxazolo", "Trimetoprim-Sulfametoxazolo",
            "Fluconazolo", "Voriconazolo", "Metronidazolo",
            "Amiodarone"
        };

        if (highRiskDrugs.Any(hrd => drug.DrugName.Contains(hrd, StringComparison.OrdinalIgnoreCase)))
        {
            return InteractionLevel.High;
        }

        // Moderato rischio: OR 1.5-2.5 o management raccomanda riduzione 10-30%
        if (drug.OddsRatio >= 1.5m && drug.OddsRatio < 2.5m)
        {
            return InteractionLevel.Moderate;
        }

        if (drug.FCSAManagement?.Contains("10-25%", StringComparison.OrdinalIgnoreCase) == true ||
            drug.FCSAManagement?.Contains("monitoraggio", StringComparison.OrdinalIgnoreCase) == true)
        {
            return InteractionLevel.Moderate;
        }

        // Basso rischio: tutto il resto con interazione documentata
        return InteractionLevel.Low;
    }

    private static string BuildInteractionMessage(InteractionDrug drug, InteractionLevel level)
    {
        var severity = level switch
        {
            InteractionLevel.High => "ALTO RISCHIO",
            InteractionLevel.Moderate => "MODERATO RISCHIO",
            InteractionLevel.Low => "BASSO RISCHIO",
            _ => "NESSUN RISCHIO"
        };

        var effectDescription = drug.InteractionEffect switch
        {
            Shared.Enums.InteractionEffect.Increases => "aumenta l'effetto anticoagulante (INR ↑)",
            Shared.Enums.InteractionEffect.Decreases => "diminuisce l'effetto anticoagulante (INR ↓)",
            Shared.Enums.InteractionEffect.Variable => "effetto variabile sull'anticoagulazione",
            _ => "interazione documentata"
        };

        return $"⚠️ {severity} - {drug.DrugName} {effectDescription}";
    }

    private DoseAdjustmentRecommendation BuildFCSARecommendation(
        InteractionDrug drug,
        decimal currentWeeklyDoseMg)
    {
        var recommendation = new DoseAdjustmentRecommendation
        {
            GuidelineUsed = GuidelineType.FCSA,
            RecommendedINRCheckDays = drug.RecommendedINRCheckDays
        };

        // Parsing raccomandazioni FCSA
        if (string.IsNullOrEmpty(drug.FCSAManagement))
        {
            recommendation.Recommendation = "Gestione specifica non disponibile. Monitorare INR.";
            recommendation.RecommendedINRCheckDays ??= 7;
            return recommendation;
        }

        var management = drug.FCSAManagement;

        // Estrazione percentuale riduzione/aumento suggerita
        decimal? percentageChange = null;

        if (management.Contains("25-40%"))
            percentageChange = drug.InteractionEffect == Shared.Enums.InteractionEffect.Increases ? -32.5m : 32.5m; // Media
        else if (management.Contains("20-30%"))
            percentageChange = drug.InteractionEffect == Shared.Enums.InteractionEffect.Increases ? -25m : 25m;
        else if (management.Contains("10-25%"))
            percentageChange = drug.InteractionEffect == Shared.Enums.InteractionEffect.Increases ? -17.5m : 17.5m;
        else if (management.Contains("10-15%"))
            percentageChange = drug.InteractionEffect == Shared.Enums.InteractionEffect.Increases ? -12.5m : 12.5m;
        else if (management.Contains("5-10%"))
            percentageChange = drug.InteractionEffect == Shared.Enums.InteractionEffect.Increases ? -7.5m : 7.5m;
        else if (management.Contains("50-100%"))
            percentageChange = drug.InteractionEffect == Shared.Enums.InteractionEffect.Decreases ? 75m : -75m;

        if (percentageChange.HasValue)
        {
            recommendation.PercentageAdjustment = percentageChange.Value;
            recommendation.SuggestedWeeklyDoseMg = 
                Math.Round(currentWeeklyDoseMg * (1 + percentageChange.Value / 100m), 1);
        }

        recommendation.Recommendation = drug.FCSAManagement;
        recommendation.Notes = !string.IsNullOrEmpty(drug.Mechanism)
            ? $"Meccanismo: {drug.Mechanism}"
            : string.Empty;

        // Default INR check se non specificato
        recommendation.RecommendedINRCheckDays ??= DetermineInteractionLevel(drug) switch
        {
            InteractionLevel.High => 3,
            InteractionLevel.Moderate => 5,
            _ => 7
        };

        return recommendation;
    }

    private DoseAdjustmentRecommendation BuildACCPRecommendation(
        InteractionDrug drug,
        decimal currentWeeklyDoseMg)
    {
        var recommendation = new DoseAdjustmentRecommendation
        {
            GuidelineUsed = GuidelineType.ACCP,
            RecommendedINRCheckDays = drug.RecommendedINRCheckDays
        };

        if (string.IsNullOrEmpty(drug.ACCPManagement))
        {
            // Fallback a raccomandazioni FCSA se ACCP non disponibile
            recommendation.Recommendation = "Linee guida ACCP non specifiche. Riferirsi a FCSA.";
            recommendation.Notes = drug.FCSAManagement ?? "Monitorare INR strettamente";
            recommendation.RecommendedINRCheckDays ??= 7;
            return recommendation;
        }

        var management = drug.ACCPManagement;

        // Le linee guida ACCP sono generalmente più conservative
        // Spesso raccomandano monitoraggio senza aggiustamento empirico
        if (management.Contains("monitoraggio", StringComparison.OrdinalIgnoreCase) ||
            management.Contains("monitoring", StringComparison.OrdinalIgnoreCase))
        {
            recommendation.Recommendation = management;
            recommendation.Notes = "ACCP raccomanda approccio conservativo con monitoraggio frequente";
            recommendation.RecommendedINRCheckDays ??= 5;
        }
        else
        {
            // Parsing simile a FCSA se ci sono percentuali
            recommendation.Recommendation = management;
            recommendation.RecommendedINRCheckDays ??= 7;
        }

        return recommendation;
    }

    #endregion
}
