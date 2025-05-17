using Microsoft.AspNetCore.Mvc;
using Integration_System.DAL;
using Integration_System.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;
using Integration_System.Services;
using Microsoft.AspNetCore.Authorization; // <<< Thêm using
using Integration_System.Constants;       // <<< Thêm using Roles

namespace Integration_System.Controllers
{
    [Route("api/email")]
    [ApiController]
    public class EmailNotificationController : ControllerBase
    {
        private readonly IGmailService _gmailService;
        private readonly EmployeeDAL _employeeDAL;
        private readonly SalaryDAL _salaryDAL;
        private readonly ILogger<EmailNotificationController> _logger;

        public EmailNotificationController(
            IGmailService gmailService,
            EmployeeDAL employeeDAL,
            SalaryDAL salaryDAL,
            ILogger<EmailNotificationController> logger)
        {
            _gmailService = gmailService;
            _employeeDAL = employeeDAL;
            _salaryDAL = salaryDAL;
            _logger = logger;
        }

        [HttpPost("send-salary-to-employee/{employeeId}")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.PayrollManagement}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendSalaryToEmployee(int employeeId)
        {
            _logger.LogInformation("User {User} initiating salary email to employee with ID {EmployeeId}.", User.Identity?.Name, employeeId);

            if (employeeId <= 0)
            {
                _logger.LogWarning("Invalid employee ID {EmployeeId} provided by user {User}.", employeeId, User.Identity?.Name);
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Invalid employee ID." });
            }

            try
            {
                var employee = await _employeeDAL.GetEmployeeIdAsync(employeeId);

                if (employee == null)
                {
                    _logger.LogWarning("Employee with ID {EmployeeId} not found when requested by user {User}.", employeeId, User.Identity?.Name);
                    return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Employee with ID {employeeId} not found." });
                }

                if (string.IsNullOrEmpty(employee.Email) || !employee.Email.Contains('@'))
                {
                    _logger.LogWarning("Employee {EmployeeId} ({FullName}) has an invalid or missing email address. Skipping email send.", employeeId, employee.FullName);
                    return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = $"Employee {employee.FullName} (ID: {employeeId}) has an invalid or missing email address." });
                }

                if (employee.Status != null && employee.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation($"Employee {employeeId} ({employee.FullName}) is inactive. Skipping salary email.");
                    return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = $"Employee {employee.FullName} (ID: {employeeId}) is inactive. Cannot send salary email." });
                }

                var salary = await _salaryDAL.getLatestEmployeeID(employeeId);

                if (salary == null)
                {
                    _logger.LogWarning($"Latest salary data for employee {employeeId} not found when requested by user {User.Identity?.Name}.");
                    return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Salary data not found for employee with ID {employeeId}." });
                }

                try
                {
                    string subject = $"Thông báo lương tháng {salary.SalaryMonth:MM/yyyy} - {employee.FullName}";
                    string baseSalaryFormatted = string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:N0} VND", salary.BaseSalary);
                    string bonusFormatted = string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:N0} VND", salary.Bonus);
                    string deductionsFormatted = string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:N0} VND", salary.Deductions);
                    string netSalaryFormatted = string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:N0} VND", salary.NetSalary);

                    string body = $@"
                        <html>
                        <body style='font-family: sans-serif;'>
                            <h2>Xin chào {employee.FullName},</h2>
                            <p>Công ty xin gửi bạn thông tin lương tháng {salary.SalaryMonth:MM/yyyy} như sau:</p>
                            <table border='1' cellpadding='5' style='border-collapse: collapse;'>
                                <tr><td><strong>Lương cơ bản</strong></td><td style='text-align: right;'>{baseSalaryFormatted}</td></tr>
                                <tr><td><strong>Thưởng</strong></td><td style='text-align: right;'>{bonusFormatted}</td></tr>
                                <tr><td><strong>Khấu trừ</strong></td><td style='text-align: right;'>{deductionsFormatted}</td></tr>
                                <tr><td><strong>Lương thực nhận</strong></td><td style='text-align: right;'><strong>{netSalaryFormatted}</strong></td></tr>
                            </table>
                            <br/>
                            <p>Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ Bộ phận Kế toán.</p>
                            <p>Trân trọng,</p>
                            <p><strong>Bộ phận Kế toán</strong></p>
                        </body>
                        </html>";

                    await _gmailService.SendEmailAsync(employee.Email, subject, body);
                    _logger.LogInformation("Successfully sent salary email to employee {EmployeeId} ({Email}) by user {User}.", employeeId, employee.Email, User.Identity?.Name);
                    return Ok(new { Message = $"Salary email sent successfully to {employee.Email}." });
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, $"Failed to send salary email via Gmail API to employee {employeeId} ({employee.Email}) for month {salary.SalaryMonth:MM/yyyy}.");
                    return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Email Sending Failed", Detail = $"Failed to send salary email to {employee.Email}. Error: {emailEx.Message}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing salary email request for Employee ID {EmployeeId} by user {User}.", employeeId, User.Identity?.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Server Error", Detail = "An internal server error occurred while processing the request." });
            }
        }
    }
}