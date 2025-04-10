using Integration_System.Model;
using MySql.Data.MySqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.DataClassification;
using System.Data;
namespace Integration_System.DAL
{
    public class PositionDAL
    {
        public readonly string _mySQlConnectionString;
        public readonly string _SQLServerConnectionString;
        private readonly ILogger<EmployeeDAL> _logger;

        public PositionDAL(IConfiguration configuration, ILogger<EmployeeDAL> logger)
        {
            _logger = logger;
            _mySQlConnectionString = configuration.GetConnectionString("MySqlConnection");
            _SQLServerConnectionString = configuration.GetConnectionString("SQLServerConnection");
        }

        public PositionModel MapReaderMySQlToPositionModel(MySqlDataReader reader)
        {
            return new PositionModel
            {
                PositionId = reader.GetInt32("PositionID"),
                PositionName = reader.GetString("PositionName"),
            };
        }
        public PositionModel MapReaderSQLServerToPositionModel(SqlDataReader reader)
        {
            return new PositionModel
            {
                PositionId = reader.GetInt32("PositionID"),
                PositionName = reader.GetString("PositionName"),
            };
        }
        public async Task<List<PositionModel>> getPositions()
        {
            List<PositionModel> positions = new List<PositionModel>();
            using var connectionMySQL = new MySqlConnection(_mySQlConnectionString);
            MySqlDataReader readerMySQL = null;
            try
            {
                await connectionMySQL.OpenAsync();
                string query = "SELECT PositionID, PositionName FROM positions";
                MySqlCommand command = new MySqlCommand(query, connectionMySQL);
                readerMySQL = (MySqlDataReader)await command.ExecuteReaderAsync();
                while (await readerMySQL.ReadAsync())
                {
                    PositionModel position = MapReaderMySQlToPositionModel(readerMySQL);
                    positions.Add(position);
                }
                _logger.LogInformation("Successfully retrieved all positions.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving positions");
            }
            finally
            {
                if (readerMySQL != null)
                {
                    await readerMySQL.CloseAsync();
                }
                await connectionMySQL.CloseAsync();
            }
            return positions;
        }

        public async Task<PositionModel> GetPositionByID(int PositionID)
        {
            using var connectionSQlServer = new SqlConnection(_SQLServerConnectionString);
            SqlDataReader readerSQLServer = null;
            PositionModel position = new PositionModel();
            try
            {
               
                await connectionSQlServer.OpenAsync();
                string query = @"SELECT * FROM Positions  WHERE PositionID = @PositionID";
                SqlCommand command = new SqlCommand(query, connectionSQlServer);
                command.Parameters.AddWithValue("@PositionID", PositionID);
                readerSQLServer = (SqlDataReader)await command.ExecuteReaderAsync();
                if (await readerSQLServer.ReadAsync())
                {
                    position = MapReaderSQLServerToPositionModel(readerSQLServer);
                }
                else
                {
                    return null;
                }
                    _logger.LogInformation("Successfully retrieved employee.");
        

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving position by ID");
            }
            finally
            {
                if (readerSQLServer != null)
                {
                    await readerSQLServer.CloseAsync();
                }
                await connectionSQlServer.CloseAsync();
            }
            return position;
        }
    }
}
