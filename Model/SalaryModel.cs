using System.ComponentModel.DataAnnotations;

namespace Integration_System.Model
{
    public class SalaryModel
    {
        [Required]
        private int salaryId;
        public int SalaryId { get { return salaryId; } set { salaryId = value; } }
        ///////////////////////////
        private int employeeId;
        public int EmployeeId { get { return employeeId; } set { employeeId = value; } }
        ///////////////////////////
        [Required]
        private DateTime salaryDate;
        public DateTime SalaryDate { get { return salaryDate; } set { salaryDate = value; } }
        ///////////////////////////
        [Required]
        private decimal baseSalary;
        public decimal BaseSalary { get { return baseSalary; } set { baseSalary = value; } }
        ///////////////////////////
        private decimal bonus;
        public decimal Bonus { get { return bonus; } set { bonus = value; } }
        ///////////////////////////
        private decimal deductions;
        public decimal Deductions { get { return deductions; } set { deductions = value; } }
        ///////////////////////////
        [Required]
        private decimal netSalary;
        public decimal NetSalary { get { return netSalary; } set { netSalary = value; } }
        ///////////////////////////
        //private int createrAt;
        //private int CreaterAt { get { return createrAt; } set { createrAt = value; } }
    }
}
