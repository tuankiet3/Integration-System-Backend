using Integration_System.Model;
using Integration_System.Dtos;
using MySql.Data.MySqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.DataClassification;
using System.Data;
using Integration_System.Dtos.SalaryDTO;
using System.Configuration;
using System.Collections.Generic;
using Integration_System.Dtos.ReportDto;
using System.Text;
namespace Integration_System.DAL
{
    public class SalaryDAL
    {
        private readonly string _mySQlConnectionString;
        private readonly ILogger<SalaryDAL> _logger;
        private readonly string _sqlServerConnectionString;
        private readonly AttendanceDAL _AttendanceDAL;
        public SalaryDAL(IConfiguration configuration, ILogger<SalaryDAL> logger, AttendanceDAL attendanceDAL)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mySQlConnectionString = configuration?.GetConnectionString("MySqlConnection")
                                     ?? throw new ArgumentNullException(nameof(configuration), "MySqlConnection string is null.");
            _sqlServerConnectionString = configuration?.GetConnectionString("SqlServerConnection")
                             ?? throw new ArgumentNullException(nameof(configuration), "SqlServerConnection string is null.");
            _AttendanceDAL = attendanceDAL;
        }

        public SalaryModel MapReaderMySQlToSalaryModel(MySqlDataReader reader)
        {
            return new SalaryModel
            {
                SalaryId = reader.GetInt32("SalaryID"),
                EmployeeId = reader.GetInt32("EmployeeID"),
                SalaryMonth = reader.GetDateTime("SalaryMonth"),
                BaseSalary = reader.GetDecimal("BaseSalary"),
                Bonus = reader.GetDecimal("Bonus"),
                Deductions = reader.GetDecimal("Deductions"),
                NetSalary = reader.GetDecimal("NetSalary"),
            };
        }

