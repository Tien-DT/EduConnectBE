using System;

namespace EduConnect.Application.DTOs.Request
{
    public class GetStudentReportDetailsRequest
    {
        public string Period { get; set; } = "weekly";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? NoteType { get; set; }
    }
}
