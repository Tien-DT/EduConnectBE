using System;

namespace EduConnect.Application.DTOs.Response
{
    public class StudentResponse
    {
        public string StudentID { get; set; } = string.Empty;
        public string AccountID { get; set; } = string.Empty;
        public string ParentID { get; set; } = string.Empty;
        public int? ClassID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
    }
}
