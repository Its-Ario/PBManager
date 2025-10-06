namespace PBManager.Core.Entities
{
    public class Exam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int MaxScore { get; set; }
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
