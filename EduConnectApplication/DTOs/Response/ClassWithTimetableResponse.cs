namespace EduConnectApplication.DTOs.Response
{
    public class ClassWithTimetableResponse
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int? TimetableSlotId { get; set; }
        // Add more properties as needed, e.g. timetable info
    }
}
