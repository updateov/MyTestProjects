using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace PeriGen.Patterns.Settings
{
	/// <summary>
	/// Manage application settings
	/// </summary>
	public static class SettingsManager
	{
		internal const string PeriGenDefaultCertificateThumbprint = "8b6de359868d59b66e83c7e0b3c39271f4d157b8";

		static Dictionary<String, String> Values = null;

		static FileSystemWatcher settingsWatcher = null;

		static SettingsManager()
		{
			// Make sure the default PeriGen certificate is installed
			ValidatePeriGenCertificateInstalled();

			//Load the settings
			LoadSettings();

			//Set watcher to settings file
			SetWatcher();

		}

		static private bool _automaticReloadSettings = false;
		public static bool AutomaticReloadSettings
		{
			get
			{
				return _automaticReloadSettings;
			}
			set
			{
				_automaticReloadSettings = value;
				SetWatcher();
			}
		}

		private static void SetWatcher()
		{

			//remove previous handlers and destroy watcher always
			if (settingsWatcher != null)
			{
				settingsWatcher.EnableRaisingEvents = false;
				settingsWatcher.Changed -= settingsWatcher_Changed;
				settingsWatcher = null;
			}

			//Enable Automatic reload
			if (AutomaticReloadSettings)
			{
				//Set watcher to reload after changes if there is a file to watch
				string SettingsFile = string.Empty;
				SettingsFile = GetSettingsFilePath();
				if (!string.IsNullOrEmpty(SettingsFile))
				{
					//recreate brand new watcher
					settingsWatcher = new FileSystemWatcher();
					settingsWatcher.Path = Path.GetDirectoryName(SettingsFile);
					settingsWatcher.Filter = Path.GetFileName(SettingsFile);
					settingsWatcher.IncludeSubdirectories = false;
					settingsWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite;
					settingsWatcher.Changed += settingsWatcher_Changed;
					settingsWatcher.EnableRaisingEvents = true;
				}
			}
		}

		static void settingsWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			//stop watching temporarily
			settingsWatcher.EnableRaisingEvents = false;

			lock (Values)
			{
				//Reload the settings
				LoadSettings();
			}
			//restart watching...
			settingsWatcher.EnableRaisingEvents = true;
		}

		/// <summary>
		/// Load settings file, decrypt it if necessary and populate dictionary 
		/// </summary>
		public static void LoadSettings()
		{
			// Initialize dictionary
			Values = new Dictionary<string, string>();

			// Get Settings file
			string SettingsFile = string.Empty;
			SettingsFile = GetSettingsFilePath();
			if (string.IsNullOrEmpty(SettingsFile)) return;
			try
			{
				// Load settings file
				var doc = XDocument.Load(SettingsFile);
				var settings = doc.Element("settings");

				if (settings == null)
					throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Invalid settings file content!\n{0}", SettingsFile));

				// Read thumbprint
				SetValue("thumbprint", settings.Attribute("thumbprint").Value);
				SettingsSecurity.SetThumbprint(settings.Attribute("thumbprint").Value);

				// Verify Certificate is Installed
				if (!CertificateIsValid(settings.Attribute("thumbprint").Value))
					throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Certificate with thumbprint '{0}' not found.", settings.Attribute("thumbprint").Value));

				// Decrypt if necessary
				if ((settings.Elements().Count() == 0) && (!string.IsNullOrEmpty(settings.Value)))
				{
					// Decrypt xml
					settings = XElement.Parse(SettingsSecurity.Decrypt(Convert.FromBase64String(settings.Value)));
				}

				// Go through the values and populate dictionary
				if (settings.Descendants("setting").Count() == 0)
					throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Empty settings file content!\n{0}", SettingsFile));

				// Load the settings
				foreach (var item in settings.Descendants("setting"))
				{
					SetValue(item.Attribute("key").Value, item.Attribute("value").Value);
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Error loading the settings file.", ex);
			}
		}

		/// <summary>
		/// Return the path of the configuration file depending on if is in web app or a desktop app
		/// </summary>
		/// <returns></returns>
		private static string GetSettingsFilePath()
		{
			var SettingsFile = string.Empty;

			SettingsFile = System.Configuration.ConfigurationManager.AppSettings["config"];

			// No settings file defined???
			if (string.IsNullOrEmpty(SettingsFile)) return string.Empty;

			SettingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFile);

			// Check settings file
			if (!File.Exists(SettingsFile))
				throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Missing settings file!\n{0}", SettingsFile));
			SettingsFile = Path.GetFullPath(SettingsFile);

			return SettingsFile;
		}

		/// <summary>
		/// Encrypt the settings if not already done
		/// </summary>
		public static void EncryptSettings()
		{

			// Settings file
			var SettingsFile = GetSettingsFilePath();

			//Must Encrypt?
			if (string.IsNullOrEmpty(SettingsFile)) return;

			try
			{
				// Load settings file
				var doc = XDocument.Load(SettingsFile);
				var settings = doc.Element("settings");

				if (settings == null)
					throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Invalid settings file content!\n{0}", SettingsFile));

				// Read thumbprint
				SetValue("thumbprint", settings.Attribute("thumbprint").Value);
				SettingsSecurity.SetThumbprint(settings.Attribute("thumbprint").Value);

				// Verify Certificate is Installed
				if (!CertificateIsValid(settings.Attribute("thumbprint").Value))
					throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Certificate with thumbprint '{0}' not found.", settings.Attribute("thumbprint").Value));

				// Encrypt if necessary
				if (settings.Elements().Count() > 0)
				{
					// Encrypt xml
					var encrypted = Convert.ToBase64String(SettingsSecurity.Encrypt(UTF8Encoding.UTF8.GetBytes(settings.ToString())));

					// Save it
					var encryptedDocument = new XDocument();
					encryptedDocument.Add(new XElement("settings", new XAttribute("thumbprint", GetValue("thumbprint"))) { Value = encrypted });
					encryptedDocument.Save(SettingsFile);
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Error encrypting the settings file.", ex);
			}
		}

		/// <summary>
		/// Set thumbprint value
		/// </summary>
		/// <param name="thumbprint"></param>
		public static void InitializeToolSettings(string thumbprint)
		{
			SettingsSecurity.SetThumbprint(thumbprint);
		}

		/// <summary>
		/// Set the given setting
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public static void SetValue(string key, string value)
		{
			if (Values == null)
				throw new InvalidOperationException("You must initialize the settings before using them!");

			Values[key.ToUpperInvariant()] = value;
		}

		/// <summary>
		/// Return value from dictionary
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetValue(string key)
		{
			if (Values == null)
				throw new InvalidOperationException("You must initialize the settings before using them!");

			string value;
			if (!Values.TryGetValue(key.ToUpperInvariant(), out value))
			{
				Debug.Assert(false, "Missing setting value " + key);
				return string.Empty;
			}

			return value;
		}

		/// <summary>
		/// Return value from dictionary
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static int GetInteger(string key)
		{
			var valueAsString = GetValue(key);
			if (string.IsNullOrEmpty(key))
			{
				Debug.Assert(false, "Missing setting value: " + key);
				return 0;
			}

			int value;
			if (!int.TryParse(valueAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
			{
				Debug.Assert(false, "Setting value is not a valid integer: " + key);
				return 0;
			}

			return value;
		}

		/// <summary>
		/// Return value from dictionary
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool GetBoolean(string key)
		{
			var valueAsString = GetValue(key);
			if (string.IsNullOrEmpty(key))
			{
				Debug.Assert(false, "Missing setting value");
				return false;
			}

			bool value;
			if (!bool.TryParse(valueAsString, out value))
			{
				Debug.Assert(false, "Missing setting value");
				return false;
			}

			return value;
		}

		/// <summary>
		/// Validate and install certificate for data encryption
		/// </summary>
		public static void ValidatePeriGenCertificateInstalled()
		{
			// Check if certificate exist
			if (CertificateIsValid(PeriGenDefaultCertificateThumbprint))
				return;

			// It does not exist, try to install it
			try
			{
				var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PeriGen.Patterns.Settings.Resources.PeriGenSettingsCertificate.pfx");
				if (stream == null) throw new InvalidDataException("Application cannot find the resource PeriGenSettingsCertificate.");

				//Read certificate
				stream.Position = 0;
				Byte[] cert = new byte[stream.Length];
				stream.Read(cert, 0, (int)stream.Length);
				stream.Close();

				//Open storage
				X509Certificate2 certificate = new X509Certificate2(cert, "demo", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
				X509Store store = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);

				//Write
				store.Open(OpenFlags.ReadWrite);
				store.Add(certificate);
				store.Close();
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Error installing PeriGenSettingsCertificate.", ex);
			}
		}

		/// <summary>
		/// Validate certificate based in Thumbprint
		/// </summary>
		/// <param name="thumbprint"></param>
		/// <returns></returns>
		public static bool CertificateIsValid(string thumbprint)
		{
			//Find certificate
			var x509Store = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
			x509Store.Open(OpenFlags.ReadOnly);

			foreach (var storeCertificate in x509Store.Certificates)
			{
				if (string.Compare(thumbprint, storeCertificate.Thumbprint, true) == 0)
				{
					return true;
				}
			}
			return false;
		}
	}
}
