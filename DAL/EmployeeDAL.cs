using Integration_System.Model;
using MySql.Data.MySqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.DataClassification;
using System.Data;
using Integration_System.Dtos.EmployeeDTO;
using MySqlX.XDevAPI.Common;
using System.Collections.Generic;
using Integration_System.Dtos.ReportDto;
using Microsoft.AspNetCore.Identity;
using static Integration_System.ENUM;
using Microsoft.AspNetCore.Mvc;
using Integration_System.Constants;
using Integration_System.Services;
namespace Integration_System.DAL
{
    public class EmployeeDAL
    {
        public readonly string _mySQlConnectionString;
        public readonly string _SQLServerConnectionString;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<EmployeeDAL> _logger;
        private readonly IAuthService _authService;
        public EmployeeDAL(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager ,ILogger<EmployeeDAL> logger, IConfiguration configuration, IAuthService authService)
        {
            _logger = logger;
            _mySQlConnectionString = configuration.GetConnectionString("MySqlConnection")
                                     ?? throw new ArgumentNullException(nameof(_mySQlConnectionString), "MySqlConnection string is null.");
            _SQLServerConnectionString = configuration.GetConnectionString("SQLServerConnection")
                                         ?? throw new ArgumentNullException(nameof(_SQLServerConnectionString), "SQLServerConnection string is null.");
            _userManager = userManager;
            _roleManager = roleManager;
            _authService = authService;

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
        public async Task<InsertEmployeeResult> checkInsert(EmployeeInsertDTO employeeDTO)
        {
            // check  Email
            using var connectionSQLServer = new SqlConnection(_SQLServerConnectionString);
            await connectionSQLServer.OpenAsync();
            string checkEmailQuery = @"SELECT COUNT(*) FROM Employees WHERE Email = @Email";
            SqlCommand checkEmailCommand = new SqlCommand(checkEmailQuery, connectionSQLServer);
            checkEmailCommand.Parameters.AddWithValue("@Email", employeeDTO.Email);
            var emailCount = (int)await checkEmailCommand.ExecuteScalarAsync();
            if (emailCount > 0)
                return InsertEmployeeResult.EmailAlreadyExists;

            string checkDepartmentQuery = @"SELECT COUNT(*) FROM Departments WHERE DepartmentID = @DepartmentId";
            SqlCommand checkDepartmentCommand = new SqlCommand(checkDepartmentQuery, connectionSQLServer);
            checkDepartmentCommand.Parameters.AddWithValue("@DepartmentId", employeeDTO.DepartmentId);
            var deptCount = (int)await checkDepartmentCommand.ExecuteScalarAsync();
            if (deptCount == 0)
                return InsertEmployeeResult.InvalidDepartment;

            string checkPositionQuery = @"SELECT COUNT(*) FROM Positions WHERE PositionID = @PositionID";
            SqlCommand checkPositionCommand = new SqlCommand(checkPositionQuery, connectionSQLServer);
            checkPositionCommand.Parameters.AddWithValue("@PositionID", employeeDTO.PositionId);
            var postCount = (int)await checkPositionCommand.ExecuteScalarAsync();
            if (postCount == 0)
                return InsertEmployeeResult.InvalidPosition;

            return InsertEmployeeResult.Success;
        }

        public async Task<bool> InsertEmployeeAsync(EmployeeInsertDTO employeeDTO)
        {
            using var connectionMySQL = new MySqlConnection(_mySQlConnectionString);
            using var connectionSQLServer = new SqlConnection(_SQLServerConnectionString);

            try
            {
                // Insert into SQL Server
                await connectionSQLServer.OpenAsync();
                Console.WriteLine("✅ Kết nối SQL Server thành công!");

                string querySQLServer = @"
                    INSERT INTO Employees 
                    (FullName, DateofBirth, Gender, PhoneNumber, Email, HireDate, DepartmentID, PositionID, Status, CreatedAt, UpdatedAt)
                    VALUES 
                    (@FullName, @DateofBirth, @Gender, @PhoneNumber, @Email, @HireDate, @DepartmentId, @PositionId, @Status, @CreatedAt, @UpdatedAt);
                    SELECT SCOPE_IDENTITY();";

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

                object? insertedIdObj = await commandSQLServer.ExecuteScalarAsync();
                int newEmployeeID = Convert.ToInt32(insertedIdObj);
                Console.WriteLine($"✅ Insert vào SQL Server thành công, ID mới: {newEmployeeID}");

                // Insert into MySQL
                await connectionMySQL.OpenAsync();
                Console.WriteLine("✅ Kết nối MySQL thành công!");

                string queryMySQL = @"INSERT INTO employees (EmployeeID, FullName, DepartmentID, PositionID, Status) 
                                      VALUES (@EmployeeID, @FullName, @DepartmentId, @PositionId, @Status)";
                MySqlCommand commandMySQL = new MySqlCommand(queryMySQL, connectionMySQL);
                commandMySQL.Parameters.AddWithValue("@EmployeeID", newEmployeeID);
                commandMySQL.Parameters.AddWithValue("@FullName", employeeDTO.FullName);
                commandMySQL.Parameters.AddWithValue("@DepartmentId", employeeDTO.DepartmentId);
                commandMySQL.Parameters.AddWithValue("@PositionId", employeeDTO.PositionId);
                commandMySQL.Parameters.AddWithValue("@Status", employeeDTO.Status);

                // create user
                IdentityUser user = new()
                {
                    Id = newEmployeeID.ToString(),
                    Email = employeeDTO.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = employeeDTO.FullName,
                    EmailConfirmed = true, // confirm email by default

                };

                // save user to database
                var result = await _userManager.CreateAsync(user, employeeDTO.PhoneNumber);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    _logger.LogError("User creation failed for {Username}. Errors: {Errors}", employeeDTO.FullName, errors);
                    return false;
                }

                _logger.LogInformation("User {Username} created successfully. Assigning default role.", employeeDTO.FullName);
                // Updated line to handle nullable value type
                await _authService.SetRole(employeeDTO.DepartmentId ?? throw new ArgumentNullException(nameof(employeeDTO.DepartmentId)), employeeDTO.FullName, user);

                int rowsAffectedMySQL = await commandMySQL.ExecuteNonQueryAsync();

                if (rowsAffectedMySQL > 0)
                {
                    _logger.LogInformation("✅ Insert thành công vào cả hai hệ thống.");
                    return true;
                }
                else
                {
                    _logger.LogWarning("❌ Insert vào MySQL thất bại.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi insert nhân viên vào SQL Server hoặc MySQL.");
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
            using var connectionSQLServer = new SqlConnection(_SQLServerConnectionString);
            using var connectionMySQL = new MySqlConnection(_mySQlConnectionString);
            SqlTransaction? sqlTransaction = null;
            string? emailToDelete = null;

            try
            {
                await connectionSQLServer.OpenAsync();
                sqlTransaction = connectionSQLServer.BeginTransaction();
                _logger.LogDebug("SQL Server connection opened and transaction started for DeleteEmployeeAsync (ID: {EmployeeId}).", EmployeeId);

                string getEmailQuery = "SELECT Email FROM Employees WHERE EmployeeID = @EmployeeId";
                using (SqlCommand getEmailCommand = new SqlCommand(getEmailQuery, connectionSQLServer, sqlTransaction)) 
                {
                    getEmailCommand.Parameters.AddWithValue("@EmployeeId", EmployeeId);
                    object? result = await getEmailCommand.ExecuteScalarAsync();
                    emailToDelete = result?.ToString();
                }

                if (string.IsNullOrEmpty(emailToDelete))
                {
                    _logger.LogWarning("Employee with ID {EmployeeId} not found or email is missing in SQL Server. Cannot proceed with deletion.", EmployeeId);
                    await sqlTransaction.RollbackAsync(); 
                    return false;
                }
                _logger.LogInformation("Found email '{Email}' for EmployeeID {EmployeeId}. Proceeding with deletion.", emailToDelete, EmployeeId);

                string querySQLServer = "DELETE FROM Employees WHERE EmployeeID = @EmployeeId;";
                using (SqlCommand commandSQLServer = new SqlCommand(querySQLServer, connectionSQLServer, sqlTransaction)) // Gán transaction
                {
                    commandSQLServer.Parameters.AddWithValue("@EmployeeId", EmployeeId);
                    int rowsAffectedSQLServer = await commandSQLServer.ExecuteNonQueryAsync();
                    if (rowsAffectedSQLServer == 0)
                    {
                        _logger.LogWarning("Employee with ID {EmployeeId} was not found during SQL Server DELETE operation (potentially deleted concurrently).", EmployeeId);
                        await sqlTransaction.RollbackAsync();
                        return false; 
                    }
                    _logger.LogInformation("Successfully deleted record from SQL Server Employees table for ID: {EmployeeId}", EmployeeId);
                }
                await connectionMySQL.OpenAsync();
                _logger.LogDebug("MySQL connection opened for DeleteEmployeeAsync (ID: {EmployeeId}).", EmployeeId);

                string deleteSalariesQuery = "DELETE FROM salaries WHERE EmployeeID = @EmployeeId";
                using (MySqlCommand deleteSalariesCommand = new MySqlCommand(deleteSalariesQuery, connectionMySQL))
                {
                    deleteSalariesCommand.Parameters.AddWithValue("@EmployeeId", EmployeeId);
                    int salaryRowsAffected = await deleteSalariesCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("Deleted {Count} salary records from MySQL for EmployeeID: {EmployeeId}", salaryRowsAffected, EmployeeId);
                }
                string deleteAttendanceQuery = "DELETE FROM attendance WHERE EmployeeID = @EmployeeId";
                using (MySqlCommand deleteAttendanceCommand = new MySqlCommand(deleteAttendanceQuery, connectionMySQL))
                {
                    deleteAttendanceCommand.Parameters.AddWithValue("@EmployeeId", EmployeeId);
                    int attendanceRowsAffected = await deleteAttendanceCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("Deleted {Count} attendance records from MySQL for EmployeeID: {EmployeeId}", attendanceRowsAffected, EmployeeId);
                }
                string queryMySQL = @"DELETE FROM employees WHERE EmployeeID = @EmployeeId";
                using (MySqlCommand commandMySQL = new MySqlCommand(queryMySQL, connectionMySQL))
                {
                    commandMySQL.Parameters.AddWithValue("@EmployeeId", EmployeeId);
                    int rowsAffectedMySQL = await commandMySQL.ExecuteNonQueryAsync();
                    if (rowsAffectedMySQL == 0)
                    {
                       
                        _logger.LogWarning("Employee with ID {EmployeeId} was not found during MySQL DELETE operation.", EmployeeId);
                    }
                    else
                    {
                        _logger.LogInformation("Successfully deleted record from MySQL employees table for ID: {EmployeeId}", EmployeeId);
                    }
                }


               
                bool authUserDeleted = await _authService.DeleteUser(emailToDelete);

                if (!authUserDeleted)
                {
                    
                    _logger.LogError("CRITICAL: Failed to delete Identity user with email {Email} after deleting employee data for ID {EmployeeId}. Manual cleanup required!", emailToDelete, EmployeeId);
                    
                }
                else
                {
                    _logger.LogInformation("Successfully requested deletion of Identity user with email {Email}.", emailToDelete);
                }
                await sqlTransaction.CommitAsync();
                _logger.LogInformation("SQL Server transaction committed for DeleteEmployeeAsync (ID: {EmployeeId}). Employee deletion process completed.", EmployeeId);
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during DeleteEmployeeAsync for EmployeeID: {EmployeeId}", EmployeeId);
                
                if (sqlTransaction != null && connectionSQLServer.State == ConnectionState.Open)
                {
                    try { await sqlTransaction.RollbackAsync(); _logger.LogWarning("Rolled back SQL Server transaction due to exception during delete."); }
                    catch (Exception rollbackEx) { _logger.LogError(rollbackEx, "Failed to rollback SQL Server transaction during delete."); }
                }
                return false; 
            }
            finally
            {
                if (connectionSQLServer.State == ConnectionState.Open)
                {
                    await connectionSQLServer.CloseAsync();
                    _logger.LogDebug("SQL Server connection closed for DeleteEmployeeAsync.");
                }
                if (connectionMySQL.State == ConnectionState.Open)
                {
                    await connectionMySQL.CloseAsync();
                    _logger.LogDebug("MySQL connection closed for DeleteEmployeeAsync.");
                }
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
                if (affectedRowsMySQL > 0 && affectedRowsSQLServer > 0)
                {
                    _logger.LogInformation("Successfully updated employee.");
                    var user = await _userManager.FindByEmailAsync(employeeDTO.Email??throw new Exception());

                    if (user != null)
                    {
                        var result = await _userManager.UpdateAsync(user);
                        if (!result.Succeeded)
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                            _logger.LogError("User update failed for {Username}. Errors: {Errors}", employeeDTO.FullName, errors);
                            return false;
                        }
                        _logger.LogInformation("User {Username} updated successfully. Assigning new role.", employeeDTO.FullName);
                        await _authService.SetRole(employeeDTO.DepartmentId ?? throw new ArgumentNullException(nameof(employeeDTO.DepartmentId)), employeeDTO.FullName, user);
                    }
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
        // method of report

        public async Task<List<DistributionByDeptDto>> GetEmployeeDistributionByDeptAsync()
        {
            var distribution = new List<DistributionByDeptDto>();
            using var connection = new SqlConnection(_SQLServerConnectionString);
            string query = @"
                SELECT ISNULL(d.DepartmentName, 'N/A') as DepartmentName, COUNT(e.EmployeeID) as EmployeeCount
                FROM Employees e
                LEFT JOIN Departments d ON e.DepartmentID = d.DepartmentID
                GROUP BY ISNULL(d.DepartmentName, 'N/A')
                ORDER BY DepartmentName;";
            SqlCommand command = new SqlCommand(query, connection);

            try
            {
                await connection.OpenAsync();
                SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    distribution.Add(new DistributionByDeptDto
                    {
                        DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                        EmployeeCount = reader.GetInt32(reader.GetOrdinal("EmployeeCount"))
                    });
                }
                await reader.CloseAsync();
                _logger.LogInformation("Retrieved employee distribution by department.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee distribution by department.");
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open) await connection.CloseAsync();
            }
            return distribution;
        }

        public async Task<List<StatusDistributionDto>> GetEmployeeDistributionByStatusAsync()
        {
            var distribution = new List<StatusDistributionDto>();
            using var connection = new SqlConnection(_SQLServerConnectionString);
            string query = @"
                SELECT ISNULL(Status, 'N/A') as Status, COUNT(EmployeeID) as EmployeeCount
                FROM Employees
                GROUP BY ISNULL(Status, 'N/A')
                ORDER BY Status;";
            SqlCommand command = new SqlCommand(query, connection);

            try
            {
                await connection.OpenAsync();
                SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    distribution.Add(new StatusDistributionDto
                    {
                        Status = reader.GetString(reader.GetOrdinal("Status")),
                        EmployeeCount = reader.GetInt32(reader.GetOrdinal("EmployeeCount"))
                    });
                }
                await reader.CloseAsync();
                _logger.LogInformation("Retrieved employee distribution by status.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee distribution by status.");
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open) await connection.CloseAsync();
            }
            return distribution;
        }
    }
}
