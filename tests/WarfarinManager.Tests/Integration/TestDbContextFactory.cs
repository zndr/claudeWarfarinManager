using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Context;

namespace WarfarinManager.Tests.Integration;

/// <summary>
/// Factory per creare DbContext di test (in-memory e SQLite)
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Crea un DbContext in-memory per test veloci (senza persistenza)
    /// </summary>
    public static WarfarinDbContext CreateInMemoryContext(string databaseName = "TestDb")
    {
        var options = new DbContextOptionsBuilder<WarfarinDbContext>()
            .UseInMemoryDatabase(databaseName)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new WarfarinDbContext(options);
        
        // Assicura che il database sia creato
        context.Database.EnsureCreated();
        
        return context;
    }

    /// <summary>
    /// Crea un DbContext SQLite in-memory per test con schema reale
    /// </summary>
    public static WarfarinDbContext CreateSqliteInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<WarfarinDbContext>()
            .UseSqlite("DataSource=:memory:")
            .EnableSensitiveDataLogging()
            .Options;

        var context = new WarfarinDbContext(options);
        
        // Per SQLite in-memory, dobbiamo aprire la connessione e tenerla aperta
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        
        return context;
    }

    /// <summary>
    /// Crea un DbContext SQLite su file per test di integrazione completi
    /// </summary>
    public static WarfarinDbContext CreateSqliteFileContext(string filePath)
    {
        var options = new DbContextOptionsBuilder<WarfarinDbContext>()
            .UseSqlite($"DataSource={filePath}")
            .EnableSensitiveDataLogging()
            .Options;

        var context = new WarfarinDbContext(options);
        
        // Elimina DB esistente e ricrea
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        return context;
    }

    /// <summary>
    /// Pulisce e ricrea il database
    /// </summary>
    public static void RecreateDatabase(WarfarinDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}
