namespace EduConnect.Application.DTOs.Request
{
    public class AttendanceRequest
    {
        public int TimetableSlotID { get; set; }
        public string StudentID { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? Note { get; set; }
    }
} 