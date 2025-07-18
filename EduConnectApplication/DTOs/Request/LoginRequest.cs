using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Request
{
    public class LoginRequest
    {
            [Required]
            [EmailAddress(ErrorMessage = "Please input email!")]
            public string? Email { get; set; } = null!;

            [Required(ErrorMessage = "Please input password!")]
            public string? Password { get; set; } = null!;
    }
}
