using Integration_System.Model;
using Integration_System.Dtos;
using MySql.Data.MySqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.DataClassification;
using System.Data;
using Integration_System.Dtos.SalaryDTO;
using System.Configuration;
namespace Integration_System.DAL
{
    public class SalaryDAL
    {
        private readonly string _mySQlConnectionString;
        private readonly ILogger<SalaryDAL> _logger;
        public SalaryDAL(IConfiguration configuration, ILogger<SalaryDAL> logger)
        {
            _logger = logger;
            _mySQlConnectionString = configuration.GetConnectionString("MySqlConnection");
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
            MySqlDataReader readerMySQL = null;
            try
            {
                await connectionMySQL.OpenAsync();
                string query = "SELECT SalaryID, EmployeeID, SalaryMonth, BaseSalary, Bonus, Deductions, NetSalary FROM salaries";
                MySqlCommand command = new MySqlCommand(query, connectionMySQL);
                readerMySQL = (MySqlDataReader)await command.ExecuteReaderAsync();
                while (await readerMySQL.ReadAsync())
                {
                    SalaryModel salary = MapReaderMySQlToSalaryModel(readerMySQL);
                    salaries.Add(salary);
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
                command.Parameters.AddWithValue("@Deductions", salary.Deductions);
                decimal NetSalary = salary.BaseSalary + (salary.Bonus ?? 0) - (salary.Deductions ?? 0);
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
        public async Task<SalaryModel> getHistorySalary(int employeeID, int month)
        {
            using var connectionMySQl = new MySqlConnection(_mySQlConnectionString);
            MySqlDataReader readerMySQL = null;
            SalaryModel salary = new SalaryModel();
            try
            {
                await connectionMySQl.OpenAsync();
                string query = @"SELECT * FROM salaries WHERE EmployeeID = @EmployeeID AND MONTH(SalaryMonth) = @SalaryMonth";
                MySqlCommand command = new MySqlCommand(query, connectionMySQl);
                command.Parameters.AddWithValue("@EmployeeID", employeeID);
                command.Parameters.AddWithValue("@SalaryMonth", month);
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
                await connectionMySQl.CloseAsync();
            }
        }
        //public async Task<bool> DeleteSalary(int SalaryID)
        //{
        //    using var connectionMySQL = new MySqlConnection(_mySQlConnectionString);
        //    MySqlDataReader readerMySQL = null;
        //    try
        //    {
        //        await connectionMySQL.OpenAsync();
        //        string query = @"DELETE FROM salaries WHERE SalaryID = @SalaryID ";
        //        MySqlCommand command = new MySqlCommand(query, connectionMySQL);
        //        command.Parameters.AddWithValue("@SalaryID", SalaryID);
        //        int rowsAffected = await command.ExecuteNonQueryAsync();
        //        if (rowsAffected > 0)
        //        {
        //            string checkEmpty = "SELECT COUNT(*) FROM salaries";
        //            MySqlCommand commandCount = new MySqlCommand(checkEmpty, connectionMySQL);
        //            long count = (long)await commandCount.ExecuteScalarAsync();
        //            if (count == 0)
        //            {
        //                string resetQuery = "ALTER TABLE salaries AUTO_INCREMENT = 1;";
        //                using var resetCommand = new MySqlCommand(resetQuery, connectionMySQL);
        //                await resetCommand.ExecuteNonQueryAsync();
        //            }
        //            _logger.LogInformation($"Successfully deleted salary with ID {SalaryID}.");
        //            return true;

        //        }
        //        else
        //        {
        //            _logger.LogWarning($"No salary found with ID {SalaryID}.");
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error deleting salary");
        //        return false;
        //    }
        //    finally
        //    {
        //        if (readerMySQL != null)
        //        {
        //            await readerMySQL.CloseAsync();
        //        }
        //        await connectionMySQL.CloseAsync();
        //    }
        //}
        public async Task<SalaryModel> getLatestEmployeeID(int EmployeeID)
        {
            using var mySqlConnection = new MySqlConnection(_mySQlConnectionString);
            MySqlDataReader readerMySQL = null;
            SalaryModel salary = new SalaryModel();
            try
            {
                await mySqlConnection.OpenAsync();
                string query = @"SELECT TOP 1 * FROM salaries WHERE EmployeeID = @EmployeeID ORDER BY SalaryMonth DESC";
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
    }
}
