using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
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

        public static string ToSqlBool(bool boolean)
        {
            if (boolean)
            {
                return "TRUE";
            }

            return "FALSE";
        }

        public static string ToSqlDateTime(DateTime time)
        {
            return $"{time.Year:0000}-{time.Month:00}-{time.Day:00} {time.Hour:00}:{time.Minute:00}:{time.Second:00}";
        }

        public static string ToSqlObjectArray<T>(T[] values)
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

        public static string EncryptString(string input, ulong guildId, ulong channelId, ulong messageId, ulong userId)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            bytes = Encryption.Encrypt(bytes, Encryption.GeneratePassword(guildId, channelId, messageId, userId));

            return Convert.ToBase64String(bytes);
        }

        public static string DecryptString(string input, ulong guildId, ulong channelId, ulong messageId, ulong userId)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(input);

                bytes = Encryption.Decrypt(bytes, Encryption.GeneratePassword(guildId, channelId, messageId, userId));

                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return input;
            }
        }
    }

    internal class ScrambleKey
    {

    }
}
