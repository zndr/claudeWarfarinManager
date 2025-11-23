using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Tests.Integration;

/// <summary>
/// Test creazione database e validazione seeding dati lookup
/// </summary>
public class DatabaseCreationAndSeedingTests : IDisposable
{
    private readonly WarfarinDbContext _context;

    public DatabaseCreationAndSeedingTests()
    {
        _context = TestDbContextFactory.CreateSqliteInMemoryContext();
    }

    [Fact]
    public void Database_ShouldBeCreated_WithAllTables()
    {
        // Arrange & Act
        var canConnect = _context.Database.CanConnect();

        // Assert
        canConnect.Should().BeTrue("il database deve essere creato correttamente");
        
        // Verifica che tutte le tabelle siano create
        var tableNames = _context.Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();

        tableNames.Should().Contain("Patients");
        tableNames.Should().Contain("Indications");
        tableNames.Should().Contain("Medications");
        tableNames.Should().Contain("INRControls");
        tableNames.Should().Contain("DailyDoses");
        tableNames.Should().Contain("DosageSuggestions");
        tableNames.Should().Contain("AdverseEvents");
        tableNames.Should().Contain("BridgeTherapyPlans");
        tableNames.Should().Contain("IndicationTypes");
        tableNames.Should().Contain("InteractionDrugs");
    }

    [Fact]
    public async Task InteractionDrugs_ShouldBeSeeeded_WithCorrectData()
    {
        // Act
        var drugs = await _context.InteractionDrugs.ToListAsync();

        // Assert
        drugs.Should().NotBeEmpty("i dati di seeding devono essere presenti");
        drugs.Should().HaveCountGreaterThan(15, "ci devono essere almeno 16 farmaci critici");

        // Verifica farmaci ad ALTO RISCHIO specifici del PRD
        var cotrimoxazolo = drugs.FirstOrDefault(d => d.DrugName.Contains("Cotrimoxazolo"));
        cotrimoxazolo.Should().NotBeNull();
        cotrimoxazolo!.InteractionLevel.Should().Be(InteractionLevel.High);
        cotrimoxazolo.InteractionEffect.Should().Be(InteractionEffect.Increases);
        cotrimoxazolo.RecommendedINRCheckDays.Should().Be(3);

        var fluconazolo = drugs.FirstOrDefault(d => d.DrugName.Contains("Fluconazolo"));
        fluconazolo.Should().NotBeNull();
        fluconazolo!.OddsRatio.Should().Be(4.57m, "come specificato nel PRD");

        var amiodarone = drugs.FirstOrDefault(d => d.DrugName.Contains("Amiodarone"));
        amiodarone.Should().NotBeNull();
        amiodarone!.FCSAManagement.Should().Contain("20-30%", "riduzione empirica iniziale");

        // Verifica farmaci che RIDUCONO INR
        var rifampicina = drugs.FirstOrDefault(d => d.DrugName.Contains("Rifampicina"));
        rifampicina.Should().NotBeNull();
        rifampicina!.InteractionEffect.Should().Be(InteractionEffect.Decreases);
        rifampicina.FCSAManagement.Should().Contain("100%", "può richiedere aumento fino a 100%");
    }

    [Fact]
    public async Task IndicationTypes_ShouldBeSeeded_WithAllCategories()
    {
        // Act
        var indications = await _context.IndicationTypes.ToListAsync();

        // Assert
        indications.Should().NotBeEmpty();
        indications.Should().HaveCountGreaterThan(10, "devono esserci tutte le indicazioni principali");

        // Verifica categorie principali
        var categories = indications.Select(i => i.Category).Distinct().ToList();
        categories.Should().Contain("TROMBOEMBOLISMO VENOSO");
        categories.Should().Contain("FIBRILLAZIONE ATRIALE");
        categories.Should().Contain("PROTESI VALVOLARI");

        // Verifica target INR corretti
        var fa = indications.FirstOrDefault(i => i.Description.Contains("Fibrillazione atriale"));
        if (fa != null)
        {
            fa.TargetINRMin.Should().Be(2.0m);
            fa.TargetINRMax.Should().Be(3.0m);
        }

        var protesiMeccaniche = indications.FirstOrDefault(i => 
            i.Description.Contains("Protesi") && i.Description.Contains("meccaniche"));
        if (protesiMeccaniche != null)
        {
            protesiMeccaniche.TargetINRMin.Should().Be(2.5m);
            protesiMeccaniche.TargetINRMax.Should().Be(3.5m);
        }
    }

