using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBManager.MVVM.Model
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<StudyRecord> StudyRecords { get; set; }
        public ICollection<GradeRecord> GradeRecords { get; set; }
    }
}
