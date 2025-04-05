using System.ComponentModel.DataAnnotations;

namespace Integration_System.Model
{
    public class DepartmentModel
    {
        [Required]
        private int departmentId;
        public int DepartmentId { get { return departmentId; } set { departmentId = value; } }
        ///////////////////////////
        [Required]
        private string departmentName;
        public string DepartmentName { get { return departmentName; } set { departmentName = value; } }
        ///////////////////////////
        private DateTime createdAt;
        public DateTime CreatedAt { get { return createdAt; } set { createdAt = value; } }
        ///////////////////////////
        private DateTime updatedAt;
        public DateTime UpdatedAt { get { return updatedAt; } set { updatedAt = value; } }
    }
}
