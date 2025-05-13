using Microsoft.AspNetCore.Mvc;
using Integration_System.DAL;
using Integration_System.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Integration_System.Constants;

namespace Integration_System.Controllers
{
    [Route("api/positions")]
    [ApiController]
    [Authorize] 
    public class PositionControllers : Controller
    {
        public readonly PositionDAL _positionDAL;
        public readonly ILogger<PositionControllers> _logger;
        public PositionControllers(PositionDAL positionDAL, ILogger<PositionControllers> logger)
        {
            _positionDAL = positionDAL;
            _logger = logger;
        }
        [HttpGet]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Hr}, {UserRoles.PayrollManagement}")]
        [ProducesResponseType(typeof(IEnumerable<PositionModel>), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPositions()
        {
            try
            {
                List<PositionModel> positions = await _positionDAL.getPositions();
                _logger.LogInformation("Retrieved all positions successfully by user {User}", User.Identity?.Name);
                return Ok(positions ?? Enumerable.Empty<PositionModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving positions");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Server Error", Detail = "Internal server error retrieving positions." });
            }
        }
        [HttpGet("{PositionID}")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Hr}, {UserRoles.PayrollManagement}")]
        [ProducesResponseType(typeof(PositionModel), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPositionById(int PositionID)
        {
            try
            {
                PositionModel? position = await _positionDAL.GetPositionByID(PositionID);
                if (position == null)
                {
                    _logger.LogWarning($"Position with ID {PositionID} not found");
                    return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Position with ID {PositionID} not found." });
                }
                _logger.LogInformation($"Retrieved position with ID {PositionID} successfully by user {User.Identity?.Name}");
                return Ok(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving position by ID {PositionID}", PositionID);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Server Error", Detail = "Internal server error retrieving position." });
            }
        }
    }
}