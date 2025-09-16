using Microsoft.EntityFrameworkCore;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Data;

namespace PBManager.Infrastructure.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly DatabaseContext _db;

    public StudentRepository(DatabaseContext db) { _db = db; }

    public Task<Student?> FindByIdAsync(int id) => _db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    public Task<List<Student>> GetAllWithClassAsync() => _db.Students.AsNoTracking().Include(r => r.Class).AsNoTracking().ToListAsync();
    public Task<bool> ExistsByNationalCodeAsync(string code) => _db.Students.AsNoTracking().AnyAsync(s => s.NationalCode == code);
    public async Task AddAsync(Student s) => await _db.Students.AddAsync(s);
    public void Update(Student s) => _db.Students.Update(s);
    public void Delete(Student s) => _db.Students.Remove(s);
    public Task<int> GetCountAsync() => _db.Students.AsNoTracking().CountAsync();
    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

    public async Task<int> AddRangeAsync(IEnumerable<Student> students)
    {
        if (students == null || !students.Any()) return 0;

        var nationalCodes = students.Select(s => s.NationalCode).ToList();

        // fetch existing students once
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