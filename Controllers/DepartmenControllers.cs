using Integration_System.DAL;
using Integration_System.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
namespace Integration_System.Controllers
{
    [Route("api/departments")] // Route to access the API
    public class DepartmenControllers : Controller
    {
        public readonly DepartmentDAL _departmentDAL;
        public readonly ILogger<DepartmenControllers> _logger;
        public DepartmenControllers(DepartmentDAL departmentDAL, ILogger<DepartmenControllers> logger)
        {
            _departmentDAL = departmentDAL;
            _logger = logger;
        }
        [HttpGet] // api/departments
        [ProducesResponseType(typeof(IEnumerable<DepartmentModel>), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDepartments()
        {
            try
            {
                List<DepartmentModel> departments = await _departmentDAL.getDepartments();
                _logger.LogInformation("Retrieved all departments successfully");
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving departments");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
        [HttpGet("{DepartmentID}")] // api/departments/{id}
        [ProducesResponseType(typeof(DepartmentModel), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDepartmentById(int DepartmentID)
        {
            try
            {
                DepartmentModel department = await _departmentDAL.GetDepartmentByID(DepartmentID);
                if (department == null)
                {
                    _logger.LogWarning($"Department with ID {DepartmentID} not found");
                    return NotFound("Department not found");
                }
                _logger.LogInformation($"Retrieved department with ID {DepartmentID} successfully");
                return Ok(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving department by ID");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
    
}

