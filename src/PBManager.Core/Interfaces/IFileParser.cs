namespace PBManager.Core.Interfaces;

public interface IFileParser<T>
{
    IAsyncEnumerable<T> ParseAsync(Stream fileStream);
    int SkippedCount { get; }
}