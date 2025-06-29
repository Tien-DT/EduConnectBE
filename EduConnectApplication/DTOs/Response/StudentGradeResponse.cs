using System;

namespace EduConnect.Application.DTOs.Response
{
    public class StudentGradeResponse
    {
        public int GradeID { get; set; }
        public int LessonID { get; set; }
        public string LessonName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string TeacherID { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public double? Score { get; set; }
        public string? GradeType { get; set; }
        public int? GradeLevel { get; set; }
        public string? Semester { get; set; }
        public string? Evaluation { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class StudentGradeSummaryResponse
    {
        public string StudentID { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int ClassID { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public List<StudentGradeResponse> Grades { get; set; } = new List<StudentGradeResponse>();
        public double? AverageScore { get; set; }
        public int TotalSubjects { get; set; }
    }
} 