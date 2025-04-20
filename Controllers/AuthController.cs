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

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
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

            // check if user exists
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                _logger.LogWarning("Registration failed: Username {Username} already exists.", model.Username);
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = $"Username '{model.Username}' already exists!" });
            }

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
            if (await _roleManager.RoleExistsAsync(UserRoles.Employee))
            {
                var addToRoleResult = await _userManager.AddToRoleAsync(user, UserRoles.Employee);
                if (addToRoleResult.Succeeded)
                {
                    _logger.LogInformation("Assigned role '{RoleName}' to user {Username}.", UserRoles.Employee, model.Username);
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
            _logger.LogInformation("Login attempt for user: {Username}", model.Username);
            // find user by username
            var user = await _userManager.FindByNameAsync(model.Username);
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
                var token = CreateToken(authClaims);
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogInformation("User {Username} logged in successfully.", model.Username);
                return Ok(new LoginResponseDto
                {
                    Token = tokenString,
                    Expiration = token.ValidTo,
                    Username = user.UserName,
                    Roles = userRoles.ToList() // return roles as a list
                });
            }
            _logger.LogWarning("Login failed for user: {Username}. Invalid username or password.", model.Username);
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "Invalid username or password." });
        }
        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                _logger.LogError("JWT Key, Issuer or Audience is not configured in appsettings.json");
                throw new InvalidOperationException("JWT settings are missing or invalid.");
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);
            if (tokenValidityInMinutes <= 0)
            {
                tokenValidityInMinutes = 60; // Default value
                _logger.LogWarning("JWT:TokenValidityInMinutes not configured or invalid. Using default value: {DefaultMinutes} minutes.", tokenValidityInMinutes);
            }


            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

    }
}
