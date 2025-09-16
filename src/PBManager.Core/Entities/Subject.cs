namespace PBManager.Core.Entities
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<StudyRecord> StudyRecords { get; set; }
        public ICollection<GradeRecord> GradeRecords { get; set; }
    }
}
