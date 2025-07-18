using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class CreateLessonReportRequest
    {
        public int? LessonId { get; set; }

        [Required]
        public int TimetableSlotId { get; set; }

        [Required]
        public DateTime LessonDate { get; set; }

        public int? TotalStudents { get; set; }

        public int? AbsentStudents { get; set; }

        public string? Remarks { get; set; }

        public List<CreateStudentLessonNoteRequest>? StudentNotes { get; set; } = new List<CreateStudentLessonNoteRequest>();
    }

    public class CreateStudentLessonNoteRequest
    {
        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        public string NoteType { get; set; } = string.Empty;

        public string? NoteContent { get; set; }
    }
}
