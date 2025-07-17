using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class UpdateFirebaseTokenRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        
        public string? DeviceType { get; set; }
        
        public string? DeviceId { get; set; }
    }

    public class GetFirebaseTokenRequest
    {
        [Required]
        public string DeviceType { get; set; } = string.Empty; 
        
        [Required]
        public string FCMToken { get; set; } = string.Empty; 
        
        public string? DeviceId { get; set; }
    }
}
