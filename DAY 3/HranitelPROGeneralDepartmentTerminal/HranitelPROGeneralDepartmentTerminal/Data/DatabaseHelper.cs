using Npgsql;
using System;
using System.Data;

namespace HranitelPROGeneralDepartmentTerminal.Data
{
    public static class DatabaseHelper
    {
        private static readonly string ConnectionString =
            "Host=127.0.0.1;Username=postgres;Password=1;Database=hranitelpro";

        public static NpgsqlConnection GetConnection() => new NpgsqlConnection(ConnectionString);

        public static object ExecuteScalar(string sql, NpgsqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteScalar();
                }
            }
        }

        public static int ExecuteNonQuery(string sql, NpgsqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataTable ExecuteQuery(string sql, NpgsqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    using (var adapter = new NpgsqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }
    }
}