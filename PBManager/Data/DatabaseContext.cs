using Microsoft.EntityFrameworkCore;
using PBManager.MVVM.Model;

namespace PBManager.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<GradeRecord> GradeRecords { get; set; }
        public DbSet<StudyRecord> StudyRecords { get; set; }
    }

}
