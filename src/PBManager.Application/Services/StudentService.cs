using Microsoft.Extensions.Caching.Memory;
using PBManager.Application.DTOs;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Enums;
using PBManager.Core.Interfaces;

namespace PBManager.Application.Services;

public class StudentService(
    IStudentRepository studentRepository,
    IMemoryCache cache,
    IAuditLogService auditLogService) : IStudentService
{
    private readonly IStudentRepository _studentRepository = studentRepository;
    private readonly IMemoryCache _cache = cache;
    private readonly IAuditLogService _auditLogService = auditLogService;

    public async Task<bool> AddStudentAsync(Student student)
    {
        if (await _studentRepository.ExistsByNationalCodeAsync(student.NationalCode))
        {
            return false;
        }

        await _studentRepository.AddAsync(student);
        await _studentRepository.SaveChangesAsync();

        _cache.Remove("AllStudents");
        _cache.Remove("StudentsCount");

        await _auditLogService.LogAsync(ActionType.Create, nameof(Student), student.Id, $"دانش آموز جدید ایجاد شد: {student.FullName}");

        return true;
    }

    public async Task<bool> DeleteStudentAsync(int id)
    {
        var student = await _studentRepository.FindByIdAsync(id);
        if (student == null) return false;

        _studentRepository.Delete(student);
        await _studentRepository.SaveChangesAsync();

        _cache.Remove("AllStudents");
        _cache.Remove("StudentsCount");
        _cache.Remove($"Student_{id}");

        await _auditLogService.LogAsync(ActionType.Delete, nameof(Student), id, $"دانش آموز حذف شد: {student.FullName}");

        return true;
    }

    public async Task UpdateStudentAsync(Student student)
    {
        _studentRepository.Update(student);
        await _studentRepository.SaveChangesAsync();

        _cache.Remove("AllStudents");
        _cache.Remove($"Student_{student.Id}");

        await _auditLogService.LogAsync(ActionType.Update, nameof(Student), student.Id, $"دانش آموز ویرایش شد: {student.FullName}");
    }

    public async Task<int> AddStudentsAsync(IEnumerable<Student> students)
    {
        var insertedCount = await _studentRepository.AddRangeAsync(students);
        if (insertedCount > 0)
        {
            _cache.Remove("AllStudents");
            _cache.Remove("StudentsCount");
        }
        return insertedCount;
    }

    public async Task<List<Student>> GetAllStudentsAsync()
    {
        const string cacheKey = "AllStudents";
        if (!_cache.TryGetValue(cacheKey, out List<Student>? students))
        {
            students = await _studentRepository.GetAllWithClassAsync();
            _cache.Set(cacheKey, students, TimeSpan.FromMinutes(30));
        }
        return students ?? [];
    }

    public async Task<ImportResult> ImportStudentsAsync(Stream fileStream, IFileParser<Student> parser)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentNullException.ThrowIfNull(parser);

        try
        {
            var students = new List<Student>();
            var result = new ImportResult();
            await foreach (var student in parser.ParseAsync(fileStream))
            {
                students.Add(student);
                _cache.Remove($"Student_{student.Id}");
            }

            if (students.Count != 0)
            {
                result.ImportedCount = await _studentRepository.AddRangeAsync(students);
                await _studentRepository.AddRangeAsync(students);
                _cache.Remove("StudentsCount");
            }
            result.SkippedCount = parser.SkippedCount;

            await _auditLogService.LogAsync(ActionType.Import, nameof(Student), null, $"تعداد {result.ImportedCount} دانش آموز از فایل ورودی ثبت شد و {result.SkippedCount} ردیف نادیده گرفته شد.");


            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("The file could not be processed. Please check the format and content.", ex);
        }
    }

    public async Task ExportAllStudentsAsync(Stream stream, IDataExporter<Student> exporter)
    {
        var students = await _studentRepository.GetAllWithClassAsync();
        await exporter.ExportAsync(students, stream);

        await _auditLogService.LogAsync(ActionType.Export, nameof(Student), null, $"خروجی از اطلاعات {students.Count} دانش آموز گرفته شد.");
    }

    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        string cacheKey = $"Student_{id}";
        if (!_cache.TryGetValue(cacheKey, out Student? student))
        {
            student = await _studentRepository.FindByIdAsync(id);
            if (student != null)
            {
                _cache.Set(cacheKey, student, TimeSpan.FromMinutes(30));
            }
        }
        return student;
    }

    public async Task<int> GetStudentsCountAsync()
    {
        const string cacheKey = "StudentsCount";
        if (!_cache.TryGetValue(cacheKey, out int count))
        {
            count = await _studentRepository.GetCountAsync();
            _cache.Set(cacheKey, count, TimeSpan.FromMinutes(30));
        }
        return count;
    }
}