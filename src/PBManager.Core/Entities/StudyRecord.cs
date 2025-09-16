﻿namespace PBManager.Core.Entities
{
    public class StudyRecord
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public int MinutesStudied { get; set; }
        public DateTime Date { get; set; }
    }

}
