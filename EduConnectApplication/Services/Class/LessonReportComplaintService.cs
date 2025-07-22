using EduConnect.Application.DTOs;
using EduConnect.Application.Services.Interface;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.Extensions.Logging;

namespace EduConnect.Application.Services.Class
{
    public class LessonReportComplaintService : ILessonReportComplaintService
    {
        private readonly IStudentComplaintCacheService _complaintCacheService;
        private readonly ILessonReportRepository _lessonReportRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ILogger<LessonReportComplaintService> _logger;

        public LessonReportComplaintService(
            IStudentComplaintCacheService complaintCacheService,
            ILessonReportRepository lessonReportRepository,
            IStudentRepository studentRepository,
            ILogger<LessonReportComplaintService> logger)
        {
            _complaintCacheService = complaintCacheService;
            _lessonReportRepository = lessonReportRepository;
            _studentRepository = studentRepository;
            _logger = logger;
        }

        public async Task ProcessLessonReportComplaintsAsync(int lessonReportId)
        {
            try
            {
                var lessonReport = await _lessonReportRepository.GetLessonReportWithDetailsAsync(lessonReportId);
                if (lessonReport == null)
                {
                    _logger.LogWarning($"Lesson report with ID {lessonReportId} not found");
                    return;
                }
                var complaintNotes = lessonReport.StudentLessonNotes
                    .Where(note => IsComplaintNote(note.NoteType) && !string.IsNullOrEmpty(note.Description))
                    .ToList();

                if (!complaintNotes.Any())
                {
                    _logger.LogInformation($"No complaint notes found in lesson report {lessonReportId}");
                    return;
                }
                var studentGroups = complaintNotes.GroupBy(note => note.StudentID);

                foreach (var studentGroup in studentGroups)
                {
                    await ProcessStudentComplaints(studentGroup.Key, studentGroup.ToList(), lessonReport);
                }

                _logger.LogInformation($"Processed {studentGroups.Count()} students with complaints from lesson report {lessonReportId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing lesson report complaints for report {lessonReportId}");
                throw;
            }
        }

        private async Task ProcessStudentComplaints(string studentId, List<Domain.Entities.StudentLessonNote> complaintNotes, Domain.Entities.LessonReport lessonReport)
        {
            try
            {
                var student = await _studentRepository.GetStudentWithParentAsync(studentId);
                if (student?.Parent == null)
                {
                    _logger.LogWarning($"Student {studentId} or parent information not found");
                    return;
                }
                var timetableSlot = lessonReport.TimetableSlot;
                var className = student.Class?.ClassName ?? "Unknown";
                var subjectName = timetableSlot?.Subject?.SubjectName ?? "Unknown";
                var teacherName = timetableSlot?.Teacher?.FullName ?? "Unknown";
                var complaintData = new StudentComplaintData
                {
                    StudentId = student.StudentID,
                    StudentName = student.FullName ?? "Unknown",
                    ParentId = student.ParentID,
                    ParentName = student.Parent.FullName ?? "Unknown",
                    ClassName = className,
                    Date = lessonReport.LessonDate.Date,
                    Complaints = complaintNotes.Select(note => new ComplaintDetail
                    {
                        SubjectName = subjectName,
                        TeacherName = teacherName,
                        NoteType = note.NoteType,
                        NoteContent = note.Description ?? "",
                        Period = $"Tiết {timetableSlot?.Period ?? 0}",
                        CreatedAt = note.CreatedAt
                    }).ToList()
                };
                await _complaintCacheService.AddStudentComplaintAsync(complaintData);

                _logger.LogInformation($"Added complaint data for student {studentId} to cache for date {lessonReport.LessonDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing complaints for student {studentId}");
                throw;
            }
        }

        private bool IsComplaintNote(string noteType)
        {
            var complaintTypes = new[] { "Violation", "Warning", "Complain", "Complaint" };
            return complaintTypes.Contains(noteType, StringComparer.OrdinalIgnoreCase);
        }
    }
}
