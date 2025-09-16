namespace PBManager.Core.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string NationalCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        public int ClassId { get; set; }
        public Class Class { get; set; }

        public ICollection<StudyRecord> StudyRecords { get; set; }
        public ICollection<GradeRecord> GradeRecords { get; set; }
    }
}
