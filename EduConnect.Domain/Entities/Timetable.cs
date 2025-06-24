using EduConnect.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public partial class Timetable
{
    [Key]
    public int TimetableID { get; set; }

    public int ClassID { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty; 

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("ClassID")]
    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<TimetableSlot> TimetableSlots { get; set; } = new List<TimetableSlot>();
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}