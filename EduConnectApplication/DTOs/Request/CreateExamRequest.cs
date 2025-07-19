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
        public string ExamType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ExamNote { get; set; }

        [Required]
        public DateTime ExamDate { get; set; }

        public DateTime? GradeDate { get; set; }

        [StringLength(50)]
        public string? Room { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;
    }

    public class CreateClassExamRequest
    {
        [Required]
        public int SubjectId { get; set; }

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
        public int ClassId { get; set; }

        [Required]
        public string TeacherId { get; set; } = string.Empty;
    }

    public class CreatePrincipalExamRequest
    {
        [Required]
        public int SubjectId { get; set; }

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
        public string PrincipalId { get; set; } = string.Empty;

        [Required]
        public List<string> StudentIds { get; set; } = new List<string>();
    }

    public class UpdateExamRequest
    {
        [Required]
        public int SubjectId { get; set; }

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
        public string StudentId { get; set; } = string.Empty;
    }
}