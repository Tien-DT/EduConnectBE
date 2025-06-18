using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Domain.Entities
{
    public class Exam
    {
        [Key]
        public int ExamId { get; set; }

        [Required]
        public int SubjectId { get; set; } 

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string ExamType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ExamNote { get; set; }

        [Required]
        public DateTime ExamDate { get; set; }

        public DateTime? GradeDate { get; set; }

        [StringLength(50)]
        public string? Room { get; set; }

        [Required]
        public string StudentID { get; set; } = string.Empty;

        [ForeignKey("StudentID")]
        public virtual Student Student { get; set; } = null!;
    }
}
