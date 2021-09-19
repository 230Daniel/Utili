using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Database
{
    internal static class Encryption
    {
        private static readonly byte[] Salt = { 0x4c, 0x7a, 0xef, 0x10, 0x3c, 0xed, 0xdc, 0x2e, 0xc5, 0xfe, 0x07, 0xbd, 0x26, 0x08, 0x22, 0xad };

        public static byte[] Encrypt(byte[] plain, string password)
        {
            var rijndael = Rijndael.Create();
            Rfc2898DeriveBytes pdb = new(password, Salt);
            rijndael.Key = pdb.GetBytes(32);
            rijndael.IV = pdb.GetBytes(16);
            var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(plain, 0, plain.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }

        public static byte[] Decrypt(byte[] cipher, string password)
        {
            try
            {
                var rijndael = Rijndael.Create();
                Rfc2898DeriveBytes pdb = new(password, Salt);
                rijndael.Key = pdb.GetBytes(32);
                rijndael.IV = pdb.GetBytes(16);
                MemoryStream memoryStream = new();
                CryptoStream cryptoStream =
                    new(memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(cipher, 0, cipher.Length);
                cryptoStream.Close();
                return memoryStream.ToArray();
            }
            catch
            {
                return cipher;
            }
        }

        public static string GeneratePassword(ulong[] ids)
        {
            var passwordBuilder = new StringBuilder();

            var amountTake = 16;
            foreach (var id in ids)
            {
                passwordBuilder.Append(id.ToString().Substring(0, amountTake));

                if (amountTake > 2) amountTake -= 2;
            }

            return passwordBuilder.ToString();
        }
    }
}
