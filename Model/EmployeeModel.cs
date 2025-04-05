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
        private string fullName;
        public string FullName { get { return fullName; } set { fullName = value; } }
        ///////////////////////////
        [Required]
        private DateTime dateofBirth;
        public DateTime DateofBirth { get { return dateofBirth; } set { dateofBirth = value; } }
        ///////////////////////////
        private bool gender;
        public bool Gender { get { return gender; } set { gender = value; } }
        ///////////////////////////
        private string phoneNumber;
        public string PhoneNumber { get { return phoneNumber; } set { phoneNumber = value; } }
        ///////////////////////////
        private string email;
        public string Email { get { return email; } set { email = value; } }
        ///////////////////////////
        [Required]
        private DateTime hireDate;
        public DateTime HireDate { get { return hireDate; } set { hireDate = value; } }
        ///////////////////////////
        private int departmentId;
        public int DepartmentId { get { return departmentId; } set { departmentId = value; } }
        ///////////////////////////
        private int positionId;
        public int PositionId { get { return positionId; } set { positionId = value; } }
        ///////////////////////////
        private string status;
        public string Status { get { return status; } set { status = value; } }
        ///////////////////////////
        private DateTime createdAt;
        public DateTime CreatedAt { get { return createdAt; } set { createdAt = value; } }
        ///////////////////////////
        private DateTime updatedAt;
        public DateTime UpdatedAt { get { return updatedAt; } set { updatedAt = value; } }


    }
}
