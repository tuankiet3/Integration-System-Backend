using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth.OAuth2;
using MailKit; // Cần thiết

namespace Integration_System.Services
{
    public class GmailService : IGmailService
    {
        private readonly IGoogleAuthService _googleAuthService;
        private readonly ILogger<GmailService> _logger;

        public GmailService(IGoogleAuthService googleAuthService, ILogger<GmailService> logger)
        {
            _googleAuthService = googleAuthService;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            try
            {
                var credential = await _googleAuthService.GetUserCredentialAsync(cancellationToken);
                if (credential?.Token == null)
                {
                    _logger.LogError("Failed to obtain Google User Credential or Token.");
                    throw new InvalidOperationException("Authentication failed: Could not get Google credentials.");
                }

                // Lấy access token hiện tại
                string accessToken = await credential.GetAccessTokenForRequestAsync(cancellationToken: cancellationToken);
                if (string.IsNullOrEmpty(accessToken))
                {
                    // Thử làm mới nếu access token không có (có thể đã hết hạn và chưa tự làm mới)
                    _logger.LogWarning("Access token was null or empty, attempting refresh before sending.");
                    if (await credential.RefreshTokenAsync(cancellationToken))
                    {
                        accessToken = await credential.GetAccessTokenForRequestAsync(cancellationToken: cancellationToken);
                        _logger.LogInformation("Token refreshed, proceeding to send email.");
                    }
                    else
                    {
                        _logger.LogError("Failed to refresh token. Cannot send email.");
                        throw new InvalidOperationException("Authentication failed: Could not refresh token.");
                    }

                }


                string senderEmail = _googleAuthService.GetSendingUserEmail(); // Lấy email người gửi từ service auth

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderEmail, senderEmail)); // Tên có thể tùy chỉnh
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = body };
                message.Body = bodyBuilder.ToMessageBody();

                _logger.LogInformation("Connecting to smtp.gmail.com using OAuth2...");
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls, cancellationToken);
                    _logger.LogInformation("Connected. Authenticating...");

                    // Xác thực bằng OAuth 2.0 (XOAUTH2)
                    var oauth2 = new SaslMechanismOAuth2(senderEmail, accessToken);
                    await client.AuthenticateAsync(oauth2, cancellationToken);

                    _logger.LogInformation("Authenticated successfully. Sending email to {ToEmail}...", toEmail);
                    await client.SendAsync(message, cancellationToken);
                    _logger.LogInformation("Email sent successfully to {ToEmail}.", toEmail);

                    await client.DisconnectAsync(true, cancellationToken);
                    _logger.LogInformation("Disconnected from SMTP server.");
                }
            }
            catch (AuthenticationException authEx)
            {
                _logger.LogError(authEx, "Authentication failed when sending email via Gmail API to {ToEmail}. Check credentials/token.", toEmail);
                throw;
            }
            catch (ServiceNotAuthenticatedException snAuthEx)
            {
                _logger.LogError(snAuthEx, "Service Not Authenticated when sending email via Gmail API to {ToEmail}. Check credentials/token.", toEmail);
                throw;
            }
            catch (SmtpCommandException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP Command Error sending email to {ToEmail}. Status Code: {StatusCode}, Response: {Response}", toEmail, smtpEx.StatusCode, smtpEx.Message);
                throw;
            }
            catch (SmtpProtocolException smtpPEx)
            {
                _logger.LogError(smtpPEx, "SMTP Protocol Error sending email to {ToEmail}. Response: {Response}", toEmail, smtpPEx.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General Error sending email via Gmail API to {ToEmail}", toEmail);
                throw;
            }
        }
    }
}