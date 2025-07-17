namespace EduConnect.Application.Services.Interface
{
    public interface ILessonReportComplaintService
    {
        Task ProcessLessonReportComplaintsAsync(int lessonReportId);
    }
}
