namespace EduConnect.Application.DTOs.Request
{
    public class CreateGradeRequest
    {
        public string StudentID { get; set; } = string.Empty;
        public int LessonID { get; set; }
        public double? Score { get; set; }
        public string? GradeType { get; set; }
        public int? GradeLevel { get; set; }
        public string? Semester { get; set; }
        public string? Evaluation { get; set; }
    }
} 