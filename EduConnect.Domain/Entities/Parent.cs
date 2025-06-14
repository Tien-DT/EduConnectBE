using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Domain.Entities;

[Table("Parent")]
public partial class Parent
{
    [Key]
    [Column("ParentID")]
    public int ParentId { get; set; }

    [Column("AccountID")]
    public Guid AccountId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? FullName { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("Parents")]
    public virtual Account Account { get; set; } = null!;

    [InverseProperty("Parent")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    [InverseProperty("Parent")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
