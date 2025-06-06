using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Request
{
    public class ChatMessageRequest
    {
        public string Role { get; set; }
        public string Message { get; set; }
    }
}
