using Integration_System.DAL;
using Integration_System.Dtos.AttendanceDto;
using Integration_System.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Integration_System.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Integration_System.Controllers
{
    [Route("api/[controller]")] // api/attendances
    [ApiController]
    [Authorize]
    public class AttendancesController : Controller
    {
        private readonly AttendanceDAL _attendanceDAL;
        private readonly ILogger<AttendancesController> _logger;

        public AttendancesController(AttendanceDAL attendanceDAL, ILogger<AttendancesController> logger) 
        {
            _attendanceDAL = attendanceDAL;
            _logger = logger;
        }

        [HttpGet] //GET: api/attendances
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Hr}")]
        [ProducesResponseType(typeof(IEnumerable<AttendanceModel>), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAttendances()
        {
            try
            {
                var attendanceRecords = await _attendanceDAL.GetAttendancesAsync();
                int count = attendanceRecords?.Count() ?? 0; 
                _logger.LogInformation("Retrieved {Count} attendance records successfully by user {User}.", count, User.Identity?.Name);
                return Ok(attendanceRecords ?? Enumerable.Empty<AttendanceModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all attendance records.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Server Error", Detail = "Internal server error retrieving attendance records." });
            }
        }

        [HttpGet("{attendanceId}")] //GET: api/attendances/{attendanceId}
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Hr}")]
        [ProducesResponseType(typeof(AttendanceModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceById(int attendanceId)
        {
            try
            {
                var attendanceRecord = await _attendanceDAL.GetAttendanceByAttendanceIdAsync(attendanceId);
                if (attendanceRecord == null)
                {
                    _logger.LogWarning("Attendance record with ID {AttendanceId} not found.", attendanceId);
                    return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Attendance record with ID {attendanceId} not found." });
                }
                _logger.LogInformation("Attendance record with ID {AttendanceId} retrieved successfully by user {User}.", attendanceId, User.Identity?.Name);
                return Ok(attendanceRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance record with ID {AttendanceId}.", attendanceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Server Error", Detail = "Internal server error retrieving attendance record." });
            }
        }
    }
}
