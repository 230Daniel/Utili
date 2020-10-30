﻿using System;
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

        public static string GetBool(bool boolean)
        {
            if (boolean)
            {
                return "TRUE";
            }

            return "FALSE";
        }

        public static string ConvertToSqlTime(DateTime time)
        {
            return $"{time.Year:0000}-{time.Month:00}-{time.Day:00} {time.Hour:00}:{time.Minute:00}:{time.Second:00}";
        }
    }
}
