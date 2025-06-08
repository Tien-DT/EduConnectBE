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
    public int ParentID { get; set; }

    [StringLength(255)]
    public string? FullName { get; set; }

    [StringLength(255)]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Email { get; set; }

    [InverseProperty("Parent")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    [InverseProperty("Parent")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
