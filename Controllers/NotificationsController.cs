using Integration_System.DAL;
using Integration_System.Dtos.NotificationDTO;
using Integration_System.Middleware;
using Integration_System.Model;
using Integration_System.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; 
using Integration_System.Constants;       

namespace Integration_System.Controllers
{
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly ILogger<NotificationsController> _logger;
        private readonly NotificationSalaryMDW _notificationSalaryMDW;
        private readonly NotificationSalaryService _notificationSalaryService;

        public NotificationsController(
            ILogger<NotificationsController> logger,
            NotificationSalaryMDW notificationSalaryMDW,
            NotificationSalaryService notificationSalaryService)
        {
            _logger = logger;
            _notificationSalaryMDW = notificationSalaryMDW;
            _notificationSalaryService = notificationSalaryService;
        }

        [HttpPost("anniversary")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> TriggerAnniversaryNotifications()
        {
            try
            {
                bool result = await _notificationSalaryMDW.CheckAndNotificationAnniversary();
                if(result)
                {
                    _logger.LogInformation("Anniversary notifications triggered successfully.");
                    return Ok(new { Message = "Anniversary notifications triggered successfully." });
                }
                else
                {
                    _logger.LogWarning("No anniversary notifications to trigger.");
                    return Ok(new { Message = "No anniversary notifications to trigger." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual trigger of anniversary notifications.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Internal Server Error", Detail = "Error triggering anniversary notifications.", Status = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost("absent")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> TriggerAbsentDayNotifications(int employeeId, int month)
        {
            try
            {
                bool result = await _notificationSalaryMDW.CheckAndNotifyAbsentDays(employeeId, month);
                if (result)
                {
                    _logger.LogInformation("Absent day notifications triggered successfully.");
                    return Ok(new { Message = "Absent day notifications triggered successfully."
                    });
                }
                else
                {
                    _logger.LogWarning("No absent day notifications to trigger.");
                    return Ok(new { Message = "No absent day notifications to trigger." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual trigger of absent day notifications.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Internal Server Error", Detail = "Error triggering absent day notifications.", Status = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(List<NotificationSalaryDTO>), statusCode: 200)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSalaryNotifications()
        {
            _logger.LogInformation("User {User} requested list of salary/system notifications from Redis.", User.Identity?.Name);
            try
            {
                var notifications = await _notificationSalaryService.GetAllNotificationsAsync();
                _logger.LogInformation("Retrieved {Count} notifications from Redis.", notifications?.Count ?? 0);
                return Ok(notifications ?? new List<NotificationSalaryDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salary/system notifications from Redis.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Server Error", Detail = "Internal server error retrieving notifications from Redis." });
            }
        }
    }
}