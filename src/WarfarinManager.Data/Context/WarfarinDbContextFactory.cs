using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WarfarinManager.Data.Context;

/// <summary>
/// Factory per creare DbContext durante le migrations
/// </summary>
public class WarfarinDbContextFactory : IDesignTimeDbContextFactory<WarfarinDbContext>
{
    public WarfarinDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WarfarinDbContext>();
        
        // Database SQLite locale per development
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dbDirectory = Path.Combine(appDataPath, "WarfarinManager");
        
        // Crea la directory se non esiste
        if (!Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }
        
        var dbPath = Path.Combine(dbDirectory, "warfarin.db");
        
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        
        return new WarfarinDbContext(optionsBuilder.Options);
    }
}
