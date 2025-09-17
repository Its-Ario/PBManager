using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Data;

namespace PBManager.Infrastructure.Services
{
    public class DatabaseManagementService : IDatabaseManagementService
    {
        private readonly DatabaseContext _db;

        public DatabaseManagementService(DatabaseContext dbContext)
        {
            _db = dbContext;
        }

        public void WipeDatabase()
        {
            var dbPath = _db.Database.GetDbConnection().DataSource;

            SqliteConnection.ClearAllPools();

            Thread.Sleep(500);

            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }

        public async Task<string> CheckIntegrityAsync()
        {
            var connection = _db.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA integrity_check;";

            var result = await command.ExecuteScalarAsync() as string;

            await connection.CloseAsync();
            return result ?? "ok";
        }

        public async Task CompactDatabaseAsync()
        {
            await _db.Database.ExecuteSqlRawAsync("VACUUM;");
        }
    }
}