    [Fact]
    public void Database_Indexes_ShouldBeConfigured()
    {
        // Arrange
        var patientEntity = _context.Model.FindEntityType(typeof(WarfarinManager.Data.Entities.Patient));
        
        // Assert
        patientEntity.Should().NotBeNull();
        
        var indexes = patientEntity!.GetIndexes().ToList();
        indexes.Should().NotBeEmpty("devono esserci indici configurati");

        // Verifica indice su FiscalCode (unique)
        var fiscalCodeIndex = indexes.FirstOrDefault(i => 
            i.Properties.Any(p => p.Name == "FiscalCode"));
        fiscalCodeIndex.Should().NotBeNull("deve esserci indice su FiscalCode");
        fiscalCodeIndex!.IsUnique.Should().BeTrue("FiscalCode deve essere univoco");

        // Verifica indice composito su LastName + FirstName
        var nameIndex = indexes.FirstOrDefault(i => 
            i.Properties.Any(p => p.Name == "LastName") && 
            i.Properties.Any(p => p.Name == "FirstName"));
        nameIndex.Should().NotBeNull("deve esserci indice su cognome+nome");
    }

    [Fact]
    public async Task Database_Constraints_ShouldWork()
    {
        // Arrange
        var patient = new WarfarinManager.Data.Entities.Patient
        {
            FirstName = "Mario",
            LastName = "Rossi",
            BirthDate = new DateTime(1960, 1, 1),
            FiscalCode = "RSSMRA60A01H501Z"
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        // Act - Tentativo di inserire duplicato FiscalCode
        var duplicatePatient = new WarfarinManager.Data.Entities.Patient
        {
            FirstName = "Giovanni",
            LastName = "Bianchi",
            BirthDate = new DateTime(1970, 1, 1),
            FiscalCode = "RSSMRA60A01H501Z" // Stesso CF!
        };

        _context.Patients.Add(duplicatePatient);
        var act = async () => await _context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>(
            "non deve permettere FiscalCode duplicati");
    }

    [Fact]
    public async Task EnumConversions_ShouldStoreAsStrings()
    {
        // Arrange
        var patient = new WarfarinManager.Data.Entities.Patient
        {
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Today.AddYears(-50),
            FiscalCode = "TSTURS74A01H501X",
            Gender = Gender.Male
        };

        // Act
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        // Verifica nel database raw
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Gender FROM Patients WHERE FiscalCode = 'TSTURS74A01H501X'";
        var result = await command.ExecuteScalarAsync();

        // Assert
        result.Should().NotBeNull();
        result.ToString().Should().Be("Male", "enum deve essere salvato come stringa");
    }

    [Fact]
    public async Task Timestamps_ShouldBeAutoPopulated()
    {
        // Arrange
        var beforeSave = DateTime.UtcNow;
        
        var patient = new WarfarinManager.Data.Entities.Patient
        {
            FirstName = "Test",
            LastName = "Timestamp",
            BirthDate = DateTime.Today.AddYears(-40),
            FiscalCode = "TSTTMS84A01H501Y"
        };

        // Act
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        var afterSave = DateTime.UtcNow;

        // Assert
        patient.CreatedAt.Should().BeAfter(beforeSave.AddSeconds(-1));
        patient.CreatedAt.Should().BeBefore(afterSave.AddSeconds(1));
        patient.UpdatedAt.Should().BeCloseTo(patient.CreatedAt, TimeSpan.FromSeconds(1));

        // Test update
        patient.FirstName = "Updated";
        var beforeUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        var afterUpdate = DateTime.UtcNow;

        patient.UpdatedAt.Should().BeAfter(beforeUpdate.AddSeconds(-1));
        patient.UpdatedAt.Should().BeBefore(afterUpdate.AddSeconds(1));
        patient.UpdatedAt.Should().BeAfter(patient.CreatedAt);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
