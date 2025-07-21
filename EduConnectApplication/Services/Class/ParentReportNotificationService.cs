using AutoMapper;
using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Options;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class ParentReportNotificationService : IParentReportNotificationService
    {
        private readonly ILessonReportRepository _lessonReportRepository;
        private readonly IStudentLessonNoteRepository _studentLessonNoteRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IParentRepository _parentRepository;
        private readonly INotificationService _notificationService;
        private readonly EduConnectDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly GeminiOption _geminiOptions;
        private readonly ILogger<ParentReportNotificationService> _logger;
        private readonly IMapper _mapper;

        public ParentReportNotificationService(
            ILessonReportRepository lessonReportRepository,
            IStudentLessonNoteRepository studentLessonNoteRepository,
            IStudentRepository studentRepository,
            IParentRepository parentRepository,
            INotificationService notificationService,
            EduConnectDbContext context,
            HttpClient httpClient,
            IOptions<GeminiOption> geminiOptions,
            ILogger<ParentReportNotificationService> logger,
            IMapper mapper)
        {
            _lessonReportRepository = lessonReportRepository;
            _studentLessonNoteRepository = studentLessonNoteRepository;
            _studentRepository = studentRepository;
            _parentRepository = parentRepository;
            _notificationService = notificationService;
            _context = context;
            _httpClient = httpClient;
            _geminiOptions = geminiOptions.Value;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResponse<string>> SendDailyNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Bắt đầu gửi thông báo hàng ngày cho phụ huynh...");

                var (startDate, endDate) = GetDateRange("daily");
                return await SendNotificationsByPeriod("daily", startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo hàng ngày");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi thông báo hàng ngày",
                    Data = ex.Message
                };
            }
        }

        public async Task<ApiResponse<string>> SendWeeklyNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Bắt đầu gửi thông báo hàng tuần cho phụ huynh...");

                var (startDate, endDate) = GetDateRange("weekly");
                return await SendNotificationsByPeriod("weekly", startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo hàng tuần");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi thông báo hàng tuần",
                    Data = ex.Message
                };
            }
        }

        public async Task<ApiResponse<string>> SendMonthlyNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Bắt đầu gửi thông báo hàng tháng cho phụ huynh...");

                var (startDate, endDate) = GetDateRange("monthly");
                return await SendNotificationsByPeriod("monthly", startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo hàng tháng");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi thông báo hàng tháng",
                    Data = ex.Message
                };
            }
        }

        public async Task<ApiResponse<string>> SendNotificationToParentAsync(string parentId, string period)
        {
            try
            {
                _logger.LogInformation($"Gửi thông báo {period} cho phụ huynh: {parentId}");

                var (startDate, endDate) = GetDateRange(period);

                // Lấy tất cả học sinh của phụ huynh này
                var students = await _context.Students
                    .Where(s => s.ParentID == parentId && (s.IsDeleted == null || s.IsDeleted == false))
                    .ToListAsync();

                if (!students.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Không tìm thấy học sinh nào của phụ huynh này"
                    };
                }

                var studentIds = students.Select(s => s.StudentID).ToList();

                // Lấy các lesson notes của tất cả học sinh trong khoảng thời gian
                var allStudentNotes = await GetStudentNotesByPeriod(studentIds, startDate, endDate);

                if (!allStudentNotes.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = true,
                        Message = $"Không có phản ánh nào cho các con em trong {GetPeriodDisplayName(period)} này",
                        Data = "No notifications needed"
                    };
                }

                var result = await SendNotificationToParent(parentId, allStudentNotes, startDate, endDate, period);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi thông báo {period} cho phụ huynh {parentId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi thông báo",
                    Data = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PagedResponse<object>>> GetStudentReportsAsync(GetStudentReportsRequest request)
        {
            try
            {
                var (startDate, endDate) = GetDateRangeFromRequest(request);

                _logger.LogInformation($"Lấy báo cáo phản ánh {request.Period} từ {startDate:yyyy-MM-dd} đến {endDate:yyyy-MM-dd}");

                // Build query
                var query = _context.StudentLessonNotes
                    .Include(n => n.Student)
                        .ThenInclude(s => s.Class)
                    .Include(n => n.LessonReport)
                        .ThenInclude(lr => lr.TimetableSlot)
                            .ThenInclude(ts => ts.Subject)
                    .Include(n => n.LessonReport)
                        .ThenInclude(lr => lr.TimetableSlot)
                            .ThenInclude(ts => ts.Teacher)
                    .Where(n => (n.IsDeleted == null || n.IsDeleted == false) &&
                               n.CreatedAt >= startDate && n.CreatedAt <= endDate);

                // Apply filters
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(n => (n.Student.FullName ?? "").Contains(request.SearchTerm) ||
                                           (n.Description ?? "").Contains(request.SearchTerm));
                }

                if (request.ClassId.HasValue)
                {
                    query = query.Where(n => n.Student.ClassID == request.ClassId.Value);
                }

                if (!string.IsNullOrWhiteSpace(request.NoteType))
                {
                    query = query.Where(n => n.NoteType == request.NoteType);
                }

                var totalCount = await query.CountAsync();

                // Group by student and apply sorting
                var studentReports = await query
                    .GroupBy(n => n.StudentID)
                    .Select(g => new
                    {
                        StudentId = g.Key,
                        StudentName = g.First().Student.FullName,
                        ClassName = g.First().Student.Class.ClassName,
                        ClassId = g.First().Student.ClassID,
                        TotalNotes = g.Count(),
                        ViolationCount = g.Count(n => n.NoteType == "Violation"),
                        WarningCount = g.Count(n => n.NoteType == "Warning"),
                        BonusPointCount = g.Count(n => n.NoteType == "BonusPoint"),
                        GoodCount = g.Count(n => n.NoteType == "Good"),
                        TotalPoints = g.Sum(n => n.Points ?? 0),
                        LatestCreatedAt = g.Max(n => n.CreatedAt),
                        Notes = g.Select(n => new
                        {
                            n.StudentLessonNoteID,
                            n.NoteType,
                            n.Points,
                            n.Description,
                            n.CreatedAt,
                            Subject = n.LessonReport.TimetableSlot.Subject.SubjectName,
                            Teacher = n.LessonReport.TimetableSlot.Teacher.FullName,
                            LessonDate = n.LessonReport.LessonDate
                        }).OrderByDescending(n => n.CreatedAt).Take(3).ToList()
                    })
                    .ToListAsync();

                // Apply sorting
                var sortedReports = ApplySorting(studentReports, request.SortBy, request.Ascending);

                // Apply paging
                var pagedReports = sortedReports
                    .Skip(request.PageIndex * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var response = new PagedResponse<object>
                {
                    Items = pagedReports.Cast<object>().ToList(),
                    TotalCount = sortedReports.Count,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize
                };

                return new ApiResponse<PagedResponse<object>>
                {
                    Success = true,
                    Message = $"Báo cáo phản ánh {GetPeriodDisplayName(request.Period)} từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy báo cáo phản ánh {request.Period}");
                return new ApiResponse<PagedResponse<object>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy báo cáo phản ánh"
                };
            }
        }

        public async Task<ApiResponse<object>> GetStudentReportDetailsAsync(string studentId, GetStudentReportDetailsRequest request)
        {
            try
            {
                var (startDate, endDate) = GetDateRangeFromDetailsRequest(request);

                _logger.LogInformation($"Lấy chi tiết báo cáo {request.Period} cho học sinh {studentId}");

                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null)
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy học sinh"
                    };
                }

                var query = _context.StudentLessonNotes
                    .Include(n => n.LessonReport)
                        .ThenInclude(lr => lr.TimetableSlot)
                            .ThenInclude(ts => ts.Subject)
                    .Include(n => n.LessonReport)
                        .ThenInclude(lr => lr.TimetableSlot)
                            .ThenInclude(ts => ts.Teacher)
                    .Where(n => n.StudentID == studentId &&
                               (n.IsDeleted == null || n.IsDeleted == false) &&
                               n.CreatedAt >= startDate && n.CreatedAt <= endDate);

                if (!string.IsNullOrWhiteSpace(request.NoteType))
                {
                    query = query.Where(n => n.NoteType == request.NoteType);
                }

                var studentNotes = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                var summary = new
                {
                    StudentId = studentId,
                    StudentName = student.FullName,
                    ClassName = student.Class?.ClassName,
                    Period = GetPeriodDisplayName(request.Period),
                    DateRange = new { StartDate = startDate, EndDate = endDate },
                    TotalNotes = studentNotes.Count,
                    ViolationCount = studentNotes.Count(n => n.NoteType == "Violation"),
                    WarningCount = studentNotes.Count(n => n.NoteType == "Warning"),
                    BonusPointCount = studentNotes.Count(n => n.NoteType == "BonusPoint"),
                    GoodCount = studentNotes.Count(n => n.NoteType == "Good"),
                    TotalPoints = studentNotes.Sum(n => n.Points ?? 0),
                    Notes = studentNotes.Select(n => new
                    {
                        n.StudentLessonNoteID,
                        n.NoteType,
                        n.Points,
                        n.Description,
                        n.CreatedAt,
                        LessonInfo = new
                        {
                            n.LessonReport.LessonDate,
                            Subject = n.LessonReport.TimetableSlot?.Subject?.SubjectName,
                            Teacher = n.LessonReport.TimetableSlot?.Teacher?.FullName
                        }
                    }).ToList()
                };

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Chi tiết báo cáo {GetPeriodDisplayName(request.Period)} của học sinh {student.FullName}",
                    Data = summary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy chi tiết báo cáo {request.Period} cho học sinh {studentId}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy chi tiết báo cáo học sinh"
                };
            }
        }

        #region Private Methods

        private (DateTime StartDate, DateTime EndDate) GetDateRange(string period)
        {
            var today = DateTime.Today;

            return period.ToLower() switch
            {
                "daily" => (today, today.AddDays(1).AddSeconds(-1)),
                "weekly" => GetCurrentWeekRange(),
                "monthly" => GetCurrentMonthRange(),
                _ => GetCurrentWeekRange()
            };
        }

        private (DateTime StartDate, DateTime EndDate) GetDateRangeFromRequest(GetStudentReportsRequest request)
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                return (request.StartDate.Value.Date, request.EndDate.Value.Date.AddDays(1).AddSeconds(-1));
            }

            return GetDateRange(request.Period);
        }

        private (DateTime StartDate, DateTime EndDate) GetDateRangeFromDetailsRequest(GetStudentReportDetailsRequest request)
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                return (request.StartDate.Value.Date, request.EndDate.Value.Date.AddDays(1).AddSeconds(-1));
            }

            return GetDateRange(request.Period);
        }

        private (DateTime StartOfWeek, DateTime EndOfWeek) GetCurrentWeekRange()
        {
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;
            
            var startOfWeek = today.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
            var endOfWeek = startOfWeek.AddDays(6).AddDays(1).AddSeconds(-1);

            return (startOfWeek, endOfWeek);
        }

        private (DateTime StartOfMonth, DateTime EndOfMonth) GetCurrentMonthRange()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddSeconds(-1);

            return (startOfMonth, endOfMonth);
        }

        private string GetPeriodDisplayName(string period)
        {
            return period.ToLower() switch
            {
                "daily" => "hàng ngày",
                "weekly" => "hàng tuần", 
                "monthly" => "hàng tháng",
                _ => "hàng tuần"
            };
        }

        private async Task<ApiResponse<string>> SendNotificationsByPeriod(string period, DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation($"Xử lý dữ liệu {period} từ {startDate:yyyy-MM-dd} đến {endDate:yyyy-MM-dd}");

            var weeklyReports = await _lessonReportRepository.GetByDateRangeAsync(startDate, endDate);
            var reportsWithNotes = weeklyReports.Where(r => r.StudentLessonNotes?.Any() == true).ToList();

            if (!reportsWithNotes.Any())
            {
                _logger.LogInformation($"Không có phản ánh nào trong {period} này");
                return new ApiResponse<string>
                {
                    Success = true,
                    Message = $"Không có phản ánh nào trong {GetPeriodDisplayName(period)} này để gửi thông báo",
                    Data = "No notifications needed"
                };
            }

            var parentNotifications = await GroupNotificationsByParent(reportsWithNotes);

            var successCount = 0;
            var failCount = 0;

            foreach (var parentNotification in parentNotifications)
            {
                try
                {
                    var result = await SendNotificationToParent(parentNotification.Key, parentNotification.Value, startDate, endDate, period);
                    if (result.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                        _logger.LogWarning($"Gửi thông báo {period} thất bại cho phụ huynh {parentNotification.Key}: {result.Message}");
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    _logger.LogError(ex, $"Lỗi khi gửi thông báo {period} cho phụ huynh {parentNotification.Key}");
                }

                await Task.Delay(1000); // Rate limiting
            }

            var message = $"Đã gửi thành công {successCount} thông báo {GetPeriodDisplayName(period)}, thất bại {failCount} thông báo";
            _logger.LogInformation(message);

            return new ApiResponse<string>
            {
                Success = successCount > 0,
                Message = message,
                Data = $"Success: {successCount}, Failed: {failCount}"
            };
        }

        private async Task<List<StudentLessonNote>> GetStudentNotesByPeriod(List<string> studentIds, DateTime startDate, DateTime endDate)
        {
            var allStudentNotes = new List<StudentLessonNote>();
            
            foreach (var studentId in studentIds)
            {
                var reports = await _lessonReportRepository.GetByStudentIdAsync(studentId, startDate, endDate);
                var studentNotes = reports.SelectMany(r => r.StudentLessonNotes ?? new List<StudentLessonNote>())
                                        .Where(n => n.StudentID == studentId && (n.IsDeleted == null || n.IsDeleted == false))
                                        .ToList();
                allStudentNotes.AddRange(studentNotes);
            }

            return allStudentNotes;
        }

        private async Task<Dictionary<string, List<StudentLessonNote>>> GroupNotificationsByParent(List<LessonReport> reports)
        {
            var parentNotifications = new Dictionary<string, List<StudentLessonNote>>();

            foreach (var report in reports)
            {
                if (report.StudentLessonNotes == null) continue;

                foreach (var note in report.StudentLessonNotes.Where(n => n.IsDeleted == null || n.IsDeleted == false))
                {
                    var student = await _studentRepository.GetByIdAsync(note.StudentID);
                    if (student?.ParentID != null)
                    {
                        if (!parentNotifications.ContainsKey(student.ParentID))
                        {
                            parentNotifications[student.ParentID] = new List<StudentLessonNote>();
                        }
                        parentNotifications[student.ParentID].Add(note);
                    }
                }
            }

            return parentNotifications;
        }

        private async Task<ApiResponse<string>> SendNotificationToParent(string parentId, List<StudentLessonNote> studentNotes, DateTime startDate, DateTime endDate, string period)
        {
            try
            {
                var parent = await _parentRepository.GetByIdAsync(parentId);
                if (parent == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin phụ huynh"
                    };
                }

                var notesByStudent = studentNotes.GroupBy(n => n.StudentID).ToList();
                var notificationContent = await GenerateNotificationContent(notesByStudent, startDate, endDate, period);

                var title = $"📋 Báo cáo {GetPeriodDisplayName(period)} ({startDate:dd/MM} - {endDate:dd/MM})";
                
                var data = new Dictionary<string, string>
                {
                    { "type", $"{period}_report" },
                    { "parent_id", parentId },
                    { "period", period },
                    { "start_date", startDate.ToString("yyyy-MM-dd") },
                    { "end_date", endDate.ToString("yyyy-MM-dd") },
                    { "total_notes", studentNotes.Count.ToString() }
                };

                var result = await _notificationService.SendNotificationToAllDevicesAsync(
                    parent.AccountID, 
                    title, 
                    notificationContent.Summary, 
                    data);

                if (result.Success)
                {
                    _logger.LogInformation($"Đã gửi thông báo {period} thành công cho phụ huynh {parentId}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi thông báo {period} cho phụ huynh {parentId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi thông báo"
                };
            }
        }

        private async Task<(string Summary, string DetailContent)> GenerateNotificationContent(
            IEnumerable<IGrouping<string, StudentLessonNote>> notesByStudent, 
            DateTime startDate, 
            DateTime endDate,
            string period)
        {
            try
            {
                var studentsInfo = new List<object>();

                foreach (var studentGroup in notesByStudent)
                {
                    var student = await _studentRepository.GetByIdAsync(studentGroup.Key);
                    var notes = studentGroup.ToList();

                    studentsInfo.Add(new
                    {
                        StudentName = student?.FullName ?? "Unknown",
                        Notes = notes.Select(n => new
                        {
                            Type = n.NoteType,
                            Points = n.Points,
                            Description = n.Description,
                            Date = n.CreatedAt.ToString("dd/MM/yyyy"),
                            Subject = n.LessonReport?.TimetableSlot?.Subject?.SubjectName,
                            Teacher = n.LessonReport?.TimetableSlot?.Teacher?.FullName
                        }).ToList()
                    });
                }

                var prompt = $@"
Hãy tạo một thông báo ngắn gọn cho phụ huynh về tình hình học tập của con em họ trong {GetPeriodDisplayName(period)} từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}.

Thông tin chi tiết:
{System.Text.Json.JsonSerializer.Serialize(studentsInfo, new System.Text.Json.JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping })}

Yêu cầu:
1. Tạo một tóm tắt ngắn (1-2 câu) để làm nội dung push notification
2. Tạo nội dung chi tiết hơn để gửi qua email hoặc tin nhắn

Trả về theo định dạng JSON:
{{
    ""summary"": ""Tóm tắt ngắn cho push notification"",
    ""detail"": ""Nội dung chi tiết""
}}

Chú ý:
- Sử dụng ngôn ngữ tích cực, khuyến khích
- Nếu có vi phạm, đề xuất cách cải thiện
- Nếu có điểm tốt, khen ngợi cụ thể
- Ngắn gọn, dễ hiểu";

                var geminiResponse = await GenerateContentWithGeminiAsync(prompt);

                if (geminiResponse.Success && !string.IsNullOrEmpty(geminiResponse.Data))
                {
                    try
                    {
                        var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(geminiResponse.Data);
                        if (jsonResponse != null)
                        {
                            return (jsonResponse.GetValueOrDefault("summary", "Có thông tin mới về con em"), 
                                   jsonResponse.GetValueOrDefault("detail", "Vui lòng kiểm tra ứng dụng để xem chi tiết"));
                        }
                    }
                    catch
                    {
                        // Fallback nếu không parse được JSON
                    }
                }

                // Fallback content
                return (CreateFallbackSummary(notesByStudent, period), CreateFallbackDetail(notesByStudent, startDate, endDate, period));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo nội dung thông báo bằng AI");
                return (CreateFallbackSummary(notesByStudent, period), CreateFallbackDetail(notesByStudent, startDate, endDate, period));
            }
        }

        private string CreateFallbackSummary(IEnumerable<IGrouping<string, StudentLessonNote>> notesByStudent, string period)
        {
            var totalNotes = notesByStudent.Sum(g => g.Count());
            var violationCount = notesByStudent.Sum(g => g.Count(n => n.NoteType == "Violation"));
            var goodCount = notesByStudent.Sum(g => g.Count(n => n.NoteType == "Good" || n.NoteType == "BonusPoint"));

            var periodName = GetPeriodDisplayName(period);

            if (violationCount > 0 && goodCount > 0)
            {
                return $"Con em có {goodCount} điểm tốt và {violationCount} lưu ý cần cải thiện trong {periodName} này";
            }
            else if (violationCount > 0)
            {
                return $"Con em có {violationCount} lưu ý cần cải thiện trong {periodName} này";
            }
            else if (goodCount > 0)
            {
                return $"Con em có {goodCount} điểm tốt trong {periodName} này! 🎉";
            }
            else
            {
                return $"Có {totalNotes} ghi chú về con em trong {periodName} này";
            }
        }

        private string CreateFallbackDetail(IEnumerable<IGrouping<string, StudentLessonNote>> notesByStudent, DateTime startDate, DateTime endDate, string period)
        {
            var detail = $"📅 Báo cáo {GetPeriodDisplayName(period)} từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}\n\n";

            foreach (var studentGroup in notesByStudent)
            {
                detail += $"👤 Học sinh: {studentGroup.First().Student?.FullName ?? "Unknown"}\n";
                
                foreach (var note in studentGroup.OrderByDescending(n => n.CreatedAt))
                {
                    var icon = note.NoteType switch
                    {
                        "Violation" => "⚠️",
                        "Warning" => "⚡",
                        "Good" => "👍",
                        "BonusPoint" => "⭐",
                        _ => "📝"
                    };

                    detail += $"{icon} {note.Description} ({note.CreatedAt:dd/MM})\n";
                }
                detail += "\n";
            }

            return detail;
        }

        private async Task<ApiResponse<string>> GenerateContentWithGeminiAsync(string prompt)
        {
            try
            {
                var request = new GeminiRequest
                {
                    Contents = new[]
                    {
                        new GeminiContent
                        {
                            Role = "user",
                            Parts = new[] { new GeminiPart { Text = prompt } }
                        }
                    },
                    GenerationConfig = new GenerationConfig
                    {
                        Temperature = 0.7,
                        TopK = 1,
                        TopP = 1,
                        MaxOutputTokens = 1024
                    },
                    SafetySettings = new[]
                    {
                        new SafetySetting { Category = "HARM_CATEGORY_HARASSMENT", Threshold = "BLOCK_NONE" },
                        new SafetySetting { Category = "HARM_CATEGORY_HATE_SPEECH", Threshold = "BLOCK_NONE" },
                        new SafetySetting { Category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", Threshold = "BLOCK_NONE" },
                        new SafetySetting { Category = "HARM_CATEGORY_DANGEROUS_CONTENT", Threshold = "BLOCK_NONE" }
                    }
                };

                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var fullUrl = $"{_geminiOptions.Url}?key={_geminiOptions.ApiKey}";
                var response = await _httpClient.PostAsync(fullUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(jsonResponse);
                    
                    if (geminiResponse?.Candidates != null && geminiResponse.Candidates.Length > 0 &&
                        geminiResponse.Candidates[0]?.Content?.Parts != null && geminiResponse.Candidates[0].Content.Parts.Length > 0)
                    {
                        var messageResponse = geminiResponse.Candidates[0].Content.Parts[0].Text ?? "No response received";
                        return new ApiResponse<string>
                        {
                            Success = true,
                            Data = messageResponse
                        };
                    }
                }

                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid response from AI"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Gemini AI");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gọi AI"
                };
            }
        }

        private List<T> ApplySorting<T>(List<T> reports, string sortBy, bool ascending) where T : class
        {
            return sortBy.ToLower() switch
            {
                "studentname" => ascending 
                    ? reports.OrderBy(r => ((dynamic)r).StudentName).ToList()
                    : reports.OrderByDescending(r => ((dynamic)r).StudentName).ToList(),
                "totalnotes" => ascending 
                    ? reports.OrderBy(r => ((dynamic)r).TotalNotes).ToList()
                    : reports.OrderByDescending(r => ((dynamic)r).TotalNotes).ToList(),
                "totalpoints" => ascending 
                    ? reports.OrderBy(r => ((dynamic)r).TotalPoints).ToList()
                    : reports.OrderByDescending(r => ((dynamic)r).TotalPoints).ToList(),
                "createdat" or _ => ascending 
                    ? reports.OrderBy(r => ((dynamic)r).LatestCreatedAt).ToList()
                    : reports.OrderByDescending(r => ((dynamic)r).LatestCreatedAt).ToList()
            };
        }

        #endregion
    }
}
