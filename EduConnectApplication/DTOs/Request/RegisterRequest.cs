using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Request
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Please input Email!")]
        public string? Email { get; set; } = null!;

        [Required(ErrorMessage = "Please input First Name!")]
        public string? FirstName { get; set; }= null!;

        [Required(ErrorMessage = "Please input Last Name!")]
        public string? LastName { get; set; } = null!;

        [Required(ErrorMessage = "Please input Password!")]
        public string? Password { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm Password!")]
        public string? ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "Please input Role!")]
        public int? Role { get; set; } = null!;
    }
}
