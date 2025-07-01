using System.Collections.Generic;

namespace EduConnect.Application.DTOs.Request
{
    public class AddStudentsToClassRequest
    {
        public int ClassID { get; set; }
        public List<string> StudentIDs { get; set; } = new List<string>();
    }
}


