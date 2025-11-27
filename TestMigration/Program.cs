using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "WarfarinManager",
    "warfarin.db"
);

Console.WriteLine($"Database path: {dbPath}");
Console.WriteLine($"Database exists: {File.Exists(dbPath)}");

var optionsBuilder = new DbContextOptionsBuilder<WarfarinDbContext>();
optionsBuilder.UseSqlite($"Data Source={dbPath}");

using var context = new WarfarinDbContext(optionsBuilder.Options);

// Apply pending migrations
Console.WriteLine("\n=== Applying Migrations ===");
try
{
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        Console.WriteLine("Pending migrations:");
        foreach (var m in pendingMigrations)
        {
            Console.WriteLine($"  - {m}");
        }

        await context.Database.MigrateAsync();
        Console.WriteLine("✅ Migrations applied!");
    }
    else
    {
        Console.WriteLine("✅ No pending migrations");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error applying migrations: {ex.Message}");
}

// Test PreTaoAssessments table
Console.WriteLine("\n=== Testing PreTaoAssessments Table ===");
try
{
    var count = await context.PreTaoAssessments.CountAsync();
    Console.WriteLine($"✅ PreTaoAssessments table exists! Count: {count}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ PreTaoAssessments table error: {ex.Message}");
}

// Test 2: Creazione paziente con nuovi campi
Console.WriteLine("\n=== TEST 2: Creazione Paziente con Campi CHA₂DS₂-VASc ===");
try
{
    var testPatient = new Patient
    {
        FirstName = "Test",
        LastName = "Bridge",
        FiscalCode = "TSTBRG80A01H501T",
        BirthDate = new DateTime(1980, 1, 1),
        HasCongestiveHeartFailure = true,
        HasHypertension = true,
        HasDiabetes = false,
        HasVascularDisease = true
    };
    
    context.Patients.Add(testPatient);
    await context.SaveChangesAsync();
    
    Console.WriteLine($"✅ Paziente test creato con ID: {testPatient.Id}");
    Console.WriteLine($"   - HasCongestiveHeartFailure: {testPatient.HasCongestiveHeartFailure}");
    Console.WriteLine($"   - HasHypertension: {testPatient.HasHypertension}");
    Console.WriteLine($"   - HasDiabetes: {testPatient.HasDiabetes}");
    Console.WriteLine($"   - HasVascularDisease: {testPatient.HasVascularDisease}");
    
    // Cleanup
    context.Patients.Remove(testPatient);
    await context.SaveChangesAsync();
    Console.WriteLine("✅ Paziente test rimosso");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Errore test paziente: {ex.Message}");
    Console.WriteLine($"   Stack: {ex.StackTrace}");
}

Console.WriteLine("\n=== TEST COMPLETATO ===");
Console.WriteLine("Premi ENTER per uscire...");
Console.ReadLine();
