using Integration_System.Model;
using MySql.Data.MySqlClient;
using Microsoft.Data.SqlClient;
using Integration_System.Dtos;
using Microsoft.Data.SqlClient.DataClassification;
using System.Data;
using Integration_System.Dtos.EmployeeDTO;
using MySqlX.XDevAPI.Common;
namespace Integration_System.DAL
{
    public class EmployeeDAL
    {
        public readonly string _mySQlConnectionString;
        public readonly string _SQLServerConnectionString;
        private readonly ILogger<EmployeeDAL> _logger;
        public EmployeeDAL(ILogger<EmployeeDAL> logger, IConfiguration configuration)
        {
            _logger = logger;
            _mySQlConnectionString = configuration.GetConnectionString("MySqlConnection");
            _SQLServerConnectionString = configuration.GetConnectionString("SQLServerConnection");
        }

        public EmployeeModel MapReaderMySQlToEmployeeModel(MySqlDataReader reader)
        {
            return new EmployeeModel
            {
                EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                DepartmentId = reader.IsDBNull(reader.GetOrdinal("DepartmentID"))
                ? 0 : reader.GetInt32(reader.GetOrdinal("DepartmentID")),
                PositionId = reader.IsDBNull(reader.GetOrdinal("PositionID"))
                ? 0 : reader.GetInt32(reader.GetOrdinal("PositionID")),
                Status = reader.IsDBNull(reader.GetOrdinal("Status"))
                ? string.Empty : reader.GetString(reader.GetOrdinal("Status")),
            };
        }

