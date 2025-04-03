namespace Integration_System.Model
{
    public class PayrollModel
    {
        private int payrollId;
        public int PayrollId { get { return payrollId; } set { payrollId = value; } }
        ///////////////////////////
        private int employeeId;
        public int EmployeeId { get { return employeeId; } set { employeeId = value; } }
        ///////////////////////////
        private DateOnly payrollDate;
        public DateOnly PayrollDate { get { return payrollDate; } set { payrollDate = value; } }
        ///////////////////////////
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
