using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Request
{
    public class GetNewTokenRequest
    {
        public required string AccountId { get; set; } = null!;
        public required string RefreshToken { get; set; } = null!;
    }
}
