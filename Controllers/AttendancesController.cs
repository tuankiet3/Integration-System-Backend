using Integration_System.DAL;
using Integration_System.Dtos.AttendanceDto;
using Integration_System.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Integration_System.Controllers
{
    [Route("api/[controller]")] // api/addtendances
    [ApiController]
    public class AttendancesController : Controller
    {
        private readonly AttendanceDAL _attendanceDAL;
        private readonly ILogger<AttendancesController> _logger;

        public AttendancesController(IConfiguration configuration,ILogger<AttendancesController> logger,ILogger<AttendanceDAL> dalLogger)
        {
            _logger = logger;
            _attendanceDAL = new AttendanceDAL(configuration, dalLogger);
        }

        [HttpGet] //GET: api/attendances
        [ProducesResponseType(typeof(IEnumerable<AttendanceModel>), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAttendances()
        {
            try
            {
                var attendanceRecords = await _attendanceDAL.GetAttendancesAsync();
                _logger.LogInformation("Returns {Count} The timekeeping record.", ((List<AttendanceModel>)attendanceRecords).Count);
                return Ok(attendanceRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting all attendanceRecords");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpGet("{attendanceId}")] //GET: api/attendances/{attendanceId}
        [ProducesResponseType(typeof(AttendanceModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceById(int attendanceId)
        {
            try
            {
                var attendanceRecord = await _attendanceDAL.GetAttendanceByAttendanceIdAsync(attendanceId);
                if (attendanceRecord == null)
                {
                    _logger.LogWarning("Attendance record with ID {AttendanceId} not found", attendanceId);
                    return NotFound($"Attendance record with ID {attendanceId} not found.");
                }
                _logger.LogInformation("Attendance record with ID {AttendanceId} retrieved successfully", attendanceId);
                return Ok(attendanceRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting attendance record with ID {AttendanceId}", attendanceId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

    }
}
