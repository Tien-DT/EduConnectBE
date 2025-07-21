using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
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
}
