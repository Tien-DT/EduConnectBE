using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class AssignTeacherRoleRequest
    {
        [Required]
        public string AccountID { get; set; } = string.Empty;
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string TeacherID { get; set; } = string.Empty;
    }

    public class AssignTeacherRoleListRequest
    {
        [Required]
        public List<AssignTeacherRoleRequest> Teachers { get; set; } = new();
    }
}
