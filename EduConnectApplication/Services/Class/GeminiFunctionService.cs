using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using EduConnect.Data;
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

        public GeminiFunctionService(
            EduConnectDbContext context,
            IStudentService studentService,
            ITeacherService teacherService,
            IClassService classService,
            ITimetableService timetableService)
        {
            _context = context;
            _studentService = studentService;
            _teacherService = teacherService;
            _classService = classService;
            _timetableService = timetableService;
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
                "get_timetable_by_class"
            };
            return supportedFunctions.Contains(functionName);
        }

        public async Task<string> ExecuteFunctionAsync(string functionName, Dictionary<string, object> args)
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
    }
}
