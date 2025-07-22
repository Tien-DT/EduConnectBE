using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Request
{
    public class CreateSubjectRequest
    {
        [Required]
        [StringLength(100)]
        public string SubjectName { get; set; } = string.Empty;

        [StringLength(10)]
        public string? SubjectCode { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateSubjectRequest
    {
        [Required]
        [StringLength(100)]
        public string SubjectName { get; set; } = string.Empty;

        [StringLength(10)]
        public string? SubjectCode { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
