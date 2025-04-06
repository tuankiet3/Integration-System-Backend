using Integration_System.DAL;
using Integration_System.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Integration_System.Controllers
{
    [Route("api/employees")] // Route to access the API
    [ApiController]
    public class EmployeeControllers : Controller
    {
        private readonly EmployeeDAL _employeeDAL;
        private readonly ILogger<EmployeeControllers> _logger;

        public EmployeeControllers(EmployeeDAL employeeDAL, ILogger<EmployeeControllers> logger)
        {
            _employeeDAL = employeeDAL;
            _logger = logger;
        }


        [HttpGet] // api/employees
        [ProducesResponseType(typeof(IEnumerable<EmployeeModel>), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                List<EmployeeModel> employees = await _employeeDAL.GetAllEmployeesAsync();
                _logger.LogInformation("Retrieved all employees successfully");
                return Ok(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }


        [HttpGet("{id}")] // api/employees/{id}
        [ProducesResponseType(typeof(EmployeeModel), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployeeById(int EmployeeId)
        {
            try
            {
                EmployeeModel employee = await _employeeDAL.GetEmployeeIdAsync(EmployeeId);
                if (employee == null)
                {
                    _logger.LogWarning($"Employee with ID {EmployeeId} not found");
                    return NotFound();
                }
                _logger.LogInformation($"Retrieved employee with ID {EmployeeId} successfully");
                return Ok(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
        [HttpPost] // api/employees
        [ProducesResponseType(typeof(EmployeeModel), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeModel employee)
        {
            if (employee == null)
            {
                _logger.LogWarning("Employee object is null");
                return BadRequest("Employee object is null");
            }
            try
            {
                bool createdEmployee = await _employeeDAL.InsertEmployeeAsync(employee);
                if (!createdEmployee)
                {
                    _logger.LogWarning("Failed to create employee");
                    return BadRequest("Failed to create employee");
                }
                else
                {
                    _logger.LogInformation("Employee created successfully");
                    return Ok(new { Message = "Employee Inserted successfully." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPut("{id}")] // api/employees/{id}
        [ProducesResponseType(typeof(EmployeeModel), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEmployee(int EmployeeId, [FromBody] EmployeeModel employee)
        {
            if (employee == null)
            {
                _logger.LogWarning("Employee object is null");
                return BadRequest("Employee object is null");
            }
            try
            {
                bool updatedEmployee = await _employeeDAL.UpdateEmployee(EmployeeId, employee);
                if (!updatedEmployee)
                {
                    _logger.LogWarning($"Failed to update employee with ID {EmployeeId}");
                    return NotFound(new {Message = "Failed to update" });
                }
                _logger.LogInformation($"Employee with ID {EmployeeId} updated successfully");
                return Ok(new { Message = "Employee updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
        [HttpDelete("{id}")] // api/employees/{id}
        [ProducesResponseType(typeof(EmployeeModel), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEmployee(int EmployeeId)
        {
            try
            {
                bool deletedEmployee = await _employeeDAL.DeleteEmployeeAsync(EmployeeId);
                if (!deletedEmployee)
                {
                    _logger.LogWarning($"Failed to delete employee with ID {EmployeeId}");
                    return NotFound();
                }
                _logger.LogInformation($"Employee with ID {EmployeeId} deleted successfully");
                return Ok(new {Message ="Deleted sucsessasdajsd"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

    }
}

