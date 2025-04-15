using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1; // Cần cho Scopes
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO; // Thêm using này
using System.Threading;
using System.Threading.Tasks;
using Integration_System.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Integration_System.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly GoogleAuthSettings _authSettings;
        private readonly ILogger<GoogleAuthService> _logger;
        private UserCredential? _credential; // Cache credential

        public GoogleAuthService(IOptions<GoogleAuthSettings> authSettings, ILogger<GoogleAuthService> logger)
        {
            _authSettings = authSettings.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_authSettings.ClientSecretPath) ||
                string.IsNullOrWhiteSpace(_authSettings.TokenStorePath) ||
                 string.IsNullOrWhiteSpace(_authSettings.SendingUserEmail))
            {
                _logger.LogError("GoogleAuthSettings are not configured properly in appsettings.json.");
                throw new InvalidOperationException("GoogleAuth settings are missing or invalid.");
            }
            _logger.LogInformation($"ClientSecretPath configured: {_authSettings.ClientSecretPath}");
            _logger.LogInformation($"TokenStorePath configured: {_authSettings.TokenStorePath}");
            _logger.LogInformation($"SendingUserEmail configured: {_authSettings.SendingUserEmail}");
        }

        public string GetSendingUserEmail()
        {
            return _authSettings.SendingUserEmail;
        }


        public async Task<UserCredential> GetUserCredentialAsync(CancellationToken cancellationToken = default)
        {
            // Kiểm tra cache credential và làm mới nếu cần (Giữ nguyên logic này)
            if (_credential != null && (!_credential.Token.IsExpired(_credential.Flow.Clock) || _credential.Token.RefreshToken != null))
            {
                if (_credential.Token.IsExpired(_credential.Flow.Clock) && _credential.Token.RefreshToken != null)
                {
                    _logger.LogInformation("Access token expired, attempting refresh...");
                    if (await _credential.RefreshTokenAsync(cancellationToken))
                    {
                        _logger.LogInformation("Access token refreshed successfully.");
                    }
                    else
                    {
                        _logger.LogWarning("Failed to refresh access token. Re-authorization might be required.");
                        // Có thể cân nhắc xóa _credential ở đây để buộc xác thực lại hoàn toàn
                        // _credential = null;
                    }
                }
                // Chỉ trả về credential nếu nó vẫn hợp lệ sau khi thử refresh (nếu có)
                if (_credential != null && (!_credential.Token.IsExpired(_credential.Flow.Clock) || _credential.Token.RefreshToken != null))
                {
                    return _credential;
                }
                else
                {
                    // Nếu refresh thất bại và token vẫn hết hạn, xóa credential để lấy lại từ đầu
                    _logger.LogWarning("Token could not be refreshed and is expired. Clearing cached credential.");
                    _credential = null;
                }
            }


            _logger.LogInformation("Attempting to load client secret from: {path}", _authSettings.ClientSecretPath);
            GoogleClientSecrets clientSecrets;
            try
            {
                using (var stream = new FileStream(_authSettings.ClientSecretPath, FileMode.Open, FileAccess.Read))
                {
                    clientSecrets = await GoogleClientSecrets.FromStreamAsync(stream, cancellationToken);
                }
                _logger.LogInformation("Client secret loaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load client secret file from {path}.", _authSettings.ClientSecretPath);
                throw;
            }

            // ---- PHẦN CHỈNH SỬA ĐỂ XỬ LÝ ĐƯỜNG DẪN ----
            string configuredPath = _authSettings.TokenStorePath;
            string finalTokenFolderPath; // Thư mục cuối cùng để lưu token

            if (Path.IsPathRooted(configuredPath))
            {
                // Nếu đường dẫn đã là tuyệt đối, sử dụng trực tiếp
                finalTokenFolderPath = configuredPath;
                _logger.LogInformation("Using absolute TokenStorePath: {path}", finalTokenFolderPath);
            }
            else
            {
                // Nếu là tương đối, kết hợp với thư mục chạy ứng dụng
                finalTokenFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuredPath);
                _logger.LogInformation("Using relative TokenStorePath, combined path: {path}", finalTokenFolderPath);
            }
            _logger.LogInformation("Final token store folder path: {credPath}", finalTokenFolderPath);
            // ---- KẾT THÚC PHẦN CHỈNH SỬA ĐƯỜNG DẪN ----


            // FileDataStore mong đợi đường dẫn đến THƯ MỤC
            var dataStore = new FileDataStore(finalTokenFolderPath, true);

            try
            {
                _logger.LogInformation("Authorizing user: {user}", _authSettings.SendingUserEmail);
                _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets.Secrets,
                    // ---- SỬA DÒNG NÀY ĐỂ ĐÚNG SCOPE ----
                    new[] { "https://mail.google.com/" },
                    // ---- KẾT THÚC SỬA ----
                    _authSettings.SendingUserEmail,
                    cancellationToken,
                    dataStore); // Sử dụng dataStore đã được tạo với đường dẫn đúng
                _logger.LogInformation("Authorization successful for user: {user}", _authSettings.SendingUserEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authorization failed for user: {user}", _authSettings.SendingUserEmail);
                throw;
            }

            _logger.LogInformation("Credential obtained for user: {UserId}", _credential.UserId);
            return _credential;
        }
    }
}