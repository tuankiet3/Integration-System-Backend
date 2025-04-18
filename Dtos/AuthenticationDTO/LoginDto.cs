using System.ComponentModel.DataAnnotations;

namespace Integration_System.Dtos.AuthenticationDTO
{
    public class LoginDto
    {
        [Required(ErrorMessage = "User Name is required")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}
