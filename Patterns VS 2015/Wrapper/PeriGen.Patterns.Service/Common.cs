using System.Diagnostics;
using System.Net;
using System.Net.Cache;

namespace PeriGen.Patterns.Service
{
	/// <summary>
	/// Simple singleton implementation with by default behavior
	/// </summary>
    internal static class Common
    {
		/// <summary>
		/// In demo mode or registered mode?
		/// </summary>
		public static bool DemoMode { get; set; }

        /// <summary>
        /// Chalkboard
        /// </summary>
        public static PatternsChalkboard Chalkboard { get; set; }

		/// <summary>
		/// The ONE source to use to trace data (Windows event log / DebugView...)
		/// </summary>
		public static TraceSource Source = new TraceSource("PatternsService");

		/// <summary>
		/// Statis constructor to set some configuration values
		/// </summary>
		static Common()
		{
			// This ensure we don't use cache when querying OBLink
			WebRequest.DefaultCachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
			HttpWebRequest.DefaultCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

			// Avoid validating wrong/self https certificates 
			ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };

			// This avoid frequently time out baucause the use of multiples connections. .NET has a default limit of 2. 
			// We must override this limit for all connections by code or config. Unfortunately, config modifications work only in sites
			// with asp.net apps. Therefore, we must do it manually. The max recommended value is 12 times the processors in the pc
			ServicePointManager.DefaultConnectionLimit = PatternsServiceSettings.Instance.DefaultConnectionLimit;
			ServicePointManager.MaxServicePointIdleTime = PatternsServiceSettings.Instance.MaxServicePointIdleTime;
		}
    }
}
