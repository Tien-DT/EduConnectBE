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
    public class WeeklyParentNotificationService : IWeeklyParentNotificationService
    {
        private readonly ILessonReportRepository _lessonReportRepository;
        private readonly IStudentLessonNoteRepository _studentLessonNoteRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IParentRepository _parentRepository;
        private readonly INotificationService _notificationService;
        private readonly EduConnectDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly GeminiOption _geminiOptions;
        private readonly ILogger<WeeklyParentNotificationService> _logger;
        private readonly IMapper _mapper;

        public WeeklyParentNotificationService(
            ILessonReportRepository lessonReportRepository,
            IStudentLessonNoteRepository studentLessonNoteRepository,
            IStudentRepository studentRepository,
            IParentRepository parentRepository,
            INotificationService notificationService,
            EduConnectDbContext context,
            HttpClient httpClient,
            IOptions<GeminiOption> geminiOptions,
            ILogger<WeeklyParentNotificationService> logger,
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

        public async Task<ApiResponse<string>> SendWeeklyNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Bắt đầu gửi thông báo hàng tuần cho phụ huynh...");

                // Lấy ngày bắt đầu và kết thúc tuần (Thứ 2 đến Chủ nhật)
                var (startOfWeek, endOfWeek) = GetCurrentWeekRange();

                _logger.LogInformation($"Xử lý dữ liệu từ {startOfWeek:yyyy-MM-dd} đến {endOfWeek:yyyy-MM-dd}");

                // Lấy tất cả các lesson reports trong tuần có student notes
                var weeklyReports = await _lessonReportRepository.GetByDateRangeAsync(startOfWeek, endOfWeek);
                var reportsWithNotes = weeklyReports.Where(r => r.StudentLessonNotes?.Any() == true).ToList();

                if (!reportsWithNotes.Any())
                {
                    _logger.LogInformation("Không có phản ánh nào trong tuần này");
                    return new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Không có phản ánh nào trong tuần này để gửi thông báo",
                        Data = "No notifications needed"
                    };
                }

                // Nhóm theo parent ID
                var parentNotifications = await GroupNotificationsByParent(reportsWithNotes);

                var successCount = 0;
                var failCount = 0;

                foreach (var parentNotification in parentNotifications)
                {
                    try
                    {
                        var result = await SendNotificationToParent(parentNotification.Key, parentNotification.Value, startOfWeek, endOfWeek);
                        if (result.Success)
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                            _logger.LogWarning($"Gửi thông báo thất bại cho phụ huynh {parentNotification.Key}: {result.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        _logger.LogError(ex, $"Lỗi khi gửi thông báo cho phụ huynh {parentNotification.Key}");
                    }

                    // Tạm dừng 1 giây giữa các lần gửi để tránh rate limit
                    await Task.Delay(1000);
                }

                var message = $"Đã gửi thành công {successCount} thông báo, thất bại {failCount} thông báo";
                _logger.LogInformation(message);

                return new ApiResponse<string>
                {
                    Success = successCount > 0,
                    Message = message,
                    Data = $"Success: {successCount}, Failed: {failCount}"
                };
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

        public async Task<ApiResponse<string>> SendWeeklyNotificationToParentAsync(string parentId)
        {
            try
            {
                _logger.LogInformation($"Gửi thông báo hàng tuần cho phụ huynh: {parentId}");

                var (startOfWeek, endOfWeek) = GetCurrentWeekRange();

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

                // Lấy các lesson notes của tất cả học sinh trong tuần
                var allStudentNotes = new List<StudentLessonNote>();
                foreach (var studentId in studentIds)
                {
                    var reports = await _lessonReportRepository.GetByStudentIdAsync(studentId, startOfWeek, endOfWeek);
                    var studentNotes = reports.SelectMany(r => r.StudentLessonNotes ?? new List<StudentLessonNote>())
                                            .Where(n => n.StudentID == studentId && (n.IsDeleted == null || n.IsDeleted == false))
                                            .ToList();
                    allStudentNotes.AddRange(studentNotes);
                }

                if (!allStudentNotes.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Không có phản ánh nào cho các con em trong tuần này",
                        Data = "No notifications needed"
                    };
                }

                var result = await SendNotificationToParent(parentId, allStudentNotes, startOfWeek, endOfWeek);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi thông báo cho phụ huynh {parentId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi thông báo",
                    Data = ex.Message
                };
            }
        }

        public async Task<ApiResponse<object>> GetWeeklyStudentReportsAsync()
        {
            try
            {
                var (startOfWeek, endOfWeek) = GetCurrentWeekRange();

                var weeklyReports = await _lessonReportRepository.GetByDateRangeAsync(startOfWeek, endOfWeek);
                var reportsWithNotes = weeklyReports.Where(r => r.StudentLessonNotes?.Any() == true).ToList();

                var summary = reportsWithNotes
                    .SelectMany(r => r.StudentLessonNotes ?? new List<StudentLessonNote>())
                    .Where(n => n.IsDeleted == null || n.IsDeleted == false)
                    .GroupBy(n => n.StudentID)
                    .Select(g => new
                    {
                        StudentId = g.Key,
                        StudentName = g.First().Student?.FullName ?? "Unknown",
                        TotalNotes = g.Count(),
                        ViolationCount = g.Count(n => n.NoteType == "Violation"),
                        WarningCount = g.Count(n => n.NoteType == "Warning"),
                        BonusPointCount = g.Count(n => n.NoteType == "BonusPoint"),
                        GoodCount = g.Count(n => n.NoteType == "Good"),
                        TotalPoints = g.Sum(n => n.Points ?? 0),
                        Notes = g.Select(n => new
                        {
                            n.NoteType,
                            n.Points,
                            n.Description,
                            n.CreatedAt
                        }).OrderByDescending(n => n.CreatedAt).ToList()
                    })
                    .OrderByDescending(s => s.TotalNotes)
                    .ToList();

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Tóm tắt phản ánh từ {startOfWeek:dd/MM/yyyy} đến {endOfWeek:dd/MM/yyyy}",
                    Data = new
                    {
                        WeekRange = new { StartDate = startOfWeek, EndDate = endOfWeek },
                        TotalStudentsWithNotes = summary.Count,
                        StudentReports = summary
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tóm tắt phản ánh hàng tuần");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy tóm tắt phản ánh"
                };
            }
        }

        public async Task<ApiResponse<object>> GetWeeklyStudentReportAsync(string studentId)
        {
            try
            {
                var (startOfWeek, endOfWeek) = GetCurrentWeekRange();

                var reports = await _lessonReportRepository.GetByStudentIdAsync(studentId, startOfWeek, endOfWeek);
                var studentNotes = reports.SelectMany(r => r.StudentLessonNotes ?? new List<StudentLessonNote>())
                                        .Where(n => n.StudentID == studentId && (n.IsDeleted == null || n.IsDeleted == false))
                                        .OrderByDescending(n => n.CreatedAt)
                                        .ToList();

                var student = await _studentRepository.GetByIdAsync(studentId);

                var summary = new
                {
                    StudentId = studentId,
                    StudentName = student?.FullName ?? "Unknown",
                    WeekRange = new { StartDate = startOfWeek, EndDate = endOfWeek },
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
                            n.LessonReport?.LessonDate,
                            SubjectName = n.LessonReport?.TimetableSlot?.Subject?.SubjectName,
                            TeacherName = n.LessonReport?.TimetableSlot?.Teacher?.FullName
                        }
                    }).ToList()
                };

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Tóm tắt phản ánh của học sinh {student?.FullName} từ {startOfWeek:dd/MM/yyyy} đến {endOfWeek:dd/MM/yyyy}",
                    Data = summary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy tóm tắt phản ánh của học sinh {studentId}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy tóm tắt phản ánh của học sinh"
                };
            }
        }

        #region Private Methods

        private (DateTime StartOfWeek, DateTime EndOfWeek) GetCurrentWeekRange()
        {
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;
            
            // Tính ngày thứ 2 của tuần (DayOfWeek.Monday = 1)
            var startOfWeek = today.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
            var endOfWeek = startOfWeek.AddDays(6); // Chủ nhật

            return (startOfWeek, endOfWeek);
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

        private async Task<ApiResponse<string>> SendNotificationToParent(string parentId, List<StudentLessonNote> studentNotes, DateTime startDate, DateTime endDate)
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

                // Nhóm các notes theo học sinh
                var notesByStudent = studentNotes.GroupBy(n => n.StudentID).ToList();

                // Tạo nội dung thông báo bằng Gemini AI
                var notificationContent = await GenerateNotificationContent(notesByStudent, startDate, endDate);

                // Gửi thông báo qua Firebase
                var title = $"📋 Báo cáo hàng tuần ({startDate:dd/MM} - {endDate:dd/MM})";
                
                var data = new Dictionary<string, string>
                {
                    { "type", "weekly_report" },
                    { "parent_id", parentId },
                    { "week_start", startDate.ToString("yyyy-MM-dd") },
                    { "week_end", endDate.ToString("yyyy-MM-dd") },
                    { "total_notes", studentNotes.Count.ToString() }
                };

                var result = await _notificationService.SendNotificationToAllDevicesAsync(
                    parent.AccountID, 
                    title, 
                    notificationContent.Summary, 
                    data);

                if (result.Success)
                {
                    _logger.LogInformation($"Đã gửi thông báo thành công cho phụ huynh {parentId}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi thông báo cho phụ huynh {parentId}");
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
            DateTime endDate)
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

                // Gọi Gemini AI để tạo nội dung thông báo
                var prompt = $@"
Hãy tạo một thông báo ngắn gọn cho phụ huynh về tình hình học tập của con em họ trong tuần từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}.

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
                        else
                        {
                            return (CreateFallbackSummary(notesByStudent), geminiResponse.Data);
                        }
                    }
                    catch
                    {
                        // Fallback nếu không parse được JSON
                        return (CreateFallbackSummary(notesByStudent), geminiResponse.Data);
                    }
                }

                // Fallback content nếu Gemini không hoạt động
                return (CreateFallbackSummary(notesByStudent), CreateFallbackDetail(notesByStudent, startDate, endDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo nội dung thông báo bằng AI");
                return (CreateFallbackSummary(notesByStudent), CreateFallbackDetail(notesByStudent, startDate, endDate));
            }
        }

        private string CreateFallbackSummary(IEnumerable<IGrouping<string, StudentLessonNote>> notesByStudent)
        {
            var totalNotes = notesByStudent.Sum(g => g.Count());
            var violationCount = notesByStudent.Sum(g => g.Count(n => n.NoteType == "Violation"));
            var goodCount = notesByStudent.Sum(g => g.Count(n => n.NoteType == "Good" || n.NoteType == "BonusPoint"));

            if (violationCount > 0 && goodCount > 0)
            {
                return $"Con em có {goodCount} điểm tốt và {violationCount} lưu ý cần cải thiện trong tuần này";
            }
            else if (violationCount > 0)
            {
                return $"Con em có {violationCount} lưu ý cần cải thiện trong tuần này";
            }
            else if (goodCount > 0)
            {
                return $"Con em có {goodCount} điểm tốt trong tuần này! 🎉";
            }
            else
            {
                return $"Có {totalNotes} ghi chú về con em trong tuần này";
            }
        }

        private string CreateFallbackDetail(IEnumerable<IGrouping<string, StudentLessonNote>> notesByStudent, DateTime startDate, DateTime endDate)
        {
            var detail = $"📅 Báo cáo tuần từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}\n\n";

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
                    else
                    {
                        return new ApiResponse<string>
                        {
                            Success = false,
                            Message = "Invalid response format from Gemini API"
                        };
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Gemini API request failed: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Gemini AI");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gọi AI",
                    Data = ex.Message
                };
            }
        }

        #endregion
    }
}
