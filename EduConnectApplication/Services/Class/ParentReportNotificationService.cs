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
        private readonly IEnhancedNotificationService _enhancedNotificationService;
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
            IEnhancedNotificationService enhancedNotificationService,
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
            _enhancedNotificationService = enhancedNotificationService;
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

                var sortedReports = ApplySorting(studentReports, request.SortBy, request.Ascending);

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
                _logger.LogInformation($"🤖 Gemini AI đang phân tích dữ liệu {period} cho phụ huynh {parentId}...");

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
                
                // 🧠 Tạo comprehensive analysis data cho Gemini AI
                var comprehensiveData = await GenerateComprehensiveAnalysisData(notesByStudent, startDate, endDate, period);
                
                // 🤖 Gemini AI phân tích và tạo nội dung thông báo thông minh
                var aiNotificationContent = await GenerateNotificationContent(notesByStudent, startDate, endDate, period);

                // 🔮 Gemini AI phân tích xu hướng gia đình
                var familyTrendAnalysis = await GenerateFamilyTrendAnalysis(comprehensiveData);

                // 🎯 Tạo detailed content với AI analysis
                var detailedContent = await GenerateDetailedReportContent(notesByStudent, startDate, endDate, period);
                
                // Kết hợp AI analysis vào nội dung chi tiết
                var enhancedDetailContent = $@"{detailedContent.DetailContent}

🤖 **PHÂN TÍCH THÔNG MINH TỪ AI:**
{familyTrendAnalysis}

� **Summary:** {aiNotificationContent.Summary}

