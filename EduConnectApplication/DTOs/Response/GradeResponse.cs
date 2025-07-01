using System;

namespace EduConnect.Application.DTOs.Response
{
    public class GradeResponse
    {
        public int GradeID { get; set; }
        public string StudentID { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int LessonID { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string TeacherID { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public double? Score { get; set; }
        public string? GradeType { get; set; }
        public int? GradeLevel { get; set; }
        public string? Semester { get; set; }
        public string? Evaluation { get; set; }
    }
} 