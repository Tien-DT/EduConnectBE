using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Request
{
    public class ChatWithHistoryRequest
    {
        public required string UserId { get; set; }
        public required string Message { get; set; }
    }
}