        public EmployeeModel MapReaderSQLServerToEmployeeModel(SqlDataReader reader)
        {
            return new EmployeeModel
            {
                EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                DateofBirth = reader.GetDateTime(reader.GetOrdinal("DateofBirth")),
                Gender = reader.IsDBNull(reader.GetOrdinal("Gender"))
                ? "null" : reader.GetString(reader.GetOrdinal("Gender")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber"))
                ? string.Empty : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                ? string.Empty : reader.GetString(reader.GetOrdinal("Email")),
                HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
                DepartmentId = reader.IsDBNull(reader.GetOrdinal("DepartmentID"))
                ? 0 : reader.GetInt32(reader.GetOrdinal("DepartmentID")),
                PositionId = reader.IsDBNull(reader.GetOrdinal("PositionID"))
                ? 0 : reader.GetInt32(reader.GetOrdinal("PositionID")),
                Status = reader.IsDBNull(reader.GetOrdinal("Status"))
                ? string.Empty : reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt")
            };
        }
        public EmployeeGetDTO MapReaderSQLServerToGetEmployeeModel(SqlDataReader reader)
        {
            return new EmployeeGetDTO
            {
               
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateofBirth")),
                Gender =reader.GetString(reader.GetOrdinal("Gender")),
                PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentID")),
                PositionId = reader.GetInt32(reader.GetOrdinal("PositionID")),
                Status =reader.GetString(reader.GetOrdinal("Status")),
            };
        }

        public async Task<List<EmployeeModel>> GetAllEmployeesAsync()
        {
            List<EmployeeModel> employees = new List<EmployeeModel>();
            using var connectionSQLServer = new SqlConnection(_SQLServerConnectionString);
            SqlDataReader readerSQLServer = null;
            try
            {
                await connectionSQLServer.OpenAsync();
                Console.WriteLine("✅ Kết nối MySQL thành công!");
                string query = "SELECT * FROM Employees";
                SqlCommand command = new SqlCommand(query, connectionSQLServer);
                readerSQLServer = (SqlDataReader)await command.ExecuteReaderAsync();
                while (await readerSQLServer.ReadAsync())
                {
                    EmployeeModel employee = MapReaderSQLServerToEmployeeModel(readerSQLServer);
                    employees.Add(employee);
                }
                _logger.LogInformation("Successfully retrieved all employees.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("✅ Kết nsssssối MySQL thànsng!");
                _logger.LogError(ex, "Error while getting all employees");
                throw;
            }
            finally
            {
                if (readerSQLServer != null && readerSQLServer.IsClosed)
                {
                    await readerSQLServer.CloseAsync();
                }
                await connectionSQLServer.CloseAsync();
            }
            return employees;
        }

        public async Task<bool> InsertEmployeeAsync(EmployeeInsertDTO employeeDTO)
        {
            using var connectionMySQL = new MySqlConnection(_mySQlConnectionString);
            using var connectionSQLServer = new SqlConnection(_SQLServerConnectionString);
            try
            {

                // Insert into SQL Server
                await connectionSQLServer.OpenAsync();
                Console.WriteLine("✅ Kết nối SQl Server thành công!");
                string querySQLServer = @"INSERT INTO Employees (FullName, DateofBirth, Gender, PhoneNumber, Email, HireDate, DepartmentID, PositionID, Status, CreatedAt, UpdatedAt) VALUES (@FullName, @DateofBirth, @Gender, @PhoneNumber, @Email, @HireDate, @DepartmentId, @PositionId, @Status, @CreatedAt, @UpdatedAt)";
                SqlCommand commandSQLServer = new SqlCommand(querySQLServer, connectionSQLServer);
                commandSQLServer.Parameters.AddWithValue("@FullName", employeeDTO.FullName);
                commandSQLServer.Parameters.AddWithValue("@DateofBirth", employeeDTO.DateofBirth);
                commandSQLServer.Parameters.AddWithValue("@Gender", employeeDTO.Gender);
                commandSQLServer.Parameters.AddWithValue("@PhoneNumber", employeeDTO.PhoneNumber);
                commandSQLServer.Parameters.AddWithValue("@Email", employeeDTO.Email);
                commandSQLServer.Parameters.AddWithValue("@HireDate", employeeDTO.HireDate);
                commandSQLServer.Parameters.AddWithValue("@DepartmentId", employeeDTO.DepartmentId);
                commandSQLServer.Parameters.AddWithValue("@PositionId", employeeDTO.PositionId);
                commandSQLServer.Parameters.AddWithValue("@Status", employeeDTO.Status);
                commandSQLServer.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                commandSQLServer.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                // Insert into MySQL
                await connectionMySQL.OpenAsync();
                Console.WriteLine("✅ Kết nối MySQL thành công!");
                string queryMySQL = @"INSERT INTO employees (EmployeeID, FullName, DepartmentID, PositionID, Status) VALUES (@EmployeeID, @FullName, @DepartmentId, @PositionId, @Status)";
                MySqlCommand commandMySQL = new MySqlCommand(queryMySQL, connectionMySQL);
                int maxID = await maxEmployeeID() + 1;
                commandMySQL.Parameters.AddWithValue("@EmployeeID",maxID);
                commandMySQL.Parameters.AddWithValue("@FullName", employeeDTO.FullName);
                commandMySQL.Parameters.AddWithValue("@DepartmentId", employeeDTO.DepartmentId);
                commandMySQL.Parameters.AddWithValue("@PositionId", employeeDTO.PositionId);
                commandMySQL.Parameters.AddWithValue("@Status", employeeDTO.Status);

                int rowsAffectedSQLServer = await commandSQLServer.ExecuteNonQueryAsync();
                int rowsAffectedMySQL = await commandMySQL.ExecuteNonQueryAsync();
                // Check if the insert was successful in both databases
                if (rowsAffectedMySQL > 0 && rowsAffectedSQLServer > 0)
                {
                    _logger.LogInformation("Successfully inserted employee into.");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to insert employee.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while inserting employee into MySQL database or SQL Server database");
                return false;
            }
            finally
            {
                await connectionSQLServer.CloseAsync();
                await connectionMySQL.CloseAsync();

            }
        }

        public async Task<bool> DeleteEmployeeAsync(int EmployeeId)
        {
            using var connectionMySQL = new MySqlConnection(_mySQlConnectionString);
            using var connectionSQLServer = new SqlConnection(_SQLServerConnectionString);
            try
            {
                // Delete from MySQL
                await connectionMySQL.OpenAsync();
                string queryMySQL = @"DELETE FROM employees WHERE EmployeeID = @EmployeeId";
                MySqlCommand commandMySQL = new MySqlCommand(queryMySQL, connectionMySQL);
                commandMySQL.Parameters.AddWithValue("@EmployeeId", EmployeeId);
                int rowsAffectedMySQL = await commandMySQL.ExecuteNonQueryAsync();
                // Delete from SQL Server
                await connectionSQLServer.OpenAsync();
                string querySQLServer = "DELETE FROM Employees WHERE EmployeeID = @EmployeeId; " +
                                         "IF NOT EXISTS (SELECT 1 FROM Employees) DBCC CHECKIDENT ('Employees', RESEED, 0);";
                SqlCommand commandSQLServer = new SqlCommand(querySQLServer, connectionSQLServer);
                commandSQLServer.Parameters.AddWithValue("@EmployeeId", EmployeeId);
                int rowsAffectedSQLServer = await commandSQLServer.ExecuteNonQueryAsync();
                // Check if the delete was successful in both databases
                if (rowsAffectedMySQL > 0 && rowsAffectedSQLServer > 0)
                {
                    _logger.LogInformation("Successfully deleted employee.");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to delete employee.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting employee from MySQL database or SQL Server database");
                return false;
            }
            finally
            {
                await connectionSQLServer.CloseAsync();
                await connectionMySQL.CloseAsync();
            }
        }

        public async Task<bool> UpdateEmployee(int EmployeeId, EmployeeUpdateDTO employeeDTO)
        {
            using var connectionMySQL = new MySqlConnection(_mySQlConnectionString);
            using var connectionSQLServer = new SqlConnection(_SQLServerConnectionString);
            Console.WriteLine(employeeDTO);
            Console.WriteLine(EmployeeId);
            try
            {
                await connectionSQLServer.OpenAsync();
                string querySQLServer = @"UPDATE Employees SET FullName = @FullName, DateofBirth = @DateofBirth, Gender = @Gender, PhoneNumber = @PhoneNumber, Email = @Email, HireDate = @HireDate, DepartmentID = @DepartmentId, PositionID = @PositionId, Status = @Status,  UpdatedAt = @UpdatedAt WHERE EmployeeID = @EmployeeId";
                SqlCommand commandSQLServer = new SqlCommand(querySQLServer, connectionSQLServer);
                commandSQLServer.Parameters.AddWithValue("@FullName", employeeDTO.FullName);
                commandSQLServer.Parameters.AddWithValue("@DateofBirth", employeeDTO.DateofBirth);
                commandSQLServer.Parameters.AddWithValue("@Gender", employeeDTO.Gender);
                commandSQLServer.Parameters.AddWithValue("@PhoneNumber", employeeDTO.PhoneNumber);
                commandSQLServer.Parameters.AddWithValue("@Email", employeeDTO.Email);
                commandSQLServer.Parameters.AddWithValue("@HireDate", employeeDTO.HireDate);
                commandSQLServer.Parameters.AddWithValue("@DepartmentId", employeeDTO.DepartmentId);
                commandSQLServer.Parameters.AddWithValue("@PositionId", employeeDTO.PositionId);
                commandSQLServer.Parameters.AddWithValue("@Status", employeeDTO.Status);
                commandSQLServer.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                commandSQLServer.Parameters.AddWithValue("@EmployeeId", EmployeeId);
                int affectedRowsSQLServer = await commandSQLServer.ExecuteNonQueryAsync();
            

                await connectionMySQL.OpenAsync();
                string queryMySQL = @"UPDATE employees SET FullName = @FullName, DepartmentID= @DepartmentId, PositionID = @PositionId, Status = @Status WHERE EmployeeId = @EmployeeId";
                MySqlCommand commandMySQL = new MySqlCommand(queryMySQL, connectionMySQL);
                commandMySQL.Parameters.AddWithValue("@FullName", employeeDTO.FullName);
                commandMySQL.Parameters.AddWithValue("@DepartmentId", employeeDTO.DepartmentId);
                commandMySQL.Parameters.AddWithValue("@PositionId", employeeDTO.PositionId);
                commandMySQL.Parameters.AddWithValue("@Status", employeeDTO.Status);
                commandMySQL.Parameters.AddWithValue("@EmployeeId", EmployeeId);
                int affectedRowsMySQL = await commandMySQL.ExecuteNonQueryAsync();
                // Check if the update was successful in both databases
                if (affectedRowsMySQL > 0 && affectedRowsSQLServer > 0)
                {
                    _logger.LogInformation("Successfully updated employee.");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to update employee.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating employee");
                throw;
            }
            finally
            {
                await connectionMySQL.CloseAsync();
            }
        }
       public async Task<EmployeeGetDTO> GetEmployeeIdAsync(int EmployeeId)
        {
            EmployeeGetDTO employee = new EmployeeGetDTO();
            using var connectionSQLServer = new SqlConnection(_SQLServerConnectionString);
            SqlDataReader readerSQLServer = null;
            try
            {
                await connectionSQLServer.OpenAsync();
                string query = "SELECT * FROM Employees WHERE EmployeeID= @EmployeeId";
                SqlCommand command = new SqlCommand(query, connectionSQLServer);
                command.Parameters.AddWithValue("@EmployeeId", EmployeeId);
                readerSQLServer = (SqlDataReader)await command.ExecuteReaderAsync();
                if (await readerSQLServer.ReadAsync())
                {
                    employee = MapReaderSQLServerToGetEmployeeModel(readerSQLServer);
                }
                else
                {
                    return null;
                }
                    _logger.LogInformation("Successfully retrieved employee.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting employee");
                throw;
            }
            finally
            {
                if (readerSQLServer != null && readerSQLServer.IsClosed)
                {
                    await readerSQLServer.CloseAsync();
                }
                await connectionSQLServer.CloseAsync();
            }
            return employee;
        }
        public async Task<int> maxEmployeeID()
        {
            using var connectionMySQL = new MySqlConnection(_mySQlConnectionString);
            await connectionMySQL.OpenAsync();
            string query = "SELECT MAX(EmployeeID) FROM employees;";
            MySqlCommand command = new MySqlCommand(query, connectionMySQL);
            object result = await command.ExecuteScalarAsync(); // Trả về object vì kết quả có thể là int, null, decimal, v.v...
            int max = result != DBNull.Value ? Convert.ToInt32(result) : 0;
            return max;
        }
    }
}
