using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class AssignStudentRoleRequest
    {
        [Required]
        public string AccountID { get; set; } = string.Empty;
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string StudentID { get; set; } = string.Empty;
        
        [Required]
        public string ParentID { get; set; } = string.Empty;
    }

    public class AssignStudentRoleListRequest
    {
        [Required]
        public List<AssignStudentRoleRequest> Students { get; set; } = new();
    }
}
