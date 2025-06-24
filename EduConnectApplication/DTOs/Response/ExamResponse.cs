using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Response
{
    public class ExamResponse
    {
        public int ExamId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string ExamType { get; set; } = string.Empty;
        public string ExamNote { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public DateTime? GradeDate { get; set; }
        public string Room { get; set; } = string.Empty;
    }
}