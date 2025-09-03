using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBManager.MVVM.Model
{
    public class GradeRecord
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public double Score { get; set; }
        public DateTime Date { get; set; }
    }
}
