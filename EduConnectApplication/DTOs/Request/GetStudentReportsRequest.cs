using System;
using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class GetStudentReportsRequest
    {
        /// <summary>
        /// Khoảng thời gian: daily, weekly, monthly
        /// </summary>
        [Required]
        public string Period { get; set; } = "weekly";

        /// <summary>
        /// Từ khóa tìm kiếm (tên học sinh, mô tả phản ánh)
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// ID lớp học
        /// </summary>
        public int? ClassId { get; set; }

        /// <summary>
        /// Loại phản ánh (Violation, Warning, Good, BonusPoint)
        /// </summary>
        public string? NoteType { get; set; }

        /// <summary>
        /// Ngày bắt đầu
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Trang hiện tại (bắt đầu từ 0)
        /// </summary>
        [Range(0, int.MaxValue)]
        public int PageIndex { get; set; } = 0;

        /// <summary>
        /// Số lượng item mỗi trang
        /// </summary>
        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Trường sắp xếp (CreatedAt, StudentName, TotalNotes, TotalPoints)
        /// </summary>
        public string SortBy { get; set; } = "CreatedAt";

        /// <summary>
        /// Thứ tự sắp xếp (true: tăng dần, false: giảm dần)
        /// </summary>
        public bool Ascending { get; set; } = false;
    }
}
