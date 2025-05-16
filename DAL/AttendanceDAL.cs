using Integration_System.Model;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data; // Cần using System.Data
using System.Threading.Tasks;
using Integration_System.Dtos.AttendanceDto;
// Bỏ Mysqlx.Prepare nếu không dùng đến
// using Mysqlx.Prepare;


namespace Integration_System.DAL
{
    public class AttendanceDAL
    {
        private readonly string _mysqlConnectionString;
        private readonly ILogger<AttendanceDAL> _logger;


        public AttendanceDAL(IConfiguration configuration, ILogger<AttendanceDAL> logger)
        {
            _mysqlConnectionString = configuration.GetConnectionString("MySqlConnection") ?? throw new InvalidOperationException("Connection string 'MySqlConnection' not found");
            _logger = logger;
        }

        // Các phương thức GetAbsentDayAsync, GetLeaveDayAsync vẫn giữ nguyên
        // vì chúng nhận tháng dưới dạng int và truy vấn database dựa trên số tháng

        public async Task<int> GetAbsentDayAsync(int employeeID, int month)
        {
            using var connection = new MySqlConnection(_mysqlConnectionString);
            MySqlDataReader? reader = null;
            int absentDays = 0;
            try
            {
                await connection.OpenAsync();
                // Truy vấn database dựa trên tháng (int) và năm hiện tại (hoặc năm cụ thể nếu cần)
                // Giả định bạn muốn lấy số ngày nghỉ của tháng và năm cụ thể
                // Nếu cột AttendanceMonth trong DB chỉ lưu tháng, bạn cần logic khác.
                // Nếu cột AttendanceMonth lưu full date (như giả định trước), truy vấn này hoạt động
                string query = @"SELECT AbsentDays
                                 FROM attendance
                                 WHERE EmployeeID = @EmployeeID
                                   AND MONTH(AttendanceMonth) = @Month
                                   AND YEAR(AttendanceMonth) = @Year"; // Thêm điều kiện năm
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", employeeID);
                command.Parameters.AddWithValue("@Month", month);
                command.Parameters.AddWithValue("@Year", DateTime.Now.Year); // Lấy năm hiện tại. Cân nhắc thêm tham số năm nếu cần lấy theo năm khác
                reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    absentDays = reader.GetInt32(reader.GetOrdinal("AbsentDays"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when taking the absent day for employee {EmployeeID} in month {Month}.", employeeID, month);
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    await reader.DisposeAsync();
                }
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
            return absentDays;
        }

        public async Task<int> GetLeaveDayAsync(int employeeID, int month)
        {
            using var connection = new MySqlConnection(_mysqlConnectionString);
            MySqlDataReader? reader = null;
            int leaveDays = 0;
            try
            {
                await connection.OpenAsync();
                string query = @"SELECT LeaveDays
                                 FROM attendance
                                 WHERE EmployeeID = @EmployeeID
                                   AND MONTH(AttendanceMonth) = @Month
                                   AND YEAR(AttendanceMonth) = @Year"; // Thêm điều kiện năm
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", employeeID);
                command.Parameters.AddWithValue("@Month", month);
                command.Parameters.AddWithValue("@Year", DateTime.Now.Year); // Lấy năm hiện tại. Cân nhắc thêm tham số năm nếu cần lấy theo năm khác
                reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    leaveDays = reader.GetInt32(reader.GetOrdinal("LeaveDays"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when taking the leave day for employee {EmployeeID} in month {Month}.", employeeID, month);
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    await reader.DisposeAsync();
                }
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
            return leaveDays;
        }


        public async Task<IEnumerable<AttendanceModel>> GetAttendancesAsync()
        {
            var attendanceList = new List<AttendanceModel>();
            using var connection = new MySqlConnection(_mysqlConnectionString);
            MySqlDataReader? reader = null;
            try
            {
                await connection.OpenAsync();
                // Đảm bảo query lấy cột AttendanceMonth
                string query = "SELECT AttendanceID, EmployeeID, WorkDays, AbsentDays, LeaveDays, AttendanceMonth, CreatedAt FROM attendance ORDER BY AttendanceMonth DESC, EmployeeID ASC";

                using var command = new MySqlCommand(query, connection);
                reader = (MySqlDataReader)await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    attendanceList.Add(MapReaderToAttendanceModel(reader));
                }
                _logger.LogInformation("Successfully retrieved {Count} attendance records.", attendanceList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when taking the timekeeping list.");
                throw;
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    await reader.DisposeAsync();
                }
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
            return attendanceList;
        }

        public async Task<AttendanceModel?> GetAttendanceByAttendanceIdAsync(int attendanceId)
        {
            AttendanceModel? attendance = null;
            using var connection = new MySqlConnection(_mysqlConnectionString);
            MySqlDataReader? reader = null;
            try
            {
                await connection.OpenAsync();
                // Đảm bảo query lấy cột AttendanceMonth
                string query = "SELECT AttendanceID, EmployeeID, WorkDays, AbsentDays, LeaveDays, AttendanceMonth, CreatedAt FROM attendance WHERE AttendanceID = @AttendanceID";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@AttendanceID", attendanceId);
                reader = (MySqlDataReader)await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    attendance = MapReaderToAttendanceModel(reader);
                }
                if (attendance != null)
                {
                    _logger.LogInformation("Take information ID: {AttendanceID} Success.", attendanceId);
                }
                else
                {
                    _logger.LogWarning("The record not found with ID: {AttendanceID}", attendanceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when taking information timekeeping ID: {AttendanceID}.", attendanceId);
                throw;
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    await reader.DisposeAsync();
                }
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
            return attendance;
        }

        // Phương thức mới để lấy attendance theo EmployeeID
        public async Task<IEnumerable<AttendanceModel>> GetAttendancesByEmployeeIdAsync(int employeeID)
        {
            var attendanceList = new List<AttendanceModel>();
            using var connection = new MySqlConnection(_mysqlConnectionString);
            MySqlDataReader? reader = null;
            try
            {
                await connection.OpenAsync();
                // Đảm bảo query lấy cột AttendanceMonth
                string query = "SELECT AttendanceID, EmployeeID, WorkDays, AbsentDays, LeaveDays, AttendanceMonth, CreatedAt FROM attendance WHERE EmployeeID = @EmployeeID ORDER BY AttendanceMonth DESC, AttendanceID ASC";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", employeeID);

                reader = (MySqlDataReader)await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    attendanceList.Add(MapReaderToAttendanceModel(reader));
                }
                _logger.LogInformation("Successfully retrieved {Count} attendance records for Employee ID {EmployeeID}.", attendanceList.Count, employeeID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when taking the timekeeping list for Employee ID {EmployeeID}.", employeeID);
                throw;
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    await reader.DisposeAsync();
                }
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
            return attendanceList;
        }


        private AttendanceModel MapReaderToAttendanceModel(MySqlDataReader reader)
        {
            // Lấy giá trị DateTime trực tiếp từ cột AttendanceMonth
            DateTime attendanceMonthValue = reader.GetDateTime(reader.GetOrdinal("AttendanceMonth"));

            return new AttendanceModel
            {
                AttendanceId = reader.GetInt32(reader.GetOrdinal("AttendanceID")),
                EmployeeId = reader.IsDBNull(reader.GetOrdinal("EmployeeID")) ? 0 : reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                WorkDays = reader.GetInt32(reader.GetOrdinal("WorkDays")),
                AbsentDays = reader.IsDBNull(reader.GetOrdinal("AbsentDays")) ? 0 : reader.GetInt32(reader.GetOrdinal("AbsentDays")),
                LeaveDays = reader.IsDBNull(reader.GetOrdinal("LeaveDays")) ? 0 : reader.GetInt32(reader.GetOrdinal("LeaveDays")),
                AttendanceMonth = attendanceMonthValue // Gán giá trị DateTime đã đọc
            };
        }
    }
}