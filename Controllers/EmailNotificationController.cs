 using Microsoft.AspNetCore.Mvc;
// using Integration_System.Services; // Xóa using cũ nếu không dùng IEmailService nữa
using Integration_System.DAL;
using Integration_System.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;
using Integration_System.Services; // Thêm using cho IGmailService

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendSalaryNotifications([FromQuery] int month, [FromQuery] int year)
        {
            if (month < 1 || month > 12 || year <= 0)
            {
                return BadRequest("Invalid month or year.");
            }

            _logger.LogInformation($"Starting to send salary notifications via Gmail API for {month}/{year}");
            int successCount = 0;
            int failCount = 0;
            List<string> failedEmails = new List<string>();

            try
            {
                List<EmployeeModel> employees = await _employeeDAL.GetAllEmployeesAsync();

                foreach (var employee in employees)
                {
                    if (string.IsNullOrEmpty(employee.Email))
                    {
                        _logger.LogWarning($"Employee {employee.EmployeeId} ({employee.FullName}) does not have an email address. Skipping.");
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
                        continue;
                    }

                    try
                    {
                        string subject = $"Thông báo lương tháng {month}/{year} - {employee.FullName}";
                        string body = $@"
                            <html>
                            <body>
                                <h2>Xin chào {employee.FullName},</h2>
                                <p>Công ty xin gửi bạn thông tin lương tháng {month}/{year}:</p>
                                <ul>
                                    <li><strong>Lương cơ bản:</strong> {salary.BaseSalary:N0} VND</li>
                                    <li><strong>Thưởng:</strong> {salary.Bonus:N0} VND</li>
                                    <li><strong>Khấu trừ:</strong> {salary.Deductions:N0} VND</li>
                                    <li><strong>Lương thực nhận:</strong> {salary.NetSalary:N0} VND</li>
                                </ul>
                                <p>Trân trọng,</p>
                                <p><strong>Bộ phận Kế toán</strong></p>
                            </body>
                            </html>";

                        await _gmailService.SendEmailAsync(employee.Email, subject, body);
                        successCount++;
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, $"Failed to send salary email via Gmail API to {employee.FullName} ({employee.Email}) for {month}/{year}.");
                        failCount++;
                        failedEmails.Add($"{employee.FullName} ({employee.Email})");
                    }
                }

                _logger.LogInformation($"Finished sending salary notifications via Gmail API for {month}/{year}. Success: {successCount}, Failed: {failCount}");

                if (failCount > 0)
                {
                    return Ok(new
                    {
                        Message = $"Process completed. Sent: {successCount}, Failed: {failCount}.",
                        FailedRecipients = failedEmails
                    });
                }

                return Ok(new { Message = $"Successfully sent {successCount} salary notification emails via Gmail API for {month}/{year}." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An critical error occurred during the SendSalaryNotifications (Gmail API) process for {month}/{year}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred while sending salary notifications.");
            }
        }
    }
}