// File: Integration-System/Controllers/AuthController.cs
using Integration_System.Dtos.AuthenticationDTO;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Integration_System.Dtos.AuthenticationDTO;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Integration_System.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Integration_System.Services;
using Integration_System.DAL;
namespace Integration_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ILogger<AuthController> logger, IAuthService authService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("register/admin")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Registration attempt for email: {Email}", model.Email);

            var emailExists = await _userManager.FindByEmailAsync(model.Email);
            if (emailExists != null)
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists.", model.Email);
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = $"Email '{model.Email}' already exists!" });
            }

            var userExists = await _userManager.FindByNameAsync(model.Email);
            if (userExists != null)
            {
                _logger.LogWarning("Registration failed: Username (Email) {Email} already exists.", model.Email);
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = $"Username '{model.Email}' already exists!" });
            }


            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email, // Use Email as UserName
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                _logger.LogError("User creation failed for {Email}. Errors: {Errors}", model.Email, errors);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "User Creation Failed", Detail = errors });
            }

            _logger.LogInformation("User with email {Email} created successfully. Assigning default role.", model.Email);

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                var addToRoleResult = await _userManager.AddToRoleAsync(user, UserRoles.Admin);
                if (addToRoleResult.Succeeded)
                {
                    _logger.LogInformation("Assigned role '{RoleName}' to user {Email}.", UserRoles.Admin, model.Email);
                }
                else
                {
                    var roleErrors = string.Join(", ", addToRoleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    _logger.LogError("Failed to assign role '{RoleName}' to user {Email}. Errors: {Errors}", UserRoles.Admin, model.Email, roleErrors);
                }
            }
            else
            {
                _logger.LogWarning("Role '{RoleName}' does not exist. Cannot assign to user {Email}.", UserRoles.Admin, model.Email);
            }

            return StatusCode(StatusCodes.Status201Created, new { Message = "User created successfully!" });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Login attempt for user: {email}", model.Email);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Email ?? string.Empty), // Use Email for Name claim
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = _authService.CreateToken(authClaims);
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogInformation("Login successful for user: {email}", model.Email);
                return Ok(new LoginResponseDto
                {
                    Id = user.Id,
                    Token = tokenString,
                    Expiration = token.ValidTo,
                    Username = user.Email, // Return Email as Username
                    Roles = userRoles.ToList(),
                   

                });
            }
            _logger.LogWarning("Login failed for user: {Email}. Invalid Email or password.", model.Email);
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "Invalid email or password." });
        }
    }
}