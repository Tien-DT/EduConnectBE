using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Response
{
    public class ChatHistoryEntry
    {
        public required string Role { get; set; }
        public required string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
