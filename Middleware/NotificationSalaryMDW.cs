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
        public NotificationSalaryMDW(SalaryDAL salaryDAL, ILogger<NotificationSalaryMDW> logger, NotificationSalaryService redisService, EmployeeDAL employeeDAL)
        {
            _salaryDAL = salaryDAL;
            _logger = logger;
            _redisService = redisService;
            _employeeDAL = employeeDAL;
        }

        public async Task<bool> CheckAndNotificationAnniversary()
        {
            List<EmployeeModel> listEmployees = await _employeeDAL.GetAllEmployeesAsync();
            foreach (var employee in listEmployees)
            {
                if ((DateTime.UtcNow - employee.HireDate).TotalDays >= 365 && (DateTime.UtcNow - employee.HireDate).TotalDays < 366)
                {
                    string message = $"Today is the anniversary of employee {employee.EmployeeId}";
                    await _redisService.AddNotificationAsync(new NotificationSalaryDTO
                    {
                        EmployeeId = employee.EmployeeId,
                        Message = message,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            if (listEmployees.Count == 0)
            {
                _logger.LogWarning("No employees found for anniversary notification");
                return false;
            }
            return true;
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
