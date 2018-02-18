using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PatternsPluginsCommon
{
    public static class EncDec
    {
        public static byte[] s_IV = { 2, 123, 89, 12, 99, 23, 23, 12, 32, 221, 93, 45, 32, 12, 33, 99, 12, 32, 199 };
        public static string s_Salt = "A Touch Of Salt";
        public static int s_KeySize = 256;

        public static String RijndaelEncrypt(String message, String paraphrase)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);

            PasswordDeriveBytes password = new PasswordDeriveBytes(paraphrase, Encoding.Unicode.GetBytes(s_Salt));
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(password.GetBytes(s_KeySize / 8), s_IV);

            using (MemoryStream memoryStream = new MemoryStream())
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public static string RijndaelDecrypt(string message, string paraphrase)
        {
            byte[] data = Convert.FromBase64String(message);

            PasswordDeriveBytes password = new PasswordDeriveBytes(paraphrase, Encoding.Unicode.GetBytes(s_Salt));
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(password.GetBytes(s_KeySize / 8), s_IV);

            byte[] buffer = new byte[data.Length];

            using (MemoryStream memoryStream = new MemoryStream(data))
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                int size = cryptoStream.Read(buffer, 0, buffer.Length);
                return Encoding.Unicode.GetString(buffer, 0, size);
            }
        }
    }
}
