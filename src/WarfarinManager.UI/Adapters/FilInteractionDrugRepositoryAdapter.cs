using WarfarinManager.Core.Interfaces;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories.Interfaces;

namespace WarfarinManager.UI.Adapters;

/// <summary>
/// Adapter che implementa l'interfaccia Core usando il repository Data
/// </summary>
public class InteractionDrugRepositoryAdapter : Core.Interfaces.IInteractionDrugRepository
    {
    private readonly Data.Repositories.Interfaces.IInteractionDrugRepository _dataRepo;

    public InteractionDrugRepositoryAdapter(Data.Repositories.Interfaces.IInteractionDrugRepository dataRepo)
        {
        _dataRepo = dataRepo;
        }

    public async Task<InteractionDrug?> FindByNameAsync(string drugName)
        {
        return await _dataRepo.FindByNameAsync(drugName);
        }

    public async Task<IEnumerable<InteractionDrug>> SearchByNameAsync(string partialName, int maxResults = 10)
        {
        return await _dataRepo.SearchByNameAsync(partialName, maxResults);
        }

    public async Task<IEnumerable<InteractionDrug>> GetHighRiskDrugsAsync()
        {
        return await _dataRepo.GetHighRiskDrugsAsync();
        }

    public async Task<IEnumerable<InteractionDrug>> GetByCategoryAsync(string category)
        {
        var all = await _dataRepo.GetAllAsync();
        return all.Where(d => d.Category.ToString() == category);
        }
    public async Task<IEnumerable<InteractionDrug>> GetByEffectAsync(string effect)
        {
        var all = await _dataRepo.GetAllAsync();
        return all.Where(d => d.InteractionEffect.ToString() == effect);
        }
    public async Task<IEnumerable<InteractionDrug>> GetAllAsync()
        {
        return await _dataRepo.GetAllAsync();
        }

    public async Task<bool> ExistsAsync(string drugName)
        {
        var drug = await _dataRepo.FindByNameAsync(drugName);
        return drug != null;
        }
    }