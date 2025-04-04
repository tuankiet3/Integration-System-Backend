using Integration_System.Model;
using MySql.Data.MySqlClient;
namespace Integration_System.DAL
{
    public class ApplicantDAL
    {
        private readonly string _connectionString;

        public ApplicantDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }
        //IConfiguration được sử dụng để đọc dữ liệu cấu hình từ file appsettings.json hoặc các nguồn khác (biến môi trường, dòng lệnh, v.v.).

        public List<ApplicantModel> GetAttendances()
        {
            List<ApplicantModel> applicants = new List<ApplicantModel>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Applicants", conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        applicants.Add(new ApplicantModel
                        {
                            ApplicantId = reader.GetInt32("ApplicantId"),
                            FirstName = reader.GetString("FirstName"),
                            LastName = reader.GetString("LastName"),
                            Email = reader.GetString("Email"),
                            Phone = reader.GetString("Phone"),
                            ApplicationDate = reader.GetDateTime("ApplicationDate"),
                            Status = reader.GetString("Status"),
                            JobId = reader.GetInt32("JobId")

                        });
                    }
                }
            }
            return applicants;
        }
        public bool AddApplicant(ApplicantModel applicant)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO Applicants (FirstName, LastName, Email, Phone, ApplicationDate, Status, JobId) VALUES (@FirstName, @LastName, @Email, @Phone, @ApplicationDate, @Status, @JobId)", conn);
                cmd.Parameters.AddWithValue("@FirstName", applicant.FirstName);
                cmd.Parameters.AddWithValue("@LastName", applicant.LastName);
                cmd.Parameters.AddWithValue("@Email", applicant.Email);
                cmd.Parameters.AddWithValue("@Phone", applicant.Phone);
                cmd.Parameters.AddWithValue("@ApplicationDate", applicant.ApplicationDate);
                cmd.Parameters.AddWithValue("@Status", applicant.Status);
                cmd.Parameters.AddWithValue("@JobId", applicant.JobId);
                int result = cmd.ExecuteNonQuery();
                return result > 0; // Trả về true nếu có ít nhất một dòng được thêm vào
            }
        }

        public bool DeleteApplicant(int applicantId)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("DELETE FROM Applicants WHERE ApplicantId = @ApplicantId", conn);
                cmd.Parameters.AddWithValue("@ApplicantId", applicantId);
                int result = cmd.ExecuteNonQuery();
                return result > 0; // Trả về true nếu có ít nhất một dòng được xóa
            }
        }
        public bool UpdateApplicant(ApplicantModel applicant)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("UPDATE Applicants SET FirstName = @FirstName, LastName = @LastName, Email = @Email, Phone = @Phone, ApplicationDate = @ApplicationDate, Status = @Status, JobId = @JobId WHERE ApplicantId = @ApplicantId", conn);
                cmd.Parameters.AddWithValue("@FirstName", applicant.FirstName);
                cmd.Parameters.AddWithValue("@LastName", applicant.LastName);
                cmd.Parameters.AddWithValue("@Email", applicant.Email);
                cmd.Parameters.AddWithValue("@Phone", applicant.Phone);
                cmd.Parameters.AddWithValue("@ApplicationDate", applicant.ApplicationDate);
                cmd.Parameters.AddWithValue("@Status", applicant.Status);
                cmd.Parameters.AddWithValue("@JobId", applicant.JobId);
                cmd.Parameters.AddWithValue("@ApplicantId", applicant.ApplicantId);
                int result = cmd.ExecuteNonQuery();
                return result > 0; // Trả về true nếu có ít nhất một dòng được cập nhật
            }
        }
    }
}
