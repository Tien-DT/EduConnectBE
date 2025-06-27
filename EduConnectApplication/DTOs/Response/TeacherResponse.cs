using System;
using System.Collections.Generic;

namespace EduConnect.Application.DTOs.Response
{
    public class TeacherResponse
    {
        public string TeacherID { get; set; } = string.Empty;
        public string AccountID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Classes { get; set; } = new List<string>();
    }
}
