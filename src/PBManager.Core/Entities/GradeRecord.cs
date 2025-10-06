using System.ComponentModel.DataAnnotations.Schema;

namespace PBManager.Core.Entities
{
    public class GradeRecord
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public double Score { get; set; }
        public DateTime Date { get; set; }

        public Student Student { get; set; }
        public Subject Subject { get; set; }

        public int? ExamId { get; set; }
        public Exam? Exam { get; set; }

        [NotMapped]
        public double NormalizedScore =>
            Exam != null && Exam.MaxScore > 0
                ? (Score / Exam.MaxScore) * 100.0
                : 0;
    }
}
