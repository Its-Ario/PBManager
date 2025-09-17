using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;

namespace PBManager.Infrastructure.Services.Parsers
{
    public class CsvStudentParser : IFileParser<Student>
    {
        private readonly IClassRepository _classRepository;
        private int _skippedCount;

        public int SkippedCount => _skippedCount;

        public CsvStudentParser(IClassRepository classRepository)
        {
            _classRepository = classRepository ?? throw new ArgumentNullException(nameof(classRepository));
        }

        public async IAsyncEnumerable<Student> ParseAsync(Stream fileStream)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));

            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            await foreach (var record in csv.GetRecordsAsync<dynamic>())
            {
                Student student = null;

                try
                {
                    string nationalCode = record.NationalCode;
                    string firstName = record.FirstName;
                    string lastName = record.LastName;
                    string className = record.ClassName;

                    if (string.IsNullOrWhiteSpace(nationalCode) ||
                        string.IsNullOrWhiteSpace(firstName) ||
                        string.IsNullOrWhiteSpace(lastName) ||
                        string.IsNullOrWhiteSpace(className))
                    {
                        _skippedCount++;
                        continue;
                    }

                    var classEntity = await _classRepository.GetByNameAsync(className.Trim());
                    if (classEntity == null)
                    {
                        _skippedCount++;
                        continue;
                    }

                    student = new Student
                    {
                        NationalCode = nationalCode.Trim(),
                        FirstName = firstName.Trim(),
                        LastName = lastName.Trim(),
                        ClassId = classEntity.Id
                    };
                }
                catch
                {
                    _skippedCount++;
                }

                if (student != null)
                    yield return student;
            }
        }
    }
}
