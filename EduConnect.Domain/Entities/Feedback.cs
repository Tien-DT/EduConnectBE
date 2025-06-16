using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities;

public partial class Feedback
{
    [Key]
    public int FeedbackID { get; set; }

    public int ParentID { get; set; }

    [Column(TypeName = "text")]
    public string? Content { get; set; }

    public DateTime? DateTime { get; set; }

    [StringLength(100)]
    public string? Target { get; set; }

    [ForeignKey("ParentID")]
    public virtual Parent Parent { get; set; } = null!;
}
