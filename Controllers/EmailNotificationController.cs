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
    public class EmailNotificationController : ControllerBase // Đổi tên Controller nếu muốn
    {
        // ---- THAY ĐỔI Ở ĐÂY ----
        private readonly IGmailService _gmailService; // Đổi tên và kiểu
        // ---- KẾT THÚC THAY ĐỔI ----
        private readonly EmployeeDAL _employeeDAL;
        private readonly SalaryDAL _salaryDAL;
        private readonly ILogger<EmailNotificationController> _logger; // Đổi tên logger nếu đổi tên controller

        public EmailNotificationController(
            // ---- THAY ĐỔI Ở ĐÂY ----
            IGmailService gmailService, // Đổi kiểu tham số
                                        // ---- KẾT THÚC THAY ĐỔI ----
            EmployeeDAL employeeDAL,
            SalaryDAL salaryDAL,
            ILogger<EmailNotificationController> logger) // Đổi tên logger nếu đổi tên controller
        {
            // ---- THAY ĐỔI Ở ĐÂY ----
            _gmailService = gmailService; // Gán giá trị
            // ---- KẾT THÚC THAY ĐỔI ----
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
                // Lấy UserCredential một lần ở đầu nếu muốn, hoặc để trong vòng lặp nếu cần làm mới thường xuyên
                // await _googleAuthService.GetUserCredentialAsync(); // Ví dụ gọi để kích hoạt xác thực nếu chưa có

                List<EmployeeModel> employees = await _employeeDAL.GetAllEmployeesAsync();

                foreach (var employee in employees)
                {
                    // ... (Logic kiểm tra email, status, lấy salary giữ nguyên) ...
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

                    SalaryModel? salary = await _salaryDAL.getHistorySalary(employee.EmployeeId, month); // Nhớ kiểm tra logic lấy lương theo năm

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

                        // ---- THAY ĐỔI Ở ĐÂY ----
                        await _gmailService.SendEmailAsync(employee.Email, subject, body); // Gọi service mới
                        // ---- KẾT THÚC THAY ĐỔI ----
                        successCount++;
                    }
                    catch (Exception emailEx) // Bắt lỗi cụ thể hơn nếu cần (ví dụ: AuthenticationException từ MailKit)
                    {
                        _logger.LogError(emailEx, $"Failed to send salary email via Gmail API to {employee.FullName} ({employee.Email}) for {month}/{year}.");
                        failCount++;
                        failedEmails.Add($"{employee.FullName} ({employee.Email})");
                    }
                }

                _logger.LogInformation($"Finished sending salary notifications via Gmail API for {month}/{year}. Success: {successCount}, Failed: {failCount}");

                if (failCount > 0)
                {
                    // Trả về 207 Multi-Status nếu có cả thành công và thất bại? Hoặc vẫn là OK nhưng có chi tiết lỗi.
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
                // Lỗi này có thể xảy ra khi lấy danh sách nhân viên hoặc lỗi xác thực ban đầu
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred while sending salary notifications.");
            }
        }
    }
}