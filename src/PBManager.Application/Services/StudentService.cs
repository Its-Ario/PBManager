using PBManager.Application.Interfaces;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;

namespace PBManager.Application.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;

    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<bool> AddStudentAsync(Student student)
    {
        if (await _studentRepository.ExistsByNationalCodeAsync(student.NationalCode))
        {
            return false;
        }

        await _studentRepository.AddAsync(student);
        await _studentRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteStudentAsync(int id)
    {
        var student = await _studentRepository.FindByIdAsync(id);
        if (student == null)
            return false;

        _studentRepository.Delete(student);
        await _studentRepository.SaveChangesAsync();
        return true;
    }

    public async Task UpdateStudentAsync(Student student)
    {
        _studentRepository.Update(student);
        await _studentRepository.SaveChangesAsync();
    }

    public Task<int> AddStudentsAsync(IEnumerable<Student> students)
    {
        return _studentRepository.AddRangeAsync(students);
    }

    public Task<List<Student>> GetAllStudentsAsync() => _studentRepository.GetAllWithClassAsync();
    public Task<Student?> GetStudentByIdAsync(int id) => _studentRepository.FindByIdAsync(id);
    public Task<int> GetStudentsCountAsync() => _studentRepository.GetCountAsync();
}