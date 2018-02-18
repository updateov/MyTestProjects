using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.ServiceProcess;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Resources;

namespace PeriGen.Patterns.Service
{
	static class Program
	{
        private const string PeriGenDefaultCertificateThumbprint = "8b6de359868d59b66e83c7e0b3c39271f4d157b8";
		/// <summary>
		/// The main entry point for the application.
		/// </summary>		
		static void Main(string[] args)
		{
            try
            {
                // Validate and Install certificate if it is necessary
                ValidatePeriGenCertificateInstalled();

                // Log certificate status
                Console.WriteLine("PeriGen Settings Certificate validated/installed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Error.\nDetails: {0}", ex.ToString()));
                return;
            }

			if (Environment.UserInteractive && args.Length > 0)
			{
				string parameter;

				// /help
				parameter = (from h in args where h.ToUpperInvariant() == "/HELP" select h).FirstOrDefault();
				if (!string.IsNullOrEmpty(parameter))
				{
					DisplayHelp();
					return;
				}

				// /certificate
				parameter = (from c in args where c.ToUpperInvariant() == "/CERTIFICATE" select c).FirstOrDefault();
				if (!string.IsNullOrEmpty(parameter))
				{
					try
					{
						// Validate and Install certificate if it is necessary
						ValidatePeriGenCertificateInstalled();

						// Log certificate status
						Console.WriteLine("PeriGen Settings Certificate validated/installed.");
					}
					catch (Exception ex)
					{
						Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Error.\nDetails: {0}", ex.ToString()));
						return;
					}
				}

				// /counter
				parameter = (from c in args where c.ToUpperInvariant() == "/COUNTER" select c).FirstOrDefault();
				if (!string.IsNullOrEmpty(parameter))
				{
					PeriGen.Patterns.Service.PerformanceCounterHelper.InstallPerformanceCounters();
					PeriGen.Patterns.Engine.PerformanceCounterHelper.InstallPerformanceCounters();

					// Log certificate status
					Console.WriteLine("Performance counters installed.");
				}

				// /counter
				parameter = (from c in args where c.ToUpperInvariant() == "/EVENTLOG" select c).FirstOrDefault();
				if (!string.IsNullOrEmpty(parameter))
				{
					EventLog.DeleteEventSource("PeriGen Patterns Service");
					EventLog.CreateEventSource("PeriGen Patterns Service", "Application");
					EventLog.DeleteEventSource("PeriGen Patterns Engine");
					EventLog.CreateEventSource("PeriGen Patterns Engine", "Application");

					// Log certificate status
					Console.WriteLine("EventLog source recreated.");
				}

				// /console
				parameter = (from c in args where c.ToUpperInvariant() == "/CONSOLE" select c).FirstOrDefault();
				if (!string.IsNullOrEmpty(parameter))
				{
					using (var service = new PatternsService())
					{
						try
						{
							Console.WriteLine("Starting service...");
							service.StartTask();
						}
						catch (Exception ex)
						{
							Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Error. Service '{0}' cannot be started.\nDetails: {1}", PatternsService.PatternsServiceName, ex.ToString()));
							return;
						}

						Console.WriteLine("Service started!");
						Console.WriteLine("Press <ENTER> to stop the service...");
						Console.Read();
						try
						{
							Console.WriteLine("Stopping service...");
							service.StopTask();
							Console.WriteLine("Service Stopped!");
						}
						catch (Exception ex)
						{
							Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Error. Service '{0}' cannot be stopped.\nDetails: {1}", PatternsService.PatternsServiceName, ex.ToString()));
						}
						return;
					}
				}
			}
			else
			{
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[] { new PatternsService() };
				ServiceBase.Run(ServicesToRun);
			}
		}

		/// <summary>
		/// Display help for command line use of the service
		/// </summary>
		private static void DisplayHelp()
		{
			Console.WriteLine("Syntax: ");
			Console.WriteLine("");

			Console.WriteLine("PeriGen.Patterns.Service.exe {[/console]|[/help][/certificate][/counter][/eventlog]}");
			Console.WriteLine("");
			Console.WriteLine("/console     run service in debug mode");
			Console.WriteLine("/help        display this help");
			Console.WriteLine("/certificate validate/install certificate");
			Console.WriteLine("/counter     re-create the performance counters");
			Console.WriteLine("/eventlog    re-register the eventlog source");

			Console.WriteLine("");
		}

        /// <summary>
        /// Validate and install certificate for data encryption
        /// </summary>
        private static void ValidatePeriGenCertificateInstalled()
        {
            // Check if certificate exist
            if (CertificateIsValid(PeriGenDefaultCertificateThumbprint))
                return;

            // It does not exist, try to install it
            try
            {
                Byte[] cert = Properties.Resource.PeriGenSettingsCertificate;

                if (cert == null) 
                    throw new InvalidDataException("Application cannot find the resource PeriGenSettingsCertificate.");

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
        private static bool CertificateIsValid(string thumbprint)
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
