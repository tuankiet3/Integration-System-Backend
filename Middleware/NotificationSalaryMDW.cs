using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Integration_System.DAL;
using Integration_System.Model;
using Integration_System.Services;
using Microsoft.Extensions.Logging;
using Integration_System.Dtos.SalaryDTO;
using Integration_System.Dtos.NotificationDTO;
namespace Integration_System.Middleware
{
    public class NotificationSalaryMDW
    {
        private readonly SalaryDAL _salaryDAL;
        private readonly ILogger<NotificationSalaryMDW> _logger;
        private readonly NotificationSalaryService _redisService;
        private readonly EmployeeDAL _employeeDAL;
        private AttendanceDAL _attendanceDAL;

        public NotificationSalaryMDW(SalaryDAL salaryDAL, ILogger<NotificationSalaryMDW> logger, NotificationSalaryService redisService, EmployeeDAL employeeDAL, AttendanceDAL attendanceDAL)
        {
            _salaryDAL = salaryDAL;
            _logger = logger;
            _redisService = redisService;
            _employeeDAL = employeeDAL;
            _attendanceDAL = attendanceDAL;
        }

        public async Task<bool> CheckAndNotificationAnniversary()
        {
            List<EmployeeModel> listEmployees = await _employeeDAL.GetAllEmployeesAsync();
            bool notificationsTriggered = false; // Biến theo dõi xem có thông báo nào được kích hoạt không

            foreach (var employee in listEmployees)
            {
                // Kiểm tra nếu số ngày làm việc >= 365 (khoảng 1 năm)
                // và < 366 (đảm bảo chỉ chạy trong 1 ngày kỷ niệm)
                if ((DateTime.UtcNow - employee.HireDate).TotalDays >= 365 && (DateTime.UtcNow - employee.HireDate).TotalDays < 366)
                {
                    string message = $"Chúc mừng kỷ niệm 1 năm làm việc tại công ty, nhân viên {employee.FullName} (ID: {employee.EmployeeId})!";
                    _logger.LogInformation("Kích hoạt thông báo kỷ niệm 1 năm cho nhân viên {EmployeeId}", employee.EmployeeId);

                    await _redisService.AddNotificationAsync(new NotificationSalaryDTO
                    {
                        EmployeeId = employee.EmployeeId,
                        Message = message,
                        CreatedAt = DateTime.UtcNow
                    });
                    notificationsTriggered = true; // Đánh dấu đã kích hoạt ít nhất một thông báo
                }
            }
            if (!notificationsTriggered)
            {
                _logger.LogInformation("Không có nhân viên nào đạt kỷ niệm 1 năm trong hôm nay.");
            }

            return notificationsTriggered; // Trả về true nếu có thông báo được kích hoạt, ngược lại trả về false
        }

        public async Task<bool> CheckAndNotifyAbsentDays(int employeeId, int month) 
        {
            _logger.LogInformation("Checking absent days for EmployeeId {EmployeeId} for month {Month}", employeeId, month);
            try
            {
                int absentDays = await _attendanceDAL.GetAbsentDayAsync(employeeId, month);

                _logger.LogInformation("EmployeeId {EmployeeId} has {AbsentDays} absent days for month {Month}", employeeId, absentDays, month);

                if (absentDays > 3)
                {
                    string message = $"Employee {employeeId} as rested for more than 3 days in the month {month}. Total number of holidays:{absentDays}.";
                    _logger.LogInformation("Notification to be created: {Message}", message);

                    await _redisService.AddNotificationAsync(new NotificationSalaryDTO
                    {
                        EmployeeId = employeeId,
                        Message = message,
                        CreatedAt = DateTime.UtcNow
                    });
                    _logger.LogInformation("Absent days notification successfully added for EmployeeId {EmployeeId}, month {Month}.", employeeId, month);
                    return true; 
                    }
                    _logger.LogInformation("EmployeeId {EmployeeId} has {AbsentDays} absent days for month {Month}", employeeId, absentDays, month);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("EmployeeId {EmployeeId} has {AbsentDays} absent days for month {Month}", "Error checking and notifying absent days for EmployeeId {EmployeeId}, month {Month}.", employeeId, month);
                return false;
            }
        }

        public async Task<bool> CheckAndNotificationLeave(bool check, int EmployeeId)
        {
            if (check)
            {
                string message = $"Employee with {EmployeeId} has deleted";
                await _redisService.AddNotificationAsync(new NotificationSalaryDTO
                {
                    EmployeeId = EmployeeId,
                    Message = message,
                    CreatedAt = DateTime.UtcNow
                });
                return true;
            }
            return false;
        }

        public async Task<bool> CheckAndNotifySalary(SalaryInsertDTO salary)
        {
            SalaryModel salaryModel = await _salaryDAL.getLatestEmployeeID(salary.EmployeeId);
            int absenDay = await _attendanceDAL.GetAbsentDayAsync(salary.EmployeeId, salary.SalaryMonth.Month);
            decimal deductions = (salary.BaseSalary * 0.10m) + (200 * absenDay);
            decimal netSalary = salary.BaseSalary + (salary.Bonus ?? 0) - (deductions);

            if (salaryModel == null)
            {
                _logger.LogWarning($"No salary found for EmployeeId {salary.EmployeeId}");
                return false;
            }
            else if (Math.Abs(netSalary - salaryModel.NetSalary) > 5000)
            {
                string message = $"Net salary for Employee {salary.EmployeeId} has an unsual gap";
                await _redisService.AddNotificationAsync(new NotificationSalaryDTO
                {
                    EmployeeId = salary.EmployeeId,
                    Message = message,
                    CreatedAt = DateTime.UtcNow
                });
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
