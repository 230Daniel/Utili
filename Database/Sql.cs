using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Mozilla;

namespace Database
{
    internal static class Sql
    {
        private static string ConnectionString { get; set; }

        public static void SetCredentials(string server, string database, string username, string password)
        {
            ConnectionString = $"Server={server};Database={database};Uid={username};Pwd={password};";
        }

        public static int RunNonQuery(string commandText, (string, string)[] values = null)
        {
            try
            {
                MySqlCommand command = GetCommand(commandText, values);

                return command.ExecuteNonQuery();
            }
            catch
            {
                return 0;
            }
        }

        public static MySqlCommand GetCommand(string commandText, (string, string)[] values = null)
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            MySqlCommand command = connection.CreateCommand();

            connection.Open();
            command.CommandText = commandText;

            if (values != null)
            {
                foreach ((string, string) value in values)
                {
                    command.Parameters.Add(new MySqlParameter(value.Item1, value.Item2));
                }
            }

            return command;
        }
    }
}
