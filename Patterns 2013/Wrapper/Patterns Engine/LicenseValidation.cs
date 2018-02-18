using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Diagnostics;

namespace PeriGen.Patterns.Engine
{
	/// <summary>
	/// Possible status of the license
	/// </summary>
	public enum LicenseStatus 
	{ 
		/// <summary>
		/// No license detected
		/// </summary>
		None,

		/// <summary>
		/// Error with the license validation
		/// </summary>
		Error, 

		/// <summary>
		/// A license but it is expired
		/// </summary>
		Expired, 

		/// <summary>
		/// The computer clock was reset, license blocked
		/// </summary>
		Corrupted, 

		/// <summary>
		/// Full license installed
		/// </summary>
		Registered, 

		/// <summary>
		/// Time based full license installed
		/// </summary>
		TimeLimited, 

		/// <summary>
		/// Demo mode
		/// </summary>
		DemoMode };

	/// <summary>
	/// Helper class to validate license registration
	/// </summary>
	public static class LicenseValidation
	{
		#region Unmanaged DLL encapsulation

		/// <summary>
		/// For using the unmanaged SerialShield.dll that handle license validation
		/// </summary>
		static class SafeNativeMethods
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "0"), DllImport("serialshield.dll", EntryPoint = "SS_R", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
			public static extern int SS_R(string Name, string Key);

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "0"), DllImport("serialshield.dll", EntryPoint = "SetApplicationInfo", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
			public static extern void SetApplicationInfo(string AppName, string AppKey);

			[DllImport("serialshield.dll", EntryPoint = "SS_Initialize", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
			public static extern void SS_Initialize();

			[DllImport("serialshield.dll", EntryPoint = "SS_IsUnlocked", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool SS_IsUnlocked();

			[DllImport("serialshield.dll", EntryPoint = "SS_TrialExpired", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool SS_TrialExpired();

			[DllImport("serialshield.dll", EntryPoint = "SS_TrialMode", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
			public static extern int SS_TrialMode();

			[DllImport("serialshield.dll", EntryPoint = "GetHardwardID", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
			public static extern string GetHardwardID();

			[DllImport("serialshield.dll", EntryPoint = "SS_RemoveKey", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool SS_RemoveKey();

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "0"), DllImport("serialshield.dll", EntryPoint = "SSUser", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
			public static extern int SSUser(string FullName, string Key, string SerialID);

			[DllImport("serialshield.dll", EntryPoint = "SS_LicenseInfo", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
			public static extern string SS_LicenseInfo();

		}

		#endregion

		/// <summary>
		/// Serial dll initialized
		/// </summary>
		static bool LicenseInitialized { get; set; }

		/// <summary>
		/// Initialization for a given product/application
		/// </summary>
		static LicenseValidation()
		{
			try
			{
				SafeNativeMethods.SS_R("Bruno Bendavid", "lXcsBCcUEgf/vg+bEYzzb8r4cksDbB0wV5+isMmk9BvvayXF+j4fChYY2+L83URYqlTCIxoRL16PcHoMtHICEQ==");
				SafeNativeMethods.SetApplicationInfo("PeriCALM Patterns Engine", "84591A5B-F651-430e-A17E-D2EDF096010B");
				SafeNativeMethods.SS_Initialize();

				LicenseInitialized = true;
			}
			catch (Exception ex)
			{
				PatternsEngineWrapper.Source.TraceEvent(TraceEventType.Error, 1124, "Unable to initialize the license engine.\n\n{0}", ex);
				LicenseInitialized = false;
			}
		}

		/// <summary>
		/// Check if there is a valid license installed
		/// </summary>
		public static bool HasValidLicense
		{
			get
			{
				var license = CurrentLicense;
				switch (license)
				{
					case PeriGen.Patterns.Engine.LicenseStatus.Registered:
					case PeriGen.Patterns.Engine.LicenseStatus.TimeLimited:
					case PeriGen.Patterns.Engine.LicenseStatus.DemoMode:
						return true;

					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Return the current license status
		/// </summary>
		public static LicenseStatus CurrentLicense
		{
			get
			{
				if (!LicenseInitialized)
					return LicenseStatus.Error;

				if (SafeNativeMethods.SS_IsUnlocked())
					return LicenseStatus.Registered;

				if (SafeNativeMethods.SS_TrialExpired())
					return LicenseStatus.Expired;

				switch (SafeNativeMethods.SS_TrialMode())
				{
					// Date tempered
					case 99:
						return LicenseStatus.Corrupted;

					// Time base / Run based / Date based
					case 1:
					case 2:
					case 3:
						return LicenseStatus.TimeLimited;

					// Trial but free mode (no limit)
					case 4:
						return LicenseStatus.DemoMode;
				}

				// Trial mode or any other unexpected mode is OFF
				return LicenseStatus.None;
			}
		}

		/// <summary>
		/// Return the string that represent the current license information
		/// </summary>
		public static string CurrentLicenseDescription
		{
			get
			{
				switch (CurrentLicense)
				{
					case LicenseStatus.Error:
						return "Unable to initialize the license engine";

					case LicenseStatus.Registered:
						return "Full license";

					case LicenseStatus.DemoMode:
						return "Demo mode";

					case LicenseStatus.TimeLimited:
						switch (SafeNativeMethods.SS_TrialMode())
						{
							case 1:
							case 2:
								return string.Format(CultureInfo.CurrentUICulture, "Time-limited license\n{0} day(s) remain(s) before expiration", SafeNativeMethods.SS_LicenseInfo());
							case 3:
								return string.Format(CultureInfo.CurrentUICulture, "Time-limited license\n{0} run(s) remain(s) before expiration", SafeNativeMethods.SS_LicenseInfo());
						}
						return "Time-limited license";

					case LicenseStatus.Corrupted:
						return "Corrupted license\nYour date system has been moved back";

					case LicenseStatus.Expired:
						return "Expired license";

					case LicenseStatus.None:
					default:
						return "No license";
				}
			}
		}

		/// <summary>
		/// Retrieve the serial ID associated to the current machine
		/// </summary>
		public static string SerialID
		{
			get
			{
				if (LicenseInitialized)
				{
					return SafeNativeMethods.GetHardwardID();
				}
				return string.Empty;
			}
		}

		/// <summary>
		/// Register the application with the given info
		/// </summary>
		/// <param name="key">The license key</param>
		/// <param name="serial">The serial ID</param>
		/// <param name="error">Error message in case the validation of the key failed</param>
		/// <returns>True after registration, the license is valid</returns>
		public static bool Register(string serial, string key, out string error)
		{
			try
			{
				error = string.Empty;

				if (string.IsNullOrEmpty(key))
				{
					error = "Please load a valid license";
					return false;
				}

				if (!LicenseInitialized)
				{
					error = "Unable to initialize the license engine";
					return false;
				}

				// Register now!
				switch (SafeNativeMethods.SSUser(serial, key, serial))
				{
					case 1:
						switch (SafeNativeMethods.SS_TrialMode())
						{
							case 5:
								error = "This serial key has been already used, you cannot reuse it!";
								return false;

							case 99:
								error = "Date system moved back";
								return false;
						}
						break;

					case 3:
						error = "Invalid serial key license";
						return false;

					case 4:
						break;

					default:
						error = "Please load a valid license";
						return false;
				}

				// Registration worked but double check the new license
				switch (CurrentLicense)
				{
					case LicenseStatus.Expired:
					case LicenseStatus.Corrupted:
					case LicenseStatus.None:
						error = CurrentLicenseDescription;
						return false;
				}

				// Success
				return true;
			}
			catch (Exception e)
			{
				error = String.Format(CultureInfo.InvariantCulture, "Error while applying a new license!\n\n{0}", e);
				return false;
			}
		}

		/// <summary>
		/// Remove license information there...
		/// </summary>
		public static void EnableDemoMode()
		{
			if (LicenseInitialized)
			{
				SafeNativeMethods.SS_RemoveKey();

				// Enter a demo serial
				string error = string.Empty;
				if (!Register(string.Empty, "TL-B76BD47AAGbb24aZKkhXOkzUWAvfdwmblpqpgycP", out error))
				{
					PatternsEngineWrapper.Source.TraceEvent(TraceEventType.Error, 1125, "Unable to set the license to demo mode!\n\n{0}", error);
				}
			}
		}
	}
}
