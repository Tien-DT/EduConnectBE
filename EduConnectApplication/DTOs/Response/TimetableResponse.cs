using System;
using System.Collections.Generic;

namespace EduConnect.Application.DTOs.Response
{
    public class TimetableSlotResponse
    {
        public int TimetableSlotId { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Period { get; set; }
        public string? Room { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
    }

    public class TimetableResponse
    {
        public int TimetableId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<TimetableSlotResponse> Slots { get; set; } = new();
    }
}
