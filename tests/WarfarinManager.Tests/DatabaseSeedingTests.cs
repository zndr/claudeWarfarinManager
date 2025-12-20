using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;

namespace WarfarinManager.Tests;

/// <summary>
/// Test per verificare il seeding del database
/// </summary>
public class DatabaseSeedingTests
{
    [Fact]
    public void Database_Should_Contain_Seeded_InteractionDrugs()
    {
        // Arrange
        var factory = new WarfarinDbContextFactory();
        using var context = factory.CreateDbContext(Array.Empty<string>());
        
        // Act
        var drugCount = context.InteractionDrugs.Count();
        var cotrimoxazolo = context.InteractionDrugs
            .FirstOrDefault(d => d.DrugName.Contains("Cotrimoxazolo"));
        
        // Assert
        Assert.Equal(20, drugCount);
        Assert.NotNull(cotrimoxazolo);
        Assert.Equal("High", cotrimoxazolo.InteractionLevel.ToString());
    }
    
    [Fact]
    public void Database_Should_Contain_Seeded_IndicationTypes()
    {
        // Arrange
        var factory = new WarfarinDbContextFactory();
        using var context = factory.CreateDbContext(Array.Empty<string>());
        
        // Act
        var indicationCount = context.IndicationTypes.Count();
        var faIndication = context.IndicationTypes
            .FirstOrDefault(i => i.Code == "FA_STROKE_PREVENTION");
        
        // Assert
        Assert.Equal(23, indicationCount);
        Assert.NotNull(faIndication);
        Assert.Equal(2.0m, faIndication.TargetINRMin);
        Assert.Equal(3.0m, faIndication.TargetINRMax);
    }
    
    [Fact]
    public void Database_Should_Have_All_Tables_Created()
    {
        // Arrange
        var factory = new WarfarinDbContextFactory();
        using var context = factory.CreateDbContext(Array.Empty<string>());
        
        // Act & Assert - verifica che le tabelle esistano
        Assert.True(context.Database.CanConnect());
        
        // Verifica che possiamo interrogare tutte le entità
        var patients = context.Patients.ToList();
        var indications = context.Indications.ToList();
        var medications = context.Medications.ToList();
        var inrControls = context.INRControls.ToList();
        var adverseEvents = context.AdverseEvents.ToList();
        
        // Database vuoto ma struttura ok
        Assert.Empty(patients);
        Assert.Empty(indications);
        Assert.Empty(medications);
    }
}
