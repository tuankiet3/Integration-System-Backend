using StackExchange.Redis;
using System.Text.Json;
using Integration_System.Dtos;
using Integration_System.Dtos.NotificationDTO;

namespace Integration_System.Services
{
    public class NotificationSalaryService
    {
        private readonly IDatabase _db;
        private const string Key = "salary:notifications";

        public NotificationSalaryService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        // Phương thức kiểm tra kết nối Redis
        public bool CheckRedisConnection()
        {
            try
            {
                Console.WriteLine("Checking Redis connection..."); // Ghi lại thông báo kiểm tra kết nối
                // Thực hiện một phép thử đơn giản bằng cách kiểm tra trạng thái của Redis
                var pingResponse = _db.Ping();  // Ping Redis để kiểm tra kết nối
                return pingResponse.TotalMilliseconds >= 0; // Kiểm tra nếu kết quả ping hợp lệ
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi nếu không thể kết nối Redis
                Console.WriteLine($"Redis connection failed: {ex.Message}");
                return false;
            }
        }

        public async Task AddNotificationAsync(NotificationSalaryDTO notification)
        {
            // Kiểm tra kết nối Redis trước khi thực hiện thao tác
            if (!CheckRedisConnection())
            {
                throw new InvalidOperationException("Could not connect to Redis.");
            }

            string json = JsonSerializer.Serialize(notification); // Chuyển đối tượng thông báo thành chuỗi JSON
            await _db.ListRightPushAsync(Key, json); // Đưa thông báo vào cuối danh sách Redis
            await _db.KeyExpireAsync(Key, TimeSpan.FromDays(1)); // Đặt thời gian hết CheckAndNotificationAnniversaryhạn cho khóa, tự động xóa sau 1 ngày
        }

        public async Task<List<NotificationSalaryDTO>> GetAllNotificationsAsync()
        {
            // Kiểm tra kết nối Redis trước khi thực hiện thao tác
            if (!CheckRedisConnection())
            {
                throw new InvalidOperationException("Could not connect to Redis.");
            }

            var values = await _db.ListRangeAsync(Key);  // Lấy tất cả các phần tử từ danh sách Redis
            var result = new List<NotificationSalaryDTO>();
            foreach (var val in values)
            {
                var item = JsonSerializer.Deserialize<NotificationSalaryDTO>(val); // Chuyển chuỗi JSON thành đối tượng NotificationSalaryDTO
                if (item != null) result.Add(item);
            }
            return result;
        }
    }
}
