using CsvHelper;
using PBManager.Core.Interfaces;
using System.Globalization;

namespace PBManager.Infrastructure.Parsers;

public class CsvParser<T> : IFileParser<T> where T : class
{
    private int _skippedCount;
    public int SkippedCount
    {
        get => _skippedCount;
    }
    public async IAsyncEnumerable<T> ParseAsync(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        await foreach (var record in csv.GetRecordsAsync<T>())
        {
            yield return record;
        }
    }
}