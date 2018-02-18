using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Net;
using System.Diagnostics;
using PeriGen.Patterns.Settings;

namespace PeriGen.Patterns.GE.Interface.TracingServices
{
	/// <summary>
	/// The base class for reading tracings from GE OBLink
	/// </summary>
	public abstract class TracingProcessorBase : IDisposable
	{
		#region Settings

		internal static DateTime Settings_OBLinkTracingReferenceTime = DateTime.ParseExact(SettingsManager.GetValue("OBLinkTracingReferenceTime"), "s", CultureInfo.InvariantCulture);
		internal static DateTime Settings_OBLinkTracingReferenceTimeEDT = Settings_OBLinkTracingReferenceTime.Subtract(TimeZoneInfo.Local.BaseUtcOffset);

		internal static string Settings_OBLinkURL = SettingsManager.GetValue("OBLinkURL");
		internal static string Settings_OBLinkPatientTracingsRequestUrl = SettingsManager.GetValue("OBLinkPatientTracingsRequestUrl");

		internal static int Settings_HistoricalTracingMaximumDuration = SettingsManager.GetInteger("HistoricalTracingMaximumDuration");
		internal static int Settings_HistoricalTracingHasMoreTracingThreshold = SettingsManager.GetInteger("HistoricalTracingHasMoreTracingThreshold");
		internal static int Settings_HistoricalTracingBlockDuration = SettingsManager.GetInteger("HistoricalTracingBlockDuration");
		internal static int Settings_HistoricalTracingOpenTimeout = SettingsManager.GetInteger("HistoricalTracingOpenTimeout");
		internal static int Settings_HistoricalTracingReadTimeout = SettingsManager.GetInteger("HistoricalTracingReadTimeout");
		internal static int Settings_HistoricalTracingRetryLimit = SettingsManager.GetInteger("HistoricalTracingRetryLimit");
		internal static int Settings_HistoricalTracingOverlapDuration = SettingsManager.GetInteger("HistoricalTracingOverlapDuration");

		internal static int Settings_LiveTracingMaximumDuration = SettingsManager.GetInteger("LiveTracingMaximumDuration");
		internal static int Settings_LiveTracingOverlapDuration = SettingsManager.GetInteger("LiveTracingOverlapDuration");
		internal static int Settings_LiveTracingOpenTimeout = SettingsManager.GetInteger("LiveTracingOpenTimeout");
		internal static int Settings_LiveTracingReadTimeout = SettingsManager.GetInteger("LiveTracingReadTimeout");

		internal static int Settings_TracingMergeBridgeableGap = SettingsManager.GetInteger("TracingMergeBridgeableGap");

		internal static bool Settings_TraceExtendedInformationWithPHIData = SettingsManager.GetBoolean("TraceExtendedInformationWithPHIData");

		#endregion

		#region Constants for GE OBLink Time conversion

		/// <summary>
		/// Calculate real time based in GE time
		/// </summary>
		/// <param name="inTime"></param>
		/// <returns></returns>
		protected static DateTime QSTimeToDateTime(long inTime)
		{
			return Settings_OBLinkTracingReferenceTimeEDT.AddSeconds(inTime);
		}

		/// <summary>
		/// Calculate real time based in GE time
		/// </summary>
		/// <param name="inTime"></param>
		/// <returns></returns>
		protected static long DateTimeToQSTime(DateTime inTime)
		{
			return Math.Max(0, (long)((inTime - Settings_OBLinkTracingReferenceTimeEDT).TotalSeconds));
		}

		#endregion

		#region Helper for reading QSLink response

		/// <summary>
		/// The request for data to OBLink
		/// </summary>
		protected HttpWebRequest Request { get; set; }

		/// <summary>
		/// The request answer from OBLink
		/// </summary>
		protected WebResponse Response { get; set; }

		/// <summary>
		/// A stream on the data returned from OBLink
		/// </summary>
		protected Stream InputStream { get; set; }

		/// <summary>
		/// Open the connection to the server
		/// </summary>
		/// <param name="url">url of the requested data</param>
		/// <param name="requestTimeout">timeout for request</param>
		/// <param name="readTimeout">timeout for read operations</param>
		/// <returns>True if succesfully opened</returns>
		protected bool OpenConnection(Uri url, int requestTimeout, int readTimeout)
		{
			// Clean up previous connection if any
			this.ReleaseResources();

			try
			{
				// Create initial request
				this.Request = WebRequest.Create(url) as HttpWebRequest;
				this.Request.ReadWriteTimeout = readTimeout;
				this.Request.Timeout = requestTimeout;

				// Request!
				this.Response = this.Request.GetResponse();
				this.InputStream = this.Response.GetResponseStream();

				return true;
			}
			catch (Exception e)
			{
				this.ReleaseResources();

				if (e.Message.Contains("The server committed a protocol violation"))
				{
					if (Settings_TraceExtendedInformationWithPHIData)
					{
						// This is typical of no tracings there, not really an error
						Common.Source.TraceEvent(TraceEventType.Verbose, 6101, "Tracing: unable to open connection to read tracing for {0}.\nThere must be no tracing at all after {1:s}.\nURL: {2}", this, this.StartTime, url);
					}
					else
					{
						// This is typical of no tracings there, not really an error
						Common.Source.TraceEvent(TraceEventType.Verbose, 6101, "Tracing: unable to open connection to read tracing for {0}.\nThere must be no tracing at all after {1:s}.", this, this.StartTime);
					}
					return false;
				}
				throw;
			}
		}

