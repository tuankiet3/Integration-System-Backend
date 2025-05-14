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
using Integration_System.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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

        public SalaryControllers(SalaryDAL salaryDAL, ILogger<SalaryControllers> logger, NotificationSalaryMDW notificationSalary, NotificationSalaryService notificationSalaryService)
        {
            _salaryDAL = salaryDAL;
            _logger = logger;
            _notificationSalary = notificationSalary;
            _notificationSalaryService = notificationSalaryService;
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.PayrollManagement}")]
        [ProducesResponseType(typeof(IEnumerable<SalaryModel>), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllSalaries()
        {
            try
            {
                List<SalaryModel> salaries = await _salaryDAL.getSalaries();
                _logger.LogInformation("User {User} retrieved all salaries successfully", User.Identity?.Name);
                return Ok(salaries ?? Enumerable.Empty<SalaryModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all salaries");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Server Error", Detail = "Internal server error retrieving salaries." });
            }
        }
        [HttpGet("employee/{employeeID}")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.PayrollManagement},{UserRoles.Employee}")]
        [ProducesResponseType(typeof(IEnumerable<SalaryModel>), statusCode: 200)]
        [ProducesResponseType(typeof(SalaryModel), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSalaryByEmployeeId(int employeeID)
        {
            var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdminOrPayroll = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.PayrollManagement);

            if (!isAdminOrPayroll)
            {
                _logger.LogWarning("Potential Forbidden Access: User {UserId} (Role: Employee) requested salary for employee {TargetEmployeeId}. Access allowed for now, requires ID check implementation.", currentUserIdClaim, employeeID);
            }
            try
            {
               List<SalaryModel> salary = await _salaryDAL.GetSalariesByEmployeeIDAsync(employeeID);
                if (salary == null)
                {
                    _logger.LogWarning($"Salary for employee {employeeID} not found.");
                    return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Salary not found for employee {employeeID}." });
                }
                _logger.LogInformation($"User {User.Identity?.Name} retrieved salary for employee {employeeID} successfully.");
                return Ok(salary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salary for employee {EmployeeId}", employeeID);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Server Error", Detail = "Internal server error retrieving salary." });
            }
        }
        [HttpGet("employee/{employeeID}/history/{month}")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.PayrollManagement},{UserRoles.Employee}, {UserRoles.Hr}")]
        [ProducesResponseType(typeof(SalaryModel), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHistorySalary(int employeeID, int month)
        {
            var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdminOrPayroll = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.PayrollManagement);

            if (!isAdminOrPayroll)
            {
                _logger.LogWarning("Potential Forbidden Access: User {UserId} (Role: Employee) requested salary history for employee {TargetEmployeeId}. Access allowed for now, requires ID check implementation.", currentUserIdClaim, employeeID);
            }

            try
            {
                SalaryModel? salary = await _salaryDAL.getHistorySalary(employeeID, month);
                if (salary == null)
                {
                    _logger.LogWarning($"Salary history for employee {employeeID} in month {month} not found.");
                    return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Salary history not found for employee {employeeID}, month {month}." });
                }
                _logger.LogInformation($"User {User.Identity?.Name} retrieved salary history for employee {employeeID} in month {month} successfully.");
                return Ok(salary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salary history for employee {EmployeeId}, month {Month}", employeeID, month);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Server Error", Detail = "Internal server error retrieving salary history." });
            }
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.PayrollManagement}")]
        [ProducesResponseType(typeof(object), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateSalary([FromBody] SalaryInsertDTO salary)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (salary == null)
            {
                _logger.LogWarning("CreateSalary attempted with a null DTO by user {User}.", User.Identity?.Name);
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Salary data cannot be null." });
            }
            try
            {
                bool checkInsert = await _salaryDAL.checkInsertSalary(salary.EmployeeId, salary.SalaryMonth);
                if (checkInsert) { 
                    return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = $"Salary record already exists for this employeeID {salary.EmployeeId} for month {salary.SalaryMonth.Month}." });
                }
                bool isWarning = await _notificationSalary.CheckAndNotifySalary(salary);
                if (isWarning)
                {
                    _logger.LogInformation("Unusual salary deviation detected and notification sent for EmployeeId {EmployeeId} by user {User}.", salary.EmployeeId, User.Identity?.Name);
                    return BadRequest(new ProblemDetails { Title = "Warning", Detail = "Unusual salary deviation detected. Notification sent." });
                }

                bool createdSalary = await _salaryDAL.InserSalary(salary);
                if (createdSalary)
                {
                    _logger.LogInformation($"User {User.Identity?.Name} created salary successfully for EmployeeId {salary.EmployeeId} for month {salary.SalaryMonth:yyyy-MM}.");
                    return Ok(new { Message = "Salary Created Successfully" });
                }
                else
                {
                    _logger.LogError("Failed to insert salary into database for EmployeeId {EmployeeId}, month {SalaryMonth}, by user {User}.", salary.EmployeeId, salary.SalaryMonth, User.Identity?.Name);
                    return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Operation Failed", Detail = "Failed to save salary record." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating salary for EmployeeId {EmployeeId} by user {User}", salary.EmployeeId, User.Identity?.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Server Error", Detail = "An internal error occurred while creating the salary record." });
            }
        }
    }
}
