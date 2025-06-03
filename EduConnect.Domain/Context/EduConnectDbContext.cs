using System;
using System.Collections.Generic;
using EduConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Domain.Context;

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
    }
}

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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