		/// <summary>
		/// Read data from the input stream
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="count"></param>
		/// <returns>How many bytes really read</returns>
		protected int Read(byte[] buffer, int count)
		{
			// Flush previous buffer content
			Array.Clear(buffer, 0, count);

			// Read the requested bits
			int nRead = 0;
			while (nRead < count)
			{
				try
				{
					int actualRead = this.InputStream.Read(buffer, nRead, count - nRead);
					if (actualRead == -1)
					{
						// End of stream!
						break;
					}
					nRead += actualRead;
				}
				catch (WebException)
				{
					if (nRead == 0)
					{
						// End of stream (the connection is closed by the server when all data is sent
						return 0;
					}
					throw;
				}
				catch (IOException)
				{
					if (nRead == 0)
					{
						// End of stream (the connection is closed by the server when all data is sent
						return 0;
					}
					throw;
				}
			}

			return nRead;
		}

		/// <summary>
		/// Read data from the input stream
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="count"></param>
		protected void Skip(int count)
		{
			for (int i = 0; i < count; ++i)
			{
				if (this.InputStream.ReadByte() == -1)
				{
					// End of stream!
					throw new EndOfStreamException();
				}
			}
		}

		/// <summary>
		/// Read a 32-bit integer from a buffer at a given index
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		protected static int GetInteger32(byte[] buffer, int offset)
		{
			return (((buffer[offset + 3] & 0xff) << 24) | ((buffer[offset + 2] & 0xff) << 16) | ((buffer[offset + 1] & 0xff) << 8) | ((buffer[offset] & 0xff)));
		}

		/// <summary>
		/// Read a 16-bit integer from a buffer at a given index
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		protected static int GetInteger16(byte[] buffer, int offset)
		{
			return (((buffer[offset + 1] & 0xff) << 8) | (buffer[offset] & 0xff));
		}

		/// <summary>
		/// Read a 8-bit integer from a buffer at a given index
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		protected static int GetInteger8(byte[] buffer, int offset)
		{
			return (buffer[offset] & 0xff);
		}

		#endregion

		#region Members

		/// <summary>
		/// The patient unique ID
		/// </summary>
		public int PatientUniqueID { get; protected set; }

		/// <summary>
		/// The bed ID
		/// </summary>
		public string BedID { get; protected set; }

		/// <summary>
		/// The patient ID
		/// </summary>
		public string PatientID { get; protected set; }

		/// <summary>
		/// The start time of the request
		/// </summary>
		public DateTime StartTime { get; protected set; }

		/// <summary>
		/// The end time of the last block of tracing read
		/// </summary>
		public DateTime LastTracingTime { get; protected set; }

		#endregion

		#region Methods

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="patient_uniqueid"></param>
		/// <param name="patient_id"></param>
		/// <param name="bed_id"></param>
		/// <param name="start"></param>
		protected TracingProcessorBase(int patient_uniqueid, string patient_id, string bed_id, DateTime start)
		{
			this.PatientUniqueID = patient_uniqueid;
			this.PatientID = patient_id;
			this.BedID = bed_id;

			// Align start to the minute
			this.StartTime = start;
			this.LastTracingTime = this.StartTime;
		}

		/// <summary>
		/// Check if the buffer contains an error return string... (HTTP...)
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		protected static bool IsDataHeader(byte[] buffer)
		{
			// In case of an error, the server return a string starting with 'http'
			return !((buffer[0] == 'H') || (buffer[0] == 'h'))
						&& ((buffer[1] == 'T') || (buffer[1] == 't'))
						&& ((buffer[2] == 'T') || (buffer[2] == 't'))
						&& ((buffer[3] == 'P') || (buffer[3] == 'p'));
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Destructor
		/// </summary>
		~TracingProcessorBase()
		{
			this.Dispose(false);
		}

		/// <summary>
		/// To remember if it was already disposed
		/// </summary>
		private bool IsDisposed;

		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this.IsDisposed)
			{
				if (disposing)
				{
					this.ReleaseResources();
				}

				this.IsDisposed = true;
			}
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Release resources and cleanup
		/// </summary>
		protected void ReleaseResources()
		{
			// Abort the request in case it's still active
			if (this.Request != null)
			{
				this.Request.Abort();
				this.Request = null;
			}

			// Release the stream
			if (this.InputStream != null)
			{
				this.InputStream.Close();
				this.InputStream = null;
			}
			
			// Release the response
			if (this.Response != null)
			{
				this.Response.Close();
				this.Response = null;
			}
		}

		#endregion

	}
}
