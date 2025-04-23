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
        // allow to access without authentication
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Registration attempt for user: {Username}", model.Username);

            // check if email exists
            var emailExists = await _userManager.FindByEmailAsync(model.Email);
            if (emailExists != null)
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists.", model.Email);
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = $"Email '{model.Email}' already exists!" });
            }

            // create user
            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                EmailConfirmed = true, // confirm email by default
            };

            // save user to database
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                _logger.LogError("User creation failed for {Username}. Errors: {Errors}", model.Username, errors);
                // Trả về lỗi chi tiết hơn nếu có thể
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "User Creation Failed", Detail = errors });
            }

            _logger.LogInformation("User {Username} created successfully. Assigning default role.", model.Username);

            // set default role is Employee
            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                var addToRoleResult = await _userManager.AddToRoleAsync(user, UserRoles.Admin);
                if (addToRoleResult.Succeeded)
                {
                    _logger.LogInformation("Assigned role '{RoleName}' to user {Username}.", UserRoles.Admin, model.Username);
                }
                else
                {
                    var roleErrors = string.Join(", ", addToRoleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    _logger.LogError("Failed to assign role '{RoleName}' to user {Username}. Errors: {Errors}", UserRoles.Employee, model.Username, roleErrors);
                    // Cân nhắc xử lý lỗi này (ví dụ: xóa user vừa tạo hoặc báo lỗi cụ thể)
                }
            }
            else
            {
                _logger.LogWarning("Role '{RoleName}' does not exist. Cannot assign to user {Username}.", UserRoles.Employee, model.Username);
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
            // find user by username
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id), // UserId
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty), // UserName
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""), // Email
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique ID of token
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole)); // Thêm các roles vào claims
                }
                // create token based on claims
                var token = _authService.CreateToken(authClaims);
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogInformation("Login attempt for user: {email}", model.Email);
                return Ok(new LoginResponseDto
                {
                    Token = tokenString,
                    Expiration = token.ValidTo,
                    Username = user.UserName,
                    Roles = userRoles.ToList() // return roles as a list
                });
            }
            _logger.LogWarning("Login failed for user: {Email}. Invalid Email or password.", model.Email);
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "Invalid username or password." });
        }
    }
}
