namespace PBManager.Core.Interfaces
{
    public interface IDatabasePorter
    {
        Task ExportDatabaseAsync(string destinationPath);
        Task<bool> ImportDatabaseAsync(string sourcePath);
        bool IsPendingImportOnRestart();
    }
}