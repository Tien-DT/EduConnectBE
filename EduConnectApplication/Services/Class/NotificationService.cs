using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Infrastruture.Options;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;

namespace EduConnect.Application.Services.Class
{
    public class NotificationService : INotificationService
    {
        private readonly IFirebaseTokenRepository _firebaseTokenRepository;
        private readonly ILogger<NotificationService> _logger;
        private readonly FirebaseOption _firebaseOptions;
        private readonly IUnitOfWork _unitOfWork;
        private FirebaseApp? _firebaseApp;

        public NotificationService(
            IFirebaseTokenRepository firebaseTokenRepository,
            ILogger<NotificationService> logger,
            IOptions<FirebaseOption> firebaseOptions,
            IUnitOfWork unitOfWork)
        {
            _firebaseTokenRepository = firebaseTokenRepository;
            _logger = logger;
            _firebaseOptions = firebaseOptions.Value;
            _unitOfWork = unitOfWork;
            InitializeFirebaseApp();
        }

        private void InitializeFirebaseApp()
        {
            try
            {
                if (_firebaseApp == null && !string.IsNullOrEmpty(_firebaseOptions.ServiceAccountKeyPath))
                {
                    var basePath = AppDomain.CurrentDomain.BaseDirectory;
                    var serviceAccountPath = Path.IsPathRooted(_firebaseOptions.ServiceAccountKeyPath) 
                        ? _firebaseOptions.ServiceAccountKeyPath 
                        : Path.Combine(basePath, _firebaseOptions.ServiceAccountKeyPath);

                    if (File.Exists(serviceAccountPath))
                    {
                        _firebaseApp = FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile(serviceAccountPath),
                            ProjectId = _firebaseOptions.ProjectId
                        }, "NotificationService");
                        _logger.LogInformation($"Firebase app initialized for NotificationService with service account key from: {serviceAccountPath}");
                    }
                    else
                    {
                        _logger.LogError($"Service account key file not found at: {serviceAccountPath}");
                    }
                }
                else if (_firebaseApp == null && !string.IsNullOrEmpty(_firebaseOptions.ServiceAccountJson))
                {
                    var credential = GoogleCredential.FromJson(_firebaseOptions.ServiceAccountJson);
                    _firebaseApp = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = credential,
                        ProjectId = _firebaseOptions.ProjectId
                    }, "NotificationService");
                    _logger.LogInformation("Firebase app initialized for NotificationService with service account JSON from configuration");
                }
                else
                {
                    _logger.LogWarning("No Firebase service account configuration found for NotificationService");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase app for NotificationService");
            }
        }

        public async Task<ApiResponse<string>> SendTestNotificationAsync(string userId, SendTestNotificationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Title and Body are required"
                    };
                }

                if (!string.IsNullOrWhiteSpace(request.SpecificToken))
                {
                    return await SendNotificationToTokenAsync(request.SpecificToken, request.Title, request.Body, request.Data);
                }

                return await SendNotificationToAllDevicesAsync(userId, request.Title, request.Body, request.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending test notification for user {userId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to send test notification"
                };
            }
        }

        public async Task<ApiResponse<string>> SendNotificationToAllDevicesAsync(string userId, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var firebaseTokens = await _firebaseTokenRepository.GetActiveTokensByUserIdAsync(userId);
                if (!firebaseTokens.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "No active Firebase tokens found for user"
                    };
                }

                var successCount = 0;
                var failedCount = 0;
                var errors = new List<string>();

                foreach (var token in firebaseTokens)
                {
                    var result = await SendNotificationToTokenAsync(token.Token, title, body, data);
                    if (result.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failedCount++;
                        errors.Add($"Token {token.Token[..10]}...: {result.Message}");
                    }
                }

                var message = $"Sent to {successCount}/{firebaseTokens.Count} devices";
                if (failedCount > 0)
                {
                    message += $". {failedCount} failed.";
                }

                return new ApiResponse<string>
                {
                    Success = successCount > 0,
                    Message = message,
                    Data = errors.Any() ? string.Join("; ", errors) : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notifications to all devices for user {userId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to send notifications to devices"
                };
            }
        }

        public async Task<ApiResponse<string>> SendNotificationToTokenAsync(string firebaseToken, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                if (_firebaseApp == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Firebase app is not initialized"
                    };
                }

                var messaging = FirebaseMessaging.GetMessaging(_firebaseApp);

                var message = new Message()
                {
                    Token = firebaseToken,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? new Dictionary<string, string>
                    {
                        ["type"] = "test_notification",
                        ["timestamp"] = DateTime.UtcNow.ToString("o")
                    },
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification()
                        {
                            Icon = "notification_icon",
                            ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                        }
                    },
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            Alert = new ApsAlert()
                            {
                                Title = title,
                                Body = body
                            }
                        }
                    }
                };

                var response = await messaging.SendAsync(message);
                
                _logger.LogInformation($"Firebase notification sent successfully. Message ID: {response}");
                
                return new ApiResponse<string>
                {
                    Success = true,
                    Message = "Notification sent successfully",
                    Data = response
                };
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError(ex, $"Firebase messaging error for token {firebaseToken[..Math.Min(10, firebaseToken.Length)]}...: {ex.MessagingErrorCode}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Firebase messaging error: {ex.MessagingErrorCode}",
                    Data = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to token {firebaseToken[..Math.Min(10, firebaseToken.Length)]}...");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to send notification"
                };
            }
        }

        public async Task<ApiResponse<List<string>>> GetActiveTokensAsync(string userId)
        {
            try
            {
                var tokens = await _firebaseTokenRepository.GetActiveTokensByUserIdAsync(userId);
                var tokenList = tokens.Select(t => t.Token).ToList();
                
                return new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = $"Found {tokenList.Count} active tokens",
                    Data = tokenList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting active tokens for user {userId}");
                return new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "Failed to retrieve active tokens"
                };
            }
        }

        public async Task<ApiResponse<string>> SendClassNotificationAsync(string senderId, SendClassNotificationRequest request)
        {
            try
            {
                if (request.ClassId <= 0)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid class ID"
                    };
                }

                if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Title and Body are required"
                    };
                }

                // Nếu không gửi cho ai cả thì báo lỗi
                if (!request.SendToStudents && !request.SendToParents)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "At least one recipient type (students or parents) must be selected"
                    };
                }

                // Lấy danh sách các student ID từ class ID
                var students = await _unitOfWork.StudentRepository.GetStudentsByClassIdAsync(request.ClassId);
                if (!students.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"No students found in class with ID {request.ClassId}"
                    };
                }

                // Danh sách userID để gửi notification
                var userIds = new HashSet<string>();

                // Thêm student userIds nếu cần
                if (request.SendToStudents)
                {
                    foreach (var student in students)
                    {
                        if (!string.IsNullOrEmpty(student.UserId))
                        {
                            userIds.Add(student.UserId);
                        }
                    }
                }

                // Thêm parent userIds nếu cần
                if (request.SendToParents)
                {
                    foreach (var student in students)
                    {
                        // Lấy thông tin phụ huynh của học sinh
                        var parents = await _unitOfWork.ParentRepository.GetParentsByStudentIdAsync(student.Id);
                        foreach (var parent in parents)
                        {
                            if (!string.IsNullOrEmpty(parent.UserId))
                            {
                                userIds.Add(parent.UserId);
                            }
                        }
                    }
                }

                if (!userIds.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "No recipients found with valid Firebase tokens"
                    };
                }

                // Chuẩn bị dữ liệu thêm cho thông báo
                var data = request.Data ?? new Dictionary<string, string>();
                data["type"] = "class_notification";
                data["class_id"] = request.ClassId.ToString();
                data["sender_id"] = senderId;
                data["timestamp"] = DateTime.UtcNow.ToString("o");

                // Gửi thông báo đến tất cả user
                var successCount = 0;
                var failedCount = 0;
                var errors = new List<string>();

                foreach (var userId in userIds)
                {
                    var result = await SendNotificationToAllDevicesAsync(userId, request.Title, request.Body, data);
                    if (result.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failedCount++;
                        errors.Add($"User {userId}: {result.Message}");
                    }
                }

                var message = $"Notification sent to {successCount}/{userIds.Count} users";
                if (failedCount > 0)
                {
                    message += $". {failedCount} failed.";
                }

                return new ApiResponse<string>
                {
                    Success = successCount > 0,
                    Message = message,
                    Data = errors.Any() ? string.Join("; ", errors) : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending class notification for class {request.ClassId} by sender {senderId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to send class notification"
                };
            }
        }

        public async Task<ApiResponse<string>> SendExamNotificationAsync(string senderId, SendExamNotificationRequest request)
        {
            try
            {
                if (request.ExamId <= 0)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid exam ID"
                    };
                }

                // Nếu không gửi cho ai cả thì báo lỗi
                if (!request.SendToStudents && !request.SendToParents)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "At least one recipient type (students or parents) must be selected"
                    };
                }

                // Lấy thông tin bài thi
                var exam = await _unitOfWork.ExamRepository.GetByIdAsync(request.ExamId);
                if (exam == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Exam with ID {request.ExamId} not found"
                    };
                }

                // Lấy thông tin lớp học
                var classInfo = await _unitOfWork.ClassRepository.GetByIdAsync(exam.ClassId);
                if (classInfo == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Class with ID {exam.ClassId} not found"
                    };
                }

                // Lấy thông tin môn học
                var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(exam.SubjectId);
                
                // Tạo tiêu đề và nội dung thông báo
                string title = request.CustomTitle ?? $"Thông báo: Bài kiểm tra {subject?.Name ?? "môn học"}";
                string body;

                if (!string.IsNullOrEmpty(request.CustomBody))
                {
                    body = request.CustomBody;
                }
                else
                {
                    var examDate = exam.ExamDate.ToString("dd/MM/yyyy");
                    var examTime = exam.StartTime?.ToString("HH:mm") ?? "Chưa xác định";
                    body = $"Lớp {classInfo.Name} có bài kiểm tra {subject?.Name ?? "môn học"} vào ngày {examDate} lúc {examTime}. Vui lòng chuẩn bị kỹ lưỡng.";
                }

                // Lấy danh sách học sinh trong lớp
                var students = await _unitOfWork.StudentRepository.GetStudentsByClassIdAsync(exam.ClassId);
                if (!students.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"No students found in class with ID {exam.ClassId}"
                    };
                }

                // Danh sách userID để gửi notification
                var userIds = new HashSet<string>();

                // Thêm student userIds nếu cần
                if (request.SendToStudents)
                {
                    foreach (var student in students)
                    {
                        if (!string.IsNullOrEmpty(student.UserId))
                        {
                            userIds.Add(student.UserId);
                        }
                    }
                }

                // Thêm parent userIds nếu cần
                if (request.SendToParents)
                {
                    foreach (var student in students)
                    {
                        // Lấy thông tin phụ huynh của học sinh
                        var parents = await _unitOfWork.ParentRepository.GetParentsByStudentIdAsync(student.Id);
                        foreach (var parent in parents)
                        {
                            if (!string.IsNullOrEmpty(parent.UserId))
                            {
                                userIds.Add(parent.UserId);
                            }
                        }
                    }
                }

                if (!userIds.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "No recipients found with valid Firebase tokens"
                    };
                }

                // Chuẩn bị dữ liệu thêm cho thông báo
                var data = request.AdditionalData ?? new Dictionary<string, string>();
                data["type"] = "exam_notification";
                data["exam_id"] = request.ExamId.ToString();
                data["class_id"] = exam.ClassId.ToString();
                data["subject_id"] = exam.SubjectId.ToString();
                data["exam_date"] = exam.ExamDate.ToString("yyyy-MM-dd");
                data["start_time"] = exam.StartTime?.ToString("HH:mm") ?? "";
                data["sender_id"] = senderId;
                data["timestamp"] = DateTime.UtcNow.ToString("o");

                // Gửi thông báo đến tất cả user
                var successCount = 0;
                var failedCount = 0;
                var errors = new List<string>();

                foreach (var userId in userIds)
                {
                    var result = await SendNotificationToAllDevicesAsync(userId, title, body, data);
                    if (result.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failedCount++;
                        errors.Add($"User {userId}: {result.Message}");
                    }
                }

                var message = $"Exam notification sent to {successCount}/{userIds.Count} users";
                if (failedCount > 0)
                {
                    message += $". {failedCount} failed.";
                }

                return new ApiResponse<string>
                {
                    Success = successCount > 0,
                    Message = message,
                    Data = errors.Any() ? string.Join("; ", errors) : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending exam notification for exam {request.ExamId} by sender {senderId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to send exam notification"
                };
            }
        }

        public async Task<ApiResponse<string>> SendScheduledExamNotificationsAsync()
        {
            try
            {
                // Lấy danh sách các bài thi trong vòng 7 ngày tới
                var nextWeekDate = DateTime.Today.AddDays(7);
                var exams = await _unitOfWork.ExamRepository.GetUpcomingExamsAsync(DateTime.Today, nextWeekDate);
                
                if (!exams.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = true,
                        Message = "No upcoming exams found for notification"
                    };
                }

                var sentCount = 0;
                var failedCount = 0;
                var errors = new List<string>();

                // Xử lý từng bài thi
                foreach (var exam in exams)
                {
                    // Tính số ngày từ hôm nay đến ngày thi
                    var daysUntilExam = (exam.ExamDate.Date - DateTime.Today).Days;
                    
                    // Gửi thông báo cho các bài thi đúng theo yêu cầu ngày
                    // Mặc định là 1 ngày trước
                    if (daysUntilExam == 1)
                    {
                        // Tạo request để gửi thông báo
                        var request = new SendExamNotificationRequest
                        {
                            ExamId = exam.Id,
                            SendToParents = true,
                            SendToStudents = true,
                            DaysBeforeExam = 1
                        };

                        // Gửi thông báo với system user ID
                        var result = await SendExamNotificationAsync("system", request);
                        
                        if (result.Success)
                        {
                            sentCount++;
                        }
                        else
                        {
                            failedCount++;
                            errors.Add($"Exam {exam.Id}: {result.Message}");
                        }
                    }
                }

                return new ApiResponse<string>
                {
                    Success = true,
                    Message = $"Processed {sentCount} exam notifications. {failedCount} failed.",
                    Data = errors.Any() ? string.Join("; ", errors) : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending scheduled exam notifications");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to send scheduled exam notifications"
                };
            }
        }
    }
}
