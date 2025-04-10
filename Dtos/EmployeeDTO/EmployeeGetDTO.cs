using System.ComponentModel.DataAnnotations;

namespace Integration_System.Dtos.EmployeeDTO
{
    public class EmployeeGetDTO
    {
        //[Required]
        //public int EmployeeID { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string? Gender { get; set; } = "null";

        public string? PhoneNumber { get; set; } = "null";

        public string? Email { get; set; } = "null";

        [Required]
        public DateTime HireDate { get; set; }

        public int DepartmentId { get; set; }

        public int PositionId { get; set; }

        public string? Status { get; set; } = "null";

    }
}
