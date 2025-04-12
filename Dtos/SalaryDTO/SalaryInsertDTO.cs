using System.ComponentModel.DataAnnotations;

namespace Integration_System.Dtos.SalaryDTO
{
    public class SalaryInsertDTO
    {
        [Required(ErrorMessage = "ID nhân viên là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "ID nhân viên không hợp lệ")]
        public int EmployeeId { get; set; }


        [Required(ErrorMessage = "Tháng là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime SalaryMonth { get; set; }


        [Required(ErrorMessage = "Lương cơ bản là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Lương cơ bản không hợp lệ")]
        public decimal BaseSalary { get; set; } = 0;


        public decimal? Bonus { get; set; } = 0;
        public decimal? Deductions { get; set; } = 0;
        //[Required(ErrorMessage = "Net salary là bắt buộc")]
        //[Range(0, double.MaxValue, ErrorMessage = "Net salary không hợp lệ")]
        //public decimal NetSalary { get; set; }
    }
}
