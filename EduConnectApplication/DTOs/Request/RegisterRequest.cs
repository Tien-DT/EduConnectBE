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
        public string? Email { get; set; }

        [Required(ErrorMessage = "Please input Full Name!")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Please input Password!")]
        public string? Password { get; set; }
    }
}
