using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

namespace PeriGen.Patterns.WebServiceManagement
{
	namespace Factory
	{
		/// <summary>
		/// Delegate uses as a signature to execute the methods in teh service
		/// </summary>
		/// <typeparam name="T">Type of service interface</typeparam>
		/// <param name="proxy"></param>
		public delegate void ExecuteServiceDelegate<T>(T proxy);

		/// <summary>
		/// Class that contains parameters used to build the service proxy
		/// </summary>        
		public class ServiceProxyArgs
		{
			public ServiceProxyArgs()
			{
				Address = String.Empty;
				CertificateHash = string.Empty;
				HeaderValues = new Dictionary<string, object>();
			}
			public string Address { get; set; }
			public string CertificateHash { get; set; }
			public Dictionary<string, object> HeaderValues { get; private set; }
		}

		/// <summary>
		/// Generic Service Proxy 
		/// </summary>
		/// <typeparam name="T">Type Interface</typeparam>
		public static class ServiceProxy<T>
		{
			/// <summary>
			/// Execute service method declared in the interface T
			/// </summary>
			/// <param name="serviceArgs">parameters used to build the service</param>
			/// <param name="method">Method to execute in the service</param>
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
			public static void Execute(ServiceProxyArgs serviceArgs, ExecuteServiceDelegate<T> method)
			{
				//Avoid issues with certificates
				ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });

				//Check Security
				var isSecure = serviceArgs.Address.ToUpperInvariant().StartsWith("HTTPS");

				//Binding
				WSHttpBinding wsBinding = new WSHttpBinding();

				if (isSecure)
				{
					wsBinding.Security.Mode = SecurityMode.Transport;
					wsBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
				}
				else
				{
					wsBinding.Security.Mode = SecurityMode.None;
				}

				wsBinding.MaxReceivedMessageSize = 2147483647;
				wsBinding.ReaderQuotas.MaxArrayLength = 2147483647;
				wsBinding.ReaderQuotas.MaxStringContentLength = 2147483647;
				wsBinding.ReaderQuotas.MaxBytesPerRead = 2147483647;
				wsBinding.CloseTimeout = new TimeSpan(0, 1, 0);
				wsBinding.OpenTimeout = new TimeSpan(0, 1, 0);
				wsBinding.ReceiveTimeout = new TimeSpan(0, 3, 0);
				wsBinding.SendTimeout = new TimeSpan(0, 1, 0);

				//Endpoint
				EndpointAddress ep = new EndpointAddress(serviceArgs.Address);


				//Create Channel
				ChannelFactory<T> myChannel = new ChannelFactory<T>();

				//----- IMPORTANT! To avoid Caching!!!! --------------------
				//----- DO NOT use ChannelFactory Ctor with this properties: 
				//----- Use Properties!! -----------------------------------
				myChannel.Endpoint.Binding = wsBinding;
				myChannel.Endpoint.Address = ep;
				//---------------------------------------------------------------				

				//Add Client Certificate for authentication if it is secure conexion
				if (isSecure)
				{
					var hash = serviceArgs.CertificateHash;
					myChannel.Credentials.ClientCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindByThumbprint, hash);
				}

				//Proxy, client
				T proxy = myChannel.CreateChannel();

				try
				{
					((IClientChannel)proxy).Open();
					using (OperationContextScope scope = new OperationContextScope((IClientChannel)proxy))
					{
						//Add header values
						foreach (var item in serviceArgs.HeaderValues)
						{
							HeaderManager.AddHeader(item.Key, item.Value);
						}

						method((T)proxy);
					}
					((IClientChannel)proxy).Close();
				}
				catch (Exception)
				{
					if (proxy != null)
					{
						if (((IClientChannel)proxy).State != CommunicationState.Closing &&
							((IClientChannel)proxy).State != CommunicationState.Closed)
						{
							((IClientChannel)proxy).Abort();
						}
					}
					throw;

				}
				finally
				{

					//close channel also
					if (myChannel != null)
					{
						if (myChannel.State != CommunicationState.Closed && myChannel.State != CommunicationState.Closing)
						{
							myChannel.Abort();
						}
					}

					///Set null values
					proxy = default(T);
					myChannel = null;
					wsBinding = null;

					///Return to default value
					ServicePointManager.ServerCertificateValidationCallback = null;

					///Collect garbage
					GC.Collect();
					GC.WaitForPendingFinalizers();
				}
			}
		}
	}
}
