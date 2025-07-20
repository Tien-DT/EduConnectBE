using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class SendTestNotificationRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Body { get; set; } = string.Empty;
        
        public Dictionary<string, string>? Data { get; set; }
        
        public string? SpecificToken { get; set; } 
    }

    public class SendNotificationToTokenRequest
    {
        [Required]
        public string FirebaseToken { get; set; } = string.Empty;
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Body { get; set; } = string.Empty;
        
        public Dictionary<string, string>? Data { get; set; }
    }

// q/firebase-message
    public class SendClassNotificationRequest
    {
        [Required]
        public int ClassId { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Body { get; set; } = string.Empty;
        
        public Dictionary<string, string>? Data { get; set; }
        
        public bool SendToParents { get; set; } = true;
        
        public bool SendToStudents { get; set; } = true;
    }

    public class SendExamNotificationRequest
    {
        [Required]
        public int ExamId { get; set; }
        
        public string? CustomTitle { get; set; }
        
        public string? CustomBody { get; set; }
        
        public Dictionary<string, string>? AdditionalData { get; set; }
        
        public bool SendToParents { get; set; } = true;
        
        public bool SendToStudents { get; set; } = true;
        
        [Range(0, 7)]
        public int DaysBeforeExam { get; set; } = 1;
    }
}
