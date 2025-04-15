using Google.Apis.Auth.OAuth2;

namespace Integration_System.Services
{
    public interface IGoogleAuthService
    {
        Task<UserCredential> GetUserCredentialAsync(CancellationToken cancellationToken = default);
        string GetSendingUserEmail();
    }
}
