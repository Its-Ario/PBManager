using ExcelDataReader;
using PBManager.Core.Interfaces;
using PBManager.Core.Entities;
using System.Text;

namespace PBManager.Infrastructure.Parsers;

public class XlsxStudentParser : IFileParser<Student>
{
    private readonly IClassRepository _classRepository;

    private int _skippedCount;
    public int SkippedCount
    {
        get => _skippedCount;
    }

    public XlsxStudentParser(IClassRepository classRepository)
    {
        _classRepository = classRepository;
    }
    public async IAsyncEnumerable<Student> ParseAsync(Stream fileStream)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var reader = ExcelReaderFactory.CreateReader(fileStream);
        var result = reader.AsDataSet();
        var table = result.Tables[0];

        for (int i = 1; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];

            var firstName = row[0]?.ToString();
            var lastName = row[1]?.ToString();
            var nationalCode = row[2]?.ToString();

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(nationalCode))
            {
                _skippedCount++;
                continue;
            }

            var className = row[3]?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(className))
            {
                _skippedCount++;
                continue;
            }

            var classEntity = await _classRepository.GetByNameAsync(className);
            if (classEntity == null)
            {
                _skippedCount++;
                continue;
            }

            yield return new Student
            {
                FirstName = firstName,
                LastName = lastName,
                NationalCode = nationalCode,
                ClassId = classEntity.Id
            };
        }
    }
}