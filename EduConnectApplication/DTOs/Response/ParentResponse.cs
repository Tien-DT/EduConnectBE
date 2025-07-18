using System;
using System.Collections.Generic;

namespace EduConnect.Application.DTOs.Response
{
    public class ParentResponse
    {
        public string ParentId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> StudentNames { get; set; } = new List<string>();
        public int TotalStudents { get; set; }
    }
}
