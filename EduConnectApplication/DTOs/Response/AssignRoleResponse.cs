namespace EduConnect.Application.DTOs.Response
{
    public class AssignRoleResult
    {
        public string AccountId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string RoleAssigned { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class AssignStudentRoleResult : AssignRoleResult
    {
        public string ParentId { get; set; } = string.Empty;
    }

    public class AssignRoleResponse
    {
        public List<AssignRoleResult> Results { get; set; } = new();
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
    }
}