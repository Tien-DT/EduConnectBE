using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class AddStudentsToClassRequest
    {
        [Required(ErrorMessage = "Student IDs list is required")]
        [MinLength(1, ErrorMessage = "At least one student ID is required")]
        public List<string> StudentIds { get; set; } = new List<string>();
    }
}


