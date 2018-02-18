using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Globalization;

namespace PeriGen.Patterns.GE.Web
{
	namespace Security
	{
		/// <summary>
		/// Class that allows to encrypt/decrypt data with RC4 algorithm
		/// </summary>
		internal static class RC4
		{
			static int[] _box;
			static int[] _key;

			/// <summary>
			/// Initialize algorithm
			/// </summary>
			/// <param name="value">Key used to initialize algorithm</param>
			static void RC4Initialize(string value)
			{
				_box = new int[256];
				_key = new int[256];

				int len = value.Length;

				for(int a = 0; a <= 255; a++)
				{
					_key[a] = (int)Convert.ToChar(value.Substring((a % len), 1));
					_box[a] = a;
				}

				int x = 0;
				for(int b = 0; b <= 255; b++)
				{
					x = (x + _box[b] + _key[b]) % 256;
					int tempSwap = _box[b];
					_box[b] = _box[x];
					_box[x] = tempSwap;
				}
			}

			/// <summary>
			/// Decrypt method.
			/// </summary>
			/// <param name="value">data to decrypt in Hexadecimal format</param>
			/// <param name="key">key used to decrypt data</param>
			/// <returns>data decrypted as string</returns>
			public static string Decrypt(string value, string key)
			{

				//retrieve encoded data from Hexadecimal values
				StringBuilder sb = new StringBuilder();
				while(value.Length > 0)
				{
					sb.Append(System.Convert.ToChar(System.Convert.ToUInt32(value.Substring(0, 2), 16)).ToString());
					value = value.Substring(2, value.Length - 2);
				}

				//decrypt
				List<char> list = EncryptDecrypt(sb.ToString(), key);

				//return result decrypted
				return new string(list.ToArray());
			}

			/// <summary>
			/// Encryption method.
			/// </summary>
			/// <param name="value">data to encrypt</param>
			/// <param name="key">key used to encrypt data</param>
			/// <returns>Encrypted data encoded in Hexadecimal string</returns>
			public static string Encrypt(string value, string key)
			{
				//prepare buffer
				StringBuilder sb = new StringBuilder();

				//encrypt
				List<char> list = EncryptDecrypt(value, key);

				//convert to Hex
				list.ForEach(c => sb.Append(Convert.ToInt32(c, CultureInfo.InvariantCulture).ToString("X2")));

				//return encrypted value in Hex format
				return sb.ToString();
			}

			/// <summary>
			/// Encrypt and Decrypt method
			/// </summary>
			/// <param name="value">value to encrypt/decrypt</param>
			/// <param name="key">key used to ecrypt/decrypt</param>
			/// <returns>list of chars with values encrypted/decrypted</returns>
			private static List<char> EncryptDecrypt(string value, string key)
			{
				int i = 0;
				int j = 0;

				//Initialize algorithm
				RC4Initialize(key);

				//prepare buffer
				List<char> list = new List<char>();

				//encrypt/decrypt
				for(int a = 0; a < value.Length; a++)
				{
					int temp = 0;

					i = (i + 1) % 256;
					j = (j + _box[i]) % 256;
					temp = _box[i];
					_box[i] = _box[j];
					_box[j] = temp;

					int k = _box[(_box[i] + _box[j]) % 256];

					int cipherby = ((int)Convert.ToChar(value.Substring(a, 1))) ^ k;

					list.Add(Convert.ToChar(cipherby));
				}

				return list;
			}
		}
	}
}
