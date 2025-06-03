using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Response
{
    public class GetNewTokenResponse
    {
        public required string AccessToken { get; set; } = null!;
        public required string RefreshToken { get; set; } = null!;
    }
}
