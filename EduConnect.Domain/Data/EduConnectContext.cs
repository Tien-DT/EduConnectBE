using System;
using System.Collections.Generic;
using EduConnect.Domain.Models;
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

            entity.Property(e => e.AccountId).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassID).HasName("PK__Class__CB1927A0E33D2D67");

            entity.Property(e => e.ClassID).ValueGeneratedNever();
        });

        modelBuilder.Entity<ClassTeacher>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK__ClassTea__3214EC2753758FF5");

            entity.Property(e => e.ID).ValueGeneratedNever();

            entity.HasOne(d => d.Class).WithMany(p => p.ClassTeachers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClassTeac__Class__5165187F");

            entity.HasOne(d => d.Teacher).WithMany(p => p.ClassTeachers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClassTeac__Teach__52593CB8");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackID).HasName("PK__Feedback__6A4BEDF6FF9FC0A0");

            entity.Property(e => e.FeedbackID).ValueGeneratedNever();

            entity.HasOne(d => d.Parent).WithMany(p => p.Feedbacks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__Parent__5AEE82B9");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.GradeID).HasName("PK__Grade__54F87A3788DC3708");

            entity.Property(e => e.GradeID).ValueGeneratedNever();

            entity.HasOne(d => d.Lesson).WithMany(p => p.Grades)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Grade__LessonID__59FA5E80");

            entity.HasOne(d => d.Student).WithMany(p => p.Grades)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Grade__StudentID__59063A47");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonID).HasName("PK__Lesson__B084ACB013724F69");

            entity.Property(e => e.LessonID).ValueGeneratedNever();

            entity.HasOne(d => d.Class).WithMany(p => p.Lessons)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lesson__ClassID__5535A963");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Lessons)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lesson__TeacherI__5629CD9C");

            entity.HasOne(d => d.Timetable).WithMany(p => p.Lessons)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lesson__Timetabl__571DF1D5");
        });

        modelBuilder.Entity<LessonReport>(entity =>
        {
            entity.HasKey(e => e.ReportID).HasName("PK__LessonRe__D5BD48E5C205E4F3");

            entity.Property(e => e.ReportID).ValueGeneratedNever();

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonReports)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LessonRep__Lesso__5812160E");
        });

        modelBuilder.Entity<Parent>(entity =>
        {
            entity.HasKey(e => e.ParentID).HasName("PK__Parent__D339510F4B4F345B");

            entity.Property(e => e.ParentID).ValueGeneratedNever();
        });

        modelBuilder.Entity<Principal>(entity =>
        {
            entity.HasKey(e => e.PrincipalID).HasName("PK__Principa__EB24D48F67612569");

            entity.Property(e => e.PrincipalID).ValueGeneratedNever();

            entity.HasOne(d => d.Account).WithMany(p => p.Principals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Principal_Account1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleID).HasName("PK__Role__8AFACE3A2CD8389F");

            entity.Property(e => e.RoleID).ValueGeneratedNever();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentID).HasName("PK__Student__32C52A79D2CBD709");

            entity.Property(e => e.StudentID).ValueGeneratedNever();

            entity.HasOne(d => d.Class).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student__ClassID__4D94879B");

            entity.HasOne(d => d.Parent).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student__ParentI__4E88ABD4");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherID).HasName("PK__Teacher__EDF259440F70F5B5");

            entity.Property(e => e.TeacherID).ValueGeneratedNever();
        });

        modelBuilder.Entity<Timetable>(entity =>
        {
            entity.HasKey(e => e.TimetableID).HasName("PK__Timetabl__68413F400E66B93F");

            entity.Property(e => e.TimetableID).ValueGeneratedNever();

            entity.HasOne(d => d.Class).WithMany(p => p.Timetables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Timetable__Class__534D60F1");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Timetables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Timetable__Teach__5441852A");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
