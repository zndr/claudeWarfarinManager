using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories;
using WarfarinManager.Shared.Enums;
using WarfarinManager.Core.Services;

namespace WarfarinManager.Tests.Integration;

/// <summary>
/// Test integrazione INRControlRepository con calcolo TTR da database
/// </summary>
public class INRControlRepositoryIntegrationTests : IDisposable
{
    private readonly WarfarinDbContext _context;
    private readonly INRControlRepository _repository;
    private readonly PatientRepository _patientRepository;
    private readonly TTRCalculatorService _ttrCalculator;

    public INRControlRepositoryIntegrationTests()
    {
        _context = TestDbContextFactory.CreateSqliteInMemoryContext();
        _repository = new INRControlRepository(_context);
        _patientRepository = new PatientRepository(_context);
        _ttrCalculator = new TTRCalculatorService(NullLogger<TTRCalculatorService>.Instance);
    }

    [Fact]
    public async Task AddINRControl_WithDailyDoses_ShouldPersist()
    {
        // Arrange
        var patient = await CreateTestPatient();

        var control = new INRControl
        {
            PatientId = patient.Id,
            ControlDate = DateTime.Today,
            INRValue = 2.5m,
            CurrentWeeklyDose = 35.0m,
            PhaseOfTherapy = TherapyPhase.Maintenance,
            IsCompliant = true,
            Notes = "Controllo regolare"
        };

        // Aggiungi dosi giornaliere
        for (int day = 1; day <= 7; day++)
        {
            control.DailyDoses.Add(new DailyDose
            {
                DayOfWeek = day,
                DoseMg = 5.0m
            });
        }

        // Act
        await _repository.AddAsync(control);
        await _context.SaveChangesAsync();

        // Assert
        control.Id.Should().BeGreaterThan(0);
        
        var retrieved = await _repository.GetByIdAsync(control.Id);
        retrieved.Should().NotBeNull();
        retrieved!.DailyDoses.Should().HaveCount(7);
        retrieved.DailyDoses.Sum(d => d.DoseMg).Should().Be(35.0m);
    }

    [Fact]
    public async Task GetPatientINRHistoryAsync_ShouldReturnOrderedByDate()
    {
        // Arrange
        var patient = await CreateTestPatient();
        await CreateINRHistory(patient.Id);

        // Act
        var history = await _repository.GetPatientINRHistoryAsync(patient.Id);

        // Assert
        history.Should().HaveCount(5);
        history.Should().BeInDescendingOrder(c => c.ControlDate, "deve essere ordinato per data DESC");
        
        // Verifica che il più recente sia il primo
        history.First().ControlDate.Should().Be(DateTime.Today);
        history.Last().ControlDate.Should().Be(DateTime.Today.AddMonths(-4));
    }

    [Fact]
    public async Task GetINRControlsInDateRangeAsync_ShouldFilterCorrectly()
    {
        // Arrange
        var patient = await CreateTestPatient();
        await CreateINRHistory(patient.Id);

        var startDate = DateTime.Today.AddMonths(-2);
        var endDate = DateTime.Today.AddDays(-15);

        // Act
        var filtered = await _repository.GetINRControlsInDateRangeAsync(
            patient.Id, startDate, endDate);

        // Assert
        filtered.Should().NotBeEmpty();
        filtered.Should().OnlyContain(c => 
            c.ControlDate >= startDate && c.ControlDate <= endDate);
    }

