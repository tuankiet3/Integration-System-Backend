using System;
using System.ComponentModel.DataAnnotations;
namespace Integration_System.Dtos.AttendanceDto
{
    // this class is used to update attendance
    public class UpdateAttendanceDto
    {
        [Required(ErrorMessage = "number of working days is required")]
        [Range(0, 31, ErrorMessage = "number of working days must be between 0 and 31")]
        public int NumberOfWorkingDays { get; set; }

        [Required(ErrorMessage = "number of absent days must be between 0 and 31")]
        public int NumberOfAbsentDays { get; set; } = 0; // default value is 0

        [Range(0, 31, ErrorMessage = "number of leave days  must be between 0 and 31")]
        public int NumberOfLeaveDays { get; set; } = 0; // default value is 0
    }
}
