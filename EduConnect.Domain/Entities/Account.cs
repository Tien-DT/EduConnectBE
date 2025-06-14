using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Domain.Entities;

[Table("Account")]
public partial class Account
{
    [Key]
    [Column("AccountID")]
    public Guid AccountId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Password { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [Column("RoleID")]
    public int? RoleId { get; set; }

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    public string? RefreshToken { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RefreshTokenExpTime { get; set; }

    [InverseProperty("Account")]
    public virtual ICollection<Parent> Parents { get; set; } = new List<Parent>();

    [InverseProperty("Account")]
    public virtual ICollection<Principal> Principals { get; set; } = new List<Principal>();

    [ForeignKey("RoleId")]
    [InverseProperty("Accounts")]
    public virtual Role? Role { get; set; }

    [InverseProperty("Account")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    [InverseProperty("Account")]
    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
