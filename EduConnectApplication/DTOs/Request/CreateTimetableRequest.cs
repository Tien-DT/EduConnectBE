using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EduConnect.Application.DTOs.Request
{
    public class CreateTimetableSlotRequest
    {
        [Required]
        public string TeacherID { get; set; } = string.Empty;

        [Required]
        public int SubjectId { get; set; }

        [Required]
        [StringLength(20)]
        public string DayOfWeek { get; set; } = string.Empty;

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public string? Room { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Period must be between 1 and 20")]
        public int Period { get; set; }
    }

    public class CreateTimetableRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ClassID must be greater than 0")]
        public int ClassID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one timetable slot must be provided")]
        public List<CreateTimetableSlotRequest> Slots { get; set; } = new List<CreateTimetableSlotRequest>();
    }
}