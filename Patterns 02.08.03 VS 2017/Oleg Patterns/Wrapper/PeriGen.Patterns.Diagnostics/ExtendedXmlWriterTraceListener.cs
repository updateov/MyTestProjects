using System;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Runtime.Versioning;

namespace PeriGen.Patterns.Diagnostics
{
	/// <summary>
	/// An extended XmlWriterTraceListener that will segment the output file to a maximum size
	/// </summary>
	[System.Security.Permissions.HostProtection(Synchronization = true)]
	public class ExtendedXmlWriterTraceListener : XmlWriterTraceListener
	{
		/// <summary>
		/// The base filename since we don't have access to the base filename instance...
		/// </summary>
		string DefaultFileName { get; set; }

		/// <summary>
		/// Last file name generated using the template
		/// </summary>
		string LastFileName { get; set; }

		/// <summary>
		/// Ctr
		/// </summary>
		/// <param name="fileName"></param>
		[ResourceExposure(ResourceScope.Machine)]
		[ResourceConsumption(ResourceScope.Machine)]
		public ExtendedXmlWriterTraceListener(string fileName)
			: base(fileName)
		{
			// Make sure the path exists
			try
			{
				//Ensure get full install directory for the log
				var combinedFileAndFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

				//get full path including filename
				this.DefaultFileName = Path.GetFullPath(combinedFileAndFolder);

				//create directory if it is not exist
				Directory.CreateDirectory(Path.GetDirectoryName(this.DefaultFileName));
			}
			catch (Exception e)
			{
				// We cannot fail here of else if will fail inside the calling assembly!
				Debug.Assert(false, e.Message);
			}
		}

		/// <summary>
		/// Ctr
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="name"></param>
		[ResourceExposure(ResourceScope.Machine)]
		[ResourceConsumption(ResourceScope.Machine)]
		public ExtendedXmlWriterTraceListener(string fileName, string name)
			: base(fileName, name)
		{
			this.DefaultFileName = Path.GetFullPath(fileName);

			// Make sure the path exists
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(this.DefaultFileName));
			}
			catch (Exception e)
			{
				// We cannot fail here of else if will fail inside the calling assembly!
				Debug.Assert(false, e.Message);
			}
		}

		#region Overrides

		public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, int id, string format, params object[] args)
		{
			this.StartNewTraceFileIfNecessary();
			base.TraceEvent(eventCache, source, eventType, id, format, args);
		}

		public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, int id, string message)
		{
			this.StartNewTraceFileIfNecessary();
			base.TraceEvent(eventCache, source, eventType, id, message);
		}

		public override void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, int id, object data)
		{
			this.StartNewTraceFileIfNecessary();
			base.TraceData(eventCache, source, eventType, id, data);
		}

		public override void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, int id, params object[] data)
		{
			this.StartNewTraceFileIfNecessary();
			base.TraceData(eventCache, source, eventType, id, data);
		}

		public override void TraceTransfer(TraceEventCache eventCache, String source, int id, string message, Guid relatedActivityId)
		{
			this.StartNewTraceFileIfNecessary();
			base.TraceTransfer(eventCache, source, id, message, relatedActivityId);
		}

		#endregion

		#region New attributes

		/// <summary>
		/// Gets the custom attributes supported by the trace listener.
		/// </summary>
		/// <returns></returns>
		protected override string[] GetSupportedAttributes()
		{
			return new string[2] { "MaximumFileSize", "MaximumFileCount" };
		}

		long _MaximumFileSize = -1;

		/// <summary>
		/// Gets or sets the maximum size of the trace file.
		/// </summary>
		public long MaximumFileSize
		{
			get
			{
				// Load from attributes
				if ((this._MaximumFileSize < 0) && (this.Attributes.ContainsKey("MaximumFileSize")))
				{
					long value;
					if (long.TryParse(this.Attributes["MaximumFileSize"], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
					{
						this._MaximumFileSize = value;
					}
				}

				// Defaulted in case...
				if (this._MaximumFileSize < 0)
				{
					this._MaximumFileSize = 52428800; // 50 MB
				}

				return this._MaximumFileSize;
			}

			set
			{
				this._MaximumFileSize = value;
			}
		}

		int _MaximumFileCount = -1;

		/// <summary>
		/// Gets or sets the maximum size of the trace file.
		/// </summary>
		public int MaximumFileCount
		{
			get
			{
				// Load from attributes
				if ((this._MaximumFileCount < 0) && (this.Attributes.ContainsKey("MaximumFileCount")))
				{
					int value;
					if (int.TryParse(this.Attributes["MaximumFileCount"], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
					{
						this._MaximumFileCount = value;
					}
				}

				// Defaulted in case...
				if (this._MaximumFileCount < 0)
				{
					this._MaximumFileCount = 10;
				}

				return this._MaximumFileCount;
			}

			set
			{
				this._MaximumFileCount = value;
			}
		}

		#endregion

		private static Encoding GetEncodingWithFallback(Encoding encoding)
		{
			// Clone it and set the "?" replacement fallback 
			Encoding fallbackEncoding = (Encoding)encoding.Clone();
			fallbackEncoding.EncoderFallback = EncoderFallback.ReplacementFallback;
			fallbackEncoding.DecoderFallback = DecoderFallback.ReplacementFallback;

			return fallbackEncoding;
		}

		/// <summary>
		/// Causes the writer to start a new trace file with an increased number in the file names suffix
		/// </summary>
		void StartNewTraceFileIfNecessary()
		{
			try
			{
				// If there is no LastFilename, that means it's the firt time ever it's called so no need to check
				if (!string.IsNullOrEmpty(this.LastFileName))
				{
					// Check if we need to do it then
					var stream = ((this.Writer as StreamWriter).BaseStream) as FileStream;

					// Do we need to change file?
					if (stream != null)
					{
						var file = new FileInfo(stream.Name);
						if ((file.Exists) && (file.Length < this.MaximumFileSize))
							return;

						this.Flush();
						this.Close();
					}
				}

				// Delete old files if too many of them
				var files = Directory.GetFiles(Path.GetDirectoryName(this.DefaultFileName), string.Format(CultureInfo.InvariantCulture, "{0}*{1}", Path.GetFileNameWithoutExtension(this.DefaultFileName), Path.GetExtension(this.DefaultFileName))).Select(f => new FileInfo(f)).OrderBy(f => f.CreationTimeUtc).ToList();
				if (files.Count >= this.MaximumFileCount)
				{
					var filesToDelete = files.Take(files.Count - (this.MaximumFileCount - 1)).ToList();
					foreach (var f in filesToDelete)
					{
						try
						{
							f.Delete();
						}
						catch (Exception e)
						{
							// We cannot fail here of else if will fail inside the calling assembly!
							Debug.Assert(false, e.Message);
						}
					}
				}

				// Create a new file stream and a new stream writer and pass it to the listener
				this.LastFileName = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(this.DefaultFileName), string.Format(CultureInfo.InvariantCulture, "{0}.{1:yyyyMMdd-HHmmss}{2}", Path.GetFileNameWithoutExtension(this.DefaultFileName), DateTime.UtcNow, Path.GetExtension(this.DefaultFileName))));
				this.Writer = new StreamWriter(this.LastFileName, true, GetEncodingWithFallback(new UTF8Encoding(false)), 4096);
			}
			catch (Exception e)
			{
				// We cannot fail here of else if will fail inside the calling assembly!
				Debug.Assert(false, e.Message);
			}
		}
	}
}
