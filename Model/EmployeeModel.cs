using System.ComponentModel.DataAnnotations;

namespace Integration_System.Model
{
    public class EmployeeModel
    {
        [Required]
        private int employeeId;
        public int EmployeeId { get { return employeeId; } set { employeeId = value; } }
        ///////////////////////////
        [Required]
        private int applicantId;
        public int ApplicantId { get { return applicantId; } set { applicantId = value; } }
        ///////////////////////////
        [Required]
        private int departmentId;
        public int DepartmentId { get { return departmentId; } set { departmentId = value; } }
        ///////////////////////////
        [Required]
        private DateOnly hireDate;
        public DateOnly HireDate { get { return hireDate; } set { hireDate = value; } }
        ///////////////////////////
        [Required]
        private decimal salary;
        public decimal Salary { get { return salary; } set { salary = value; } }
        ///////////////////////////
        private string status;
        public string Status { get { return status; } set { status = value; } }
        ///////////////////////////
        [Required]
        private int positionId;
        public int PositionId { get { return positionId; } set { positionId = value; } }
        ///////////////////////////
        [Required]
        private string fullName;
        public string FullName { get { return fullName; } set { fullName = value; } }

    }
}