    [Fact]
    public async Task CalculateTTR_FromDatabaseControls_ShouldBeAccurate()
    {
        // Arrange
        var patient = await CreateTestPatient();
        
        // Crea uno storico INR realistico per target 2.0-3.0
        var controls = new[]
        {
            CreateControl(patient.Id, DateTime.Today.AddDays(-90), 2.5m, 35.0m),  // In range
            CreateControl(patient.Id, DateTime.Today.AddDays(-76), 2.8m, 35.0m),  // In range
            CreateControl(patient.Id, DateTime.Today.AddDays(-62), 3.2m, 32.5m),  // Sopra range
            CreateControl(patient.Id, DateTime.Today.AddDays(-48), 2.6m, 32.5m),  // In range
            CreateControl(patient.Id, DateTime.Today.AddDays(-34), 2.4m, 32.5m),  // In range
            CreateControl(patient.Id, DateTime.Today.AddDays(-20), 1.8m, 35.0m),  // Sotto range
            CreateControl(patient.Id, DateTime.Today.AddDays(-6), 2.3m, 35.0m)    // In range
        };

        foreach (var control in controls)
        {
            await _repository.AddAsync(control);
        }
        await _context.SaveChangesAsync();

        // Act
        var history = await _repository.GetPatientINRHistoryAsync(patient.Id);
        
        // Converti in modelli Core per TTR calculator
        var coreControls = history.Select(h => new Core.Models.INRControl
        {
            ControlDate = h.ControlDate,
            INRValue = h.INRValue
        }).ToList();

        var ttrResult = _ttrCalculator.CalculateTTR(coreControls, 2.0m, 3.0m);

        // Assert
        ttrResult.Should().NotBeNull();
        ttrResult.TTRPercentage.Should().BeGreaterThan(0);
        ttrResult.TTRPercentage.Should().BeLessThan(100);
        ttrResult.TotalDays.Should().Be(90, "dal primo all'ultimo controllo");
        
        // Verifica classificazione qualità
        if (ttrResult.TTRPercentage >= 70)
        {
            ttrResult.Quality.Should().Be(Core.Models.TTRQuality.Excellent);
        }
        else if (ttrResult.TTRPercentage >= 60)
        {
            ttrResult.Quality.Should().Be(Core.Models.TTRQuality.Acceptable);
        }
    }

    [Fact]
    public async Task GetControlsRequiringFollowUp_ShouldIdentifyOutOfRange()
    {
        // Arrange
        var patient = await CreateTestPatient();
        
        var recentOutOfRange = CreateControl(
            patient.Id, DateTime.Today.AddDays(-3), 3.8m, 35.0m);
        
        var oldOutOfRange = CreateControl(
            patient.Id, DateTime.Today.AddDays(-50), 1.5m, 40.0m);
        
        var recentInRange = CreateControl(
            patient.Id, DateTime.Today.AddDays(-2), 2.5m, 32.5m);

        await _repository.AddAsync(recentOutOfRange);
        await _repository.AddAsync(oldOutOfRange);
        await _repository.AddAsync(recentInRange);
        await _context.SaveChangesAsync();

        // Act
        var needsFollowUp = await _repository.GetControlsRequiringFollowUpAsync(
            daysThreshold: 7, targetMin: 2.0m, targetMax: 3.0m);

        // Assert
        needsFollowUp.Should().Contain(c => c.INRValue == 3.8m, 
            "controllo recente fuori range deve richiedere follow-up");
        needsFollowUp.Should().NotContain(c => c.INRValue == 1.5m,
            "controllo vecchio non deve essere nel follow-up");
        needsFollowUp.Should().NotContain(c => c.INRValue == 2.5m,
            "controllo in range non richiede follow-up");
    }

    [Fact]
    public async Task GetLastINRControl_ShouldReturnMostRecent()
    {
        // Arrange
        var patient = await CreateTestPatient();
        await CreateINRHistory(patient.Id);

        // Act
        var lastControl = await _repository.GetLastINRControlAsync(patient.Id);

        // Assert
        lastControl.Should().NotBeNull();
        lastControl!.ControlDate.Should().Be(DateTime.Today);
    }

