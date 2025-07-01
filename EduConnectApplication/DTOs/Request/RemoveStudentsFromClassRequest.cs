using System.Collections.Generic;

namespace EduConnect.Application.DTOs.Request
{
    public class RemoveStudentsFromClassRequest
    {
        public int ClassID { get; set; }
        public List<string> StudentIDs { get; set; } = new List<string>();
    }
} 