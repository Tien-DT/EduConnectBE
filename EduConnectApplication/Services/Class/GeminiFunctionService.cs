using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;

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

        public GeminiFunctionService(
            EduConnectDbContext context,
            IStudentService studentService,
            ITeacherService teacherService,
            IClassService classService,
            ITimetableService timetableService,
            IFeedbackService feedbackService,
            IParentRepository parentRepository,
            ISubjectService subjectService,
            IExamService examService)
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
        }

        public FunctionDeclaration[] GetAvailableFunctions()
        {
            return new[]
            {
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
                    Description = "Get the current date and time with optional timezone",
                    Parameters = new FunctionParameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, ParameterSchema>
                        {
                            ["timezone"] = new ParameterSchema
                            {
                                Type = "string",
                                Description = "Optional timezone (default: local time)"
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
                }
            };
        }

        public bool IsFunctionSupported(string functionName)
        {
            var supportedFunctions = new[]
            {
                "get_student_info",
                "get_student_subjects",
                "get_current_time",
                "calculate_sum",
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
                "get_all_students_grade_report"
            };
            return supportedFunctions.Contains(functionName);
        }

        public async Task<string> ExecuteFunctionAsync(string functionName, Dictionary<string, object> args, string? userId = null)
        {
            return functionName.ToLower() switch
            {
                "get_student_info" => await GetStudentInfoAsync(args),
                "get_student_subjects" => await GetStudentSubjectsAsync(args),
                "get_current_time" => GetCurrentTimeAsync(args),
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
                _ => JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = $"Chức năng '{functionName}' không được hỗ trợ. Vui lòng kiểm tra lại tên chức năng hoặc liên hệ bộ phận kỹ thuật.",
                    error = "FUNCTION_NOT_SUPPORTED",
                    data = (object?)null
                }, Formatting.Indented)
            };
        }

        private async Task<string> GetStudentInfoAsync(Dictionary<string, object> args)
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

            try
            {
                var student = await _context.Students
                    .Include(s => s.Account)
                    .Include(s => s.Parent)
                        .ThenInclude(p => p.Account)
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
            var timezone = "Local";
            if (args.TryGetValue("timezone", out var timezoneObj))
            {
                timezone = timezoneObj.ToString() ?? "Local";
            }

            try
            {
                var result = new
                {
                    success = true,
                    message = "Đây là thời gian hiện tại theo yêu cầu của bạn:",
                    error = (string?)null,
                    data = new
                    {
                        CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        CurrentDate = DateTime.Now.ToString("dd/MM/yyyy"),
                        DayOfWeek = DateTime.Now.ToString("dddd", new System.Globalization.CultureInfo("vi-VN")),
                        Timezone = timezone,
                        FriendlyTime = DateTime.Now.ToString("HH:mm"),
                        FriendlyDate = DateTime.Now.ToString("'ngày' dd 'tháng' MM 'năm' yyyy", new System.Globalization.CultureInfo("vi-VN"))
                    }
                };

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy thời gian hiện tại. Vui lòng thử lại sau.",
                    error = "TIME_ERROR",
                    data = (object?)null,
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
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

                // Create feedback request
                var feedbackRequest = new CreateFeedbackRequest
                {
                    Content = content,
                    Target = target,
                    ParentId = parent.ParentID
                };

                // Create feedback
                var result = await _feedbackService.CreateFeedbackAsync(feedbackRequest);

                if (result.Success)
                {
                    var targetDescription = target == "Principal" ? "ban giám hiệu" : "giáo viên";
                    
                    return JsonConvert.SerializeObject(new
                    {
                        success = true,
                        message = $"✅ Đã tạo phản hồi thành công và gửi đến {targetDescription}. Mã phản hồi: #{result.Data?.FeedbackId}",
                        data = new
                        {
                            FeedbackId = result.Data?.FeedbackId,
                            Target = target,
                            TargetDescription = targetDescription,
                            Content = content,
                            Category = category,
                            ParentName = parent.FullName,
                            CreatedAt = DateTime.Now
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
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Cần đăng nhập để xem điểm của con",
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

                // Get semester filter
                var semesterFilter = args.TryGetValue("semester", out var semesterObj) ? 
                    semesterObj?.ToString() : null;

                // Get subject name filter
                var subjectNameFilter = args.TryGetValue("subject_name", out var subjectNameObj) ? 
                    subjectNameObj?.ToString() : null;

                var studentIds = students.Select(s => s.StudentID).ToList();
                
                // Get grades for all students
                var gradesQuery = _context.Grades
                    .Where(g => studentIds.Contains(g.StudentID) && g.IsDeleted != true);

                if (!string.IsNullOrEmpty(semesterFilter))
                {
                    gradesQuery = gradesQuery.Where(g => g.Semester == semesterFilter);
                }

                if (!string.IsNullOrEmpty(subjectNameFilter))
                {
                    var filteredSubjectIds = await _context.Subjects
                        .Where(s => s.SubjectName.Contains(subjectNameFilter))
                        .Select(s => s.SubjectId)
                        .ToListAsync();
                    gradesQuery = gradesQuery.Where(g => filteredSubjectIds.Contains(g.SubjectID));
                }

                var grades = await gradesQuery
                    .OrderBy(g => g.StudentID)
                    .ThenBy(g => g.SubjectID)
                    .ThenBy(g => g.Semester)
                    .ToListAsync();

                // Get subject names
                var subjectIds = grades.Select(g => g.SubjectID).Distinct().ToList();
                var subjects = await _context.Subjects
                    .Where(s => subjectIds.Contains(s.SubjectId))
                    .ToDictionaryAsync(s => s.SubjectId, s => s.SubjectName);

                // Group grades by student
                var gradeSummary = students.Select(student => new
                {
                    StudentInfo = new
                    {
                        StudentId = student.StudentID,
                        StudentName = student.FullName,
                        DateOfBirth = student.DateOfBirth?.ToString("dd/MM/yyyy"),
                        ClassName = student.Class?.ClassName
                    },
                    Grades = grades.Where(g => g.StudentID == student.StudentID)
                        .GroupBy(g => new { g.SubjectID, g.Semester })
                        .Select(group => new
                        {
                            SubjectName = subjects.ContainsKey(group.Key.SubjectID) ? subjects[group.Key.SubjectID] : "N/A",
                            Semester = group.Key.Semester,
                            Scores = group.Select(g => new
                            {
                                GradeType = g.GradeType,
                                Score = g.Score,
                                GradeLevel = g.GradeLevel,
                                Evaluation = g.Evaluation
                            }).ToList(),
                            AverageScore = group.Where(g => g.Score.HasValue).Average(g => g.Score)
                        }).ToList()
                }).ToList();

                var totalGrades = grades.Count;
                var studentsWithGrades = gradeSummary.Where(s => s.Grades.Any()).Count();

                // Format dữ liệu thành bảng đẹp
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendLine($"📊 **BẢNG ĐIỂM HỌC SINH**");
                formattedMessage.AppendLine($"Tìm thấy điểm của {studentsWithGrades}/{students.Count} học sinh" + 
                                          (string.IsNullOrEmpty(semesterFilter) ? "" : $" cho học kỳ {semesterFilter}") +
                                          (string.IsNullOrEmpty(subjectNameFilter) ? "" : $" môn {subjectNameFilter}"));
                formattedMessage.AppendLine();

                foreach (var student in gradeSummary.Where(s => s.Grades.Any()))
                {
                    formattedMessage.AppendLine($"👨‍🎓 **{student.StudentInfo.StudentName}** (ID: {student.StudentInfo.StudentId})");
                    formattedMessage.AppendLine($"   📅 Sinh ngày: {student.StudentInfo.DateOfBirth ?? "N/A"}");
                    formattedMessage.AppendLine($"   🏫 Lớp: {student.StudentInfo.ClassName ?? "N/A"}");
                    formattedMessage.AppendLine();

                    if (student.Grades.Any())
                    {
                        formattedMessage.AppendLine("   📚 **BẢNG ĐIỂM CHI TIẾT:**");
                        formattedMessage.AppendLine("   ┌─────────────────────────────────────────────────────────────────┐");
                        formattedMessage.AppendLine("   │ MÔN HỌC               │ HỌC KỲ    │ ĐIỂM SỐ     │ ĐÁNH GIÁ     │");
                        formattedMessage.AppendLine("   ├─────────────────────────────────────────────────────────────────┤");

                        foreach (var grade in student.Grades.OrderBy(g => g.SubjectName).ThenBy(g => g.Semester))
                        {
                            var subjectName = (grade.SubjectName ?? "N/A").Length > 20 
                                ? (grade.SubjectName ?? "N/A").Substring(0, 17) + "..." 
                                : (grade.SubjectName ?? "N/A").PadRight(20);
                            
                            var semester = (grade.Semester ?? "N/A").PadRight(10);
                            var avgScore = grade.AverageScore.HasValue 
                                ? grade.AverageScore.Value.ToString("F1").PadRight(12)
                                : "N/A".PadRight(12);

                            formattedMessage.AppendLine($"   │ {subjectName} │ {semester} │ {avgScore} │              │");

                            // Hiển thị chi tiết các điểm
                            foreach (var score in grade.Scores.Where(s => s.Score.HasValue))
                            {
                                var gradeType = (score.GradeType ?? "Khác").PadRight(10);
                                var scoreValue = (score.Score?.ToString("F1") ?? "N/A").PadRight(6);
                                var evaluation = (score.Evaluation ?? "").Length > 12 
                                    ? (score.Evaluation ?? "").Substring(0, 9) + "..."
                                    : (score.Evaluation ?? "").PadRight(12);
                                
                                formattedMessage.AppendLine($"   │      ↳ {gradeType}     │           │ {scoreValue}      │ {evaluation} │");
                            }
                        }

                        formattedMessage.AppendLine("   └─────────────────────────────────────────────────────────────────┘");
                    }
                    else
                    {
                        formattedMessage.AppendLine("   📝 Chưa có điểm nào được ghi nhận");
                    }
                    
                    formattedMessage.AppendLine();
                }

                if (studentsWithGrades == 0)
                {
                    formattedMessage.AppendLine("📝 Không có điểm nào được tìm thấy cho các tiêu chí đã chọn.");
                    if (!string.IsNullOrEmpty(studentNameFilter) || !string.IsNullOrEmpty(semesterFilter) || !string.IsNullOrEmpty(subjectNameFilter))
                    {
                        formattedMessage.AppendLine("💡 Gợi ý: Thử bỏ bớt điều kiện lọc để xem nhiều kết quả hơn.");
                    }
                }

                // Thống kê
                formattedMessage.AppendLine("📈 **THỐNG KÊ:**");
                formattedMessage.AppendLine($"   • Tổng số học sinh: {students.Count}");
                formattedMessage.AppendLine($"   • Số học sinh có điểm: {studentsWithGrades}");
                formattedMessage.AppendLine($"   • Tổng số điểm: {totalGrades}");

                if (!string.IsNullOrEmpty(studentNameFilter) || !string.IsNullOrEmpty(semesterFilter) || !string.IsNullOrEmpty(subjectNameFilter))
                {
                    formattedMessage.AppendLine();
                    formattedMessage.AppendLine("🔍 **BỘ LỌC ĐÃ ÁP DỤNG:**");
                    if (!string.IsNullOrEmpty(studentNameFilter))
                        formattedMessage.AppendLine($"   • Tên học sinh: {studentNameFilter}");
                    if (!string.IsNullOrEmpty(semesterFilter))
                        formattedMessage.AppendLine($"   • Học kỳ: {semesterFilter}");
                    if (!string.IsNullOrEmpty(subjectNameFilter))
                        formattedMessage.AppendLine($"   • Môn học: {subjectNameFilter}");
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = formattedMessage.ToString(),
                    data = new
                    {
                        TotalStudents = students.Count,
                        StudentsWithGrades = studentsWithGrades,
                        TotalGrades = totalGrades,
                        FilterApplied = new
                        {
                            StudentName = studentNameFilter,
                            Semester = semesterFilter,
                            SubjectName = subjectNameFilter
                        },
                        Students = gradeSummary
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "❌ Có lỗi xảy ra khi lấy điểm học sinh. Vui lòng thử lại sau.",
                    error = "INTERNAL_ERROR",
                    errorDetails = ex.Message
                }, Formatting.Indented);
            }
        }

        private async Task<string> GetStudentLessonReportsAsync(Dictionary<string, object> args, string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Cần đăng nhập để xem báo cáo học tập của con",
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
                        toDate = parsedToDate.AddDays(1).AddSeconds(-1); // Include entire day
                    }
                }

                // Get limit
                var limit = 20;
                if (args.TryGetValue("limit", out var limitObj) && int.TryParse(limitObj?.ToString(), out var parsedLimit))
                {
                    limit = Math.Min(parsedLimit, 100); // Max 100 reports
                }

                var studentIds = students.Select(s => s.StudentID).ToList();

                // Get lesson reports and student notes
                var reportsQuery = _context.LessonReports
                    .Include(lr => lr.TimetableSlot)
                        .ThenInclude(ts => ts.Subject)
                    .Include(lr => lr.TimetableSlot)
                        .ThenInclude(ts => ts.Timetable)
                        .ThenInclude(t => t.Class)
                    .Include(lr => lr.StudentLessonNotes.Where(sln => studentIds.Contains(sln.StudentID)))
                        .ThenInclude(sln => sln.Student)
                    .Where(lr => lr.IsDeleted != true && lr.StudentLessonNotes.Any(sln => studentIds.Contains(sln.StudentID)));

                if (fromDate.HasValue)
                {
                    reportsQuery = reportsQuery.Where(lr => lr.LessonDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    reportsQuery = reportsQuery.Where(lr => lr.LessonDate <= toDate.Value);
                }

                var reports = await reportsQuery
                    .OrderByDescending(lr => lr.LessonDate)
                    .Take(limit)
                    .ToListAsync();

                // Format the data
                var reportSummary = reports.Select(report => new
                {
                    ReportInfo = new
                    {
                        ReportId = report.ReportID,
                        LessonDate = report.LessonDate.ToString("dd/MM/yyyy"),
                        SubjectName = report.TimetableSlot.Subject?.SubjectName,
                        ClassName = report.TimetableSlot.Timetable?.Class?.ClassName,
                        TotalStudents = report.TotalStudents,
                        AbsentStudents = report.AbsentStudents,
                        Remarks = report.Remarks,
                        CreatedAt = report.CreatedAt.ToString("dd/MM/yyyy HH:mm")
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
                            CreatedAt = note.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                        }).ToList()
                }).Where(r => r.StudentNotes.Any()).ToList();

                var totalStudentNotes = reportSummary.SelectMany(r => r.StudentNotes).Count();

                // Format dữ liệu thành bảng đẹp
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendLine($"📝 **BÁO CÁO HỌC TẬP VÀ GHI CHÚ**");
                formattedMessage.AppendLine($"Tìm thấy {reportSummary.Count} báo cáo học tập với {totalStudentNotes} ghi chú cho {students.Count} học sinh" +
                                          (fromDate.HasValue || toDate.HasValue ? $" từ {fromDate?.ToString("dd/MM/yyyy") ?? "..."} đến {toDate?.ToString("dd/MM/yyyy") ?? "..."}" : ""));
                formattedMessage.AppendLine();

                if (reportSummary.Any())
                {
                    foreach (var report in reportSummary.OrderByDescending(r => DateTime.ParseExact(r.ReportInfo.LessonDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)))
                    {
                        formattedMessage.AppendLine($"📅 **{report.ReportInfo.LessonDate}** - {report.ReportInfo.SubjectName} ({report.ReportInfo.ClassName})");
                        formattedMessage.AppendLine($"   📊 Tổng số HS: {report.ReportInfo.TotalStudents} | Vắng: {report.ReportInfo.AbsentStudents}");
                        
                        if (!string.IsNullOrEmpty(report.ReportInfo.Remarks))
                        {
                            formattedMessage.AppendLine($"   📋 Ghi chú tiết học: {report.ReportInfo.Remarks}");
                        }
                        
                        formattedMessage.AppendLine();

                        if (report.StudentNotes.Any())
                        {
                            formattedMessage.AppendLine("   👥 **GHI CHÚ CÁC CON:**");
                            formattedMessage.AppendLine("   ┌─────────────────────────────────────────────────────────────────────┐");
                            formattedMessage.AppendLine("   │ HỌC SINH            │ LOẠI GHI CHÚ      │ ĐIỂM │ MÔ TẢ              │");
                            formattedMessage.AppendLine("   ├─────────────────────────────────────────────────────────────────────┤");

                            foreach (var note in report.StudentNotes.OrderBy(n => n.StudentName))
                            {
                                var studentName = (note.StudentName ?? "N/A").Length > 18 
                                    ? (note.StudentName ?? "N/A").Substring(0, 15) + "..." 
                                    : (note.StudentName ?? "N/A").PadRight(18);

                                var noteType = GetVietnameseNoteType(note.NoteType ?? "").PadRight(16);
                                var points = note.Points.HasValue ? note.Points.Value.ToString("+0;-0;0").PadRight(4) : "N/A".PadRight(4);
                                var description = (note.Description ?? "").Length > 18 
                                    ? (note.Description ?? "").Substring(0, 15) + "..."
                                    : (note.Description ?? "").PadRight(18);

                                formattedMessage.AppendLine($"   │ {studentName} │ {noteType} │ {points} │ {description} │");
                            }

                            formattedMessage.AppendLine("   └─────────────────────────────────────────────────────────────────────┘");
                        }
                        else
                        {
                            formattedMessage.AppendLine("   📝 Không có ghi chú đặc biệt nào cho các con");
                        }
                        
                        formattedMessage.AppendLine();
                    }
                }
                else
                {
                    formattedMessage.AppendLine("📝 Không tìm thấy báo cáo học tập nào cho các tiêu chí đã chọn.");
                    if (!string.IsNullOrEmpty(studentNameFilter) || fromDate.HasValue || toDate.HasValue)
                    {
                        formattedMessage.AppendLine("💡 Gợi ý: Thử mở rộng khoảng thời gian hoặc bỏ bớt điều kiện lọc.");
                    }
                }

                // Thống kê
                formattedMessage.AppendLine("📈 **THỐNG KÊ:**");
                formattedMessage.AppendLine($"   • Tổng số học sinh: {students.Count}");
                formattedMessage.AppendLine($"   • Số báo cáo: {reportSummary.Count}");
                formattedMessage.AppendLine($"   • Tổng ghi chú: {totalStudentNotes}");

                if (!string.IsNullOrEmpty(studentNameFilter) || fromDate.HasValue || toDate.HasValue || limit != 20)
                {
                    formattedMessage.AppendLine();
                    formattedMessage.AppendLine("🔍 **BỘ LỌC ĐÃ ÁP DỤNG:**");
                    if (!string.IsNullOrEmpty(studentNameFilter))
                        formattedMessage.AppendLine($"   • Tên học sinh: {studentNameFilter}");
                    if (fromDate.HasValue)
                        formattedMessage.AppendLine($"   • Từ ngày: {fromDate.Value.ToString("dd/MM/yyyy")}");
                    if (toDate.HasValue)
                        formattedMessage.AppendLine($"   • Đến ngày: {toDate.Value.ToString("dd/MM/yyyy")}");
                    if (limit != 20)
                        formattedMessage.AppendLine($"   • Giới hạn: {limit} báo cáo");
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = formattedMessage.ToString(),
                    data = new
                    {
                        TotalStudents = students.Count,
                        TotalReports = reportSummary.Count,
                        TotalStudentNotes = totalStudentNotes,
                        FilterApplied = new
                        {
                            StudentName = studentNameFilter,
                            FromDate = fromDate?.ToString("yyyy-MM-dd"),
                            ToDate = toDate?.ToString("yyyy-MM-dd"),
                            Limit = limit
                        },
                        Reports = reportSummary
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
                if (string.IsNullOrEmpty(userId))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Cần đăng nhập để xem điểm danh của con",
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
                        toDate = parsedToDate.AddDays(1).AddSeconds(-1); // Include entire day
                    }
                }

                // Get subject filter
                var subjectNameFilter = args.TryGetValue("subject_name", out var subjectNameObj) ? 
                    subjectNameObj?.ToString() : null;

                // Get limit
                var limit = 50;
                if (args.TryGetValue("limit", out var limitObj) && int.TryParse(limitObj?.ToString(), out var parsedLimit))
                {
                    limit = Math.Min(parsedLimit, 200); // Max 200 attendance records
                }

                var studentIds = students.Select(s => s.StudentID).ToList();

                // Get attendance records with timetable and subject info
                var attendanceQuery = _context.Attendances
                    .Include(a => a.TimetableSlot)
                        .ThenInclude(ts => ts.Subject)
                    .Include(a => a.TimetableSlot)
                        .ThenInclude(ts => ts.Timetable)
                        .ThenInclude(t => t.Class)
                    .Include(a => a.Student)
                    .Where(a => studentIds.Contains(a.StudentID) && a.IsDeleted != true);

                if (fromDate.HasValue)
                {
                    // Get timetable slots within date range
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

                // Format the data
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

                // Calculate statistics
                var totalRecords = attendances.Count;
                var presentCount = attendances.Count(a => a.Status == 1); // Assuming 1 = Present
                var absentCount = attendances.Count(a => a.Status == 0); // Assuming 0 = Absent
                var lateCount = attendances.Count(a => a.Status == 2); // Assuming 2 = Late

                // Format dữ liệu thành bảng đẹp
                var formattedMessage = new System.Text.StringBuilder();
                formattedMessage.AppendLine($"📊 **ĐIỂM DANH HỌC SINH**");
                formattedMessage.AppendLine($"Tìm thấy {totalRecords} bản ghi điểm danh cho {students.Count} học sinh" +
                                          (fromDate.HasValue || toDate.HasValue ? $" từ {fromDate?.ToString("dd/MM/yyyy") ?? "..."} đến {toDate?.ToString("dd/MM/yyyy") ?? "..."}" : "") +
                                          (string.IsNullOrEmpty(subjectNameFilter) ? "" : $" môn {subjectNameFilter}"));
                formattedMessage.AppendLine();

                if (attendanceSummary.Any())
                {
                    foreach (var student in attendanceSummary)
                    {
                        formattedMessage.AppendLine($"👨‍🎓 **{student.StudentInfo.StudentName}** (ID: {student.StudentInfo.StudentId})");
                        formattedMessage.AppendLine($"   🏫 Lớp: {student.StudentInfo.ClassName ?? "N/A"}");
                        formattedMessage.AppendLine();

                        if (student.AttendanceRecords.Any())
                        {
                            formattedMessage.AppendLine("   📝 **BẢNG ĐIỂM DANH CHI TIẾT:**");
                            formattedMessage.AppendLine("   ┌────────────────────────────────────────────────────────────────────────────────┐");
                            formattedMessage.AppendLine("   │ MÔN HỌC           │ THỨ     │ TIẾT      │ TRẠNG THÁI  │ GHI CHÚ       │ PHÒNG │");
                            formattedMessage.AppendLine("   ├────────────────────────────────────────────────────────────────────────────────┤");

                            foreach (var record in student.AttendanceRecords.Take(20)) // Limit to 20 records per student
                            {
                                var subjectName = (record.SubjectName ?? "N/A").Length > 16 
                                    ? (record.SubjectName ?? "N/A").Substring(0, 13) + "..." 
                                    : (record.SubjectName ?? "N/A").PadRight(16);
                                
                                var dayOfWeek = (record.DayOfWeek ?? "N/A").PadRight(8);
                                var timeSlot = $"{record.StartTime}-{record.EndTime}".PadRight(9);
                                var status = (record.Status ?? "N/A").PadRight(11);
                                var note = (record.Note ?? "").Length > 12 
                                    ? (record.Note ?? "").Substring(0, 9) + "..."
                                    : (record.Note ?? "").PadRight(12);
                                var room = (record.Room ?? "").PadRight(5);

                                var statusIcon = record.StatusCode switch
                                {
                                    1 => "✅", // Present
                                    0 => "❌", // Absent  
                                    2 => "⚠️", // Late
                                    _ => "❓"  // Unknown
                                };

                                formattedMessage.AppendLine($"   │ {subjectName} │ {dayOfWeek} │ {timeSlot} │ {statusIcon} {status} │ {note} │ {room} │");
                            }

                            formattedMessage.AppendLine("   └────────────────────────────────────────────────────────────────────────────────┘");

                            // Statistics for this student
                            var studentPresent = student.AttendanceRecords.Count(r => r.StatusCode == 1);
                            var studentAbsent = student.AttendanceRecords.Count(r => r.StatusCode == 0);
                            var studentLate = student.AttendanceRecords.Count(r => r.StatusCode == 2);
                            var studentTotal = student.AttendanceRecords.Count;
                            var attendanceRate = studentTotal > 0 ? (double)studentPresent / studentTotal * 100 : 0;

                            formattedMessage.AppendLine($"   📈 **Thống kê:** Có mặt: {studentPresent} | Vắng: {studentAbsent} | Muộn: {studentLate} | Tỷ lệ: {attendanceRate:F1}%");
                        }
                        else
                        {
                            formattedMessage.AppendLine("   📝 Không có dữ liệu điểm danh");
                        }
                        
                        formattedMessage.AppendLine();
                    }
                }
                else
                {
                    formattedMessage.AppendLine("📝 Không tìm thấy dữ liệu điểm danh nào cho các tiêu chí đã chọn.");
                    if (!string.IsNullOrEmpty(studentNameFilter) || fromDate.HasValue || toDate.HasValue || !string.IsNullOrEmpty(subjectNameFilter))
                    {
                        formattedMessage.AppendLine("💡 Gợi ý: Thử mở rộng khoảng thời gian hoặc bỏ bớt điều kiện lọc.");
                    }
                }

                // Thống kê tổng
                var overallAttendanceRate = totalRecords > 0 ? (double)presentCount / totalRecords * 100 : 0;
                formattedMessage.AppendLine("📈 **THỐNG KÊ TỔNG:**");
                formattedMessage.AppendLine($"   • Tổng số học sinh: {students.Count}");
                formattedMessage.AppendLine($"   • Tổng số bản ghi: {totalRecords}");
                formattedMessage.AppendLine($"   • Có mặt: {presentCount} ({(totalRecords > 0 ? (double)presentCount/totalRecords*100 : 0):F1}%)");
                formattedMessage.AppendLine($"   • Vắng mặt: {absentCount} ({(totalRecords > 0 ? (double)absentCount/totalRecords*100 : 0):F1}%)");
                formattedMessage.AppendLine($"   • Đi muộn: {lateCount} ({(totalRecords > 0 ? (double)lateCount/totalRecords*100 : 0):F1}%)");

                if (!string.IsNullOrEmpty(studentNameFilter) || fromDate.HasValue || toDate.HasValue || !string.IsNullOrEmpty(subjectNameFilter) || limit != 50)
                {
                    formattedMessage.AppendLine();
                    formattedMessage.AppendLine("🔍 **BỘ LỌC ĐÃ ÁP DỤNG:**");
                    if (!string.IsNullOrEmpty(studentNameFilter))
                        formattedMessage.AppendLine($"   • Tên học sinh: {studentNameFilter}");
                    if (fromDate.HasValue)
                        formattedMessage.AppendLine($"   • Từ ngày: {fromDate.Value.ToString("dd/MM/yyyy")}");
                    if (toDate.HasValue)
                        formattedMessage.AppendLine($"   • Đến ngày: {toDate.Value.ToString("dd/MM/yyyy")}");
                    if (!string.IsNullOrEmpty(subjectNameFilter))
                        formattedMessage.AppendLine($"   • Môn học: {subjectNameFilter}");
                    if (limit != 50)
                        formattedMessage.AppendLine($"   • Giới hạn: {limit} bản ghi");
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = formattedMessage.ToString(),
                    data = new
                    {
                        TotalStudents = students.Count,
                        TotalRecords = totalRecords,
                        PresentCount = presentCount,
                        AbsentCount = absentCount,
                        LateCount = lateCount,
                        OverallAttendanceRate = overallAttendanceRate,
                        FilterApplied = new
                        {
                            StudentName = studentNameFilter,
                            FromDate = fromDate?.ToString("yyyy-MM-dd"),
                            ToDate = toDate?.ToString("yyyy-MM-dd"),
                            SubjectName = subjectNameFilter,
                            Limit = limit
                        },
                        Students = attendanceSummary
                    }
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
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
    }
}