    [Fact]
    public async Task ComplexQuery_PatientsWithTTRBelowThreshold_ShouldWork()
    {
        // Arrange - Crea 3 pazienti con diversi TTR
        
        // Paziente 1: TTR alto (>70%)
        var patient1 = await CreateTestPatient("HGHTTR60A01H501A");
        await CreateStableINRHistory(patient1.Id, 2.5m, count: 10); // Tutti in range
        
        // Paziente 2: TTR medio (~60%)
        var patient2 = await CreateTestPatient("MDMTTR65A01H501B");
        await CreateVariableINRHistory(patient2.Id); // Mix in/out range
        
        // Paziente 3: TTR basso (<50%)
        var patient3 = await CreateTestPatient("LWTTR70A01H501C");
        await CreateInstableINRHistory(patient3.Id); // Maggiormente fuori range

        // Act - Calcola TTR per tutti i pazienti
        var allPatients = await _patientRepository.GetAllAsync();
        
        var patientsWithLowTTR = new List<Patient>();
        
        foreach (var patient in allPatients)
        {
            var controls = await _repository.GetPatientINRHistoryAsync(patient.Id);
            if (controls.Count() >= 2)
            {
                var coreControls = controls.Select(c => new Core.Models.INRControl
                {
                    ControlDate = c.ControlDate,
                    INRValue = c.INRValue
                }).ToList();

                var ttr = _ttrCalculator.CalculateTTR(coreControls, 2.0m, 3.0m);
                
                if (ttr.TTRPercentage < 60)
                {
                    patientsWithLowTTR.Add(patient);
                }
            }
        }

        // Assert
        patientsWithLowTTR.Should().Contain(p => p.FiscalCode == "LWTTR70A01H501C",
            "paziente con TTR basso deve essere identificato");
        patientsWithLowTTR.Should().NotContain(p => p.FiscalCode == "HGHTTR60A01H501A",
            "paziente con TTR alto non deve essere nella lista");
    }

    // Helper methods

    private async Task<Patient> CreateTestPatient(string? fiscalCode = null)
    {
        var patient = new Patient
        {
            FirstName = "Test",
            LastName = "Patient",
            BirthDate = DateTime.Today.AddYears(-65),
            FiscalCode = fiscalCode ?? "TSTPTN59A01H501Z"
        };

        await _patientRepository.AddAsync(patient);
        await _context.SaveChangesAsync();
        
        return patient;
    }

    private async Task CreateINRHistory(int patientId)
    {
        var controls = new[]
        {
            CreateControl(patientId, DateTime.Today, 2.5m, 35.0m),
            CreateControl(patientId, DateTime.Today.AddMonths(-1), 2.3m, 35.0m),
            CreateControl(patientId, DateTime.Today.AddMonths(-2), 2.7m, 32.5m),
            CreateControl(patientId, DateTime.Today.AddMonths(-3), 2.4m, 35.0m),
            CreateControl(patientId, DateTime.Today.AddMonths(-4), 2.6m, 35.0m)
        };

        foreach (var control in controls)
        {
            await _repository.AddAsync(control);
        }
        
        await _context.SaveChangesAsync();
    }

    private async Task CreateStableINRHistory(int patientId, decimal inrValue, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var control = CreateControl(
                patientId, 
                DateTime.Today.AddDays(-14 * i), 
                inrValue, 
                35.0m);
            
            await _repository.AddAsync(control);
        }
        
        await _context.SaveChangesAsync();
    }

    private async Task CreateVariableINRHistory(int patientId)
    {
        var inrValues = new[] { 2.5m, 1.9m, 2.7m, 2.2m, 3.1m, 2.4m, 1.8m, 2.6m };
        
        for (int i = 0; i < inrValues.Length; i++)
        {
            var control = CreateControl(
                patientId,
                DateTime.Today.AddDays(-14 * i),
                inrValues[i],
                35.0m);
            
            await _repository.AddAsync(control);
        }
        
        await _context.SaveChangesAsync();
    }

    private async Task CreateInstableINRHistory(int patientId)
    {
        var inrValues = new[] { 3.5m, 1.6m, 3.8m, 1.5m, 3.2m, 1.7m, 3.6m, 1.8m };
        
        for (int i = 0; i < inrValues.Length; i++)
        {
            var control = CreateControl(
                patientId,
                DateTime.Today.AddDays(-14 * i),
                inrValues[i],
                37.5m);
            
            await _repository.AddAsync(control);
        }
        
        await _context.SaveChangesAsync();
    }

    private static INRControl CreateControl(int patientId, DateTime date, decimal inr, decimal dose)
    {
        return new INRControl
        {
            PatientId = patientId,
            ControlDate = date,
            INRValue = inr,
            CurrentWeeklyDose = dose,
            PhaseOfTherapy = TherapyPhase.Maintenance,
            IsCompliant = true
        };
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
