using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class UpdateGradeRequest
    {
        [Range(0, 10, ErrorMessage = "Score must be between 0 and 10")]
        public double? Score { get; set; }

        [StringLength(50, ErrorMessage = "Grade type cannot exceed 50 characters")]
        public string? GradeType { get; set; }

        [Range(1, 12, ErrorMessage = "Grade level must be between 1 and 12")]
        public int? GradeLevel { get; set; }

        [StringLength(50, ErrorMessage = "Semester cannot exceed 50 characters")]
        public string? Semester { get; set; }

        [StringLength(255, ErrorMessage = "Evaluation cannot exceed 255 characters")]
        public string? Evaluation { get; set; }
    }
} 