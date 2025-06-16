using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities;

public partial class ClassTeacher
{
    [Key]
    public int ID { get; set; }

    public int ClassID { get; set; }

    public int TeacherID { get; set; }

    [ForeignKey("ClassID")]
    public virtual Class Class { get; set; } = null!;

    [ForeignKey("TeacherID")]
    public virtual Teacher Teacher { get; set; } = null!;
}
