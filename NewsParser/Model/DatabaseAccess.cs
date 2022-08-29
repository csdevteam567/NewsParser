using Dapper;
using System.Data;
using System.Data.SQLite;

namespace NewsParser.Model
{
    public class DatabaseAccess
    {
        private string connectionString;
        private IDbConnection dbConnection;
        public DatabaseAccess(string _connectionString)
        {
            connectionString = _connectionString;
            dbConnection = new SQLiteConnection(connectionString);
        }

        public List<News> GetNewsList()
        {
            List<News> result = new List<News>();
            string sqlQuery = "SELECT * FROM News";
            result = dbConnection.Query<News>(sqlQuery).ToList();

            return result;
        }

        public void AddNewsRecord(News record)
        {
            string sqlQuery = "INSERT INTO News(Title, Content) VALUES (@Title, @Content)";
            dbConnection.Execute(sqlQuery, record);
        }
    }
}
