using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request;

public class CreateLessonRequest
{
    [Required]
    public int ClassId { get; set; }

    [Required]
    public string TeacherId { get; set; } = string.Empty;

    [Required]
    public int TimetableId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public int SubjectId { get; set; }
}
