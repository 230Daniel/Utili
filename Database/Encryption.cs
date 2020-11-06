using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Database
{
    internal class Encryption
    {
        private static readonly byte[] Salt = { 0x4c, 0x7a, 0xef, 0x10, 0x3c, 0xed, 0xdc, 0x2e, 0xc5, 0xfe, 0x07, 0xbd, 0x26, 0x08, 0x22, 0xad };

        public static byte[] Encrypt(byte[] plain, string password)
        {
            Rijndael rijndael = Rijndael.Create();
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, Salt);
            rijndael.Key = pdb.GetBytes(32);
            rijndael.IV = pdb.GetBytes(16);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(plain, 0, plain.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }

        public static byte[] Decrypt(byte[] cipher, string password)
        {
            try
            {
                Rijndael rijndael = Rijndael.Create();
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, Salt);
                rijndael.Key = pdb.GetBytes(32);
                rijndael.IV = pdb.GetBytes(16);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream =
                    new CryptoStream(memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(cipher, 0, cipher.Length);
                cryptoStream.Close();
                return memoryStream.ToArray();
            }
            catch
            {
                return cipher;
            }
        }

        public static string GeneratePassword(ulong arg1, ulong arg2, ulong arg3, ulong arg4)
        {
            string password = arg1.ToString().Substring(0, 16) + arg2.ToString().Substring(0, 14) +
                              arg3.ToString().Substring(0, 12) + arg4.ToString().Substring(0, 10);

            return password;
        }
    }
}
