using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class UpdateLessonReportRequest
    {
        [Required]
        public int ReportID { get; set; }

        public int? LessonID { get; set; }

        public int? TotalStudents { get; set; }

        public int? AbsentStudents { get; set; }

        public string? Remarks { get; set; }

        public List<UpdateStudentLessonNoteRequest>? StudentNotes { get; set; } = new List<UpdateStudentLessonNoteRequest>();
    }

    public class UpdateStudentLessonNoteRequest
    {
        public int? NoteID { get; set; } // null for new notes

        [Required]
        public string StudentID { get; set; } = string.Empty;

        [Required]
        public string NoteType { get; set; } = string.Empty;

        public string? NoteContent { get; set; }
    }
}
