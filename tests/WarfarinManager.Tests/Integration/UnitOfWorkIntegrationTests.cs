using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Data.Repositories;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Tests.Integration;

/// <summary>
/// Test Unit of Work pattern e gestione transazioni
/// </summary>
public class UnitOfWorkIntegrationTests : IDisposable
{
    private readonly TestWarfarinDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkIntegrationTests()
    {
        _context = TestDbContextFactory.CreateSqliteInMemoryContext();
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task UnitOfWork_ShouldProvideAccessToAllRepositories()
    {
        // Assert
        _unitOfWork.Patients.Should().NotBeNull();
        _unitOfWork.INRControls.Should().NotBeNull();
        _unitOfWork.InteractionDrugs.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistAllChanges()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "Test",
            LastName = "UnitOfWork",
            BirthDate = DateTime.Today.AddYears(-60),
            FiscalCode = "TSTUOW64A01H501Z"
        };

        // Act
        await _unitOfWork.Patients.AddAsync(patient);
        var saveResult = await _unitOfWork.SaveChangesAsync();

        // Assert
        saveResult.Should().BeGreaterThan(0, "dovrebbe salvare almeno 1 entità");
        patient.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Transaction_Commit_ShouldPersistAllChanges()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "Transaction",
            LastName = "Commit",
            BirthDate = DateTime.Today.AddYears(-55),
            FiscalCode = "TRNCMT69A01H501A"
        };

        var indication = new Indication
        {
            Patient = patient,
            IndicationTypeCode = "FA_PREVENTION",
            TargetINRMin = 2.0m,
            TargetINRMax = 3.0m,
            StartDate = DateTime.Today,
            IsActive = true
        };

        // Act
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            await _unitOfWork.Patients.AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();

            patient.Indications.Add(indication);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        // Assert
        var savedPatient = await _unitOfWork.Patients.GetByIdAsync(patient.Id);
        savedPatient.Should().NotBeNull();
        savedPatient!.Indications.Should().HaveCount(1);
    }

    [Fact]
    public async Task Transaction_Rollback_ShouldNotPersistChanges()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "Transaction",
            LastName = "Rollback",
            BirthDate = DateTime.Today.AddYears(-62),
            FiscalCode = "TRNRBK62A01H501B"
        };

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            await _unitOfWork.Patients.AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();

            var patientId = patient.Id;

            // Simula errore
            throw new InvalidOperationException("Simulated error");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
        }

        // Assert - Paziente non dovrebbe essere salvato
        var allPatients = await _unitOfWork.Patients.GetAllAsync();
        allPatients.Should().NotContain(p => p.FiscalCode == "TRNRBK62A01H501B");
    }

    [Fact]
    public async Task ComplexTransaction_MultipleEntities_ShouldBeAtomic()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "Complex",
            LastName = "Transaction",
            BirthDate = DateTime.Today.AddYears(-58),
            FiscalCode = "CMPTRS66A01H501C"
        };

        var indication = new Indication
        {
            IndicationTypeCode = "FA_PREVENTION",
            TargetINRMin = 2.0m,
            TargetINRMax = 3.0m,
            StartDate = DateTime.Today.AddMonths(-3),
            IsActive = true
        };

        var control = new INRControl
        {
            ControlDate = DateTime.Today.AddDays(-7),
            INRValue = 2.5m,
            CurrentWeeklyDose = 35.0m,
            PhaseOfTherapy = TherapyPhase.Maintenance,
            IsCompliant = true
        };

        var medication = new Medication
        {
            MedicationName = "Amiodarone",
            Dosage = "200mg",
            Frequency = "1/die",
            StartDate = DateTime.Today.AddMonths(-1),
            IsActive = true,
            InteractionLevel = InteractionLevel.High
        };

        // Act
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Crea paziente
            await _unitOfWork.Patients.AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();

            // 2. Aggiungi indicazione
            indication.PatientId = patient.Id;
            patient.Indications.Add(indication);
            await _unitOfWork.SaveChangesAsync();

            // 3. Aggiungi controllo INR
            control.PatientId = patient.Id;
            patient.INRControls.Add(control);
            await _unitOfWork.SaveChangesAsync();

            // 4. Aggiungi farmaco
            medication.PatientId = patient.Id;
            patient.Medications.Add(medication);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        // Assert
        var savedPatient = await _unitOfWork.Patients
            .GetByFiscalCodeAsync("CMPTRS66A01H501C");

        savedPatient.Should().NotBeNull();
        savedPatient!.Indications.Should().HaveCount(1);
        savedPatient.INRControls.Should().HaveCount(1);
        savedPatient.Medications.Should().HaveCount(1);
        
        // Verifica relazioni
        savedPatient.Indications.First().PatientId.Should().Be(savedPatient.Id);
        savedPatient.INRControls.First().PatientId.Should().Be(savedPatient.Id);
        savedPatient.Medications.First().PatientId.Should().Be(savedPatient.Id);
    }

    [Fact]
    public async Task ConcurrentAccess_ShouldHandleCorrectly()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "Concurrent",
            LastName = "Test",
            BirthDate = DateTime.Today.AddYears(-60),
            FiscalCode = "CNCRNT64A01H501D"
        };

        await _unitOfWork.Patients.AddAsync(patient);
        await _unitOfWork.SaveChangesAsync();

        // Act - Simula due aggiornamenti concorrenti
        var update1Task = Task.Run(async () =>
        {
            using var uow = new UnitOfWork(_context);
            var p = await uow.Patients.GetByIdAsync(patient.Id);
            p!.Phone = "111111111";
            await uow.SaveChangesAsync();
        });

        var update2Task = Task.Run(async () =>
        {
            using var uow = new UnitOfWork(_context);
            var p = await uow.Patients.GetByIdAsync(patient.Id);
            p!.Email = "test@test.com";
            await uow.SaveChangesAsync();
        });

        // Assert - Entrambi devono completare
        await Task.WhenAll(update1Task, update2Task);
        
        var updated = await _unitOfWork.Patients.GetByIdAsync(patient.Id);
        updated.Should().NotBeNull();
        // Almeno uno dei due aggiornamenti deve essere persistito
        (updated!.Phone != null || updated.Email != null).Should().BeTrue();
    }

    [Fact]
    public async Task Dispose_ShouldCleanupResources()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "Dispose",
            LastName = "Test",
            BirthDate = DateTime.Today.AddYears(-65),
            FiscalCode = "DSPTST59A01H501E"
        };

        // Act
        await _unitOfWork.Patients.AddAsync(patient);
        await _unitOfWork.SaveChangesAsync();
        
        _unitOfWork.Dispose();

        // Assert - Nuovo UnitOfWork dovrebbe ancora vedere i dati
        using var newUow = new UnitOfWork(_context);
        var retrieved = await newUow.Patients.GetByFiscalCodeAsync("DSPTST59A01H501E");
        retrieved.Should().NotBeNull("i dati devono persistere dopo dispose");
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _context.Dispose();
    }
}
