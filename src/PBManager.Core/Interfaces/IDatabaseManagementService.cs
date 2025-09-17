namespace PBManager.Core.Interfaces
{
    public interface IDatabaseManagementService
    {
        void WipeDatabase();
        Task CompactDatabaseAsync();
        Task<string> CheckIntegrityAsync();
    }
}
