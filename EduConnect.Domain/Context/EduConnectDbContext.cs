using EduConnect.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EduConnect.Data
{
    public class EduConnectDbContext : IdentityDbContext<ApplicationUser>
    {
        public EduConnectDbContext(DbContextOptions<EduConnectDbContext> options) : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Principal> Principals { get; set; }
        public DbSet<ClassTeacher> ClassTeachers { get; set; }
        public DbSet<Timetable> Timetables { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<LessonReport> LessonReports { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<TimetableSlot> TimetableSlots { get; set; }
        public DbSet<StudentLessonNote> StudentLessonNotes { get; set; }
        public DbSet<FirebaseToken> FirebaseTokens { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Class>().ToTable("Class");
            modelBuilder.Entity<Student>().ToTable("Student");
            modelBuilder.Entity<Teacher>().ToTable("Teacher");
            modelBuilder.Entity<Parent>().ToTable("Parent");
            modelBuilder.Entity<Principal>().ToTable("Principal");
            modelBuilder.Entity<ClassTeacher>().ToTable("ClassTeacher");
            modelBuilder.Entity<Timetable>().ToTable("Timetable");
            modelBuilder.Entity<TimetableSlot>().ToTable("TimetableSlot"); 
            modelBuilder.Entity<Lesson>().ToTable("Lesson");
            modelBuilder.Entity<Attendance>().ToTable("Attendance");
            modelBuilder.Entity<Grade>().ToTable("Grade");
            modelBuilder.Entity<LessonReport>().ToTable("LessonReport");
            modelBuilder.Entity<Feedback>().ToTable("Feedback");
            modelBuilder.Entity<RefreshToken>().ToTable("RefreshToken");
            modelBuilder.Entity<Exam>().ToTable("Exam");
            modelBuilder.Entity<Subject>().ToTable("Subject");
            modelBuilder.Entity<StudentLessonNote>().ToTable("StudentLessonNote");
            modelBuilder.Entity<FirebaseToken>().ToTable("FirebaseToken");
            modelBuilder.Entity<Notification>().ToTable("Notification");

            modelBuilder.Entity<Subject>()
                .HasIndex(s => s.SubjectCode)
                .HasDatabaseName("UQ_Subject_Code")
                .IsUnique();

            modelBuilder.Entity<ClassTeacher>()
                .HasIndex(ct => new { ct.ClassID, ct.TeacherID })
                .HasDatabaseName("UQ_ClassTeacher")
                .IsUnique();

            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.TimetableSlotID, a.StudentID })
                .HasDatabaseName("UQ_Attendance")
                .IsUnique();

            modelBuilder.Entity<Grade>()
                .HasIndex(g => new { g.StudentID, g.SubjectID, g.GradeType })
                .HasDatabaseName("UQ_Grade")
                .IsUnique();

            modelBuilder.Entity<Lesson>()
                .HasIndex(l => new { l.ClassID, l.SubjectId, l.Date, l.TeacherID })
                .HasDatabaseName("UQ_Lesson")
                .IsUnique();

            modelBuilder.Entity<TimetableSlot>()
                .HasIndex(ts => new { ts.TimetableID, ts.DayOfWeek, ts.Period })
                .HasDatabaseName("UQ_TimetableSlot_Period")
                .IsUnique();

            modelBuilder.Entity<StudentLessonNote>()
                .HasIndex(sln => new { sln.LessonReportID, sln.StudentID, sln.NoteType })
                .HasDatabaseName("UQ_StudentLessonNote")
                .IsUnique();

            modelBuilder.Entity<LessonReport>()
                .HasIndex(lr => new { lr.TimetableSlotID, lr.LessonDate })
                .HasDatabaseName("UQ_LessonReport_Date")
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Account)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.AccountID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Student_AspNetUsers");

            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.Account)
                .WithOne(u => u.Teacher)
                .HasForeignKey<Teacher>(t => t.AccountID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Teacher_AspNetUsers");

            modelBuilder.Entity<Parent>()
                .HasOne(p => p.Account)
                .WithOne(u => u.Parent)
                .HasForeignKey<Parent>(p => p.AccountID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Parent_AspNetUsers");

            modelBuilder.Entity<Principal>()
                .HasOne(p => p.Account)
                .WithOne(u => u.Principal)
                .HasForeignKey<Principal>(p => p.AccountID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Principal_AspNetUsers");

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithOne(u => u.RefreshToken)
                .HasForeignKey<RefreshToken>(rt => rt.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Parent)
                .WithMany(p => p.Students)
                .HasForeignKey(s => s.ParentID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.ClassID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Timetable)
                .WithMany(t => t.Lessons)
                .HasForeignKey(l => l.TimetableID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ClassTeacher>()
                .HasOne(ct => ct.Class)
                .WithMany(c => c.ClassTeachers)
                .HasForeignKey(ct => ct.ClassID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ClassTeacher>()
                .HasOne(ct => ct.Teacher)
                .WithMany(t => t.ClassTeachers)
                .HasForeignKey(ct => ct.TeacherID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.TimetableSlot)
                .WithMany(ts => ts.Attendances)
                .HasForeignKey(a => a.TimetableSlotID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Subject)
                .WithMany(s => s.Exams)
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Exams)
                .HasForeignKey(e => e.StudentID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Exam>()
                .HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Subject)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.SubjectId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TimetableSlot>()
                .HasOne(ts => ts.Timetable)
                .WithMany(t => t.TimetableSlots)
                .HasForeignKey(ts => ts.TimetableID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimetableSlot>()
                .HasOne(ts => ts.Teacher)
                .WithMany() 
                .HasForeignKey(ts => ts.TeacherID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TimetableSlot>()
                .HasOne(ts => ts.Subject)
                .WithMany() 
                .HasForeignKey(ts => ts.SubjectId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LessonReport>()
                .HasOne(lr => lr.TimetableSlot)
                .WithMany(ts => ts.LessonReports)
                .HasForeignKey(lr => lr.TimetableSlotID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LessonReport>()
                .HasOne(lr => lr.Lesson)
                .WithMany(l => l.LessonReports)
                .HasForeignKey(lr => lr.LessonID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StudentLessonNote>()
                .HasOne(sln => sln.LessonReport)
                .WithMany(lr => lr.StudentLessonNotes)
                .HasForeignKey(sln => sln.LessonReportID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentLessonNote>()
                .HasOne(sln => sln.Student)
                .WithMany(s => s.StudentLessonNotes)
                .HasForeignKey(sln => sln.StudentID)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Teacher)
                .WithMany()
                .HasForeignKey(f => f.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Account)
                .WithMany()
                .HasForeignKey(r => r.AccountId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
