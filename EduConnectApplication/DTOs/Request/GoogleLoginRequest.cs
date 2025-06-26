using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class GoogleLoginRequest
    {
        [Required(ErrorMessage = "Google ID is required")]
        public string GoogleId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        public string GivenName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string FamilyName { get; set; } = string.Empty;

        public string Picture { get; set; } = string.Empty;

        public bool EmailVerified { get; set; }
    }
}
