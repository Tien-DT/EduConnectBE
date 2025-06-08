using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Domain.Entities;

[Table("Feedback")]
public partial class Feedback
{
    [Key]
    public int FeedbackID { get; set; }

    public int ParentID { get; set; }

    [Column(TypeName = "text")]
    public string? Content { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateTime { get; set; }

    [StringLength(255)]
    public string? Target { get; set; }

    [ForeignKey("ParentID")]
    [InverseProperty("Feedbacks")]
    public virtual Parent Parent { get; set; } = null!;
}
