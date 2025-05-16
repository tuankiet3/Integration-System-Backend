using Integration_System.Model;
using MySql.Data.MySqlClient; // Import thư viện MySQL
// Bỏ using System.Data.Common; nếu không dùng đến DbDataReader nữa
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Integration_System.Dtos.AttendanceDto;
using Mysqlx.Prepare;


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

        public async Task<int> GetAbsentDayAsync(int employeeID, int month)
        {
            using var connection = new MySqlConnection(_mysqlConnectionString);
            MySqlDataReader? reader = null;
            int absentDays = 0;
            try
            {
                await connection.OpenAsync();
                string query = @"SELECT AbsentDays 
                                 FROM attendance 
                                 WHERE EmployeeID = @EmployeeID 
                                   AND MONTH(AttendanceMonth) = @Month";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", employeeID);
                command.Parameters.AddWithValue("@Month", month);
                reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    absentDays = reader.GetInt32(reader.GetOrdinal("AbsentDays"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when taking the absent day.");
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    await reader.DisposeAsync();
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
                                   AND MONTH(AttendanceMonth) = @Month";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", employeeID);
                command.Parameters.AddWithValue("@Month", month);
                reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    leaveDays = reader.GetInt32(reader.GetOrdinal("LeaveDays"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when taking the leave day.");
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    await reader.DisposeAsync();
                }
            }
            return leaveDays;
        }
        public async Task<IEnumerable<AttendanceModel>> GetAttendancesByEmployeeIdAsync(int employeeID)
        {
            var attendanceList = new List<AttendanceModel>();
            using var connection = new MySqlConnection(_mysqlConnectionString);
            MySqlDataReader? reader = null;
            try
            {
                await connection.OpenAsync();
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
            }
            return attendanceList;
        }
        public async Task<IEnumerable<AttendanceModel>> GetAttendancesAsync()
        {
            var attendanceList = new List<AttendanceModel>();
            // use using statement to ensure connection is closed
            using var connection = new MySqlConnection(_mysqlConnectionString);
            MySqlDataReader? reader = null;
            try
            {
                // open connection
                await connection.OpenAsync();
                string query = "SELECT AttendanceID, EmployeeID, WorkDays, AbsentDays, LeaveDays, AttendanceMonth, CreatedAt FROM attendance ORDER BY AttendanceMonth DESC, EmployeeID ASC";
                
                using var command = new MySqlCommand(query, connection);
                // ExecuteReaderAsync is used to execute the command and return a MySqlDataReader
                reader = (MySqlDataReader)await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    // use MapReaderToAttendanceModel to map the data from the reader to the AttendanceModel
                    attendanceList.Add(MapReaderToAttendanceModel(reader));
                }
                _logger.LogInformation("Success {Count} attendance records.", attendanceList.Count);
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
                string query = "SELECT AttendanceID, EmployeeID, WorkDays, AbsentDays, LeaveDays, AttendanceMonth, CreatedAt FROM attendance WHERE AttendanceID = @AttendanceID";
                using var command = new MySqlCommand(query, connection);
                // add parameter to prevent SQL injection
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
            }
            return attendance;
        }   
        private AttendanceModel MapReaderToAttendanceModel(MySqlDataReader reader)
        {
            int attendanceMonthValue = 0;
            try
            {
                // getOrdinal is used to get the index of the column
                DateTime attendanceMonthDate = reader.GetDateTime(reader.GetOrdinal("AttendanceMonth"));
                attendanceMonthValue = attendanceMonthDate.Month;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when reading or converting Attendancemonth.");
            }

            return new AttendanceModel
            {
                AttendanceId = reader.GetInt32(reader.GetOrdinal("AttendanceID")),
                EmployeeId = reader.IsDBNull(reader.GetOrdinal("EmployeeID")) ? 0 : reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                WorkDays = reader.GetInt32(reader.GetOrdinal("WorkDays")),
                AbsentDays = reader.IsDBNull(reader.GetOrdinal("AbsentDays")) ? 0 : reader.GetInt32(reader.GetOrdinal("AbsentDays")),
                LeaveDays = reader.IsDBNull(reader.GetOrdinal("LeaveDays")) ? 0 : reader.GetInt32(reader.GetOrdinal("LeaveDays")),
                AttendanceMonth = attendanceMonthValue
            };
        }
    }
}