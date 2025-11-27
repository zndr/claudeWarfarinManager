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

// Check table schema and force recreate if needed
Console.WriteLine("\n=== Checking AdverseEvents Schema ===");
bool needsRecreation = false;
try
{
    // Try to query with new schema
    var testQuery = context.AdverseEvents.Take(1).ToList();
    Console.WriteLine("✅ AdverseEvents table has correct schema");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  AdverseEvents table has wrong schema: {ex.Message}");
    Console.WriteLine("\n=== Force Recreating AdverseEvents Table ===");
    needsRecreation = true;

    try
    {
        // Drop existing table
        await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS AdverseEvents");
        Console.WriteLine("✅ Dropped old AdverseEvents table");

        // Remove migration history entry for this migration
        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM __EFMigrationsHistory WHERE MigrationId = '20251127120000_UpdateAdverseEventsSchema'");
        Console.WriteLine("✅ Removed migration history entry");
    }
    catch (Exception recreateEx)
    {
        Console.WriteLine($"❌ Error dropping/removing: {recreateEx.Message}");
    }

    // Dispose old context and create new one to refresh migration cache
    await context.DisposeAsync();

    Console.WriteLine("Creating fresh context to apply migration...");
    var migrateOptionsBuilder = new DbContextOptionsBuilder<WarfarinDbContext>();
    migrateOptionsBuilder.UseSqlite($"Data Source={dbPath}");
    using var migrateContext = new WarfarinDbContext(migrateOptionsBuilder.Options);

    try
    {
        // Check pending migrations with fresh context
        var pendingMigrations = await migrateContext.Database.GetPendingMigrationsAsync();
        Console.WriteLine($"📋 Pending migrations: {string.Join(", ", pendingMigrations)}");

        // Manually execute the CREATE TABLE SQL since MigrateAsync won't work
        await migrateContext.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE AdverseEvents (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                PatientId INTEGER NOT NULL,
                OnsetDate TEXT NOT NULL,
                ReactionType TEXT NOT NULL,
                Severity TEXT NOT NULL,
                CertaintyLevel TEXT NOT NULL,
                MeasuresTaken TEXT NULL,
                INRAtEvent TEXT NULL,
                Notes TEXT NULL,
                LinkedINRControlId INTEGER NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY (PatientId) REFERENCES Patients(Id) ON DELETE CASCADE,
                FOREIGN KEY (LinkedINRControlId) REFERENCES INRControls(Id) ON DELETE SET NULL
            )");
        Console.WriteLine("✅ Created AdverseEvents table manually");

        // Create indexes
        await migrateContext.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IX_AdverseEvents_Patient_Date ON AdverseEvents (PatientId, OnsetDate DESC)");
        await migrateContext.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IX_AdverseEvents_Severity ON AdverseEvents (Severity)");
        await migrateContext.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IX_AdverseEvents_LinkedINRControlId ON AdverseEvents (LinkedINRControlId)");
        Console.WriteLine("✅ Created indexes");

        // Add migration history entry
        await migrateContext.Database.ExecuteSqlRawAsync(@"
            INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
            VALUES ('20251127120000_UpdateAdverseEventsSchema', '8.0.0')");
        Console.WriteLine("✅ Added migration history entry");
    }
    catch (Exception migrateEx)
    {
        Console.WriteLine($"❌ Error applying migration: {migrateEx.Message}");
    }
}

// Verify with a fresh context if we recreated the table
if (needsRecreation)
{
    Console.WriteLine("\n=== Verifying AdverseEvents Table with Fresh Context ===");
    var verifyOptionsBuilder = new DbContextOptionsBuilder<WarfarinDbContext>();
    verifyOptionsBuilder.UseSqlite($"Data Source={dbPath}");
    using var verifyContext = new WarfarinDbContext(verifyOptionsBuilder.Options);

    try
    {
        var verifyQuery = verifyContext.AdverseEvents.Take(1).ToList();
        Console.WriteLine("✅ Verified: Table now has correct schema");
    }
    catch (Exception verifyEx)
    {
        Console.WriteLine($"❌ Verification failed: {verifyEx.Message}");
    }
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
