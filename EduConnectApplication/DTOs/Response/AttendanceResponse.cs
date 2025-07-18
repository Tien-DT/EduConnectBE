namespace EduConnect.Application.DTOs.Response
{
    public class AttendanceResponse
    {
        public int AttendanceID { get; set; }
        public int LessonID { get; set; }
        public string StudentID { get; set; } = string.Empty;
        public bool? IsPresent { get; set; }
        public string? Note { get; set; }
    }
} 