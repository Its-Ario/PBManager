using Microsoft.Extensions.Caching.Memory;
using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;

namespace PBManager.Application.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IMemoryCache _cache;

    public StudentService(IStudentRepository studentRepository, IMemoryCache cache)
    {
        _studentRepository = studentRepository;
        _cache = cache;
    }

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

        return true;
    }

    public async Task<bool> DeleteStudentAsync(int id)
    {
        var student = await _studentRepository.FindByIdAsync(id);
        if (student == null)
            return false;

        _studentRepository.Delete(student);
        await _studentRepository.SaveChangesAsync();

        _cache.Remove("AllStudents");
        _cache.Remove("StudentsCount");
        _cache.Remove($"Student_{id}");

        return true;
    }

    public async Task UpdateStudentAsync(Student student)
    {
        _studentRepository.Update(student);
        await _studentRepository.SaveChangesAsync();

        _cache.Remove("AllStudents");
        _cache.Remove($"Student_{student.Id}");
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
        if (!_cache.TryGetValue(cacheKey, out List<Student> students))
        {
            students = await _studentRepository.GetAllWithClassAsync();

            _cache.Set(cacheKey, students, TimeSpan.FromMinutes(30));
        }
        return students;
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