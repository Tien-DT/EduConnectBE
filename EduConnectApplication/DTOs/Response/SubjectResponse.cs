using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Response
{
    public class SubjectResponse
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
        public string? Description { get; set; }
        public bool? IsDeleted { get; set; }
        public int LessonCount { get; set; }
        public int ExamCount { get; set; }
    }
}
