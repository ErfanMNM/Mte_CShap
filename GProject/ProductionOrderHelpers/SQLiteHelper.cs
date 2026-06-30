using System.Data;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Helper methods cho SQLite operations
    /// </summary>
    public static class SQLiteHelper
    {
        /// <summary>
        /// Execute query và fill DataTable
        /// </summary>
        public static DataTable ExecuteQuery(string connectionString, string sql, params Microsoft.Data.Sqlite.SqliteParameter[] parameters)
        {
            var table = new DataTable();
            using var con = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }
            using var rd = cmd.ExecuteReader();
            table.Load(rd);
            return table;
        }

        /// <summary>
        /// Execute non-query command
        /// </summary>
        public static int ExecuteNonQuery(string connectionString, string sql, params Microsoft.Data.Sqlite.SqliteParameter[] parameters)
        {
            using var con = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Execute scalar command
        /// </summary>
        public static object? ExecuteScalar(string connectionString, string sql, params Microsoft.Data.Sqlite.SqliteParameter[] parameters)
        {
            using var con = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }
            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// Execute reader command
        /// </summary>
        public static Microsoft.Data.Sqlite.SqliteDataReader ExecuteReader(string connectionString, string sql, params Microsoft.Data.Sqlite.SqliteParameter[] parameters)
        {
            var con = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }
            return cmd.ExecuteReader();
        }
    }
}
