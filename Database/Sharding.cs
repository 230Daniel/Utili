using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class Sharding
    {
        public static int GetTotalShards()
        {
            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM Sharding WHERE Id = 1;").ExecuteReader();

            reader.Read();

            return reader.GetInt32(1);
        }
    }
}
