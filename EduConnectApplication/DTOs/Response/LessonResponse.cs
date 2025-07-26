namespace EduConnect.Application.DTOs.Response;

public class LessonResponse
{
    public int LessonId { get; set; }
    public int ClassId { get; set; }
    public string TeacherId { get; set; } = string.Empty;
    public int TimetableId { get; set; }
    public DateTime Date { get; set; }
    public int SubjectId { get; set; }
    public bool? IsDeleted { get; set; }
    
    // Navigation properties
    public string ClassName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string TimetableSlot { get; set; } = string.Empty;
}
