namespace Integration_System.Services
{
    public interface IGmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
    }
}
