using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBManager.Application.DTOs
{
    public class StudentExamScore
    {
        public int StudentId { get; set; }
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public DateTime ExamDate { get; set; }
        public double AverageScore { get; set; }
    }
}
