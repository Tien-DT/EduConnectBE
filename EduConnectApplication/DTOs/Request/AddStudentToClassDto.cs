namespace EduConnectApplication.DTOs.Request;

public class AddStudentToClassDto
{
    public required string StudentID { get; set; }
    public required string FullName { get; set; }
    public string? AccountID { get; set; }
    public string? ParentID { get; set; }
}
