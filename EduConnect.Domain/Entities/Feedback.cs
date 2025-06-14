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
    [Column("FeedbackID")]
    public int FeedbackId { get; set; }

    [Column("ParentID")]
    public int ParentId { get; set; }

    [Column(TypeName = "text")]
    public string? Content { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateTime { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Target { get; set; }

    [ForeignKey("ParentId")]
    [InverseProperty("Feedbacks")]
    public virtual Parent Parent { get; set; } = null!;
}
