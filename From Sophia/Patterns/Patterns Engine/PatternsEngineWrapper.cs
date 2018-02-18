using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;
using System.Diagnostics;
using System.Globalization;
using PeriGen.Patterns.Engine.Data;

namespace PeriGen.Patterns.Engine
{
	[SecurityPermissionAttribute(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
	public class PatternsEngineWrapper : IDisposable
	{
		#region Unmanaged DLL encapsulation

		static class SafeNativeMethods
		{
			/// <summary>
			/// Implementation of a Critical handle to properly wrap the unmanaged data pointer returned and used by the DLL
			/// </summary>
			public sealed class EngineCriticalHandle : CriticalHandleZeroOrMinusOneIsInvalid
			{
				/// <summary>
				/// This default ctor will be called by P/Invoke smart marshalling when returning MySafeHandle in a method call 
				/// </summary>
				private EngineCriticalHandle()
				{
					SetHandle((IntPtr)0);
				}

				/// <summary>
				/// Copy constructor. We need this so that we can do our own marshalling (and may be for user supplied handles)
				/// </summary>
				/// <param name="preexistingHandle">Pre existing handle</param>
				internal EngineCriticalHandle(IntPtr preexistingHandle)
				{
					SetHandle(preexistingHandle);
				}

				/// <summary>
				/// Release the internal handle
				/// We should not provide a finalizer - SafeHandle's critical finalizer will call ReleaseHandle inside a CER for us. 
				/// </summary>
				/// <returns></returns>
				override protected bool ReleaseHandle()
				{
					if (!IsInvalid)
					{
						// Release the handle
						SafeNativeMethods.EngineUninitialize(this);

						// Mark the handle as invalid for future users.
						SetHandleAsInvalid();
						return true;
					}

					// Return false. 
					return false;
				}
			}

			[DllImport("PatternsDriver.dll", CallingConvention = CallingConvention.Winapi)]
			public static extern EngineCriticalHandle EngineInitialize();

			[DllImport("PatternsDriver.dll", CallingConvention = CallingConvention.Winapi)]
			public static extern int EngineProcessHR(EngineCriticalHandle param, byte[] signal, int start, int count);

			[DllImport("PatternsDriver.dll", CallingConvention = CallingConvention.Winapi)]
			public static extern int EngineProcessUP(EngineCriticalHandle param, byte[] signal, int start, int count);

			[DllImport("PatternsDriver.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool EngineReadResults(EngineCriticalHandle param, StringBuilder output_buffer, int output_size);

			[DllImport("PatternsDriver.dll", CallingConvention = CallingConvention.Winapi)]
			public static extern void EngineUninitialize(EngineCriticalHandle param);
		}

		#endregion

		SafeNativeMethods.EngineCriticalHandle EngineWorkerHandle;

		/// <summary>
		/// The Start time used to create the engine
		/// </summary>
		public DateTime StartTime { get; protected set; }

		/// <summary>
		/// The current time associated to the next process
		/// </summary>
		public DateTime CurrentTime { get; protected set; }

		#region Constructor and IDisposable implementation

		// This class implement IDisposable, so track if dispose was done
		private bool IsDisposed;

		// For thread security
		private object LockObject = new object();

		/// <summary>
		/// Name used for logging, optional
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="startTime"></param>
		public PatternsEngineWrapper(DateTime startTime)
			: this(startTime, string.Empty)
		{
		}

		/// <summary>
		/// For nice trace logs and easier debugging
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "PatternsEngine {0}", (string.IsNullOrEmpty(this.Name) ? "-" : this.Name));
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="startTime"></param>
		public PatternsEngineWrapper(DateTime startTime, string name)
		{
			this.Name = name;
			this.StartTime = startTime;
			this.CurrentTime = startTime;
			this.EngineWorkerHandle = new SafeNativeMethods.EngineCriticalHandle((IntPtr)0);

			// Trace and counters
			PatternsProcessorWrapper.Source.TraceEvent(TraceEventType.Verbose, 1200, "{0} instanciation at {1:s}", this, startTime);

			// Initialize performance counters
			PerformanceCounterHelper.InitializePerformanceCounters();
		}

		/// <summary>
		/// Default destructor (needed since this class use unmanaged resource)
		/// </summary>
		~PatternsEngineWrapper()
		{
            PatternsProcessorWrapper.Source.TraceEvent(TraceEventType.Verbose, 1201, "{0} disposed", this);
			Dispose(false);
		}

		/// <summary>
		/// Dispose method (Implement IDisposable)
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose method
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			// Reclaim unmanaged resources
			if (!this.IsDisposed)
			{
				this.ReleaseResources();
				this.IsDisposed = true;
			}
		}

		/// <summary>
		/// Close the unmanaged resources, called once
		/// </summary>
		[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
		void ReleaseResources()
		{
			if (!this.EngineWorkerHandle.IsClosed)
			{
				this.EngineWorkerHandle.Close();
			}
		}

		#endregion

		#region Public Control methods

		/// <summary>
		/// Indicates if the unmanaged engine is up
		/// </summary>
		internal bool IsEngineInitialized
		{
			get
			{
				return !this.EngineWorkerHandle.IsInvalid && !this.EngineWorkerHandle.IsClosed;
			}
		}

		/// <summary>
		/// Add the given FHR & UP signal to the engine and detect the events & contractions
		/// </summary>
		/// <param name="hrs">FHR1 signal in 4Hz</param>
		/// <param name="ups">UP signal in 1Hz</param>
		/// <returns></returns>
        public IEnumerable<DetectedObject> Process(byte[] hrs, byte[] ups)
        {
            var candidatesToRet = Process(hrs, ups, 0, ups.GetLength(0));
            var toRet = from c in candidatesToRet
                        where c is DetectedObject
                        select c as DetectedObject;

            return toRet;
        }

		/// <summary>
		/// Add the given FHR & UP signal to the engine and detect the events & contractions
		/// </summary>
		/// <param name="hrs">FHR1 signal in 4Hz</param>
		/// <param name="ups">UP signal in 1Hz</param>
		/// <param name="startIndex">Index to start processing as per the 1Hz signal</param>
		/// <param name="length">Number of seconds of the buffer to process as per the 1Hz signal</param>
		/// <returns></returns>
		public IEnumerable<DetectedObject> Process(byte[] hrs, byte[] ups, int startIndex, int length)
		{
			try
			{
                var results = new List<DetectedObject>();
				if (length <= 0)
				{
					return results;
				}

				// Some validation
				if (hrs == null)
				{
					throw new ArgumentNullException("hrs");
				}
				if (ups == null)
				{
					throw new ArgumentNullException("ups");
				}

				// We want UP in 1Hz and HR in 4Hz and an equal number of seconds of UP and HR!
				if (hrs.GetLength(0) != 4 * ups.GetLength(0))
				{
					throw new InvalidOperationException("Mismatch between FHR and UP signal lengths");
				}

				if ((startIndex < 0) || (ups.GetLength(0) < startIndex + length))
				{
					throw new InvalidOperationException("Out of bound parameters");
				}

				lock (this.LockObject)
				{
					// Start the engine if necessary
					if (!this.IsEngineInitialized)
					{
						this.EngineWorkerHandle = SafeNativeMethods.EngineInitialize();
					}

					// Make sure the engine was successfully started
					if (!this.IsEngineInitialized)
					{
						throw new InvalidOperationException("Unable to initialize the calculation engine");
					}

					// Process the data AS IS
					int buffer_size = 0;

					// Simulate a limited seconds feed of 1 day of tracing at once maximum
					int time_frame = 86400;
					int position = startIndex;
					int end = startIndex + length;
					while (position < end)
					{
						int block_size = Math.Min(time_frame, end - position);

						buffer_size = SafeNativeMethods.EngineProcessUP(this.EngineWorkerHandle, ups, position, block_size);
						buffer_size = SafeNativeMethods.EngineProcessHR(this.EngineWorkerHandle, hrs, 4 * position, 4 * block_size);

						position += time_frame;
					}

					// Read the results
					if (buffer_size > 0)
					{
						var data = new StringBuilder(buffer_size);

						bool moreData = SafeNativeMethods.EngineReadResults(this.EngineWorkerHandle, data, buffer_size);
						Debug.Assert(!moreData);

						string[] lines = data.ToString().Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

						foreach (string line in lines)
						{
							var artifact = line.ToDetectionArtifact(this.StartTime);
							if (artifact != null)
							{
								results.Add(artifact);
							}
						}
					}

					// Count Artifacts
					PerformanceCounterHelper.AddArtifactsFound(results.Count);

					// Count Seconds
					PerformanceCounterHelper.AddSecondsOfTracingProcessed(length);

					// Remember current time possition
					this.CurrentTime = this.CurrentTime.AddSeconds(length);

					return results;
				}
			}
			catch (Exception ex)
			{
                PatternsProcessorWrapper.Source.TraceEvent(TraceEventType.Error, 1299, "{0} [Process Method]\n:{0}", ex);
				throw;
			}
		}

		#endregion
	}
}
