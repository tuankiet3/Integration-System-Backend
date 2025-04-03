using System.ComponentModel.DataAnnotations;

namespace Integration_System.Model
{
    public class JobModel
    {
        [Required]
        private string jobId;
        public string JobId { get { return jobId; } set { jobId = value; } }
        ///////////////////////////
        [Required]
        private string jobTitle;
        public string JobTitle { get { return jobTitle; } set { jobTitle = value; } }
        ///////////////////////////
        private decimal minSalary;
        public decimal MinSalary { get { return minSalary; } set { minSalary = value; } }
        ///////////////////////////
        private decimal maxSalary;
        public decimal MaxSalary { get { return maxSalary; } set { maxSalary = value; } }
    }
}

//if (string.IsNullOrEmpty(value))
//{
//    throw new ArgumentException("JobId cannot be null or empty.");
//}
//jobId = value;