═══════════════════════════════
🕐 Báo cáo được tạo bằng AI vào: {DateTime.Now:dd/MM/yyyy HH:mm}";

                var title = $"🤖 AI Report {GetPeriodDisplayName(period)} ({startDate:dd/MM} - {endDate:dd/MM})";
                
                var data = new Dictionary<string, string>
                {
                    { "type", $"ai_{period}_report" },
                    { "parent_id", parentId },
                    { "period", period },
                    { "start_date", startDate.ToString("yyyy-MM-dd") },
                    { "end_date", endDate.ToString("yyyy-MM-dd") },
                    { "total_notes", studentNotes.Count.ToString() },
                    { "ai_enhanced", "true" },
                    { "analysis_version", "v2.0" }
                };

                // 🚀 Gửi thông báo với AI enhancement
                var result = await _enhancedNotificationService.CreateAndSendNotificationAsync(
                    parent.AccountID, 
                    title, 
                    enhancedDetailContent,
                    aiNotificationContent.Summary,       
                    $"ai_{period}_report", 
                    data);

                if (result.Success)
                {
                    _logger.LogInformation($"✅ Gemini AI đã phân tích và gửi thông báo {period} thành công cho phụ huynh {parentId}");
                }
                else
                {
                    _logger.LogWarning($"⚠️ Gemini AI analysis thành công nhưng gửi thông báo thất bại: {result.Message}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Lỗi khi Gemini AI phân tích và gửi thông báo {period} cho phụ huynh {parentId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi AI phân tích và gửi thông báo"
                };
            }
        }

        private async Task<object> GenerateComprehensiveAnalysisData(
            IEnumerable<IGrouping<string, StudentLessonNote>> notesByStudent, 
            DateTime startDate, 
            DateTime endDate, 
            string period)
        {
            try
            {
                var studentsAnalysis = new List<object>();

                foreach (var studentGroup in notesByStudent)
                {
                    var student = await _studentRepository.GetByIdAsync(studentGroup.Key);
                    var notes = studentGroup.ToList();

                    // Lấy thêm dữ liệu lịch sử để so sánh xu hướng
                    var historicalData = await GetHistoricalDataForComparison(studentGroup.Key, startDate, period);
                    
                    // Phân tích chi tiết từng học sinh
                    var studentAnalysis = new
                    {
                        StudentInfo = new
                        {
                            StudentId = studentGroup.Key,
                            Name = student?.FullName ?? "Unknown",
                            Class = student?.Class?.ClassName ?? "Unknown",
                            Grade = ExtractGradeFromClass(student?.Class?.ClassName ?? "Unknown")
                        },
                        
                        CurrentPeriodStats = new
                        {
                            TotalNotes = notes.Count,
                            ViolationCount = notes.Count(n => n.NoteType == "Violation"),
                            WarningCount = notes.Count(n => n.NoteType == "Warning"),
                            GoodCount = notes.Count(n => n.NoteType == "Good"),
                            BonusPointCount = notes.Count(n => n.NoteType == "BonusPoint"),
                            TotalPoints = notes.Sum(n => n.Points ?? 0),
                            AveragePointsPerNote = notes.Any() ? notes.Average(n => n.Points ?? 0) : 0
                        },

                        SubjectPerformance = notes.GroupBy(n => n.LessonReport?.TimetableSlot?.Subject?.SubjectName ?? "Unknown")
                            .Select(g => new
                            {
                                Subject = g.Key,
                                TotalNotes = g.Count(),
                                ViolationCount = g.Count(n => n.NoteType == "Violation"),
                                GoodCount = g.Count(n => n.NoteType == "Good" || n.NoteType == "BonusPoint"),
                                TotalPoints = g.Sum(n => n.Points ?? 0),
                                Teacher = g.FirstOrDefault()?.LessonReport?.TimetableSlot?.Teacher?.FullName
                            }).ToList(),

                        TimePattern = new
                        {
                            NotesPerWeek = notes.GroupBy(n => GetWeekOfYear(n.CreatedAt))
                                .Select(g => new { Week = g.Key, Count = g.Count() }).ToList(),
                            RecentTrend = AnalyzeRecentTrend(notes),
                            MostActiveDay = GetMostActiveDay(notes)
                        },

                        DetailedNotes = notes.OrderByDescending(n => n.CreatedAt).Select(n => new
                        {
                            Date = n.CreatedAt.ToString("dd/MM/yyyy"),
                            Type = n.NoteType,
                            Points = n.Points,
                            Description = n.Description,
                            Subject = n.LessonReport?.TimetableSlot?.Subject?.SubjectName,
                            Teacher = n.LessonReport?.TimetableSlot?.Teacher?.FullName,
                            Severity = ClassifyNoteSeverity(n)
                        }).ToList(),

                        HistoricalComparison = historicalData,
                        
                        RiskAssessment = new
                        {
                            RiskLevel = CalculateRiskLevel(notes),
                            KeyConcerns = IdentifyKeyConcerns(notes),
                            PositiveIndicators = IdentifyPositiveIndicators(notes)
                        }
                    };

                    studentsAnalysis.Add(studentAnalysis);
                }

                // Tạo context tổng quan cho toàn bộ gia đình
                var familyOverview = new
                {
                    Period = new
                    {
                        Type = period,
                        DisplayName = GetPeriodDisplayName(period),
                        StartDate = startDate.ToString("dd/MM/yyyy"),
                        EndDate = endDate.ToString("dd/MM/yyyy"),
                        TotalDays = (endDate - startDate).Days + 1
                    },

                    FamilyStats = new
                    {
                        TotalStudents = studentsAnalysis.Count,
                        TotalNotes = notesByStudent.Sum(g => g.Count()),
                        TotalViolations = notesByStudent.Sum(g => g.Count(n => n.NoteType == "Violation")),
                        TotalPositiveNotes = notesByStudent.Sum(g => g.Count(n => n.NoteType == "Good" || n.NoteType == "BonusPoint")),
                        OverallTrend = AnalyzeFamilyTrend(notesByStudent),
                        NeedsAttention = studentsAnalysis.Count(s => ((dynamic)s).RiskAssessment.RiskLevel == "High")
                    },

                    StudentsAnalysis = studentsAnalysis
                };

                return familyOverview;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo comprehensive analysis data");
                return CreateSimpleAnalysisData(notesByStudent, startDate, endDate, period);
            }
        }

        private async Task<object> GetHistoricalDataForComparison(string studentId, DateTime currentStart, string period)
        {
            try
            {
                // Lấy dữ liệu kỳ trước để so sánh
                var (prevStart, prevEnd) = GetPreviousPeriodRange(currentStart, period);
                
                var previousNotes = await _context.StudentLessonNotes
                    .Where(n => n.StudentID == studentId && 
                               n.CreatedAt >= prevStart && n.CreatedAt <= prevEnd &&
                               (n.IsDeleted == null || n.IsDeleted == false))
                    .ToListAsync();

                return new
                {
                    PreviousPeriod = new
                    {
                        StartDate = prevStart.ToString("dd/MM/yyyy"),
                        EndDate = prevEnd.ToString("dd/MM/yyyy"),
                        TotalNotes = previousNotes.Count,
                        ViolationCount = previousNotes.Count(n => n.NoteType == "Violation"),
                        GoodCount = previousNotes.Count(n => n.NoteType == "Good" || n.NoteType == "BonusPoint"),
                        TotalPoints = previousNotes.Sum(n => n.Points ?? 0)
                    },
                    Trend = new
                    {
                        NotesChange = previousNotes.Count == 0 ? "N/A" : "Improved", // Sẽ tính toán sau
                        PointsChange = previousNotes.Sum(n => n.Points ?? 0) == 0 ? "N/A" : "Improved"
                    }
                };
            }
            catch
            {
                return new { Message = "Không có dữ liệu lịch sử để so sánh" };
            }
        }

        private (DateTime Start, DateTime End) GetPreviousPeriodRange(DateTime currentStart, string period)
        {
            return period.ToLower() switch
            {
                "daily" => (currentStart.AddDays(-1), currentStart.AddSeconds(-1)),
                "weekly" => (currentStart.AddDays(-7), currentStart.AddSeconds(-1)),
                "monthly" => (currentStart.AddMonths(-1), currentStart.AddSeconds(-1)),
                _ => (currentStart.AddDays(-7), currentStart.AddSeconds(-1))
            };
        }

        private string ExtractGradeFromClass(string className)
        {
            if (string.IsNullOrEmpty(className)) return "Unknown";
            
            // Extract grade from class name (e.g., "10A1" -> "10")
            var match = System.Text.RegularExpressions.Regex.Match(className, @"\d+");
            return match.Success ? match.Value : "Unknown";
        }

        private int GetWeekOfYear(DateTime date)
        {
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        private string AnalyzeRecentTrend(List<StudentLessonNote> notes)
        {
            if (notes.Count < 3) return "Insufficient data";

            var recentNotes = notes.OrderByDescending(n => n.CreatedAt).Take(3).ToList();
            var violations = recentNotes.Count(n => n.NoteType == "Violation");
            var positives = recentNotes.Count(n => n.NoteType == "Good" || n.NoteType == "BonusPoint");

            if (violations > positives) return "Declining";
            if (positives > violations) return "Improving";
            return "Stable";
        }

        private string GetMostActiveDay(List<StudentLessonNote> notes)
        {
            if (!notes.Any()) return "None";

            var dayGroups = notes.GroupBy(n => n.CreatedAt.DayOfWeek)
                                .OrderByDescending(g => g.Count())
                                .FirstOrDefault();

            return dayGroups?.Key.ToString() ?? "None";
        }

        private string ClassifyNoteSeverity(StudentLessonNote note)
        {
            return note.NoteType switch
            {
                "Violation" => note.Points <= -5 ? "Critical" : "Moderate",
                "Warning" => "Low",
                "Good" => "Positive",
                "BonusPoint" => "Excellent",
                _ => "Normal"
            };
        }

        private string CalculateRiskLevel(List<StudentLessonNote> notes)
        {
            var violationCount = notes.Count(n => n.NoteType == "Violation");
            var totalNegativePoints = notes.Where(n => n.Points < 0).Sum(n => n.Points ?? 0);

            if (violationCount >= 5 || totalNegativePoints <= -20) return "High";
            if (violationCount >= 3 || totalNegativePoints <= -10) return "Medium";
            return "Low";
        }

        private List<string> IdentifyKeyConcerns(List<StudentLessonNote> notes)
        {
            var concerns = new List<string>();
            
            var violations = notes.Where(n => n.NoteType == "Violation").ToList();
            if (violations.Count >= 3)
            {
                concerns.Add($"Có {violations.Count} vi phạm cần chú ý");
            }

            var subjectViolations = violations.GroupBy(n => n.LessonReport?.TimetableSlot?.Subject?.SubjectName)
                                            .Where(g => g.Count() >= 2)
                                            .Select(g => g.Key)
                                            .ToList();

            if (subjectViolations.Any())
            {
                concerns.Add($"Vi phạm tập trung ở môn: {string.Join(", ", subjectViolations)}");
            }

            return concerns;
        }

        private List<string> IdentifyPositiveIndicators(List<StudentLessonNote> notes)
        {
            var positives = new List<string>();
            
            var goodNotes = notes.Where(n => n.NoteType == "Good" || n.NoteType == "BonusPoint").ToList();
            if (goodNotes.Count >= 3)
            {
                positives.Add($"Có {goodNotes.Count} điểm tích cực");
            }

            var strongSubjects = goodNotes.GroupBy(n => n.LessonReport?.TimetableSlot?.Subject?.SubjectName)
                                         .Where(g => g.Count() >= 2)
                                         .Select(g => g.Key)
                                         .ToList();

            if (strongSubjects.Any())
            {
                positives.Add($"Thể hiện tốt ở môn: {string.Join(", ", strongSubjects)}");
            }

            return positives;
        }

        private string AnalyzeFamilyTrend(IEnumerable<IGrouping<string, StudentLessonNote>> notesByStudent)
        {
            var allNotes = notesByStudent.SelectMany(g => g).ToList();
            var violations = allNotes.Count(n => n.NoteType == "Violation");
            var positives = allNotes.Count(n => n.NoteType == "Good" || n.NoteType == "BonusPoint");

            if (positives > violations * 2) return "Very Positive";
            if (positives > violations) return "Positive";
            if (violations > positives * 2) return "Concerning";
            return "Mixed";
        }

        private object CreateSimpleAnalysisData(IEnumerable<IGrouping<string, StudentLessonNote>> notesByStudent, 
            DateTime startDate, DateTime endDate, string period)
        {
            // Fallback simple data nếu comprehensive analysis thất bại
            return new
            {
                Period = GetPeriodDisplayName(period),
                DateRange = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}",
                Students = notesByStudent.Select(g => new
                {
                    StudentId = g.Key,
                    TotalNotes = g.Count(),
                    Notes = g.Select(n => new
                    {
                        Type = n.NoteType,
                        Description = n.Description,
                        Date = n.CreatedAt.ToString("dd/MM/yyyy")
                    }).ToList()
                }).ToList()
            };
        }

        private async Task<(string Summary, string DetailContent)> GenerateNotificationContent(
            IEnumerable<IGrouping<string, StudentLessonNote>> notesByStudent, 
            DateTime startDate, 
            DateTime endDate,
            string period)
        {
            try
            {
                // Tạo comprehensive data để Gemini AI phân tích
                var comprehensiveData = await GenerateComprehensiveAnalysisData(notesByStudent, startDate, endDate, period);
                
                var prompt = $@"
Bạn là một trợ lý AI chuyên phân tích giáo dục. Hãy phân tích dữ liệu học tập của học sinh và tạo thông báo thông minh cho phụ huynh.

🔍 **DỮ LIỆU PHÂN TÍCH:**
{System.Text.Json.JsonSerializer.Serialize(comprehensiveData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping })}

📋 **YÊU CẦU PHÂN TÍCH:**

1. **Đánh giá tổng quan:** Phân tích xu hướng học tập của từng học sinh
2. **So sánh tiến bộ:** So với các kỳ trước (nếu có dữ liệu)
3. **Nhận diện vấn đề:** Tìm ra các vấn đề cần can thiệp sớm
4. **Đề xuất cải thiện:** Đưa ra lời khuyên cụ thể cho phụ huynh
5. **Khen ngợi thành tựu:** Nhận diện và khen ngợi điểm mạnh

📱 **ĐỊNH DẠNG TRẢ VỀ JSON:**
{{
    ""summary"": ""Tóm tắt thông minh 1-2 câu với emoji phù hợp"",
    ""detail"": ""Nội dung chi tiết với phân tích chuyên sâu, xu hướng, và khuyến nghị cụ thể"",
    ""insights"": ""Những insight quan trọng từ dữ liệu"",
    ""recommendations"": ""Khuyến nghị cụ thể cho phụ huynh""
}}

🎯 **NGUYÊN TẮC:**
- Sử dụng ngôn ngữ tích cực, khuyến khích
- Đưa ra phân tích dựa trên dữ liệu cụ thể
- Cung cấp lời khuyên thực tế, khả thi
- Nhận diện xu hướng và pattern từ dữ liệu
- Đề xuất hành động cụ thể cho phụ huynh

Hãy phân tích sâu và tạo thông báo thông minh dựa trên dữ liệu thực tế!";

                var geminiResponse = await GenerateContentWithGeminiAsync(prompt);

                if (geminiResponse.Success && !string.IsNullOrEmpty(geminiResponse.Data))
                {
                    try
                    {
                        var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(geminiResponse.Data);
                        if (jsonResponse != null)
                        {
                            var summary = jsonResponse.GetValueOrDefault("summary", "Có thông tin mới về con em");
                            var detail = jsonResponse.GetValueOrDefault("detail", "Vui lòng kiểm tra ứng dụng để xem chi tiết");
                            
                            // Bổ sung insights và recommendations nếu có
                            var insights = jsonResponse.GetValueOrDefault("insights", "");
                            var recommendations = jsonResponse.GetValueOrDefault("recommendations", "");
                            
                            if (!string.IsNullOrEmpty(insights))
                            {
                                detail += $"\n\n🔍 **NHẬN XÉT CHUYÊN MÔN:**\n{insights}";
                            }
                            
                            if (!string.IsNullOrEmpty(recommendations))
                            {
                                detail += $"\n\n💡 **KHUYẾN NGHỊ:**\n{recommendations}";
                            }

                            _logger.LogInformation("Gemini AI đã phân tích và tạo thông báo thông minh thành công");
                            return (summary, detail);
                        }
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogWarning(parseEx, "Lỗi parse JSON response từ Gemini AI, sử dụng fallback");
                    }
                }

                _logger.LogWarning("Gemini AI response không hợp lệ, sử dụng fallback content");
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
                // Tăng cường prompt để Gemini AI hiểu rõ hơn về vai trò phân tích giáo dục
                var enhancedPrompt = $@"
🎓 **VAI TRÒ:** Bạn là một chuyên gia tâm lý giáo dục và phân tích học tập với kinh nghiệm 15 năm.

🎯 **NHIỆM VỤ:** Phân tích dữ liệu học tập và hành vi học sinh để tạo báo cáo thông minh cho phụ huynh.

📊 **DỮ LIỆU CẦN PHÂN TÍCH:**
{prompt}

🧠 **PHÂN TÍCH CHUYÊN SÂU:**
- Đánh giá xu hướng học tập và hành vi
- Nhận diện pattern và correlation trong dữ liệu  
- Dự đoán rủi ro và cơ hội cải thiện
- So sánh với tiêu chuẩn lứa tuổi
- Đưa ra assessment chính xác dựa trên evidence

⚡ **LƯU Ý QUAN TRỌNG:**
- Phân tích dựa trên DATA, không chỉ cảm tính
- Đưa ra insight có thể hành động được
- Cân bằng giữa khuyến khích và cảnh báo
- Sử dụng ngôn ngữ phù hợp với phụ huynh Việt Nam
- Đề xuất hành động cụ thể, khả thi

Hãy phân tích như một chuyên gia thực thụ!";

                var request = new GeminiRequest
                {
                    Contents = new[]
                    {
                        new GeminiContent
                        {
                            Role = "user",
                            Parts = new[] { new GeminiPart { Text = enhancedPrompt } }
                        }
                    },
                    GenerationConfig = new GenerationConfig
                    {
                        Temperature = 0.4, // Giảm temperature để có kết quả ổn định hơn
                        TopK = 1,
                        TopP = 0.8,
                        MaxOutputTokens = 2048 // Tăng token limit để có phân tích chi tiết hơn
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

        private async Task<(string Summary, string DetailContent)> GenerateDetailedReportContent(
            IEnumerable<IGrouping<string, StudentLessonNote>> notesByStudent, 
            DateTime startDate, 
            DateTime endDate,
            string period)
        {
            try
            {
                var detailText = $"📅 **Báo cáo {GetPeriodDisplayName(period)}** từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}\n\n";

                foreach (var studentGroup in notesByStudent)
                {
                    var student = await _studentRepository.GetByIdAsync(studentGroup.Key);
                    var notes = studentGroup.OrderByDescending(n => n.CreatedAt).ToList();

                    detailText += $"👤 **Học sinh: {student?.FullName ?? "Unknown"}**\n";
                    detailText += $"🏫 **Lớp: {student?.Class?.ClassName ?? "Unknown"}**\n\n";

                    var violationCount = notes.Count(n => n.NoteType == "Violation");
                    var warningCount = notes.Count(n => n.NoteType == "Warning");
                    var goodCount = notes.Count(n => n.NoteType == "Good");
                    var bonusCount = notes.Count(n => n.NoteType == "BonusPoint");
                    var totalPoints = notes.Sum(n => n.Points ?? 0);

                    detailText += "📊 **Tổng kết:**\n";
                    if (violationCount > 0) detailText += $"   ❌ Vi phạm: {violationCount} lần\n";
                    if (warningCount > 0) detailText += $"   ⚠️ Cảnh báo: {warningCount} lần\n";
                    if (goodCount > 0) detailText += $"   ✅ Khen ngợi: {goodCount} lần\n";
                    if (bonusCount > 0) detailText += $"   ⭐ Điểm thưởng: {bonusCount} lần\n";
                    detailText += $"   📊 Tổng điểm: {totalPoints}\n\n";

                    detailText += "📝 **Chi tiết các ghi chú:**\n";

                    if (violationCount > 0)
                    {
                        detailText += "🔴 **VI PHẠM:**\n";
                        var violations = notes.Where(n => n.NoteType == "Violation").ToList();
                        foreach (var note in violations)
                        {
                            detailText += $"   • **{note.CreatedAt:dd/MM/yyyy}** - {note.Description}\n";
                            detailText += $"     📚 Môn: {note.LessonReport?.TimetableSlot?.Subject?.SubjectName ?? "N/A"}\n";
                            detailText += $"     👨‍🏫 GV: {note.LessonReport?.TimetableSlot?.Teacher?.FullName ?? "N/A"}\n";
                            if (note.Points.HasValue) detailText += $"     📊 Điểm: {note.Points.Value}\n";
                            detailText += "\n";
                        }
                    }

                    if (warningCount > 0)
                    {
                        detailText += "🟡 **CẢNH BÁO:**\n";
                        var warnings = notes.Where(n => n.NoteType == "Warning").ToList();
                        foreach (var note in warnings)
                        {
                            detailText += $"   • **{note.CreatedAt:dd/MM/yyyy}** - {note.Description}\n";
                            detailText += $"     📚 Môn: {note.LessonReport?.TimetableSlot?.Subject?.SubjectName ?? "N/A"}\n";
                            detailText += $"     👨‍🏫 GV: {note.LessonReport?.TimetableSlot?.Teacher?.FullName ?? "N/A"}\n";
                            if (note.Points.HasValue) detailText += $"     📊 Điểm: {note.Points.Value}\n";
                            detailText += "\n";
                        }
                    }

                    if (goodCount > 0 || bonusCount > 0)
                    {
                        detailText += "🟢 **KHEN NGỢI & ĐIỂM THƯỞNG:**\n";
                        var positiveNotes = notes.Where(n => n.NoteType == "Good" || n.NoteType == "BonusPoint").ToList();
                        foreach (var note in positiveNotes)
                        {
                            detailText += $"   • **{note.CreatedAt:dd/MM/yyyy}** - {note.Description}\n";
                            detailText += $"     📚 Môn: {note.LessonReport?.TimetableSlot?.Subject?.SubjectName ?? "N/A"}\n";
                            detailText += $"     👨‍🏫 GV: {note.LessonReport?.TimetableSlot?.Teacher?.FullName ?? "N/A"}\n";
                            if (note.Points.HasValue) detailText += $"     📊 Điểm: {note.Points.Value}\n";
                            detailText += "\n";
                        }
                    }

                    detailText += "═══════════════════════════════\n\n";
                }

                detailText += $"📞 **Liên hệ nhà trường nếu cần hỗ trợ thêm.**\n";
                detailText += $"🕐 Báo cáo được tạo lúc: {DateTime.Now:dd/MM/yyyy HH:mm}";

                // Tạo summary ngắn
                var totalViolations = notesByStudent.Sum(g => g.Count(n => n.NoteType == "Violation"));
                var totalGood = notesByStudent.Sum(g => g.Count(n => n.NoteType == "Good" || n.NoteType == "BonusPoint"));
                var totalStudents = notesByStudent.Count();

                string summary;
                if (totalViolations > 0 && totalGood > 0)
                {
                    summary = $"Báo cáo {GetPeriodDisplayName(period)}: {totalStudents} con em có {totalGood} điểm tích cực và {totalViolations} lưu ý cần cải thiện";
                }
                else if (totalViolations > 0)
                {
                    summary = $"Báo cáo {GetPeriodDisplayName(period)}: {totalStudents} con em có {totalViolations} lưu ý cần cải thiện";
                }
                else
                {
                    summary = $"Báo cáo {GetPeriodDisplayName(period)}: {totalStudents} con em có {totalGood} điểm tích cực! 🎉";
                }

                return (summary, detailText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo nội dung báo cáo chi tiết");
                return (CreateFallbackSummary(notesByStudent, period), CreateFallbackDetail(notesByStudent, startDate, endDate, period));
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

        /// <summary>
        /// Gemini AI tự động đánh giá và tạo score cho học sinh
        /// </summary>
        private async Task<object> GenerateAIStudentAssessment(List<StudentLessonNote> notes, object studentInfo)
        {
            try
            {
                var assessmentPrompt = $@"
🤖 **VAI TRÒ:** AI Assessment System - Chuyên gia đánh giá học tập tự động

📊 **DỮ LIỆU HỌC SINH:**
{System.Text.Json.JsonSerializer.Serialize(studentInfo, new System.Text.Json.JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping })}

🎯 **YÊU CẦU ĐÁNH GIÁ:**
1. **Academic Performance Score** (0-100): Điểm học tập tổng thể
2. **Behavior Score** (0-100): Điểm hành vi, kỷ luật  
3. **Improvement Potential** (0-100): Tiềm năng cải thiện
4. **Risk Assessment** (Low/Medium/High): Mức độ rủi ro
5. **Key Strengths**: 3 điểm mạnh chính
6. **Areas for Improvement**: 3 khu vực cần cải thiện
7. **Specific Recommendations**: 5 khuyến nghị cụ thể cho phụ huynh

🧮 **CÔNG THỨC TÍNH ĐIỂM:**
- Positive notes: +5-10 points each
- Violations: -5-15 points each  
- Consistency bonus: +10 points
- Improvement trend: +5-20 points
- Subject balance: +10 points if good across subjects

📋 **ĐỊNH DẠNG JSON TRẢ VỀ:**
{{
    ""academicScore"": 85,
    ""behaviorScore"": 75,
    ""improvementPotential"": 90,
    ""riskLevel"": ""Medium"",
    ""overallGrade"": ""B+"",
    ""keyStrengths"": [""Strength 1"", ""Strength 2"", ""Strength 3""],
    ""areasForImprovement"": [""Area 1"", ""Area 2"", ""Area 3""],
    ""recommendations"": [""Rec 1"", ""Rec 2"", ""Rec 3"", ""Rec 4"", ""Rec 5""],
    ""summary"": ""Tóm tắt đánh giá tổng thể"",
    ""nextSteps"": ""Bước tiếp theo cần làm""
}}

💡 **CHÚ Ý:** Đánh giá dựa trên dữ liệu thực tế, cân bằng giữa động viên và thực tế.";

                var response = await GenerateContentWithGeminiAsync(assessmentPrompt);
                
                if (response.Success && !string.IsNullOrEmpty(response.Data))
                {
                    try
                    {
                        var assessment = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(response.Data);
                        return assessment ?? (object)new { error = "Failed to parse assessment" };
                    }
                    catch
                    {
                        return new { error = "Invalid JSON response from AI assessment" };
                    }
                }
                
                return new { error = "AI assessment service unavailable" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo AI assessment");
                return new { error = ex.Message };
            }
        }

        /// <summary>
        /// Gemini AI phân tích xu hướng và dự đoán cho gia đình
        /// </summary>
        private async Task<string> GenerateFamilyTrendAnalysis(object comprehensiveData)
        {
            try
            {
                var trendPrompt = $@"
🔮 **VAI TRÒ:** AI Predictive Analyst - Chuyên gia dự đoán xu hướng giáo dục

📈 **DỮ LIỆU GIA ĐÌNH:**
{System.Text.Json.JsonSerializer.Serialize(comprehensiveData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping })}

🎯 **PHÂN TÍCH XU HƯỚNG:**
1. **Dự đoán hiệu suất 30 ngày tới**
2. **Nhận diện risk factors sớm**  
3. **Cơ hội improvement opportunities**
4. **Pattern recognition trong hành vi học tập**
5. **Correlation analysis giữa các môn học**

🔍 **CÁC CHỈ SỐ QUAN TRỌNG:**
- Learning momentum (đà học tập)
- Behavioral consistency (tính nhất quán) 
- Subject performance balance (cân bằng môn học)
- Teacher feedback alignment (sự đồng thuận của giáo viên)
- Time management patterns (pattern quản lý thời gian)

📊 **TRẢ VỀ ĐỊNH DẠNG TEXT (không phải JSON):**
Một đoạn phân tích 200-300 từ về xu hướng gia đình, bao gồm:
- Đánh giá tình hình hiện tại
- Dự đoán xu hướng 1-2 tháng tới
- Warning signals cần chú ý
- Opportunities để tối ưu hóa
- Action items cụ thể cho gia đình

💫 **TONE:** Chuyên nghiệp, khuyến khích, dựa trên data, có thể hành động được.";

                var response = await GenerateContentWithGeminiAsync(trendPrompt);
                
                if (response.Success && !string.IsNullOrEmpty(response.Data))
                {
                    return response.Data;
                }
                
                return "Không thể tạo phân tích xu hướng lúc này. Vui lòng thử lại sau.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo family trend analysis");
                return $"Lỗi phân tích xu hướng: {ex.Message}";
            }
        }

        #endregion
    }
}
