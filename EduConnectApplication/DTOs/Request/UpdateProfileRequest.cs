using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class UpdateProfileRequest
    {
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        public string? FirstName { get; set; }
        
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        public string? LastName { get; set; }
        
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }
        
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        public IFormFile? AvatarFile { get; set; }
    }
}
