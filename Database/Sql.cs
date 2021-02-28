using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database
{
    public static class Sql
    {
        private static string ConnectionString { get; set; }

        public static int Queries;

        public static void SetCredentials(string server, int port, string database, string username, string password)
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
            {
                Server = server,
                Port = (uint) port,
                Database = database,
                UserID = username,
                Password = password,
                MaximumPoolSize = 500
            };
            ConnectionString = builder.ConnectionString;
        }

        public static async Task<int> ExecuteAsync(string command, params (string, object)[] parameters)
        {
            Interlocked.Increment(ref Queries);
            MySqlParameter[] commandParameters = parameters.Select(x => new MySqlParameter(x.Item1, x.Item2)).ToArray();
            commandParameters = PrepareParameters(commandParameters);
            return await MySqlHelper.ExecuteNonQueryAsync(ConnectionString, command, commandParameters);
        }

        public static async Task<MySqlDataReader> ExecuteReaderAsync(string command, params (string, object)[] parameters)
        {
            Interlocked.Increment(ref Queries);
            MySqlParameter[] commandParameters = parameters.Select(x => new MySqlParameter(x.Item1, x.Item2)).ToArray();
            commandParameters = PrepareParameters(commandParameters);
            return await MySqlHelper.ExecuteReaderAsync(ConnectionString, command, commandParameters);
        }

        private static MySqlParameter[] PrepareParameters(IEnumerable<MySqlParameter> parameters)
        {
            return parameters.Select(parameter =>
            {
                object value = parameter.Value;
                if(value is null) return new MySqlParameter(parameter.ParameterName, null);

                Type type = value.GetType();
                object preparedValue = value.ToString();

                if (type == typeof(DateTime))
                {
                    DateTime time = (DateTime) value;
                    preparedValue = $"{time.Year:0000}-{time.Month:00}-{time.Day:00} {time.Hour:00}:{time.Minute:00}:{time.Second:00}";
                }
                if (value is bool boolean)
                {
                    preparedValue = boolean;
                }
                if (type == typeof(EString))
                {
                    throw new ArgumentException(
                        "Can not store EString in database. Use either EString.EncodedValue or EString.EncryptedValue.");
                }

                return new MySqlParameter(parameter.ParameterName, preparedValue);
            }).ToArray();
        }

        public static async Task<int> PingAsync()
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            connection.Ping();
            sw.Stop();

            await connection.CloseAsync();
            return (int) sw.ElapsedMilliseconds;
        }

        public static string ToSqlObjectArray(ulong[] values)
        {
            string sqlArray = "(";
            for (int i = 0; i < values.Length; i++)
            {
                sqlArray += $"{values[i]}";
                if (i != values.Length - 1) sqlArray += ",";
            }
            sqlArray += ")";

            return sqlArray;
        }
    }
}
