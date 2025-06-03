using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace EduConnect.Application.DTOs.Response
{
    public class TokenResponse
    {
        public required string AccessToken { get; set; } = null!;
        public required string RefreshToken { get; set; } = null!;
    }
}
