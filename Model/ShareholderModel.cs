using System.ComponentModel.DataAnnotations;

namespace Integration_System.Model
{
    public class ShareholderModel
    {
        [Required]
        private int shareholderId;
        public int ShareholderId { get { return shareholderId; } set { shareholderId = value; } }
        ///////////////////////////
        [Required]
        private string fullName;
        public string FullName { get { return fullName; } set { fullName = value; } }
        ///////////////////////////
        [Required]
        private string email;
        public string Email { get { return email; } set { email = value; } }
        ///////////////////////////
        private int phone;
        public int Phone { get { return phone; } set { phone = value; } }
        ///////////////////////////
        private string address;
        public string Address { get { return address; } set { address = value; } }
        ///////////////////////////
        private bool isEmployee;
        public bool IsEmployee { get { return isEmployee; } set { isEmployee = value; } }

    }
}
