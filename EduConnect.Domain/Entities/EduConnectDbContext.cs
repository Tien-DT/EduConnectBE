using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Domain.Entities;

public partial class EduConnectDbContext : DbContext
{
    public EduConnectDbContext()
    {
    }

    public EduConnectDbContext(DbContextOptions<EduConnectDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassTeacher> ClassTeachers { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonReport> LessonReports { get; set; }

    public virtual DbSet<Parent> Parents { get; set; }

    public virtual DbSet<Principal> Principals { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<Timetable> Timetables { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tienlab.myds.me,1433;Database=EduConnect;User Id=sa;Password=123456789@Tien;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__349DA586CCB1D2EA");

            entity.Property(e => e.AccountId).ValueGeneratedNever();
            entity.Property(e => e.FullName).HasDefaultValue("");
            entity.Property(e => e.PasswordHash).HasDefaultValue("");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts).HasConstraintName("FK__Account__RoleID__276EDEB3");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69263C9176A83C");

            entity.Property(e => e.AttendanceId).ValueGeneratedNever();

            entity.HasOne(d => d.Lesson).WithMany(p => p.Attendances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Lesso__48CFD27E");

            entity.HasOne(d => d.Student).WithMany(p => p.Attendances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Stude__49C3F6B7");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Class__CB1927A0F07E6033");

            entity.Property(e => e.ClassId).ValueGeneratedNever();
        });

        modelBuilder.Entity<ClassTeacher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClassTea__3214EC274290D640");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Class).WithMany(p => p.ClassTeachers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClassTeac__Class__3A81B327");

            entity.HasOne(d => d.Teacher).WithMany(p => p.ClassTeachers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClassTeac__Teach__3B75D760");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDF6583119E1");

            entity.Property(e => e.FeedbackId).ValueGeneratedNever();

            entity.HasOne(d => d.Parent).WithMany(p => p.Feedbacks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__Parent__5441852A");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.GradeId).HasName("PK__Grade__54F87A37FA63C6B0");

            entity.Property(e => e.GradeId).ValueGeneratedNever();

            entity.HasOne(d => d.Lesson).WithMany(p => p.Grades)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Grade__LessonID__5165187F");

            entity.HasOne(d => d.Student).WithMany(p => p.Grades)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Grade__StudentID__5070F446");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__Lesson__B084ACB042C10F11");

            entity.Property(e => e.LessonId).ValueGeneratedNever();

            entity.HasOne(d => d.Class).WithMany(p => p.Lessons)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lesson__ClassID__4316F928");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Lessons)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lesson__TeacherI__440B1D61");

            entity.HasOne(d => d.Timetable).WithMany(p => p.Lessons)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lesson__Timetabl__44FF419A");
        });

        modelBuilder.Entity<LessonReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__LessonRe__D5BD48E58A72EA8C");

            entity.Property(e => e.ReportId).ValueGeneratedNever();

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonReports)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LessonRep__Lesso__4CA06362");
        });

        modelBuilder.Entity<Parent>(entity =>
        {
            entity.HasKey(e => e.ParentId).HasName("PK__Parent__D339510F4A358604");

            entity.Property(e => e.ParentId).ValueGeneratedNever();

            entity.HasOne(d => d.Account).WithMany(p => p.Parents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Parent__AccountI__2C3393D0");
        });

        modelBuilder.Entity<Principal>(entity =>
        {
            entity.HasKey(e => e.PrincipalId).HasName("PK__Principa__EB24D48FDE31796C");

            entity.Property(e => e.PrincipalId).ValueGeneratedNever();

            entity.HasOne(d => d.Account).WithMany(p => p.Principals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Principal__Accou__31EC6D26");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3AAD2FA929");

            entity.Property(e => e.RoleId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Student__32C52A79D2892AE8");

            entity.Property(e => e.StudentId).ValueGeneratedNever();

            entity.HasOne(d => d.Account).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student__Account__34C8D9D1");

            entity.HasOne(d => d.Class).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student__ClassID__36B12243");

            entity.HasOne(d => d.Parent).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student__ParentI__35BCFE0A");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId).HasName("PK__Teacher__EDF2594401777594");

            entity.Property(e => e.TeacherId).ValueGeneratedNever();

            entity.HasOne(d => d.Account).WithMany(p => p.Teachers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Teacher__Account__2F10007B");
        });

        modelBuilder.Entity<Timetable>(entity =>
        {
            entity.HasKey(e => e.TimetableId).HasName("PK__Timetabl__68413F40A8C66EE3");

            entity.Property(e => e.TimetableId).ValueGeneratedNever();

            entity.HasOne(d => d.Class).WithMany(p => p.Timetables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Timetable__Class__3E52440B");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Timetables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Timetable__Teach__3F466844");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
