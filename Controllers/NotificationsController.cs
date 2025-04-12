using Integration_System.Dtos.NotificationDTO;
using Integration_System.Middleware;
using Integration_System.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Integration_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ILogger<NotificationsController> _logger;

        private readonly NotificationSalaryMDW _notificationSalary;
        private readonly NotificationSalaryService _notificationSalaryService;
        public NotificationsController(ILogger<NotificationsController> logger, NotificationSalaryMDW notificationSalary, NotificationSalaryService notificationSalaryService)
        {
            _logger = logger;
            _notificationSalary = notificationSalary;
            _notificationSalaryService = notificationSalaryService;
        }

        // POST: api/notifications/trigger/anniversary
        [HttpPost("trigger/anniversary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TriggerAnniversaryNotifications()
        {
            _logger.LogInformation("Manual trigger for anniversary notifications requested.");
            try
            {
                _logger.LogInformation("Executing anniversary notification trigger (Placeholder)...");
                await Task.Delay(50); // Simulate work
                _logger.LogInformation("Anniversary notification process triggered (Placeholder).");
                return Ok(new { message = "Anniversary notification process triggered successfully (Placeholder)." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering anniversary notifications.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Internal Server Error", Detail = "Error triggering anniversary notifications.", Status = StatusCodes.Status500InternalServerError });
            }
        }

        // POST: api/notifications/trigger/leave
        [HttpPost("trigger/leave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TriggerLeaveNotifications()
        {
            _logger.LogInformation("Manual trigger for excess leave notifications requested.");
            try
            {
                _logger.LogInformation("Executing excess leave notification trigger (Placeholder)...");
                await Task.Delay(50);
                _logger.LogInformation("Excess leave notification process triggered (Placeholder).");
                return Ok(new { message = "Excess leave notification process triggered successfully (Placeholder)." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering excess leave notifications.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Internal Server Error", Detail = "Error triggering excess leave notifications.", Status = StatusCodes.Status500InternalServerError });
            }
        }

        // POST: api/notifications/trigger/payroll-email
        [HttpPost("trigger/payroll-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TriggerPayrollEmailNotifications([FromQuery] int? year, [FromQuery] int? month)
        {
            if (!year.HasValue || !month.HasValue || month < 1 || month > 12)
            {
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Valid year and month query parameters are required.", Status = StatusCodes.Status400BadRequest });
            }
            _logger.LogInformation("Manual trigger for payroll email notifications requested for {Year}-{Month}.", year, month);
            try
            {
                _logger.LogInformation("Executing payroll email notification trigger for {Year}-{Month} (Placeholder)...", year, month);
                await Task.Delay(50);
                _logger.LogInformation("Payroll email notification process triggered for {Year}-{Month} (Placeholder).", year, month);
                return Ok(new { message = $"Payroll email notification process triggered successfully for {year}-{month} (Placeholder)." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering payroll email notifications for {Year}-{Month}.", year, month);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Internal Server Error", Detail = $"Error triggering payroll email notifications for {year}-{month}.", Status = StatusCodes.Status500InternalServerError });
            }
        }
        // GET all salary notifications
        [HttpGet("notifications")]
        [ProducesResponseType(typeof(List<NotificationSalaryDTO>), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSalaryNotifications()
        {
            try
            {
                var notifications = await _notificationSalaryService.GetAllNotificationsAsync();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salary notifications from Redis");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}