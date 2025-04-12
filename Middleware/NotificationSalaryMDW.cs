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
        public NotificationSalaryMDW(SalaryDAL salaryDAL, ILogger<NotificationSalaryMDW> logger, NotificationSalaryService redisService)
        {
            _salaryDAL = salaryDAL;
            _logger = logger;
            _redisService = redisService;
        }
        public async Task<bool> CheckAndNotifySalary(SalaryInsertDTO salary)
        {
            SalaryModel salaryModel = await _salaryDAL.getLatestEmployeeID(salary.EmployeeId);
            decimal netSalary = salary.BaseSalary + (salary.Bonus ?? 0) - (salary.Deductions ?? 0);

            if (salaryModel == null)
            {
                _logger.LogWarning($"No salary found for EmployeeId {salary.EmployeeId}");
                return false;
            }
            else if (Math.Abs(netSalary - salaryModel.NetSalary) > 2000)
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
