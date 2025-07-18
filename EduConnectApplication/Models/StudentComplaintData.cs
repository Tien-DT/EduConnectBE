using System;
using System.Collections.Generic;

namespace EduConnect.Application.Models
{
    public class StudentComplaintData
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string ParentId { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<ComplaintDetail> Complaints { get; set; } = new List<ComplaintDetail>();
    }

    public class ComplaintDetail
    {
        public string SubjectName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string NoteType { get; set; } = string.Empty;
        public string NoteContent { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class DailyComplaintSummary
    {
        public DateTime Date { get; set; }
        public List<StudentComplaintData> Students { get; set; } = new List<StudentComplaintData>();
    }
}
