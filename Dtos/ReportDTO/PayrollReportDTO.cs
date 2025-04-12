using System.Collections.Generic;

namespace Integration_System.Dtos.ReportDto
{
    public class PayrollReportDto
    {
        public string ReportType { get; set; }
        public object Data { get; set; }
    }

    // total salary
    public class TotalBudgetDto
    {
        public decimal TotalNetSalary { get; set; }
        //public int? Year { get; set; }
        public int? Month { get; set; }
    }

    // average salary by department
    public class AvgSalaryByDeptDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public decimal AverageNetSalary { get; set; }
        //public int? Year { get; set; }
        public int? Month { get; set; }
    }
}