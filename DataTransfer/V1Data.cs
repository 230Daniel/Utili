using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Database.Data;
using Discord.Commands;
using MySql.Data.MySqlClient;
using ECCurve = Org.BouncyCastle.Math.EC.ECCurve;

namespace DataTransfer
{
    internal class V1Data
    {
        public static List<V1Data> Cache;

        public static string ConnectionString = "";

        public static void SetConnectionString(string con)
        {
            ConnectionString = con;
        }

        public static int RunNonQuery(string commandText, (string, string)[] values = null)
        {
            try
            {
                using MySqlConnection connection = new MySqlConnection(ConnectionString);
                using MySqlCommand command = connection.CreateCommand();
                connection.Open();
                command.CommandText = commandText;

                if (values != null)
                {
                    foreach ((string, string) value in values)
                    {
                        command.Parameters.Add(new MySqlParameter(value.Item1, value.Item2));
                    }
                }

                return command.ExecuteNonQuery();
            }
            catch
            {
                return 0;
            }
        }

        public static void SaveData(string guildId, string type, string value = "", bool ignoreCache = false, bool cacheOnly = false, string table = "Utili")
        {
            V1Data v1Data = new V1Data(guildId, type, value);

            if (!ignoreCache)
            {
                try { Cache.Add(new V1Data(guildId, type, value)); } catch { }
            }

            if (!cacheOnly) RunNonQuery($"INSERT INTO {table}(GuildID, DataType, DataValue) VALUES(@GuildID, @Type, @Value);", new[] { ("GuildID", guildId), ("Type", type), ("Value", value) });
        }

        public static List<V1Data> GetData(string guildId = null, string type = null, string value = null, bool ignoreCache = false, string table = "Utili")
        {
            V1Data v1Data = new V1Data(guildId, type, value);

            return GetDataList(guildId, type, value, ignoreCache, table);
        }

        public static V1Data GetFirstData(string guildId = null, string type = null, string value = null, bool ignoreCache = false, string table = "Utili")
        {
            try
            {
                if (!ignoreCache)
                {
                    V1Data v1Data = new V1Data(guildId, type, value);

                    if (guildId != null && type != null && value != null) return Cache.First(x => x.GuildId == guildId && x.Type == type && x.Value == value);
                    if (type != null && value != null) return Cache.First(x => x.Type == type && x.Value == value);
                    if (guildId != null && value != null) return Cache.First(x => x.GuildId == guildId && x.Value == value);
                    if (guildId != null && type != null) return Cache.First(x => x.GuildId == guildId && x.Type == type);
                    if (value != null) return Cache.First(x => x.Value == value);
                    if (guildId != null) return Cache.First(x => x.GuildId == guildId);
                    if (type != null) return Cache.First(x => x.Type == type);
                    return Cache.First();
                }

                return GetDataList(guildId, type, value, true, table).First();
            }
            catch
            {
                return null;
            }
        }

        public static bool DataExists(string guildId = null, string type = null, string value = null, bool ignoreCache = false, string table = "Utili")
        {
            if (GetFirstData(guildId, type, value, ignoreCache, table) != null) return true;
            return false;
        }

