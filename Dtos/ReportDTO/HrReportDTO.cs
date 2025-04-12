using System.Collections.Generic;

namespace Integration_System.Dtos.ReportDto
{
    public class HrReportDto
    {
        public string ReportType { get; set; }
        public object Data { get; set; }
    }

    public class EmployeeCountDto
    {
        public int TotalEmployees { get; set; }
    }

    public class DistributionByDeptDto
    {
        public string DepartmentName { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class StatusDistributionDto
    {
        public string Status { get; set; }
        public int EmployeeCount { get; set; }
    }
}