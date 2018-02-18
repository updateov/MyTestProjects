using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Globalization;
using PeriGen.Patterns.GE.Interface.Demographics;

namespace PeriGen.Patterns.GE.Interface
{
	public static class Common
	{
		/// <summary>
		/// Statis constructor to set some configuration values
		/// </summary>
		static Common()
		{
			// This ensure we don't use cache when querying OBLink
			WebRequest.DefaultCachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
			HttpWebRequest.DefaultCachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);
			
			///Avoid validating wrong/self https certificates 
			ServicePointManager.ServerCertificateValidationCallback += delegate{return true;};

			///This avoid frequently time out baucause the use of multiples connections. .NET has a default limit of 2. 
			///We must override this limit for all connections by code or config. Unfortunately, config modifications work only in sites
			///with asp.net apps. Therefore, we must do it manually. The max recommended value is 12 times the processors in the pc
			System.Net.ServicePointManager.DefaultConnectionLimit = Settings.SettingsManager.GetInteger("DefaultConnectionLimit"); 
			System.Net.ServicePointManager.MaxServicePointIdleTime = Settings.SettingsManager.GetInteger("MaxServicePointIdleTime");

			// Create the source
			try
			{
				if (!EventLog.SourceExists("PeriGen Patterns Interface"))
				{
					EventLog.CreateEventSource("PeriGen Patterns Interface", "Application");
				}
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Warning, 6501, "Warning, unable to create the log source.\n{0}", e);
			}
		}

		/// <summary>
		/// The source for logging
		/// </summary>
		internal static TraceSource Source = new TraceSource("PatternsInterface");

		/// <summary>
		/// Create a request based in the urls. 
		/// It is used basically to create connections to get the list of patients, trail and no trail information
		/// </summary>
		/// <param name="Url"></param>
		/// <returns></returns>
		public static HttpWebRequest CreateRequest(Uri Url)
		{
			HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;

			request.Timeout = 1000;
			request.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);
			request.KeepAlive = false;
			request.Proxy = null;
			request.Headers.Add("Pragma", "no-cache");
			return request;
		}

	}
}
