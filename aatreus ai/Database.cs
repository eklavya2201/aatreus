using Microsoft.Data.Sqlite;
using System;

namespace SalesCopilot
{
    public class Database
    {
        private const string Db = "calls.db";

        public Database()
        {
            using var c = new SqliteConnection($"Data Source={Db}");
            c.Open();
            var cmd = c.CreateCommand();
            cmd.CommandText =
                "CREATE TABLE IF NOT EXISTS Logs(Time TEXT, Text TEXT)";
            cmd.ExecuteNonQuery();
        }

        public void Save(string text)
        {
            using var c = new SqliteConnection($"Data Source={Db}");
            c.Open();
            var cmd = c.CreateCommand();
            cmd.CommandText =
                "INSERT INTO Logs VALUES($t,$x)";
            cmd.Parameters.AddWithValue("$t", DateTime.Now.ToString());
            cmd.Parameters.AddWithValue("$x", text);
            cmd.ExecuteNonQuery();
        }
    }
}
