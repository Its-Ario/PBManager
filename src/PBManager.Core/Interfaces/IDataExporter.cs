namespace PBManager.Core.Interfaces
{
    public interface IDataExporter<T> where T : class
    {
        Task ExportAsync(IEnumerable<T> data, Stream stream);
    }
}
