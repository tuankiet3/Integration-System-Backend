using Microsoft.AspNetCore.Mvc;
using Integration_System.DAL;
using Integration_System.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace Integration_System.Controllers
{
    [Route("api/positions")] // Route to access the API
    [ApiController]
    public class PositionControllers: Controller
    {
        public readonly PositionDAL _positionDAL;
        public readonly ILogger<PositionControllers> _logger;
        public PositionControllers(PositionDAL positionDAL, ILogger<PositionControllers> logger)
        {
            _positionDAL = positionDAL;
            _logger = logger;
        }
        [HttpGet] // api/positions
        [ProducesResponseType(typeof(IEnumerable<PositionModel>), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPositions()
        {
            try
            {
                List<PositionModel> positions = await _positionDAL.getPositions();
                _logger.LogInformation("Retrieved all positions successfully");
                return Ok(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving positions");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
        [HttpGet("{PositionID}")] // api/positions/{id}
        [ProducesResponseType(typeof(PositionModel), statusCode: 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPositionById(int PositionID)
        {
            try
            {
                PositionModel position = await _positionDAL.GetPositionByID(PositionID);
                if (position == null)
                {
                    _logger.LogWarning($"Position with ID {PositionID} not found");
                    return NotFound("Position not found");
                }
                _logger.LogInformation($"Retrieved position with ID {PositionID} successfully");
                return Ok(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving position by ID");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
