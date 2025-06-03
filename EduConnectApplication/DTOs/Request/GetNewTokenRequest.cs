using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Request
{
    public class GetNewTokenRequest
    {
        public required Guid AccountId { get; set; } 
        public required string RefreshToken { get; set; } = null!;
    }
}
