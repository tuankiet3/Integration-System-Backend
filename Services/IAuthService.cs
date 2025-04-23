using Integration_System.Constants;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Integration_System.Services
{
    public interface IAuthService
    {
        Task<bool> DeleteUser(string email);
        Task<bool> SetRole(int departmentID, string fullName, IdentityUser user);
        JwtSecurityToken CreateToken(List<Claim> authClaims);
    }
}
