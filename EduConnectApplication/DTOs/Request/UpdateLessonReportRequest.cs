using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class UpdateLessonReportRequest
    {
        [Required]
        public int ReportId { get; set; }

        public int? LessonId { get; set; }

        public int? TotalStudents { get; set; }

        public int? AbsentStudents { get; set; }

        public string? Remarks { get; set; }

        public List<UpdateStudentLessonNoteRequest>? StudentNotes { get; set; } = new List<UpdateStudentLessonNoteRequest>();
    }

    public class UpdateStudentLessonNoteRequest
    {
        public int? NoteId { get; set; } 

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        public string NoteType { get; set; } = string.Empty;

        public string? NoteContent { get; set; }
    }
}
