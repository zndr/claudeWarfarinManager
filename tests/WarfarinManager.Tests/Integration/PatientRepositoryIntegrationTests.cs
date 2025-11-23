using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Tests.Integration;

/// <summary>
/// Test integrazione PatientRepository con database reale
/// </summary>
public class PatientRepositoryIntegrationTests : IDisposable
{
    private readonly WarfarinDbContext _context;
    private readonly PatientRepository _repository;

    public PatientRepositoryIntegrationTests()
    {
        _context = TestDbContextFactory.CreateSqliteInMemoryContext();
        _repository = new PatientRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistPatient()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "Mario",
            LastName = "Rossi",
            BirthDate = new DateTime(1960, 1, 1),
            FiscalCode = "RSSMRA60A01H501Z",
            Gender = Gender.Male,
            Phone = "0123456789",
            Email = "mario.rossi@email.it"
        };

        // Act
        await _repository.AddAsync(patient);
        await _context.SaveChangesAsync();

        // Assert
        patient.Id.Should().BeGreaterThan(0, "l'ID deve essere generato");
        
        var retrieved = await _repository.GetByIdAsync(patient.Id);
        retrieved.Should().NotBeNull();
        retrieved!.FullName.Should().Be("Rossi Mario");
        retrieved.Age.Should().BeGreaterThan(60);
    }

    [Fact]
    public async Task GetByFiscalCodeAsync_ShouldReturnCorrectPatient()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "Giovanni",
            LastName = "Bianchi",
            BirthDate = new DateTime(1970, 5, 15),
            FiscalCode = "BNCGNN70E15H501A"
        };
        
        await _repository.AddAsync(patient);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByFiscalCodeAsync("BNCGNN70E15H501A");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.FirstName.Should().Be("Giovanni");
        retrieved.LastName.Should().Be("Bianchi");
    }

    [Fact]
    public async Task GetPatientsWithActiveIndicationsAsync_ShouldIncludeRelatedData()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "Test",
            LastName = "Patient",
            BirthDate = DateTime.Today.AddYears(-65),
            FiscalCode = "TSTPTN59A01H501B"
        };

        var indication = new Indication
        {
            Patient = patient,
            IndicationTypeCode = "FA_PREVENTION",
            TargetINRMin = 2.0m,
            TargetINRMax = 3.0m,
            StartDate = DateTime.Today.AddMonths(-6),
            IsActive = true
        };

        patient.Indications.Add(indication);
        
        await _repository.AddAsync(patient);
        await _context.SaveChangesAsync();

        // Act
        var patientsWithIndications = await _repository.GetPatientsWithActiveIndicationsAsync();

        // Assert
        patientsWithIndications.Should().NotBeEmpty();
        var retrievedPatient = patientsWithIndications.First(p => p.FiscalCode == "TSTPTN59A01H501B");
        retrievedPatient.Indications.Should().NotBeEmpty();
        retrievedPatient.Indications.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetPatientsWithRecentINRAsync_ShouldFilterCorrectly()
    {
        // Arrange
        var patientWithRecentINR = new Patient
        {
            FirstName = "Recent",
            LastName = "INR",
            BirthDate = DateTime.Today.AddYears(-70),
            FiscalCode = "RCNINR54A01H501C"
        };

        var recentControl = new INRControl
        {
            Patient = patientWithRecentINR,
            ControlDate = DateTime.Today.AddDays(-5),
            INRValue = 2.5m,
            CurrentWeeklyDose = 35.0m,
            PhaseOfTherapy = TherapyPhase.Maintenance,
            IsCompliant = true
        };

        patientWithRecentINR.INRControls.Add(recentControl);

        var patientWithOldINR = new Patient
        {
            FirstName = "Old",
            LastName = "INR",
            BirthDate = DateTime.Today.AddYears(-68),
            FiscalCode = "OLDINR56A01H501D"
        };

        var oldControl = new INRControl
        {
            Patient = patientWithOldINR,
            ControlDate = DateTime.Today.AddDays(-50),
            INRValue = 2.3m,
            CurrentWeeklyDose = 30.0m,
            PhaseOfTherapy = TherapyPhase.Maintenance,
            IsCompliant = true
        };

        patientWithOldINR.INRControls.Add(oldControl);

        await _repository.AddAsync(patientWithRecentINR);
        await _repository.AddAsync(patientWithOldINR);
        await _context.SaveChangesAsync();

        // Act
        var patientsWithRecent = await _repository.GetPatientsWithRecentINRAsync(30);

        // Assert
        patientsWithRecent.Should().Contain(p => p.FiscalCode == "RCNINR54A01H501C");
        patientsWithRecent.Should().NotContain(p => p.FiscalCode == "OLDINR56A01H501D");
    }

    [Fact]
    public async Task SearchPatientsAsync_ShouldFindByMultipleCriteria()
    {
        // Arrange
        await SeedTestPatients();

        // Act - Search by LastName
        var byLastName = await _repository.SearchPatientsAsync("Rossi");
        byLastName.Should().Contain(p => p.LastName == "Rossi");

        // Act - Search by FirstName
        var byFirstName = await _repository.SearchPatientsAsync("Maria");
        byFirstName.Should().Contain(p => p.FirstName == "Maria");

        // Act - Search by FiscalCode fragment
        var byFiscalCode = await _repository.SearchPatientsAsync("VRD");
        byFiscalCode.Should().Contain(p => p.FiscalCode.Contains("VRD"));
    }

    [Fact]
    public async Task UpdateSlowMetabolizerFlag_ShouldWorkCorrectly()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "Slow",
            LastName = "Metabolizer",
            BirthDate = DateTime.Today.AddYears(-80),
            FiscalCode = "SLWMTB44A01H501E",
            IsSlowMetabolizer = false
        };

        await _repository.AddAsync(patient);
        await _context.SaveChangesAsync();

        // Act - Simula paziente che passa a <15 mg/settimana
        patient.IsSlowMetabolizer = true;
        await _repository.UpdateAsync(patient);
        await _context.SaveChangesAsync();

        // Assert
        var updated = await _repository.GetByIdAsync(patient.Id);
        updated!.IsSlowMetabolizer.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldCascadeToRelatedEntities()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "Delete",
            LastName = "Test",
            BirthDate = DateTime.Today.AddYears(-60),
            FiscalCode = "DLTTST64A01H501F"
        };

        var indication = new Indication
        {
            Patient = patient,
            IndicationTypeCode = "TVP_TREATMENT",
            TargetINRMin = 2.0m,
            TargetINRMax = 3.0m,
            StartDate = DateTime.Today,
            IsActive = true
        };

        patient.Indications.Add(indication);
        
        await _repository.AddAsync(patient);
        await _context.SaveChangesAsync();

        var patientId = patient.Id;

        // Act
        await _repository.DeleteAsync(patient);
        await _context.SaveChangesAsync();

        // Assert
        var deleted = await _repository.GetByIdAsync(patientId);
        deleted.Should().BeNull();

        // Verifica cascade delete sulle indicazioni
        var orphanedIndications = await _context.Indications
            .Where(i => i.PatientId == patientId)
            .ToListAsync();
        orphanedIndications.Should().BeEmpty("cascade delete deve rimuovere indicazioni");
    }

    private async Task SeedTestPatients()
    {
        var patients = new[]
        {
            new Patient
            {
                FirstName = "Mario",
                LastName = "Rossi",
                BirthDate = new DateTime(1960, 1, 1),
                FiscalCode = "RSSMRA60A01H501Z"
            },
            new Patient
            {
                FirstName = "Maria",
                LastName = "Verdi",
                BirthDate = new DateTime(1965, 3, 15),
                FiscalCode = "VRDMRA65C15H501A"
            },
            new Patient
            {
                FirstName = "Giuseppe",
                LastName = "Bianchi",
                BirthDate = new DateTime(1955, 7, 20),
                FiscalCode = "BNCGPP55L20H501B"
            }
        };

        foreach (var patient in patients)
        {
            await _repository.AddAsync(patient);
        }
        
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
