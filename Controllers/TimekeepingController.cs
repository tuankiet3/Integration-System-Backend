using Integration_System.DAL;
using Integration_System.Dtos.AttendanceDto;
using Integration_System.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Integration_System.Controllers
{
    [Route("api/[controller]")] // api/timekeeping
    [ApiController]
    public class TimekeepingController : Controller
    {
        private readonly AttendanceDAL _attendanceDAL;
        private readonly ILogger<TimekeepingController> _logger;

        public TimekeepingController(IConfiguration configuration,ILogger<TimekeepingController> logger,ILogger<AttendanceDAL> dalLogger)
        {
            _logger = logger;
            _attendanceDAL = new AttendanceDAL(configuration, dalLogger);
        }

        [HttpGet] //GET: api/timekeeping
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

        [HttpGet("{attendanceId}")] //GET: api/timekeeping/{attendanceId}
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

        // PUT: api/timekeeping/{id}
        [HttpPut("{attendanceId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAttendance(int attendanceId, [FromBody] UpdateAttendanceDto attendanceDto)
        {
            // Validate DTO
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Request updateattendance is not valid for ID {AttendanceID}: {Modelstate}", attendanceId, ModelState);
                return ValidationProblem(ModelState);
            }

            // check if attendance record exists
            var existingRecord = await _attendanceDAL.GetAttendanceByAttendanceIdAsync(attendanceId);
            if (existingRecord == null)
            {
                _logger.LogWarning("The ID {AttendanceID} timekeeping is not found to update.", attendanceId);
                return NotFound(new { message = $"The record not found with ID {attendanceId}." });
            }


            try
            {
                bool success = await _attendanceDAL.UpdateAttendanceAsync(attendanceId, attendanceDto);
                _logger.LogInformation("Success update the ID Timekeeping: {AttendanceID}", attendanceId);
                return NoContent();
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unwanted Error in Action Updateattantance for ID {AttendanceId}: {@AttendanceDto}", attendanceId, attendanceDto);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error has occurred the server when updating timekeeping.*");
            }
        }


    }
}
