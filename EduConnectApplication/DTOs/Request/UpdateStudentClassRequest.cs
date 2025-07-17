using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class UpdateStudentClassRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Class ID must be greater than 0")]
        public int? NewClassId { get; set; }
    }
} 