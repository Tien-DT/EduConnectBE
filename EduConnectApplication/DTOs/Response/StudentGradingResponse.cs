namespace EduConnect.Application.DTOs.Response
{
    public class StudentGradingResponse
    {
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        
        /// <summary>
        /// Danh sách điểm hiện có của học sinh trong môn này
        /// </summary>
        public List<GradeResponse> ExistingGrades { get; set; } = new List<GradeResponse>();
    }
}
