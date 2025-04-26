// File: Services/AuthService.cs
using Integration_System.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging; // Thêm using cho ILogger
using Microsoft.Extensions.Configuration; // Thêm using cho IConfiguration

namespace Integration_System.Services
{

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _configuration = configuration;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<bool> SetRole(int departmentID, string userName, IdentityUser user)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);

            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeRolesResult.Succeeded)
            {
                var removeErrors = string.Join(", ", removeRolesResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                _logger.LogError("Failed to remove existing roles from user {Username}. Errors: {Errors}", userName, removeErrors);
                return false;
            }

            string newRole = departmentID switch
            {
                1 => UserRoles.Hr,
                2 => UserRoles.PayrollManagement,
                _ => UserRoles.Employee
            };

            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                _logger.LogError("Role '{RoleName}' does not exist. Cannot assign to user {Username}.", newRole, userName);
                return false;
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, newRole);
            if (addToRoleResult.Succeeded)
            {
                _logger.LogInformation("Assigned role '{RoleName}' to user {Username}.", newRole, userName);
                return true;
            }
            else
            {
                var roleErrors = string.Join(", ", addToRoleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                _logger.LogError("Failed to assign role '{RoleName}' to user {Username}. Errors: {Errors}", newRole, userName, roleErrors);
                return false;
            }
        }

        public JwtSecurityToken CreateToken(List<Claim> authClaims)
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
                tokenValidityInMinutes = 60;
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

        public async Task<bool> DeleteUser(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("DeleteUser called with null or empty email.");
                return false;
            }

            _logger.LogInformation("Attempting to delete Identity user with email: {Email}", email);
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Identity user with email {Email} not found. No deletion needed.", email);
                return true;
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("Successfully deleted Identity user with email {Email} (UserId: {UserId}).", email, user.Id);
                return true;
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                _logger.LogError("Failed to delete Identity user with email {Email} (UserId: {UserId}). Errors: {Errors}", email, user.Id, errors);
                return false;
            }
        }
    }
}