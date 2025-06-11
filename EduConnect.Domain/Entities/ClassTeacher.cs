using System;
using System.Collections.Generic;

namespace EduConnect.Domain.Entities;

public partial class ClassTeacher
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int TeacherId { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;
}
