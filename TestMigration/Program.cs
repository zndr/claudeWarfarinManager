using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "WarfarinManager",
    "warfarin.db"
);

var optionsBuilder = new DbContextOptionsBuilder<WarfarinDbContext>();
optionsBuilder.UseSqlite($"Data Source={dbPath}");

using var context = new WarfarinDbContext(optionsBuilder.Options);

// Test 1: Verifica schema
Console.WriteLine("=== TEST 1: Verifica Schema Tabella Patients ===");
try
{
    var connection = context.Database.GetDbConnection();
    await connection.OpenAsync();
    
    using var command = connection.CreateCommand();
    command.CommandText = "PRAGMA table_info(Patients)";
    
    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        var columnName = reader.GetString(1);
        var columnType = reader.GetString(2);
        if (columnName.StartsWith("Has"))
        {
            Console.WriteLine($"✅ Colonna trovata: {columnName} ({columnType})");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Errore verifica schema: {ex.Message}");
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