        public static List<V1Data> GetDataList(string guildId = null, string type = null, string value = null, bool ignoreCache = false, string table = "Utili")
        {
            List<V1Data> data = new List<V1Data>();

            if (!ignoreCache)
            {
                if (guildId != null && type != null && value != null) return Cache.Where(x => x.GuildId == guildId && x.Type == type && x.Value == value).ToList();
                if (type != null && value != null) return Cache.Where(x => x.Type == type && x.Value == value).ToList();
                if (guildId != null && value != null) return Cache.Where(x => x.GuildId == guildId && x.Value == value).ToList();
                if (guildId != null && type != null) return Cache.Where(x => x.GuildId == guildId && x.Type == type).ToList();
                if (value != null) return Cache.Where(x => x.Value == value).ToList();
                if (guildId != null) return Cache.Where(x => x.GuildId == guildId).ToList();
                if (type != null) return Cache.Where(x => x.Type == type).ToList();
                return Cache;
            }

            using MySqlConnection connection = new MySqlConnection(ConnectionString);
            using MySqlCommand command = connection.CreateCommand();
            connection.Open();

            string commandText;
            if (guildId == null & type == null & value == null) commandText = $"SELECT * FROM {table};";
            else
            {
                commandText = $"SELECT * FROM {table} WHERE(";
                if (guildId != null)
                {
                    commandText += "GuildID = @GuildID AND ";
                    command.Parameters.Add(new MySqlParameter("GuildID", guildId));
                }
                if (type != null)
                {
                    commandText += "DataType = @Type AND ";
                    command.Parameters.Add(new MySqlParameter("Type", type));
                }
                if (value != null)
                {
                    commandText += "DataValue = @Value";
                    command.Parameters.Add(new MySqlParameter("Value", value));
                }
                if (commandText.Substring(commandText.Length - 5) == " AND ") commandText = commandText.Substring(0, commandText.Length - 5);
                commandText += ");";
            }

            command.CommandText = commandText;
            MySqlDataReader dataReader = null;
            try
            {
                dataReader = command.ExecuteReader();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            while (dataReader.Read())
            {
                V1Data @new = new V1Data(dataReader.GetString(1), dataReader.GetString(2), dataReader.GetString(3))
                {
                    Id = dataReader.GetInt32(0)
                };
                data.Add(@new);
            }

            return data;
        }

        public static List<V1Data> GetDataWhere(string where)
        {
            List<V1Data> data = new List<V1Data>();

            using MySqlConnection connection = new MySqlConnection(ConnectionString);
            using MySqlCommand command = connection.CreateCommand();
            connection.Open();

            string commandText = $"SELECT * FROM Utili WHERE({@where});";

            command.CommandText = commandText;
            MySqlDataReader dataReader = null;
            try
            {
                dataReader = command.ExecuteReader();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            while (dataReader.Read())
            {
                V1Data @new = new V1Data(dataReader.GetString(1), dataReader.GetString(2), dataReader.GetString(3))
                {
                    Id = dataReader.GetInt32(0)
                };
                data.Add(@new);
            }

            return data;
        }

        public static void DeleteDataWhere(string where, string table)
        {
            RunNonQuery($"DELETE FROM {table} WHERE {where};");
        }

        public static void DeleteData(string guildId = null, string type = null, string value = null, bool ignoreCache = false, bool cacheOnly = false, string table = "Utili")
        {
            if (guildId == null & type == null & value == null) throw new Exception();

            if (!ignoreCache)
            {
                List<V1Data> toDelete = GetDataList(guildId, type, value);
                foreach (V1Data item in toDelete) { Cache.Remove(item); }
            }

            if (!cacheOnly)
            {
                string command = $"DELETE FROM {table} WHERE(";
                if (guildId != null)
                {
                    command += "GuildID = @GuildID AND ";
                }
                if (type != null)
                {
                    command += "DataType = @Type AND ";
                }
                if (value != null)
                {
                    command += "DataValue = @Value";
                }
                if (command.Substring(command.Length - 5) == " AND ") command = command.Substring(0, command.Length - 5);
                command += ");";

                RunNonQuery(command, new[] { ("GuildID", guildId), ("Type", type), ("Value", value) });
            }
        }

        public static async Task SaveMessageAsync(SocketCommandContext context)
        {
            string content = Encrypt(context.Message.Content, context.Guild.Id, context.Channel.Id);

            RunNonQuery("INSERT INTO Utili_MessageLogs(GuildID, ChannelID, MessageID, UserID, Content, Timestamp) VALUES(@GuildID, @ChannelID, @MessageID, @UserID, @Content, @Timestamp);", new[] {
                ("GuildID", context.Guild.Id.ToString()),
                ("ChannelID", context.Channel.Id.ToString()),
                ("MessageID", context.Message.Id.ToString()),
                ("UserID", context.User.Id.ToString()),
                ("Content", content),
                ("Timestamp", ToSqlTime(context.Message.CreatedAt.DateTime)) });
        }

        public static async Task<MessageData> GetMessageAsync(ulong messageId)
        {
            using MySqlConnection connection = new MySqlConnection(ConnectionString);
            using MySqlCommand command = connection.CreateCommand();
            connection.Open();

            string commandText = $"SELECT * FROM Utili_MessageLogs WHERE MessageID = '{messageId}'";

            command.CommandText = commandText;
            MySqlDataReader dataReader = null;
            try
            {
                dataReader = command.ExecuteReader();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            while (dataReader.Read())
            {
                MessageData data = new MessageData
                {
                    Id = dataReader.GetInt32(0),
                    GuildId = dataReader.GetString(1),
                    ChannelId = dataReader.GetString(2),
                    MessageId = dataReader.GetString(3),
                    UserId = dataReader.GetString(4),
                    EncryptedContent = dataReader.GetString(5),
                    Timestmap = dataReader.GetDateTime(6)
                };

                return data;
            }

            return null;
        }

        public static string ToSqlTime(DateTime time)
        {
            return $"{time.Year:0000}-{time.Month:00}-{time.Day:00} {time.Hour:00}:{time.Minute:00}:{time.Second:00}";
        }

        public int Id { get; set; }
        public string GuildId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public V1Data(string guildid, string type, string value)
        {
            GuildId = guildid;
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"{GuildId}, {Type}, {Value}";
        }

        public static string Encrypt(string textData, ulong guildId, ulong channelId)
        {
            string encryptionKey = $"{guildId * channelId}-{channelId * 3 - guildId}";

            RijndaelManaged objrij = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 0x80,
                BlockSize = 0x80
            };
            byte[] passBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] encryptionkeyBytes = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int len = passBytes.Length;
            if (len > encryptionkeyBytes.Length)
            {
                len = encryptionkeyBytes.Length;
            }
            Array.Copy(passBytes, encryptionkeyBytes, len);
            objrij.Key = encryptionkeyBytes;
            objrij.IV = encryptionkeyBytes;
            ICryptoTransform objtransform = objrij.CreateEncryptor();
            byte[] textDataByte = Encoding.UTF8.GetBytes(textData);
            return Convert.ToBase64String(objtransform.TransformFinalBlock(textDataByte, 0, textDataByte.Length));
        }

        public static string Decrypt(string encryptedText, ulong guildId, ulong channelId)
        {
            string encryptionKey = $"{guildId * channelId}-{channelId * 3 - guildId}";

            RijndaelManaged objrij = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 0x80,
                BlockSize = 0x80
            };
            byte[] encryptedTextByte = Convert.FromBase64String(encryptedText);
            byte[] passBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] encryptionkeyBytes = new byte[0x10];
            int len = passBytes.Length;
            if (len > encryptionkeyBytes.Length)
            {
                len = encryptionkeyBytes.Length;
            }
            Array.Copy(passBytes, encryptionkeyBytes, len);
            objrij.Key = encryptionkeyBytes;
            objrij.IV = encryptionkeyBytes;
            byte[] textByte = objrij.CreateDecryptor().TransformFinalBlock(encryptedTextByte, 0, encryptedTextByte.Length);
            return Encoding.UTF8.GetString(textByte);
        }
    }

    internal class MessageData
    {
        public int Id;
        public string GuildId;
        public string ChannelId;
        public string MessageId;
        public string UserId;
        public string EncryptedContent;
        public DateTime Timestmap;
    }
}