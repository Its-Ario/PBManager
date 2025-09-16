using PBManager.Core.Entities;

namespace PBManager.Application.Interfaces
{
    public interface IStudentService
    {
        Task<List<Student>> GetAllStudentsAsync();
        Task<Student?> GetStudentByIdAsync(int id);
        Task<bool> AddStudentAsync(Student student);
        Task UpdateStudentAsync(Student student);
        Task<bool> DeleteStudentAsync(int id);
        Task<int> AddStudentsAsync(IEnumerable<Student> students);
        Task<int> GetStudentsCountAsync();
    }
}