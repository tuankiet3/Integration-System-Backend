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

        [HttpPost("send-salary-notifications")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendSalaryNotifications([FromQuery] int month, [FromQuery] int year)
        {
            if (month < 1 || month > 12)
            {
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Invalid month. Month must be between 1 and 12." });
            }
            if (year <= 1900 || year > DateTime.Now.Year + 5)
            {
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Invalid year." });
            }

            _logger.LogInformation("User {User} initiated sending salary notifications via Gmail API for {Month}/{Year}", User.Identity?.Name, month, year);
            int successCount = 0;
            int failCount = 0;
            List<string> failedRecipientsDetails = new List<string>();

            try
            {
                List<EmployeeModel> employees = await _employeeDAL.GetAllEmployeesAsync();
                if (employees == null || !employees.Any())
                {
                    _logger.LogWarning("No employees found to send salary notifications.");
                    return Ok(new { Message = "No employees found to send notifications." });
                }

                foreach (var employee in employees)
                {
                    if (string.IsNullOrEmpty(employee.Email) || !employee.Email.Contains('@'))
                    {
                        _logger.LogWarning($"Employee {employee.EmployeeId} ({employee.FullName}) has an invalid or missing email address. Skipping.");
                        failCount++;
                        failedRecipientsDetails.Add($"{employee.FullName} (ID: {employee.EmployeeId}) - Invalid/Missing Email");
                        continue;
                    }

                    if (employee.Status != null && employee.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation($"Employee {employee.EmployeeId} ({employee.FullName}) is inactive. Skipping.");
                        continue;
                    }

                    SalaryModel? salary = await _salaryDAL.getHistorySalary(employee.EmployeeId, month);

                    if (salary == null)
                    {
                        _logger.LogWarning($"Salary for employee {employee.EmployeeId} for {month}/{year} not found. Skipping.");
                        failedRecipientsDetails.Add($"{employee.FullName} (ID: {employee.EmployeeId}) - Salary Data Not Found");
                        continue;
                    }

                    try
                    {
                        string subject = $"Thông báo lương tháng {month}/{year} - {employee.FullName}";
                        string baseSalaryFormatted = string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:N0} VND", salary.BaseSalary);
                        string bonusFormatted = string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:N0} VND", salary.Bonus);
                        string deductionsFormatted = string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:N0} VND", salary.Deductions);
                        string netSalaryFormatted = string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:N0} VND", salary.NetSalary);

                        string body = $@"
                            <html>
                            <body style='font-family: sans-serif;'>
                                <h2>Xin chào {employee.FullName},</h2>
                                <p>Công ty xin gửi bạn thông tin lương tháng {month}/{year} như sau:</p>
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
                        successCount++;
                        _logger.LogInformation("Successfully sent salary email to {FullName} ({Email})", employee.FullName, employee.Email);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, $"Failed to send salary email via Gmail API to {employee.FullName} ({employee.Email}) for {month}/{year}.");
                        failCount++;
                        failedRecipientsDetails.Add($"{employee.FullName} ({employee.Email}) - Error: {emailEx.Message}");
                    }
                    await Task.Delay(100);
                }

                string finalMessage = $"Finished sending salary notifications for {month}/{year}. Success: {successCount}, Failed/Skipped: {failCount + failedRecipientsDetails.Count(s => s.Contains("Not Found") || s.Contains("Invalid"))}";
                _logger.LogInformation(finalMessage);

                return Ok(new
                {
                    Message = finalMessage,
                    SuccessfulSends = successCount,
                    FailedOrSkippedDetails = failedRecipientsDetails
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"A critical error occurred during the SendSalaryNotifications process for {month}/{year}.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Server Error", Detail = "An internal server error occurred while processing salary notifications." });
            }
        }
    }
}