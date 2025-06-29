namespace EduConnect.Application.DTOs.Request
{
    public class UpdateGradeRequest
    {
        public int GradeID { get; set; }
        public double? Score { get; set; }
        public string? GradeType { get; set; }
        public int? GradeLevel { get; set; }
        public string? Semester { get; set; }
        public string? Evaluation { get; set; }
    }
} 