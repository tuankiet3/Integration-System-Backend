using System.ComponentModel.DataAnnotations;

namespace Integration_System.Model
{
    public class DividendModel
    {
        [Required]
        private int dividendId;
        public int DividendId { get { return dividendId; } set { dividendId = value; } }
        ///////////////////////////
        [Required]
        private int shareholderId;
        public int ShareholderId { get { return shareholderId; } set { shareholderId = value; } }
        ///////////////////////////
        [Required]
        private decimal dividendAmount;
        public decimal DividendAmount { get { return dividendAmount; } set { dividendAmount = value; } }
        ///////////////////////////
        [Required]
        private DateTime paymentDate;
        public DateTime PaymentDate { get { return paymentDate; } set { paymentDate = value; } }

    }
}
