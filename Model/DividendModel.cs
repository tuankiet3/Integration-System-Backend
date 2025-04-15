using System.ComponentModel.DataAnnotations;

namespace Integration_System.Model
{
    public class DividendModel
    {
        [Required]
        private int dividendId;
        public int DividendId { get { return dividendId; } set { dividendId = value; } }
        ///////////////////////////
        private int employeeId;
        public int EmployeeId { get { return employeeId; } set { employeeId = value; } }
        ///////////////////////////
        [Required]
        private decimal dividendAmount;
        public decimal DividendAmount { get { return dividendAmount; } set { dividendAmount = value; } }
        ///////////////////////////
        [Required]
        private DateTime dividendDate;
        public DateTime DividendDate { get { return dividendDate; } set { dividendDate = value; } }
        ///////////////////////////
        private DateTime createdAt;
        public DateTime CreatedAt { get { return createdAt; } set { createdAt = value; } }

    }
}
