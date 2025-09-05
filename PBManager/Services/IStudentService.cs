using PBManager.MVVM.Model;

namespace PBManager.Services
{
    public interface IStudentService
    {
        Task<List<Student>> GetAllStudentsAsync();
        Task<Student> GetStudentByIdAsync(int id);
        Task<bool> AddStudentAsync(Student student);
        Task<int> AddStudentsAsync(IEnumerable<Student> students);
        Task UpdateStudentAsync(Student student);
        Task<bool> DeleteStudentAsync(int id);
        Task SubmitStudyRecordAsync(StudyRecord record);
        Task SubmitGradeRecordAsync(GradeRecord record);

    }
}
