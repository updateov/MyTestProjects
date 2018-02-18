using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace PeriGen.Patterns.WebSite.common
{
	static class DecryptCALMPost
	{
		/// <summary>
		/// Constant Initialization vector (fixed in CC)
		/// </summary>
		static byte[] IV = { 2, 123, 89, 12, 99, 23, 23, 12, 32, 221, 93, 45, 32, 12, 33, 99, 12, 32, 199 };

		/// <summary>
		/// Constant SALT key (fixed in CC)
		/// </summary>
		static string Salt = "A Touch Of Salt";

		/// <summary>
		/// Constant Key size (fixed in CC)
		/// </summary>
		static int KeySize = 256;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public static string Decrypt(string encryptedMessage, string paraphrase)
		{
			byte[] data = Convert.FromBase64String(encryptedMessage);

			var password = new PasswordDeriveBytes(paraphrase, Encoding.Unicode.GetBytes(Salt));
			using (var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC })
			using (var decryptor = symmetricKey.CreateDecryptor(password.GetBytes(KeySize / 8), IV))
			{
				var buffer = new byte[data.Length];

				using (MemoryStream memoryStream = new MemoryStream(data))
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
				{
					int size = cryptoStream.Read(buffer, 0, buffer.Length);
					return Encoding.Unicode.GetString(buffer, 0, size);
				}
			}
		}
	}
}