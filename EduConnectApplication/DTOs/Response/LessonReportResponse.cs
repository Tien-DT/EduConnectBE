using System;
using System.Collections.Generic;

namespace EduConnect.Application.DTOs.Response
{
    public class LessonReportResponse
    {
        public int ReportID { get; set; }
        public int? LessonID { get; set; }
        public int TimetableSlotID { get; set; }
        public DateTime LessonDate { get; set; }
        public int? TotalStudents { get; set; }
        public int? AbsentStudents { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public string? LessonName { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string TeacherID { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int ClassID { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public int Period { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Room { get; set; }
        
        public List<StudentLessonNoteResponse> StudentNotes { get; set; } = new List<StudentLessonNoteResponse>();
    }

    public class StudentLessonNoteResponse
    {
        public int NoteID { get; set; }
        public string StudentID { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string NoteType { get; set; } = string.Empty;
        public string? NoteContent { get; set; }
    }
}
