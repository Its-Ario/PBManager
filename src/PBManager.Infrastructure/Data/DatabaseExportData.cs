using PBManager.Core.Entities;

namespace PBManager.Infrastructure.Data
{
    public class DatabaseExportData
    {
        public List<Student> Students { get; set; } = new();
        public List<Subject> Subjects { get; set; } = new();
        public List<Class> Classes { get; set; } = new();
        public List<GradeRecord> GradeRecords { get; set; } = new();
        public List<StudyRecord> StudyRecords { get; set; } = new();
        public List<AuditLog> AuditLogs { get; set; } = new();
        public List<Exam> Exams { get; set; } = new();
    }
}
