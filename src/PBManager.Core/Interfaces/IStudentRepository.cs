using PBManager.Core.Entities;

namespace PBManager.Core.Interfaces;

public interface IStudentRepository
{
    Task<Student?> FindByIdAsync(int id);
    Task<List<Student>> GetAllWithClassAsync();
    Task<bool> ExistsByNationalCodeAsync(string nationalCode);
    Task AddAsync(Student student);
    Task<int> AddRangeAsync(IEnumerable<Student> students);
    void Update(Student student);
    void Delete(Student student);
    Task<int> GetCountAsync();
    Task<int> SaveChangesAsync();
}