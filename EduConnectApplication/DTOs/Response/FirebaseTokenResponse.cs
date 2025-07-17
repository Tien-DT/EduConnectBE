using System;

namespace EduConnect.Application.DTOs.Response
{
    public class FirebaseTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public string? DeviceType { get; set; }
        public string? DeviceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
}
