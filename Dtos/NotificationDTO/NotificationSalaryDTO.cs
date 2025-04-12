namespace Integration_System.Dtos.NotificationDTO
{
    public class NotificationSalaryDTO
    {
        public int EmployeeId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
