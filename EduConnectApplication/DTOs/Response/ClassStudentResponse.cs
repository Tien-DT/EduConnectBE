using System;

namespace EduConnect.Application.DTOs.Response
{
    public class ClassStudentResponse
    {
        public string StudentID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string ParentID { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public int? ClassID { get; set; }
        public string? ClassName { get; set; }
    }
} 