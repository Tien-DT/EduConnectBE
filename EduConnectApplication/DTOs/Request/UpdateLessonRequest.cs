using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request;

public class UpdateLessonRequest
{
    public int? ClassId { get; set; }

    public int? TimetableId { get; set; }

    public DateTime? Date { get; set; }

    public int? SubjectId { get; set; }
}
