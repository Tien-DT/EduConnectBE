using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class GoogleTokenLoginRequest
    {
        [Required(ErrorMessage = "Google token is required")]
        public string GoogleToken { get; set; } = string.Empty;
    }
}
