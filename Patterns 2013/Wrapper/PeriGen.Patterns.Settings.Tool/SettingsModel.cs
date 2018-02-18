using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;
using System.Xml;

namespace PeriGen.Patterns.Settings.Tool
{
	internal class SettingsModel
	{
		/// <summary>
		/// ctor
		/// </summary>
		public SettingsModel()
		{
			Reset();
		}

		/// <summary>
		/// Indicates if the model has changed
		/// </summary>
		public bool HasBeenChanged
		{
			get
			{
				return !(Sections == null || (Sections != null && Sections.Sum(l => (l.Settings.Count(s => s.Edited))) == 0));
			}
		}

		/// <summary>
		/// Trace source fopr logging
		/// </summary>
		public TraceSource Source
		{
			get;
			set;
		}

		/// <summary>
		/// Thumbprint of current settings file
		/// </summary>
		public string Thumbprint
		{
			get;
			private set;
		}

		/// <summary>
		/// List of sections found in the settings file
		/// </summary>
		public List<SettingsSection> Sections
		{
			get;
			private set;
		}

		/// <summary>
		/// Current opened filename 
		/// </summary>
		public string CurrentFileName
		{
			get;
			private set;
		}

		/// <summary>
		/// Open settings file
		/// </summary>
		/// <param name="filename"></param>
		public void OpenSettingsFromFile(string filename)
		{
			try
			{
				///Try to load settings file
				XDocument doc = XDocument.Load(filename);
				XElement settings = doc.Element("settings");

				CurrentFileName = filename;

				//no thumbprint, raise exception
				if (settings.Attribute("thumbprint") == null) throw new InvalidDataException("Thumbprint not found in settings file.");
				var thumbprint = settings.Attribute("thumbprint").Value;

				//try to initialize SettingsManager and Security
				SettingsManager.InitializeToolSettings(thumbprint);

				Thumbprint = thumbprint;

				///check if settings is encrypted or not
				///if it is not encrypted, just dump the xml
				if (settings.Elements().Count() == 0)
				{
					///settings is encrypted
					///decrypt content
					///Check old certificate
					if (!SettingsManager.CertificateIsValid(Thumbprint))
					{
						Source.TraceEvent(TraceEventType.Error, 1008, "Thumbprint found in the file is not valid.\r\nVerify that the thumbprint is the right one and the certificate is installed.");
						throw new InvalidOperationException("Thumbprint found in the file is not valid.\r\nVerify that the thumbprint is the right one and the certificate is installed.");
					}

					var settingsContent = settings.Value;
					if (string.IsNullOrEmpty(settingsContent)) throw new InvalidDataException("Invalid content in settings file.");

					///decrypt values						
					settings = XElement.Parse(SettingsSecurity.Decrypt(Convert.FromBase64String(settingsContent)));
				}

				//load decrypted XML 
				var document = new XmlDocument();
				document.LoadXml(settings.ToString());
				var children = document.DocumentElement.SelectNodes("sections/section");

				//sections
				Sections = new List<SettingsSection>();
				foreach (XmlNode child in children)
				{
					//New section
					SettingsSection s = new SettingsSection();
					s.Level = int.Parse(child.Attributes["level"].Value);
					s.Name = child.Attributes["name"].Value;

					//Load items for section
					foreach (XmlNode setting in child.SelectNodes("setting"))
					{
						//Item for section						
						var data = new SettingData();
						data.Type = setting.Attributes["type"].Value;
						data.Key = setting.Attributes["key"].Value;
						data.Value = setting.Attributes["value"].Value;
						data.ResetValue = setting.Attributes["value"].Value;
						data.Info = setting.Attributes["info"].Value;
						data.Edited = false;
						s.Settings.Add(data);
					}
					Sections.Add(s);
				}
			}
			catch (Exception)
			{
				Reset();
				throw;
			}
		}

		/// <summary>
		/// Save changes into settings file
		/// </summary>
		/// <param name="newThumbprint">new thumbprint if is needed</param>
		/// <param name="mustReloadFile">indicate if must reload file because change of thumbprint</param>
		public void SaveSettingsToFile(string newThumbprint, out bool mustReload)
		{

			//Backup thumbprint
			var tempThumbprint = Thumbprint;

			//Check old certificate
			if (!SettingsManager.CertificateIsValid(Thumbprint))
			{
				Source.TraceEvent(TraceEventType.Error, 1003, "Current certificate not found.\r\nVerify that the thumbprint is the right one and the certificate is installed.");
				throw new InvalidOperationException("Current certificate not found.\r\nVerify that the thumbprint is the right one and the certificate is installed.");
			}

			if (!string.IsNullOrEmpty(newThumbprint))
			{
				//Validate thumbprint
				if (!SettingsManager.CertificateIsValid(newThumbprint))
				{
					Source.TraceEvent(TraceEventType.Error, 1004, "New certificate thumbprint is not valid.\r\nVerify that the thumbprint is the right one and the certificate is installed.");
					throw new InvalidOperationException("New certificate thumbprint is not valid.\r\nVerify that the thumbprint is the right one and the certificate is installed.");
				}
				tempThumbprint = newThumbprint;
			}

			//Create a new settings XML
			//New XML has to have Settings, Sections and Section items
			//root
			XElement file = new XElement("settings");

			//thumbprint
			file.Add(new XAttribute("thumbprint", tempThumbprint));

			//Sections
			XElement sections = new XElement("sections");
			file.Add(sections);

			//Add individual sections
			foreach (var item in Sections)
			{
				//new section
				XElement section = new XElement("section");
				section.Add(new XAttribute("level", item.Level));
				section.Add(new XAttribute("name", item.Name));
				sections.Add(section);

				//items for section
				foreach (var item1 in item.Settings)
				{
					section.Add(new XComment(item1.Info));

					XElement setting = new XElement("setting");
					setting.Add(new XAttribute("key", item1.Key));
					setting.Add(new XAttribute("value", item1.Value));
					setting.Add(new XAttribute("info", item1.Info));
					setting.Add(new XAttribute("type", item1.Type));
					section.Add(setting);
				}

			}

			///update thumbprint
			SettingsManager.InitializeToolSettings(tempThumbprint);

			///encrypt settings
			var encrypted = Convert.ToBase64String(SettingsSecurity.Encrypt(UTF8Encoding.UTF8.GetBytes(file.ToString())));
			file.RemoveNodes();

			///save settings encrypted
			file.SetValue(encrypted);

			///Write content to the file
			File.WriteAllText(CurrentFileName, file.ToString());

			//File was saved correctly. I can change the thumbprint now
			Thumbprint = tempThumbprint;

			//Reset edited
			Sections.ForEach(i => i.Settings.ForEach(s => s.Edited = false));

			//Must reload file???
			mustReload = (string.Compare(Thumbprint, newThumbprint, true) == 0);

			//If I Must reload, reset
			if (mustReload) Reset();

		}

		/// <summary>
		/// Reset model to default values
		/// </summary>
		public void Reset()
		{
			CurrentFileName = string.Empty;
			Sections = null;
			Thumbprint = string.Empty;
		}
	}
}
