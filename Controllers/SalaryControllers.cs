using Microsoft.AspNetCore.Mvc;
using Integration_System.DAL;
using Integration_System.Model;
using Integration_System.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Integration_System.Dtos;
using Integration_System.Dtos.SalaryDTO;
using Integration_System.Middleware;
using Integration_System.Dtos.NotificationDTO;
namespace Integration_System.Controllers
{
    [Route("api/salaries")] // Route to access the API
    [ApiController]
    public class SalaryControllers : Controller
    {
        private readonly SalaryDAL _salaryDAL;
        private readonly ILogger<SalaryControllers> _logger;
        private readonly NotificationSalaryMDW _notificationSalary;
        private readonly NotificationSalaryService _notificationSalaryService;
        // Constructor to initialize dependencies
        public SalaryControllers(SalaryDAL salaryDAL, ILogger<SalaryControllers> logger, NotificationSalaryMDW notificationSalary, NotificationSalaryService notificationSalaryService)
        {
            _salaryDAL = salaryDAL;
            _logger = logger;
            _notificationSalary = notificationSalary;
            _notificationSalaryService = notificationSalaryService;
        }
        // GET all salaries
        [HttpGet] // api/salaries
        [ProducesResponseType(typeof(IEnumerable<SalaryModel>), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllSalaries()
        {
            try
            {
                List<SalaryModel> salaries = await _salaryDAL.getSalaries();
                _logger.LogInformation("Retrieved all salaries successfully");
                return Ok(salaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salaries");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
        // GET salary by employee ID and month
        [HttpGet("/history/{employeeID}/{month}")] // api/salaries/history/{id}/{id}
        [ProducesResponseType(typeof(SalaryModel), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHistorySalary(int employeeID, int month)
        {
            try
            {
                SalaryModel? salary = await _salaryDAL.getHistorySalary(employeeID, month);
                if (salary == null)
                {
                    _logger.LogWarning($"Salary for employee {employeeID} in month {month} not found");
                    return NotFound();
                }
                _logger.LogInformation($"Retrieved salary for employee {employeeID} in month {month} successfully");
                return Ok(salary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salary by ID");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
        // Insert salary by employee ID
        [HttpPost] // api/salaries
        [ProducesResponseType(statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateSalary([FromBody] SalaryInsertDTO  salary)
        {
            if (salary == null)
            {
                _logger.LogWarning("Salary object is null");
                return BadRequest("Salary object is null");
            }
            try
            {
                bool isWarning = await _notificationSalary.CheckAndNotifySalary(salary);
                Console.WriteLine(isWarning);
                bool createdSalary = await _salaryDAL.InserSalary(salary);
                    if(createdSalary == true)
                    {
                        _logger.LogInformation($"Created salary successfully for EmployeeId {salary.EmployeeId}");
                        return Ok(new { Message = "Salary Created Successfully" });
                    }
                    else
                    {

                        _logger.LogWarning("Failed to create salary");
                        return BadRequest("Error creating salary");
                    
                     }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating salary");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
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
        //[HttpDelete("{salaryID}")] // api/salaries/{id}
        //[ProducesResponseType(statusCode: 200)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> DeleteSalary(int salaryID)
        //{
        //    try
        //    {
        //        bool deletedSalary = await _salaryDAL.DeleteSalary(salaryID);
        //        if (deletedSalary == false)
        //        {
        //            _logger.LogWarning($"Salary with ID {salaryID} not found");
        //            return NotFound();
        //        }
        //        _logger.LogInformation($"Deleted salary with ID {salaryID} successfully");
        //        return Ok(new { Message = "Salary deleted successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error deleting salary");
        //        return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        //    }
        //}

    }
}
