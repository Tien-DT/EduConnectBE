using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities;

public partial class Parent
{
    [Key]
    public string ParentID { get; set; } = string.Empty;

    [Required]
    public string AccountID { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FullName { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("AccountID")]
    public virtual ApplicationUser Account { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
