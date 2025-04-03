using System.ComponentModel.DataAnnotations;

namespace Integration_System.Model
{
    public class ApplicantModel
    {
        [Required]
        private int applicantId;
        public int ApplicantId { get { return applicantId; } set { applicantId = value; } }
        ///////////////////////////
        [Required]
        private string firstName;
        public string FirstName { get { return firstName; } set { firstName = value; } }
        ///////////////////////////
        [Required]
        private string lastName;
        public string LastName { get { return lastName; } set { lastName = value; } }
        ///////////////////////////
        [Required]
        private string email;
        public string Email { get { return email; } set { email = value; } }
        ///////////////////////////
        private string phone;
        public string Phone { get { return phone; } set { phone = value; } }
        ///////////////////////////
        [Required]
        private DateTime applicationDate;
        public DateTime ApplicationDate { get { return applicationDate; } set { applicationDate = value; } }
        ///////////////////////////
        private string status;
        public string Status { get { return status; } set { status = value; } }
        ///////////////////////////
        [Required]
        private int jobId;
        public int JobId { get { return jobId; } set { jobId = value; } }


    }
}
