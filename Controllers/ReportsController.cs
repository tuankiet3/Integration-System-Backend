using Integration_System.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Integration_System.Dtos.ReportDto;
using System.Collections.Generic;

namespace Integration_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly EmployeeDAL _employeeDAL;
        private readonly SalaryDAL _salaryDAL;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            EmployeeDAL employeeDAL,
            SalaryDAL salaryDAL,
            ILogger<ReportsController> logger)
        {
            _employeeDAL = employeeDAL;
            _salaryDAL = salaryDAL;
            _logger = logger;
        }

        // GET: api/reports/hr?type={reportType}
        [HttpGet("hr")]
        [ProducesResponseType(typeof(HrReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHrReport([FromQuery] string type)
        {
            _logger.LogInformation("Request received for HR report type: {ReportType}", type);
            if (string.IsNullOrWhiteSpace(type))
            {
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Report type parameter is required.", Status = StatusCodes.Status400BadRequest });
            }

            try
            {
                object reportData = null;
                string reportTypeLower = type.ToLowerInvariant();

                switch (reportTypeLower)
                {
                    case "employee_count":
                        var employees = await _employeeDAL.GetAllEmployeesAsync();
                        reportData = new EmployeeCountDto { TotalEmployees = employees?.Count ?? 0 };
                        _logger.LogInformation("Generated employee_count report.");
                        break;

                    case "distribution_by_dept":
                        reportData = await _employeeDAL.GetEmployeeDistributionByDeptAsync();
                        _logger.LogInformation("Generated distribution_by_dept report.");
                        break;

                    case "status_distribution":
                        reportData = await _employeeDAL.GetEmployeeDistributionByStatusAsync();
                        _logger.LogInformation("Generated status_distribution report.");
                        break;

                    default:
                        _logger.LogWarning("Invalid HR report type requested: {ReportType}", type);
                        return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = $"Invalid HR report type: '{type}'. Valid types are: employee_count, distribution_by_dept, status_distribution.", Status = StatusCodes.Status400BadRequest });
                }

                return Ok(new HrReportDto { ReportType = reportTypeLower, Data = reportData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating HR report type '{ReportType}'", type);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Internal Server Error", Detail = "An error occurred while generating the HR report.", Status = StatusCodes.Status500InternalServerError });
            }
        }

        // GET: api/reports/payroll?type={reportType}
        [HttpGet("payroll")]
        [ProducesResponseType(typeof(PayrollReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPayrollReport([FromQuery] string type, [FromQuery] int? month)
        {
            _logger.LogInformation("Request received for Payroll report type: {ReportType}, Month: {Month}", type,month);
            if (string.IsNullOrWhiteSpace(type))
            {
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Report type parameter is required.", Status = StatusCodes.Status400BadRequest });
            }

            try
            {
                object? reportData = null;
                string reportTypeLower = type.ToLowerInvariant();

                switch (reportTypeLower)
                {
                    case "total_budget":
                        reportData = await _salaryDAL.GetTotalSalaryBudgetAsync( month);
                        _logger.LogInformation("Generated total_budget report.");
                        break;

                    case "avg_salary_by_dept":
                        reportData = await _salaryDAL.GetAverageSalaryByDeptAsync(month);
                        _logger.LogInformation("Generated avg_salary_by_dept report.");
                        break;

                    default:
                        _logger.LogWarning("Invalid Payroll report type requested: {ReportType}", type);
                        return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = $"Invalid Payroll report type: '{type}'. Valid types are: total_budget, avg_salary_by_dept.", Status = StatusCodes.Status400BadRequest });
                }

                return Ok(new PayrollReportDto { ReportType = reportTypeLower, Data = reportData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Payroll report type '{ReportType}' for Month: {Month}", type, month);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Internal Server Error", Detail = "An error occurred while generating the Payroll report.", Status = StatusCodes.Status500InternalServerError });
            }
        }
    }
}