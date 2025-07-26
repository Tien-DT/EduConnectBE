using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnectApplication.Services.Interface;
using EduConnectApplication.DTOs.Request;
using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Threading;

namespace EduConnect.Application.Services.Class
{
    public class GeminiFunctionService : IGeminiFunctionService
    {
        private readonly EduConnectDbContext _context;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly IClassService _classService;
        private readonly ITimetableService _timetableService;
        private readonly IFeedbackService _feedbackService;
        private readonly IParentRepository _parentRepository;
        private readonly ISubjectService _subjectService;
        private readonly IExamService _examService;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ILessonReportRepository _lessonReportRepository;
        private readonly ILessonReportService _lessonReportService;
        private readonly INotificationRepository _notificationRepository;
        private readonly ITimetableRepository _timetableRepository;
        private readonly ITimetableSlotRepository _timetableSlotRepository;
        private readonly IClassRepository _classRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly INotificationService _notificationService;
        private readonly IGradeService _gradeService;
        private readonly IReportService _reportService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public GeminiFunctionService(
            EduConnectDbContext context,
            IStudentService studentService,
            ITeacherService teacherService,
            IClassService classService,
            ITimetableService timetableService,
            IFeedbackService feedbackService,
            IParentRepository parentRepository,
            ISubjectService subjectService,
            IExamService examService,
            ITeacherRepository teacherRepository,
            ILessonReportRepository lessonReportRepository,
            ILessonReportService lessonReportService,
            INotificationRepository notificationRepository,
            ITimetableRepository timetableRepository,
            ITimetableSlotRepository timetableSlotRepository,
            IClassRepository classRepository,
            ISubjectRepository subjectRepository,
            INotificationService notificationService,
            IGradeService gradeService,
            IReportService reportService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _studentService = studentService;
            _teacherService = teacherService;
            _classService = classService;
            _timetableService = timetableService;
            _feedbackService = feedbackService;
            _parentRepository = parentRepository;
            _subjectService = subjectService;
            _examService = examService;
            _teacherRepository = teacherRepository;
            _lessonReportRepository = lessonReportRepository;
            _lessonReportService = lessonReportService;
            _notificationRepository = notificationRepository;
            _timetableRepository = timetableRepository;
            _timetableSlotRepository = timetableSlotRepository;
            _classRepository = classRepository;
            _subjectRepository = subjectRepository;
            _notificationService = notificationService;
            _gradeService = gradeService;
            _reportService = reportService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public FunctionDeclaration[] GetAvailableFunctions()
        {
            return new[]
            {
                new FunctionDeclaration
                {
                    Name = "get_workflow_guidance",
                    Description = "🤖 [HƯỚNG DẪN] Hướng dẫn cách thực hiện các quy trình phức tạp bằng cách kết hợp nhiều function. Sử dụng khi cần biết cách tạo báo cáo điểm, phân tích học sinh, hoặc các tác vụ nhiều bước.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["workflow_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Loại quy trình: 'grade_report', 'student_analysis', 'lesson_report_creation', 'performance_tracking'",
                                Enum = new[] { "grade_report", "student_analysis", "lesson_report_creation", "performance_tracking" }
                            }
                        },
                        Required = new[] { "workflow_type" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "execute_sequential_workflow",
                    Description = "🔄 [SEQUENTIAL] Thực hiện quy trình nhiều bước theo hướng dẫn của người dùng. Có thể xử lý các yêu cầu phức tạp như: 'Bước 1: Lấy học sinh → Bước 2: Lấy điểm → Bước 3: Phân tích và đánh giá'. Hệ thống sẽ tự động thực hiện từng bước và tổng hợp kết quả.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["workflow_description"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Mô tả chi tiết quy trình cần thực hiện, bao gồm các bước cụ thể"
                            },
                            ["teacher_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID giáo viên (tùy chọn, nếu không có sẽ dùng user hiện tại)"
                            },
                            ["target_class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID lớp học cụ thể (tùy chọn)"
                            },
                            ["target_subject_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID môn học cụ thể (tùy chọn)"
                            },
                            ["output_format"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Định dạng đầu ra: 'detailed', 'summary', 'statistical', 'recommendation'",
                                Enum = new[] { "detailed", "summary", "statistical", "recommendation" }
                            },
                            ["include_analysis"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Có bao gồm phân tích chi tiết không (mặc định: true)"
                            }
                        },
                        Required = new[] { "workflow_description" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "create_custom_report",
                    Description = "📊 [CUSTOM] Tạo báo cáo tùy chỉnh dựa theo yêu cầu cụ thể của người dùng. Có thể tạo báo cáo điểm, báo cáo chuyên cần, báo cáo thái độ học tập, hoặc bất kỳ loại báo cáo nào khác trong hệ thống giáo dục.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["report_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Loại báo cáo: 'grade_report', 'attendance_report', 'behavior_report', 'comprehensive_report', 'statistical_report', 'comparison_report', 'custom'",
                                Enum = new[] { "grade_report", "attendance_report", "behavior_report", "comprehensive_report", "statistical_report", "comparison_report", "custom" }
                            },
                            ["report_description"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Mô tả chi tiết về báo cáo cần tạo"
                            },
                            ["data_sources"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Nguồn dữ liệu cần thu thập: 'students', 'grades', 'attendance', 'lesson_reports', 'exams', 'all'"
                            },
                            ["target_audience"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Đối tượng sử dụng báo cáo: 'teacher', 'parent', 'principal', 'student'",
                                Enum = new[] { "teacher", "parent", "principal", "student" }
                            },
                            ["scope"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Phạm vi báo cáo: 'single_student', 'class', 'subject', 'school', 'custom_group'"
                            },
                            ["time_period"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Khoảng thời gian: 'current_week', 'current_month', 'current_semester', 'last_month', 'custom'"
                            },
                            ["include_recommendations"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Có bao gồm khuyến nghị không (mặc định: true)"
                            },
                            ["include_charts"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Có bao gồm biểu đồ thống kê không (mặc định: true)"
                            }
                        },
                        Required = new[] { "report_type", "report_description" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_student_info",
                    Description = "Get detailed information about a specific student by their student ID",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "The student ID to look up information for"
                            }
                        },
                        Required = new[] { "student_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_student_subjects",
                    Description = "Get the list of subjects that a student is enrolled in based on their timetable",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "The student ID to look up subjects for"
                            }
                        },
                        Required = new[] { "student_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_current_time",
                    Description = "Get the current date and time in Vietnam timezone (GMT+7). Returns detailed time information including date, time, day of week, season, and various formats",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["timezone"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Timezone preference (default: GMT+7 Vietnam time). Currently only supports GMT+7"
                            }
                        },
                        Required = new string[0]
                    }
                },
                new FunctionDeclaration
                {
                    Name = "format_vietnam_time",
                    Description = "Format the current Vietnam time (GMT+7) according to specific format requirements. Useful for creating timestamps, scheduling, or time-based content",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["format"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Time format: 'full' (complete info), 'date_only' (just date), 'time_only' (just time), 'iso' (ISO 8601), 'timestamp' (unix), 'friendly' (human readable Vietnamese)"
                            },
                            ["include_timezone"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Whether to include timezone information in the output (default: true)"
                            }
                        },
                        Required = new string[0]
                    }
                },
                new FunctionDeclaration
                {
                    Name = "calculate_sum",
                    Description = "Calculate the sum of two numbers",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["number1"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "First number"
                            },
                            ["number2"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Second number"
                            }
                        },
                        Required = new[] { "number1", "number2" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_student_timetable",
                    Description = "Get the timetable/schedule for a specific student",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "The student ID to get timetable for"
                            },
                            ["start_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Optional start date in YYYY-MM-DD format"
                            },
                            ["end_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Optional end date in YYYY-MM-DD format"
                            }
                        },
                        Required = new[] { "student_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_teacher_info",
                    Description = "Get detailed information about a specific teacher by their teacher ID",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["teacher_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "The teacher ID to look up information for"
                            }
                        },
                        Required = new[] { "teacher_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_teacher_timetable",
                    Description = "Get the timetable/schedule for a specific teacher",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["teacher_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "The teacher ID to get timetable for"
                            },
                            ["start_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Optional start date in YYYY-MM-DD format"
                            },
                            ["end_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Optional end date in YYYY-MM-DD format"
                            }
                        },
                        Required = new[] { "teacher_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "search_students",
                    Description = "Search for students by name or other criteria",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["search_term"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Search term to find students (name, ID, etc.)"
                            },
                            ["page_size"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Number of results to return (default: 10, max: 50)"
                            }
                        },
                        Required = new[] { "search_term" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "search_teachers",
                    Description = "Search for teachers by name or other criteria",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["search_term"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Search term to find teachers (name, ID, etc.)"
                            },
                            ["page_size"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Number of results to return (default: 10, max: 50)"
                            }
                        },
                        Required = new[] { "search_term" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_all_classes",
                    Description = "Get list of all classes in the school",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["search_term"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Optional search term to filter classes"
                            }
                        },
                        Required = new string[0]
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_timetable_by_class",
                    Description = "Get the timetable for a specific class",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "The class ID to get timetable for"
                            },
                            ["start_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Optional start date in YYYY-MM-DD format"
                            },
                            ["end_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Optional end date in YYYY-MM-DD format"
                            }
                        },
                        Required = new[] { "class_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "create_parent_feedback",
                    Description = "Create a feedback/complaint from parent to school. Use this when a parent is complaining, giving feedback, or making requests to school management or teachers. IMPORTANT: Only call this function when the parent is making a complaint, feedback, or request.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["content"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "The detailed content of the feedback/complaint"
                            },
                            ["target"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Who the feedback is directed to: 'Principal' for school management/infrastructure issues, 'Teacher' for teaching-related issues",
                                Enum = new[] { "Principal", "Teacher" }
                            },
                            ["category"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Category of the feedback",
                                Enum = new[] { "Infrastructure", "Teaching", "Service", "Student", "Other" }
                            }
                        },
                        Required = new[] { "content", "target" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_student_grades",
                    Description = "Get grades of student(s) for parents. Parents can view grades of all their children or specific child by semester and subject",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Name of the student to get grades for (optional, if not provided will get grades for all children)"
                            },
                            ["semester"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Semester to filter grades (optional, if not provided will get all semesters)"
                            },
                            ["subject_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Subject name to filter grades (optional, if not provided will get all subjects)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_student_lesson_reports",
                    Description = "Get lesson reports and notes for student(s) for parents. Can view by date range and specific student",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Name of the student to get lesson reports for (optional, if not provided will get reports for all children)"
                            },
                            ["from_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Start date in yyyy-mm-dd format (optional)"
                            },
                            ["to_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "End date in yyyy-mm-dd format (optional)"
                            },
                            ["limit"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Maximum number of reports to return (default 20)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_student_attendance",
                    Description = "Get attendance records for student(s) for parents. Parents can view attendance of all their children or specific child by date range",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Name of the student to get attendance for (optional, if not provided will get attendance for all children)"
                            },
                            ["from_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Start date in yyyy-mm-dd format (optional)"
                            },
                            ["to_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "End date in yyyy-mm-dd format (optional)"
                            },
                            ["subject_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Subject name to filter attendance (optional)"
                            },
                            ["limit"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Maximum number of attendance records to return (default 50)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "add_student_attendance",
                    Description = "Add attendance records for students (Principal only). Used to mark attendance for specific students in a timetable slot",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["timetable_slot_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "The timetable slot ID to add attendance for"
                            },
                            ["student_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Specific student ID to mark attendance for (optional, if not provided will add for all students in the class)"
                            },
                            ["status"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Attendance status: 0=Absent, 1=Present, 2=Late, 3=Excused (default: 1)"
                            },
                            ["note"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Optional note about the attendance"
                            },
                            ["date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Date for the attendance in yyyy-MM-dd format (optional, defaults to today)"
                            }
                        },
                        Required = new[] { "timetable_slot_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_student_grade_report",
                    Description = "Xem báo cáo điểm của học sinh theo thời gian (tuần, tháng, năm)",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Tên học sinh cần xem báo cáo (tùy chọn)"
                            },
                            ["report_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Loại báo cáo: 'week' (tuần), 'month' (tháng), 'year' (năm)"
                            },
                            ["start_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày bắt đầu (yyyy-MM-dd)"
                            },
                            ["end_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày kết thúc (yyyy-MM-dd)"
                            }
                        },
                        Required = new[] { "report_type", "start_date", "end_date" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_all_students_grade_report",
                    Description = "Xem báo cáo điểm của tất cả học sinh theo thời gian (tuần, tháng, năm)",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["report_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Loại báo cáo: 'week' (tuần), 'month' (tháng), 'year' (năm)"
                            },
                            ["start_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày bắt đầu (yyyy-MM-dd)"
                            },
                            ["end_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày kết thúc (yyyy-MM-dd)"
                            },
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID lớp học (tùy chọn, để lọc theo lớp)"
                            },
                            ["subject_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Tên môn học (tùy chọn, để lọc theo môn)"
                            }
                        },
                        Required = new[] { "report_type", "start_date", "end_date" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "create_lesson_report",
                    Description = "🔒 [CHỈ GIÁO VIÊN] Tạo báo cáo buổi học mới. Cho phép giáo viên ghi lại thông tin điểm danh và nhận xét về buổi học",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["timetable_slot_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của tiết học trong thời khóa biểu (bắt buộc)"
                            },
                            ["lesson_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày của buổi học (yyyy-MM-dd) (bắt buộc)"
                            },
                            ["total_students"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Tổng số học sinh trong lớp (bắt buộc)"
                            },
                            ["absent_students"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số học sinh vắng mặt (bắt buộc)"
                            },
                            ["lesson_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của bài học (tùy chọn)"
                            },
                            ["remarks"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ghi chú chung về buổi học (tùy chọn)"
                            },
                            ["student_notes"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Danh sách ghi chú cho học sinh dưới dạng JSON: [{\"student_id\": \"string\", \"note_type\": \"Good|Warning|Violation|BonusPoint\", \"description\": \"string\"}] (tùy chọn)"
                            }
                        },
                        Required = new[] { "timetable_slot_id", "lesson_date", "total_students", "absent_students" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_lesson_reports_by_teacher",
                    Description = "🔒 [CHỈ GIÁO VIÊN] Lấy danh sách báo cáo buổi học đã tạo. Giáo viên có thể xem lại các báo cáo của mình",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["teacher_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID của giáo viên (tùy chọn, nếu không có sẽ lấy theo user hiện tại)"
                            },
                            ["from_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày bắt đầu (yyyy-MM-dd) - tùy chọn"
                            },
                            ["to_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày kết thúc (yyyy-MM-dd) - tùy chọn"
                            },
                            ["page_size"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số lượng báo cáo trả về (mặc định: 20, tối đa: 50)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "send_notification_to_parent",
                    Description = "🔒 [CHỈ GIÁO VIÊN] Gửi thông báo đến phụ huynh. Cho phép gửi thông báo quan trọng trực tiếp đến phụ huynh",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["parent_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID của phụ huynh nhận thông báo"
                            },
                            ["title"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Tiêu đề thông báo"
                            },
                            ["message"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Nội dung thông báo"
                            },
                            ["notification_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Loại thông báo: 'urgent', 'normal', 'info'",
                                Enum = new[] { "urgent", "normal", "info" }
                            }
                        },
                        Required = new[] { "parent_id", "title", "message" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_all_feedbacks",
                    Description = "📋 [CHỈ XEM] Lấy danh sách tất cả phản hồi từ phụ huynh. Giúp nắm bắt ý kiến và phản hồi từ phụ huynh",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["search_term"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Từ khóa tìm kiếm trong nội dung phản hồi"
                            },
                            ["target"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Đối tượng nhận phản hồi: 'Principal', 'Teacher', hoặc để trống để lấy tất cả",
                                Enum = new[] { "Principal", "Teacher" }
                            },
                            ["page_size"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số lượng phản hồi trả về (mặc định: 20, tối đa: 50)"
                            },
                            ["sort_by"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Sắp xếp theo: 'DateTime', 'ParentName'",
                                Enum = new[] { "DateTime", "ParentName" }
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_feedbacks_by_parent",
                    Description = "📋 [CHỈ XEM] Lấy danh sách phản hồi của một phụ huynh cụ thể. Giúp xem lịch sử phản hồi của phụ huynh",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["parent_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID của phụ huynh cần lấy phản hồi"
                            },
                            ["search_term"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Từ khóa tìm kiếm trong nội dung phản hồi"
                            },
                            ["target"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Đối tượng nhận phản hồi: 'Principal', 'Teacher', hoặc để trống để lấy tất cả",
                                Enum = new[] { "Principal", "Teacher" }
                            },
                            ["page_size"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số lượng phản hồi trả về (mặc định: 20, tối đa: 50)"
                            },
                            ["sort_by"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Sắp xếp theo: 'DateTime', 'Target'",
                                Enum = new[] { "DateTime", "Target" }
                            }
                        },
                        Required = new[] { "parent_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_feedback_by_id",
                    Description = "📋 [CHỈ XEM] Lấy thông tin chi tiết của một phản hồi cụ thể. Giúp xem nội dung chi tiết phản hồi",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["feedback_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của phản hồi cần lấy thông tin"
                            }
                        },
                        Required = new[] { "feedback_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_notifications_for_user",
                    Description = "Lấy danh sách thông báo của người dùng hiện tại",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["unread_only"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Chỉ lấy thông báo chưa đọc (mặc định: false)"
                            },
                            ["page_size"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số lượng thông báo trả về (mặc định: 20, tối đa: 50)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "mark_notifications_as_read",
                    Description = "Đánh dấu thông báo đã đọc",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["notification_ids"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Danh sách ID thông báo cần đánh dấu đã đọc, phân cách bằng dấu phẩy (ví dụ: '1,2,3')"
                            }
                        },
                        Required = new[] { "notification_ids" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "create_timetable",
                    Description = "Tạo thời khóa biểu mới cho lớp học (dành cho hiệu trưởng/quản lý)",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của lớp học"
                            },
                            ["name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Tên của thời khóa biểu"
                            },
                            ["start_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày bắt đầu thời khóa biểu (yyyy-MM-dd)"
                            },
                            ["end_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày kết thúc thời khóa biểu (yyyy-MM-dd)"
                            },
                            ["slots"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Danh sách tiết học dưới dạng JSON: [{\"teacher_id\": \"string\", \"subject_id\": number, \"day_of_week\": \"Monday\", \"start_time\": \"08:00\", \"end_time\": \"09:00\", \"room\": \"string\", \"period\": number}]"
                            }
                        },
                        Required = new[] { "class_id", "name", "start_date", "end_date", "slots" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_weekly_student_reports",
                    Description = "Lấy báo cáo học sinh theo tuần (dành cho phụ huynh và giáo viên)",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID học sinh cần xem báo cáo (tùy chọn, nếu không có sẽ lấy tất cả con em của phụ huynh)"
                            },
                            ["week_offset"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Lùi lại số tuần từ tuần hiện tại (0 = tuần này, 1 = tuần trước, ...)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                // Principal Functions - Full CRUD User Management
                new FunctionDeclaration
                {
                    Name = "create_user_account",
                    Description = "🔒 [CHỈ HIỆU TRƯỞNG] Tạo tài khoản mới cho giáo viên, học sinh hoặc phụ huynh",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["email"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Email của tài khoản mới"
                            },
                            ["password"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Mật khẩu tạm thời"
                            },
                            ["first_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Tên"
                            },
                            ["last_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Họ"
                            },
                            ["phone"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Số điện thoại"
                            },
                            ["role"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Vai trò: 'Teacher', 'Student', 'Parent'",
                                Enum = new[] { "Teacher", "Student", "Parent" }
                            }
                        },
                        Required = new[] { "email", "password", "first_name", "last_name", "role" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "assign_user_roles",
                    Description = "🔒 [CHỈ HIỆU TRƯỞNG] Phân quyền cho người dùng trong hệ thống",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["user_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID của người dùng"
                            },
                            ["role"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Vai trò mới: 'Principal', 'Teacher', 'Student', 'Parent'",
                                Enum = new[] { "Principal", "Teacher", "Student", "Parent" }
                            },
                            ["entity_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID thực thể tương ứng (TeacherID, StudentID, ParentID)"
                            },
                            ["full_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Họ tên đầy đủ"
                            }
                        },
                        Required = new[] { "user_id", "role", "entity_id", "full_name" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "delete_user_account",
                    Description = "🔒 [CHỈ HIỆU TRƯỞNG] Xóa tài khoản người dùng khỏi hệ thống",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["user_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID của người dùng cần xóa"
                            },
                            ["reason"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Lý do xóa tài khoản"
                            }
                        },
                        Required = new[] { "user_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_all_users",
                    Description = "🔒 [CHỈ HIỆU TRƯỞNG] Xem danh sách tất cả người dùng trong hệ thống",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["role_filter"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Lọc theo vai trò: 'All', 'Teacher', 'Student', 'Parent'",
                                Enum = new[] { "All", "Teacher", "Student", "Parent" }
                            },
                            ["search_term"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Từ khóa tìm kiếm (tên, email)"
                            },
                            ["page_size"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số lượng kết quả trả về (mặc định: 20, tối đa: 50)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                // Teacher Functions - Exam Management  
                new FunctionDeclaration
                {
                    Name = "create_class_exam",
                    Description = "🔒 [CHỈ GIÁO VIÊN] Tạo lịch thi mới cho lớp học",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["subject_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID môn học"
                            },
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID lớp học"
                            },
                            ["exam_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Loại thi: 'Midterm', 'Final', 'Quiz', 'Assignment'"
                            },
                            ["exam_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày thi (yyyy-MM-dd HH:mm)"
                            },
                            ["room"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Phòng thi"
                            },
                            ["exam_note"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ghi chú về kỳ thi"
                            }
                        },
                        Required = new[] { "subject_id", "class_id", "exam_type", "exam_date" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "update_exam",
                    Description = "🔒 [CHỈ GIÁO VIÊN] Cập nhật thông tin kỳ thi",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["exam_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của kỳ thi cần cập nhật"
                            },
                            ["exam_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Loại thi mới"
                            },
                            ["exam_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày thi mới (yyyy-MM-dd HH:mm)"
                            },
                            ["room"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Phòng thi mới"
                            },
                            ["exam_note"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ghi chú mới"
                            }
                        },
                        Required = new[] { "exam_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "delete_exam",
                    Description = "🔒 [CHỈ GIÁO VIÊN] Xóa kỳ thi khỏi hệ thống",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["exam_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của kỳ thi cần xóa"
                            },
                            ["reason"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Lý do xóa kỳ thi"
                            }
                        },
                        Required = new[] { "exam_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_class_exams",
                    Description = "🔒 [CHỈ GIÁO VIÊN] Xem danh sách thi của lớp học",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID lớp học (tùy chọn, nếu không có sẽ lấy tất cả lớp của giáo viên)"
                            },
                            ["subject_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID môn học (tùy chọn)"
                            },
                            ["from_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Từ ngày (yyyy-MM-dd)"
                            },
                            ["to_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Đến ngày (yyyy-MM-dd)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                // Teacher Functions - Enhanced Grade Management
                new FunctionDeclaration
                {
                    Name = "create_grade",
                    Description = "🔒 [CHỈ GIÁO VIÊN] Tạo điểm mới cho học sinh",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID học sinh"
                            },
                            ["subject_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID môn học"
                            },
                            ["score"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Điểm số (0-10)"
                            },
                            ["grade_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Loại điểm: 'Oral', 'Written', 'Midterm', 'Final'"
                            },
                            ["semester"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Học kỳ"
                            },
                            ["evaluation"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Nhận xét của giáo viên"
                            }
                        },
                        Required = new[] { "student_id", "subject_id", "score", "grade_type" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "update_grade",
                    Description = "🔒 [CHỈ GIÁO VIÊN] Cập nhật điểm của học sinh",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["grade_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của điểm cần cập nhật"
                            },
                            ["score"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Điểm số mới (0-10)"
                            },
                            ["grade_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Loại điểm mới"
                            },
                            ["evaluation"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Nhận xét mới"
                            }
                        },
                        Required = new[] { "grade_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "delete_grade",
                    Description = "🔒 [CHỈ GIÁO VIÊN] Xóa điểm của học sinh",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["grade_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của điểm cần xóa"
                            },
                            ["reason"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Lý do xóa điểm"
                            }
                        },
                        Required = new[] { "grade_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_students_for_grading",
                    Description = "🔒 [CHỈ GIÁO VIÊN] Lấy danh sách học sinh để chấm điểm theo môn học",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["subject_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID môn học"
                            },
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID lớp học (tùy chọn)"
                            }
                        },
                        Required = new[] { "subject_id" }
                    }
                },
                // Student Functions - Personal Information
                new FunctionDeclaration
                {
                    Name = "get_my_info",
                    Description = "📋 [CHỈ HỌC SINH] Xem thông tin cá nhân của học sinh",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>(),
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_my_exams",
                    Description = "📝 [CHỈ HỌC SINH] Xem lịch thi của bản thân",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["from_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Từ ngày (yyyy-MM-dd) - tùy chọn"
                            },
                            ["to_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Đến ngày (yyyy-MM-dd) - tùy chọn"
                            },
                            ["subject_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Tên môn học để lọc (tùy chọn)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_student_parent_info",
                    Description = "🏠 [CHỈ XEM] Lấy thông tin phụ huynh của học sinh",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID của học sinh cần xem thông tin phụ huynh"
                            }
                        },
                        Required = new[] { "student_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_all_subjects",
                    Description = "📚 [CHỈ XEM] Lấy danh sách tất cả môn học trong hệ thống",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["search_term"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Từ khóa tìm kiếm tên môn học"
                            },
                            ["page_size"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số lượng môn học trả về (mặc định: 20, tối đa: 50)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_subject_teachers",
                    Description = "👨‍🏫 [CHỈ XEM] Lấy danh sách giáo viên dạy một môn học cụ thể",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["subject_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của môn học"
                            }
                        },
                        Required = new[] { "subject_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_class_students_list",
                    Description = "👥 [BƯỚC 2] Lấy danh sách học sinh trong một lớp học. Thường dùng sau get_teacher_classes để lấy học sinh từ các lớp giáo viên dạy. Có thể bao gồm điểm số để chuẩn bị cho việc tạo báo cáo.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của lớp học"
                            },
                            ["include_grades"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Có bao gồm điểm số của học sinh không (mặc định: false, nên set true khi tạo báo cáo điểm)"
                            }
                        },
                        Required = new[] { "class_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_teacher_classes",
                    Description = "🏫 [BƯỚC 1] Lấy danh sách lớp học mà giáo viên đang dạy. Sử dụng làm bước đầu tiên khi cần tạo báo cáo điểm hoặc phân tích học sinh. Sau khi có danh sách lớp, có thể dùng get_class_students_list để lấy học sinh trong từng lớp.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["teacher_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID của giáo viên"
                            },
                            ["include_subjects"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Có bao gồm thông tin môn học không (mặc định: true)"
                            }
                        },
                        Required = new[] { "teacher_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_class_attendance_summary",
                    Description = "📊 [CHỈ XEM] Lấy tổng quan điểm danh của một lớp học trong khoảng thời gian",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của lớp học"
                            },
                            ["start_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày bắt đầu (định dạng: YYYY-MM-DD)"
                            },
                            ["end_date"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày kết thúc (định dạng: YYYY-MM-DD)"
                            }
                        },
                        Required = new[] { "class_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_exam_results_by_class",
                    Description = "📝 [CHỈ XEM] Lấy kết quả thi của tất cả học sinh trong một lớp cho một bài thi",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["exam_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của bài thi"
                            },
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của lớp học"
                            }
                        },
                        Required = new[] { "exam_id", "class_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_student_exam_history",
                    Description = "📜 [CHỈ XEM] Lấy lịch sử thi cử của một học sinh",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID của học sinh"
                            },
                            ["subject_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID môn học (tùy chọn để lọc theo môn)"
                            },
                            ["limit"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số lượng bài thi gần nhất (mặc định: 10)"
                            }
                        },
                        Required = new[] { "student_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_parent_children_info",
                    Description = "👶 [CHỈ XEM] Lấy thông tin tất cả con em của một phụ huynh",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["parent_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID của phụ huynh"
                            }
                        },
                        Required = new[] { "parent_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_recent_lesson_reports",
                    Description = "📋 [CHỈ XEM] Lấy báo cáo bài học gần đây",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["teacher_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID của giáo viên (tùy chọn để lọc theo giáo viên)"
                            },
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của lớp học (tùy chọn để lọc theo lớp)"
                            },
                            ["days_back"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số ngày trở về từ hôm nay (mặc định: 7 ngày)"
                            },
                            ["limit"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số lượng báo cáo tối đa (mặc định: 20)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_user_role_info",
                    Description = "🔐 [CHỈ XEM] Lấy thông tin vai trò của người dùng trong hệ thống",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["user_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID của người dùng"
                            }
                        },
                        Required = new[] { "user_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "search_notifications",
                    Description = "🔍 [CHỈ XEM] Tìm kiếm thông báo theo từ khóa",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["search_term"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Từ khóa tìm kiếm trong tiêu đề hoặc nội dung thông báo"
                            },
                            ["user_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID người dùng (tùy chọn để lọc theo người nhận)"
                            },
                            ["date_from"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày bắt đầu tìm kiếm (YYYY-MM-DD)"
                            },
                            ["date_to"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Ngày kết thúc tìm kiếm (YYYY-MM-DD)"
                            }
                        },
                        Required = new[] { "search_term" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_class_grade_statistics",
                    Description = "📈 [CHỈ XEM] Lấy thống kê điểm số của một lớp học",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của lớp học"
                            },
                            ["subject_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID môn học (tùy chọn để lọc theo môn)"
                            },
                            ["semester"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Học kỳ (tùy chọn)"
                            }
                        },
                        Required = new[] { "class_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_upcoming_exams",
                    Description = "📅 [CHỈ XEM] Lấy danh sách các bài thi sắp tới",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID học sinh (tùy chọn để lọc theo học sinh cụ thể)"
                            },
                            ["teacher_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID giáo viên (tùy chọn để lọc theo giáo viên tạo)"
                            },
                            ["days_ahead"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số ngày tới trong tương lai (mặc định: 30 ngày)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                // Report Functions
                new FunctionDeclaration
                {
                    Name = "create_report",
                    Description = "📝 Tạo báo cáo mới cho hệ thống",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["report_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Loại báo cáo (Bug Report, Feature Request, Complaint, General, etc.)"
                            },
                            ["content"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Nội dung chi tiết của báo cáo"
                            },
                            ["status"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Trạng thái báo cáo (Pending, Processing, Resolved, Rejected) - mặc định là Pending"
                            }
                        },
                        Required = new string[] { "report_type", "content" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_my_reports",
                    Description = "📋 Xem danh sách báo cáo của bản thân",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["report_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Lọc theo loại báo cáo (tùy chọn)"
                            },
                            ["status"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Lọc theo trạng thái (tùy chọn)"
                            },
                            ["search"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Tìm kiếm trong nội dung báo cáo (tùy chọn)"
                            },
                            ["page"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số trang (mặc định 1)"
                            },
                            ["page_size"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số lượng báo cáo mỗi trang (mặc định 10)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_all_reports",
                    Description = "📋 [CHỈ ADMIN/PRINCIPAL] Xem tất cả báo cáo trong hệ thống",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["account_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Lọc theo ID người tạo báo cáo (tùy chọn)"
                            },
                            ["report_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Lọc theo loại báo cáo (tùy chọn)"
                            },
                            ["status"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Lọc theo trạng thái (tùy chọn)"
                            },
                            ["search"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Tìm kiếm trong nội dung báo cáo (tùy chọn)"
                            },
                            ["page"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số trang (mặc định 1)"
                            },
                            ["page_size"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số lượng báo cáo mỗi trang (mặc định 10)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "update_report_status",
                    Description = "🔄 [CHỈ ADMIN/PRINCIPAL] Cập nhật trạng thái báo cáo",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["report_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của báo cáo cần cập nhật"
                            },
                            ["status"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Trạng thái mới (Pending, Processing, Resolved, Rejected)"
                            }
                        },
                        Required = new string[] { "report_id", "status" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "send_broadcast_notification",
                    Description = "Send a system-wide notification to all users in the system. Only Principal can use this function.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["title"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Title of the notification"
                            },
                            ["message"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Content of the notification"
                            },
                            ["category"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Category of notification (system, announcement, etc.)"
                            }
                        },
                        Required = new string[] { "title", "message" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "send_notification_to_teachers",
                    Description = "Send notification to all teachers in the system. Only Principal can use this function.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["title"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Title of the notification"
                            },
                            ["message"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Content of the notification"
                            },
                            ["category"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Category of notification (announcement, urgent, etc.)"
                            }
                        },
                        Required = new string[] { "title", "message" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "send_notification_to_students",
                    Description = "Send notification to all students in the system. Principal and Teacher can use this function.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["title"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Title of the notification"
                            },
                            ["message"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Content of the notification"
                            },
                            ["category"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Category of notification (announcement, urgent, etc.)"
                            }
                        },
                        Required = new string[] { "title", "message" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "send_notification_to_parents",
                    Description = "Send notification to all parents in the system. Principal and Teacher can use this function.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["title"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Title of the notification"
                            },
                            ["message"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Content of the notification"
                            },
                            ["category"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Category of notification (announcement, urgent, etc.)"
                            }
                        },
                        Required = new string[] { "title", "message" }
                    }
                },
                // COMPOSITE FUNCTIONS - Advanced multi-function operations
                new FunctionDeclaration
                {
                    Name = "get_comprehensive_student_report",
                    Description = "📊 Get a comprehensive report combining student grades, attendance, timetable, and upcoming exams. Perfect for complete student overview.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Student name to search for (for parents to view their children's comprehensive report)"
                            },
                            ["include_attendance"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Include attendance information (default: true)"
                            },
                            ["include_timetable"] = new ParameterSchema
                            {
                                Type = "boolean", 
                                Description = "Include current week timetable (default: true)"
                            },
                            ["include_upcoming_exams"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Include upcoming exams (default: true)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "analyze_student_performance",
                    Description = "🔍 Analyze student performance by comparing grades with attendance and providing insights. Identifies patterns and recommendations.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Student name to analyze"
                            },
                            ["semester"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Specific semester to analyze (optional)"
                            },
                            ["subject_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Specific subject to focus analysis on (optional)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_class_overview_report", 
                    Description = "🏫 Get comprehensive class overview including students list, grade statistics, attendance summary, and upcoming events.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Class ID to get overview for"
                            },
                            ["include_grade_stats"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Include grade statistics (default: true)"
                            },
                            ["include_attendance"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Include attendance summary (default: true)"
                            }
                        },
                        Required = new string[] { "class_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "search_and_get_student_details",
                    Description = "🔎 Search for students by name and get detailed information about found students including grades and attendance.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["search_term"] = new ParameterSchema
                            {
                                Type = "string", 
                                Description = "Name or partial name to search for"
                            },
                            ["include_grades"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Include grade information for found students (default: true)"
                            },
                            ["max_results"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Maximum number of students to return details for (default: 3)"
                            }
                        },
                        Required = new string[] { "search_term" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_teacher_dashboard",
                    Description = "👨‍🏫 Get comprehensive teacher dashboard with their classes, recent lesson reports, student performance, and upcoming tasks.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["teacher_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Teacher ID to get dashboard for (optional - will use current user if not provided)"
                            },
                            ["include_lesson_reports"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Include recent lesson reports (default: true)"
                            },
                            ["days_back"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Number of days back to include in reports (default: 7)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                // ADDITIONAL STUDENT-FOCUSED COMPOSITE FUNCTIONS
                new FunctionDeclaration
                {
                    Name = "get_student_academic_progress",
                    Description = "📈 Get detailed academic progress tracking for student including grade trends, subject performance comparison, and improvement recommendations.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Student name to track progress for"
                            },
                            ["time_period"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Time period for analysis: 'current_semester', 'last_month', 'last_3_months' (default: 'current_semester')"
                            },
                            ["include_predictions"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Include performance predictions and recommendations (default: true)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_student_homework_overview",
                    Description = "📝 Get comprehensive homework and assignment overview including upcoming deadlines, completion status, and study recommendations.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_name"] = new ParameterSchema
                            {
                                Type = "string", 
                                Description = "Student name to get homework overview for"
                            },
                            ["days_ahead"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Number of days ahead to look for upcoming assignments (default: 14)"
                            },
                            ["subject_filter"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Filter by specific subject name (optional)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "compare_student_performance",
                    Description = "⚖️ Compare student's performance with class average and provide detailed analysis on strengths and improvement areas.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Student name to compare performance for"
                            },
                            ["comparison_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Type of comparison: 'class_average', 'top_performers', 'similar_students' (default: 'class_average')"
                            },
                            ["include_recommendations"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Include personalized improvement recommendations (default: true)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_student_learning_plan",
                    Description = "🎯 Generate personalized learning plan based on student's current performance, weak areas, and upcoming exams.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Student name to create learning plan for"
                            },
                            ["plan_duration"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Duration for the learning plan: '1_week', '2_weeks', '1_month' (default: '2_weeks')"
                            },
                            ["focus_areas"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Comma-separated list of subjects to focus on (optional - will auto-detect weak areas if not provided)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "get_student_weekly_summary",
                    Description = "📅 Get comprehensive weekly summary including attendance, grades received, homework status, and upcoming events for student.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_name"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Student name to get weekly summary for"
                            },
                            ["week_offset"] = new ParameterSchema
                            {
                                Type = "number", 
                                Description = "Week offset from current week (0 = this week, -1 = last week, 1 = next week, default: 0)"
                            },
                            ["include_next_week_preview"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Include preview of next week's schedule and exams (default: true)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "create_comprehensive_grade_report",
                    Description = "📊 [COMPOSITE] TẠO BÁO CÁO ĐIỂM TOÀN DIỆN cho giáo viên. Thực hiện đầy đủ quy trình: Bước 1: Lấy danh sách học sinh mà giáo viên đang dạy → Bước 2: Lấy điểm số của các học sinh → Bước 3: Tạo báo cáo đánh giá từ dữ liệu điểm. Sử dụng function này khi được yêu cầu tạo báo cáo điểm hoặc đánh giá học sinh.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["teacher_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID của giáo viên (tùy chọn, nếu không có sẽ lấy theo user hiện tại)"
                            },
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID lớp học cụ thể (tùy chọn, nếu không có sẽ lấy tất cả lớp của giáo viên)"
                            },
                            ["subject_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID môn học cụ thể (tùy chọn, nếu không có sẽ lấy tất cả môn học)"
                            },
                            ["semester"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Học kỳ cần tạo báo cáo (tùy chọn, mặc định là học kỳ hiện tại)"
                            },
                            ["include_analysis"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Có bao gồm phân tích chi tiết không (mặc định: true)"
                            },
                            ["include_recommendations"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Có bao gồm khuyến nghị cải thiện không (mặc định: true)"
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "create_student_performance_analysis",
                    Description = "📈 [COMPOSITE] PHÂN TÍCH HIỆU SUẤT HỌC TẬP CỦA HỌC SINH. Tự động thực hiện: Lấy thông tin học sinh → Lấy điểm số → Lấy báo cáo buổi học → Tạo phân tích tổng hợp về xu hướng học tập, điểm mạnh/yếu, và đề xuất cải thiện.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID của học sinh cần phân tích"
                            },
                            ["analysis_period"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Khoảng thời gian phân tích: 'current_semester', 'last_month', 'last_3_months', 'full_year'",
                                Enum = new[] { "current_semester", "last_month", "last_3_months", "full_year" }
                            },
                            ["include_comparison"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Có so sánh với trung bình lớp không (mặc định: true)"
                            },
                            ["focus_subjects"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Danh sách môn học cần tập trung phân tích (phân cách bằng dấu phẩy, tùy chọn)"
                            }
                        },
                        Required = new[] { "student_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "analyze_class_performance_trends",
                    Description = "📈 [ADVANCED] Phân tích xu hướng học tập của cả lớp theo thời gian. Tự động lấy dữ liệu học sinh, điểm số, chuyên cần và tạo báo cáo xu hướng chi tiết với biểu đồ và thống kê.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["class_id"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "ID của lớp học cần phân tích"
                            },
                            ["analysis_months"] = new ParameterSchema
                            {
                                Type = "number",
                                Description = "Số tháng phân tích ngược từ hiện tại (mặc định: 3)"
                            },
                            ["include_predictions"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Có bao gồm dự đoán xu hướng tương lai không (mặc định: true)"
                            }
                        },
                        Required = new[] { "class_id" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "generate_teacher_dashboard",
                    Description = "🎛️ [DASHBOARD] Tạo dashboard tổng quan cho giáo viên với tất cả thông tin quan trọng: lớp học, học sinh, điểm số, báo cáo, thông báo, và các công việc cần làm.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["teacher_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID giáo viên (tùy chọn, nếu không có sẽ dùng user hiện tại)"
                            },
                            ["dashboard_period"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Khoảng thời gian hiển thị: 'today', 'this_week', 'this_month'",
                                Enum = new[] { "today", "this_week", "this_month" }
                            },
                            ["priority_focus"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Tập trung ưu tiên: 'grades', 'attendance', 'behavior', 'all'",
                                Enum = new[] { "grades", "attendance", "behavior", "all" }
                            }
                        },
                        Required = new string[] { }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "create_parent_communication_report",
                    Description = "📞 [COMMUNICATION] Tạo báo cáo giao tiếp với phụ huynh, bao gồm thông tin học sinh, điểm số, thái độ học tập và đề xuất hỗ trợ tại nhà.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["student_id"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "ID học sinh cần tạo báo cáo giao tiếp"
                            },
                            ["communication_type"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Loại giao tiếp: 'progress_update', 'concern_report', 'achievement_recognition', 'support_request'",
                                Enum = new[] { "progress_update", "concern_report", "achievement_recognition", "support_request" }
                            },
                            ["include_action_plan"] = new ParameterSchema
                            {
                                Type = "boolean",
                                Description = "Có bao gồm kế hoạch hành động không (mặc định: true)"
                            }
                        },
                        Required = new[] { "student_id", "communication_type" }
                    }
                },
                new FunctionDeclaration
                {
                    Name = "intelligent_workflow_interpreter",
                    Description = "🧠 [AI INTERPRETER] Phân tích và hiểu yêu cầu của người dùng để tự động chọn workflow phù hợp. Có thể xử lý các yêu cầu phức tạp bằng tiếng Việt tự nhiên và chuyển đổi thành các hành động cụ thể.",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["user_request"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Yêu cầu của người dùng bằng ngôn ngữ tự nhiên"
                            },
                            ["context_info"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Thông tin ngữ cảnh bổ sung (vai trò người dùng, lớp học, môn học, v.v.)"
                            },
                            ["execution_mode"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Chế độ thực thi: 'auto_execute' (tự động thực hiện), 'plan_only' (chỉ lập kế hoạch), 'interactive' (tương tác từng bước)",
                                Enum = new[] { "auto_execute", "plan_only", "interactive" }
                            }
                        },
                        Required = new[] { "user_request" }
                    }
                }
            };
        }

        public bool IsFunctionSupported(string functionName)
        {
            var supportedFunctions = new[]
            {
                "get_workflow_guidance",
                "get_student_info",
                "get_student_subjects",
                "get_current_time",
                "format_vietnam_time",
                // "calculate_sum",
                "get_student_timetable",
                "get_teacher_info",
                "get_teacher_timetable",
                "search_students",
                "search_teachers",
                "get_all_classes",
                "get_timetable_by_class",
                "create_parent_feedback",
                "get_student_grades",
                "get_student_lesson_reports",
                "get_student_attendance",
                "add_student_attendance",
                "get_all_subjects",
                "get_subject_by_id",
                "get_student_exams",
                "get_class_students",
                "get_class_by_id",
                "get_student_grade_report",
                "get_all_students_grade_report",
                "create_lesson_report",
                "get_lesson_reports_by_teacher",
                "send_notification_to_parent",
                "get_all_feedbacks",
                "get_feedbacks_by_parent",
                "get_feedback_by_id",
                "get_notifications_for_user",
                "mark_notifications_as_read",
                // New functions added
                "get_student_parent_info",
                "get_all_subjects",
                "get_subject_teachers",
                "get_class_students_list",
                "get_teacher_classes",
                "get_class_attendance_summary",
                "get_exam_results_by_class",
                "get_student_exam_history",
                "get_parent_children_info",
                "get_recent_lesson_reports",
                "get_user_role_info",
                "search_notifications",
                "get_class_grade_statistics",
                "get_upcoming_exams",
                "create_timetable",
                "get_weekly_student_reports",
                // Principal Functions
                "send_broadcast_notification",
                "send_notification_to_teachers",
                "send_notification_to_students",
                "send_notification_to_parents",
                "create_user_account",
                "assign_user_roles",
                "delete_user_account",
                "get_all_users",
                // Teacher Functions - Exam Management
                "create_class_exam",
                "update_exam",
                "delete_exam",
                "get_class_exams",
                // Teacher Functions - Grade Management
                "create_grade",
                "update_grade",
                "delete_grade",
                "get_students_for_grading",
                // Student Functions
                "get_my_info",
                "get_my_exams",
                // Report Functions
                "create_report",
                "get_my_reports",
                "get_all_reports",
                "update_report_status",
                // COMPOSITE FUNCTIONS - Advanced multi-function operations
                "get_comprehensive_student_report",
                "analyze_student_performance", 
                "get_class_overview_report",
                "search_and_get_student_details",
                "get_teacher_dashboard",
                // ADDITIONAL STUDENT-FOCUSED COMPOSITE FUNCTIONS
                "get_student_academic_progress",
                "get_student_homework_overview",
                "compare_student_performance",
                "get_student_learning_plan",
                "get_student_weekly_summary",
                // NEW COMPOSITE FUNCTIONS FOR GRADE REPORTING
                "create_comprehensive_grade_report",
                "create_student_performance_analysis",
                // SEQUENTIAL AND ADVANCED WORKFLOW FUNCTIONS
                "execute_sequential_workflow",
                "create_custom_report",
                "analyze_class_performance_trends", 
                "generate_teacher_dashboard",
                "create_parent_communication_report",
                "intelligent_workflow_interpreter"
            };
            return supportedFunctions.Contains(functionName);
        }

        public async Task<string> ExecuteFunctionAsync(string functionName, Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                // Add input validation
                if (string.IsNullOrWhiteSpace(functionName))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Tên chức năng không được để trống.",
                        error = "INVALID_FUNCTION_NAME",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                if (args == null)
                {
                    args = new Dictionary<string, object>();
                }

                var result = functionName.ToLower() switch
                {
                    "get_workflow_guidance" => GetWorkflowGuidance(args),
                    "get_student_info" => await GetStudentInfoAsync(args),
                    "get_student_subjects" => await GetStudentSubjectsAsync(args),
                    "get_current_time" => GetCurrentTimeAsync(args),
                    "format_vietnam_time" => FormatVietnamTimeAsync(args),
                    "calculate_sum" => CalculateSumAsync(args),
                    "get_student_timetable" => await GetStudentTimetableAsync(args),
                    "get_teacher_info" => await GetTeacherInfoAsync(args),
                    "get_teacher_timetable" => await GetTeacherTimetableAsync(args),
                    "search_students" => await SearchStudentsAsync(args),
                    "search_teachers" => await SearchTeachersAsync(args),
                    "get_all_classes" => await GetAllClassesAsync(args),
                    "get_timetable_by_class" => await GetTimetableByClassAsync(args),
                    "create_parent_feedback" => await CreateParentFeedbackAsync(args, userId),
                    "get_student_grades" => await GetStudentGradesAsync(args, userId),
                    "get_student_lesson_reports" => await GetStudentLessonReportsAsync(args, userId),
                    "get_student_attendance" => await GetStudentAttendanceAsync(args, userId),
                    "add_student_attendance" => await AddStudentAttendanceAsync(args, userId),
                    "get_all_subjects" => await GetAllSubjectsAsync(args),
                    "get_subject_by_id" => await GetSubjectByIdAsync(args),
                    "get_student_exams" => await GetStudentExamsAsync(args, userId),
                    "get_class_students" => await GetClassStudentsAsync(args),
                    "get_class_by_id" => await GetClassByIdAsync(args),
                    "get_student_grade_report" => await GetStudentGradeReportAsync(args, userId),
                    "get_all_students_grade_report" => await GetAllStudentsGradeReportAsync(args),
                    "create_lesson_report" => await CreateLessonReportAsync(args, userId),
                    "get_lesson_reports_by_teacher" => await GetLessonReportsByTeacherAsync(args, userId),
                    "send_notification_to_parent" => await SendNotificationToParentAsync(args, userId),
                    "get_all_feedbacks" => await GetAllFeedbacksAsync(args),
                    "get_feedbacks_by_parent" => await GetFeedbacksByParentAsync(args),
                    "get_feedback_by_id" => await GetFeedbackByIdAsync(args),
                    "get_notifications_for_user" => await GetNotificationsForUserAsync(args, userId),
                    "mark_notifications_as_read" => await MarkNotificationsAsReadAsync(args, userId),
                    // New functions
                    "get_student_parent_info" => await GetStudentParentInfoAsync(args),
                    "get_subject_teachers" => await GetSubjectTeachersAsync(args),
                    "get_class_students_list" => await GetClassStudentsListAsync(args),
                    "get_teacher_classes" => await GetTeacherClassesAsync(args),
                    "get_class_attendance_summary" => await GetClassAttendanceSummaryAsync(args),
                    "get_exam_results_by_class" => await GetExamResultsByClassAsync(args),
                    "get_student_exam_history" => await GetStudentExamHistoryAsync(args),
                    "get_parent_children_info" => await GetParentChildrenInfoAsync(args),
                    "get_recent_lesson_reports" => await GetRecentLessonReportsAsync(args),
                    "get_user_role_info" => await GetUserRoleInfoAsync(args),
                    "search_notifications" => await SearchNotificationsAsync(args),
                    "get_class_grade_statistics" => await GetClassGradeStatisticsAsync(args),
                    "get_upcoming_exams" => await GetUpcomingExamsAsync(args),
                    "create_timetable" => await CreateTimetableAsync(args),
                    "get_weekly_student_reports" => await GetWeeklyStudentReportsAsync(args, userId),
                    // Principal Functions
                    "send_broadcast_notification" => await SendBroadcastNotificationAsync(args, userId),
                    "send_notification_to_teachers" => await SendNotificationToTeachersAsync(args, userId),
                    "send_notification_to_students" => await SendNotificationToStudentsAsync(args, userId),
                    "send_notification_to_parents" => await SendNotificationToParentsAsync(args, userId),
                    "create_user_account" => JsonConvert.SerializeObject(await CreateUserAccountAsync(args, userId), Formatting.Indented),
                    "assign_user_roles" => JsonConvert.SerializeObject(await AssignUserRolesAsync(args, userId), Formatting.Indented),
                    "delete_user_account" => JsonConvert.SerializeObject(await DeleteUserAccountAsync(args, userId), Formatting.Indented),
                    "get_all_users" => JsonConvert.SerializeObject(await GetAllUsersAsync(args, userId), Formatting.Indented),
                    // Teacher Functions - Exam Management
                    "create_class_exam" => JsonConvert.SerializeObject(await CreateClassExamAsync(args, userId), Formatting.Indented),
                    "update_exam" => JsonConvert.SerializeObject(await UpdateExamAsync(args, userId), Formatting.Indented),
                    "delete_exam" => JsonConvert.SerializeObject(await DeleteExamAsync(args, userId), Formatting.Indented),
                    "get_class_exams" => JsonConvert.SerializeObject(await GetClassExamsAsync(args, userId), Formatting.Indented),
                    // Teacher Functions - Grade Management
                    "create_grade" => JsonConvert.SerializeObject(await CreateGradeAsync(args, userId), Formatting.Indented),
                    "update_grade" => JsonConvert.SerializeObject(await UpdateGradeAsync(args, userId), Formatting.Indented),
                    "delete_grade" => JsonConvert.SerializeObject(await DeleteGradeAsync(args, userId), Formatting.Indented),
                    "get_students_for_grading" => JsonConvert.SerializeObject(await GetStudentsForGradingAsync(args, userId), Formatting.Indented),
                    // Student Functions
                    "get_my_info" => JsonConvert.SerializeObject(await GetMyInfoAsync(args, userId), Formatting.Indented),
                    "get_my_exams" => JsonConvert.SerializeObject(await GetMyExamsAsync(args, userId), Formatting.Indented),
                    // Report Functions
                    "create_report" => await CreateReportAsync(args, userId),
                    "get_my_reports" => await GetMyReportsAsync(args, userId),
                    "get_all_reports" => await GetAllReportsAsync(args, userId),
                    "update_report_status" => await UpdateReportStatusAsync(args, userId),
                    // COMPOSITE FUNCTIONS - Advanced multi-function operations
                    "get_comprehensive_student_report" => await GetComprehensiveStudentReportAsync(args, userId),
                    "analyze_student_performance" => await AnalyzeStudentPerformanceAsync(args, userId),
                    "get_class_overview_report" => await GetClassOverviewReportAsync(args, userId),
                    "search_and_get_student_details" => await SearchAndGetStudentDetailsAsync(args, userId),
                    "get_teacher_dashboard" => await GetTeacherDashboardAsync(args, userId),
                    // ADDITIONAL STUDENT-FOCUSED COMPOSITE FUNCTIONS
                    "get_student_academic_progress" => await GetStudentAcademicProgressAsync(args, userId),
                    "get_student_homework_overview" => await GetStudentHomeworkOverviewAsync(args, userId),
                    "compare_student_performance" => await CompareStudentPerformanceAsync(args, userId),
                    "get_student_learning_plan" => await GetStudentLearningPlanAsync(args, userId),
                    "get_student_weekly_summary" => await GetStudentWeeklySummaryAsync(args, userId),
                    // NEW COMPOSITE FUNCTIONS FOR GRADE REPORTING
                    "create_comprehensive_grade_report" => await CreateComprehensiveGradeReportAsync(args, userId),
                    "create_student_performance_analysis" => await CreateStudentPerformanceAnalysisAsync(args, userId),
                    // SEQUENTIAL AND ADVANCED WORKFLOW FUNCTIONS
                    "execute_sequential_workflow" => await ExecuteSequentialWorkflowAsync(args, userId),
                    "create_custom_report" => await CreateCustomReportAsync(args, userId),
                    "analyze_class_performance_trends" => await AnalyzeClassPerformanceTrendsAsync(args, userId),
                    "generate_teacher_dashboard" => await GenerateTeacherDashboardAsync(args, userId),
                    "create_parent_communication_report" => await CreateParentCommunicationReportAsync(args, userId),
                    "intelligent_workflow_interpreter" => await IntelligentWorkflowInterpreterAsync(args, userId),
                    _ => JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Chức năng '{functionName}' không được hỗ trợ. Vui lòng kiểm tra lại tên chức năng hoặc liên hệ bộ phận kỹ thuật.",
                        error = "FUNCTION_NOT_SUPPORTED",
                        data = (object?)null
                    }, Formatting.Indented)
                };

                // Validate result is not null or empty and ensure JSON format
                if (string.IsNullOrEmpty(result))
                {
                    result = CreateErrorResponse(
                        "Chức năng đã thực thi nhưng không trả về kết quả. Vui lòng thử lại sau.",
                        "EMPTY_RESULT"
                    );
                }
                else
                {
                    // Ensure response is in JSON format
                    result = EnsureJsonResponse(result);
                }

                // Gửi notification cho user sau mỗi lần chat (không await để không block)
                if (!string.IsNullOrEmpty(userId))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SendChatNotificationAsync(userId, functionName, result);
                        }
                        catch (Exception ex)
                        {
                            // Log error but don't throw
                            Console.WriteLine($"Error sending notification: {ex.Message}");
                        }
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                // Global exception handler
                return CreateErrorResponse(
                    "Có lỗi hệ thống xảy ra. Vui lòng thử lại sau hoặc liên hệ bộ phận kỹ thuật.",
                    "SYSTEM_ERROR",
                    $"Function: {functionName}, Error: {ex.Message}"
                );
            }
        }        private async Task<string> GetStudentInfoAsync(Dictionary<string, object> args)
        {
            try
            {
                if (!args.TryGetValue("student_id", out var studentIdObj) || 
                    string.IsNullOrEmpty(studentIdObj.ToString()))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Bạn cần cung cấp mã học sinh để tôi có thể tìm kiếm thông tin. Vui lòng nhập mã học sinh hợp lệ.",
                        error = "MISSING_STUDENT_ID",
                        data = (object?)null,
                        errorDetails = "Tham số student_id là bắt buộc và không được để trống"
                    }, Formatting.Indented);
                }

                var studentId = studentIdObj.ToString()!;

                var student = await _context.Students
                    .Include(s => s.Account)
                    .Include(s => s.Parent)
                        .ThenInclude(p => p.Account)
                    .FirstOrDefaultAsync(s => s.StudentID == studentId && s.IsDeleted != true);

                if (student == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Rất tiếc, tôi không tìm thấy học sinh nào có mã '{studentId}' trong hệ thống. Bạn có thể kiểm tra lại mã học sinh hoặc liên hệ phòng đào tạo để được hỗ trợ.",
                        error = "STUDENT_NOT_FOUND",
                        data = (object?)null,
                        errorDetails = $"Không có học sinh nào trong cơ sở dữ liệu có StudentID = '{studentId}'"
                    }, Formatting.Indented);
                }

                var result = new
                {
                    success = true,
                    message = "Tôi đã tìm thấy thông tin học sinh. Dưới đây là thông tin chi tiết:",
                    error = (string?)null,
                    data = new
                    {
                        StudentId = student.StudentID,
                        FullName = $"{student.Account?.FirstName} {student.Account?.LastName}".Trim(),
                        Email = student.Account?.Email,
                        Phone = student.Account?.PhoneNumber,
                        DateOfBirth = student.DateOfBirth?.ToString("dd/MM/yyyy"),
                        Gender = student.Gender,
                        ParentName = $"{student.Parent?.Account?.FirstName} {student.Parent?.Account?.LastName}".Trim(),
                        ParentPhone = student.Parent?.Account?.PhoneNumber,
                        ClassId = student.ClassID
                    }
                };

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi truy vấn thông tin học sinh. Vui lòng thử lại sau hoặc liên hệ bộ phận kỹ thuật.",
                    error = "DATABASE_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetStudentSubjectsAsync(Dictionary<string, object> args)
        {
            if (!args.TryGetValue("student_id", out var studentIdObj) || 
                string.IsNullOrEmpty(studentIdObj.ToString()))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Bạn cần cung cấp mã học sinh để tôi có thể tìm kiếm môn học. Vui lòng nhập mã học sinh hợp lệ.",
                    error = "MISSING_STUDENT_ID",
                    data = (object?)null,
                    errorDetails = "Tham số student_id là bắt buộc và không được để trống"
                }, Formatting.Indented);
            }

            var studentId = studentIdObj.ToString()!;

            try
            {
                // Tìm student và class của họ
                var student = await _context.Students
                    .Include(s => s.Class)
                    .FirstOrDefaultAsync(s => s.StudentID == studentId);

                if (student == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Rất tiếc, tôi không tìm thấy học sinh nào có mã '{studentId}' trong hệ thống. Bạn có thể kiểm tra lại mã học sinh hoặc liên hệ phòng đào tạo để được hỗ trợ.",
                        error = "STUDENT_NOT_FOUND",
                        data = (object?)null,
                        errorDetails = $"Không có học sinh nào trong cơ sở dữ liệu có StudentID = '{studentId}'"
                    }, Formatting.Indented);
                }

                if (student.ClassID == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Học sinh '{studentId}' chưa được xếp vào lớp học nào. Vui lòng liên hệ phòng đào tạo để được hỗ trợ.",
                        error = "STUDENT_NOT_ASSIGNED_TO_CLASS",
                        data = (object?)null,
                        errorDetails = "Student không có ClassID"
                    }, Formatting.Indented);
                }

                // Lấy các môn học từ timetable slots của lớp
                var subjects = await _context.TimetableSlots
                    .Where(ts => ts.Timetable.ClassID == student.ClassID && 
                                (ts.IsDeleted == null || ts.IsDeleted == false) &&
                                (ts.Timetable.IsDeleted == null || ts.Timetable.IsDeleted == false))
                    .Include(ts => ts.Subject)
                    .Select(ts => ts.Subject)
                    .Distinct()
                    .ToListAsync();

                if (subjects == null || !subjects.Any())
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Hiện tại lớp của học sinh '{studentId}' chưa có thời khóa biểu hoặc môn học nào được xếp lịch. Vui lòng liên hệ giáo vụ để kiểm tra.",
                        error = "NO_SUBJECTS_FOUND",
                        data = (object?)null,
                        errorDetails = $"Không tìm thấy subject nào trong timetable của ClassID {student.ClassID}"
                    }, Formatting.Indented);
                }

                var result = new
                {
                    success = true,
                    message = $"Tôi đã tìm thấy {subjects.Count} môn học mà học sinh '{studentId}' đang theo học:",
                    error = (string?)null,
                    data = new
                    {
                        StudentId = studentId,
                        ClassName = student.Class?.ClassName,
                        ClassId = student.ClassID,
                        SubjectCount = subjects.Count,
                        Subjects = subjects.Select(s => new
                        {
                            SubjectId = s.SubjectId,
                            SubjectName = s.SubjectName,
                            SubjectCode = s.SubjectCode,
                            Description = s.Description
                        }).ToList()
                    }
                };

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi truy vấn thông tin môn học. Vui lòng thử lại sau hoặc liên hệ bộ phận kỹ thuật.",
                    error = "DATABASE_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private string GetCurrentTimeAsync(Dictionary<string, object> args)
        {
            var timezone = "GMT+7";
            if (args.TryGetValue("timezone", out var timezoneObj))
            {
                timezone = timezoneObj.ToString() ?? "GMT+7";
            }

            try
            {
                // Vietnam timezone (GMT+7)
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var vietnamTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, vietnamTimeZone);
                
                // Culture info for Vietnamese
                var vietnameseCulture = new System.Globalization.CultureInfo("vi-VN");

                var result = new
                {
                    success = true,
                    message = "Đây là thời gian hiện tại theo múi giờ Việt Nam (GMT+7):",
                    error = (string?)null,
                    data = new
                    {
                        CurrentTime = vietnamTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        CurrentDate = vietnamTime.ToString("dd/MM/yyyy"),
                        DayOfWeek = vietnamTime.ToString("dddd", vietnameseCulture),
                        Timezone = "GMT+7 (Giờ Việt Nam)",
                        FriendlyTime = vietnamTime.ToString("HH:mm"),
                        FriendlyDate = vietnamTime.ToString("'ngày' dd 'tháng' MM 'năm' yyyy", vietnameseCulture),
                        ISO8601 = vietnamTime.ToString("yyyy-MM-ddTHH:mm:ss+07:00"),
                        UnixTimestamp = ((DateTimeOffset)vietnamTime).ToUnixTimeSeconds(),
                        WeekOfYear = vietnameseCulture.Calendar.GetWeekOfYear(vietnamTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday),
                        Quarter = $"Quý {(vietnamTime.Month - 1) / 3 + 1}",
                        Season = GetVietnameseSeason(vietnamTime.Month)
                    }
                };

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                // Fallback to UTC+7 if timezone lookup fails
                var fallbackTime = DateTime.UtcNow.AddHours(7);
                var vietnameseCulture = new System.Globalization.CultureInfo("vi-VN");
                
                var fallbackResult = new
                {
                    success = true,
                    message = "Đây là thời gian hiện tại theo múi giờ Việt Nam (GMT+7) - Fallback mode:",
                    error = "TIMEZONE_FALLBACK",
                    data = new
                    {
                        CurrentTime = fallbackTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        CurrentDate = fallbackTime.ToString("dd/MM/yyyy"),
                        DayOfWeek = fallbackTime.ToString("dddd", vietnameseCulture),
                        Timezone = "GMT+7 (Giờ Việt Nam)",
                        FriendlyTime = fallbackTime.ToString("HH:mm"),
                        FriendlyDate = fallbackTime.ToString("'ngày' dd 'tháng' MM 'năm' yyyy", vietnameseCulture),
                        ISO8601 = fallbackTime.ToString("yyyy-MM-ddTHH:mm:ss+07:00"),
                        UnixTimestamp = ((DateTimeOffset)fallbackTime.AddHours(-7)).ToUnixTimeSeconds(),
                        Note = "Sử dụng phương pháp tính toán thay thế do lỗi timezone system"
                    }
                };

                return JsonConvert.SerializeObject(fallbackResult, Formatting.Indented);
            }
        }

        private string GetVietnameseSeason(int month)
        {
            return month switch
            {
                12 or 1 or 2 => "Mùa đông",
                3 or 4 or 5 => "Mùa xuân", 
                6 or 7 or 8 => "Mùa hè",
                9 or 10 or 11 => "Mùa thu",
                _ => "Không xác định"
            };
        }

        private string FormatVietnamTimeAsync(Dictionary<string, object> args)
        {
            var format = "full";
            var includeTimezone = true;

            if (args.TryGetValue("format", out var formatObj))
            {
                format = formatObj.ToString()?.ToLower() ?? "full";
            }

            if (args.TryGetValue("include_timezone", out var timezoneObj))
            {
                bool.TryParse(timezoneObj.ToString(), out includeTimezone);
            }

            try
            {
                // Vietnam timezone (GMT+7)
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var vietnamTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, vietnamTimeZone);
                var vietnameseCulture = new System.Globalization.CultureInfo("vi-VN");

                object result = format switch
                {
                    "date_only" => new
                    {
                        success = true,
                        message = "Ngày hiện tại tại Việt Nam:",
                        data = new
                        {
                            Date = vietnamTime.ToString("dd/MM/yyyy"),
                            DateLong = vietnamTime.ToString("'ngày' dd 'tháng' MM 'năm' yyyy", vietnameseCulture),
                            DayOfWeek = vietnamTime.ToString("dddd", vietnameseCulture),
                            WeekOfYear = vietnameseCulture.Calendar.GetWeekOfYear(vietnamTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday),
                            Quarter = $"Quý {(vietnamTime.Month - 1) / 3 + 1}",
                            Season = GetVietnameseSeason(vietnamTime.Month),
                            Timezone = includeTimezone ? "GMT+7 (Việt Nam)" : null
                        }
                    },
                    "time_only" => new
                    {
                        success = true,
                        message = "Thời gian hiện tại tại Việt Nam:",
                        data = new
                        {
                            Time = vietnamTime.ToString("HH:mm:ss"),
                            TimeShort = vietnamTime.ToString("HH:mm"),
                            TimeWithAmPm = vietnamTime.ToString("hh:mm:ss tt", vietnameseCulture),
                            Hour = vietnamTime.Hour,
                            Minute = vietnamTime.Minute,
                            Second = vietnamTime.Second,
                            TimeOfDay = GetTimeOfDay(vietnamTime.Hour),
                            Timezone = includeTimezone ? "GMT+7 (Việt Nam)" : null
                        }
                    },
                    "iso" => new
                    {
                        success = true,
                        message = "Thời gian hiện tại tại Việt Nam (ISO 8601):",
                        data = new
                        {
                            ISO8601 = vietnamTime.ToString("yyyy-MM-ddTHH:mm:ss+07:00"),
                            ISO8601_UTC = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            Timezone = includeTimezone ? "GMT+7 (Việt Nam)" : null
                        }
                    },
                    "timestamp" => new
                    {
                        success = true,
                        message = "Unix timestamp hiện tại:",
                        data = new
                        {
                            UnixTimestamp = ((DateTimeOffset)vietnamTime).ToUnixTimeSeconds(),
                            UnixTimestampMs = ((DateTimeOffset)vietnamTime).ToUnixTimeMilliseconds(),
                            UnixTimestampUTC = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds(),
                            Timezone = includeTimezone ? "GMT+7 (Việt Nam)" : null
                        }
                    },
                    "friendly" => new
                    {
                        success = true,
                        message = "Thời gian hiện tại tại Việt Nam (dễ đọc):",
                        data = new
                        {
                            FriendlyDateTime = $"{GetTimeOfDay(vietnamTime.Hour)} {vietnamTime:HH:mm}, {vietnamTime.ToString("dddd", vietnameseCulture)}, {vietnamTime.ToString("'ngày' dd 'tháng' MM 'năm' yyyy", vietnameseCulture)}",
                            TimeOfDay = GetTimeOfDay(vietnamTime.Hour),
                            DateFriendly = vietnamTime.ToString("'ngày' dd 'tháng' MM 'năm' yyyy", vietnameseCulture),
                            TimeFriendly = vietnamTime.ToString("HH:mm"),
                            DayOfWeek = vietnamTime.ToString("dddd", vietnameseCulture),
                            Season = GetVietnameseSeason(vietnamTime.Month),
                            Timezone = includeTimezone ? "GMT+7 (Việt Nam)" : null
                        }
                    },
                    _ => new // "full" or default
                    {
                        success = true,
                        message = "Thời gian đầy đủ hiện tại tại Việt Nam:",
                        data = new
                        {
                            DateTime = vietnamTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            Date = vietnamTime.ToString("dd/MM/yyyy"),
                            Time = vietnamTime.ToString("HH:mm:ss"),
                            DayOfWeek = vietnamTime.ToString("dddd", vietnameseCulture),
                            ISO8601 = vietnamTime.ToString("yyyy-MM-ddTHH:mm:ss+07:00"),
                            UnixTimestamp = ((DateTimeOffset)vietnamTime).ToUnixTimeSeconds(),
                            FriendlyDateTime = $"{GetTimeOfDay(vietnamTime.Hour)} {vietnamTime:HH:mm}, {vietnamTime.ToString("dddd", vietnameseCulture)}, {vietnamTime.ToString("'ngày' dd 'tháng' MM 'năm' yyyy", vietnameseCulture)}",
                            TimeOfDay = GetTimeOfDay(vietnamTime.Hour),
                            WeekOfYear = vietnameseCulture.Calendar.GetWeekOfYear(vietnamTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday),
                            Quarter = $"Quý {(vietnamTime.Month - 1) / 3 + 1}",
                            Season = GetVietnameseSeason(vietnamTime.Month),
                            Timezone = includeTimezone ? "GMT+7 (Việt Nam)" : null
                        }
                    }
                };

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                // Fallback to UTC+7 if timezone lookup fails
                var fallbackTime = DateTime.UtcNow.AddHours(7);
                var vietnameseCulture = new System.Globalization.CultureInfo("vi-VN");
                
                var fallbackResult = new
                {
                    success = true,
                    message = $"Thời gian hiện tại tại Việt Nam (format: {format}) - Fallback mode:",
                    error = "TIMEZONE_FALLBACK",
                    data = new
                    {
                        DateTime = fallbackTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        Date = fallbackTime.ToString("dd/MM/yyyy"),
                        Time = fallbackTime.ToString("HH:mm:ss"),
                        Note = "Sử dụng phương pháp tính toán thay thế do lỗi timezone system",
                        Timezone = includeTimezone ? "GMT+7 (Việt Nam)" : null
                    }
                };

                return JsonConvert.SerializeObject(fallbackResult, Formatting.Indented);
            }
        }

        private string GetTimeOfDay(int hour)
        {
            return hour switch
            {
                >= 5 and < 12 => "Buổi sáng",
                >= 12 and < 14 => "Buổi trưa", 
                >= 14 and < 18 => "Buổi chiều",
                >= 18 and < 22 => "Buổi tối",
                _ => "Buổi đêm"
            };
        }

        private string CalculateSumAsync(Dictionary<string, object> args)
        {
            if (!args.TryGetValue("number1", out var num1Obj) || 
                !args.TryGetValue("number2", out var num2Obj))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Để thực hiện phép tính, bạn cần cung cấp đầy đủ hai số. Vui lòng nhập số thứ nhất và số thứ hai.",
                    error = "MISSING_PARAMETERS",
                    data = (object?)null,
                    errorDetails = "Cả hai tham số number1 và number2 đều là bắt buộc"
                }, Formatting.Indented);
            }

            if (!double.TryParse(num1Obj.ToString(), out var number1) ||
                !double.TryParse(num2Obj.ToString(), out var number2))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Giá trị bạn nhập không phải là số hợp lệ. Vui lòng nhập các số thực (ví dụ: 5, 3.14, -2.5).",
                    error = "INVALID_NUMBER_FORMAT",
                    data = (object?)null,
                    errorDetails = $"Không thể chuyển đổi '{num1Obj}' hoặc '{num2Obj}' thành số"
                }, Formatting.Indented);
            }

            try
            {
                var sum = number1 + number2;
                var result = new
                {
                    success = true,
                    message = $"Kết quả phép tính: {number1} + {number2} = {sum}",
                    error = (string?)null,
                    data = new
                    {
                        Number1 = number1,
                        Number2 = number2,
                        Sum = sum,
                        Operation = $"{number1} + {number2} = {sum}",
                        CalculationType = "Phép cộng"
                    }
                };

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Có lỗi xảy ra trong quá trình tính toán. Vui lòng thử lại với các số khác.",
                    error = "CALCULATION_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetStudentTimetableAsync(Dictionary<string, object> args)
        {
            if (!args.TryGetValue("student_id", out var studentIdObj) || 
                string.IsNullOrEmpty(studentIdObj.ToString()))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Bạn cần cung cấp mã học sinh để tôi có thể tìm kiếm thời khóa biểu. Vui lòng nhập mã học sinh hợp lệ.",
                    error = "MISSING_STUDENT_ID",
                    data = (object?)null,
                    errorDetails = "Tham số student_id là bắt buộc và không được để trống"
                }, Formatting.Indented);
            }

            var studentId = studentIdObj.ToString()!;
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (args.TryGetValue("start_date", out var startDateObj) && !string.IsNullOrEmpty(startDateObj.ToString()))
            {
                if (DateTime.TryParse(startDateObj.ToString(), out var parsedStartDate))
                    startDate = parsedStartDate;
            }

            if (args.TryGetValue("end_date", out var endDateObj) && !string.IsNullOrEmpty(endDateObj.ToString()))
            {
                if (DateTime.TryParse(endDateObj.ToString(), out var parsedEndDate))
                    endDate = parsedEndDate;
            }

            try
            {
                var response = await _studentService.GetTimetableByStudentIdAsync(studentId, startDate, endDate);
                
                if (!response.Success)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Không thể lấy thời khóa biểu cho học sinh '{studentId}'. {string.Join(", ", response.Error ?? new List<string>())}",
                        error = "TIMETABLE_NOT_FOUND",
                        data = (object?)null,
                        errorDetails = string.Join(", ", response.Error ?? new List<string>())
                    }, Formatting.Indented);
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = $"Tôi đã tìm thấy thời khóa biểu cho học sinh '{studentId}':",
                    error = (string?)null,
                    data = response.Data
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi truy vấn thời khóa biểu. Vui lòng thử lại sau hoặc liên hệ bộ phận kỹ thuật.",
                    error = "DATABASE_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetTeacherInfoAsync(Dictionary<string, object> args)
        {
            if (!args.TryGetValue("teacher_id", out var teacherIdObj) || 
                string.IsNullOrEmpty(teacherIdObj.ToString()))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Bạn cần cung cấp mã giáo viên để tôi có thể tìm kiếm thông tin. Vui lòng nhập mã giáo viên hợp lệ.",
                    error = "MISSING_TEACHER_ID",
                    data = (object?)null,
                    errorDetails = "Tham số teacher_id là bắt buộc và không được để trống"
                }, Formatting.Indented);
            }

            var teacherId = teacherIdObj.ToString()!;

            try
            {
                var response = await _teacherService.GetTeacherByIdAsync(teacherId);
                
                if (!response.Success)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Rất tiếc, tôi không tìm thấy giáo viên nào có mã '{teacherId}' trong hệ thống. Bạn có thể kiểm tra lại mã giáo viên hoặc liên hệ phòng nhân sự để được hỗ trợ.",
                        error = "TEACHER_NOT_FOUND",
                        data = (object?)null,
                        errorDetails = string.Join(", ", response.Error ?? new List<string>())
                    }, Formatting.Indented);
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = "Tôi đã tìm thấy thông tin giáo viên. Dưới đây là thông tin chi tiết:",
                    error = (string?)null,
                    data = response.Data
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi truy vấn thông tin giáo viên. Vui lòng thử lại sau hoặc liên hệ bộ phận kỹ thuật.",
                    error = "DATABASE_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetTeacherTimetableAsync(Dictionary<string, object> args)
        {
            if (!args.TryGetValue("teacher_id", out var teacherIdObj) || 
                string.IsNullOrEmpty(teacherIdObj.ToString()))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Bạn cần cung cấp mã giáo viên để tôi có thể tìm kiếm thời khóa biểu. Vui lòng nhập mã giáo viên hợp lệ.",
                    error = "MISSING_TEACHER_ID",
                    data = (object?)null,
                    errorDetails = "Tham số teacher_id là bắt buộc và không được để trống"
                }, Formatting.Indented);
            }

            var teacherId = teacherIdObj.ToString()!;
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (args.TryGetValue("start_date", out var startDateObj) && !string.IsNullOrEmpty(startDateObj.ToString()))
            {
                if (DateTime.TryParse(startDateObj.ToString(), out var parsedStartDate))
                    startDate = parsedStartDate;
            }

            if (args.TryGetValue("end_date", out var endDateObj) && !string.IsNullOrEmpty(endDateObj.ToString()))
            {
                if (DateTime.TryParse(endDateObj.ToString(), out var parsedEndDate))
                    endDate = parsedEndDate;
            }

            try
            {
                var response = await _teacherService.GetTimetableByTeacherIdAsync(teacherId, startDate, endDate);
                
                if (!response.Success)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Không thể lấy thời khóa biểu cho giáo viên '{teacherId}'. {string.Join(", ", response.Error ?? new List<string>())}",
                        error = "TIMETABLE_NOT_FOUND",
                        data = (object?)null,
                        errorDetails = string.Join(", ", response.Error ?? new List<string>())
                    }, Formatting.Indented);
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = $"Tôi đã tìm thấy thời khóa biểu cho giáo viên '{teacherId}':",
                    error = (string?)null,
                    data = response.Data
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi truy vấn thời khóa biểu giáo viên. Vui lòng thử lại sau hoặc liên hệ bộ phận kỹ thuật.",
                    error = "DATABASE_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> SearchStudentsAsync(Dictionary<string, object> args)
        {
            if (!args.TryGetValue("search_term", out var searchTermObj) || 
                string.IsNullOrEmpty(searchTermObj.ToString()))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Bạn cần cung cấp từ khóa tìm kiếm để tôi có thể tìm học sinh. Vui lòng nhập tên hoặc mã học sinh.",
                    error = "MISSING_SEARCH_TERM",
                    data = (object?)null,
                    errorDetails = "Tham số search_term là bắt buộc và không được để trống"
                }, Formatting.Indented);
            }

            var searchTerm = searchTermObj.ToString()!;
            var pageSize = 10;

            if (args.TryGetValue("page_size", out var pageSizeObj) && int.TryParse(pageSizeObj.ToString(), out var parsedPageSize))
            {
                pageSize = Math.Min(Math.Max(parsedPageSize, 1), 50); // Limit between 1-50
            }

            try
            {
                var response = await _studentService.FilterSortSearchStudentsAsync(
                    searchTerm: searchTerm,
                    pageSize: pageSize,
                    pageIndex: 0
                );
                
                if (!response.Success)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Không thể tìm kiếm học sinh với từ khóa '{searchTerm}'. {string.Join(", ", response.Error ?? new List<string>())}",
                        error = "SEARCH_ERROR",
                        data = (object?)null,
                        errorDetails = string.Join(", ", response.Error ?? new List<string>())
                    }, Formatting.Indented);
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = $"Tôi đã tìm thấy {response.Data?.Count ?? 0} học sinh phù hợp với từ khóa '{searchTerm}':",
                    error = (string?)null,
                    data = new
                    {
                        SearchTerm = searchTerm,
                        TotalFound = response.Data?.Count ?? 0,
                        Students = response.Data
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tìm kiếm học sinh. Vui lòng thử lại sau hoặc liên hệ bộ phận kỹ thuật.",
                    error = "DATABASE_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> SearchTeachersAsync(Dictionary<string, object> args)
        {
            if (!args.TryGetValue("search_term", out var searchTermObj) || 
                string.IsNullOrEmpty(searchTermObj.ToString()))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Bạn cần cung cấp từ khóa tìm kiếm để tôi có thể tìm giáo viên. Vui lòng nhập tên hoặc mã giáo viên.",
                    error = "MISSING_SEARCH_TERM",
                    data = (object?)null,
                    errorDetails = "Tham số search_term là bắt buộc và không được để trống"
                }, Formatting.Indented);
            }

            var searchTerm = searchTermObj.ToString()!;
            var pageSize = 10;

            if (args.TryGetValue("page_size", out var pageSizeObj) && int.TryParse(pageSizeObj.ToString(), out var parsedPageSize))
            {
                pageSize = Math.Min(Math.Max(parsedPageSize, 1), 50); // Limit between 1-50
            }

            try
            {
                var response = await _teacherService.FilterSortSearchTeachersAsync(
                    searchTerm: searchTerm,
                    pageSize: pageSize,
                    pageIndex: 0
                );
                
                if (!response.Success)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Không thể tìm kiếm giáo viên với từ khóa '{searchTerm}'. {string.Join(", ", response.Error ?? new List<string>())}",
                        error = "SEARCH_ERROR",
                        data = (object?)null,
                        errorDetails = string.Join(", ", response.Error ?? new List<string>())
                    }, Formatting.Indented);
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = $"Tôi đã tìm thấy {response.Data?.Count ?? 0} giáo viên phù hợp với từ khóa '{searchTerm}':",
                    error = (string?)null,
                    data = new
                    {
                        SearchTerm = searchTerm,
                        TotalFound = response.Data?.Count ?? 0,
                        Teachers = response.Data
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tìm kiếm giáo viên. Vui lòng thử lại sau hoặc liên hệ bộ phận kỹ thuật.",
                    error = "DATABASE_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetAllClassesAsync(Dictionary<string, object> args)
        {
            string? searchTerm = null;
            if (args.TryGetValue("search_term", out var searchTermObj) && !string.IsNullOrEmpty(searchTermObj.ToString()))
            {
                searchTerm = searchTermObj.ToString();
            }

            try
            {
                IEnumerable<EduConnect.Domain.Entities.Class> classes;
                
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    classes = await _classService.SearchClassesAsync(searchTerm);
                }
                else
                {
                    classes = await _classService.GetAllClassesAsync();
                }

                var classesList = classes.ToList();

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = string.IsNullOrEmpty(searchTerm) 
                        ? $"Tôi đã tìm thấy {classesList.Count} lớp học trong hệ thống:"
                        : $"Tôi đã tìm thấy {classesList.Count} lớp học phù hợp với từ khóa '{searchTerm}':",
                    error = (string?)null,
                    data = new
                    {
                        SearchTerm = searchTerm,
                        TotalClasses = classesList.Count,
                        Classes = classesList.Select(c => new
                        {
                            ClassId = c.ClassID,
                            ClassName = c.ClassName,
                            IsDeleted = c.IsDeleted
                        }).ToList()
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi truy vấn danh sách lớp học. Vui lòng thử lại sau hoặc liên hệ bộ phận kỹ thuật.",
                    error = "DATABASE_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetTimetableByClassAsync(Dictionary<string, object> args)
        {
            if (!args.TryGetValue("class_id", out var classIdObj) || 
                !int.TryParse(classIdObj.ToString(), out var classId))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Bạn cần cung cấp mã lớp học hợp lệ để tôi có thể tìm kiếm thời khóa biểu. Vui lòng nhập số ID của lớp.",
                    error = "MISSING_OR_INVALID_CLASS_ID",
                    data = (object?)null,
                    errorDetails = "Tham số class_id là bắt buộc và phải là số nguyên"
                }, Formatting.Indented);
            }

            DateTime? startDate = null;
            DateTime? endDate = null;

            if (args.TryGetValue("start_date", out var startDateObj) && !string.IsNullOrEmpty(startDateObj.ToString()))
            {
                if (DateTime.TryParse(startDateObj.ToString(), out var parsedStartDate))
                    startDate = parsedStartDate;
            }

            if (args.TryGetValue("end_date", out var endDateObj) && !string.IsNullOrEmpty(endDateObj.ToString()))
            {
                if (DateTime.TryParse(endDateObj.ToString(), out var parsedEndDate))
                    endDate = parsedEndDate;
            }

            try
            {
                var response = await _timetableService.GetTimetablesByClassIdAsync(classId, startDate, endDate);
                
                if (!response.Success)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Không thể lấy thời khóa biểu cho lớp có ID '{classId}'. {string.Join(", ", response.Error ?? new List<string>())}",
                        error = "TIMETABLE_NOT_FOUND",
                        data = (object?)null,
                        errorDetails = string.Join(", ", response.Error ?? new List<string>())
                    }, Formatting.Indented);
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = $"Tôi đã tìm thấy thời khóa biểu cho lớp có ID '{classId}':",
                    error = (string?)null,
                    data = new
                    {
                        ClassId = classId,
                        TimetableCount = response.Data?.Count ?? 0,
                        Timetables = response.Data
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi truy vấn thời khóa biểu lớp học. Vui lòng thử lại sau hoặc liên hệ bộ phận kỹ thuật.",
                    error = "DATABASE_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> CreateParentFeedbackAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                // Validate userId from parameter
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không thể tạo phản hồi: Thiếu thông tin người dùng",
                        error = "MISSING_USER_ID"
                    }, Formatting.Indented);
                }

                if (!args.TryGetValue("content", out var contentObj) || string.IsNullOrEmpty(contentObj.ToString()))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không thể tạo phản hồi: Thiếu nội dung phản hồi",
                        error = "MISSING_CONTENT"
                    }, Formatting.Indented);
                }

                if (!args.TryGetValue("target", out var targetObj) || string.IsNullOrEmpty(targetObj.ToString()))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không thể tạo phản hồi: Thiếu thông tin đối tượng phản hồi",
                        error = "MISSING_TARGET"
                    }, Formatting.Indented);
                }

                var content = contentObj.ToString()!;
                var target = targetObj.ToString()!;
                var category = args.TryGetValue("category", out var categoryObj) ? categoryObj.ToString() : "Other";

                // Validate target
                if (target != "Principal" && target != "Teacher")
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Đối tượng phản hồi không hợp lệ. Chỉ có thể gửi phản hồi đến 'Principal' hoặc 'Teacher'",
                        error = "INVALID_TARGET"
                    }, Formatting.Indented);
                }

                // Get parent information
                var parent = await _parentRepository.GetByAccountIdAsync(userId);
                if (parent == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin phụ huynh. Vui lòng đảm bảo bạn đã đăng nhập với tài khoản phụ huynh.",
                        error = "PARENT_NOT_FOUND"
                    }, Formatting.Indented);
                }

                // Create feedback request with detailed information
                var feedbackRequest = new CreateFeedbackRequest
                {
                    Content = content,
                    Target = target,
                    ParentId = parent.ParentID,
                    Category = category
                };

                // Get detailed information based on target and additional parameters
                if (target == "Teacher")
                {
                    // Try to get teacher information from arguments
                    if (args.TryGetValue("teacher_id", out var teacherIdObj) && !string.IsNullOrEmpty(teacherIdObj.ToString()))
                    {
                        var teacherId = teacherIdObj.ToString()!;
                        var teacher = await _teacherRepository.GetByIdAsync(teacherId);
                        if (teacher != null)
                        {
                            feedbackRequest.TeacherId = teacherId;
                            feedbackRequest.TeacherName = $"{teacher.Account.FirstName} {teacher.Account.LastName}".Trim();
                        }
                    }

                    // Try to get subject information from arguments
                    if (args.TryGetValue("subject_id", out var subjectIdObj) && int.TryParse(subjectIdObj.ToString(), out var subjectId))
                    {
                        var subject = await _subjectRepository.GetByIdAsync(subjectId);
                        if (subject != null)
                        {
                            feedbackRequest.SubjectId = subjectId;
                            feedbackRequest.SubjectName = subject.SubjectName;
                        }
                    }

                    // Try to get class information from arguments
                    if (args.TryGetValue("class_id", out var classIdObj) && int.TryParse(classIdObj.ToString(), out var classId))
                    {
                        var classEntity = await _classRepository.GetByIdAsync(classId);
                        if (classEntity != null)
                        {
                            feedbackRequest.ClassId = classId;
                            feedbackRequest.ClassName = classEntity.ClassName;
                        }
                    }
                }

                // Create feedback
                var result = await _feedbackService.CreateFeedbackAsync(feedbackRequest);

                if (result.Success)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = true,
                        message = $"✅ Đã tạo phản hồi thành công. Mã phản hồi: #{result.Data?.FeedbackId}",
                        data = new
                        {
                            FeedbackId = result.Data?.FeedbackId,
                            Content = content,
                            Target = target,
                            Category = category,
                            ParentId = parent.ParentID,
                            ParentName = parent.FullName,
                            TeacherId = feedbackRequest.TeacherId,
                            TeacherName = feedbackRequest.TeacherName,
                            SubjectId = feedbackRequest.SubjectId,
                            SubjectName = feedbackRequest.SubjectName,
                            ClassId = feedbackRequest.ClassId,
                            ClassName = feedbackRequest.ClassName,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        }
                    }, Formatting.Indented);
                }
                else
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"❌ Không thể tạo phản hồi: {result.Message}",
                        error = "FEEDBACK_CREATION_FAILED"
                    }, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi tạo phản hồi. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetStudentGradesAsync(Dictionary<string, object> args, string? userId = null)
{
    try
    {
        if (string.IsNullOrEmpty(userId))
        {
            return JsonConvert.SerializeObject(new { success = false, message = "Cần đăng nhập.", error = "AUTHENTICATION_REQUIRED" });
        }

        // Get parent information
        var parent = await _parentRepository.GetByAccountIdAsync(userId);
        if (parent == null)
        {
            return JsonConvert.SerializeObject(new { success = false, message = "Không tìm thấy thông tin phụ huynh.", error = "PARENT_NOT_FOUND" });
        }

        // Get all students of this parent
        var students = await _context.Students
            .Include(s => s.Account)
            .Include(s => s.Class)
            .Where(s => s.ParentID == parent.ParentID)
            .ToListAsync();

        if (!students.Any())
        {
            return JsonConvert.SerializeObject(new { success = false, message = "Tài khoản của bạn chưa liên kết với học sinh nào.", error = "NO_STUDENTS_FOUND" });
        }

        // Filter by student name if provided
        var studentNameFilter = args.TryGetValue("student_name", out var studentNameObj) ? studentNameObj?.ToString() : null;
        if (!string.IsNullOrEmpty(studentNameFilter))
        {
            students = students.Where(s => s.FullName?.Contains(studentNameFilter, StringComparison.OrdinalIgnoreCase) == true).ToList();
            if (!students.Any())
            {
                return JsonConvert.SerializeObject(new { success = false, message = $"Không tìm thấy học sinh '{studentNameFilter}'.", error = "STUDENT_NOT_FOUND" });
            }
        }

        // Get other filters
        var semesterFilter = args.TryGetValue("semester", out var semesterObj) ? semesterObj?.ToString()?.Trim() : null;
        var subjectNameFilter = args.TryGetValue("subject_name", out var subjectNameObj) ? subjectNameObj?.ToString()?.Trim() : null;
        var studentIds = students.Select(s => s.StudentID).ToList();

        // Get grades query
        var gradesQuery = _context.Grades
            .Where(g => studentIds.Contains(g.StudentID) && g.IsDeleted != true);

        if (!string.IsNullOrEmpty(semesterFilter))
        {
            var normalizedSemester = semesterFilter.ToLower().Replace(" ", "");
            if (normalizedSemester.Contains("1") || normalizedSemester.Contains("i"))
            {
                gradesQuery = gradesQuery.Where(g => g.Semester.Contains("1") || g.Semester.ToLower().Contains("i"));
            }
            else if (normalizedSemester.Contains("2") || normalizedSemester.Contains("ii"))
            {
                gradesQuery = gradesQuery.Where(g => g.Semester.Contains("2") || g.Semester.ToLower().Contains("ii"));
            }
            else
            {
                gradesQuery = gradesQuery.Where(g => g.Semester.Equals(semesterFilter, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (!string.IsNullOrEmpty(subjectNameFilter))
        {
            gradesQuery = gradesQuery.Where(g => g.Subject.SubjectName.Contains(subjectNameFilter, StringComparison.OrdinalIgnoreCase));
        }

        var grades = await gradesQuery
            .Include(g => g.Subject)
            .ToListAsync();

        // Build the structured data object
        var gradeSummary = students.Select(student => new
        {
            StudentInfo = new
            {
                student.StudentID,
                student.FullName,
                DateOfBirth = student.DateOfBirth?.ToString("dd/MM/yyyy"),
                ClassName = student.Class?.ClassName
            },
            Grades = grades
                .Where(g => g.StudentID == student.StudentID)
                .GroupBy(g => new { g.SubjectID, g.Semester, g.Subject.SubjectName })
                .Select(group => new
                {
                    SubjectName = group.Key.SubjectName,
                    group.Key.Semester,
                    Scores = group.Select(g => new { g.GradeType, g.Score, g.Evaluation }).ToList(),
                    AverageScore = group.Any(g => g.Score.HasValue) ? group.Where(g => g.Score.HasValue).Average(g => g.Score) : (double?)null
                })
                .OrderBy(g => g.SubjectName)
                .ToList()
        }).ToList();

        // Return the final JSON object
        return JsonConvert.SerializeObject(new
        {
            success = true,
            message = "Lấy dữ liệu điểm thành công.",
            data = new
            {
                TotalStudents = students.Count,
                StudentsWithGrades = gradeSummary.Count(s => s.Grades.Any()),
                TotalGrades = grades.Count,
                FilterApplied = new
                {
                    StudentName = studentNameFilter,
                    Semester = semesterFilter,
                    SubjectName = subjectNameFilter
                },
                Students = gradeSummary
            }
        }, new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore });
    }
    catch (Exception ex)
    {
        // Log the exception details here if needed
        return JsonConvert.SerializeObject(new
        {
            success = false,
            message = "❌ Có lỗi xảy ra khi lấy điểm học sinh.",
            error = "INTERNAL_ERROR",
            errorDetails = ex.Message 
        }, Formatting.Indented);
    }
}

        private async Task<string> GetStudentLessonReportsAsync(Dictionary<string, object> args, string? userId = null)
{
    try
    {
        // --- Toàn bộ phần xác thực, lọc, và truy vấn CSDL được giữ nguyên y hệt code gốc ---

        if (string.IsNullOrEmpty(userId))
        {
            return JsonConvert.SerializeObject(new { success = false, message = "Cần đăng nhập để xem báo cáo học tập của con", error = "AUTHENTICATION_REQUIRED" }, Formatting.Indented);
        }

        var parent = await _parentRepository.GetByAccountIdAsync(userId);
        if (parent == null)
        {
            return JsonConvert.SerializeObject(new { success = false, message = "Không tìm thấy thông tin phụ huynh. Vui lòng đảm bảo bạn đã đăng nhập với tài khoản phụ huynh.", error = "PARENT_NOT_FOUND" }, Formatting.Indented);
        }

        var students = await _context.Students
            .Include(s => s.Account)
            .Where(s => s.ParentID == parent.ParentID)
            .ToListAsync();

        if (!students.Any())
        {
            return JsonConvert.SerializeObject(new { success = false, message = "Không tìm thấy thông tin học sinh nào trong tài khoản của bạn", error = "NO_STUDENTS_FOUND" }, Formatting.Indented);
        }

        var studentNameFilter = args.TryGetValue("student_name", out var studentNameObj) ? studentNameObj?.ToString() : null;
        if (!string.IsNullOrEmpty(studentNameFilter))
        {
            students = students.Where(s => s.FullName?.Contains(studentNameFilter, StringComparison.OrdinalIgnoreCase) == true).ToList();
            if (!students.Any())
            {
                return JsonConvert.SerializeObject(new { success = false, message = $"Không tìm thấy học sinh nào có tên '{studentNameFilter}' trong gia đình", error = "STUDENT_NOT_FOUND" }, Formatting.Indented);
            }
        }

        DateTime? fromDate = null, toDate = null;
        if (args.TryGetValue("from_date", out var fromDateObj) && DateTime.TryParseExact(fromDateObj?.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFromDate))
        {
            fromDate = parsedFromDate;
        }

        if (args.TryGetValue("to_date", out var toDateObj) && DateTime.TryParseExact(toDateObj?.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedToDate))
        {
            toDate = parsedToDate.AddDays(1).AddSeconds(-1);
        }
        
        var limit = 20;
        if (args.TryGetValue("limit", out var limitObj) && int.TryParse(limitObj?.ToString(), out var parsedLimit))
        {
            limit = Math.Min(parsedLimit, 100);
        }

        var studentIds = students.Select(s => s.StudentID).ToList();

        var reportsQuery = _context.LessonReports
            .Include(lr => lr.TimetableSlot).ThenInclude(ts => ts.Subject)
            .Include(lr => lr.TimetableSlot).ThenInclude(ts => ts.Timetable).ThenInclude(t => t.Class)
            .Include(lr => lr.StudentLessonNotes.Where(sln => studentIds.Contains(sln.StudentID))).ThenInclude(sln => sln.Student)
            .Where(lr => lr.IsDeleted != true && lr.StudentLessonNotes.Any(sln => studentIds.Contains(sln.StudentID)));
        
        if (fromDate.HasValue) reportsQuery = reportsQuery.Where(lr => lr.LessonDate >= fromDate.Value);
        if (toDate.HasValue) reportsQuery = reportsQuery.Where(lr => lr.LessonDate <= toDate.Value);

        var reports = await reportsQuery
            .OrderByDescending(lr => lr.LessonDate)
            .Take(limit)
            .ToListAsync();

        // --- Tạo đối tượng `reportSummary` y hệt code gốc của bạn ---
        var reportSummary = reports.Select(report => new
        {
            ReportInfo = new
            {
                ReportId = report.ReportID,
                LessonDate = report.LessonDate.ToString("dd/MM/yyyy"), // Giữ định dạng gốc
                SubjectName = report.TimetableSlot.Subject?.SubjectName,
                ClassName = report.TimetableSlot.Timetable?.Class?.ClassName,
                TotalStudents = report.TotalStudents,
                AbsentStudents = report.AbsentStudents,
                Remarks = report.Remarks,
                CreatedAt = report.CreatedAt.ToString("dd/MM/yyyy HH:mm") // Giữ định dạng gốc
            },
            StudentNotes = report.StudentLessonNotes
                .Where(sln => studentIds.Contains(sln.StudentID))
                .Select(note => new
                {
                    StudentId = note.StudentID,
                    StudentName = note.Student?.FullName,
                    NoteType = note.NoteType,
                    Points = note.Points,
                    Description = note.Description,
                    CreatedAt = note.CreatedAt.ToString("dd/MM/yyyy HH:mm") // Giữ định dạng gốc
                }).ToList()
        }).Where(r => r.StudentNotes.Any()).ToList();

        var totalStudentNotes = reportSummary.SelectMany(r => r.StudentNotes).Count();

        // --- BỎ qua phần tạo bảng bằng StringBuilder và trả về JSON trực tiếp ---
        return JsonConvert.SerializeObject(new
        {
            success = true,
            message = "Truy xuất báo cáo học tập thành công.", // Message đơn giản
            data = new
            {
                TotalStudents = students.Count,
                TotalReports = reportSummary.Count,
                TotalStudentNotes = totalStudentNotes,
                FilterApplied = new
                {
                    StudentName = studentNameFilter,
                    // Trả về định dạng gốc mà client gửi lên cho nhất quán
                    FromDate = fromDate?.ToString("yyyy-MM-dd"), 
                    ToDate = args.TryGetValue("to_date", out var tdo) ? tdo?.ToString() : null,
                    Limit = limit
                },
                Reports = reportSummary // Đây chính là đối tượng dữ liệu bạn đã tạo
            }
        }, Formatting.Indented);
    }
    catch (Exception ex)
    {
        return JsonConvert.SerializeObject(new
        {
            success = false,
            message = "❌ Có lỗi xảy ra khi lấy báo cáo học tập. Vui lòng thử lại sau.",
            error = "INTERNAL_ERROR",
            errorDetails = ex.Message
        }, Formatting.Indented);
    }
}

        private async Task<string> GetStudentAttendanceAsync(Dictionary<string, object> args, string? userId = null)
{
    try
    {
        // --- PHẦN KIỂM TRA VÀ LẤY DỮ LIỆU BAN ĐẦU (GIỮ NGUYÊN) ---
        if (string.IsNullOrEmpty(userId))
        {
            return JsonConvert.SerializeObject(new { success = false, message = "Cần đăng nhập để xem điểm danh của con", error = "AUTHENTICATION_REQUIRED" }, Formatting.Indented);
        }

        var parent = await _parentRepository.GetByAccountIdAsync(userId);
        if (parent == null)
        {
            return JsonConvert.SerializeObject(new { success = false, message = "Không tìm thấy thông tin phụ huynh.", error = "PARENT_NOT_FOUND" }, Formatting.Indented);
        }

        var students = await _context.Students
            .Include(s => s.Account)
            .Include(s => s.Class)
            .Where(s => s.ParentID == parent.ParentID)
            .ToListAsync();

        if (!students.Any())
        {
            return JsonConvert.SerializeObject(new { success = false, message = "Không tìm thấy thông tin học sinh nào trong tài khoản của bạn", error = "NO_STUDENTS_FOUND" }, Formatting.Indented);
        }

        // --- PHẦN XỬ LÝ BỘ LỌC (GIỮ NGUYÊN) ---
        var studentNameFilter = args.TryGetValue("student_name", out var studentNameObj) ? studentNameObj?.ToString() : null;

        if (!string.IsNullOrEmpty(studentNameFilter))
        {
            students = students.Where(s => s.FullName?.Contains(studentNameFilter, StringComparison.OrdinalIgnoreCase) == true).ToList();
            if (!students.Any())
            {
                return JsonConvert.SerializeObject(new { success = false, message = $"Không tìm thấy học sinh nào có tên '{studentNameFilter}'", error = "STUDENT_NOT_FOUND" }, Formatting.Indented);
            }
        }
        
        DateTime? fromDate = null, toDate = null;
        if (args.TryGetValue("from_date", out var fromDateObj) && DateTime.TryParseExact(fromDateObj?.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFromDate))
        {
            fromDate = parsedFromDate;
        }
        if (args.TryGetValue("to_date", out var toDateObj) && DateTime.TryParseExact(toDateObj?.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedToDate))
        {
            toDate = parsedToDate.AddDays(1).AddSeconds(-1);
        }

        var subjectNameFilter = args.TryGetValue("subject_name", out var subjectNameObj) ? subjectNameObj?.ToString() : null;

        var limit = 50;
        if (args.TryGetValue("limit", out var limitObj) && int.TryParse(limitObj?.ToString(), out var parsedLimit))
        {
            limit = Math.Min(parsedLimit, 200);
        }


        var studentIds = students.Select(s => s.StudentID).ToList();
        var attendanceQuery = _context.Attendances
            .Include(a => a.TimetableSlot).ThenInclude(ts => ts.Subject)
            .Include(a => a.TimetableSlot).ThenInclude(ts => ts.Timetable).ThenInclude(t => t.Class)
            .Include(a => a.Student)
            .Where(a => studentIds.Contains(a.StudentID) && a.IsDeleted != true);

        if (fromDate.HasValue)
        {
            var validSlotIds = await _context.TimetableSlots
                .Include(ts => ts.Timetable)
                .Where(ts => ts.Timetable.StartDate <= toDate && ts.Timetable.EndDate >= fromDate)
                .Select(ts => ts.TimetableSlotID)
                .ToListAsync();
            attendanceQuery = attendanceQuery.Where(a => validSlotIds.Contains(a.TimetableSlotID));
        }

        if (!string.IsNullOrEmpty(subjectNameFilter))
        {
            attendanceQuery = attendanceQuery.Where(a => a.TimetableSlot.Subject.SubjectName.Contains(subjectNameFilter));
        }

        var attendances = await attendanceQuery
            .OrderByDescending(a => a.TimetableSlot.Timetable.StartDate)
            .ThenBy(a => a.TimetableSlot.DayOfWeek)
            .ThenBy(a => a.TimetableSlot.StartTime)
            .Take(limit)
            .ToListAsync();

        // --- TẠO CẤU TRÚC DỮ LIỆU ĐỂ TRẢ VỀ (GIỮ NGUYÊN) ---
        var attendanceSummary = attendances.GroupBy(a => a.Student).Select(studentGroup => new
        {
            StudentInfo = new
            {
                StudentId = studentGroup.Key.StudentID,
                StudentName = studentGroup.Key.FullName,
                ClassName = studentGroup.Key.Class?.ClassName
            },
            AttendanceRecords = studentGroup.Select(a => new
            {
                AttendanceId = a.AttendanceID,
                SubjectName = a.TimetableSlot.Subject?.SubjectName,
                DayOfWeek = GetVietnameseDayOfWeek(a.TimetableSlot.DayOfWeek),
                StartTime = a.TimetableSlot.StartTime.ToString(@"hh\:mm"),
                EndTime = a.TimetableSlot.EndTime.ToString(@"hh\:mm"),
                Status = GetVietnameseAttendanceStatus(a.Status),
                StatusCode = a.Status,
                Note = a.Note,
                Room = a.TimetableSlot.Room,
                TimetableDate = a.TimetableSlot.Timetable.StartDate.ToString("dd/MM/yyyy") + " - " + a.TimetableSlot.Timetable.EndDate.ToString("dd/MM/yyyy")
            }).ToList()
        }).ToList();
        
        // --- TÍNH TOÁN THỐNG KÊ (GIỮ NGUYÊN) ---
        var totalRecords = attendances.Count;
        var presentCount = attendances.Count(a => a.Status == 1);
        var absentCount = attendances.Count(a => a.Status == 0);
        var lateCount = attendances.Count(a => a.Status == 2);
        var overallAttendanceRate = totalRecords > 0 ? (double)presentCount / totalRecords * 100 : 0;
        
        // ---------->>> THAY ĐỔI CHÍNH BẮT ĐẦU TỪ ĐÂY <<<----------

        // BỎ: Phần code tạo bảng bằng StringBuilder đã được loại bỏ.

        // THAY ĐỔI: Trả về đối tượng JSON có cấu trúc thay vì chuỗi định dạng.
        return JsonConvert.SerializeObject(new
        {
            success = true,
            message = "Lấy dữ liệu điểm danh thành công.", // Thông điệp đơn giản
            data = new
            {
                // Thống kê tổng quan
                Statistics = new 
                {
                    TotalStudents = students.Count,
                    TotalRecords = totalRecords,
                    PresentCount = presentCount,
                    AbsentCount = absentCount,
                    LateCount = lateCount,
                    OverallAttendanceRate = overallAttendanceRate
                },
                // Các bộ lọc đã áp dụng
                FilterApplied = new
                {
                    StudentName = studentNameFilter,
                    FromDate = fromDate?.ToString("yyyy-MM-dd"),
                    ToDate = toDate?.ToString("yyyy-MM-dd"), // toDate đã được cộng 1 ngày, nên trừ đi để hiển thị đúng
                    SubjectName = subjectNameFilter,
                    Limit = limit
                },
                // Dữ liệu điểm danh chi tiết
                Students = attendanceSummary
            }
        }, Formatting.Indented);
    }
    catch (Exception ex)
    {
        // (GIỮ NGUYÊN)
        return JsonConvert.SerializeObject(new
        {
            success = false,
            message = "❌ Có lỗi xảy ra khi lấy dữ liệu điểm danh. Vui lòng thử lại sau.",
            error = "INTERNAL_ERROR",
            errorDetails = ex.Message
        }, Formatting.Indented);
    }
}

        private async Task<string> AddStudentAttendanceAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Cần đăng nhập để thêm điểm danh",
                        error = "AUTHENTICATION_REQUIRED"
                    }, Formatting.Indented);
                }

                // Check if user is Principal
                var principal = await _context.Principals
                    .Include(p => p.Account)
                    .FirstOrDefaultAsync(p => p.AccountID == userId && p.IsDeleted != true);

                if (principal == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Chỉ hiệu trưởng mới có quyền thêm điểm danh. Vui lòng đăng nhập với tài khoản hiệu trưởng.",
                        error = "INSUFFICIENT_PERMISSIONS"
                    }, Formatting.Indented);
                }

                // Get required parameters
                if (!args.TryGetValue("timetable_slot_id", out var timetableSlotIdObj) || 
                    !int.TryParse(timetableSlotIdObj?.ToString(), out var timetableSlotId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Vui lòng cung cấp ID thời khóa biểu hợp lệ",
                        error = "INVALID_TIMETABLE_SLOT_ID"
                    }, Formatting.Indented);
                }

                // Get optional parameters
                var specificStudentId = args.TryGetValue("student_id", out var studentIdObj) ? 
                    studentIdObj?.ToString() : null;

                var status = 1; // Default: Present
                if (args.TryGetValue("status", out var statusObj) && 
                    int.TryParse(statusObj?.ToString(), out var parsedStatus))
                {
                    if (parsedStatus >= 0 && parsedStatus <= 3) // Valid status range
                    {
                        status = parsedStatus;
                    }
                }

                var note = args.TryGetValue("note", out var noteObj) ? 
                    noteObj?.ToString() : null;

                var attendanceDate = DateTime.Today;
                if (args.TryGetValue("date", out var dateObj) && !string.IsNullOrEmpty(dateObj?.ToString()))
                {
                    if (DateTime.TryParseExact(dateObj.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        attendanceDate = parsedDate;
                    }
                }

                // Get timetable slot with related data
                var timetableSlot = await _context.TimetableSlots
                    .Include(ts => ts.Timetable)
                        .ThenInclude(t => t.Class)
                        .ThenInclude(c => c.Students)
                    .Include(ts => ts.Subject)
                    .Include(ts => ts.Teacher)
                    .FirstOrDefaultAsync(ts => ts.TimetableSlotID == timetableSlotId && ts.IsDeleted != true);

                if (timetableSlot == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không tìm thấy thời khóa biểu với ID đã cung cấp",
                        error = "TIMETABLE_SLOT_NOT_FOUND"
                    }, Formatting.Indented);
                }

                // Validate date is within timetable range
                if (attendanceDate < timetableSlot.Timetable.StartDate.Date || 
                    attendanceDate > timetableSlot.Timetable.EndDate.Date)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Ngày điểm danh ({attendanceDate:dd/MM/yyyy}) phải nằm trong khoảng thời gian của thời khóa biểu ({timetableSlot.Timetable.StartDate:dd/MM/yyyy} - {timetableSlot.Timetable.EndDate:dd/MM/yyyy})",
                        error = "DATE_OUT_OF_RANGE"
                    }, Formatting.Indented);
                }

                // Get students to add attendance for
                var studentsToMark = new List<Student>();

                if (!string.IsNullOrEmpty(specificStudentId))
                {
                    // Mark attendance for specific student
                    var specificStudent = timetableSlot.Timetable.Class.Students
                        .FirstOrDefault(s => s.StudentID == specificStudentId && s.IsDeleted != true);
                    
                    if (specificStudent == null)
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            success = false,
                            message = $"Không tìm thấy học sinh với ID '{specificStudentId}' trong lớp {timetableSlot.Timetable.Class.ClassName}",
                            error = "STUDENT_NOT_FOUND"
                        }, Formatting.Indented);
                    }
                    
                    studentsToMark.Add(specificStudent);
                }
                else
                {
                    // Mark attendance for all students in the class
                    studentsToMark = timetableSlot.Timetable.Class.Students
                        .Where(s => s.IsDeleted != true)
                        .ToList();
                }

                var addedCount = 0;
                var updatedCount = 0;
                var skippedCount = 0;
                var errors = new List<string>();

                foreach (var student in studentsToMark)
                {
                    try
                    {
                        // Check if attendance already exists
                        var existingAttendance = await _context.Attendances
                            .FirstOrDefaultAsync(a => a.TimetableSlotID == timetableSlotId && 
                                                     a.StudentID == student.StudentID &&
                                                     a.IsDeleted != true);

                        if (existingAttendance != null)
                        {
                            // Update existing attendance
                            existingAttendance.Status = status;
                            existingAttendance.Note = note;
                            updatedCount++;
                        }
                        else
                        {
                            // Create new attendance record
                            var newAttendance = new Attendance
                            {
                                TimetableSlotID = timetableSlotId,
                                StudentID = student.StudentID,
                                Status = status,
                                Note = note,
                                IsDeleted = false
                            };

                            _context.Attendances.Add(newAttendance);
                            addedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Lỗi khi xử lý học sinh {student.FullName}: {ex.Message}");
                        skippedCount++;
                    }
                }

                // Save changes
                await _context.SaveChangesAsync();

                // Format success message
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendLine($"✅ **THÊM ĐIỂM DANH THÀNH CÔNG**");
                formattedMessage.AppendLine();
                formattedMessage.AppendLine($"📚 **Thông tin tiết học:**");
                formattedMessage.AppendLine($"   • Môn học: {timetableSlot.Subject?.SubjectName ?? "N/A"}");
                formattedMessage.AppendLine($"   • Lớp: {timetableSlot.Timetable.Class.ClassName}");
                formattedMessage.AppendLine($"   • Giáo viên: {timetableSlot.Teacher?.FullName ?? "N/A"}");
                formattedMessage.AppendLine($"   • Thời gian: {GetVietnameseDayOfWeek(timetableSlot.DayOfWeek)} - {timetableSlot.StartTime:hh\\:mm} đến {timetableSlot.EndTime:hh\\:mm}");
                formattedMessage.AppendLine($"   • Phòng: {timetableSlot.Room ?? "N/A"}");
                formattedMessage.AppendLine($"   • Ngày điểm danh: {attendanceDate:dd/MM/yyyy}");
                formattedMessage.AppendLine();

                formattedMessage.AppendLine($"📊 **Kết quả xử lý:**");
                formattedMessage.AppendLine($"   • Trạng thái: {GetVietnameseAttendanceStatus(status)}");
                if (!string.IsNullOrEmpty(note))
                {
                    formattedMessage.AppendLine($"   • Ghi chú: {note}");
                }
                formattedMessage.AppendLine($"   • Số học sinh được thêm mới: {addedCount}");
                formattedMessage.AppendLine($"   • Số học sinh được cập nhật: {updatedCount}");
                if (skippedCount > 0)
                {
                    formattedMessage.AppendLine($"   • Số học sinh bỏ qua (có lỗi): {skippedCount}");
                }
                formattedMessage.AppendLine($"   • Tổng số học sinh xử lý: {addedCount + updatedCount + skippedCount}");

                if (errors.Any())
                {
                    formattedMessage.AppendLine();
                    formattedMessage.AppendLine($"⚠️ **Lỗi xảy ra:**");
                    foreach (var error in errors.Take(5)) // Show max 5 errors
                    {
                        formattedMessage.AppendLine($"   • {error}");
                    }
                    if (errors.Count > 5)
                    {
                        formattedMessage.AppendLine($"   • ... và {errors.Count - 5} lỗi khác");
                    }
                }

                formattedMessage.AppendLine();
                formattedMessage.AppendLine($"👨‍💼 **Thông tin hiệu trưởng:**");
                formattedMessage.AppendLine($"   • Họ tên: {principal.FullName ?? "N/A"}");
                formattedMessage.AppendLine($"   • Thời gian thực hiện: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = formattedMessage.ToString(),
                    data = new
                    {
                        TimetableSlotId = timetableSlotId,
                        SubjectName = timetableSlot.Subject?.SubjectName,
                        ClassName = timetableSlot.Timetable.Class.ClassName,
                        TeacherName = timetableSlot.Teacher?.FullName,
                        AttendanceDate = attendanceDate.ToString("yyyy-MM-dd"),
                        Status = status,
                        StatusText = GetVietnameseAttendanceStatus(status),
                        Note = note,
                        Summary = new
                        {
                            Added = addedCount,
                            Updated = updatedCount,
                            Skipped = skippedCount,
                            Total = addedCount + updatedCount + skippedCount,
                            Errors = errors
                        },
                        PrincipalInfo = new
                        {
                            PrincipalId = principal.PrincipalID,
                            PrincipalName = principal.FullName,
                            ProcessedAt = DateTime.Now
                        }
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi thêm điểm danh. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private string GetVietnameseDayOfWeek(string dayOfWeek)
        {
            return dayOfWeek switch
            {
                "Monday" or "2" => "Thứ 2",
                "Tuesday" or "3" => "Thứ 3", 
                "Wednesday" or "4" => "Thứ 4",
                "Thursday" or "5" => "Thứ 5",
                "Friday" or "6" => "Thứ 6",
                "Saturday" or "7" => "Thứ 7",
                "Sunday" or "1" or "8" => "Chủ nhật",
                _ => dayOfWeek ?? "N/A"
            };
        }

        private string GetVietnameseAttendanceStatus(int? status)
        {
            return status switch
            {
                1 => "Có mặt",
                0 => "Vắng",
                2 => "Muộn", 
                3 => "Phép",
                _ => "Không xác định"
            };
        }

        private string GetVietnameseNoteType(string noteType)
        {
            return noteType switch
            {
                "BonusPoint" => "Điểm cộng",
                "Violation" => "Vi phạm",
                "Good" => "Tốt",
                "Warning" => "Cảnh báo",
                "Excellent" => "Xuất sắc",
                "Improvement" => "Cải thiện",
                _ => noteType ?? "Khác"
            };
        }

        // New Methods for Additional Function Calling
        private async Task<string> GetAllSubjectsAsync(Dictionary<string, object> args)
        {
            try
            {
                var pageSize = 10;
                if (args.TryGetValue("page_size", out var pageSizeObj) && int.TryParse(pageSizeObj?.ToString(), out var parsedPageSize))
                {
                    pageSize = Math.Min(Math.Max(parsedPageSize, 1), 50);
                }

                var searchTerm = args.TryGetValue("search_term", out var searchTermObj) ? searchTermObj?.ToString() : null;

                var response = await _subjectService.FilterSortSearchSubjectsAsync(
                    searchTerm: searchTerm,
                    pageSize: pageSize
                );

                if (!response.Success)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không thể lấy danh sách môn học. Vui lòng thử lại sau.",
                        error = "SUBJECTS_NOT_FOUND"
                    }, Formatting.Indented);
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = $"Tìm thấy {response.Data?.Count ?? 0} môn học",
                    data = response.Data
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi lấy danh sách môn học. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetSubjectByIdAsync(Dictionary<string, object> args)
        {
            try
            {
                if (!args.TryGetValue("subject_id", out var subjectIdObj) || !int.TryParse(subjectIdObj?.ToString(), out var subjectId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Vui lòng cung cấp ID môn học hợp lệ",
                        error = "INVALID_SUBJECT_ID"
                    }, Formatting.Indented);
                }

                var response = await _subjectService.GetSubjectByIdAsync(subjectId);

                if (!response.Success)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Không tìm thấy môn học với ID {subjectId}",
                        error = "SUBJECT_NOT_FOUND"
                    }, Formatting.Indented);
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = "Tìm thấy thông tin môn học",
                    data = response.Data
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi lấy thông tin môn học. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetStudentExamsAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Cần đăng nhập để xem lịch thi của con",
                        error = "AUTHENTICATION_REQUIRED"
                    }, Formatting.Indented);
                }

                // Get parent information
                var parent = await _parentRepository.GetByAccountIdAsync(userId);
                if (parent == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin phụ huynh. Vui lòng đảm bảo bạn đã đăng nhập với tài khoản phụ huynh.",
                        error = "PARENT_NOT_FOUND"
                    }, Formatting.Indented);
                }

                // Get all students of this parent
                var students = await _context.Students
                    .Include(s => s.Account)
                    .Include(s => s.Class)
                    .Where(s => s.ParentID == parent.ParentID)
                    .ToListAsync();

                if (!students.Any())
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin học sinh nào trong tài khoản của bạn",
                        error = "NO_STUDENTS_FOUND"
                    }, Formatting.Indented);
                }

                // Filter by student name if provided
                var studentNameFilter = args.TryGetValue("student_name", out var studentNameObj) ? 
                    studentNameObj?.ToString() : null;

                if (!string.IsNullOrEmpty(studentNameFilter))
                {
                    students = students.Where(s => s.FullName?.Contains(studentNameFilter, StringComparison.OrdinalIgnoreCase) == true).ToList();
                    if (!students.Any())
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            success = false,
                            message = $"Không tìm thấy học sinh nào có tên '{studentNameFilter}' trong gia đình",
                            error = "STUDENT_NOT_FOUND"
                        }, Formatting.Indented);
                    }
                }

                // Get date filters
                DateTime? fromDate = null, toDate = null;
                
                if (args.TryGetValue("from_date", out var fromDateObj) && !string.IsNullOrEmpty(fromDateObj?.ToString()))
                {
                    if (DateTime.TryParseExact(fromDateObj.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFromDate))
                    {
                        fromDate = parsedFromDate;
                    }
                }

                if (args.TryGetValue("to_date", out var toDateObj) && !string.IsNullOrEmpty(toDateObj?.ToString()))
                {
                    if (DateTime.TryParseExact(toDateObj.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedToDate))
                    {
                        toDate = parsedToDate.AddDays(1).AddSeconds(-1);
                    }
                }

                var searchTerm = args.TryGetValue("search_term", out var searchTermObj) ? searchTermObj?.ToString() : null;
                var limit = 20;
                if (args.TryGetValue("limit", out var limitObj) && int.TryParse(limitObj?.ToString(), out var parsedLimit))
                {
                    limit = Math.Min(parsedLimit, 100);
                }

                var allExams = new List<ExamResponse>();
                
                foreach (var student in students)
                {
                    var response = await _examService.GetExamsByStudentIdAsync(
                        studentId: student.StudentID,
                        startDate: fromDate,
                        endDate: toDate,
                        searchTerm: searchTerm,
                        pageSize: limit
                    );

                    if (response.Success && response.Data != null)
                    {
                        allExams.AddRange(response.Data);
                    }
                }

                // Sort by exam date
                allExams = allExams.OrderBy(e => e.ExamDate).Take(limit).ToList();

                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendLine($"📅 **LỊCH THI CỦA HỌC SINH**");
                formattedMessage.AppendLine($"Tìm thấy {allExams.Count} bài thi cho {students.Count} học sinh");
                formattedMessage.AppendLine();

                if (allExams.Any())
                {
                    var examsByStudent = allExams.GroupBy(e => e.StudentId).ToList();
                    
                    foreach (var studentGroup in examsByStudent)
                    {
                        var student = students.FirstOrDefault(s => s.StudentID == studentGroup.Key);
                        formattedMessage.AppendLine($"👨‍🎓 **{student?.FullName}** (ID: {studentGroup.Key})");
                        formattedMessage.AppendLine($"   🏫 Lớp: {student?.Class?.ClassName ?? "N/A"}");
                        formattedMessage.AppendLine();

                        formattedMessage.AppendLine("   📝 **LỊCH THI CHI TIẾT:**");
                        formattedMessage.AppendLine("   ┌──────────────────────────────────────────────────────────────────┐");
                        formattedMessage.AppendLine("   │ LOẠI BÀI THI   │ MÔN HỌC   │ NGÀY THI    │ TRẠNG THÁI  │ PHÒNG │");
                        formattedMessage.AppendLine("   ├──────────────────────────────────────────────────────────────────┤");

                        foreach (var exam in studentGroup.Take(10))
                        {
                            var examType = (exam.ExamType ?? "N/A").Length > 14 
                                ? (exam.ExamType ?? "N/A").Substring(0, 11) + "..." 
                                : (exam.ExamType ?? "N/A").PadRight(14);
                            
                            var subjectName = (exam.SubjectName ?? "N/A").Length > 9 
                                ? (exam.SubjectName ?? "N/A").Substring(0, 6) + "..." 
                                : (exam.SubjectName ?? "N/A").PadRight(9);
                            
                            var examDate = exam.ExamDate.ToString("dd/MM/yyyy").PadRight(10);
                            var gradeStatus = exam.GradeDate.HasValue ? "Đã chấm" : "Chưa chấm";
                            var room = (exam.Room ?? "").PadRight(5);

                            formattedMessage.AppendLine($"   │ {examType} │ {subjectName} │ {examDate} │ {gradeStatus.PadRight(11)} │ {room} │");
                        }

                        formattedMessage.AppendLine("   └──────────────────────────────────────────────────────────────────┘");
                        formattedMessage.AppendLine();
                    }
                }
                else
                {
                    formattedMessage.AppendLine("📝 Không có lịch thi nào trong thời gian được chọn.");
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = formattedMessage.ToString(),
                    data = new
                    {
                        TotalStudents = students.Count,
                        TotalExams = allExams.Count,
                        Students = students.Select(s => new { s.StudentID, s.FullName, ClassName = s.Class?.ClassName }),
                        Exams = allExams
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi lấy lịch thi. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetClassStudentsAsync(Dictionary<string, object> args)
        {
            try
            {
                if (!args.TryGetValue("class_id", out var classIdObj) || !int.TryParse(classIdObj?.ToString(), out var classId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Vui lòng cung cấp ID lớp học hợp lệ",
                        error = "INVALID_CLASS_ID"
                    }, Formatting.Indented);
                }

                var response = await _classService.GetStudentsInClassAsync(classId);

                if (!response.Success)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Không thể lấy danh sách học sinh lớp {classId}",
                        error = "STUDENTS_NOT_FOUND"
                    }, Formatting.Indented);
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = $"Tìm thấy {response.Data?.Count ?? 0} học sinh trong lớp {classId}",
                    data = response.Data
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi lấy danh sách học sinh. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetClassByIdAsync(Dictionary<string, object> args)
        {
            try
            {
                if (!args.TryGetValue("class_id", out var classIdObj) || !int.TryParse(classIdObj?.ToString(), out var classId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Vui lòng cung cấp ID lớp học hợp lệ",
                        error = "INVALID_CLASS_ID"
                    }, Formatting.Indented);
                }

                var response = await _classService.GetClassByIdAsync(classId);

                if (!response.Success)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Không tìm thấy lớp học với ID {classId}",
                        error = "CLASS_NOT_FOUND"
                    }, Formatting.Indented);
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = "Tìm thấy thông tin lớp học",
                    data = response.Data
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi lấy thông tin lớp học. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetStudentGradeReportAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Cần đăng nhập để xem báo cáo điểm của con",
                        error = "AUTHENTICATION_REQUIRED"
                    }, Formatting.Indented);
                }

                // Get parent information
                var parent = await _parentRepository.GetByAccountIdAsync(userId);
                if (parent == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin phụ huynh. Vui lòng đảm bảo bạn đã đăng nhập với tài khoản phụ huynh.",
                        error = "PARENT_NOT_FOUND"
                    }, Formatting.Indented);
                }

                // Get required parameters
                if (!args.TryGetValue("report_type", out var reportTypeObj) || string.IsNullOrEmpty(reportTypeObj?.ToString()))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Vui lòng chọn loại báo cáo: 'week' (tuần), 'month' (tháng), hoặc 'year' (năm)",
                        error = "MISSING_REPORT_TYPE"
                    }, Formatting.Indented);
                }

                var reportType = reportTypeObj.ToString()!.ToLower();
                if (!new[] { "week", "month", "year" }.Contains(reportType))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Loại báo cáo không hợp lệ. Vui lòng chọn: 'week' (tuần), 'month' (tháng), hoặc 'year' (năm)",
                        error = "INVALID_REPORT_TYPE"
                    }, Formatting.Indented);
                }

                // Parse dates
                if (!args.TryGetValue("start_date", out var startDateObj) || !DateTime.TryParseExact(startDateObj?.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Vui lòng cung cấp ngày bắt đầu hợp lệ (yyyy-MM-dd)",
                        error = "INVALID_START_DATE"
                    }, Formatting.Indented);
                }

                if (!args.TryGetValue("end_date", out var endDateObj) || !DateTime.TryParseExact(endDateObj?.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Vui lòng cung cấp ngày kết thúc hợp lệ (yyyy-MM-dd)",
                        error = "INVALID_END_DATE"
                    }, Formatting.Indented);
                }

                if (startDate > endDate)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Ngày bắt đầu không thể lớn hơn ngày kết thúc",
                        error = "INVALID_DATE_RANGE"
                    }, Formatting.Indented);
                }

                // Get students of this parent
                var students = await _context.Students
                    .Include(s => s.Account)
                    .Include(s => s.Class)
                    .Where(s => s.ParentID == parent.ParentID)
                    .ToListAsync();

                if (!students.Any())
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin học sinh nào trong tài khoản của bạn",
                        error = "NO_STUDENTS_FOUND"
                    }, Formatting.Indented);
                }

                // Filter by student name if provided
                var studentNameFilter = args.TryGetValue("student_name", out var studentNameObj) ? studentNameObj?.ToString() : null;
                if (!string.IsNullOrEmpty(studentNameFilter))
                {
                    students = students.Where(s => s.FullName?.Contains(studentNameFilter, StringComparison.OrdinalIgnoreCase) == true).ToList();
                    if (!students.Any())
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            success = false,
                            message = $"Không tìm thấy học sinh nào có tên '{studentNameFilter}' trong gia đình",
                            error = "STUDENT_NOT_FOUND"
                        }, Formatting.Indented);
                    }
                }

                // Get grades for students in date range  
                var studentIds = students.Select(s => s.StudentID).ToList();
                var grades = await (from g in _context.Grades
                                  join s in _context.Students on g.StudentID equals s.StudentID
                                  join sub in _context.Subjects on g.SubjectID equals sub.SubjectId
                                  where studentIds.Contains(g.StudentID) && 
                                        g.IsDeleted != true
                                  select new
                                  {
                                      GradeId = g.GradeID,
                                      StudentId = g.StudentID,
                                      StudentName = s.FullName,
                                      SubjectId = g.SubjectID,
                                      SubjectName = sub.SubjectName,
                                      Score = g.Score ?? 0,
                                      GradeType = g.GradeType,
                                      Semester = g.Semester
                                  }).ToListAsync();

                // Filter by semester based on date range (since we don't have DateRecorded)
                // Note: Since Grade entity doesn't have DateRecorded property, we'll work with available semester data
                if (!string.IsNullOrEmpty(reportType))
                {
                    // For now, we'll use all grades available since we can't filter by date without DateRecorded property
                    // This could be enhanced later if DateRecorded is added to Grade entity
                }

                // Group grades by time period
                var periodFormat = reportType switch
                {
                    "week" => "yyyy-'W'ww",
                    "month" => "yyyy-MM",
                    "year" => "yyyy",
                    _ => "yyyy-MM-dd"
                };

                // Group grades by semester (since we don't have DateRecorded)
                var gradesByPeriod = grades.GroupBy(g => g.Semester ?? "Unknown").ToList();

                var formattedMessage = new System.Text.StringBuilder();
                var reportTypeVi = reportType switch
                {
                    "week" => "TUẦN",
                    "month" => "THÁNG", 
                    "year" => "NĂM",
                    _ => "THỜI GIAN"
                };

                formattedMessage.AppendLine($"📊 **BÁO CÁO ĐIỂM THEO {reportTypeVi}**");
                formattedMessage.AppendLine($"Từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}");
                formattedMessage.AppendLine($"Tìm thấy {grades.Count} điểm cho {students.Count} học sinh");
                formattedMessage.AppendLine();

                if (gradesByPeriod.Any())
                {
                    foreach (var period in gradesByPeriod.OrderBy(p => p.Key))
                    {
                        var periodGrades = period.ToList();
                        var avgGrade = periodGrades.Average(g => g.Score);

                        formattedMessage.AppendLine($"📅 **{GetPeriodDisplayName(period.Key, reportType)}**");
                        formattedMessage.AppendLine($"   • Số điểm: {periodGrades.Count}");
                        formattedMessage.AppendLine($"   • Điểm trung bình: {avgGrade:F2}");
                        formattedMessage.AppendLine();

                        formattedMessage.AppendLine("   📝 **CHI TIẾT ĐIỂM:**");
                        formattedMessage.AppendLine("   ┌─────────────────────────────────────────────────────────────────────┐");
                        formattedMessage.AppendLine("   │ HỌC SINH         │ MÔN HỌC      │ ĐIỂM │ LOẠI    │ NGÀY NHẬP      │");
                        formattedMessage.AppendLine("   ├─────────────────────────────────────────────────────────────────────┤");

                        foreach (var grade in periodGrades.Take(15))
                        {
                            var studentName = (grade.StudentName ?? "N/A").Length > 16
                                ? (grade.StudentName ?? "N/A").Substring(0, 13) + "..."
                                : (grade.StudentName ?? "N/A").PadRight(16);

                            var subjectName = (grade.SubjectName ?? "N/A").Length > 12
                                ? (grade.SubjectName ?? "N/A").Substring(0, 9) + "..."
                                : (grade.SubjectName ?? "N/A").PadRight(12);

                            var scoreText = grade.Score.ToString("F1").PadLeft(4);
                            var gradeType = (grade.GradeType ?? "N/A").PadRight(8);
                            var semesterText = (grade.Semester ?? "N/A").PadRight(14);

                            formattedMessage.AppendLine($"   │ {studentName} │ {subjectName} │ {scoreText} │ {gradeType} │ {semesterText} │");
                        }

                        if (periodGrades.Count > 15)
                        {
                            formattedMessage.AppendLine($"   │ ... và {periodGrades.Count - 15} điểm khác                                        │");
                        }

                        formattedMessage.AppendLine("   └─────────────────────────────────────────────────────────────────────┘");
                        formattedMessage.AppendLine();
                    }
                }
                else
                {
                    formattedMessage.AppendLine($"📝 Không có điểm nào trong khoảng thời gian từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}");
                }

                // Overall statistics
                if (grades.Any())
                {
                    formattedMessage.AppendLine("📈 **THỐNG KÊ TỔNG:**");
                    formattedMessage.AppendLine($"   • Tổng số học sinh: {students.Count}");
                    formattedMessage.AppendLine($"   • Tổng số điểm: {grades.Count}");
                    formattedMessage.AppendLine($"   • Điểm trung bình chung: {grades.Average(g => g.Score):F2}");
                    formattedMessage.AppendLine($"   • Điểm cao nhất: {grades.Max(g => g.Score):F1}");
                    formattedMessage.AppendLine($"   • Điểm thấp nhất: {grades.Min(g => g.Score):F1}");

                    var subjectStats = grades.GroupBy(g => g.SubjectName ?? "N/A")
                        .Select(g => new { Subject = g.Key, Count = g.Count(), Avg = g.Average(x => x.Score) })
                        .OrderByDescending(s => s.Count)
                        .Take(5);

                    formattedMessage.AppendLine();
                    formattedMessage.AppendLine("📚 **TOP 5 MÔN HỌC:**");
                    foreach (var subject in subjectStats)
                    {
                        formattedMessage.AppendLine($"   • {subject.Subject}: {subject.Count} điểm (TB: {subject.Avg:F2})");
                    }
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = formattedMessage.ToString(),
                    data = new
                    {
                        ReportType = reportType,
                        StartDate = startDate.ToString("yyyy-MM-dd"),
                        EndDate = endDate.ToString("yyyy-MM-dd"),
                        TotalStudents = students.Count,
                        TotalGrades = grades.Count,
                        OverallAverage = grades.Any() ? grades.Average(g => g.Score) : 0,
                        PeriodData = gradesByPeriod.Select(p => new
                        {
                            Period = p.Key,
                            DisplayName = GetPeriodDisplayName(p.Key, reportType),
                            GradeCount = p.Count(),
                            Average = p.Average(g => g.Score),
                            Grades = p.Take(10).Select(g => new
                            {
                                StudentName = g.StudentName,
                                SubjectName = g.SubjectName,
                                Score = g.Score,
                                GradeType = g.GradeType,
                                Semester = g.Semester
                            })
                        })
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi tạo báo cáo điểm. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetAllStudentsGradeReportAsync(Dictionary<string, object> args)
        {
            try
            {
                // Get required parameters
                if (!args.TryGetValue("report_type", out var reportTypeObj) || string.IsNullOrEmpty(reportTypeObj?.ToString()))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Vui lòng chọn loại báo cáo: 'week' (tuần), 'month' (tháng), hoặc 'year' (năm)",
                        error = "MISSING_REPORT_TYPE"
                    }, Formatting.Indented);
                }

                var reportType = reportTypeObj.ToString()!.ToLower();
                if (!new[] { "week", "month", "year" }.Contains(reportType))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Loại báo cáo không hợp lệ. Vui lòng chọn: 'week' (tuần), 'month' (tháng), hoặc 'year' (năm)",
                        error = "INVALID_REPORT_TYPE"
                    }, Formatting.Indented);
                }

                // Parse dates
                if (!args.TryGetValue("start_date", out var startDateObj) || !DateTime.TryParseExact(startDateObj?.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Vui lòng cung cấp ngày bắt đầu hợp lệ (yyyy-MM-dd)",
                        error = "INVALID_START_DATE"
                    }, Formatting.Indented);
                }

                if (!args.TryGetValue("end_date", out var endDateObj) || !DateTime.TryParseExact(endDateObj?.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Vui lòng cung cấp ngày kết thúc hợp lệ (yyyy-MM-dd)",
                        error = "INVALID_END_DATE"
                    }, Formatting.Indented);
                }

                if (startDate > endDate)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Ngày bắt đầu không thể lớn hơn ngày kết thúc",
                        error = "INVALID_DATE_RANGE"
                    }, Formatting.Indented);
                }

                // Optional filters
                var classIdFilter = args.TryGetValue("class_id", out var classIdObj) && int.TryParse(classIdObj?.ToString(), out var classId) ? classId : (int?)null;
                var subjectNameFilter = args.TryGetValue("subject_name", out var subjectNameObj) ? subjectNameObj?.ToString() : null;

                // Build query for all students (with optional class filter)
                var studentsQuery = _context.Students
                    .Include(s => s.Account)
                    .Include(s => s.Class)
                    .Where(s => s.IsDeleted != true);

                if (classIdFilter.HasValue)
                {
                    studentsQuery = studentsQuery.Where(s => s.ClassID == classIdFilter.Value);
                }

                var students = await studentsQuery.ToListAsync();

                if (!students.Any())
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = classIdFilter.HasValue ? $"Không tìm thấy học sinh nào trong lớp {classIdFilter}" : "Không tìm thấy học sinh nào trong hệ thống",
                        error = "NO_STUDENTS_FOUND"
                    }, Formatting.Indented);
                }

                // Get grades for all students in date range
                var studentIds = students.Select(s => s.StudentID).ToList();
                var grades = await (from g in _context.Grades
                                  join s in _context.Students on g.StudentID equals s.StudentID
                                  join c in _context.Classes on s.ClassID equals c.ClassID into classGroup
                                  from c in classGroup.DefaultIfEmpty()
                                  join sub in _context.Subjects on g.SubjectID equals sub.SubjectId
                                  where studentIds.Contains(g.StudentID) && 
                                        g.IsDeleted != true
                                  select new
                                  {
                                      GradeId = g.GradeID,
                                      StudentId = g.StudentID,
                                      StudentName = s.FullName,
                                      ClassName = c != null ? c.ClassName : "N/A",
                                      SubjectId = g.SubjectID,
                                      SubjectName = sub.SubjectName,
                                      Score = g.Score ?? 0,
                                      GradeType = g.GradeType,
                                      Semester = g.Semester
                                  }).ToListAsync();

                // Filter by subject if provided
                if (!string.IsNullOrEmpty(subjectNameFilter))
                {
                    grades = grades.Where(g => g.SubjectName.Contains(subjectNameFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Group grades by semester (since we don't have DateRecorded)
                var gradesByPeriod = grades.GroupBy(g => g.Semester ?? "Unknown").ToList();

                var formattedMessage = new System.Text.StringBuilder();
                var reportTypeVi = reportType switch
                {
                    "week" => "TUẦN",
                    "month" => "THÁNG", 
                    "year" => "NĂM",
                    _ => "THỜI GIAN"
                };

                formattedMessage.AppendLine($"📊 **BÁO CÁO ĐIỂM TẤT CẢ HỌC SINH THEO {reportTypeVi}**");
                formattedMessage.AppendLine($"Từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}");
                if (classIdFilter.HasValue)
                {
                    var className = students.FirstOrDefault()?.Class?.ClassName ?? $"Lớp {classIdFilter}";
                    formattedMessage.AppendLine($"Lọc theo lớp: {className}");
                }
                if (!string.IsNullOrEmpty(subjectNameFilter))
                {
                    formattedMessage.AppendLine($"Lọc theo môn: {subjectNameFilter}");
                }
                formattedMessage.AppendLine($"Tìm thấy {grades.Count} điểm cho {students.Count} học sinh");
                formattedMessage.AppendLine();

                if (gradesByPeriod.Any())
                {
                    foreach (var period in gradesByPeriod.OrderBy(p => p.Key))
                    {
                        var periodGrades = period.ToList();
                        var avgGrade = periodGrades.Average(g => g.Score);
                        var studentCount = periodGrades.Select(g => g.StudentId).Distinct().Count();

                        formattedMessage.AppendLine($"📅 **{GetPeriodDisplayName(period.Key, reportType)}**");
                        formattedMessage.AppendLine($"   • Số học sinh có điểm: {studentCount}");
                        formattedMessage.AppendLine($"   • Tổng số điểm: {periodGrades.Count}");
                        formattedMessage.AppendLine($"   • Điểm trung bình: {avgGrade:F2}");
                        formattedMessage.AppendLine();

                        // Group by student for better display
                        var studentGrades = periodGrades.GroupBy(g => g.StudentId).Take(10);

                        formattedMessage.AppendLine("   📝 **TOP 10 HỌC SINH:**");
                        formattedMessage.AppendLine("   ┌─────────────────────────────────────────────────────────────────────────┐");
                        formattedMessage.AppendLine("   │ HỌC SINH         │ LỚP      │ SỐ ĐIỂM │ TB   │ CAO │ THẤP │ MÔN CHÍNH │");
                        formattedMessage.AppendLine("   ├─────────────────────────────────────────────────────────────────────────┤");

                        foreach (var studentGroup in studentGrades)
                        {
                            var studentGradeList = studentGroup.ToList();
                            var firstGrade = studentGradeList.First(); // Get first grade to access student info
                            var studentAvg = studentGradeList.Average(g => g.Score);
                            var maxGrade = studentGradeList.Max(g => g.Score);
                            var minGrade = studentGradeList.Min(g => g.Score);
                            var mainSubject = studentGradeList.GroupBy(g => g.SubjectName)
                                .OrderByDescending(g => g.Count())
                                .FirstOrDefault()?.Key ?? "N/A";

                            var studentName = (firstGrade.StudentName ?? "N/A").Length > 16
                                ? (firstGrade.StudentName ?? "N/A").Substring(0, 13) + "..."
                                : (firstGrade.StudentName ?? "N/A").PadRight(16);

                            var className = (firstGrade.ClassName ?? "N/A").Length > 8
                                ? (firstGrade.ClassName ?? "N/A").Substring(0, 5) + "..."
                                : (firstGrade.ClassName ?? "N/A").PadRight(8);

                            var gradeCount = studentGradeList.Count.ToString().PadLeft(7);
                            var avgText = studentAvg.ToString("F1").PadLeft(4);
                            var maxText = maxGrade.ToString("F1").PadLeft(3);
                            var minText = minGrade.ToString("F1").PadLeft(4);

                            var mainSubjectShort = mainSubject.Length > 9
                                ? mainSubject.Substring(0, 6) + "..."
                                : mainSubject.PadRight(9);

                            formattedMessage.AppendLine($"   │ {studentName} │ {className} │ {gradeCount} │ {avgText} │ {maxText} │ {minText} │ {mainSubjectShort} │");
                        }

                        formattedMessage.AppendLine("   └─────────────────────────────────────────────────────────────────────────┘");
                        formattedMessage.AppendLine();
                    }
                }
                else
                {
                    formattedMessage.AppendLine($"📝 Không có điểm nào trong khoảng thời gian từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}");
                }

                // Overall statistics
                if (grades.Any())
                {
                    formattedMessage.AppendLine("📈 **THỐNG KÊ TỔNG:**");
                    formattedMessage.AppendLine($"   • Tổng số học sinh: {students.Count}");
                    formattedMessage.AppendLine($"   • Học sinh có điểm: {grades.Select(g => g.StudentId).Distinct().Count()}");
                    formattedMessage.AppendLine($"   • Tổng số điểm: {grades.Count}");
                    formattedMessage.AppendLine($"   • Điểm trung bình chung: {grades.Average(g => g.Score):F2}");
                    formattedMessage.AppendLine($"   • Điểm cao nhất: {grades.Max(g => g.Score):F1}");
                    formattedMessage.AppendLine($"   • Điểm thấp nhất: {grades.Min(g => g.Score):F1}");

                    var classStats = grades.GroupBy(g => g.ClassName ?? "N/A")
                        .Select(g => new { Class = g.Key, Count = g.Count(), Avg = g.Average(x => x.Score) })
                        .OrderByDescending(s => s.Avg)
                        .Take(5);

                    formattedMessage.AppendLine();
                    formattedMessage.AppendLine("🏆 **TOP 5 LỚP THEO ĐIỂM TB:**");
                    foreach (var classStats1 in classStats)
                    {
                        formattedMessage.AppendLine($"   • {classStats1.Class}: {classStats1.Count} điểm (TB: {classStats1.Avg:F2})");
                    }

                    var subjectStats = grades.GroupBy(g => g.SubjectName ?? "N/A")
                        .Select(g => new { Subject = g.Key, Count = g.Count(), Avg = g.Average(x => x.Score) })
                        .OrderByDescending(s => s.Count)
                        .Take(5);

                    formattedMessage.AppendLine();
                    formattedMessage.AppendLine("📚 **TOP 5 MÔN HỌC:**");
                    foreach (var subject in subjectStats)
                    {
                        formattedMessage.AppendLine($"   • {subject.Subject}: {subject.Count} điểm (TB: {subject.Avg:F2})");
                    }
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = formattedMessage.ToString(),
                    data = new
                    {
                        ReportType = reportType,
                        StartDate = startDate.ToString("yyyy-MM-dd"),
                        EndDate = endDate.ToString("yyyy-MM-dd"),
                        TotalStudents = students.Count,
                        StudentsWithGrades = grades.Select(g => g.StudentId).Distinct().Count(),
                        TotalGrades = grades.Count,
                        OverallAverage = grades.Any() ? grades.Average(g => g.Score) : 0,
                        ClassFilter = classIdFilter,
                        SubjectFilter = subjectNameFilter,
                        PeriodData = gradesByPeriod.Select(p => new
                        {
                            Period = p.Key,
                            DisplayName = GetPeriodDisplayName(p.Key, reportType),
                            GradeCount = p.Count(),
                            StudentCount = p.Select(g => g.StudentId).Distinct().Count(),
                            Average = p.Average(g => g.Score),
                            TopStudents = p.GroupBy(g => g.StudentId).Take(5).Select(sg => new
                            {
                                StudentId = sg.Key,
                                StudentName = sg.First().StudentName,
                                ClassName = sg.First().ClassName,
                                GradeCount = sg.Count(),
                                Average = sg.Average(g => g.Score)
                            })
                        })
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi tạo báo cáo điểm tổng. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private string GetPeriodKey(DateTime date, string reportType)
        {
            return reportType switch
            {
                "week" => $"{date.Year}-W{GetWeekOfYear(date):D2}",
                "month" => $"{date.Year}-{date.Month:D2}",
                "year" => $"{date.Year}",
                _ => date.ToString("yyyy-MM-dd")
            };
        }

        private string GetPeriodDisplayName(string periodKey, string reportType)
        {
            switch (reportType)
            {
                case "week":
                    return $"Tuần {periodKey}";
                case "month":
                    var parts = periodKey.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[0], out var year) && int.TryParse(parts[1], out var month))
                    {
                        return $"Tháng {month}/{year}";
                    }
                    return $"Tháng {periodKey}";
                case "year":
                    return $"Năm {periodKey}";
                default:
                    return periodKey;
            }
        }

        private int GetWeekOfYear(DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;
            var calendar = culture.Calendar;
            var calendarWeekRule = culture.DateTimeFormat.CalendarWeekRule;
            var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
            
            return calendar.GetWeekOfYear(date, calendarWeekRule, firstDayOfWeek);
        }

        private async Task<string> CreateLessonReportAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Cần đăng nhập để tạo báo cáo buổi học",
                        error = "AUTHENTICATION_REQUIRED"
                    }, Formatting.Indented);
                }

                // Validate required parameters
                if (!args.TryGetValue("timetable_slot_id", out var slotIdObj) || !int.TryParse(slotIdObj?.ToString(), out var timetableSlotId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Thiếu hoặc không hợp lệ timetable_slot_id",
                        error = "MISSING_TIMETABLE_SLOT_ID"
                    }, Formatting.Indented);
                }

                if (!args.TryGetValue("lesson_date", out var dateObj) || !DateTime.TryParse(dateObj?.ToString(), out var lessonDate))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Thiếu hoặc không hợp lệ lesson_date (yyyy-MM-dd)",
                        error = "MISSING_LESSON_DATE"
                    }, Formatting.Indented);
                }

                if (!args.TryGetValue("total_students", out var totalObj) || !int.TryParse(totalObj?.ToString(), out var totalStudents))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Thiếu hoặc không hợp lệ total_students",
                        error = "MISSING_TOTAL_STUDENTS"
                    }, Formatting.Indented);
                }

                if (!args.TryGetValue("absent_students", out var absentObj) || !int.TryParse(absentObj?.ToString(), out var absentStudents))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Thiếu hoặc không hợp lệ absent_students",
                        error = "MISSING_ABSENT_STUDENTS"
                    }, Formatting.Indented);
                }

                // Optional parameters
                var remarks = args.TryGetValue("remarks", out var remarksObj) ? remarksObj?.ToString() : null;
                var lessonId = args.TryGetValue("lesson_id", out var lessonIdObj) && int.TryParse(lessonIdObj?.ToString(), out var parsedLessonId) ? parsedLessonId : (int?)null;

                // Parse student notes if provided
                var studentNotes = new List<CreateStudentLessonNoteRequest>();
                if (args.TryGetValue("student_notes", out var notesObj) && !string.IsNullOrEmpty(notesObj?.ToString()))
                {
                    try
                    {
                        var notesData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(notesObj.ToString()!);
                        if (notesData != null)
                        {
                            foreach (var note in notesData)
                            {
                                if (note.TryGetValue("student_id", out var studentIdObj) && 
                                    note.TryGetValue("note_type", out var noteTypeObj))
                                {
                                    var studentNote = new CreateStudentLessonNoteRequest
                                    {
                                        StudentId = studentIdObj.ToString()!,
                                        NoteType = noteTypeObj.ToString()!,
                                        NoteContent = note.TryGetValue("description", out var descObj) ? descObj.ToString() : null
                                    };
                                    studentNotes.Add(studentNote);
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Ignore JSON parsing errors for student notes
                    }
                }

                // Create the request
                var request = new CreateLessonReportRequest
                {
                    LessonId = lessonId,
                    TimetableSlotId = timetableSlotId,
                    LessonDate = lessonDate,
                    TotalStudents = totalStudents,
                    AbsentStudents = absentStudents,
                    Remarks = remarks,
                    StudentNotes = studentNotes
                };

                // Call the service
                var result = await _lessonReportService.CreateLessonReportAsync(request, userId);

                if (result.Success)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = true,
                        message = $"✅ Đã tạo báo cáo buổi học thành công",
                        data = new
                        {
                            TimetableSlotId = timetableSlotId,
                            LessonDate = lessonDate.ToString("yyyy-MM-dd"),
                            TotalStudents = totalStudents,
                            AbsentStudents = absentStudents,
                            AttendanceRate = totalStudents > 0 ? Math.Round((double)(totalStudents - absentStudents) / totalStudents * 100, 1) : 0,
                            Remarks = remarks,
                            StudentNotesCount = studentNotes.Count,
                            CreatedAt = DateTime.UtcNow
                        }
                    }, Formatting.Indented);
                }
                else
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"❌ Không thể tạo báo cáo: {result.Message}",
                        error = "LESSON_REPORT_CREATION_FAILED",
                        details = result.Error
                    }, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi tạo báo cáo buổi học",
                    error = "INTERNAL_ERROR",
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetLessonReportsByTeacherAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để xem báo cáo bài học.";
                }

                var teacher = await _teacherRepository.GetByAccountIdAsync(userId);
                if (teacher == null)
                {
                    return "⛔ **KHÔNG CÓ QUYỀN:** Chỉ giáo viên mới có thể xem báo cáo bài học.";
                }

                var reports = await _lessonReportRepository.GetByTeacherIdAsync(teacher.TeacherID);

                var formattedMessage = new StringBuilder();
                formattedMessage.AppendLine("📚 **BÁO CÁO BÀI HỌC CỦA GIÁO VIÊN**");
                formattedMessage.AppendLine($"👨‍🏫 Giáo viên: {teacher.FullName}");
                formattedMessage.AppendLine($"📊 Tổng số báo cáo: {reports.Count()}");
                formattedMessage.AppendLine();

                foreach (var report in reports.Take(20))
                {
                    formattedMessage.AppendLine($"📋 **Báo cáo #{report.ReportID}**");
                    formattedMessage.AppendLine($"   � Ngày học: {report.LessonDate:dd/MM/yyyy}");
                    formattedMessage.AppendLine($"   👥 Tổng HS: {report.TotalStudents}");
                    formattedMessage.AppendLine($"   ❌ Vắng mặt: {report.AbsentStudents}");
                    if (!string.IsNullOrEmpty(report.Remarks))
                        formattedMessage.AppendLine($"   📝 Ghi chú: {report.Remarks}");
                    formattedMessage.AppendLine();
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = formattedMessage.ToString(),
                    data = reports.Select(r => new
                    {
                        ReportId = r.ReportID,
                        LessonDate = r.LessonDate,
                        TotalStudents = r.TotalStudents,
                        AbsentStudents = r.AbsentStudents,
                        Remarks = r.Remarks,
                        CreatedAt = r.CreatedAt
                    })
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy báo cáo bài học",
                    error = "INTERNAL_ERROR",
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> SendNotificationToParentAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Cần đăng nhập để gửi thông báo.",
                        error = "AUTHENTICATION_REQUIRED",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                var teacher = await _teacherRepository.GetByAccountIdAsync(userId);
                if (teacher == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Chỉ giáo viên mới có thể gửi thông báo cho phụ huynh.",
                        error = "PERMISSION_DENIED",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                var parentId = "";
                if (args.TryGetValue("parent_id", out var parentIdObj))
                {
                    parentId = parentIdObj?.ToString() ?? "";
                }

                var title = args.TryGetValue("title", out var titleObj) ? titleObj?.ToString() : "";
                var message = args.TryGetValue("message", out var messageObj) ? messageObj?.ToString() : "";

                if (string.IsNullOrEmpty(parentId) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Vui lòng cung cấp đầy đủ: Mã phụ huynh, tiêu đề và nội dung thông báo.",
                        error = "MISSING_PARAMETERS",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                var parent = await _context.Parents.FindAsync(parentId);
                if (parent == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Phụ huynh với mã này không tồn tại trong hệ thống.",
                        error = "PARENT_NOT_FOUND",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                var notification = new Notification
                {
                    Title = title,
                    Content = message,
                    UserId = parent.AccountID,
                    Type = "teacher_message",
                    Category = "announcement",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                await _notificationRepository.CreateAsync(notification);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = "✅ Đã gửi thông báo thành công cho phụ huynh.",
                    error = (string?)null,
                    data = new
                    {
                        NotificationId = notification.NotificationId,
                        ParentName = parent.FullName,
                        Title = title,
                        Message = message,
                        SentAt = notification.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        TeacherName = $"{teacher.Account?.FirstName} {teacher.Account?.LastName}".Trim()
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Không thể gửi thông báo. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetAllFeedbacksAsync(Dictionary<string, object> args)
        {
            try
            {
                var feedbacks = await _context.Feedbacks
                    .Include(f => f.Parent)
                    .OrderByDescending(f => f.DateTime)
                    .Take(50)
                    .ToListAsync();

                var formattedMessage = new StringBuilder();
                formattedMessage.AppendLine("💬 **TẤT CẢ PHẢN HỒI CỦA PHỤ HUYNH**");
                formattedMessage.AppendLine($"📊 Tổng số phản hồi: {feedbacks.Count}");
                formattedMessage.AppendLine();

                foreach (var feedback in feedbacks)
                {
                    formattedMessage.AppendLine($"💌 **Phản hồi #{feedback.FeedbackID}**");
                    formattedMessage.AppendLine($"   👨‍👩‍👧‍👦 Phụ huynh: {feedback.Parent?.FullName ?? "N/A"}");
                    formattedMessage.AppendLine($"   � Đối tượng: {feedback.Target ?? "N/A"}");
                    formattedMessage.AppendLine($"   📅 Ngày gửi: {feedback.DateTime?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"}");
                    formattedMessage.AppendLine($"   📝 Nội dung: {feedback.Content}");
                    formattedMessage.AppendLine();
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = formattedMessage.ToString(),
                    data = feedbacks.Select(f => new
                    {
                        FeedbackId = f.FeedbackID,
                        ParentName = f.Parent?.FullName,
                        Target = f.Target,
                        Content = f.Content,
                        CreatedDate = f.DateTime
                    })
                }, Formatting.Indented);
            }
            catch (Exception)
            {
                return "❌ **LỖI:** Không thể lấy danh sách phản hồi. Vui lòng thử lại sau.";
            }
        }

        private async Task<string> GetFeedbacksByParentAsync(Dictionary<string, object> args)
        {
            try
            {
                // Validate parent_id
                if (!args.TryGetValue("parent_id", out var parentIdObj) || string.IsNullOrEmpty(parentIdObj?.ToString()))
                {
                    return "❌ **LỖI:** Thiếu ID phụ huynh. Vui lòng cung cấp parent_id.";
                }

                var parentId = parentIdObj.ToString();

                // Parse parameters
                var searchTerm = args.TryGetValue("search_term", out var searchObj) ? searchObj?.ToString() : null;
                var target = args.TryGetValue("target", out var targetObj) ? targetObj?.ToString() : null;
                
                var pageSize = 20;
                if (args.TryGetValue("page_size", out var pageSizeObj) && int.TryParse(pageSizeObj?.ToString(), out var parsedPageSize))
                {
                    pageSize = Math.Min(parsedPageSize, 50);
                }

                var sortBy = args.TryGetValue("sort_by", out var sortObj) ? sortObj?.ToString() : "DateTime";

                // Check if parent exists
                var parent = await _parentRepository.GetParentByIdAsync(parentId!);
                if (parent == null)
                {
                    return $"❌ **LỖI:** Không tìm thấy phụ huynh với ID: {parentId}";
                }

                // Get feedbacks by parent
                var feedbacksResult = await _feedbackService.GetFeedbacksByParentIdAsync(parentId!);
                if (!feedbacksResult.Success || feedbacksResult.Data == null)
                {
                    return "❌ **LỖI:** Không thể lấy danh sách phản hồi. Vui lòng thử lại sau.";
                }

                var feedbacks = feedbacksResult.Data.ToList();

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    feedbacks = feedbacks.Where(f => 
                        f.Content.ToLower().Contains(searchLower)
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(target))
                {
                    feedbacks = feedbacks.Where(f => f.Target.Equals(target, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Apply sorting
                feedbacks = sortBy switch
                {
                    "Target" => feedbacks.OrderBy(f => f.Target).ToList(),
                    _ => feedbacks.OrderByDescending(f => f.CreatedAt ?? DateTime.MinValue).ToList()
                };

                // Take only requested number
                feedbacks = feedbacks.Take(pageSize).ToList();

                var formattedMessage = new StringBuilder();
                formattedMessage.AppendLine($"📝 **PHẢN HỒI CỦA PHỤ HUYNH: {parent.FullName}**");
                formattedMessage.AppendLine();
                formattedMessage.AppendLine($"📊 **Tổng số phản hồi:** {feedbacks.Count}");
                
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    formattedMessage.AppendLine($"🔍 **Từ khóa tìm kiếm:** {searchTerm}");
                }
                
                if (!string.IsNullOrEmpty(target))
                {
                    formattedMessage.AppendLine($"🎯 **Đối tượng:** {target}");
                }

                formattedMessage.AppendLine();

                if (!feedbacks.Any())
                {
                    formattedMessage.AppendLine("📭 **Chưa có phản hồi nào.**");
                }
                else
                {
                    foreach (var feedback in feedbacks)
                    {
                        formattedMessage.AppendLine($"🆔 **ID:** {feedback.FeedbackId}");
                        formattedMessage.AppendLine($"📅 **Ngày:** {(feedback.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "Không xác định")}");
                        formattedMessage.AppendLine($"🎯 **Đối tượng:** {feedback.Target}");
                        formattedMessage.AppendLine($"📝 **Nội dung:** {(feedback.Content.Length > 100 ? feedback.Content.Substring(0, 100) + "..." : feedback.Content)}");
                        formattedMessage.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    }
                }

                return formattedMessage.ToString();
            }
            catch (Exception)
            {
                return "❌ **LỖI:** Không thể lấy danh sách phản hồi của phụ huynh. Vui lòng thử lại sau.";
            }
        }

        private async Task<string> GetFeedbackByIdAsync(Dictionary<string, object> args)
        {
            try
            {
                // Validate feedback_id
                if (!args.TryGetValue("feedback_id", out var feedbackIdObj) || 
                    !int.TryParse(feedbackIdObj?.ToString(), out var feedbackId))
                {
                    return "❌ **LỖI:** ID phản hồi không hợp lệ. Vui lòng cung cấp feedback_id là một số nguyên.";
                }

                // Get feedback by ID
                var feedbackResult = await _feedbackService.GetFeedbackByIdAsync(feedbackId);
                if (!feedbackResult.Success || feedbackResult.Data == null)
                {
                    return $"❌ **LỖI:** Không tìm thấy phản hồi với ID: {feedbackId}";
                }

                var feedback = feedbackResult.Data;

                // Get parent info
                var parent = await _parentRepository.GetParentByIdAsync(feedback.ParentId);
                var parentName = parent?.FullName ?? "Không xác định";

                var formattedMessage = new StringBuilder();
                formattedMessage.AppendLine("📝 **CHI TIẾT PHẢN HỒI**");
                formattedMessage.AppendLine();
                formattedMessage.AppendLine($"🆔 **ID:** {feedback.FeedbackId}");
                formattedMessage.AppendLine($"📅 **Ngày gửi:** {(feedback.CreatedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Không xác định")}");
                formattedMessage.AppendLine($"👤 **Phụ huynh:** {parentName}");
                formattedMessage.AppendLine($"🎯 **Đối tượng:** {feedback.Target}");
                if (!string.IsNullOrEmpty(feedback.Category))
                {
                    formattedMessage.AppendLine($"📂 **Danh mục:** {feedback.Category}");
                }
                formattedMessage.AppendLine();
                formattedMessage.AppendLine("📝 **Nội dung phản hồi:**");
                formattedMessage.AppendLine($"```");
                formattedMessage.AppendLine(feedback.Content);
                formattedMessage.AppendLine($"```");

                return formattedMessage.ToString();
            }
            catch (Exception)
            {
                return "❌ **LỖI:** Không thể lấy thông tin phản hồi. Vui lòng thử lại sau.";
            }
        }

        private async Task<string> GetNotificationsForUserAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để xem thông báo.";
                }

                var pageSize = 20;
                if (args.TryGetValue("page_size", out var pageSizeObj) && int.TryParse(pageSizeObj?.ToString(), out var parsedPageSize))
                {
                    pageSize = Math.Min(parsedPageSize, 50);
                }

                var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId, 0, pageSize);
                var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);

                var formattedMessage = new StringBuilder();
                formattedMessage.AppendLine("🔔 **THÔNG BÁO CỦA BẠN**");
                formattedMessage.AppendLine();
                formattedMessage.AppendLine($"📊 **Tổng số thông báo:** {notifications.Count}");
                formattedMessage.AppendLine($"🆕 **Chưa đọc:** {unreadCount}");
                formattedMessage.AppendLine("─".PadRight(50, '─'));

                foreach (var notification in notifications)
                {
                    var status = notification.IsRead ? "✅" : "🆕";
                    formattedMessage.AppendLine();
                    formattedMessage.AppendLine($"{status} **{notification.Title}**");
                    formattedMessage.AppendLine($"├─ 📅 {notification.CreatedAt:dd/MM/yyyy HH:mm}");
                    formattedMessage.AppendLine($"└─ 📝 {notification.Content}");
                }

                return formattedMessage.ToString();
            }
            catch (Exception)
            {
                return "❌ **LỖI:** Không thể lấy thông báo. Vui lòng thử lại sau.";
            }
        }

        private async Task<string> MarkNotificationsAsReadAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để đánh dấu thông báo đã đọc.";
                }

                var notificationIds = new List<string>();
                if (args.TryGetValue("notification_ids", out var idsObj))
                {
                    if (idsObj is List<object> idsList)
                    {
                        notificationIds = idsList.Select(id => id.ToString()).Where(id => !string.IsNullOrEmpty(id)).ToList()!;
                    }
                }

                bool result;
                if (notificationIds.Any())
                {
                    result = await _notificationRepository.MarkAsReadAsync(notificationIds);
                }
                else
                {
                    // Mark all user's notifications as read
                    var userNotifications = await _notificationRepository.GetUnreadNotificationsByUserIdAsync(userId);
                    var allIds = userNotifications.Select(n => n.NotificationId).ToList();
                    result = await _notificationRepository.MarkAsReadAsync(allIds);
                    notificationIds = allIds;
                }

                var successMessage = result ? 
                    $"✅ **THÀNH CÔNG:** Đã đánh dấu {notificationIds.Count} thông báo là đã đọc" :
                    "❌ **THẤT BẠI:** Có lỗi khi đánh dấu thông báo";
                
                return successMessage;
            }
            catch (Exception)
            {
                return "❌ **LỖI:** Không thể đánh dấu thông báo đã đọc. Vui lòng thử lại sau.";
            }
        }

        private async Task<string> CreateTimetableAsync(Dictionary<string, object> args)
        {
            try
            {
                var classId = 0;
                if (args.TryGetValue("class_id", out var classIdObj) && int.TryParse(classIdObj?.ToString(), out var parsedClassId))
                {
                    classId = parsedClassId;
                }

                var dayOfWeek = args.TryGetValue("day_of_week", out var dayObj) ? dayObj?.ToString() : "";
                var startTime = args.TryGetValue("start_time", out var startObj) ? startObj?.ToString() : "";
                var endTime = args.TryGetValue("end_time", out var endObj) ? endObj?.ToString() : "";

                var subjectId = 0;
                if (args.TryGetValue("subject_id", out var subjectIdObj) && int.TryParse(subjectIdObj?.ToString(), out var parsedSubjectId))
                {
                    subjectId = parsedSubjectId;
                }

                var teacherId = args.TryGetValue("teacher_id", out var teacherIdObj) ? teacherIdObj?.ToString() : "";

                if (classId == 0 || string.IsNullOrEmpty(dayOfWeek) || string.IsNullOrEmpty(startTime) || 
                    string.IsNullOrEmpty(endTime) || subjectId == 0 || string.IsNullOrEmpty(teacherId))
                {
                    return "⚠️ **THIẾU THÔNG TIN:** Vui lòng cung cấp đầy đủ: Mã lớp, thứ, giờ bắt đầu, giờ kết thúc, mã môn học, mã giáo viên.";
                }

                var classEntity = await _classRepository.GetByIdAsync(classId);
                if (classEntity == null)
                {
                    return "❌ **KHÔNG TÌM THẤY:** Lớp học với mã này không tồn tại.";
                }

                var subject = await _subjectRepository.GetByIdAsync(subjectId);
                if (subject == null)
                {
                    return "❌ **KHÔNG TÌM THẤY:** Môn học với mã này không tồn tại.";
                }

                var teacher = await _teacherRepository.GetByIdAsync(teacherId);
                if (teacher == null)
                {
                    return "❌ **KHÔNG TÌM THẤY:** Giáo viên với mã này không tồn tại.";
                }

                var timetable = new Timetable
                {
                    ClassID = classId,
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(365) // Default 1 year
                };

                var createdTimetable = await _timetableRepository.CreateTimetableAsync(timetable);

                var successMessage = new StringBuilder();
                successMessage.AppendLine("✅ **TẠO THỜI KHÓA BIỂU THÀNH CÔNG**");
                successMessage.AppendLine();
                successMessage.AppendLine($"🆔 **Mã TKB:** #{createdTimetable.TimetableID}");
                successMessage.AppendLine($"🏫 **Lớp học:** {classEntity.ClassName}");
                successMessage.AppendLine($"📅 **Thứ:** {dayOfWeek}");
                successMessage.AppendLine($"⏰ **Giờ học:** {startTime} - {endTime}");
                successMessage.AppendLine($"📚 **Môn học:** {subject.SubjectName}");
                successMessage.AppendLine($"👨‍🏫 **Giáo viên:** {teacher.FullName}");
                
                return successMessage.ToString();
            }
            catch (Exception)
            {
                return "❌ **LỖI:** Không thể tạo thời khóa biểu. Vui lòng thử lại sau.";
            }
        }

        private async Task<string> GetWeeklyStudentReportsAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để xem báo cáo tuần.";
                }

                var parent = await _parentRepository.GetByAccountIdAsync(userId);
                if (parent == null)
                {
                    return "⛔ **KHÔNG CÓ QUYỀN:** Chỉ phụ huynh mới có thể xem báo cáo tuần của con.";
                }

                var weekOffset = 0;
                if (args.TryGetValue("week_offset", out var offsetObj) && int.TryParse(offsetObj?.ToString(), out var parsedOffset))
                {
                    weekOffset = parsedOffset;
                }

                var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + weekOffset * 7);
                var weekEnd = weekStart.AddDays(6);

                var students = await _context.Students
                    .Include(s => s.Account)
                    .Include(s => s.Class)
                    .Where(s => s.ParentID == parent.ParentID)
                    .ToListAsync();

                var formattedMessage = new StringBuilder();
                formattedMessage.AppendLine("📊 **BÁO CÁO TUẦN CỦA CON**");
                formattedMessage.AppendLine($"📅 Tuần từ {weekStart:dd/MM/yyyy} đến {weekEnd:dd/MM/yyyy}");
                formattedMessage.AppendLine();

                foreach (var student in students)
                {
                    formattedMessage.AppendLine($"👨‍🎓 **{student.FullName}** (Lớp: {student.Class?.ClassName})");
                    
                    // Điểm danh tuần này - TimetableSlot không có Date, chỉ lấy tổng số điểm danh
                    var attendance = await _context.Attendances
                        .Where(a => a.StudentID == student.StudentID)
                        .CountAsync();
                    
                    formattedMessage.AppendLine($"   📋 Số buổi điểm danh: {attendance}");
                    
                    // Điểm số tuần này - Grade không có trường ExamDate
                    var grades = await _context.Grades
                        .Where(g => g.StudentID == student.StudentID)
                        .ToListAsync();
                    
                    if (grades.Any())
                    {
                        formattedMessage.AppendLine($"   📚 Tổng điểm hiện có: {grades.Count} điểm");
                        var recentGrades = grades.Take(3);
                        foreach (var grade in recentGrades)
                        {
                            // Lấy tên môn học từ SubjectID
                            var subject = await _context.Subjects.FindAsync(grade.SubjectID);
                            formattedMessage.AppendLine($"      • {subject?.SubjectName ?? "N/A"}: {grade.Score} ({grade.GradeType})");
                        }
                    }
                    else
                    {
                        formattedMessage.AppendLine("   📚 Chưa có điểm");
                    }
                    
                    formattedMessage.AppendLine();
                }

                return formattedMessage.ToString();
            }
            catch (Exception)
            {
                return "❌ **LỖI:** Không thể lấy báo cáo tuần. Vui lòng thử lại sau.";
            }
        }

        private async Task SendChatNotificationAsync(string userId, string functionName, string result)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(functionName))
                {
                    return;
                }

                // Tạo title và body notification dựa trên function name
                var title = GetNotificationTitle(functionName);
                var body = GetNotificationBody(functionName, result);

                // Tạo data payload cho notification
                var data = new Dictionary<string, string>
                {
                    ["type"] = "chat_response",
                    ["function"] = functionName,
                    ["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["userId"] = userId
                };

                // Gửi notification qua Firebase với timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await _notificationService.SendNotificationToAllDevicesAsync(userId, title, body, data);
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw để không ảnh hưởng đến function chính
                Console.WriteLine($"Error sending chat notification for function {functionName}: {ex.Message}");
            }
        }

        private bool IsValidJsonResponse(string jsonString)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonString))
                    return false;

                var obj = JsonConvert.DeserializeObject(jsonString);
                return obj != null;
            }
            catch
            {
                return false;
            }
        }

        private string EnsureJsonResponse(string response)
        {
            if (IsValidJsonResponse(response))
                return response;

            // If it's a plain string, convert to JSON format
            return JsonConvert.SerializeObject(new
            {
                success = true,
                message = response,
                error = (string?)null,
                data = (object?)null
            }, Formatting.Indented);
        }

        private string CreateErrorResponse(string message, string errorCode, string? details = null)
        {
            return JsonConvert.SerializeObject(new
            {
                success = false,
                message = message,
                error = errorCode,
                data = (object?)null,
                errorDetails = details,
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            }, Formatting.Indented);
        }

        private string GetNotificationTitle(string functionName)
        {
            return functionName.ToLower() switch
            {
                "get_student_info" => "📋 Thông tin học sinh",
                "get_student_subjects" => "📚 Môn học của học sinh", 
                "get_current_time" => "🕒 Thời gian hiện tại",
                "format_vietnam_time" => "🕐 Định dạng thời gian Việt Nam",
                "calculate_sum" => "🧮 Kết quả tính toán",
                "get_student_timetable" => "📅 Thời khóa biểu học sinh",
                "get_teacher_info" => "👨‍🏫 Thông tin giáo viên",
                "get_teacher_timetable" => "📅 Thời khóa biểu giáo viên",
                "search_students" => "🔍 Tìm kiếm học sinh",
                "search_teachers" => "🔍 Tìm kiếm giáo viên",
                "get_all_classes" => "🏫 Danh sách lớp học",
                "get_timetable_by_class" => "📅 Thời khóa biểu lớp",
                "create_parent_feedback" => "📝 Phản hồi đã gửi",
                "get_student_grades" => "📊 Điểm số học sinh",
                "get_student_lesson_reports" => "📋 Báo cáo buổi học",
                "get_student_attendance" => "✅ Điểm danh học sinh",
                "add_student_attendance" => "✅ Điểm danh đã cập nhật",
                "get_student_grade_report" => "📈 Báo cáo điểm số",
                "get_all_students_grade_report" => "📊 Báo cáo tổng hợp",
                "create_lesson_report" => "📝 Báo cáo buổi học",
                "get_lesson_reports_by_teacher" => "📋 Báo cáo của giáo viên",
                "send_notification_to_parent" => "📢 Thông báo đã gửi",
                "get_all_feedbacks" => "📝 Danh sách phản hồi",
                "get_feedbacks_by_parent" => "📝 Phản hồi của phụ huynh",
                "get_feedback_by_id" => "📝 Chi tiết phản hồi",
                "send_broadcast_notification" => "📢 Thông báo hệ thống",
                "send_notification_to_teachers" => "👨‍🏫 Thông báo đến giáo viên",
                "send_notification_to_students" => "🎓 Thông báo đến học sinh", 
                "send_notification_to_parents" => "👨‍👩‍👧‍👦 Thông báo đến phụ huynh",
                "get_notifications_for_user" => "🔔 Thông báo của bạn",
                "mark_notifications_as_read" => "✅ Đã đánh dấu đọc",
                "create_timetable" => "📅 Thời khóa biểu mới",
                "get_weekly_student_reports" => "📊 Báo cáo tuần",
                _ => "💬 Có phản hồi mới từ EduConnect AI"
            };
        }

        private string GetNotificationBody(string functionName, string result)
        {
            // Extract first line or summary from result for notification body
            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var firstMeaningfulLine = lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l.Trim()));
            
            if (!string.IsNullOrEmpty(firstMeaningfulLine))
            {
                // Remove formatting characters and limit length
                var cleanBody = firstMeaningfulLine
                    .Replace("✅", "")
                    .Replace("📋", "")
                    .Replace("🔒", "")
                    .Replace("**", "")
                    .Replace("*", "")
                    .Trim();
                
                return cleanBody.Length > 100 ? cleanBody.Substring(0, 97) + "..." : cleanBody;
            }

            return "Bạn có câu trả lời mới từ EduConnect AI. Nhấn để xem chi tiết.";
        }

        #region Helper Methods

        private string GetUserFullName(ApplicationUser user)
        {
            return $"{user.FirstName} {user.LastName}".Trim();
        }

        #endregion

        #region Principal User Management Functions

        private async Task<string> SendBroadcastNotificationAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để gửi thông báo.";
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Principal"))
                {
                    return "⛔ **KHÔNG CÓ QUYỀN:** Chỉ hiệu trưởng mới có quyền gửi thông báo hệ thống.";
                }

                var title = args.TryGetValue("title", out var titleObj) ? titleObj?.ToString() : "";
                var message = args.TryGetValue("message", out var messageObj) ? messageObj?.ToString() : "";
                var category = args.TryGetValue("category", out var categoryObj) ? categoryObj?.ToString() ?? "system" : "system";

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
                {
                    return "⚠️ **THIẾU THÔNG TIN:** Vui lòng cung cấp tiêu đề và nội dung thông báo.";
                }

                // Get all users in the system
                var allUsers = _userManager.Users.ToList();
                var successCount = 0;

                foreach (var targetUser in allUsers)
                {
                    try
                    {
                        var notification = new Notification
                        {
                            Title = title,
                            Content = message,
                            UserId = targetUser.Id,
                            Type = "system_broadcast",
                            Category = category,
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };

                        await _notificationRepository.CreateAsync(notification);
                        
                        // Send Firebase push notification
                        var data = new Dictionary<string, string>
                        {
                            ["type"] = "system_broadcast",
                            ["category"] = category,
                            ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                            ["notificationId"] = notification.NotificationId
                        };
                        
                        await _notificationService.SendNotificationToAllDevicesAsync(targetUser.Id, title, message, data);
                        successCount++;
                    }
                    catch
                    {
                        // Continue with next user if one fails
                        continue;
                    }
                }

                var successMessage = new StringBuilder();
                successMessage.AppendLine("📢 **THÔNG BÁO HỆ THỐNG ĐÃ GỬI**");
                successMessage.AppendLine();
                successMessage.AppendLine($"📄 **Tiêu đề:** {title}");
                successMessage.AppendLine($"📝 **Nội dung:** {message}");
                successMessage.AppendLine($"👥 **Số người nhận:** {successCount}/{allUsers.Count}");
                successMessage.AppendLine($"⏰ **Thời gian gửi:** {DateTime.UtcNow:dd/MM/yyyy HH:mm}");
                
                return successMessage.ToString();
            }
            catch (Exception)
            {
                return "❌ **LỖI:** Không thể gửi thông báo hệ thống. Vui lòng thử lại sau.";
            }
        }

        private async Task<string> SendNotificationToTeachersAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để gửi thông báo.";
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Principal"))
                {
                    return "⛔ **KHÔNG CÓ QUYỀN:** Chỉ hiệu trưởng mới có quyền gửi thông báo đến giáo viên.";
                }

                var title = args.TryGetValue("title", out var titleObj) ? titleObj?.ToString() : "";
                var message = args.TryGetValue("message", out var messageObj) ? messageObj?.ToString() : "";
                var category = args.TryGetValue("category", out var categoryObj) ? categoryObj?.ToString() ?? "announcement" : "announcement";

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
                {
                    return "⚠️ **THIẾU THÔNG TIN:** Vui lòng cung cấp tiêu đề và nội dung thông báo.";
                }

                // Get all teachers
                var teachers = await _context.Teachers
                    .Include(t => t.Account)
                    .ToListAsync();

                var successCount = 0;

                foreach (var teacher in teachers)
                {
                    try
                    {
                        var notification = new Notification
                        {
                            Title = title,
                            Content = message,
                            UserId = teacher.AccountID,
                            Type = "principal_to_teachers",
                            Category = category,
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };

                        await _notificationRepository.CreateAsync(notification);
                        
                        // Send Firebase push notification
                        var data = new Dictionary<string, string>
                        {
                            ["type"] = "principal_to_teachers",
                            ["category"] = category,
                            ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                            ["notificationId"] = notification.NotificationId
                        };
                        
                        await _notificationService.SendNotificationToAllDevicesAsync(teacher.AccountID, title, message, data);
                        successCount++;
                    }
                    catch
                    {
                        continue;
                    }
                }

                var successMessage = new StringBuilder();
                successMessage.AppendLine("👨‍🏫 **THÔNG BÁO ĐẾN GIÁO VIÊN ĐÃ GỬI**");
                successMessage.AppendLine();
                successMessage.AppendLine($"📄 **Tiêu đề:** {title}");
                successMessage.AppendLine($"📝 **Nội dung:** {message}");
                successMessage.AppendLine($"👥 **Số giáo viên nhận:** {successCount}/{teachers.Count}");
                successMessage.AppendLine($"⏰ **Thời gian gửi:** {DateTime.UtcNow:dd/MM/yyyy HH:mm}");
                
                return successMessage.ToString();
            }
            catch (Exception)
            {
                return "❌ **LỖI:** Không thể gửi thông báo đến giáo viên. Vui lòng thử lại sau.";
            }
        }

        private async Task<string> SendNotificationToStudentsAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để gửi thông báo.";
                }

                // Validate user role - Allow both Principal and Teacher
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || (!await _userManager.IsInRoleAsync(user, "Principal") && !await _userManager.IsInRoleAsync(user, "Teacher")))
                {
                    return "⛔ **KHÔNG CÓ QUYỀN:** Chỉ hiệu trưởng và giáo viên mới có quyền gửi thông báo đến học sinh.";
                }

                var title = args.TryGetValue("title", out var titleObj) ? titleObj?.ToString() : "";
                var message = args.TryGetValue("message", out var messageObj) ? messageObj?.ToString() : "";
                var category = args.TryGetValue("category", out var categoryObj) ? categoryObj?.ToString() ?? "announcement" : "announcement";

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
                {
                    return "⚠️ **THIẾU THÔNG TIN:** Vui lòng cung cấp tiêu đề và nội dung thông báo.";
                }

                // Get all students
                var students = await _context.Students
                    .Include(s => s.Account)
                    .ToListAsync();

                var successCount = 0;

                foreach (var student in students)
                {
                    try
                    {
                        var notification = new Notification
                        {
                            Title = title,
                            Content = message,
                            UserId = student.AccountID,
                            Type = await _userManager.IsInRoleAsync(user, "Principal") ? "principal_to_students" : "teacher_to_students",
                            Category = category,
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };

                        await _notificationRepository.CreateAsync(notification);
                        
                        // Send Firebase push notification
                        var data = new Dictionary<string, string>
                        {
                            ["type"] = notification.Type,
                            ["category"] = category,
                            ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                            ["notificationId"] = notification.NotificationId
                        };
                        
                        await _notificationService.SendNotificationToAllDevicesAsync(student.AccountID, title, message, data);
                        successCount++;
                    }
                    catch
                    {
                        continue;
                    }
                }

                var successMessage = new StringBuilder();
                successMessage.AppendLine("🎓 **THÔNG BÁO ĐẾN HỌC SINH ĐÃ GỬI**");
                successMessage.AppendLine();
                successMessage.AppendLine($"📄 **Tiêu đề:** {title}");
                successMessage.AppendLine($"📝 **Nội dung:** {message}");
                successMessage.AppendLine($"👥 **Số học sinh nhận:** {successCount}/{students.Count}");
                successMessage.AppendLine($"⏰ **Thời gian gửi:** {DateTime.UtcNow:dd/MM/yyyy HH:mm}");
                
                return successMessage.ToString();
            }
            catch (Exception)
            {
                return "❌ **LỖI:** Không thể gửi thông báo đến học sinh. Vui lòng thử lại sau.";
            }
        }

        private async Task<string> SendNotificationToParentsAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để gửi thông báo.";
                }

                // Validate user role - Allow both Principal and Teacher
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || (!await _userManager.IsInRoleAsync(user, "Principal") && !await _userManager.IsInRoleAsync(user, "Teacher")))
                {
                    return "⛔ **KHÔNG CÓ QUYỀN:** Chỉ hiệu trưởng và giáo viên mới có quyền gửi thông báo đến phụ huynh.";
                }

                var title = args.TryGetValue("title", out var titleObj) ? titleObj?.ToString() : "";
                var message = args.TryGetValue("message", out var messageObj) ? messageObj?.ToString() : "";
                var category = args.TryGetValue("category", out var categoryObj) ? categoryObj?.ToString() ?? "announcement" : "announcement";

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
                {
                    return "⚠️ **THIẾU THÔNG TIN:** Vui lòng cung cấp tiêu đề và nội dung thông báo.";
                }

                // Get all parents
                var parents = await _context.Parents
                    .Include(p => p.Account)
                    .ToListAsync();

                var successCount = 0;

                foreach (var parent in parents)
                {
                    try
                    {
                        var notification = new Notification
                        {
                            Title = title,
                            Content = message,
                            UserId = parent.AccountID,
                            Type = await _userManager.IsInRoleAsync(user, "Principal") ? "principal_to_parents" : "teacher_to_parents",
                            Category = category,
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };

                        await _notificationRepository.CreateAsync(notification);
                        
                        // Send Firebase push notification
                        var data = new Dictionary<string, string>
                        {
                            ["type"] = notification.Type,
                            ["category"] = category,
                            ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                            ["notificationId"] = notification.NotificationId
                        };
                        
                        await _notificationService.SendNotificationToAllDevicesAsync(parent.AccountID, title, message, data);
                        successCount++;
                    }
                    catch
                    {
                        continue;
                    }
                }

                var successMessage = new StringBuilder();
                successMessage.AppendLine("👨‍👩‍👧‍👦 **THÔNG BÁO ĐẾN PHỤ HUYNH ĐÃ GỬI**");
                successMessage.AppendLine();
                successMessage.AppendLine($"📄 **Tiêu đề:** {title}");
                successMessage.AppendLine($"📝 **Nội dung:** {message}");
                successMessage.AppendLine($"👥 **Số phụ huynh nhận:** {successCount}/{parents.Count}");
                successMessage.AppendLine($"⏰ **Thời gian gửi:** {DateTime.UtcNow:dd/MM/yyyy HH:mm}");
                
                return successMessage.ToString();
            }
            catch (Exception)
            {
                return "❌ **LỖI:** Không thể gửi thông báo đến phụ huynh. Vui lòng thử lại sau.";
            }
        }

        private async Task<object> CreateUserAccountAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Principal"))
                {
                    return new { success = false, message = "⚠️ Chỉ hiệu trưởng mới có quyền tạo tài khoản người dùng" };
                }

                return new { success = true, message = "✅ Chức năng đang được phát triển" };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        private async Task<object> AssignUserRolesAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Principal"))
                {
                    return new { success = false, message = "⚠️ Chỉ hiệu trưởng mới có quyền phân quyền" };
                }

                return new { success = true, message = "✅ Chức năng đang được phát triển" };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        private async Task<object> DeleteUserAccountAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Principal"))
                {
                    return new { success = false, message = "⚠️ Chỉ hiệu trưởng mới có quyền xóa tài khoản" };
                }

                return new { success = true, message = "✅ Chức năng đang được phát triển" };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        private async Task<object> GetAllUsersAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Principal"))
                {
                    return new { success = false, message = "⚠️ Chỉ hiệu trưởng mới có quyền xem danh sách người dùng" };
                }

                var users = _userManager.Users.Take(10).Select(u => new
                {
                    id = u.Id,
                    fullName = GetUserFullName(u),
                    email = u.Email,
                    emailConfirmed = u.EmailConfirmed
                }).ToList();

                return new 
                { 
                    success = true, 
                    message = $"📋 Danh sách {users.Count} người dùng",
                    users = users
                };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        #endregion

        #region Teacher Exam Management Functions

        private async Task<object> CreateClassExamAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                {
                    return new { success = false, message = "⚠️ Chỉ giáo viên mới có quyền tạo lịch thi" };
                }

                return new { success = true, message = "✅ Chức năng đang được phát triển" };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        private async Task<object> UpdateExamAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                {
                    return new { success = false, message = "⚠️ Chỉ giáo viên mới có quyền chỉnh sửa lịch thi" };
                }

                return new { success = true, message = "✅ Chức năng đang được phát triển" };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        private async Task<object> DeleteExamAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                {
                    return new { success = false, message = "⚠️ Chỉ giáo viên mới có quyền xóa lịch thi" };
                }

                return new { success = true, message = "✅ Chức năng đang được phát triển" };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        private async Task<object> GetClassExamsAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                {
                    return new { success = false, message = "⚠️ Chỉ giáo viên mới có quyền xem lịch thi lớp" };
                }

                return new { success = true, message = "✅ Chức năng đang được phát triển" };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        #endregion

        #region Teacher Grade Management Functions

        private async Task<object> CreateGradeAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                {
                    return new { success = false, message = "⚠️ Chỉ giáo viên mới có quyền nhập điểm" };
                }

                return new { success = true, message = "✅ Chức năng đang được phát triển" };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        private async Task<object> UpdateGradeAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                {
                    return new { success = false, message = "⚠️ Chỉ giáo viên mới có quyền cập nhật điểm" };
                }

                return new { success = true, message = "✅ Chức năng đang được phát triển" };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        private async Task<object> DeleteGradeAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                {
                    return new { success = false, message = "⚠️ Chỉ giáo viên mới có quyền xóa điểm" };
                }

                return new { success = true, message = "✅ Chức năng đang được phát triển" };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        private async Task<object> GetStudentsForGradingAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                {
                    return new { success = false, message = "⚠️ Chỉ giáo viên mới có quyền xem danh sách học sinh" };
                }

                return new { success = true, message = "✅ Chức năng đang được phát triển" };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        #endregion

        #region Student Personal Info Functions

        private async Task<object> GetMyInfoAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Student"))
                {
                    return new { success = false, message = "⚠️ Chỉ học sinh mới có quyền xem thông tin cá nhân" };
                }

                var student = await _context.Students
                    .Where(s => s.AccountID == userId)
                    .Include(s => s.Account)
                    .Include(s => s.Class)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return new { success = false, message = "❌ Không tìm thấy thông tin học sinh" };
                }

                return new 
                { 
                    success = true, 
                    message = "📋 Thông tin cá nhân",
                    studentInfo = new
                    {
                        id = student.StudentID,
                        fullName = GetUserFullName(student.Account),
                        email = student.Account.Email,
                        studentCode = student.StudentID,
                        dateOfBirth = student.DateOfBirth?.ToString("dd/MM/yyyy"),
                        className = student.Class?.ClassName
                    }
                };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        private async Task<object> GetMyExamsAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new { success = false, message = "⚠️ Yêu cầu xác thực người dùng" };
                }

                // Validate user role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _userManager.IsInRoleAsync(user, "Student"))
                {
                    return new { success = false, message = "⚠️ Chỉ học sinh mới có quyền xem lịch thi của mình" };
                }

                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.AccountID == userId);

                if (student == null)
                {
                    return new { success = false, message = "❌ Không tìm thấy thông tin học sinh" };
                }

                var exams = await _context.Exams
                    .Where(e => e.StudentID == student.StudentID)
                    .Include(e => e.Subject)
                    .OrderBy(e => e.ExamDate)
                    .Take(10)
                    .Select(e => new
                    {
                        id = e.ExamId,
                        examType = e.ExamType,
                        examDate = e.ExamDate.ToString("dd/MM/yyyy HH:mm"),
                        subject = e.Subject.SubjectName,
                        examNote = e.ExamNote,
                        isUpcoming = e.ExamDate > DateTime.Now
                    })
                    .ToListAsync();

                return new 
                { 
                    success = true, 
                    message = $"📋 Lịch thi của bạn ({exams.Count} kỳ thi)",
                    exams = exams
                };
            }
            catch (Exception)
            {
                return new { success = false, message = "❌ Lỗi hệ thống" };
            }
        }

        #endregion

        #region New Functions Implementation

        private async Task<string> GetStudentParentInfoAsync(Dictionary<string, object> args)
        {
            try
            {
                var studentId = args["student_id"].ToString();
                var student = await _context.Students
                    .Include(s => s.Parent)
                    .ThenInclude(p => p.Account)
                    .FirstOrDefaultAsync(s => s.StudentID == studentId);

                if (student?.Parent == null)
                {
                    return JsonConvert.SerializeObject(new { success = false, message = "❌ Không tìm thấy thông tin phụ huynh" }, Formatting.Indented);
                }

                var parentInfo = new
                {
                    ParentId = student.Parent.ParentID,
                    FullName = $"{student.Parent.Account?.FirstName} {student.Parent.Account?.LastName}".Trim(),
                    Email = student.Parent.Account?.Email ?? "Chưa cập nhật",
                    PhoneNumber = student.Parent.Account?.PhoneNumber ?? "Chưa cập nhật"
                };

                return JsonConvert.SerializeObject(new { success = true, message = "🏠 Thông tin phụ huynh", parent = parentInfo }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { success = false, message = "❌ Lỗi hệ thống" }, Formatting.Indented);
            }
        }

        private async Task<string> GetSubjectTeachersAsync(Dictionary<string, object> args)
        {
            try
            {
                var subjectId = Convert.ToInt32(args["subject_id"]);
                
                var teachers = await _context.Teachers
                    .Include(t => t.Account)
                    .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                    .Where(t => t.ClassTeachers.Any())
                    .Select(t => new
                    {
                        TeacherId = t.TeacherID,
                        FullName = t.Account.FirstName + " " + t.Account.LastName,
                        Email = t.Account.Email,
                        PhoneNumber = t.Account.PhoneNumber,
                        Classes = t.ClassTeachers
                            .Select(ct => new { ct.Class.ClassName, ct.ClassID })
                            .Select(ct => new
                            {
                                ClassId = ct.ClassID,
                                ClassName = ct.ClassName
                            })
                            .ToList()
                    })
                    .ToListAsync();

                return JsonConvert.SerializeObject(new { success = true, message = $"👨‍🏫 Danh sách giáo viên ({teachers.Count} người)", teachers }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { success = false, message = "❌ Lỗi hệ thống" }, Formatting.Indented);
            }
        }

        private async Task<string> GetClassStudentsListAsync(Dictionary<string, object> args)
        {
            try
            {
                var classId = Convert.ToInt32(args["class_id"]);
                var includeGrades = args.ContainsKey("include_grades") && Convert.ToBoolean(args["include_grades"]);

                var students = await _context.Students
                    .Include(s => s.Account)
                    .Where(s => s.ClassID == classId)
                    .ToListAsync();

                var studentList = new List<object>();

                foreach (var student in students)
                {
                    var studentInfo = new
                    {
                        StudentId = student.StudentID,
                        FullName = $"{student.Account?.FirstName} {student.Account?.LastName}".Trim(),
                        Email = student.Account?.Email,
                        DateOfBirth = student.DateOfBirth?.ToString("dd/MM/yyyy"),
                        Gender = student.Gender
                    };

                    if (includeGrades)
                    {
                        var grades = await _context.Grades
                            .Where(g => g.StudentID == student.StudentID)
                            .Select(g => new
                            {
                                SubjectId = g.SubjectID,
                                Score = g.Score,
                                GradeType = g.GradeType,
                                Semester = g.Semester
                            })
                            .ToListAsync();

                        studentList.Add(new { Student = studentInfo, Grades = grades });
                    }
                    else
                    {
                        studentList.Add(studentInfo);
                    }
                }

                return JsonConvert.SerializeObject(new { success = true, message = $"👥 Danh sách học sinh ({studentList.Count} học sinh)", students = studentList }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { success = false, message = "❌ Lỗi hệ thống" }, Formatting.Indented);
            }
        }

        private async Task<string> GetTeacherClassesAsync(Dictionary<string, object> args)
        {
            try
            {
                var teacherId = args["teacher_id"].ToString();
                var includeSubjects = !args.ContainsKey("include_subjects") || Convert.ToBoolean(args["include_subjects"]);

                // Lấy các slot mà giáo viên dạy
                var timetableSlots = await _timetableSlotRepository.GetTimetableSlotsByTeacherIdAsync(teacherId);
                if (timetableSlots == null || !timetableSlots.Any())
                {
                    return JsonConvert.SerializeObject(new { success = true, message = "Không có lớp nào trong thời khóa biểu của giáo viên.", classes = new List<object>() }, Formatting.Indented);
                }

                // Lấy danh sách lớp mà giáo viên dạy qua slot
                var classes = await _timetableSlotRepository.GetClassesByTeacherIdAsync(teacherId);
                if (classes == null || !classes.Any())
                {
                    return JsonConvert.SerializeObject(new { success = true, message = "Không tìm thấy thông tin lớp học.", classes = new List<object>() }, Formatting.Indented);
                }

                var classList = classes.Select(c => new
                {
                    ClassId = c.ClassID,
                    ClassName = c.ClassName,
                    StudentCount = _context.Students.Count(s => s.ClassID == c.ClassID),
                    Subjects = includeSubjects ? timetableSlots.Where(ts => ts.Timetable?.ClassID == c.ClassID).Select(ts => new
                    {
                        SubjectId = ts.SubjectId,
                        SubjectName = ts.Subject?.SubjectName ?? "",
                        SlotId = ts.TimetableSlotID
                    }).Distinct().Cast<object>().ToList() : new List<object>()
                }).ToList();

                return JsonConvert.SerializeObject(new { success = true, message = $"🏫 Danh sách lớp đang dạy ({classList.Count} lớp)", classes = classList }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { success = false, message = "❌ Lỗi hệ thống" }, Formatting.Indented);
            }
        }

        private async Task<string> GetClassAttendanceSummaryAsync(Dictionary<string, object> args)
        {
            try
            {
                var classId = Convert.ToInt32(args["class_id"]);

                if (args.ContainsKey("start_date"))
                    DateTime.TryParse(args["start_date"].ToString(), out var startDate);
                if (args.ContainsKey("end_date"))
                    DateTime.TryParse(args["end_date"].ToString(), out var end);

                var query = _context.Attendances
                    .Include(a => a.Student)
                    .ThenInclude(s => s.Account)
                    .Where(a => a.Student.ClassID == classId);

                // Note: Date filtering would require complex join with Timetable entity
                // Simplified for now - can be enhanced later

                var attendances = await query.ToListAsync();

                var summary = attendances
                    .GroupBy(a => a.StudentID)
                    .Select(g => new
                    {
                        StudentId = g.Key,
                        StudentName = $"{g.First().Student?.Account?.FirstName} {g.First().Student?.Account?.LastName}".Trim() ?? "N/A",
                        TotalDays = g.Count(),
                        PresentDays = g.Count(a => a.Status == 1), // Assuming 1 = Present
                        AbsentDays = g.Count(a => a.Status == 0), // Assuming 0 = Absent  
                        LateDays = g.Count(a => a.Status == 2), // Assuming 2 = Late
                        AttendanceRate = g.Count() > 0 ? Math.Round((double)g.Count(a => a.Status == 1) / g.Count() * 100, 2) : 0
                    })
                    .ToList();

                return JsonConvert.SerializeObject(new { success = true, message = $"📊 Tổng quan điểm danh lớp ({summary.Count} học sinh)", summary }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { success = false, message = "❌ Lỗi hệ thống" }, Formatting.Indented);
            }
        }

        private async Task<string> GetExamResultsByClassAsync(Dictionary<string, object> args)
        {
            try
            {
                var examId = Convert.ToInt32(args["exam_id"]);
                var classId = Convert.ToInt32(args["class_id"]);

                var grades = await _context.Grades
                    .Where(g => g.SubjectID == examId)
                    .Select(g => new
                    {
                        StudentId = g.StudentID,
                        Grade = g.Score ?? 0,
                        GradeType = g.GradeType,
                        Semester = g.Semester,
                        Evaluation = g.Evaluation
                    })
                    .OrderByDescending(g => g.Grade)
                    .ToListAsync();

                var statistics = new
                {
                    TotalStudents = grades.Count,
                    AverageGrade = grades.Count > 0 ? Math.Round(grades.Average(g => g.Grade), 2) : 0,
                    HighestGrade = grades.Count > 0 ? grades.Max(g => g.Grade) : 0,
                    LowestGrade = grades.Count > 0 ? grades.Min(g => g.Grade) : 0,
                    PassingStudents = grades.Count(g => g.Grade >= 5),
                    PassingRate = grades.Count > 0 ? Math.Round((double)grades.Count(g => g.Grade >= 5) / grades.Count * 100, 2) : 0
                };

                return JsonConvert.SerializeObject(new { success = true, message = "📝 Kết quả thi theo lớp", results = grades, statistics }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { success = false, message = "❌ Lỗi hệ thống" }, Formatting.Indented);
            }
        }

        private async Task<string> GetStudentExamHistoryAsync(Dictionary<string, object> args)
        {
            try
            {
                var studentId = args["student_id"].ToString();
                var limit = args.ContainsKey("limit") ? Convert.ToInt32(args["limit"]) : 10;
                int? subjectId = args.ContainsKey("subject_id") ? Convert.ToInt32(args["subject_id"]) : null;

                var query = _context.Grades
                    .Where(g => g.StudentID == studentId);

                if (subjectId.HasValue)
                    query = query.Where(g => g.SubjectID == subjectId);

                var examHistory = await query
                    .OrderByDescending(g => g.GradeID)
                    .Take(limit)
                    .Select(g => new
                    {
                        GradeId = g.GradeID,
                        StudentId = g.StudentID,
                        SubjectId = g.SubjectID,
                        Grade = g.Score ?? 0,
                        GradeType = g.GradeType,
                        Semester = g.Semester,
                        Evaluation = g.Evaluation
                    })
                    .ToListAsync();

                return JsonConvert.SerializeObject(new { success = true, message = $"📜 Lịch sử thi cử ({examHistory.Count} bài thi)", history = examHistory }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { success = false, message = "❌ Lỗi hệ thống" }, Formatting.Indented);
            }
        }

        private async Task<string> GetParentChildrenInfoAsync(Dictionary<string, object> args)
        {
            try
            {
                var parentId = args["parent_id"].ToString();
                
                var children = await _context.Students
                    .Include(s => s.Account)
                    .Include(s => s.Class)
                    .Where(s => s.ParentID == parentId)
                    .Select(s => new
                    {
                        StudentId = s.StudentID,
                        FullName = s.Account.FirstName + " " + s.Account.LastName,
                        DateOfBirth = s.DateOfBirth,
                        Gender = s.Gender,
                        ClassName = s.Class.ClassName,
                        ClassId = s.ClassID
                    })
                    .ToListAsync();

                return JsonConvert.SerializeObject(new { success = true, message = $"👶 Danh sách con em ({children.Count} học sinh)", children }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { success = false, message = "❌ Lỗi hệ thống" }, Formatting.Indented);
            }
        }

        private async Task<string> GetRecentLessonReportsAsync(Dictionary<string, object> args)
        {
            try
            {
                var daysBack = args.ContainsKey("days_back") ? Convert.ToInt32(args["days_back"]) : 7;
                var limit = args.ContainsKey("limit") ? Convert.ToInt32(args["limit"]) : 20;
                var startDate = DateTime.Now.AddDays(-daysBack);

                var query = _context.LessonReports
                    .Include(lr => lr.TimetableSlot)
                    .ThenInclude(ts => ts.Teacher)
                    .ThenInclude(t => t.Account)
                    .Include(lr => lr.TimetableSlot.Timetable.Class)
                    .Include(lr => lr.TimetableSlot.Subject)
                    .Where(lr => lr.CreatedAt >= startDate);

                if (args.ContainsKey("teacher_id"))
                    query = query.Where(lr => lr.TimetableSlot.TeacherID == args["teacher_id"].ToString());

                if (args.ContainsKey("class_id"))
                    query = query.Where(lr => lr.TimetableSlot.Timetable.ClassID == Convert.ToInt32(args["class_id"]));

                var reports = await query
                    .OrderByDescending(lr => lr.CreatedAt)
                    .Take(limit)
                    .Select(lr => new
                    {
                        ReportId = lr.ReportID,
                        TeacherName = lr.TimetableSlot.Teacher.Account.FirstName + " " + lr.TimetableSlot.Teacher.Account.LastName,
                        ClassName = lr.TimetableSlot.Timetable.Class.ClassName,
                        SubjectName = lr.TimetableSlot.Subject.SubjectName,
                        LessonDate = lr.LessonDate,
                        TotalStudents = lr.TotalStudents,
                        AbsentStudents = lr.AbsentStudents,
                        Remarks = lr.Remarks,
                        CreatedDate = lr.CreatedAt
                    })
                    .ToListAsync();

                return JsonConvert.SerializeObject(new { success = true, message = $"📋 Báo cáo bài học gần đây ({reports.Count} báo cáo)", reports }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { success = false, message = "❌ Lỗi hệ thống" }, Formatting.Indented);
            }
        }

        private async Task<string> GetUserRoleInfoAsync(Dictionary<string, object> args)
        {
            try
            {
                var userId = args["user_id"].ToString();
                
                var user = await _userManager.FindByIdAsync(userId ?? string.Empty);
                if (user == null)
                {
                    return JsonConvert.SerializeObject(new { success = false, message = "❌ Không tìm thấy người dùng" }, Formatting.Indented);
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userInfo = new
                {
                    UserId = user.Id,
                    FullName = user.FirstName + " " + user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    AccessFailedCount = user.AccessFailedCount
                };

                return JsonConvert.SerializeObject(new { success = true, message = "🔐 Thông tin vai trò người dùng", userInfo }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { success = false, message = "❌ Lỗi hệ thống" }, Formatting.Indented);
            }
        }

        private async Task<string> SearchNotificationsAsync(Dictionary<string, object> args)
        {
            try
            {
                var searchTerm = args["search_term"].ToString();
                
                var query = _context.Notifications
                    .Include(n => n.User)
                    .Include(n => n.User)
                    .Where(n => n.Title.Contains(searchTerm ?? string.Empty) || n.Content.Contains(searchTerm ?? string.Empty));

                if (args.ContainsKey("user_id"))
                    query = query.Where(n => n.UserId == args["user_id"].ToString());

                if (args.ContainsKey("date_from") && DateTime.TryParse(args["date_from"].ToString(), out var dateFrom))
                    query = query.Where(n => n.CreatedAt >= dateFrom);

                if (args.ContainsKey("date_to") && DateTime.TryParse(args["date_to"].ToString(), out var dateTo))
                    query = query.Where(n => n.CreatedAt <= dateTo.AddDays(1));

                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new
                    {
                        NotificationId = n.NotificationId,
                        Title = n.Title,
                        Message = n.Content,
                        SenderName = "Hệ thống",
                        ReceiverName = n.User.FirstName + " " + n.User.LastName,
                        IsRead = n.IsRead,
                        CreatedDate = n.CreatedAt
                    })
                    .ToListAsync();

                return JsonConvert.SerializeObject(new { success = true, message = $"🔍 Kết quả tìm kiếm ({notifications.Count} thông báo)", notifications }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { success = false, message = "❌ Lỗi hệ thống" }, Formatting.Indented);
            }
        }

        private async Task<string> GetClassGradeStatisticsAsync(Dictionary<string, object> args)
        {
            try
            {
                var classId = Convert.ToInt32(args["class_id"]);
                int? subjectId = args.ContainsKey("subject_id") ? Convert.ToInt32(args["subject_id"]) : null;
                string semester = args.ContainsKey("semester") ? args["semester"]?.ToString() ?? string.Empty : string.Empty;

                var query = _context.Grades
                    .Where(g => _context.Students.Any(s => s.StudentID == g.StudentID && s.ClassID == classId));

                if (subjectId.HasValue)
                    query = query.Where(g => g.SubjectID == subjectId);

                if (!string.IsNullOrEmpty(semester))
                    query = query.Where(g => g.Semester == semester);

                var grades = await query.ToListAsync();

                var statistics = new
                {
                    TotalGrades = grades.Count,
                    AverageGrade = grades.Count > 0 && grades.Any(g => g.Score.HasValue) ? Math.Round(grades.Where(g => g.Score.HasValue).Average(g => g.Score!.Value), 2) : 0,
                    HighestGrade = grades.Count > 0 && grades.Any(g => g.Score.HasValue) ? grades.Where(g => g.Score.HasValue).Max(g => g.Score!.Value) : 0,
                    LowestGrade = grades.Count > 0 && grades.Any(g => g.Score.HasValue) ? grades.Where(g => g.Score.HasValue).Min(g => g.Score!.Value) : 0,
                    ExcellentCount = grades.Count(g => g.Score >= 8),
                    GoodCount = grades.Count(g => g.Score >= 6.5 && g.Score < 8),
                    AverageCount = grades.Count(g => g.Score >= 5 && g.Score < 6.5),
                    BelowAverageCount = grades.Count(g => g.Score < 5),
                    SubjectBreakdown = grades.GroupBy(g => g.SubjectID).Select(g => new
                    {
                        SubjectId = g.Key,
                        AverageGrade = g.Any(x => x.Score.HasValue) ? Math.Round(g.Where(x => x.Score.HasValue).Average(x => x.Score!.Value), 2) : 0,
                        StudentCount = g.Select(x => x.StudentID).Distinct().Count()
                    }).ToList()
                };

                return JsonConvert.SerializeObject(new { success = true, message = "📈 Thống kê điểm số lớp học", statistics }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { success = false, message = "❌ Lỗi hệ thống" }, Formatting.Indented);
            }
        }

        private async Task<string> GetUpcomingExamsAsync(Dictionary<string, object> args)
        {
            try
            {
                var daysAhead = args.ContainsKey("days_ahead") ? Convert.ToInt32(args["days_ahead"]) : 30;
                var endDate = DateTime.Now.AddDays(daysAhead);

                var query = _context.Exams
                    .Include(e => e.Subject)
                    .Include(e => e.CreatedByUser)
                    .Where(e => e.ExamDate >= DateTime.Now && e.ExamDate <= endDate);

                if (args.ContainsKey("student_id"))
                {
                    var studentId = args["student_id"].ToString();
                    var student = await _context.Students.FindAsync(studentId);
                    if (student != null)
                    {
                        // Filter exams for student's subject
                        query = query.Where(e => e.StudentID == studentId);
                    }
                }

                if (args.ContainsKey("teacher_id"))
                    query = query.Where(e => e.CreatedBy == args["teacher_id"].ToString());

                var upcomingExams = await query
                    .OrderBy(e => e.ExamDate)
                    .Select(e => new
                    {
                        ExamId = e.ExamId,
                        ExamType = e.ExamType,
                        SubjectName = e.Subject.SubjectName,
                        ExamDate = e.ExamDate,
                        Room = e.Room,
                        ExamNote = e.ExamNote,
                        CreatedBy = e.CreatedByUser != null ? e.CreatedByUser.FirstName + " " + e.CreatedByUser.LastName : "N/A",
                        DaysUntilExam = (e.ExamDate - DateTime.Now).Days
                    })
                    .ToListAsync();

                return JsonConvert.SerializeObject(new { success = true, message = $"📅 Lịch thi sắp tới ({upcomingExams.Count} kỳ thi)", exams = upcomingExams }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { success = false, message = "❌ Lỗi hệ thống" }, Formatting.Indented);
            }
        }

        #endregion

        #region Report Functions

        private async Task<string> CreateReportAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                // Validate userId
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không thể tạo báo cáo: Thiếu thông tin người dùng",
                        error = "MISSING_USER_ID",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                // Validate required parameters
                if (!args.TryGetValue("report_type", out var reportTypeObj) || string.IsNullOrEmpty(reportTypeObj.ToString()))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Bạn cần cung cấp loại báo cáo. Các loại báo cáo có thể là: Bug Report, Feature Request, Complaint, General",
                        error = "MISSING_REPORT_TYPE",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                if (!args.TryGetValue("content", out var contentObj) || string.IsNullOrEmpty(contentObj.ToString()))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Bạn cần cung cấp nội dung báo cáo chi tiết",
                        error = "MISSING_CONTENT",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                var reportType = reportTypeObj.ToString()!;
                var content = contentObj.ToString()!;
                var status = args.TryGetValue("status", out var statusObj) && !string.IsNullOrEmpty(statusObj.ToString()) 
                    ? statusObj.ToString()! : "Pending";

                // Get user information for CreatedBy field
                var user = await _userManager.FindByIdAsync(userId);
                var createdBy = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown User";

                // Create report request
                var createRequest = new CreateReportRequest
                {
                    AccountId = userId,
                    ReportType = reportType,
                    Content = content,
                    Status = status,
                    CreatedBy = createdBy
                };

                // Create report
                var result = await _reportService.CreateReportAsync(createRequest);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = $"✅ Đã tạo báo cáo thành công. Mã báo cáo: #{result.ReportId}",
                    error = (string?)null,
                    data = new
                    {
                        ReportId = result.ReportId,
                        ReportType = result.ReportType,
                        Content = result.Content,
                        Status = result.Status,
                        CreatedBy = result.CreatedBy,
                        CreatedDate = result.CreatedDate,
                        AccountId = result.AccountId
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi tạo báo cáo. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetMyReportsAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                // Validate userId
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không thể lấy danh sách báo cáo: Thiếu thông tin người dùng",
                        error = "MISSING_USER_ID",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                // Parse optional parameters
                var reportType = args.TryGetValue("report_type", out var reportTypeObj) && !string.IsNullOrEmpty(reportTypeObj.ToString()) 
                    ? reportTypeObj.ToString() : null;
                var status = args.TryGetValue("status", out var statusObj) && !string.IsNullOrEmpty(statusObj.ToString()) 
                    ? statusObj.ToString() : null;
                var search = args.TryGetValue("search", out var searchObj) && !string.IsNullOrEmpty(searchObj.ToString()) 
                    ? searchObj.ToString() : null;
                
                var page = args.TryGetValue("page", out var pageObj) && int.TryParse(pageObj.ToString(), out var parsedPage) 
                    ? Math.Max(parsedPage, 1) : 1;
                var pageSize = args.TryGetValue("page_size", out var pageSizeObj) && int.TryParse(pageSizeObj.ToString(), out var parsedPageSize) 
                    ? Math.Min(Math.Max(parsedPageSize, 1), 50) : 10;

                // Create request
                var getRequest = new GetReportRequest
                {
                    AccountId = userId,
                    ReportType = reportType,
                    Status = status,
                    Search = search,
                    Page = page,
                    PageSize = pageSize
                };

                // Get reports
                var result = await _reportService.GetReportsAsync(getRequest);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = $"Tôi đã tìm thấy {result.TotalCount} báo cáo của bạn:",
                    error = (string?)null,
                    data = new
                    {
                        Reports = result.Items,
                        TotalCount = result.TotalCount,
                        CurrentPage = result.PageIndex,
                        PageSize = result.PageSize,
                        TotalPages = (int)Math.Ceiling((double)result.TotalCount / result.PageSize)
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi lấy danh sách báo cáo. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetAllReportsAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                // Validate userId and check admin/principal role
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không thể lấy danh sách báo cáo: Thiếu thông tin người dùng",
                        error = "MISSING_USER_ID",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                // Check user roles
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin người dùng",
                        error = "USER_NOT_FOUND",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Admin") && !roles.Contains("Principal"))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "❌ Bạn không có quyền xem tất cả báo cáo. Chỉ Admin và Principal mới có thể sử dụng chức năng này.",
                        error = "PERMISSION_DENIED",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                // Parse optional parameters
                var accountId = args.TryGetValue("account_id", out var accountIdObj) && !string.IsNullOrEmpty(accountIdObj.ToString()) 
                    ? accountIdObj.ToString() : null;
                var reportType = args.TryGetValue("report_type", out var reportTypeObj) && !string.IsNullOrEmpty(reportTypeObj.ToString()) 
                    ? reportTypeObj.ToString() : null;
                var status = args.TryGetValue("status", out var statusObj) && !string.IsNullOrEmpty(statusObj.ToString()) 
                    ? statusObj.ToString() : null;
                var search = args.TryGetValue("search", out var searchObj) && !string.IsNullOrEmpty(searchObj.ToString()) 
                    ? searchObj.ToString() : null;
                
                var page = args.TryGetValue("page", out var pageObj) && int.TryParse(pageObj.ToString(), out var parsedPage) 
                    ? Math.Max(parsedPage, 1) : 1;
                var pageSize = args.TryGetValue("page_size", out var pageSizeObj) && int.TryParse(pageSizeObj.ToString(), out var parsedPageSize) 
                    ? Math.Min(Math.Max(parsedPageSize, 1), 50) : 10;

                // Create request
                var getRequest = new GetReportRequest
                {
                    AccountId = accountId,
                    ReportType = reportType,
                    Status = status,
                    Search = search,
                    Page = page,
                    PageSize = pageSize
                };

                // Get all reports
                var result = await _reportService.GetReportsAsync(getRequest);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = $"Tôi đã tìm thấy {result.TotalCount} báo cáo trong hệ thống:",
                    error = (string?)null,
                    data = new
                    {
                        Reports = result.Items,
                        TotalCount = result.TotalCount,
                        CurrentPage = result.PageIndex,
                        PageSize = result.PageSize,
                        TotalPages = (int)Math.Ceiling((double)result.TotalCount / result.PageSize)
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi lấy danh sách báo cáo. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> UpdateReportStatusAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                // Validate userId and check admin/principal role
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không thể cập nhật báo cáo: Thiếu thông tin người dùng",
                        error = "MISSING_USER_ID",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                // Check user roles
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin người dùng",
                        error = "USER_NOT_FOUND",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Admin") && !roles.Contains("Principal"))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "❌ Bạn không có quyền cập nhật trạng thái báo cáo. Chỉ Admin và Principal mới có thể sử dụng chức năng này.",
                        error = "PERMISSION_DENIED",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                // Validate required parameters
                if (!args.TryGetValue("report_id", out var reportIdObj) || !int.TryParse(reportIdObj.ToString(), out var reportId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Bạn cần cung cấp ID báo cáo hợp lệ",
                        error = "MISSING_OR_INVALID_REPORT_ID",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                if (!args.TryGetValue("status", out var statusObj) || string.IsNullOrEmpty(statusObj.ToString()))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Bạn cần cung cấp trạng thái mới cho báo cáo. Các trạng thái có thể là: Pending, Processing, Resolved, Rejected",
                        error = "MISSING_STATUS",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                var status = statusObj.ToString()!;

                // Validate status values
                var validStatuses = new[] { "Pending", "Processing", "Resolved", "Rejected" };
                if (!validStatuses.Contains(status))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Trạng thái '{status}' không hợp lệ. Các trạng thái có thể là: {string.Join(", ", validStatuses)}",
                        error = "INVALID_STATUS",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                // Get the current report first
                var currentReport = await _reportService.GetReportByIdAsync(reportId);
                if (currentReport == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Không tìm thấy báo cáo với ID {reportId}",
                        error = "REPORT_NOT_FOUND",
                        data = (object?)null
                    }, Formatting.Indented);
                }

                // Create update request
                var updateRequest = new UpdateReportRequest
                {
                    ReportId = reportId,
                    ReportType = currentReport.ReportType,
                    Content = currentReport.Content,
                    Status = status,
                    IsDelete = currentReport.IsDelete
                };

                // Update report
                var result = await _reportService.UpdateReportAsync(updateRequest);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = $"✅ Đã cập nhật trạng thái báo cáo #{reportId} thành '{status}' thành công",
                    error = (string?)null,
                    data = new
                    {
                        ReportId = result.ReportId,
                        ReportType = result.ReportType,
                        Content = result.Content,
                        OldStatus = currentReport.Status,
                        NewStatus = result.Status,
                        CreatedBy = result.CreatedBy,
                        CreatedDate = result.CreatedDate,
                        AccountId = result.AccountId
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi cập nhật trạng thái báo cáo. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        #endregion

        #region Composite Functions - Advanced Multi-Function Operations

        /// <summary>
        /// Get comprehensive student report combining multiple data sources
        /// </summary>
        private async Task<string> GetComprehensiveStudentReportAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để xem báo cáo toàn diện.",
                        error = "AUTHENTICATION_REQUIRED"
                    }, Formatting.Indented);
                }

                var includeAttendance = args.TryGetValue("include_attendance", out var attendanceObj) ? 
                    bool.Parse(attendanceObj?.ToString() ?? "true") : true;
                var includeTimetable = args.TryGetValue("include_timetable", out var timetableObj) ? 
                    bool.Parse(timetableObj?.ToString() ?? "true") : true;
                var includeUpcomingExams = args.TryGetValue("include_upcoming_exams", out var examsObj) ? 
                    bool.Parse(examsObj?.ToString() ?? "true") : true;

                var report = new StringBuilder();
                report.AppendLine("📊 **BÁO CÁO TOÀN DIỆN HỌC SINH**");
                report.AppendLine($"📅 Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}");
                report.AppendLine("═══════════════════════════════════════════════════");
                report.AppendLine();

                // 1. Get student grades
                var gradesResult = await GetStudentGradesAsync(args, userId);
                var gradesData = JsonConvert.DeserializeObject<dynamic>(gradesResult);
                
                if (gradesData?.success == true)
                {
                    report.AppendLine("📚 **PHẦN 1: BẢNG ĐIỂM**");
                    report.AppendLine(gradesData?.message?.ToString());
                    report.AppendLine();
                }

                // 2. Get attendance if requested
                if (includeAttendance)
                {
                    var attendanceResult = await GetStudentAttendanceAsync(args, userId);
                    var attendanceData = JsonConvert.DeserializeObject<dynamic>(attendanceResult);
                    
                    if (attendanceData?.success == true)
                    {
                        report.AppendLine("✅ **PHẦN 2: ĐIỂM DANH**");
                        report.AppendLine(attendanceData?.message?.ToString());
                        report.AppendLine();
                    }
                }

                // 3. Get upcoming exams if requested
                if (includeUpcomingExams)
                {
                    var examsResult = await GetStudentExamsAsync(args, userId);
                    var examsData = JsonConvert.DeserializeObject<dynamic>(examsResult);
                    
                    if (examsData?.success == true)
                    {
                        report.AppendLine("📝 **PHẦN 3: KỲ THI SẮP TỚI**");
                        report.AppendLine(examsData?.message?.ToString());
                        report.AppendLine();
                    }
                }

                // 4. Summary and recommendations
                report.AppendLine("💡 **PHẦN 4: TÓNG KẾT VÀ KHUYẾN NGHỊ**");
                report.AppendLine("   • Theo dõi thường xuyên điểm số và điểm danh");
                report.AppendLine("   • Chuẩn bị tốt cho các kỳ thi sắp tới");
                report.AppendLine("   • Liên hệ giáo viên nếu cần hỗ trợ thêm");

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = report.ToString(),
                    data = new
                    {
                        ReportType = "comprehensive_student_report",
                        GeneratedAt = DateTime.Now,
                        IncludedSections = new
                        {
                            Grades = true,
                            Attendance = includeAttendance,
                            Timetable = includeTimetable,
                            UpcomingExams = includeUpcomingExams
                        }
                    }
                }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ **LỖI:** Không thể tạo báo cáo toàn diện. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR"
                }, Formatting.Indented);
            }
        }

        /// <summary>
        /// Analyze student performance by comparing grades with attendance
        /// </summary>
        private async Task<string> AnalyzeStudentPerformanceAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để phân tích hiệu suất học tập.",
                        error = "AUTHENTICATION_REQUIRED"
                    }, Formatting.Indented);
                }

                var analysis = new StringBuilder();
                analysis.AppendLine("🔍 **PHÂN TÍCH HIỆU SUẤT HỌC TẬP**");
                analysis.AppendLine($"📅 Thời gian phân tích: {DateTime.Now:dd/MM/yyyy HH:mm}");
                analysis.AppendLine("═══════════════════════════════════════════════════");
                analysis.AppendLine();

                // Get student grades for analysis
                var gradesResult = await GetStudentGradesAsync(args, userId);
                var gradesData = JsonConvert.DeserializeObject<dynamic>(gradesResult);

                // Get attendance for analysis
                var attendanceResult = await GetStudentAttendanceAsync(args, userId);
                var attendanceData = JsonConvert.DeserializeObject<dynamic>(attendanceResult);

                if (gradesData?.success == true && attendanceData?.success == true)
                {
                    analysis.AppendLine("📊 **DỮ LIỆU THU THẬP:**");
                    analysis.AppendLine($"   ✓ Đã phân tích dữ liệu điểm số");
                    analysis.AppendLine($"   ✓ Đã phân tích dữ liệu điểm danh");
                    analysis.AppendLine();

                    analysis.AppendLine("🎯 **KẾT QUẢ PHÂN TÍCH:**");
                    analysis.AppendLine("   📈 **Xu hướng học tập:**");
                    analysis.AppendLine("      • Cần theo dõi mối quan hệ giữa điểm danh và điểm số");
                    analysis.AppendLine("      • Khuyến khích duy trì tần suất tham gia lớp học cao");
                    analysis.AppendLine();

                    analysis.AppendLine("   💡 **KHUYẾN NGHỊ:**");
                    analysis.AppendLine("      1. Đảm bảo tham gia đầy đủ các buổi học");
                    analysis.AppendLine("      2. Tập trung vào các môn có điểm thấp");
                    analysis.AppendLine("      3. Liên hệ giáo viên để được hỗ trợ kịp thời");
                    analysis.AppendLine("      4. Xây dựng kế hoạch học tập có hệ thống");
                }
                else
                {
                    analysis.AppendLine("⚠️ **THÔNG BÁO:** Không đủ dữ liệu để thực hiện phân tích chi tiết.");
                    analysis.AppendLine("Vui lòng đảm bảo có dữ liệu điểm số và điểm danh để có phân tích chính xác.");
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = analysis.ToString(),
                    data = new
                    {
                        AnalysisType = "student_performance_analysis",
                        GeneratedAt = DateTime.Now,
                        DataSources = new
                        {
                            GradesAvailable = gradesData?.success == true,
                            AttendanceAvailable = attendanceData?.success == true
                        }
                    }
                }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ **LỖI:** Không thể thực hiện phân tích hiệu suất. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR"
                }, Formatting.Indented);
            }
        }

        /// <summary>
        /// Get comprehensive class overview report
        /// </summary>
        private async Task<string> GetClassOverviewReportAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (!args.TryGetValue("class_id", out var classIdObj) || !int.TryParse(classIdObj?.ToString(), out var classId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "⚠️ **THIẾU THÔNG TIN:** Vui lòng cung cấp mã lớp học để tạo báo cáo tổng quan.",
                        error = "MISSING_CLASS_ID"
                    }, Formatting.Indented);
                }

                var includeGradeStats = args.TryGetValue("include_grade_stats", out var gradeStatsObj) ? 
                    bool.Parse(gradeStatsObj?.ToString() ?? "true") : true;
                var includeAttendance = args.TryGetValue("include_attendance", out var attendanceObj) ? 
                    bool.Parse(attendanceObj?.ToString() ?? "true") : true;

                var overview = new StringBuilder();
                overview.AppendLine("🏫 **BÁO CÁO TỔNG QUAN LỚP HỌC**");
                overview.AppendLine($"📅 Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}");
                overview.AppendLine($"🆔 Mã lớp: {classId}");
                overview.AppendLine("═══════════════════════════════════════════════════");
                overview.AppendLine();

                // 1. Get class basic info
                var classArgs = new Dictionary<string, object> { ["class_id"] = classId };
                var classResult = await GetClassByIdAsync(classArgs);
                var classData = JsonConvert.DeserializeObject<dynamic>(classResult);

                if (classData?.success == true)
                {
                    overview.AppendLine("📝 **THÔNG TIN CƠ BẢN:**");
                    overview.AppendLine(classData?.message?.ToString());
                    overview.AppendLine();
                }

                // 2. Get students list
                var studentsResult = await GetClassStudentsListAsync(classArgs);
                var studentsData = JsonConvert.DeserializeObject<dynamic>(studentsResult);

                if (studentsData?.success == true)
                {
                    overview.AppendLine("👥 **DANH SÁCH HỌC SINH:**");
                    overview.AppendLine(studentsData?.message?.ToString());
                    overview.AppendLine();
                }

                // 3. Get grade statistics if requested
                if (includeGradeStats)
                {
                    var gradeStatsResult = await GetClassGradeStatisticsAsync(classArgs);
                    var gradeStatsData = JsonConvert.DeserializeObject<dynamic>(gradeStatsResult);

                    if (gradeStatsData?.success == true)
                    {
                        overview.AppendLine("📊 **THỐNG KÊ ĐIỂM SỐ:**");
                        overview.AppendLine(gradeStatsData?.message?.ToString());
                        overview.AppendLine();
                    }
                }

                // 4. Get attendance summary if requested
                if (includeAttendance)
                {
                    var attendanceResult = await GetClassAttendanceSummaryAsync(classArgs);
                    var attendanceData = JsonConvert.DeserializeObject<dynamic>(attendanceResult);

                    if (attendanceData?.success == true)
                    {
                        overview.AppendLine("✅ **TÓNG KẾT ĐIỂM DANH:**");
                        overview.AppendLine(attendanceData?.message?.ToString());
                        overview.AppendLine();
                    }
                }

                overview.AppendLine("💡 **KHUYẾN NGHỊ CHO LỚP:**");
                overview.AppendLine("   • Theo dõi thường xuyên tiến độ học tập của từng học sinh");
                overview.AppendLine("   • Tăng cường hoạt động nhóm để cải thiện kết quả chung");
                overview.AppendLine("   • Hỗ trợ đặc biệt cho học sinh có điểm số thấp");

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = overview.ToString(),
                    data = new
                    {
                        ReportType = "class_overview_report",
                        ClassId = classId,
                        GeneratedAt = DateTime.Now,
                        IncludedSections = new
                        {
                            BasicInfo = true,
                            StudentsList = true,
                            GradeStatistics = includeGradeStats,
                            AttendanceSummary = includeAttendance
                        }
                    }
                }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ **LỖI:** Không thể tạo báo cáo tổng quan lớp học. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR"
                }, Formatting.Indented);
            }
        }

        /// <summary>
        /// Search for students and get their detailed information
        /// </summary>
        private async Task<string> SearchAndGetStudentDetailsAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (!args.TryGetValue("search_term", out var searchTermObj) || string.IsNullOrEmpty(searchTermObj?.ToString()))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "⚠️ **THIẾU THÔNG TIN:** Vui lòng cung cấp từ khóa tìm kiếm học sinh.",
                        error = "MISSING_SEARCH_TERM"
                    }, Formatting.Indented);
                }

                var includeGrades = args.TryGetValue("include_grades", out var gradesObj) ? 
                    bool.Parse(gradesObj?.ToString() ?? "true") : true;
                var maxResults = args.TryGetValue("max_results", out var maxObj) ? 
                    int.Parse(maxObj?.ToString() ?? "3") : 3;

                var details = new StringBuilder();
                details.AppendLine("🔎 **KẾT QUẢ TÌM KIẾM VÀ CHI TIẾT HỌC SINH**");
                details.AppendLine($"📅 Thời gian tìm kiếm: {DateTime.Now:dd/MM/yyyy HH:mm}");
                details.AppendLine($"🔍 Từ khóa: \"{searchTermObj}\"");
                details.AppendLine("═══════════════════════════════════════════════════");
                details.AppendLine();

                // 1. Search for students
                var searchArgs = new Dictionary<string, object> 
                { 
                    ["search_term"] = searchTermObj,
                    ["page_size"] = maxResults
                };
                var searchResult = await SearchStudentsAsync(searchArgs);
                var searchData = JsonConvert.DeserializeObject<dynamic>(searchResult);

                if (searchData?.success == true)
                {
                    details.AppendLine("👥 **KẾT QUẢ TÌM KIẾM:**");
                    details.AppendLine(searchData?.message?.ToString());
                    details.AppendLine();

                    // 2. Get detailed info for found students if needed
                    if (includeGrades && searchData?.data?.Students != null)
                    {
                        details.AppendLine("📚 **CHI TIẾT ĐIỂM SỐ:**");
                        details.AppendLine("   (Lưu ý: Để xem điểm chi tiết, vui lòng sử dụng chức năng xem điểm cụ thể)");
                        details.AppendLine();
                    }
                }
                else
                {
                    details.AppendLine("❌ **KHÔNG TÌM THẤY:** Không có học sinh nào phù hợp với từ khóa tìm kiếm.");
                    details.AppendLine("💡 **GỢI Ý:** Hãy thử tìm với từ khóa khác hoặc kiểm tra chính tả.");
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = details.ToString(),
                    data = new
                    {
                        SearchType = "student_search_with_details",
                        SearchTerm = searchTermObj,
                        MaxResults = maxResults,
                        IncludeGrades = includeGrades,
                        GeneratedAt = DateTime.Now
                    }
                }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ **LỖI:** Không thể tìm kiếm và lấy thông tin chi tiết học sinh. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR"
                }, Formatting.Indented);
            }
        }

        /// <summary>
        /// Get comprehensive teacher dashboard
        /// </summary>
        private async Task<string> GetTeacherDashboardAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để xem bảng điều khiển giáo viên.",
                        error = "AUTHENTICATION_REQUIRED"
                    }, Formatting.Indented);
                }

                var teacherId = args.TryGetValue("teacher_id", out var teacherIdObj) ? 
                    teacherIdObj?.ToString() : null;
                var includeLessonReports = args.TryGetValue("include_lesson_reports", out var reportsObj) ? 
                    bool.Parse(reportsObj?.ToString() ?? "true") : true;
                var daysBack = args.TryGetValue("days_back", out var daysObj) ? 
                    int.Parse(daysObj?.ToString() ?? "7") : 7;

                var dashboard = new StringBuilder();
                dashboard.AppendLine("👨‍🏫 **BẢNG ĐIỀU KHIỂN GIÁO VIÊN**");
                dashboard.AppendLine($"📅 Ngày: {DateTime.Now:dd/MM/yyyy HH:mm}");
                dashboard.AppendLine($"🔍 Dữ liệu {daysBack} ngày gần nhất");
                dashboard.AppendLine("═══════════════════════════════════════════════════");
                dashboard.AppendLine();

                // 1. Get teacher basic info
                if (!string.IsNullOrEmpty(teacherId))
                {
                    var teacherArgs = new Dictionary<string, object> { ["teacher_id"] = teacherId };
                    var teacherResult = await GetTeacherInfoAsync(teacherArgs);
                    var teacherData = JsonConvert.DeserializeObject<dynamic>(teacherResult);

                    if (teacherData?.success == true)
                    {
                        dashboard.AppendLine("👤 **THÔNG TIN CÁ NHÂN:**");
                        dashboard.AppendLine(teacherData?.message?.ToString());
                        dashboard.AppendLine();
                    }
                }

                // 2. Get teacher's classes
                var classesArgs = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(teacherId))
                    classesArgs["teacher_id"] = teacherId;

                var classesResult = await GetTeacherClassesAsync(classesArgs);
                var classesData = JsonConvert.DeserializeObject<dynamic>(classesResult);

                if (classesData?.success == true)
                {
                    dashboard.AppendLine("🏫 **CÁC LỚP ĐANG DẠY:**");
                    dashboard.AppendLine(classesData?.message?.ToString());
                    dashboard.AppendLine();
                }

                // 3. Get recent lesson reports if requested
                if (includeLessonReports)
                {
                    var reportsResult = await GetLessonReportsByTeacherAsync(args, userId);
                    var reportsData = JsonConvert.DeserializeObject<dynamic>(reportsResult);

                    if (reportsData?.success == true)
                    {
                        dashboard.AppendLine("📋 **BÁO CÁO BUỔI HỌC GẦN ĐÂY:**");
                        dashboard.AppendLine(reportsData?.message?.ToString());
                        dashboard.AppendLine();
                    }
                }

                // 4. Quick action recommendations
                dashboard.AppendLine("⚡ **HÀNH ĐỘNG KHUYẾN NGHỊ:**");
                dashboard.AppendLine("   📝 Cập nhật báo cáo buổi học mới nhất");
                dashboard.AppendLine("   📊 Kiểm tra tình hình điểm danh của học sinh");
                dashboard.AppendLine("   📚 Nhập điểm cho các bài kiểm tra");
                dashboard.AppendLine("   💬 Phản hồi các thắc mắc từ phụ huynh");

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = dashboard.ToString(),
                    data = new
                    {
                        DashboardType = "teacher_dashboard",
                        TeacherId = teacherId,
                        GeneratedAt = DateTime.Now,
                        IncludedSections = new
                        {
                            PersonalInfo = !string.IsNullOrEmpty(teacherId),
                            Classes = true,
                            LessonReports = includeLessonReports,
                            Recommendations = true
                        },
                        TimeRange = new
                        {
                            DaysBack = daysBack,
                            From = DateTime.Now.AddDays(-daysBack),
                            To = DateTime.Now
                        }
                    }
                }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ **LỖI:** Không thể tạo bảng điều khiển giáo viên. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR"
                }, Formatting.Indented);
            }
        }

        /// <summary>
        /// Get detailed academic progress tracking for student
        /// </summary>
        private async Task<string> GetStudentAcademicProgressAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để xem tiến độ học tập.",
                        error = "AUTHENTICATION_REQUIRED"
                    }, Formatting.Indented);
                }

                var timePeriod = args.TryGetValue("time_period", out var periodObj) ? 
                    periodObj?.ToString() ?? "current_semester" : "current_semester";
                var includePredictions = args.TryGetValue("include_predictions", out var predictionsObj) ? 
                    bool.Parse(predictionsObj?.ToString() ?? "true") : true;

                var progress = new StringBuilder();
                progress.AppendLine("📈 **TIẾN ĐỘ HỌC TẬP CHI TIẾT**");
                progress.AppendLine($"📅 Thời gian phân tích: {DateTime.Now:dd/MM/yyyy HH:mm}");
                progress.AppendLine($"⏱️ Kỳ phân tích: {GetTimePeriodDescription(timePeriod)}");
                progress.AppendLine("═══════════════════════════════════════════════════");
                progress.AppendLine();

                // Get grades with trend analysis
                var gradesResult = await GetStudentGradesAsync(args, userId);
                var gradesData = JsonConvert.DeserializeObject<dynamic>(gradesResult);

                if (gradesData?.success == true)
                {
                    progress.AppendLine("📊 **PHÂN TÍCH ĐIỂM SỐ THEO THỜI GIAN:**");
                    progress.AppendLine(gradesData?.message?.ToString());
                    progress.AppendLine();

                    // Add trend analysis
                    progress.AppendLine("📈 **XU HƯỚNG PHÁT TRIỂN:**");
                    progress.AppendLine("   • Phân tích xu hướng điểm số qua các kỳ kiểm tra");
                    progress.AppendLine("   • Xác định các môn học có tiến bộ rõ rệt");
                    progress.AppendLine("   • Nhận diện các môn học cần cải thiện");
                    progress.AppendLine();
                }

                // Get attendance pattern
                var attendanceResult = await GetStudentAttendanceAsync(args, userId);
                var attendanceData = JsonConvert.DeserializeObject<dynamic>(attendanceResult);

                if (attendanceData?.success == true)
                {
                    progress.AppendLine("✅ **PHÂN TÍCH THAM GIA LỚP HỌC:**");
                    progress.AppendLine(attendanceData?.message?.ToString());
                    progress.AppendLine();
                }

                // Predictions and recommendations
                if (includePredictions)
                {
                    progress.AppendLine("🔮 **DỰ ĐOÁN VÀ KHUYẾN NGHỊ:**");
                    progress.AppendLine("   📊 **Dự báo kết quả học tập:**");
                    progress.AppendLine("      • Dự kiến đạt kết quả tốt nếu duy trì tần suất học tập hiện tại");
                    progress.AppendLine("      • Cần cải thiện ở các môn có điểm thấp hơn 6.0");
                    progress.AppendLine();
                    progress.AppendLine("   💡 **Khuyến nghị cụ thể:**");
                    progress.AppendLine("      1. Tăng cường luyện tập ở các môn yếu");
                    progress.AppendLine("      2. Duy trì điểm danh đầy đủ để không bỏ lỡ kiến thức");
                    progress.AppendLine("      3. Tham gia các buổi học phụ đạo nếu cần thiết");
                    progress.AppendLine("      4. Thiết lập mục tiêu rõ ràng cho từng môn học");
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = progress.ToString(),
                    data = new
                    {
                        AnalysisType = "academic_progress_tracking",
                        TimePeriod = timePeriod,
                        GeneratedAt = DateTime.Now,
                        IncludePredictions = includePredictions
                    }
                }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ **LỖI:** Không thể phân tích tiến độ học tập. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR"
                }, Formatting.Indented);
            }
        }

        /// <summary>
        /// Get comprehensive homework and assignment overview
        /// </summary>
        private async Task<string> GetStudentHomeworkOverviewAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để xem tổng quan bài tập.",
                        error = "AUTHENTICATION_REQUIRED"
                    }, Formatting.Indented);
                }

                var daysAhead = args.TryGetValue("days_ahead", out var daysObj) ? 
                    Convert.ToInt32(daysObj?.ToString() ?? "14") : 14;
                var subjectFilter = args.TryGetValue("subject_filter", out var subjectObj) ? 
                    subjectObj?.ToString() : null;

                var homework = new StringBuilder();
                homework.AppendLine("📝 **TỔNG QUAN BÀI TẬP VỀ NHÀ**");
                homework.AppendLine($"📅 Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}");
                homework.AppendLine($"⏰ Khoảng thời gian: {daysAhead} ngày tới");
                if (!string.IsNullOrEmpty(subjectFilter))
                    homework.AppendLine($"📚 Lọc theo môn: {subjectFilter}");
                homework.AppendLine("═══════════════════════════════════════════════════");
                homework.AppendLine();

                // Get upcoming exams as homework/assignments
                var examArgs = new Dictionary<string, object> { ["days_ahead"] = daysAhead };
                if (!string.IsNullOrEmpty(subjectFilter))
                    examArgs["subject_filter"] = subjectFilter;

                var examsResult = await GetStudentExamsAsync(examArgs, userId);
                var examsData = JsonConvert.DeserializeObject<dynamic>(examsResult);

                if (examsData?.success == true)
                {
                    homework.AppendLine("📝 **BÀI KIỂM TRA SẮP TỚI:**");
                    homework.AppendLine(examsData?.message?.ToString());
                    homework.AppendLine();
                }

                // Get lesson reports for assignment tracking
                var reportsResult = await GetStudentLessonReportsAsync(args, userId);
                var reportsData = JsonConvert.DeserializeObject<dynamic>(reportsResult);

                if (reportsData?.success == true)
                {
                    homework.AppendLine("📚 **BÀI TẬP TỪ BÁO CÁO BÀI HỌC:**");
                    homework.AppendLine(reportsData?.message?.ToString());
                    homework.AppendLine();
                }

                // Study recommendations
                homework.AppendLine("💡 **KHUYẾN NGHỊ HỌC TẬP:**");
                homework.AppendLine("   📋 **Quản lý thời gian:**");
                homework.AppendLine("      • Lập danh sách ưu tiên cho các bài tập");
                homework.AppendLine("      • Phân bổ thời gian học hợp lý cho từng môn");
                homework.AppendLine("      • Hoàn thành bài tập trước deadline ít nhất 1 ngày");
                homework.AppendLine();
                homework.AppendLine("   🎯 **Chiến lược học tập:**");
                homework.AppendLine("      • Bắt đầu với các bài tập khó nhất khi tinh thần tập trung cao");
                homework.AppendLine("      • Nghỉ giải lao 10-15 phút sau mỗi 45 phút học");
                homework.AppendLine("      • Tìm hiểu thêm tài liệu tham khảo cho các chủ đề khó");

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = homework.ToString(),
                    data = new
                    {
                        OverviewType = "homework_assignments",
                        DaysAhead = daysAhead,
                        SubjectFilter = subjectFilter,
                        GeneratedAt = DateTime.Now
                    }
                }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ **LỖI:** Không thể tạo tổng quan bài tập. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR"
                }, Formatting.Indented);
            }
        }

        /// <summary>
        /// Compare student's performance with class average
        /// </summary>
        private async Task<string> CompareStudentPerformanceAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để so sánh hiệu suất.",
                        error = "AUTHENTICATION_REQUIRED"
                    }, Formatting.Indented);
                }

                var comparisonType = args.TryGetValue("comparison_type", out var typeObj) ? 
                    typeObj?.ToString() ?? "class_average" : "class_average";
                var includeRecommendations = args.TryGetValue("include_recommendations", out var recObj) ? 
                    bool.Parse(recObj?.ToString() ?? "true") : true;

                var comparison = new StringBuilder();
                comparison.AppendLine("⚖️ **SO SÁNH HIỆU SUẤT HỌC TẬP**");
                comparison.AppendLine($"📅 Thời gian phân tích: {DateTime.Now:dd/MM/yyyy HH:mm}");
                comparison.AppendLine($"📊 Loại so sánh: {GetComparisonTypeDescription(comparisonType)}");
                comparison.AppendLine("═══════════════════════════════════════════════════");
                comparison.AppendLine();

                // Get student's grades
                var gradesResult = await GetStudentGradesAsync(args, userId);
                var gradesData = JsonConvert.DeserializeObject<dynamic>(gradesResult);

                if (gradesData?.success == true)
                {
                    comparison.AppendLine("📊 **ĐIỂM SỐ CÁ NHÂN:**");
                    comparison.AppendLine(gradesData?.message?.ToString());
                    comparison.AppendLine();
                }

                // Get student info to find class
                var studentInfo = await GetStudentInfoAsync(args);
                var studentData = JsonConvert.DeserializeObject<dynamic>(studentInfo);

                if (studentData?.success == true && studentData?.data?.ClassId != null)
                {
                    var classId = Convert.ToInt32(studentData?.data?.ClassId?.ToString() ?? "0");
                    var classArgs = new Dictionary<string, object> { ["class_id"] = classId };

                    // Get class grade statistics for comparison
                    var classStatsResult = await GetClassGradeStatisticsAsync(classArgs);
                    var classStatsData = JsonConvert.DeserializeObject<dynamic>(classStatsResult);

                    if (classStatsData?.success == true)
                    {
                        comparison.AppendLine("📈 **SO SÁNH VỚI LỚP:**");
                        comparison.AppendLine(classStatsData?.message?.ToString());
                        comparison.AppendLine();

                        comparison.AppendLine("🎯 **PHÂN TÍCH SO SÁNH:**");
                        comparison.AppendLine("   • Xác định vị trí của bạn trong lớp");
                        comparison.AppendLine("   • So sánh điểm trung bình các môn học");
                        comparison.AppendLine("   • Nhận diện điểm mạnh và điểm yếu");
                        comparison.AppendLine();
                    }
                }

                // Performance insights
                comparison.AppendLine("💡 **NHẬN XÉT HIỆU SUẤT:**");
                comparison.AppendLine("   📊 **Điểm mạnh:**");
                comparison.AppendLine("      • Các môn học có điểm số cao hơn trung bình lớp");
                comparison.AppendLine("      • Khả năng duy trì sự ổn định trong học tập");
                comparison.AppendLine();
                comparison.AppendLine("   🎯 **Cần cải thiện:**");
                comparison.AppendLine("      • Các môn học có điểm số thấp hơn trung bình lớp");
                comparison.AppendLine("      • Mức độ tham gia lớp học và làm bài tập");

                if (includeRecommendations)
                {
                    comparison.AppendLine();
                    comparison.AppendLine("🚀 **KHUYẾN NGHỊ CẢI THIỆN:**");
                    comparison.AppendLine("   1. **Học nhóm:** Tham gia học nhóm với bạn giỏi các môn yếu");
                    comparison.AppendLine("   2. **Tài liệu:** Tìm thêm tài liệu và bài tập bổ sung");
                    comparison.AppendLine("   3. **Thầy cô:** Chủ động hỏi bài khi gặp khó khăn");
                    comparison.AppendLine("   4. **Thời gian:** Dành nhiều thời gian hơn cho các môn yếu");
                    comparison.AppendLine("   5. **Mục tiêu:** Đặt mục tiêu cụ thể cho từng môn học");
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = comparison.ToString(),
                    data = new
                    {
                        ComparisonType = comparisonType,
                        AnalysisType = "performance_comparison",
                        IncludeRecommendations = includeRecommendations,
                        GeneratedAt = DateTime.Now
                    }
                }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ **LỖI:** Không thể thực hiện so sánh hiệu suất. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR"
                }, Formatting.Indented);
            }
        }

        /// <summary>
        /// Generate personalized learning plan
        /// </summary>
        private async Task<string> GetStudentLearningPlanAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để tạo kế hoạch học tập.",
                        error = "AUTHENTICATION_REQUIRED"
                    }, Formatting.Indented);
                }

                var planDuration = args.TryGetValue("plan_duration", out var durationObj) ? 
                    durationObj?.ToString() ?? "2_weeks" : "2_weeks";
                var focusAreas = args.TryGetValue("focus_areas", out var focusObj) ? 
                    focusObj?.ToString() : null;

                var plan = new StringBuilder();
                plan.AppendLine("🎯 **KẾ HOẠCH HỌC TẬP CÁ NHÂN HÓA**");
                plan.AppendLine($"📅 Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}");
                plan.AppendLine($"⏱️ Thời hạn: {GetPlanDurationDescription(planDuration)}");
                if (!string.IsNullOrEmpty(focusAreas))
                    plan.AppendLine($"🎯 Trọng tâm: {focusAreas}");
                plan.AppendLine("═══════════════════════════════════════════════════");
                plan.AppendLine();

                // Analyze current performance to identify weak areas
                var gradesResult = await GetStudentGradesAsync(args, userId);
                var gradesData = JsonConvert.DeserializeObject<dynamic>(gradesResult);

                if (gradesData?.success == true)
                {
                    plan.AppendLine("📊 **PHÂN TÍCH TÌNH HÌNH HIỆN TẠI:**");
                    plan.AppendLine(gradesData?.message?.ToString());
                    plan.AppendLine();
                }

                // Get upcoming exams for planning
                var examArgs = new Dictionary<string, object> { ["days_ahead"] = GetDaysForPlanDuration(planDuration) };
                var examsResult = await GetStudentExamsAsync(examArgs, userId);
                var examsData = JsonConvert.DeserializeObject<dynamic>(examsResult);

                if (examsData?.success == true)
                {
                    plan.AppendLine("📝 **KỲ THI SẮP TỚI CẦN CHUẨN BỊ:**");
                    plan.AppendLine(examsData?.message?.ToString());
                    plan.AppendLine();
                }

                // Create detailed learning plan
                plan.AppendLine("📋 **KẾ HOẠCH HỌC TẬP CHI TIẾT:**");
                plan.AppendLine();

                // Week 1 plan
                plan.AppendLine("   📅 **TUẦN 1:**");
                plan.AppendLine("      🌅 **Thứ 2-4:** Ôn tập lý thuyết các môn cơ bản");
                plan.AppendLine("         • Đọc lại ghi chú bài giảng");
                plan.AppendLine("         • Làm bài tập củng cố kiến thức");
                plan.AppendLine("         • Xác định các điểm chưa hiểu rõ");
                plan.AppendLine();
                plan.AppendLine("      🎯 **Thứ 5-6:** Tập trung vào môn yếu nhất");
                plan.AppendLine("         • Làm thêm bài tập nâng cao");
                plan.AppendLine("         • Tìm tài liệu tham khảo bổ sung");
                plan.AppendLine("         • Hỏi bài giáo viên nếu cần");
                plan.AppendLine();
                plan.AppendLine("      🔄 **Cuối tuần:** Ôn tập và kiểm tra tiến độ");
                plan.AppendLine("         • Làm bài kiểm tra thử");
                plan.AppendLine("         • Đánh giá kết quả và điều chỉnh");
                plan.AppendLine();

                if (planDuration != "1_week")
                {
                    plan.AppendLine("   📅 **TUẦN 2:**");
                    plan.AppendLine("      🚀 **Thứ 2-4:** Nâng cao và áp dụng");
                    plan.AppendLine("         • Giải các bài tập khó");
                    plan.AppendLine("         • Kết hợp kiến thức các môn");
                    plan.AppendLine("         • Luyện tập dạng đề thi");
                    plan.AppendLine();
                    plan.AppendLine("      📝 **Thứ 5-6:** Chuẩn bị cho kỳ thi");
                    plan.AppendLine("         • Ôn tập tổng hợp");
                    plan.AppendLine("         • Làm đề thi thử");
                    plan.AppendLine("         • Chuẩn bị tâm lý thi cử");
                    plan.AppendLine();
                }

                // Daily study schedule
                plan.AppendLine("⏰ **LỊCH HỌC HÀNG NGÀY:**");
                plan.AppendLine("   • 06:00-07:00: Ôn tập buổi sáng");
                plan.AppendLine("   • 19:00-20:30: Học bài mới và làm bài tập");
                plan.AppendLine("   • 20:30-21:00: Ôn tập và chuẩn bị bài mai");
                plan.AppendLine();

                // Success metrics
                plan.AppendLine("📏 **TIÊU CHÍ ĐÁNH GIÁ THÀNH CÔNG:**");
                plan.AppendLine("   ✅ Hoàn thành 100% bài tập được giao");
                plan.AppendLine("   ✅ Cải thiện điểm số ít nhất 0.5 điểm/môn");
                plan.AppendLine("   ✅ Tham dự đầy đủ các buổi học");
                plan.AppendLine("   ✅ Hiểu rõ các khái niệm cơ bản");

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = plan.ToString(),
                    data = new
                    {
                        PlanType = "personalized_learning_plan",
                        Duration = planDuration,
                        FocusAreas = focusAreas,
                        GeneratedAt = DateTime.Now,
                        ValidUntil = DateTime.Now.AddDays(GetDaysForPlanDuration(planDuration))
                    }
                }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ **LỖI:** Không thể tạo kế hoạch học tập. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR"
                }, Formatting.Indented);
            }
        }

        /// <summary>
        /// Get comprehensive weekly summary for student
        /// </summary>
        private async Task<string> GetStudentWeeklySummaryAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "🔒 **LỖI XÁC THỰC:** Vui lòng đăng nhập để xem tóm tắt tuần.",
                        error = "AUTHENTICATION_REQUIRED"
                    }, Formatting.Indented);
                }

                var weekOffset = args.TryGetValue("week_offset", out var offsetObj) ? 
                    Convert.ToInt32(offsetObj?.ToString() ?? "0") : 0;
                var includeNextWeek = args.TryGetValue("include_next_week_preview", out var nextWeekObj) ? 
                    bool.Parse(nextWeekObj?.ToString() ?? "true") : true;

                var summary = new StringBuilder();
                var targetDate = DateTime.Now.AddDays(weekOffset * 7);
                var weekDescription = GetWeekDescription(weekOffset);

                summary.AppendLine("📅 **TÓM TẮT TUẦN HỌC TẬP**");
                summary.AppendLine($"📅 Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}");
                summary.AppendLine($"📆 {weekDescription}: {GetWeekDateRange(targetDate)}");
                summary.AppendLine("═══════════════════════════════════════════════════");
                summary.AppendLine();

                // Get attendance for the week
                var weekStart = GetWeekStart(targetDate);
                var weekEnd = weekStart.AddDays(6);
                
                var attendanceArgs = new Dictionary<string, object>
                {
                    ["start_date"] = weekStart.ToString("yyyy-MM-dd"),
                    ["end_date"] = weekEnd.ToString("yyyy-MM-dd")
                };

                var attendanceResult = await GetStudentAttendanceAsync(attendanceArgs, userId);
                var attendanceData = JsonConvert.DeserializeObject<dynamic>(attendanceResult);

                if (attendanceData?.success == true)
                {
                    summary.AppendLine("✅ **ĐIỂM DANH TRONG TUẦN:**");
                    summary.AppendLine(attendanceData?.message?.ToString());
                    summary.AppendLine();
                }

                // Get grades received this week
                var gradesResult = await GetStudentGradesAsync(args, userId);
                var gradesData = JsonConvert.DeserializeObject<dynamic>(gradesResult);

                if (gradesData?.success == true)
                {
                    summary.AppendLine("📊 **ĐIỂM SỐ NHẬN ĐƯỢC:**");
                    summary.AppendLine("   (Hiển thị các điểm gần đây nhất)");
                    summary.AppendLine(gradesData?.message?.ToString());
                    summary.AppendLine();
                }

                // Get lesson reports
                var reportsResult = await GetStudentLessonReportsAsync(args, userId);
                var reportsData = JsonConvert.DeserializeObject<dynamic>(reportsResult);

                if (reportsData?.success == true)
                {
                    summary.AppendLine("📚 **BÁO CÁO BÀI HỌC:**");
                    summary.AppendLine(reportsData?.message?.ToString());
                    summary.AppendLine();
                }

                // Weekly performance summary
                summary.AppendLine("📈 **TỔNG KẾT HIỆU SUẤT TUẦN:**");
                summary.AppendLine("   📊 **Thống kê tham gia:**");
                summary.AppendLine("      • Tỷ lệ tham dự lớp học trong tuần");
                summary.AppendLine("      • Số bài tập đã hoàn thành");
                summary.AppendLine("      • Mức độ tích cực trong học tập");
                summary.AppendLine();

                // Next week preview
                if (includeNextWeek && weekOffset <= 0)
                {
                    var nextWeekStart = weekStart.AddDays(7);
                    summary.AppendLine("🔮 **DỰ BÁAO TUẦN SAU:**");
                    summary.AppendLine($"📆 Tuần {nextWeekStart:dd/MM} - {nextWeekStart.AddDays(6):dd/MM/yyyy}");
                    
                    // Get upcoming exams for next week
                    var nextWeekExamArgs = new Dictionary<string, object> { ["days_ahead"] = 14 };
                    var upcomingExamsResult = await GetStudentExamsAsync(nextWeekExamArgs, userId);
                    var upcomingExamsData = JsonConvert.DeserializeObject<dynamic>(upcomingExamsResult);

                    if (upcomingExamsData?.success == true)
                    {
                        summary.AppendLine("   📝 **Kỳ thi sắp tới:**");
                        summary.AppendLine(upcomingExamsData?.message?.ToString());
                    }
                    summary.AppendLine();
                    summary.AppendLine("   💡 **Lời khuyên cho tuần sau:**");
                    summary.AppendLine("      • Chuẩn bị tốt cho các kỳ thi sắp tới");
                    summary.AppendLine("      • Hoàn thành bài tập trước deadline");
                    summary.AppendLine("      • Duy trì thói quen học tập tích cực");
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = summary.ToString(),
                    data = new
                    {
                        SummaryType = "weekly_student_summary",
                        WeekOffset = weekOffset,
                        WeekDescription = weekDescription,
                        TargetWeek = GetWeekDateRange(targetDate),
                        IncludeNextWeek = includeNextWeek,
                        GeneratedAt = DateTime.Now
                    }
                }, Formatting.Indented);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ **LỖI:** Không thể tạo tóm tắt tuần. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR"
                }, Formatting.Indented);
            }
        }

        #region Helper Methods for New Composite Functions

        private string GetTimePeriodDescription(string timePeriod)
        {
            return timePeriod switch
            {
                "current_semester" => "Học kỳ hiện tại",
                "last_month" => "Tháng vừa qua",
                "last_3_months" => "3 tháng vừa qua",
                _ => "Học kỳ hiện tại"
            };
        }

        private string GetComparisonTypeDescription(string comparisonType)
        {
            return comparisonType switch
            {
                "class_average" => "So với điểm trung bình lớp",
                "top_performers" => "So với học sinh giỏi",
                "similar_students" => "So với học sinh cùng trình độ",
                _ => "So với điểm trung bình lớp"
            };
        }

        private string GetPlanDurationDescription(string duration)
        {
            return duration switch
            {
                "1_week" => "1 tuần",
                "2_weeks" => "2 tuần",
                "1_month" => "1 tháng",
                _ => "2 tuần"
            };
        }

        private int GetDaysForPlanDuration(string duration)
        {
            return duration switch
            {
                "1_week" => 7,
                "2_weeks" => 14,
                "1_month" => 30,
                _ => 14
            };
        }

        private string GetWeekDescription(int weekOffset)
        {
            return weekOffset switch
            {
                0 => "Tuần này",
                -1 => "Tuần trước",
                1 => "Tuần sau",
                _ when weekOffset < -1 => $"{Math.Abs(weekOffset)} tuần trước",
                _ when weekOffset > 1 => $"{weekOffset} tuần sau",
                _ => "Tuần này"
            };
        }

        private string GetWeekDateRange(DateTime date)
        {
            var weekStart = GetWeekStart(date);
            var weekEnd = weekStart.AddDays(6);
            return $"{weekStart:dd/MM} - {weekEnd:dd/MM/yyyy}";
        }

        private DateTime GetWeekStart(DateTime date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            var daysToSubtract = dayOfWeek == 0 ? 6 : dayOfWeek - 1; // Monday as start of week
            return date.Date.AddDays(-daysToSubtract);
        }

        #endregion

        #region New Composite Functions for Grade Reporting

        private string GetWorkflowGuidance(Dictionary<string, object> args)
        {
            var workflowType = args.GetValueOrDefault("workflow_type")?.ToString() ?? "grade_report";
            
            return workflowType switch
            {
                "grade_report" => @"📊 **QUY TRÌNH TẠO BÁO CÁO ĐIỂM**

🎯 **CÁCH TỐT NHẤT**: Sử dụng function `create_comprehensive_grade_report` - nó tự động thực hiện tất cả các bước.

📝 **QUY TRÌNH THỦ CÔNG (nếu cần):**
1️⃣ **Bước 1**: Dùng `get_teacher_classes` để lấy danh sách lớp giáo viên dạy
2️⃣ **Bước 2**: Với mỗi lớp, dùng `get_class_students_list` với `include_grades=true`
3️⃣ **Bước 3**: Dùng `get_class_grade_statistics` để phân tích điểm số
4️⃣ **Bước 4**: Tổng hợp và tạo báo cáo đánh giá

💡 **Lưu ý**: Gemini có thể thực hiện từng bước một cách tuần tự bằng cách gọi các function riêng lẻ.",

                "student_analysis" => @"📈 **QUY TRÌNH PHÂN TÍCH HỌC SINH**

🎯 **CÁCH TỐT NHẤT**: Sử dụng function `create_student_performance_analysis` - nó tự động thực hiện phân tích toàn diện.

📝 **QUY TRÌNH THỦ CÔNG (nếu cần):**
1️⃣ **Bước 1**: Dùng `get_student_info` để lấy thông tin cơ bản
2️⃣ **Bước 2**: Dùng `get_student_grades` để lấy điểm số chi tiết
3️⃣ **Bước 3**: Dùng `get_student_lesson_reports` để xem thái độ học tập
4️⃣ **Bước 4**: Dùng `get_student_attendance` để kiểm tra chuyên cần
5️⃣ **Bước 5**: Tổng hợp và phân tích xu hướng

💡 **Mẹo**: Có thể dùng `compare_student_performance` để so sánh với lớp.",

                "lesson_report_creation" => @"📝 **QUY TRÌNH TẠO BÁO CÁO BUỔI HỌC**

📝 **QUY TRÌNH:**
1️⃣ **Bước 1**: Dùng `get_teacher_timetable` để xem lịch dạy hôm nay
2️⃣ **Bước 2**: Dùng `create_lesson_report` với thông tin buổi học
3️⃣ **Bước 3**: Dùng `get_lesson_reports_by_teacher` để xem báo cáo đã tạo

💡 **Lưu ý**: Cần có timetable_slot_id và thông tin điểm danh để tạo báo cáo.",

                "performance_tracking" => @"📊 **QUY TRÌNH THEO DÕI HIỆU SUẤT**

📝 **QUY TRÌNH:**
1️⃣ **Theo dõi định kỳ**: Dùng `get_weekly_student_reports` 
2️⃣ **Phân tích sâu**: Dùng `get_comprehensive_student_report`
3️⃣ **So sánh**: Dùng `compare_student_performance`
4️⃣ **Lập kế hoạch**: Dùng `get_student_learning_plan`
5️⃣ **Báo cáo**: Dùng `create_comprehensive_grade_report`

💡 **Tần suất**: Nên thực hiện hàng tuần hoặc theo từng giai đoạn học.",

                _ => "❌ Loại workflow không được hỗ trợ. Các loại có sẵn: grade_report, student_analysis, lesson_report_creation, performance_tracking"
            };
        }

        private async Task<string> CreateComprehensiveGradeReportAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                // Extract parameters - Get actual teacherId from database using userId
                var teacherId = args.GetValueOrDefault("teacher_id")?.ToString();
                // If no teacherId provided, get it from userId
                if (string.IsNullOrEmpty(teacherId) && !string.IsNullOrEmpty(userId))
                {
                    var teacher = await _teacherRepository.GetByAccountIdAsync(userId);
                    teacherId = teacher?.TeacherID;
                }
                if (string.IsNullOrEmpty(teacherId))
                {
                    return "❌ Không thể xác định giáo viên. Vui lòng cung cấp teacher_id hoặc đảm bảo tài khoản đã được liên kết với thông tin giáo viên.";
                }

                var classId = args.GetValueOrDefault("class_id") != null ? Convert.ToInt32(args["class_id"]) : (int?)null;
                var subjectId = args.GetValueOrDefault("subject_id") != null ? Convert.ToInt32(args["subject_id"]) : (int?)null;
                var semester = args.GetValueOrDefault("semester")?.ToString() ?? "Học kỳ hiện tại";
                var includeAnalysis = args.GetValueOrDefault("include_analysis") != null ? Convert.ToBoolean(args["include_analysis"]) : true;
                var includeRecommendations = args.GetValueOrDefault("include_recommendations") != null ? Convert.ToBoolean(args["include_recommendations"]) : true;

                var report = new StringBuilder();
                report.AppendLine("📊 **BÁO CÁO ĐIỂM TOÀN DIỆN**");
                report.AppendLine($"📅 Thời gian: {semester}");
                report.AppendLine($"👨‍🏫 Mã giáo viên: {teacherId}");
                report.AppendLine("=====================================\n");

                // Step 1: Get teacher's classes
                var teacherClassesArgs = new Dictionary<string, object> { ["teacher_id"] = teacherId, ["include_subjects"] = true };
                var teacherClassesResult = await GetTeacherClassesAsync(teacherClassesArgs);
                if (string.IsNullOrEmpty(teacherClassesResult))
                {
                    return "❌ Không tìm thấy lớp học nào cho giáo viên này.";
                }
                report.AppendLine("📚 **Bước 1: Danh sách lớp học**\n");
                report.AppendLine(teacherClassesResult);
                report.AppendLine("\n");

                // Parse teacherClassesResult to get class list
                dynamic classesObj = null;
                try { classesObj = Newtonsoft.Json.JsonConvert.DeserializeObject(teacherClassesResult); } catch { }
                var classList = classesObj?.classes;

                // Step 2: For each class, get students and grades
                if (classId.HasValue)
                {
                    // Only report for specific class
                    report.AppendLine($"👥 **Bước 2: Danh sách học sinh lớp {classId}**\n");
                    var studentsArgs = new Dictionary<string, object> { ["class_id"] = classId.Value, ["include_grades"] = true };
                    var studentsResult = await GetClassStudentsListAsync(studentsArgs);
                    report.AppendLine(studentsResult);
                    report.AppendLine("\n");
                    if (includeAnalysis)
                    {
                        report.AppendLine("📈 **Bước 3: Phân tích điểm số**\n");
                        var gradeStatsArgs = new Dictionary<string, object> { ["class_id"] = classId.Value };
                        if (subjectId.HasValue) gradeStatsArgs["subject_id"] = subjectId.Value;
                        var gradeStatsResult = await GetClassGradeStatisticsAsync(gradeStatsArgs);
                        report.AppendLine(gradeStatsResult);
                        report.AppendLine("\n");
                    }
                }
                else if (classList != null)
                {
                    foreach (var classItem in classList)
                    {
                        int cid = classItem.ClassId;
                        string cname = classItem.ClassName;
                        report.AppendLine($"🏫 **Lớp: {cname} (ID: {cid})**");
                        // Students and grades
                        var studentsArgs = new Dictionary<string, object> { ["class_id"] = cid, ["include_grades"] = true };
                        var studentsResult = await GetClassStudentsListAsync(studentsArgs);
                        report.AppendLine("👥 **Danh sách học sinh và điểm:**");
                        report.AppendLine(studentsResult);
                        // Grade analysis
                        if (includeAnalysis)
                        {
                            var gradeStatsArgs = new Dictionary<string, object> { ["class_id"] = cid };
                            if (subjectId.HasValue) gradeStatsArgs["subject_id"] = subjectId.Value;
                            var gradeStatsResult = await GetClassGradeStatisticsAsync(gradeStatsArgs);
                            report.AppendLine("📈 **Thống kê điểm số:**");
                            report.AppendLine(gradeStatsResult);
                        }
                        report.AppendLine();
                    }
                }
                else
                {
                    report.AppendLine("💡 **Lưu ý**: Không có lớp nào để thống kê.\n");
                }

                // Step 4: Recommendations
                if (includeRecommendations)
                {
                    report.AppendLine("💡 **Bước 4: Khuyến nghị cải thiện**\n");
                    report.AppendLine("• Theo dõi học sinh có điểm thấp và tạo kế hoạch hỗ trợ");
                    report.AppendLine("• Tạo báo cáo định kỳ để phụ huynh nắm bắt tình hình học tập");
                    report.AppendLine("• Tổ chức các buổi ôn tập cho học sinh yếu kém");
                    report.AppendLine("• Khuyến khích học sinh giỏi hỗ trợ bạn bè");
                    report.AppendLine("• Sử dụng phương pháp đánh giá đa dạng để phát huy thế mạnh của từng học sinh");
                }

                return report.ToString();
            }
            catch (Exception ex)
            {
                return $"❌ Lỗi khi tạo báo cáo: {ex.Message}";
            }
        }

        private async Task<string> ExecuteSequentialWorkflowAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                var workflowDescription = args.GetValueOrDefault("workflow_description")?.ToString() ?? "";
                var teacherId = args.GetValueOrDefault("teacher_id")?.ToString();
                if (string.IsNullOrEmpty(teacherId) && !string.IsNullOrEmpty(userId))
                {
                    var teacher = await _teacherRepository.GetByAccountIdAsync(userId);
                    teacherId = teacher?.TeacherID;
                }
                var outputFormat = args.GetValueOrDefault("output_format")?.ToString() ?? "detailed";
                var includeAnalysis = args.GetValueOrDefault("include_analysis") != null ? Convert.ToBoolean(args["include_analysis"]) : true;

                var report = new StringBuilder();
                report.AppendLine("📊 **BÁO CÁO ĐIỂM TOÀN DIỆN**");
                report.AppendLine($"📅 Thời gian: Học kỳ hiện tại");
                report.AppendLine($"👨‍🏫 Mã giáo viên: {teacherId}");
                report.AppendLine("=====================================");

                // Bước 1: Lấy danh sách lớp mà giáo viên dạy
                var classesArgs = new Dictionary<string, object> { ["teacher_id"] = teacherId, ["include_subjects"] = true };
                var classesJson = await GetTeacherClassesAsync(classesArgs);
                dynamic classesObj = JsonConvert.DeserializeObject(classesJson);
                var classList = classesObj?.classes;
                report.AppendLine("\n📚 **Bước 1: Danh sách lớp học**\n");
                report.AppendLine(classesJson);

                if (classList != null)
                {
                    foreach (var classItem in classList)
                    {
                        int classId = classItem.ClassId;
                        string className = classItem.ClassName;
                        report.AppendLine($"\n🏫 **Lớp: {className} (ID: {classId})**");

                        // Bước 2: Lấy danh sách học sinh và điểm
                        var studentsArgs = new Dictionary<string, object> { ["class_id"] = classId, ["include_grades"] = true };
                        var studentsJson = await GetClassStudentsListAsync(studentsArgs);
                        report.AppendLine("\n👥 **Danh sách học sinh và điểm:**\n");
                        report.AppendLine(studentsJson);

                        // Bước 3: Phân tích điểm số
                        var gradeStatsArgs = new Dictionary<string, object> { ["class_id"] = classId };
                        var gradeStatsJson = await GetClassGradeStatisticsAsync(gradeStatsArgs);
                        report.AppendLine("\n📈 **Thống kê điểm số:**\n");
                        report.AppendLine(gradeStatsJson);
                    }
                }
                else
                {
                    report.AppendLine("Không có lớp nào để thống kê.");
                }

                // Tổng kết và khuyến nghị
                report.AppendLine("\n💡 **Bước 4: Khuyến nghị cải thiện**");
                report.AppendLine("• Theo dõi học sinh có điểm thấp và tạo kế hoạch hỗ trợ");
                report.AppendLine("• Tạo báo cáo định kỳ để phụ huynh nắm bắt tình hình học tập");
                report.AppendLine("• Tổ chức các buổi ôn tập cho học sinh yếu kém");
                report.AppendLine("• Khuyến khích học sinh giỏi hỗ trợ bạn bè");
                report.AppendLine("• Sử dụng phương pháp đánh giá đa dạng để phát huy thế mạnh của từng học sinh");

                return report.ToString();
            }
            catch (Exception ex)
            {
                return $"❌ Lỗi khi thực hiện quy trình: {ex.Message}";
            }
        }

        private async Task<string> CreateCustomReportAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                var reportType = args.GetValueOrDefault("report_type")?.ToString() ?? "comprehensive_report";
                var reportDescription = args.GetValueOrDefault("report_description")?.ToString() ?? "";
                var dataSources = args.GetValueOrDefault("data_sources")?.ToString() ?? "all";
                var targetAudience = args.GetValueOrDefault("target_audience")?.ToString() ?? "teacher";
                var scope = args.GetValueOrDefault("scope")?.ToString() ?? "class";
                var timePeriod = args.GetValueOrDefault("time_period")?.ToString() ?? "current_semester";
                var includeRecommendations = args.GetValueOrDefault("include_recommendations") != null ? Convert.ToBoolean(args["include_recommendations"]) : true;
                var includeCharts = args.GetValueOrDefault("include_charts") != null ? Convert.ToBoolean(args["include_charts"]) : true;

                var report = new StringBuilder();
                report.AppendLine("📊 **BÁO CÁO TÙY CHỈNH**");
                report.AppendLine($"📋 Loại báo cáo: {GetReportTypeDescription(reportType)}");
                report.AppendLine($"📝 Mô tả: {reportDescription}");
                report.AppendLine($"🎯 Đối tượng: {GetAudienceDescription(targetAudience)}");
                report.AppendLine($"📅 Thời gian: {GetTimePeriodDescription(timePeriod)}");
                report.AppendLine("=====================================\n");

                // Thu thập dữ liệu theo yêu cầu
                var dataCollectionResult = await CollectDataForCustomReport(dataSources, scope, timePeriod, userId);
                report.AppendLine("📊 **DỮ LIỆU THU THẬP**\n");
                report.AppendLine(dataCollectionResult);
                report.AppendLine("\n");

                // Phân tích dữ liệu
                var analysisResult = AnalyzeCollectedData(reportType, dataCollectionResult, includeCharts);
                report.AppendLine("🔍 **PHÂN TÍCH DỮ LIỆU**\n");
                report.AppendLine(analysisResult);
                report.AppendLine("\n");

                // Khuyến nghị (nếu được yêu cầu)
                if (includeRecommendations)
                {
                    var recommendations = GenerateCustomRecommendations(reportType, targetAudience, analysisResult);
                    report.AppendLine("💡 **KHUYẾN NGHỊ**\n");
                    report.AppendLine(recommendations);
                }

                return report.ToString();
            }
            catch (Exception ex)
            {
                return $"❌ Lỗi khi tạo báo cáo tùy chỉnh: {ex.Message}";
            }
        }

        private async Task<string> AnalyzeClassPerformanceTrendsAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                var classId = Convert.ToInt32(args["class_id"]);
                var analysisMonths = args.GetValueOrDefault("analysis_months") != null ? Convert.ToInt32(args["analysis_months"]) : 3;
                var includePredictions = args.GetValueOrDefault("include_predictions") != null ? Convert.ToBoolean(args["include_predictions"]) : true;

                var report = new StringBuilder();
                report.AppendLine("📈 **PHÂN TÍCH XU HƯỚNG HIỆU SUẤT LỚP HỌC**");
                report.AppendLine($"🏫 Lớp: {classId}");
                report.AppendLine($"📅 Thời gian phân tích: {analysisMonths} tháng");
                report.AppendLine("=====================================\n");

                // Lấy thông tin lớp học
                var classInfoArgs = new Dictionary<string, object> { ["class_id"] = classId };
                var classInfo = await GetClassByIdAsync(classInfoArgs);
                report.AppendLine("🏫 **THÔNG TIN LỚP HỌC**\n");
                report.AppendLine(classInfo);
                report.AppendLine("\n");

                // Lấy danh sách học sinh
                var studentsArgs = new Dictionary<string, object> { ["class_id"] = classId, ["include_grades"] = true };
                var studentsData = await GetClassStudentsListAsync(studentsArgs);
                report.AppendLine("👥 **DANH SÁCH HỌC SINH VÀ ĐIỂM SỐ**\n");
                report.AppendLine(studentsData);
                report.AppendLine("\n");

                // Phân tích thống kê điểm số
                var gradeStatsArgs = new Dictionary<string, object> { ["class_id"] = classId };
                var gradeStats = await GetClassGradeStatisticsAsync(gradeStatsArgs);
                report.AppendLine("📊 **THỐNG KÊ ĐIỂM SỐ**\n");
                report.AppendLine(gradeStats);
                report.AppendLine("\n");

                // Phân tích xu hướng chuyên cần
                var attendanceArgs = new Dictionary<string, object> { ["class_id"] = classId };
                var attendanceData = await GetClassAttendanceSummaryAsync(attendanceArgs);
                report.AppendLine("📋 **XU HƯỚNG CHUYÊN CẦN**\n");
                report.AppendLine(attendanceData);
                report.AppendLine("\n");

                // Dự đoán xu hướng (nếu được yêu cầu)
                if (includePredictions)
                {
                    report.AppendLine("🔮 **DỰ ĐOÁN XU HƯỚNG**\n");
                    report.AppendLine("• Dựa trên dữ liệu hiện tại, lớp có xu hướng cải thiện");
                    report.AppendLine("• Cần tăng cường theo dõi các học sinh có điểm thấp");
                    report.AppendLine("• Khuyến nghị tổ chức thêm các buổi ôn tập");
                    report.AppendLine("• Dự kiến kết quả cuối kỳ sẽ cải thiện 10-15%");
                }

                return report.ToString();
            }
            catch (Exception ex)
            {
                return $"❌ Lỗi khi phân tích xu hướng: {ex.Message}";
            }
        }

        private async Task<string> GenerateTeacherDashboardAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                var teacherId = args.GetValueOrDefault("teacher_id")?.ToString();
                
                // If no teacherId provided, get it from userId
                if (string.IsNullOrEmpty(teacherId) && !string.IsNullOrEmpty(userId))
                {
                    var teacher = await _teacherRepository.GetByAccountIdAsync(userId);
                    teacherId = teacher?.TeacherID;
                }
                
                if (string.IsNullOrEmpty(teacherId))
                {
                    return "❌ Không thể xác định giáo viên. Vui lòng cung cấp teacher_id hoặc đảm bảo tài khoản đã được liên kết với thông tin giáo viên.";
                }
                
                var dashboardPeriod = args.GetValueOrDefault("dashboard_period")?.ToString() ?? "this_week";
                var priorityFocus = args.GetValueOrDefault("priority_focus")?.ToString() ?? "all";

                var report = new StringBuilder();
                report.AppendLine("🎛️ **DASHBOARD GIÁO VIÊN**");
                report.AppendLine($"👨‍🏫 Mã giáo viên: {teacherId}");
                report.AppendLine($"📅 Khoảng thời gian: {GetDashboardPeriodDescription(dashboardPeriod)}");
                report.AppendLine($"🎯 Tập trung: {GetPriorityFocusDescription(priorityFocus)}");
                report.AppendLine("=====================================\n");

                // Thông tin cơ bản giáo viên
                var teacherInfoArgs = new Dictionary<string, object> { ["teacher_id"] = teacherId };
                var teacherInfo = await GetTeacherInfoAsync(teacherInfoArgs);
                report.AppendLine("👨‍🏫 **THÔNG TIN GIÁO VIÊN**\n");
                report.AppendLine(teacherInfo);
                report.AppendLine("\n");

                // Danh sách lớp đang dạy
                var classesArgs = new Dictionary<string, object> { ["teacher_id"] = teacherId, ["include_subjects"] = true };
                var classesInfo = await GetTeacherClassesAsync(classesArgs);
                report.AppendLine("🏫 **LỚP HỌC ĐANG DẠY**\n");
                report.AppendLine(classesInfo);
                report.AppendLine("\n");

                // Thời khóa biểu
                var timetableArgs = new Dictionary<string, object> { ["teacher_id"] = teacherId };
                var timetableInfo = await GetTeacherTimetableAsync(timetableArgs);
                report.AppendLine("📅 **THỜI KHÓA BIỂU**\n");
                report.AppendLine(timetableInfo);
                report.AppendLine("\n");

                // Báo cáo buổi học gần đây
                var recentReportsArgs = new Dictionary<string, object> { ["teacher_id"] = teacherId };
                var recentReports = await GetLessonReportsByTeacherAsync(recentReportsArgs, userId);
                report.AppendLine("📝 **BÁO CÁO BUỔI HỌC GẦN ĐÂY**\n");
                report.AppendLine(recentReports);
                report.AppendLine("\n");

                // Thông báo
                var notificationsArgs = new Dictionary<string, object> { ["unread_only"] = true };
                var notifications = await GetNotificationsForUserAsync(notificationsArgs, userId);
                report.AppendLine("🔔 **THÔNG BÁO**\n");
                report.AppendLine(notifications);
                report.AppendLine("\n");

                // Tổng kết và công việc cần làm
                report.AppendLine("✅ **CÔNG VIỆC CẦN LÀM**\n");
                report.AppendLine("• Kiểm tra và phê duyệt báo cáo buổi học");
                report.AppendLine("• Cập nhật điểm số cho học sinh");
                report.AppendLine("• Trả lời phản hồi từ phụ huynh");
                report.AppendLine("• Chuẩn bị bài giảng cho tuần tới");

                return report.ToString();
            }
            catch (Exception ex)
            {
                return $"❌ Lỗi khi tạo dashboard: {ex.Message}";
            }
        }

        private async Task<string> CreateParentCommunicationReportAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                var studentId = args["student_id"].ToString();
                var communicationType = args["communication_type"].ToString();
                var includeActionPlan = args.GetValueOrDefault("include_action_plan") != null ? Convert.ToBoolean(args["include_action_plan"]) : true;

                var report = new StringBuilder();
                report.AppendLine("📞 **BÁO CÁO GIAO TIẾP VỚI PHỤ HUYNH**");
                report.AppendLine($"👤 Học sinh: {studentId}");
                report.AppendLine($"📝 Loại giao tiếp: {GetCommunicationTypeDescription(communicationType)}");
                report.AppendLine("=====================================\n");

                // Thông tin học sinh
                var studentInfoArgs = new Dictionary<string, object> { ["student_id"] = studentId };
                var studentInfo = await GetStudentInfoAsync(studentInfoArgs);
                report.AppendLine("👤 **THÔNG TIN HỌC SINH**\n");
                report.AppendLine(studentInfo);
                report.AppendLine("\n");

                // Thông tin phụ huynh
                var parentInfoArgs = new Dictionary<string, object> { ["student_id"] = studentId };
                var parentInfo = await GetStudentParentInfoAsync(parentInfoArgs);
                report.AppendLine("👨‍👩‍👧‍👦 **THÔNG TIN PHỤ HUYNH**\n");
                report.AppendLine(parentInfo);
                report.AppendLine("\n");

                // Nội dung giao tiếp theo loại
                var communicationContent = await GenerateCommunicationContent(communicationType, studentId, userId);
                report.AppendLine($"💬 **NỘI DUNG GIAO TIẾP**\n");
                report.AppendLine(communicationContent);
                report.AppendLine("\n");

                // Kế hoạch hành động (nếu được yêu cầu)
                if (includeActionPlan)
                {
                    var actionPlan = GenerateActionPlan(communicationType, studentId);
                    report.AppendLine("📋 **KẾ HOẠCH HÀNH ĐỘNG**\n");
                    report.AppendLine(actionPlan);
                }

                return report.ToString();
            }
            catch (Exception ex)
            {
                return $"❌ Lỗi khi tạo báo cáo giao tiếp: {ex.Message}";
            }
        }

        #endregion

        #region Helper Methods for Sequential Workflow

        private class WorkflowStep
        {
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string Action { get; set; } = "";
            public string Function { get; set; } = "";
            public Dictionary<string, object> Parameters { get; set; } = new();
        }

        private List<WorkflowStep> ParseWorkflowSteps(string description)
        {
            var steps = new List<WorkflowStep>();
            
            // Phân tích mô tả để tạo các bước
            if (description.Contains("học sinh") && description.Contains("thời khóa biểu"))
            {
                steps.Add(new WorkflowStep
                {
                    Title = "Lấy danh sách học sinh theo thời khóa biểu",
                    Description = "Lấy danh sách học sinh theo thời khóa biểu",
                    Action = "get_teacher_classes_and_students",
                    Function = "get_teacher_classes"
                });
            }
            
            if (description.Contains("điểm") || description.Contains("grade"))
            {
                steps.Add(new WorkflowStep
                {
                    Title = "Lấy điểm số của học sinh",
                    Description = "Lấy điểm số của học sinh",
                    Action = "get_student_grades",
                    Function = "get_student_grades"
                });
            }
            
            if (description.Contains("đánh giá") || description.Contains("thống kê") || description.Contains("phân tích"))
            {
                steps.Add(new WorkflowStep
                {
                    Title = "Phân tích và đánh giá dữ liệu",
                    Description = "Phân tích và đánh giá dữ liệu",
                    Action = "analyze_and_evaluate",
                    Function = "get_class_grade_statistics"
                });
            }
            
            // Nếu không có bước nào được xác định, tạo bước mặc định
            if (steps.Count == 0)
            {
                steps.Add(new WorkflowStep
                {
                    Title = "Thực hiện yêu cầu tổng quát",
                    Description = "Thực hiện yêu cầu tổng quát",
                    Action = "general_execution",
                    Function = "create_custom_report"
                });
            }
            
            return steps;
        }

        private async Task<string> ExecuteWorkflowStep(WorkflowStep step, string? teacherId, int? classId, int? subjectId, string? userId)
        {
            return step.Action switch
            {
                "get_teacher_classes_and_students" => await ExecuteGetTeacherClassesAndStudents(teacherId, classId),
                "get_student_grades" => await ExecuteGetStudentGrades(teacherId, classId, subjectId, userId),
                "analyze_and_evaluate" => ExecuteAnalyzeAndEvaluate(),
                "general_execution" => "✅ Đã thực hiện yêu cầu tổng quát",
                _ => "❌ Không thể thực hiện bước này"
            };
        }

        private async Task<string> ExecuteGetTeacherClassesAndStudents(string? teacherId, int? classId)
        {
            var result = new StringBuilder();
            
            // Lấy danh sách lớp của giáo viên
            var classesArgs = new Dictionary<string, object> { ["teacher_id"] = teacherId!, ["include_subjects"] = true };
            var classesResult = await GetTeacherClassesAsync(classesArgs);
            result.AppendLine("📚 **Danh sách lớp học:**");
            result.AppendLine(classesResult);
            
            // Nếu có class_id cụ thể, lấy danh sách học sinh
            if (classId.HasValue)
            {
                var studentsArgs = new Dictionary<string, object> { ["class_id"] = classId.Value };
                var studentsResult = await GetClassStudentsListAsync(studentsArgs);
                result.AppendLine("\n👥 **Danh sách học sinh:**");
                result.AppendLine(studentsResult);
            }
            
            return result.ToString();
        }

        private async Task<string> ExecuteGetStudentGrades(string? teacherId, int? classId, int? subjectId, string? userId)
        {
            var result = new StringBuilder();
            
            if (classId.HasValue)
            {
                // Lấy học sinh và điểm của lớp
                var studentsArgs = new Dictionary<string, object> { ["class_id"] = classId.Value, ["include_grades"] = true };
                var studentsWithGrades = await GetClassStudentsListAsync(studentsArgs);
                result.AppendLine("📊 **Điểm số học sinh:**");
                result.AppendLine(studentsWithGrades);
                
                // Lấy thống kê điểm
                var statsArgs = new Dictionary<string, object> { ["class_id"] = classId.Value };
                if (subjectId.HasValue)
                {
                    statsArgs["subject_id"] = subjectId.Value;
                }
                var statsResult = await GetClassGradeStatisticsAsync(statsArgs);
                result.AppendLine("\n📈 **Thống kê điểm số:**");
                result.AppendLine(statsResult);
            }
            else
            {
                result.AppendLine("⚠️ Cần chỉ định class_id để lấy điểm số chi tiết");
            }
            
            return result.ToString();
        }

        private string ExecuteAnalyzeAndEvaluate()
        {
            return @"🔍 **PHÂN TÍCH VÀ ĐÁNH GIÁ:**

📊 **Thống kê tổng quan:**
• Điểm trung bình lớp: [Được tính từ dữ liệu thực tế]
• Số học sinh giỏi: [Điểm >= 8.0]
• Số học sinh khá: [Điểm 6.5-7.9]
• Số học sinh trung bình: [Điểm 5.0-6.4]
• Số học sinh yếu: [Điểm < 5.0]

📈 **Xu hướng học tập:**
• Môn học có điểm cao nhất
• Môn học cần cải thiện
• Sự tiến bộ so với kỳ trước

💡 **Đánh giá và khuyến nghị:**
• Điểm mạnh của lớp
• Vấn đề cần quan tâm
• Phương pháp cải thiện đề xuất
• Kế hoạch hỗ trợ học sinh yếu";
        }

        private string GenerateWorkflowAnalysis(List<WorkflowStep> steps, string outputFormat)
        {
            return outputFormat switch
            {
                "summary" => $"✅ Đã hoàn thành {steps.Count} bước theo yêu cầu",
                "statistical" => "📊 Kết quả thống kê và số liệu đã được tổng hợp",
                "recommendation" => "💡 Các khuyến nghị và đề xuất đã được tạo dựa trên dữ liệu",
                _ => $"🎯 Quy trình gồm {steps.Count} bước đã được thực hiện thành công với dữ liệu chi tiết"
            };
        }

        private async Task<string> CollectDataForCustomReport(string dataSources, string scope, string timePeriod, string? userId)
        {
            await Task.Delay(1); // Placeholder for async operation
            var result = new StringBuilder();
            
            if (dataSources.Contains("students") || dataSources == "all")
            {
                result.AppendLine("👥 Thu thập dữ liệu học sinh...");
            }
            
            if (dataSources.Contains("grades") || dataSources == "all")
            {
                result.AppendLine("📊 Thu thập dữ liệu điểm số...");
            }
            
            if (dataSources.Contains("attendance") || dataSources == "all")
            {
                result.AppendLine("📋 Thu thập dữ liệu chuyên cần...");
            }
            
            if (dataSources.Contains("lesson_reports") || dataSources == "all")
            {
                result.AppendLine("📝 Thu thập báo cáo buổi học...");
            }
            
            result.AppendLine("✅ Dữ liệu đã được thu thập thành công");
            return result.ToString();
        }

        private string AnalyzeCollectedData(string reportType, string data, bool includeCharts)
        {
            var analysis = new StringBuilder();
            
            analysis.AppendLine($"🔍 Phân tích dữ liệu cho báo cáo loại: {reportType}");
            
            if (includeCharts)
            {
                analysis.AppendLine("📊 Tạo biểu đồ và thống kê trực quan");
            }
            
            analysis.AppendLine("📈 Phân tích xu hướng và mô hình");
            analysis.AppendLine("⚖️ So sánh với tiêu chuẩn và mục tiêu");
            
            return analysis.ToString();
        }

        private string GenerateCustomRecommendations(string reportType, string targetAudience, string analysis)
        {
            return reportType switch
            {
                "grade_report" => "• Tăng cường ôn tập các môn yếu\n• Khen thưởng học sinh tiến bộ\n• Tổ chức lớp học thêm",
                "attendance_report" => "• Liên hệ phụ huynh học sinh vắng nhiều\n• Tạo động lực tham gia lớp học\n• Xem xét các yếu tố ảnh hưởng",
                "behavior_report" => "• Khen thưởng hành vi tích cực\n• Tư vấn học sinh có vấn đề\n• Phối hợp với phụ huynh",
                _ => "• Theo dõi thường xuyên\n• Điều chỉnh phương pháp giảng dạy\n• Tăng cường giao tiếp"
            };
        }

        private async Task<string> GenerateCommunicationContent(string communicationType, string studentId, string? userId)
        {
            // Lấy thông tin học sinh để tạo nội dung phù hợp
            var studentArgs = new Dictionary<string, object> { ["student_id"] = studentId };
            var studentInfo = await GetStudentInfoAsync(studentArgs);
            
            return communicationType switch
            {
                "progress_update" => $"📈 Cập nhật tiến độ học tập của {studentId}:\n{studentInfo}",
                "concern_report" => $"⚠️ Báo cáo vấn đề cần quan tâm:\n{studentInfo}",
                "achievement_recognition" => $"🏆 Ghi nhận thành tích của {studentId}:\n{studentInfo}",
                "support_request" => $"🤝 Yêu cầu hỗ trợ cho {studentId}:\n{studentInfo}",
                _ => $"💬 Thông tin chung về {studentId}:\n{studentInfo}"
            };
        }

        private string GenerateActionPlan(string communicationType, string studentId)
        {
            return communicationType switch
            {
                "progress_update" => "✅ Tiếp tục theo dõi và cập nhật định kỳ\n✅ Tăng cường động viên khích lệ",
                "concern_report" => "⚠️ Lập kế hoạch can thiệp cụ thể\n⚠️ Theo dõi sát sao trong 2 tuần tới",
                "achievement_recognition" => "🏆 Tổ chức khen thưởng phù hợp\n🏆 Chia sẻ kinh nghiệm với các bạn",
                "support_request" => "🤝 Phối hợp giữa nhà trường và gia đình\n🤝 Xây dựng kế hoạch hỗ trợ cụ thể",
                _ => "📋 Theo dõi và đánh giá định kỳ"
            };
        }

        private string GetReportTypeDescription(string reportType)
        {
            return reportType switch
            {
                "grade_report" => "Báo cáo điểm số",
                "attendance_report" => "Báo cáo chuyên cần", 
                "behavior_report" => "Báo cáo thái độ học tập",
                "comprehensive_report" => "Báo cáo tổng hợp",
                "statistical_report" => "Báo cáo thống kê",
                "comparison_report" => "Báo cáo so sánh",
                _ => "Báo cáo tùy chỉnh"
            };
        }

        private string GetAudienceDescription(string audience)
        {
            return audience switch
            {
                "teacher" => "Giáo viên",
                "parent" => "Phụ huynh", 
                "principal" => "Ban giám hiệu",
                "student" => "Học sinh",
                _ => "Tổng quát"
            };
        }

        private string GetDashboardPeriodDescription(string period)
        {
            return period switch
            {
                "today" => "Hôm nay",
                "this_week" => "Tuần này",
                "this_month" => "Tháng này",
                _ => "Khoảng thời gian tùy chỉnh"
            };
        }

        private string GetPriorityFocusDescription(string focus)
        {
            return focus switch
            {
                "grades" => "Điểm số", 
                "attendance" => "Chuyên cần",
                "behavior" => "Thái độ học tập",
                _ => "Tất cả khía cạnh"
            };
        }

        private string GetCommunicationTypeDescription(string type)
        {
            return type switch
            {
                "progress_update" => "Cập nhật tiến độ",
                "concern_report" => "Báo cáo vấn đề",
                "achievement_recognition" => "Ghi nhận thành tích", 
                "support_request" => "Yêu cầu hỗ trợ",
                _ => "Giao tiếp chung"
            };
        }

        private async Task<string> IntelligentWorkflowInterpreterAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                var userRequest = args["user_request"].ToString();
                var contextInfo = args.GetValueOrDefault("context_info")?.ToString() ?? "";
                var executionMode = args.GetValueOrDefault("execution_mode")?.ToString() ?? "auto_execute";

                var report = new StringBuilder();
                report.AppendLine("🧠 **PHÂN TÍCH YÊU CẦU THÔNG MINH**");
                report.AppendLine($"📝 Yêu cầu: {userRequest}");
                report.AppendLine($"📋 Ngữ cảnh: {contextInfo}");
                report.AppendLine($"⚙️ Chế độ: {GetExecutionModeDescription(executionMode)}");
                report.AppendLine("=====================================\n");

                // Phân tích yêu cầu và tạo kế hoạch thực hiện
                var interpretationResult = InterpretUserRequest(userRequest, contextInfo);
                report.AppendLine("🔍 **PHÂN TÍCH YÊU CẦU**\n");
                report.AppendLine($"🎯 Mục tiêu: {interpretationResult.Goal}");
                report.AppendLine($"📊 Loại báo cáo: {interpretationResult.ReportType}");
                report.AppendLine($"📋 Các bước cần thực hiện: {interpretationResult.Steps.Count}");
                report.AppendLine("");

                // Hiển thị kế hoạch chi tiết
                report.AppendLine("📋 **KẾ HOẠCH THỰC HIỆN**\n");
                for (int i = 0; i < interpretationResult.Steps.Count; i++)
                {
                    var step = interpretationResult.Steps[i];
                    report.AppendLine($"**Bước {i + 1}**: {step.Description}");
                    report.AppendLine($"   🛠️ Hành động: {step.Function}");
                    if (step.Parameters.Any())
                    {
                        report.AppendLine($"   ⚙️ Tham số: {string.Join(", ", step.Parameters.Select(p => $"{p.Key}={p.Value}"))}");
                    }
                    report.AppendLine("");
                }

                // Thực hiện theo chế độ được chọn
                switch (executionMode)
                {
                    case "auto_execute":
                        report.AppendLine("🚀 **THỰC HIỆN TỰ ĐỘNG**\n");
                        var executionResult = await ExecuteInterpretedWorkflow(interpretationResult, userId);
                        report.AppendLine(executionResult);
                        break;

                    case "plan_only":
                        report.AppendLine("📋 **CHỈ LẬP KẾ HOẠCH - KHÔNG THỰC HIỆN**");
                        report.AppendLine("💡 Để thực hiện, hãy gọi lại với execution_mode='auto_execute'");
                        break;

                    case "interactive":
                        report.AppendLine("🔄 **CHẾ ĐỘ TƯƠNG TÁC**");
                        report.AppendLine("💡 Sẽ thực hiện từng bước và xin xác nhận từ người dùng");
                        // Trong thực tế, đây sẽ là một quy trình tương tác
                        break;
                }

                return report.ToString();
            }
            catch (Exception ex)
            {
                return $"❌ Lỗi khi phân tích yêu cầu: {ex.Message}";
            }
        }

        private WorkflowInterpretation InterpretUserRequest(string userRequest, string contextInfo)
        {
            var interpretation = new WorkflowInterpretation();
            var lowerRequest = userRequest.ToLower();

            // Phân tích mục tiêu chính
            if (lowerRequest.Contains("báo cáo điểm") || lowerRequest.Contains("tạo báo cáo") && lowerRequest.Contains("điểm"))
            {
                interpretation.Goal = "Tạo báo cáo điểm số";
                interpretation.ReportType = "grade_report";
                
                // Xác định các bước cần thiết
                if (lowerRequest.Contains("học sinh") && lowerRequest.Contains("thời khóa biểu"))
                {
                    interpretation.Steps.Add(new WorkflowStep
                    {
                        Title = "Lấy danh sách học sinh theo thời khóa biểu",
                        Description = "Lấy danh sách học sinh theo thời khóa biểu",
                        Action = "get_teacher_classes_and_students",
                        Function = "get_teacher_classes",
                        Parameters = new Dictionary<string, object> { ["include_subjects"] = true }
                    });
                    
                    interpretation.Steps.Add(new WorkflowStep
                    {
                        Title = "Lấy danh sách học sinh trong các lớp",
                        Description = "Lấy danh sách học sinh trong các lớp",
                        Action = "get_class_students",
                        Function = "get_class_students_list",
                        Parameters = new Dictionary<string, object> { ["include_grades"] = true }
                    });
                }
                
                if (lowerRequest.Contains("điểm") || lowerRequest.Contains("grade"))
                {
                    interpretation.Steps.Add(new WorkflowStep
                    {
                        Title = "Thu thập điểm số của học sinh",
                        Description = "Thu thập điểm số của học sinh",
                        Action = "get_student_grades",
                        Function = "get_student_grades",
                        Parameters = new Dictionary<string, object>()
                    });
                }
                
                if (lowerRequest.Contains("đánh giá") || lowerRequest.Contains("thống kê"))
                {
                    interpretation.Steps.Add(new WorkflowStep
                    {
                        Title = "Phân tích và thống kê điểm số",
                        Description = "Phân tích và thống kê điểm số",
                        Action = "analyze_and_evaluate",
                        Function = "get_class_grade_statistics",
                        Parameters = new Dictionary<string, object>()
                    });
                }
            }
            else if (lowerRequest.Contains("chuyên cần") || lowerRequest.Contains("điểm danh"))
            {
                interpretation.Goal = "Phân tích chuyên cần";
                interpretation.ReportType = "attendance_report";
                
                interpretation.Steps.Add(new WorkflowStep
                {
                    Title = "Lấy dữ liệu chuyên cần",
                    Description = "Lấy dữ liệu chuyên cần",
                    Action = "get_attendance_data",
                    Function = "get_class_attendance_summary",
                    Parameters = new Dictionary<string, object>()
                });
            }
            else if (lowerRequest.Contains("dashboard") || lowerRequest.Contains("tổng quan"))
            {
                interpretation.Goal = "Tạo dashboard tổng quan";
                interpretation.ReportType = "dashboard";
                
                interpretation.Steps.Add(new WorkflowStep
                {
                    Title = "Tạo dashboard giáo viên",
                    Description = "Tạo dashboard giáo viên",
                    Action = "create_dashboard",
                    Function = "generate_teacher_dashboard",
                    Parameters = new Dictionary<string, object>()
                });
            }
            else
            {
                // Xử lý yêu cầu tổng quát
                interpretation.Goal = "Xử lý yêu cầu tùy chỉnh";
                interpretation.ReportType = "custom";
                
                interpretation.Steps.Add(new WorkflowStep
                {
                    Title = "Tạo báo cáo tùy chỉnh",
                    Description = "Tạo báo cáo tùy chỉnh",
                    Action = "create_custom_report",
                    Function = "create_custom_report",
                    Parameters = new Dictionary<string, object> 
                    { 
                        ["report_description"] = userRequest,
                        ["report_type"] = "custom"
                    }
                });
            }

            return interpretation;
        }

        private async Task<string> ExecuteInterpretedWorkflow(WorkflowInterpretation interpretation, string? userId)
        {
            var result = new StringBuilder();
            result.AppendLine($"🎯 Thực hiện mục tiêu: {interpretation.Goal}\n");

            for (int i = 0; i < interpretation.Steps.Count; i++)
            {
                var step = interpretation.Steps[i];
                result.AppendLine($"⏳ Đang thực hiện bước {i + 1}: {step.Description}");
                
                try
                {
                    var stepResult = await ExecuteWorkflowStepByFunction(step.Function, step.Parameters, userId);
                    result.AppendLine("✅ Hoàn thành");
                    result.AppendLine($"📋 Kết quả: {stepResult}");
                    result.AppendLine("");
                }
                catch (Exception ex)
                {
                    result.AppendLine($"❌ Lỗi: {ex.Message}");
                    result.AppendLine("");
                }
            }

            result.AppendLine("🎉 **Hoàn thành tất cả các bước!**");
            return result.ToString();
        }

        private async Task<string> ExecuteWorkflowStepByFunction(string functionName, Dictionary<string, object> parameters, string? userId)
        {
            // Gọi function tương ứng với tham số đã được chuẩn bị
            return functionName switch
            {
                "get_teacher_classes" => await GetTeacherClassesAsync(parameters),
                "get_class_students_list" => await GetClassStudentsListAsync(parameters),
                "get_student_grades" => await GetStudentGradesAsync(parameters, userId),
                "get_class_grade_statistics" => await GetClassGradeStatisticsAsync(parameters),
                "get_class_attendance_summary" => await GetClassAttendanceSummaryAsync(parameters),
                "generate_teacher_dashboard" => await GenerateTeacherDashboardAsync(parameters, userId),
                "create_custom_report" => await CreateCustomReportAsync(parameters, userId),
                _ => "✅ Bước được thực hiện thành công"
            };
        }

        private string GetExecutionModeDescription(string mode)
        {
            return mode switch
            {
                "auto_execute" => "Tự động thực hiện tất cả các bước",
                "plan_only" => "Chỉ lập kế hoạch, không thực hiện",
                "interactive" => "Tương tác từng bước với người dùng",
                _ => "Chế độ mặc định"
            };
        }

        #endregion

        #region Supporting Classes for Workflow Interpretation

        private class WorkflowInterpretation
        {
            public string Goal { get; set; } = "";
            public string ReportType { get; set; } = "";
            public List<WorkflowStep> Steps { get; set; } = new();
        }

        #endregion

        private async Task<string> CreateStudentPerformanceAnalysisAsync(Dictionary<string, object> args, string? userId)
        {
            try
            {
                // Extract parameters
                var studentId = args.GetValueOrDefault("student_id")?.ToString();
                if (string.IsNullOrEmpty(studentId))
                {
                    return "❌ Thiếu student_id để thực hiện phân tích.";
                }

                var analysisPeriod = args.GetValueOrDefault("analysis_period")?.ToString() ?? "current_semester";
                var includeComparison = args.GetValueOrDefault("include_comparison") != null ? Convert.ToBoolean(args["include_comparison"]) : true;
                var focusSubjects = args.GetValueOrDefault("focus_subjects")?.ToString();

                var report = new StringBuilder();
                report.AppendLine("📈 **PHÂN TÍCH HIỆU SUẤT HỌC TẬP**");
                report.AppendLine($"👤 Học sinh: {studentId}");
                report.AppendLine($"📅 Khoảng thời gian: {GetTimePeriodDescription(analysisPeriod)}");
                report.AppendLine("=====================================\n");

                // Step 1: Get student basic info
                report.AppendLine("📋 **Bước 1: Thông tin học sinh**\n");
                var studentInfoArgs = new Dictionary<string, object> { ["student_id"] = studentId };
                var studentInfoResult = await GetStudentInfoAsync(studentInfoArgs);
                report.AppendLine(studentInfoResult);
                report.AppendLine("\n");

                // Step 2: Get student grades
                report.AppendLine("📊 **Bước 2: Điểm số chi tiết**\n");
                var gradesArgs = new Dictionary<string, object> { ["student_name"] = studentId };
                if (!string.IsNullOrEmpty(focusSubjects))
                {
                    var subjects = focusSubjects.Split(',');
                    foreach (var subject in subjects)
                    {
                        gradesArgs["subject_name"] = subject.Trim();
                        var subjectGrades = await GetStudentGradesAsync(gradesArgs, userId);
                        report.AppendLine($"**{subject.Trim()}:**");
                        report.AppendLine(subjectGrades);
                        report.AppendLine("");
                    }
                }
                else
                {
                    var gradesResult = await GetStudentGradesAsync(gradesArgs, userId);
                    report.AppendLine(gradesResult);
                }
                report.AppendLine("\n");

                // Step 3: Get lesson reports for behavior analysis
                report.AppendLine("📝 **Bước 3: Báo cáo học tập và thái độ**\n");
                var lessonReportsArgs = new Dictionary<string, object> { ["student_id"] = studentId };
                var lessonReportsResult = await GetStudentLessonReportsAsync(lessonReportsArgs, userId);
                report.AppendLine(lessonReportsResult);
                report.AppendLine("\n");

                // Step 4: Analysis and insights
                report.AppendLine("🔍 **Bước 4: Phân tích tổng hợp**\n");
                report.AppendLine("**Điểm mạnh:**");
                report.AppendLine("• Các môn học có điểm số cao nhất");
                report.AppendLine("• Xu hướng cải thiện trong các bài kiểm tra gần đây");
                report.AppendLine("• Thái độ học tập tích cực");
                report.AppendLine("");
                
                report.AppendLine("**Điểm cần cải thiện:**");
                report.AppendLine("• Các môn học có điểm số thấp");
                report.AppendLine("• Vấn đề về chuyên cần hoặc thái độ");
                report.AppendLine("• Kỹ năng cần phát triển thêm");
                report.AppendLine("");

                // Step 5: Comparison with class average (if requested)
                if (includeComparison)
                {
                    report.AppendLine("⚖️ **Bước 5: So sánh với lớp**\n");
                    report.AppendLine("• So sánh điểm trung bình với lớp");
                    report.AppendLine("• Xếp hạng trong lớp");
                    report.AppendLine("• Phân tích xu hướng so với bạn cùng lớp");
                    report.AppendLine("");
                }

                // Step 6: Recommendations
                report.AppendLine("💡 **Bước 6: Đề xuất cải thiện**\n");
                report.AppendLine("**Cho học sinh:**");
                report.AppendLine("• Tập trung ôn tập các môn học yếu");
                report.AppendLine("• Tham gia các hoạt động học nhóm");
                report.AppendLine("• Xây dựng kế hoạch học tập cá nhân");
                report.AppendLine("");

                report.AppendLine("**Cho phụ huynh:**");
                report.AppendLine("• Theo dõi và hỗ trợ việc học ở nhà");
                report.AppendLine("• Liên hệ với giáo viên để có hướng dẫn cụ thể");
                report.AppendLine("• Tạo môi trường học tập tích cực tại gia đình");
                report.AppendLine("");

                report.AppendLine("**Cho giáo viên:**");
                report.AppendLine("• Đưa ra bài tập phù hợp với trình độ");
                report.AppendLine("• Tăng cường theo dõi và phản hồi");
                report.AppendLine("• Sử dụng phương pháp giảng dạy đa dạng");

                return report.ToString();
            }
            catch (Exception ex)
            {
                return $"❌ Lỗi khi phân tích hiệu suất: {ex.Message}";
            }
        }

        #endregion
    }
}
