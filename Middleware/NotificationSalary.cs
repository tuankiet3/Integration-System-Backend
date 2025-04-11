using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Integration_System.DAL;
using Integration_System.Model;
using Microsoft.Extensions.Logging;
using Integration_System.Dtos.SalaryDTO;
namespace Integration_System.Middleware
{
    public class NotificationSalary
    {
        private readonly SalaryDAL _salaryDAL;
        private readonly ILogger<NotificationSalary> _logger;
        public NotificationSalary(SalaryDAL salaryDAL, ILogger<NotificationSalary> logger)
        {
            _salaryDAL = salaryDAL;
            _logger = logger;
        }
        public async Task<bool> checkSalary(SalaryInsertDTO salary)
        {
            SalaryModel salaryModel = await _salaryDAL.getLatestEmployeeID(salary.EmployeeId);
            decimal netSalary = salary.BaseSalary + (salary.Bonus ?? 0) - (salary.Deductions ?? 0);
            if (salaryModel == null)
            {
                _logger.LogWarning($"No salary found for EmployeeId {salary.EmployeeId}");
                return false;
            }
            else if(Math.Abs(netSalary - salaryModel.NetSalary) > 2000){
                return true;
          
            } else
            {
                return false;
            }
        }
    }
}
