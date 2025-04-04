using System.ComponentModel.DataAnnotations;

namespace Integration_System.Model
{
    public class PayrollModel
    {
        [Required]
        private int payrollId;
        public int PayrollId { get { return payrollId; } set { payrollId = value; } }
        ///////////////////////////
        [Required]
        private int employeeId;
        public int EmployeeId { get { return employeeId; } set { employeeId = value; } }
        ///////////////////////////
        [Required]
        private DateTime payrollDate;
        public DateTime PayrollDate { get { return payrollDate; } set { payrollDate = value; } }
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
        private decimal netSalary;
        public decimal NetSalary { get { return netSalary; } set { netSalary = value; } }

    }
}
