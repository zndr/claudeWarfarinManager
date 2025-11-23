using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Tests.Integration;

/// <summary>
/// Factory per creare DbContext di test (in-memory)
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Crea un DbContext in-memory per test veloci
    /// Usa TestWarfarinDbContext per evitare problemi con seeding automatico
    /// </summary>
    public static TestWarfarinDbContext CreateInMemoryContext(string? databaseName = null)
    {
        databaseName ??= $"TestDb_{Guid.NewGuid()}";
        
        var options = new DbContextOptionsBuilder<WarfarinDbContext>()
            .UseInMemoryDatabase(databaseName)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new TestWarfarinDbContext(options);
        
        // Assicura che il database sia creato
        context.Database.EnsureCreated();
        
        // Seed manuale dei dati lookup necessari per i test
        SeedTestData(context);
        
        return context;
    }

    /// <summary>
    /// Crea un DbContext SQLite in-memory per test con schema reale
    /// </summary>
    public static TestWarfarinDbContext CreateSqliteInMemoryContext()
    {
        return CreateInMemoryContext();
    }
    
    /// <summary>
    /// Seed minimo di dati lookup per i test
    /// </summary>
    private static void SeedTestData(TestWarfarinDbContext context)
    {
        // Verifica se già seedato
        if (context.IndicationTypes.Any())
        {
            return;
        }
        
        // Seed IndicationTypes essenziali per i test
        var indicationTypes = new[]
        {
            new Data.Entities.IndicationType
            {
                Code = "FA_PREVENTION",
                Category = "FIBRILLAZIONE ATRIALE",
                Description = "Fibrillazione atriale - Prevenzione stroke",
                TargetINRMin = 2.0m,
                TargetINRMax = 3.0m,
                TypicalDuration = "Indefinita"
            },
            new Data.Entities.IndicationType
            {
                Code = "TVP_TREATMENT",
                Category = "TROMBOEMBOLISMO VENOSO",
                Description = "TVP - Trattamento",
                TargetINRMin = 2.0m,
                TargetINRMax = 3.0m,
                TypicalDuration = "3-6 mesi"
            },
            new Data.Entities.IndicationType
            {
                Code = "MECHANICAL_VALVE",
                Category = "PROTESI VALVOLARI",
                Description = "Protesi valvolari meccaniche",
                TargetINRMin = 2.5m,
                TargetINRMax = 3.5m,
                TypicalDuration = "Indefinita"
            }
        };
        
        context.IndicationTypes.AddRange(indicationTypes);
        
        // Seed InteractionDrugs essenziali per i test
        var interactionDrugs = new[]
        {
            new Data.Entities.InteractionDrug
            {
                DrugName = "Amiodarone",
                Category = "Antiaritmico",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.High,
                OddsRatio = 2.0m,
                Mechanism = "Inibizione CYP2C9",
                FCSAManagement = "Ridurre dose 20-30%",
                ACCPManagement = "Monitoraggio stretto",
                RecommendedINRCheckDays = 3
            },
            new Data.Entities.InteractionDrug
            {
                DrugName = "Cotrimoxazolo",
                Category = "Antibiotico",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.High,
                OddsRatio = 2.70m,
                Mechanism = "Inibizione CYP2C9 + sintesi Vit K",
                FCSAManagement = "Ridurre dose 25-40%",
                ACCPManagement = "Considerare alternativa",
                RecommendedINRCheckDays = 3
            },
            new Data.Entities.InteractionDrug
            {
                DrugName = "Fluconazolo",
                Category = "Antifungino",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.High,
                OddsRatio = 4.57m,
                Mechanism = "Inibizione CYP2C9",
                FCSAManagement = "Ridurre dose 25-40%",
                ACCPManagement = "Considerare alternativa",
                RecommendedINRCheckDays = 3
            }
        };
        
        context.InteractionDrugs.AddRange(interactionDrugs);
        
        // Salva
        context.SaveChanges();
    }
}
