using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Response
{
    public class ClassResponse
    {
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
