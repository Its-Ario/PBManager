using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PBManager.Infrastructure.Data;

namespace PBManager.Infrastructures
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbPath = Path.Combine(appDataPath, "PBManager", "data.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}