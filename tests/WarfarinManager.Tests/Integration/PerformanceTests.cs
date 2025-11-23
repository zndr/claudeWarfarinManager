using FluentAssertions;
using System.Diagnostics;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Tests.Integration;

/// <summary>
/// Test performance con dataset realistici
/// </summary>
public class PerformanceTests : IDisposable
{
    private readonly WarfarinDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public PerformanceTests()
    {
        _context = TestDbContextFactory.CreateSqliteInMemoryContext();
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task LoadPatientList_500Patients_ShouldBeFast()
    {
        // Arrange
        await SeedPatients(500);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var patients = await _unitOfWork.Patients.GetAllAsync();
        stopwatch.Stop();

        // Assert
        patients.Should().HaveCount(500);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, 
            "caricamento 500 pazienti deve essere <1 secondo come da PRD");
    }

    [Fact]
    public async Task LoadPatientDetails_WithRelations_ShouldBeFast()
    {
        // Arrange
        var patient = await CreateFullyPopulatedPatient();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var loaded = await _unitOfWork.Patients.GetByIdAsync(patient.Id);
        stopwatch.Stop();

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Indications.Should().NotBeEmpty();
        loaded.INRControls.Should().NotBeEmpty();
        loaded.Medications.Should().NotBeEmpty();
        
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, 
            "caricamento paziente completo deve essere <500ms come da PRD");
    }

    [Fact]
    public async Task SearchPatients_InLargeDataset_ShouldBeFast()
    {
        // Arrange
        await SeedPatients(500);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var results = await _unitOfWork.Patients.SearchPatientsAsync("Rossi");
        stopwatch.Stop();

        // Assert
        results.Should().NotBeEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, 
            "ricerca deve essere veloce grazie agli indici");
    }

    [Fact]
    public async Task GetPatientINRHistory_LargeHistory_ShouldBeFast()
    {
        // Arrange
        var patient = await CreateTestPatient();
        await CreateLargeINRHistory(patient.Id, 100);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var history = await _unitOfWork.INRControls.GetPatientINRHistoryAsync(patient.Id);
        stopwatch.Stop();

        // Assert
        history.Should().HaveCount(100);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200, 
            "caricamento storico INR deve essere rapido");
    }

    [Fact]
    public async Task BulkInsert_100Patients_ShouldBeEfficient()
    {
        // Arrange
        var patients = GeneratePatients(100);

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        foreach (var patient in patients)
        {
            await _unitOfWork.Patients.AddAsync(patient);
        }
        
        await _unitOfWork.SaveChangesAsync();
        stopwatch.Stop();

        // Assert
        var count = (await _unitOfWork.Patients.GetAllAsync()).Count();
        count.Should().Be(100);
        
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, 
            "inserimento bulk deve essere efficiente");
    }

    [Fact]
    public async Task ComplexQuery_PatientsWithCriteria_ShouldBeFast()
    {
        // Arrange
        await SeedPatients(200);
        await SeedINRControlsForAllPatients();

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        // Query complessa: pazienti con controllo recente fuori range
        var patientsWithRecentControls = await _unitOfWork.Patients
            .GetPatientsWithRecentINRAsync(30);
        
        stopwatch.Stop();

        // Assert
        patientsWithRecentControls.Should().NotBeEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, 
            "query complessa deve essere ottimizzata");
    }

    [Fact]
    public async Task IndexEfficiency_FiscalCodeLookup_ShouldBeInstant()
    {
        // Arrange
        await SeedPatients(1000);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var patient = await _unitOfWork.Patients.GetByFiscalCodeAsync("RSSMRA60A01H501Z");
        stopwatch.Stop();

        // Assert
        patient.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10, 
            "lookup su indice unique deve essere istantaneo");
    }

    [Fact]
    public async Task MemoryUsage_LargeDataset_ShouldBeReasonable()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);

        // Act - Carica dataset grande
        await SeedPatients(500);
        var patients = await _unitOfWork.Patients.GetAllAsync();
        
        var afterLoadMemory = GC.GetTotalMemory(false);
        var memoryUsedMB = (afterLoadMemory - initialMemory) / 1024.0 / 1024.0;

        // Assert
        memoryUsedMB.Should().BeLessThan(50, 
            "uso memoria per 500 pazienti deve essere ragionevole (<50 MB)");
    }

    [Fact]
    public async Task ConcurrentReads_ShouldScale()
    {
        // Arrange
        await SeedPatients(100);
        var patientIds = (await _unitOfWork.Patients.GetAllAsync())
            .Select(p => p.Id)
            .Take(20)
            .ToList();

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        var tasks = patientIds.Select(async id =>
        {
            using var context = TestDbContextFactory.CreateSqliteInMemoryContext();
            await SeedPatients(100, context); // Re-seed per ogni context
            
            using var uow = new UnitOfWork(context);
            return await uow.Patients.GetByIdAsync(id);
        });

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        results.Should().HaveCount(20);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, 
            "20 letture concorrenti devono completare in tempo ragionevole");
    }

    // Helper methods

    private async Task SeedPatients(int count, WarfarinDbContext? context = null)
    {
        var targetContext = context ?? _context;
        var patients = GeneratePatients(count);

        foreach (var patient in patients)
        {
            targetContext.Patients.Add(patient);
        }

        await targetContext.SaveChangesAsync();
    }

    private async Task SeedINRControlsForAllPatients()
    {
        var patients = await _unitOfWork.Patients.GetAllAsync();
        
        foreach (var patient in patients.Take(50)) // Solo primi 50 per performance
        {
            var control = new INRControl
            {
                PatientId = patient.Id,
                ControlDate = DateTime.Today.AddDays(-Random.Shared.Next(1, 30)),
                INRValue = 2.0m + (decimal)(Random.Shared.NextDouble() * 2.0), // 2.0-4.0
                CurrentWeeklyDose = 30.0m + (decimal)(Random.Shared.Next(0, 20) * 2.5), // 30-80 mg
                PhaseOfTherapy = TherapyPhase.Maintenance,
                IsCompliant = true
            };

            await _unitOfWork.INRControls.AddAsync(control);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Patient> CreateFullyPopulatedPatient()
    {
        var patient = new Patient
        {
            FirstName = "Full",
            LastName = "Patient",
            BirthDate = DateTime.Today.AddYears(-65),
            FiscalCode = "FLLPTN59A01H501Z"
        };

        // Indicazione
        patient.Indications.Add(new Indication
        {
            IndicationTypeCode = "FA_PREVENTION",
            TargetINRMin = 2.0m,
            TargetINRMax = 3.0m,
            StartDate = DateTime.Today.AddMonths(-12),
            IsActive = true
        });

        // Controlli INR
        for (int i = 0; i < 10; i++)
        {
            patient.INRControls.Add(new INRControl
            {
                ControlDate = DateTime.Today.AddMonths(-i),
                INRValue = 2.5m,
                CurrentWeeklyDose = 35.0m,
                PhaseOfTherapy = TherapyPhase.Maintenance,
                IsCompliant = true
            });
        }

        // Farmaci
        patient.Medications.Add(new Medication
        {
            MedicationName = "Amiodarone",
            Dosage = "200mg",
            Frequency = "1/die",
            StartDate = DateTime.Today.AddMonths(-6),
            IsActive = true,
            InteractionLevel = InteractionLevel.High
        });

        await _unitOfWork.Patients.AddAsync(patient);
        await _unitOfWork.SaveChangesAsync();

        return patient;
    }

    private async Task<Patient> CreateTestPatient()
    {
        var patient = new Patient
        {
            FirstName = "Test",
            LastName = "Performance",
            BirthDate = DateTime.Today.AddYears(-60),
            FiscalCode = $"TSTPRF{DateTime.Now.Ticks % 100000}Z"
        };

        await _unitOfWork.Patients.AddAsync(patient);
        await _unitOfWork.SaveChangesAsync();

        return patient;
    }

    private async Task CreateLargeINRHistory(int patientId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var control = new INRControl
            {
                PatientId = patientId,
                ControlDate = DateTime.Today.AddDays(-i * 14),
                INRValue = 2.0m + (decimal)(Random.Shared.NextDouble() * 1.5),
                CurrentWeeklyDose = 35.0m,
                PhaseOfTherapy = TherapyPhase.Maintenance,
                IsCompliant = true
            };

            await _unitOfWork.INRControls.AddAsync(control);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private static List<Patient> GeneratePatients(int count)
    {
        var lastNames = new[] { "Rossi", "Bianchi", "Verdi", "Ferrari", "Romano", 
            "Colombo", "Ricci", "Marino", "Greco", "Bruno" };
        var firstNames = new[] { "Mario", "Giuseppe", "Antonio", "Francesco", "Giovanni",
            "Maria", "Anna", "Francesca", "Rosa", "Angela" };

        var patients = new List<Patient>();

        for (int i = 0; i < count; i++)
        {
            var lastName = lastNames[i % lastNames.Length];
            var firstName = firstNames[i % firstNames.Length];
            var year = 1940 + (i % 50);
            
            patients.Add(new Patient
            {
                FirstName = firstName,
                LastName = $"{lastName}{i}",
                BirthDate = new DateTime(year, 1, 1),
                FiscalCode = $"{lastName.Substring(0, 3).ToUpper()}{firstName.Substring(0, 3).ToUpper()}{year % 100}A01H{500 + i % 500}Z",
                Gender = i % 2 == 0 ? Gender.Male : Gender.Female
            });
        }

        return patients;
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _context.Dispose();
    }
}
