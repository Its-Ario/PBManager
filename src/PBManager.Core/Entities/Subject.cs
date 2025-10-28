using PBManager.Core.Interfaces;

namespace PBManager.Core.Entities
{
    public class Subject : IManagedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    }
}
