using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Diagnostics;

namespace PeriGen.Patterns.Engine.Registration
{
	public partial class RegistrationForm : Form
	{
		/// <summary>
		/// The ONE source to use to trace data (Windows event log / DebugView...)
		/// </summary>
		static TraceSource Source = new TraceSource("RegistrationForm");

		/// <summary>
		/// Last registration attempt failed?
		/// </summary>
		bool RegistrationFailed { get; set; }

		/// <summary>
		/// Basic constructor
		/// </summary>
		public RegistrationForm()
		{
			// Create the windows event log source entry
			try
			{
				if (!EventLog.SourceExists("PeriGen Patterns Registration"))
				{
					EventLog.CreateEventSource("PeriGen Patterns Registration", "Application");
				}
				if (!EventLog.SourceExists("PeriGen Patterns Engine"))
				{
					EventLog.CreateEventSource("PeriGen Patterns Engine", "Application");
				}
			}
			catch (Exception e)
			{
				Source.TraceEvent(TraceEventType.Warning, 1001, "Warning, unable to create the log source.\n{0}", e);
			}

			Source.TraceEvent(TraceEventType.Information, 1093, "PeriCALM Patterns Registration application started");

			InitializeComponent();
		}

		/// <summary>
		/// Update the label that display the current license informations
		/// </summary>
		void UpdateLicenseInformation()
		{
			if (this.RegistrationFailed)
			{
				this.lblLicenseInformation.Text = "Error";
				this.lblLicenseInformation.ImageKey = "uncheck.png";
				return;
			}

			this.lblLicenseInformation.Text = LicenseValidation.CurrentLicenseDescription;
			switch (LicenseValidation.CurrentLicense)
			{
				case PeriGen.Patterns.Engine.LicenseStatus.Registered:
				case PeriGen.Patterns.Engine.LicenseStatus.TimeLimited:
					this.lblLicenseInformation.ImageKey = "check.png";
					break;
				case PeriGen.Patterns.Engine.LicenseStatus.DemoMode:
					this.lblLicenseInformation.ImageKey = "demo.png";
					break;
				default:
					this.lblLicenseInformation.ImageKey = "uncheck.png";
					break;
			}
		}

		/// <summary>
		/// On register click...
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnRegister_Click(object sender, EventArgs e)
		{
			var error = string.Empty;
			var success = LicenseValidation.Register(LicenseValidation.SerialID, txtActivationCode.Text, out error);

			this.RegistrationFailed = !success;

			// Check if it did work
			if (success)
			{               
				Source.TraceEvent(TraceEventType.Information, 1091, "Successful registration.\n\n{0}", LicenseValidation.CurrentLicenseDescription);
                MessageBox.Show(this, "Registration successful.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

				try
                {
					using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\PeriGen\PeriCALM Patterns"))
					{
						key.SetValue("Key", txtActivationCode.Text, Microsoft.Win32.RegistryValueKind.String);
					}
                }
                catch (Exception ex) 
                {
                    Source.TraceEvent(TraceEventType.Warning, 1094, "Cannot write PeriGen Patterns Engine key in registry.\n\n{0}", ex.ToString());
                }				
			}
			else
			{
				Source.TraceEvent(TraceEventType.Warning, 1092, "Registration failed.\n\n{0}", error);
				MessageBox.Show(this, string.Format(CultureInfo.CurrentUICulture, "Registration failed.\n{0}", error), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}

			// Update current license information
			this.UpdateLicenseInformation();
		}

		/// <summary>
		/// On load...
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RegistrationForm_Load(object sender, EventArgs e)
		{
			this.UpdateLicenseInformation();
			this.txtSerialID.Text = LicenseValidation.SerialID;
			this.txtActivationCode.Focus();
		}

		/// <summary>
		/// On clear license, put back the demo license
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnClear_Click(object sender, EventArgs e)
		{
			// Clean previous serial
			LicenseValidation.EnableDemoMode();
			this.RegistrationFailed = false;

            try
            {
				using (var regKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\PeriGen\PeriCALM Patterns"))
				{
					regKey.SetValue("Key", string.Empty, Microsoft.Win32.RegistryValueKind.String);
				}
            }
            catch (Exception ex) 
            {
                Source.TraceEvent(TraceEventType.Warning, 1095, "Cannot write PeriGen Patterns Engine demo key in registry.\n\n{0}", ex.ToString());
            }

			// Refresh display
			this.UpdateLicenseInformation();
		}
	}
}
