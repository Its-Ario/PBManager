using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBManager.MVVM.Model
{
    public class Student
    {
        public int Id { get; set; }
        public string NationalCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Class Class { get; set; }
        public string ClassName => Class?.Name ?? "Unassigned";

        public ICollection<StudyRecord> StudyRecords { get; set; }
        public ICollection<GradeRecord> GradeRecords { get; set; }
    }
}
