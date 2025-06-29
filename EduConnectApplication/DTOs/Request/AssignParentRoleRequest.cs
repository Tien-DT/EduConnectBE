using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class AssignParentRoleRequest
    {
        [Required]
        public string AccountID { get; set; } = string.Empty;
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string ParentID { get; set; } = string.Empty;
    }

    public class AssignParentRoleListRequest
    {
        [Required]
        public List<AssignParentRoleRequest> Parents { get; set; } = new();
    }
}
