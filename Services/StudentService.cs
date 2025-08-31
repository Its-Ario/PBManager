using Microsoft.EntityFrameworkCore;
using PBManager.MVVM.Model;

namespace PBManager.Services
{
    public class StudentService : IStudentService
    {
        public async Task<bool> AddStudentAsync(Student student)
        {
            bool exists = await App.Db.Students.AnyAsync(s => s.NationalCode == student.NationalCode);
            if (exists)
            {
                return false;
            }

            App.Db.Students.Add(student);
            await App.Db.SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteStudentAsync(int id)
        {
            Student? student = await App.Db.Students.FindAsync(id);
            if (student == null)
                return false;

            App.Db.Students.Remove(student);
            await App.Db.SaveChangesAsync();

            return true;
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            return await App.Db.Students.ToListAsync();
        }

        public async Task<Student> GetStudentByIdAsync(int id)
        {
            return await App.Db.Students.FindAsync(id);
        }

        public async Task SubmitGradeRecordAsync(GradeRecord record)
        {
            await App.Db.GradeRecords.AddAsync(record);
            await App.Db.SaveChangesAsync();
        }

        public async Task SubmitStudyRecordAsync(StudyRecord record)
        {
            await App.Db.StudyRecords.AddAsync(record);
            await App.Db.SaveChangesAsync();
        }

        public async Task UpdateStudentAsync(Student student)
        {
            App.Db.Students.Update(student);
            await App.Db.SaveChangesAsync();
        }

        public async Task<int> AddStudentsAsync(IEnumerable<Student> students)
        {
            var existingIds = await App.Db.Students
                .Where(s => students.Select(ns => ns.Id).Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            var newStudents = students.Where(s => !existingIds.Contains(s.Id)).ToList();

            if (newStudents.Count != 0)
            {
                App.Db.Students.AddRange(newStudents);
                await App.Db.SaveChangesAsync();
            }

            return newStudents.Count;
        }
    }
}
