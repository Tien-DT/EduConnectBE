using System;

namespace EduConnect.Application.DTOs.Response
{
    public class ClassStudentResponse
    {
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string ParentId { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public int? ClassId { get; set; }
        public string? ClassName { get; set; }
    }
} 