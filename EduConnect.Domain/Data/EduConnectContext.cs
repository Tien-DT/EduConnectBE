using System;
using System.Collections.Generic;
using EduConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Domain.Data;

public partial class EduConnectContext : DbContext
{
    public EduConnectContext()
    {
    }

    public EduConnectContext(DbContextOptions<EduConnectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

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
            entity.HasKey(e => e.AccountId).HasName("PK__Account__349DA5A6E17CD74C");

            entity.ToTable("Account");

            entity.HasIndex(e => e.FullName, "UQ__Account__536C85E469ED267A").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Account__A9D10534DD5429E3").IsUnique();

            entity.Property(e => e.AccountId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.RefreshTokenExpTime).HasColumnType("datetime");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Class__CB1927A0E33D2D67");

            entity.ToTable("Class");

            entity.Property(e => e.ClassId)
                .ValueGeneratedNever()
                .HasColumnName("ClassID");
            entity.Property(e => e.ClassName).HasMaxLength(255);
        });

        modelBuilder.Entity<ClassTeacher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClassTea__3214EC2753758FF5");

            entity.ToTable("ClassTeacher");

            entity.HasIndex(e => new { e.ClassId, e.TeacherId }, "ClassTeacher_index_0").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassTeachers)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClassTeac__Class__5165187F");

            entity.HasOne(d => d.Teacher).WithMany(p => p.ClassTeachers)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClassTeac__Teach__52593CB8");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDF6FF9FC0A0");

            entity.ToTable("Feedback");

            entity.Property(e => e.FeedbackId)
                .ValueGeneratedNever()
                .HasColumnName("FeedbackID");
            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.DateTime).HasColumnType("datetime");
            entity.Property(e => e.ParentId).HasColumnName("ParentID");
            entity.Property(e => e.Target).HasMaxLength(255);

            entity.HasOne(d => d.Parent).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__Parent__5AEE82B9");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.GradeId).HasName("PK__Grade__54F87A3788DC3708");

            entity.ToTable("Grade");

            entity.HasIndex(e => new { e.StudentId, e.LessonId, e.GradeType }, "Grade_index_3").IsUnique();

            entity.Property(e => e.GradeId)
                .ValueGeneratedNever()
                .HasColumnName("GradeID");
            entity.Property(e => e.Evaluation).HasMaxLength(255);
            entity.Property(e => e.GradeType).HasMaxLength(255);
            entity.Property(e => e.LessonId).HasColumnName("LessonID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.Lesson).WithMany(p => p.Grades)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Grade__LessonID__59FA5E80");

            entity.HasOne(d => d.Student).WithMany(p => p.Grades)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Grade__StudentID__59063A47");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__Lesson__B084ACB013724F69");

            entity.ToTable("Lesson");

            entity.HasIndex(e => new { e.ClassId, e.Subject, e.Date, e.TeacherId }, "Lesson_index_1").IsUnique();

            entity.Property(e => e.LessonId)
                .ValueGeneratedNever()
                .HasColumnName("LessonID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.Subject).HasMaxLength(255);
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");
            entity.Property(e => e.TimetableId).HasColumnName("TimetableID");

            entity.HasOne(d => d.Class).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lesson__ClassID__5535A963");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lesson__TeacherI__5629CD9C");

            entity.HasOne(d => d.Timetable).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.TimetableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lesson__Timetabl__571DF1D5");
        });

        modelBuilder.Entity<LessonReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__LessonRe__D5BD48E5C205E4F3");

            entity.ToTable("LessonReport");

            entity.Property(e => e.ReportId)
                .ValueGeneratedNever()
                .HasColumnName("ReportID");
            entity.Property(e => e.LessonId).HasColumnName("LessonID");
            entity.Property(e => e.Remarks).HasColumnType("text");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonReports)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LessonRep__Lesso__5812160E");
        });

        modelBuilder.Entity<Parent>(entity =>
        {
            entity.HasKey(e => e.ParentId).HasName("PK__Parent__D339510F4B4F345B");

            entity.ToTable("Parent");

            entity.Property(e => e.ParentId)
                .ValueGeneratedNever()
                .HasColumnName("ParentID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(255);
        });

        modelBuilder.Entity<Principal>(entity =>
        {
            entity.HasKey(e => e.PrincipalId).HasName("PK__Principa__EB24D48F67612569");

            entity.ToTable("Principal");

            entity.Property(e => e.PrincipalId)
                .ValueGeneratedNever()
                .HasColumnName("PrincipalID");
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(255);

            entity.HasOne(d => d.Account).WithMany(p => p.Principals)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Principal_Account1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3A2CD8389F");

            entity.ToTable("Role");

            entity.Property(e => e.RoleId)
                .ValueGeneratedNever()
                .HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(255);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Student__32C52A79D2CBD709");

            entity.ToTable("Student");

            entity.Property(e => e.StudentId)
                .ValueGeneratedNever()
                .HasColumnName("StudentID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(255);
            entity.Property(e => e.ParentId).HasColumnName("ParentID");

            entity.HasOne(d => d.Class).WithMany(p => p.Students)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student__ClassID__4D94879B");

            entity.HasOne(d => d.Parent).WithMany(p => p.Students)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student__ParentI__4E88ABD4");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId).HasName("PK__Teacher__EDF259440F70F5B5");

            entity.ToTable("Teacher");

            entity.Property(e => e.TeacherId)
                .ValueGeneratedNever()
                .HasColumnName("TeacherID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(255);
        });

        modelBuilder.Entity<Timetable>(entity =>
        {
            entity.HasKey(e => e.TimetableId).HasName("PK__Timetabl__68413F400E66B93F");

            entity.ToTable("Timetable");

            entity.Property(e => e.TimetableId)
                .ValueGeneratedNever()
                .HasColumnName("TimetableID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.DayOfWeek).HasMaxLength(255);
            entity.Property(e => e.Room).HasMaxLength(255);
            entity.Property(e => e.Subject).HasMaxLength(255);
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.Class).WithMany(p => p.Timetables)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Timetable__Class__534D60F1");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Timetables)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Timetable__Teach__5441852A");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
