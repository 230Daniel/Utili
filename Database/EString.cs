using System;
using System.Text;

namespace Database
{
    public class EString
    {
        public string Value { get; protected set; }
        public string EncodedValue { get; protected set; }

        public string GetEncryptedValue(ulong[] ids)
        {
            return Encrypt(Value, ids);
        }

        public static EString FromEncoded(string encodedValue)
        {
            return new EString
            {
                EncodedValue = encodedValue, 
                Value = Decode(encodedValue)
            };
        }

        public static EString FromDecoded(string decodedValue)
        {
            return new EString
            {
                Value = decodedValue, 
                EncodedValue = Encode(decodedValue)
            };
        }

        public static EString FromEncrypted(string encryptedValue, ulong[] ids)
        {
            return new EString
            {
                EncodedValue = Encode(Decrypt(encryptedValue, ids)),
                Value = Decrypt(encryptedValue, ids)
            };
        }
        
        private static string Encode(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        private static string Decode(string input)
        {
            byte[] bytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(bytes);
        }

        private static string Encrypt(string input, ulong[] ids)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            bytes = Encryption.Encrypt(bytes, Encryption.GeneratePassword(ids));

            return Convert.ToBase64String(bytes);
        }

        private static string Decrypt(string input, ulong[] ids)
        {
            byte[] bytes = Convert.FromBase64String(input);

            bytes = Encryption.Decrypt(bytes, Encryption.GeneratePassword(ids));

            return Encoding.UTF8.GetString(bytes);
        }
    }
}
