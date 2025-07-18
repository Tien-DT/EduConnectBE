using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class AssignParentRoleRequest
    {
        [Required]
        public string AccountId { get; set; } = string.Empty;
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string ParentId { get; set; } = string.Empty;
    }

    public class AssignParentRoleListRequest
    {
        [Required]
        public List<AssignParentRoleRequest> Parents { get; set; } = new();
    }
}
