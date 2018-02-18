using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NavigationData
{
    public class EncDec
    {
        /// <summary>
        /// Encrypt a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paraphrase"></param>
        /// <param name="IV"></param>
        /// <param name="salt"></param>
        /// <param name="keySize"></param>
        /// <returns></returns>
        public static String RijndaelEncrypt(String message, String paraphrase, byte[] IV, String salt, int keySize)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);

            PasswordDeriveBytes password = new PasswordDeriveBytes(paraphrase, Encoding.Unicode.GetBytes(salt));
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(password.GetBytes(keySize / 8), IV);

            using (MemoryStream memoryStream = new MemoryStream())
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
}
