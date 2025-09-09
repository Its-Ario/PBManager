using Microsoft.EntityFrameworkCore;
using PBManager.Data;
using PBManager.MVVM.Model;

namespace PBManager.Services
{
    public class StudentService : IStudentService
    {
        private readonly DatabaseContext _db;

        public StudentService(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<bool> AddStudentAsync(Student student)
        {
            bool exists = await _db.Students.AnyAsync(s => s.NationalCode == student.NationalCode);
            if (exists)
            {
                return false;
            }

            _db.Students.Add(student);
            await _db.SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteStudentAsync(int id)
        {
            Student? student = await _db.Students.FindAsync(id);
            if (student == null)
                return false;

            _db.Students.Remove(student);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            return await _db.Students.Include(r => r.Class).ToListAsync();
        }

        public async Task<Student> GetStudentByIdAsync(int id)
        {
            Student? student = await _db.Students.FindAsync(id);
            return student;
        }

        public async Task SubmitGradeRecordAsync(GradeRecord record)
        {
            await _db.GradeRecords.AddAsync(record);
            await _db.SaveChangesAsync();
        }

        public async Task SubmitStudyRecordAsync(StudyRecord record)
        {
            await _db.StudyRecords.AddAsync(record);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateStudentAsync(Student student)
        {
            _db.Students.Update(student);
            await _db.SaveChangesAsync();
        }

        public async Task<int> AddStudentsAsync(IEnumerable<Student> students)
        {
            if (students == null || !students.Any())
                return 0;

            using var transaction = await _db.Database.BeginTransactionAsync();

            int inserted = 0;

            foreach (var s in students)
            {
                var rows = await _db.Database.ExecuteSqlRawAsync(
                    @"INSERT OR IGNORE INTO Students 
              (NationalCode, FirstName, LastName, ClassId) 
              VALUES ({0}, {1}, {2}, {3})",
                    s.NationalCode, s.FirstName, s.LastName, s.ClassId);

                inserted += rows;
            }

            await transaction.CommitAsync();
            return inserted;
        }

        public async Task<int> GetStudentsCountAsync()
        {
            return await _db.Students.AsNoTracking().CountAsync();
        }
    }
}
