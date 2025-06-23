using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Domain.Entities
{
    public partial class TimetableSlot
    {
        [Key]
        public int TimetableSlotID { get; set; }

        public int TimetableID { get; set; }

        public string TeacherID { get; set; } = string.Empty;

        public int SubjectId { get; set; }

        [Required]
        [StringLength(20)]
        public string DayOfWeek { get; set; } = string.Empty; 

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [StringLength(50)]
        public string? Room { get; set; }

        public bool? IsDeleted { get; set; }

        public int Period { get; set; } 
        [ForeignKey("TimetableID")]
        public virtual Timetable Timetable { get; set; } = null!;

        [ForeignKey("TeacherID")]
        public virtual Teacher Teacher { get; set; } = null!;

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; } = null!;
    }
}
