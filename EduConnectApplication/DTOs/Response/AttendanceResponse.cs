namespace EduConnect.Application.DTOs.Response
{
    public class AttendanceResponse
    {
        public int AttendanceID { get; set; }
        public int TimetableSlotID { get; set; }
        public string StudentID { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? Note { get; set; }
    }
} 