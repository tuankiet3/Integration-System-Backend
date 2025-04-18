namespace Integration_System.Dtos.AuthenticationDTO
{
    public class LoginResponseDto
    {
        public required string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string? Username { get; set; }
        public List<string>? Roles { get; set; }
    }
}
