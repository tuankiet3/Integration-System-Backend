using Microsoft.AspNetCore.Mvc;
using Integration_System.DAL;
using Integration_System.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Integration_System.Dtos;
using Integration_System.Dtos.SalaryDTO;
namespace Integration_System.Controllers
{
    [Route("api/salaries")] // Route to access the API
    [ApiController]
    public class SalaryControllers : Controller
    {
        private readonly SalaryDAL _salaryDAL;
        private readonly ILogger<SalaryControllers> _logger;
        public SalaryControllers(SalaryDAL salaryDAL, ILogger<SalaryControllers> logger)
        {
            _salaryDAL = salaryDAL;
            _logger = logger;
        }
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
        [HttpGet("{salaryID}")] // api/salaries/{id}
        [ProducesResponseType(typeof(SalaryModel), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSalaryBySalaryID(int salaryID)
        {
            try
            {
                SalaryModel salary = await _salaryDAL.getSalaryBySalaryID(salaryID);
                if (salary == null)
                {
                    _logger.LogWarning($"Salary with ID {salaryID} not found");
                    return NotFound();
                }
                _logger.LogInformation($"Retrieved salary with ID {salaryID} successfully");
                return Ok(salary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salary by ID");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
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
                bool checkEmployeeID = await _salaryDAL.CheckEmployeeID(salary.EmployeeId);
                if (checkEmployeeID == true)
                {
                    _logger.LogWarning($"EmployeeID has already");
                    return BadRequest("EmployeeID has already");
                }
                else
                {
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
                 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating salary");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
        [HttpPut("{salaryID}")] // api/salaries/{id}
        [ProducesResponseType(statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSalary(int salaryID, [FromBody] SalaryUpdateDTO salary)
        {
            if (salary == null)
            {
                _logger.LogWarning("Salary object is null");
                return BadRequest("Salary object is null");
            }
            try
            {
                bool updatedSalary = await _salaryDAL.UpdateSalary(salaryID, salary);
                if (updatedSalary == false)
                {
                    _logger.LogWarning($"Salary with ID {salaryID} not found");
                    return NotFound();
                }
                _logger.LogInformation($"Updated salary with ID {salaryID} successfully");
                return Ok(new {Message = "Salary Updated Succesfully"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating salary");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
        [HttpDelete("{salaryID}")] // api/salaries/{id}
        [ProducesResponseType(statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSalary(int salaryID)
        {
            try
            {
                bool deletedSalary = await _salaryDAL.DeleteSalary(salaryID);
                if (deletedSalary == false)
                {
                    _logger.LogWarning($"Salary with ID {salaryID} not found");
                    return NotFound();
                }
                _logger.LogInformation($"Deleted salary with ID {salaryID} successfully");
                return Ok(new { Message = "Salary deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting salary");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
