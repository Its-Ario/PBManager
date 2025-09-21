using Microsoft.EntityFrameworkCore;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Data;

namespace PBManager.Infrastructure.Repositories;

public class StudentRepository(DatabaseContext db) : IStudentRepository
{
    private readonly DatabaseContext _db = db;

    public Task<Student?> FindByIdAsync(int id) => _db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    public Task<List<Student>> GetAllWithClassAsync() => _db.Students.AsNoTracking().Include(r => r.Class).AsNoTracking().ToListAsync();
    public Task<bool> ExistsByNationalCodeAsync(string nationalCode) => _db.Students.AsNoTracking().AnyAsync(s => s.NationalCode == nationalCode);
    public async Task AddAsync(Student student) => await _db.Students.AddAsync(student);
    public void Update(Student student) => _db.Students.Update(student);
    public void Delete(Student student) => _db.Students.Remove(student);
    public Task<int> GetCountAsync() => _db.Students.AsNoTracking().CountAsync();
    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

    public async Task<int> AddRangeAsync(IEnumerable<Student> students)
    {
        if (students == null || !students.Any()) return 0;

        var nationalCodes = students.Select(s => s.NationalCode).ToList();

        students = students
            .GroupBy(s => s.NationalCode)
            .Select(g => g.First())
            .ToList();

        var existingCodes = await _db.Students.AsNoTracking()
            .Where(s => nationalCodes.Contains(s.NationalCode))
            .Select(s => s.NationalCode)
            .ToListAsync();

        var newStudents = students
            .Where(s => !existingCodes.Contains(s.NationalCode))
            .ToList();

        if (newStudents.Count == 0) return 0;

        await _db.Students.AddRangeAsync(newStudents);
        return await _db.SaveChangesAsync();
    }

}