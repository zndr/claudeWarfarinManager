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
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WarfarinManager",
            "warfarin.db"
        );
        
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        
        return new WarfarinDbContext(optionsBuilder.Options);
    }
}
