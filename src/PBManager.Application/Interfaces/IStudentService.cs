using PBManager.Application.DTOs;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;

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
        Task<ImportResult> ImportStudentsAsync(Stream fileStream, IFileParser<Student> parser);
        Task ExportAllStudentsAsync(Stream stream, IDataExporter<Student> exporter);
    }
}