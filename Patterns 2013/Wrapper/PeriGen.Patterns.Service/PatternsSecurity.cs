using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PeriGen.Patterns.Service
{
	/// <summary>
	/// Security class used to encrypt/decrypt data
	/// </summary>
	public static class PatternsSecurity
	{
		/// <summary>
		/// Certificate thumbprint (this is for the code certificate for PeriGen)
		/// </summary>
		static string ThumbPrint = "8b6de359868d59b66e83c7e0b3c39271f4d157b8";

		/// <summary>
		/// Set thumbprint 
		/// </summary>
		/// <param name="value"></param>
		internal static void SetThumbprint(string value)
		{
			ThumbPrint = value;
			if (string.IsNullOrEmpty(ThumbPrint))
			{
				throw new System.Security.SecurityException("Thumbprint cannot be empty");
			}
		}

		/// <summary>
		/// Encrypt a string
		/// </summary>
		/// <param name="data">data to encrypt</param>
		/// <returns>data encrypted</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static byte[] Encrypt(string data)
		{
			///Declare encryption algorithms
			var aesAlgorithm = new RijndaelManaged();
			var aesDataProtector = new AESUtil(aesAlgorithm);
			var rsaDataProtected = new RSAUtil(ThumbPrint);

			///store for result
			List<byte> result = new List<byte>();

			//Encrypt key
			var keyCipher = rsaDataProtected.Encrypt(aesAlgorithm.Key);
			result.AddRange(keyCipher);
			//Encrypt IV
			var ivCipher = rsaDataProtected.Encrypt(aesAlgorithm.IV);
			result.AddRange(ivCipher);
			//Encrypt data
			var textCipher = aesDataProtector.Encrypt(data);
			result.AddRange(textCipher);

			//return encrypted data
			return result.ToArray();
		}

		/// <summary>
		/// Encrypt a byte array
		/// </summary>
		/// <param name="data">bytes to encrypt</param>
		/// <returns>data encrypted</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static byte[] Encrypt(byte[] data)
		{
			///Declare encryption algorithms
			var aesAlgorithm = new RijndaelManaged();
			var aesDataProtector = new AESUtil(aesAlgorithm);
			var rsaDataProtected = new RSAUtil(ThumbPrint);

			///store for result
			List<byte> result = new List<byte>();

			//Encrypt key
			var keyCipher = rsaDataProtected.Encrypt(aesAlgorithm.Key);
			result.AddRange(keyCipher);
			//Encrypt IV
			var ivCipher = rsaDataProtected.Encrypt(aesAlgorithm.IV);
			result.AddRange(ivCipher);
			//Encrypt data
			var textCipher = aesDataProtector.Encrypt(data);
			result.AddRange(textCipher);

			//return encrypted data
			return result.ToArray();
		}

		/// <summary>
		/// Decrypt byte array
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static string Decrypt(byte[] data)
		{
			var rsaDataProtector = new RSAUtil(ThumbPrint);
			byte[] keyBytes = new byte[128];
			byte[] ivBytes = new byte[128];
			byte[] dataBytes = new byte[data.Length - 256];
			Array.Copy(data, 0, keyBytes, 0, 128);
			Array.Copy(data, 128, ivBytes, 0, 128);
			Array.Copy(data, 256, dataBytes, 0, data.Length - 256);
			var aesAlgorith = new RijndaelManaged
							{
								Key = rsaDataProtector.Decrypt(keyBytes),
								IV = rsaDataProtector.Decrypt(ivBytes)
							};
			var aesDataProtector = new AESUtil(aesAlgorith);
			return aesDataProtector.Decrypt(dataBytes);
		}
	}

	/// <summary>
	/// AES wrapper
	/// </summary>
	class AESUtil
	{
		RijndaelManaged algorithm = null;
		public AESUtil(RijndaelManaged aes)
		{
			algorithm = aes;
		}

		/// <summary>
		/// Encrypt String
		/// </summary>
		/// <param name="data">data to encrypt</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public byte[] Encrypt(string data)
		{
			MemoryStream msEncrypt = null;
			CryptoStream csEncrypt = null;
			StreamWriter swEncrypt = null;
			try
			{
				var encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV);
				msEncrypt = new MemoryStream();
				csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
				swEncrypt = new StreamWriter(csEncrypt);
				swEncrypt.Write(data);
			}
			finally
			{
				if (swEncrypt != null) swEncrypt.Close();
				if (csEncrypt != null) csEncrypt.Close();
				if (msEncrypt != null) msEncrypt.Close();
				if (algorithm != null) algorithm.Clear();
			}
			return msEncrypt.ToArray();
		}

		/// <summary>
		/// Encrypt a byte array
		/// </summary>
		/// <param name="data">data to encrypt</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public byte[] Encrypt(byte[] data)
		{
			MemoryStream msEncrypt = null;
			CryptoStream csEncrypt = null;
			BinaryWriter bwEncrypt = null;
			try
			{
				var encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV);
				msEncrypt = new MemoryStream();
				csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
				bwEncrypt = new BinaryWriter(csEncrypt);
				bwEncrypt.Write(data);
			}
			finally
			{
				if (bwEncrypt != null) bwEncrypt.Close();
				if (csEncrypt != null) csEncrypt.Close();
				if (msEncrypt != null) msEncrypt.Close();
				if (algorithm != null) algorithm.Clear();
			}
			return msEncrypt.ToArray();
		}

		/// <summary>
		/// Decrypt a byte array
		/// </summary>
		/// <param name="data">data to encrypt</param>
		/// <returns>string with data decrypted</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public string Decrypt(byte[] data)
		{
			MemoryStream msDecrypt = null;
			CryptoStream csDecrypt = null;
			StreamReader srDecrypt = null;
			string result;
			try
			{
				ICryptoTransform decryptor = algorithm.CreateDecryptor(algorithm.Key, algorithm.IV);
				msDecrypt = new MemoryStream(data);
				csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
				srDecrypt = new StreamReader(csDecrypt);
				result = srDecrypt.ReadToEnd();
			}
			finally
			{
				if (srDecrypt != null) srDecrypt.Close();
				if (csDecrypt != null) csDecrypt.Close();
				if (msDecrypt != null) msDecrypt.Close();
				if (algorithm != null) algorithm.Clear();
			}
			return result;
		}
	}

	/// <summary>
	/// RSA wrapper
	/// </summary>
	class RSAUtil
	{
		/// <summary>
		/// Certificate
		/// </summary>
		X509Certificate2 SignatureCertificate { get; set; }

		/// <summary>
		/// Ctr
		/// </summary>
		/// <param name="thumbprintData"></param>
		public RSAUtil(string thumbprintData)
		{
			if (string.IsNullOrEmpty(thumbprintData))
				throw new ArgumentNullException("thumbprintData");

			var x509Store = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
			x509Store.Open(OpenFlags.ReadOnly);

			foreach (var storeCertificate in x509Store.Certificates)
			{
				if (string.Compare(thumbprintData, storeCertificate.Thumbprint, true) == 0)
				{
					this.SignatureCertificate = storeCertificate;
					break;
				}
			}

			if (this.SignatureCertificate == null)
				throw new ArgumentOutOfRangeException("thumbprintData", "Certificate cannot be found!");
		}

		/// <summary>
		/// Encrypt a byte array
		/// </summary>
		/// <param name="data">data to encrypt</param>
		/// <returns>data encrypted</returns>			
		public byte[] Encrypt(byte[] data)
		{
			RSACryptoServiceProvider rsaProvider = null;
			try
			{
				rsaProvider = (RSACryptoServiceProvider)this.SignatureCertificate.PublicKey.Key;
			}
			catch (Exception ex)
			{
				throw new SecurityException("Encryption keys cannot be retrieved.", ex);
			}
			return rsaProvider.Encrypt(data, false);
		}

		/// <summary>
		/// Decrypt a byte array
		/// </summary>
		/// <param name="data">data to decrypt</param>
		/// <returns>bytes decrypted</returns>
		public byte[] Decrypt(byte[] data)
		{
			RSACryptoServiceProvider rsaProvider = null;
			try
			{
				rsaProvider = (RSACryptoServiceProvider)this.SignatureCertificate.PrivateKey;
			}
			catch (Exception ex)
			{
				throw new SecurityException("Decryption keys cannot be retrieved.", ex);
			}

			return rsaProvider.Decrypt(data, false);
		}
	}
}
