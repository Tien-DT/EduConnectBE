using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class AssignStudentRoleRequest
    {
        [Required]
        public string AccountId { get; set; } = string.Empty;
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string StudentId { get; set; } = string.Empty;
        
        [Required]
        public string ParentId { get; set; } = string.Empty;
    }

    public class AssignStudentRoleListRequest
    {
        [Required]
        public List<AssignStudentRoleRequest> Students { get; set; } = new();
    }
}
