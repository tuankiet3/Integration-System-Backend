using System.ComponentModel.DataAnnotations;

namespace Integration_System.Dtos.AuthenticationDTO
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "User Name is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public required string Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public required string Password { get; set; }
    }
}
