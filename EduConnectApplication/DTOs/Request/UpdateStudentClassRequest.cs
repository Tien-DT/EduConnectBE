namespace EduConnect.Application.DTOs.Request
{
    public class UpdateStudentClassRequest
    {
        public string StudentID { get; set; } = string.Empty;
        public int? NewClassID { get; set; }
    }
} 