        public async Task<List<SalaryModel>> getSalaries()
        {
            List<SalaryModel> salaries = new List<SalaryModel>();
            using var connectionMySQL = new MySqlConnection(_mySQlConnectionString);
            MySqlDataReader? readerMySQL = null;
            try
            {
                await connectionMySQL.OpenAsync();
                string query = "SELECT SalaryID, EmployeeID, SalaryMonth, BaseSalary, Bonus, Deductions, NetSalary FROM salaries";
                MySqlCommand command = new MySqlCommand(query, connectionMySQL);
                readerMySQL = await command.ExecuteReaderAsync() as MySqlDataReader;
                if (readerMySQL != null)
                {
                    while (await readerMySQL.ReadAsync())
                    {
                        SalaryModel salary = MapReaderMySQlToSalaryModel(readerMySQL);
                        salaries.Add(salary);
                    }
                }

                _logger.LogInformation("Successfully retrieved all salaries.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salaries");
            }
            finally
            {
                if (readerMySQL != null)
                {
                    await readerMySQL.CloseAsync();
                }
                await connectionMySQL.CloseAsync();
            }
            return salaries;
        }
        public async Task<bool> InserSalary(SalaryInsertDTO salary)
        {
            int absenDay = await _AttendanceDAL.GetAbsentDayAsync(salary.EmployeeId, salary.SalaryMonth.Month);
            decimal deductions = (salary.BaseSalary * 0.10m) + (200*absenDay);
            Console.WriteLine($"absenDayádsadasd: {absenDay}");
            using var connectionMySQL = new MySqlConnection(_mySQlConnectionString);
            try
            {
                await connectionMySQL.OpenAsync();
                string query = @"INSERT INTO salaries (EmployeeID, SalaryMonth, BaseSalary, Bonus, Deductions, NetSalary, CreatedAt) VALUES (@EmployeeID, @SalaryMonth, @BaseSalary, @Bonus, @Deductions, @NetSalary, @CreatedAt)";
                MySqlCommand command = new MySqlCommand(query, connectionMySQL);
                command.Parameters.AddWithValue("@EmployeeID", salary.EmployeeId);
                command.Parameters.AddWithValue("@SalaryMonth", salary.SalaryMonth);
                command.Parameters.AddWithValue("@BaseSalary", salary.BaseSalary);
                command.Parameters.AddWithValue("@Bonus", salary.Bonus);
                command.Parameters.AddWithValue("@Deductions", deductions);
                decimal NetSalary = salary.BaseSalary + (salary.Bonus ?? 0) - (deductions);
                command.Parameters.AddWithValue("@NetSalary", NetSalary);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                { 
                    _logger.LogInformation($"Successfully inserted salary.");
                    return true;
                }
                else
                {
       
                    _logger.LogWarning($"Failed to insert salary.");
                    return false;
                }
            }
            catch (Exception ex)
            {
            
                _logger.LogError(ex, "Error inserting salary");
                return false;
            }
            finally
            {
                await connectionMySQL.CloseAsync();
            }
        }
        public async Task<List<SalaryModel>> GetSalariesByEmployeeIDAsync(int employeeID)
        {
            using var connectionMySql = new MySqlConnection(_mySQlConnectionString);
            var salaries = new List<SalaryModel>();
            MySqlDataReader? readerMySQL = null;

            try
            {
                await connectionMySql.OpenAsync();
                string query = @"SELECT * FROM salaries WHERE EmployeeID = @EmployeeID";
                MySqlCommand command = new MySqlCommand(query, connectionMySql);
                command.Parameters.AddWithValue("@EmployeeID", employeeID);

                readerMySQL = (MySqlDataReader)await command.ExecuteReaderAsync() ;

                if (readerMySQL != null)
                {
                    while (await readerMySQL.ReadAsync())
                    {
                        var salary = MapReaderMySQlToSalaryModel(readerMySQL);
                        salaries.Add(salary);
                    }
                }

                if (salaries.Count == 0)
                {
                    _logger.LogWarning("No salary records found for employee ID {EmployeeID}.", employeeID);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved {Count} salary record(s) for employee ID {EmployeeID}.", salaries.Count, employeeID);
                }

                return salaries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salary records for employee ID {EmployeeID}.", employeeID);
                return new List<SalaryModel>();
            }
            finally
            {
                if (readerMySQL != null)
                {
                    await readerMySQL.CloseAsync();
                }
                await connectionMySql.CloseAsync();
            }
        }
        public async Task<SalaryModel?> getHistorySalary(int employeeID, int month)
        {
            using var connectionMySQl = new MySqlConnection(_mySQlConnectionString);
            MySqlDataReader? readerMySQL = null;
            SalaryModel? salary = null;
            try
            {
                await connectionMySQl.OpenAsync();
                string query = @"SELECT * FROM salaries WHERE EmployeeID = @EmployeeID AND MONTH(SalaryMonth) = @SalaryMonth";
                MySqlCommand command = new MySqlCommand(query, connectionMySQl);
                command.Parameters.AddWithValue("@EmployeeID", employeeID);
                command.Parameters.AddWithValue("@SalaryMonth", month);
                readerMySQL = await command.ExecuteReaderAsync() as MySqlDataReader;
                if (readerMySQL != null && await readerMySQL.ReadAsync())
                {
                    salary = MapReaderMySQlToSalaryModel(readerMySQL);
                }
                else
                {
                    _logger.LogWarning("Not found historical salary of employee.");
                }
                _logger.LogInformation("Successfully retrieved historical salary of employee.");
                return salary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee");
                return null;
            }
            finally
            {
                if (readerMySQL != null)
                {
                    await readerMySQL.CloseAsync();
                }
                await connectionMySQl.CloseAsync();
            }
        }
        // method of report
        public async Task<TotalBudgetDto> GetTotalSalaryBudgetAsync(int? month)
        {
            decimal totalBudget = 0;
            using var connection = new MySqlConnection(_mySQlConnectionString);
            StringBuilder queryBuilder = new StringBuilder("SELECT SUM(NetSalary) FROM salaries");
            List<string> conditions = new List<string>();
            MySqlCommand command = new MySqlCommand();
            if (month.HasValue)
            {
                conditions.Add("MONTH(SalaryMonth) = @Month");
                command.Parameters.AddWithValue("@Month", month.Value);
            }

            if (conditions.Count > 0)
            {
                queryBuilder.Append(" WHERE ").Append(string.Join(" AND ", conditions));
            }
            command.CommandText = queryBuilder.ToString();
            command.Connection = connection;

            try
            {
                await connection.OpenAsync();
                object? result = await command.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    totalBudget = Convert.ToDecimal(result);
                }
                _logger.LogInformation("Calculated total salary budget for {Month}: {TotalBudget}", month?.ToString() ?? "All", totalBudget);
                return new TotalBudgetDto { TotalNetSalary = totalBudget, Month = month };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total salary budget for {Month}", month?.ToString() ?? "All");
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open) await connection.CloseAsync();
            }
        }

        public async Task<List<AvgSalaryByDeptDto>> GetAverageSalaryByDeptAsync(int? month)
        {
            var avgSalaries = new List<AvgSalaryByDeptDto>();
            using var connectionMySQL = new MySqlConnection(_mySQlConnectionString);
            using var connectionSQLServer = new SqlConnection(_sqlServerConnectionString);

            Dictionary<int, decimal> avgSalaryData = new Dictionary<int, decimal>();
            StringBuilder queryMySQLBuilder = new StringBuilder(@"
                SELECT e.DepartmentID, AVG(s.NetSalary) as AverageSalary
                FROM salaries s
                JOIN employees e ON s.EmployeeID = e.EmployeeID ");

            List<string> conditions = new List<string>();
            MySqlCommand commandMySQL = new MySqlCommand();
            if (month.HasValue) { conditions.Add("MONTH(s.SalaryMonth) = @Month"); commandMySQL.Parameters.AddWithValue("@Month", month.Value); }
            if (conditions.Count > 0) { queryMySQLBuilder.Append(" WHERE ").Append(string.Join(" AND ", conditions)); }
            queryMySQLBuilder.Append(" GROUP BY e.DepartmentID");
            commandMySQL.CommandText = queryMySQLBuilder.ToString();
            commandMySQL.Connection = connectionMySQL;

            try
            {
                await connectionMySQL.OpenAsync();
                MySqlDataReader readerMySQL = (MySqlDataReader)await commandMySQL.ExecuteReaderAsync();
                while (await readerMySQL.ReadAsync())
                {
                    int deptId = readerMySQL.IsDBNull(readerMySQL.GetOrdinal("DepartmentID")) ? 0 : readerMySQL.GetInt32(readerMySQL.GetOrdinal("DepartmentID"));
                    decimal avgSalary = readerMySQL.IsDBNull(readerMySQL.GetOrdinal("AverageSalary")) ? 0 : readerMySQL.GetDecimal(readerMySQL.GetOrdinal("AverageSalary"));
                    if (deptId > 0)
                    {
                        avgSalaryData[deptId] = avgSalary;
                    }
                }
                await readerMySQL.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting average salary data from MySQL for {Month}", month?.ToString() ?? "All");
                throw;
            }
            finally
            {
                if (connectionMySQL.State == ConnectionState.Open) await connectionMySQL.CloseAsync();
            }

            if (!avgSalaryData.Any())
            {
                _logger.LogInformation("No average salary data found for {Month}.", month?.ToString() ?? "All");
                return avgSalaries;
            }

            Dictionary<int, string> departmentNames = new Dictionary<int, string>();
            StringBuilder querySQLServerBuilder = new StringBuilder("SELECT DepartmentID, DepartmentName FROM Departments WHERE DepartmentID IN (");
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            int i = 0;
            foreach (var deptId in avgSalaryData.Keys)
            {
                string paramName = $"@DeptID{i}";
                querySQLServerBuilder.Append(paramName).Append(",");
                sqlParams.Add(new SqlParameter(paramName, deptId));
                i++;
            }
            querySQLServerBuilder.Length--;
            querySQLServerBuilder.Append(")");

            SqlCommand commandSQLServer = new SqlCommand(querySQLServerBuilder.ToString(), connectionSQLServer);
            commandSQLServer.Parameters.AddRange(sqlParams.ToArray());

            try
            {
                await connectionSQLServer.OpenAsync();
                SqlDataReader readerSQLServer = await commandSQLServer.ExecuteReaderAsync();
                while (await readerSQLServer.ReadAsync())
                {
                    departmentNames[readerSQLServer.GetInt32(0)] = readerSQLServer.GetString(1);
                }
                await readerSQLServer.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching department names from SQL Server.");
            }
            finally
            {
                if (connectionSQLServer.State == ConnectionState.Open) await connectionSQLServer.CloseAsync();
            }

            foreach (var kvp in avgSalaryData)
            {
                avgSalaries.Add(new AvgSalaryByDeptDto
                {
                    DepartmentId = kvp.Key,
                    DepartmentName = departmentNames.GetValueOrDefault(kvp.Key, $"Unknown Dept ({kvp.Key})"),
                    AverageNetSalary = kvp.Value,
                    Month = month
                });
            }

            _logger.LogInformation("No average salary data found for {Month}.", month?.ToString() ?? "All");
            return avgSalaries.OrderBy(s => s.DepartmentName).ToList();
        }
        public async Task<SalaryModel> getLatestEmployeeID(int EmployeeID)
        {
            using var mySqlConnection = new MySqlConnection(_mySQlConnectionString);
            MySqlDataReader readerMySQL = null;
            SalaryModel salary = new SalaryModel();
            try
            {
                await mySqlConnection.OpenAsync();
                string query = @"SELECT * FROM salaries  WHERE EmployeeID = @EmployeeID ORDER BY CreatedAt DESC LIMIT 1";
                MySqlCommand command = new MySqlCommand(query, mySqlConnection);
                command.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                readerMySQL = (MySqlDataReader)await command.ExecuteReaderAsync();
                if (await readerMySQL.ReadAsync())
                {
                    salary = MapReaderMySQlToSalaryModel(readerMySQL);
                }
                else
                {
                    _logger.LogWarning("not found historical salary of employee.");
                    return null; // No salary found with the given ID
                }
                _logger.LogInformation("Successfully retrieved historical salary of employee.");
                return salary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee");
                return null;
            }
            finally
            {
                if (readerMySQL != null)
                {
                    await readerMySQL.CloseAsync();
                }
                await mySqlConnection.CloseAsync();
            }
        }

        //internal async Task<IEnumerable<object>> GetSalariesAsync(object value, int month)
        //{
        //    throw new NotImplementedException();
        //}

        // Check Insert
        public async Task<bool> checkInsertSalary(int employeeID, DateTime month)
        {
            using var connectionMySQL = new MySqlConnection(_mySQlConnectionString);
            MySqlDataReader? readerMySQL = null;
            try
            {
                await connectionMySQL.OpenAsync();
                string query = @"SELECT 1 FROM salaries 
                         WHERE EmployeeID = @EmployeeID 
                         AND MONTH(SalaryMonth) = @Month 
                         AND YEAR(SalaryMonth) = @Year
                         LIMIT 1;";
                MySqlCommand command = new MySqlCommand(query, connectionMySQL);
                command.Parameters.AddWithValue("@EmployeeID", employeeID);
                command.Parameters.AddWithValue("@Month", month.Month);
                command.Parameters.AddWithValue("@Year", month.Year);
                readerMySQL = (MySqlDataReader)await command.ExecuteReaderAsync();
                if (readerMySQL.HasRows)
                {
                    _logger.LogWarning("Salary record already exists for employee ID {EmployeeID} for month {Month}.", employeeID, month);
                    return true; // Salary record already exists
                }
                else
                {
                    _logger.LogInformation("No salary record found for employee ID {EmployeeID} for month {Month}.", employeeID, month);
                    return false; // No salary record found
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking salary");
                return false;
            }
            finally
            {
                if (readerMySQL != null)
                {
                    await readerMySQL.CloseAsync();
                }
                await connectionMySQL.CloseAsync();
            }
        }
    }
}
