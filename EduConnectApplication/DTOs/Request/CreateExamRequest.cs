using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Request
{
    public class CreateExamRequest
    {
        [Required]
        public int SubjectId { get; set; }

        [Required]
        [StringLength(50)]
        public string ExamType { get; set; }

        [StringLength(500)]
        public string? ExamNote { get; set; }

        [Required]
        public DateTime ExamDate { get; set; }

        public DateTime? GradeDate { get; set; }

        [StringLength(50)]
        public string? Room { get; set; }

        [Required]
        public string StudentID { get; set; }
    }

    public class CreateClassExamRequest
    {
        [Required]
        public int SubjectId { get; set; }

        [Required]
        [StringLength(50)]
        public string ExamType { get; set; }

        [StringLength(500)]
        public string? ExamNote { get; set; }

        [Required]
        public DateTime ExamDate { get; set; }

        public DateTime? GradeDate { get; set; }

        [StringLength(50)]
        public string? Room { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public string TeacherAccountId { get; set; }
    }

    public class CreatePrincipalExamRequest
    {
        [Required]
        public int SubjectId { get; set; }

        [Required]
        [StringLength(50)]
        public string ExamType { get; set; }

        [StringLength(500)]
        public string? ExamNote { get; set; }

        [Required]
        public DateTime ExamDate { get; set; }

        public DateTime? GradeDate { get; set; }

        [StringLength(50)]
        public string? Room { get; set; }

        [Required]
        public string PrincipalAccountId { get; set; }

        [Required]
        public List<string> StudentIds { get; set; } = new List<string>();
    }
}