using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class UpdateStudentClassRequest
    {
        /// <summary>
        /// ID của lớp mới. Nếu null thì học sinh sẽ được chuyển về trạng thái chưa phân lớp
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Class ID must be greater than 0")]
        public int? NewClassID { get; set; }
    }
} 