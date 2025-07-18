namespace EduConnect.Application.DTOs.Request
{
    public class AttendanceRequest
    {
        public int LessonID { get; set; }
        public string StudentID { get; set; } = string.Empty;
        public bool? IsPresent { get; set; }
        public string? Note { get; set; }
    }
} 