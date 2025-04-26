using System.ComponentModel.DataAnnotations;

namespace Integration_System.Dtos.AuthenticationDTO
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}
