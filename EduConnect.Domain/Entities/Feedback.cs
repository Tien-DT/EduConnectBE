using System;
using System.Collections.Generic;

namespace EduConnect.Domain.Entities;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int ParentId { get; set; }

    public string? Content { get; set; }

    public DateTime? DateTime { get; set; }

    public string? Target { get; set; }

    public virtual Parent Parent { get; set; } = null!;
}
