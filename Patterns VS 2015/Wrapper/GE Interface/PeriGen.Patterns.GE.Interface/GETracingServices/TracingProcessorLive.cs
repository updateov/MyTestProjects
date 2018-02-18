using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeriGen.Patterns.Engine.Data;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Net;

namespace PeriGen.Patterns.GE.Interface.TracingServices
{
	/// <summary>
	/// Class that reads the live tracing data from GE OBLink
	/// </summary>
	public class TracingProcessorLive : TracingProcessorBase
	{
		#region Constants for GE OBLink data

		// Return Codes
		const int C_NORMAL = 10001;
		const int C_ALL_DATA = 19002; // Caught up to real time.
		const int C_CONNECTING = 19003; // QS in blue screen "Getting data"
		const int C_CONNECT_ERROR = 19004;
		const int C_NO_DATA = 19005; // No data available at this time.
		const int C_NOT_CONNECTED = 19006;
		const int C_POWER_OFF = 19007; // Fetal monitor has been powered off.
		const int C_PT_ID_CHANGE = 19008; // Probable new patient.

		// PMS Header
		const int BED_ID_OFFSET = 2;
		const int TIME_OFFSET = 8;
		const int EDATA_OFFSET = 12;
		const int HEADER_LENGTH = 13;
		const int FM_NOTE_LIMIT = 5000;

		// eData options
		const int PMS_ECG12SL = 0;
		const int PMS_FM_NOTE = 1;
		const int PMS_STATUS = 2;
		const int PMS_HISTORY_EVENT = 3;
		const int PMS_PARAMETER_DATA = 4;
		const int PMS_PATIENT_ID = 5;
		const int PMS_STORE_CHUNK = 6;
		const int PMS_WAVE_DATA = 7;
		const int PMS_RETURN_CODE = 14;

		// WAVE_DATA types (at TYPE_OFFSET)
		const int WAVE_FHR1 = 0;
		const int WAVE_FHR2 = 1;
		const int WAVE_MHR = 2;
		const int WAVE_UA = 3;

		// WAVE_DATA header
		const int DATA_LEN_OFFSET = 30; // Into waveHeader
		const int WAVE_HEADER_LENGTH = 32;

		// WAVE_DATA
		const int N_WAVES = 4;
		const int NUM_SAMPLES_OFFSET = 1;
		const int F_WAVE_CHUNK_OFFSET = 3;
		const int MAXIMUM_NUM_SAMPLES = 240;
		const int MAXIMUM_R_WAVE_DATA = F_WAVE_CHUNK_OFFSET + MAXIMUM_NUM_SAMPLES;

		// FM_STATUS
		const int STAT_SET = 0;
		const int HR1_MODE = 3;
		const int HR2_MODE = 4;
		const int SP_DATA = 7;
		const int FM_STATUS_SIZE = 9;
		const int RETURN_CODE_SIZE = 2;
		
		// STAT_SET bits
		const int FM_STAT_BUFFER_OVERFLOW = 0x01;
		const int FM_STAT_POWER_OFF = 0x02;
		const int FM_STAT_POWER_ON = 0x04;
		const int FM_STAT_MATERNAL_ONLY_MODE = 0x08;

		// Maternal mode values (HR1_MODE, HR2_MODE)
		const int FM_STAT_MODE_MECG = 5;
		const int FM_STAT_MODE_EXT_MHR = 9;
		const int FM_STAT_MODE_MAECG = 12;

		#endregion

		#region Buffers

		byte[] ReadBuffer = new byte[4000];

		#endregion

		/// <summary>
		/// Ctr
		/// </summary>
		/// <param name="patient_uniqueid"></param>
		/// <param name="patient_id"></param>
		/// <param name="bed_id"></param>
		/// <param name="start"></param>
		public TracingProcessorLive(int patient_uniqueid, string patient_id, string bed_id, DateTime start)
			: base(patient_uniqueid, patient_id, bed_id, start)
		{
			// We can only go back a certain duration...
			if ((start == DateTime.MinValue) || (DateTime.UtcNow - start).TotalSeconds > Settings_LiveTracingMaximumDuration)
			{
				this.StartTime = DateTime.UtcNow.AddSeconds(-Settings_LiveTracingMaximumDuration);
			}
			else
			{
				// We need a small overlap to be sure we don't have a problem with gap
				this.StartTime = start.AddSeconds(-Settings_LiveTracingOverlapDuration);
			}
		}

		/// <summary>
		/// Open the connection
		/// </summary>
		/// <returns>True if successful</returns>
		public bool Open()
		{
			try
			{
				// Safety first
				if (string.IsNullOrEmpty(this.BedID))
					return false;

				/////////////////////// Build the url for the request
				long block_duration = Math.Max(0, (long)(DateTime.UtcNow - this.StartTime).TotalSeconds);
				long block_start = DateTimeToQSTime(this.StartTime) + block_duration; // For OBLink, the start time is actually the end time of the block you want
				
				var relative_url = string.Format(CultureInfo.InvariantCulture,
														Settings_OBLinkPatientTracingsRequestUrl,
														this.BedID, "FALSE", block_duration, this.PatientID, block_start);

				/////////////////////// Connect to OBLink and open the connection
				if (this.OpenConnection(new Uri(new Uri(Settings_OBLinkURL), relative_url), Settings_LiveTracingOpenTimeout, Settings_LiveTracingReadTimeout))
				{
					Common.Source.TraceEvent(TraceEventType.Verbose, 6301, "Tracing: (Live) Start listening for live tracing for {0}", this);
					return true;
				}
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Verbose, 6302, "Tracing: (Live) Error while opening the live tracing connection for {0}!\n{1}", this, e);
				this.ReleaseResources();
			}
			return false;
		}

		/// <summary>
		/// Set to true if GE signal that the patient changed or monitor was turned off... 
		/// It means that the tracing collection should not resume til the chalkboard is refreshed
		/// and the location and patient ID of the patient is properly validated
		/// </summary>
		public bool NeedChalkboardRefresh { get; protected set; }

		// Test for the connection status
		public bool IsConnected { get { return this.InputStream != null; } }

		// The FHR1 signal can be in HR1 or HR2 data block depending on the probe...
		int HR1ProbeSignal = -1;

		/// <summary>
		/// Get a complete message from the memory buffer if there is one
		/// </summary>
		/// <returns></returns>
		public List<TracingBlock> ReadTracing()
		{
			var results = new List<TracingBlock>();
			try
			{
				if (this.Read(this.ReadBuffer, HEADER_LENGTH) != HEADER_LENGTH)
					throw new Exception("Unable to read the header");

				// Check for error
				if (IsDataHeader(this.ReadBuffer))
					throw new Exception("Server returned an error!");

				// Extract the fields we need.
				var displayTime = GetInteger32(this.ReadBuffer, TIME_OFFSET);
				var bedId = GetInteger16(this.ReadBuffer, BED_ID_OFFSET);
				if (string.CompareOrdinal(this.BedID, bedId.ToString(CultureInfo.InvariantCulture)) != 0)
					throw new Exception("Transfer detected!");

				// Check the type of data
				var eDataType = GetInteger8(this.ReadBuffer, EDATA_OFFSET);

				// Depending on the message type, the required length is different
				switch (eDataType)
				{
					case PMS_STATUS:
						{
							if (this.Read(this.ReadBuffer, FM_STATUS_SIZE) != FM_STATUS_SIZE)
								throw new Exception("Unable to read the FM status");

							// Extract useful info from the status
							int statSet = GetInteger8(this.ReadBuffer, STAT_SET);
							int hr1Mode = GetInteger8(this.ReadBuffer, HR1_MODE);
							int hr2Mode = GetInteger8(this.ReadBuffer, HR2_MODE);
							int spData = GetInteger8(this.ReadBuffer, SP_DATA);

							bool monitorOff = (statSet & FM_STAT_POWER_OFF) != 0;
							bool monitorOn = (statSet & FM_STAT_POWER_ON) != 0;
							bool bufferOverflow = (statSet & FM_STAT_BUFFER_OVERFLOW) != 0;

							// Make sure the data is consistent
							if (hr1Mode == 0 && hr2Mode == 0 && (monitorOff || monitorOn || bufferOverflow || spData != 0))
							{
								Debug.WriteLine("Ignoring that status block as per GE code");
							}
							else
							{
								HR1ProbeSignal = ((hr2Mode == FM_STAT_MODE_MECG) || (hr2Mode == FM_STAT_MODE_MAECG) || (hr2Mode == FM_STAT_MODE_EXT_MHR)) ? WAVE_FHR1 : WAVE_FHR2;
							}
							break;
						}

					case PMS_WAVE_DATA:
						{
							if (this.Read(this.ReadBuffer, WAVE_HEADER_LENGTH) != WAVE_HEADER_LENGTH)
								throw new Exception("Unable to read the wave header");

							// Check the wave length
							var lLength = GetInteger16(this.ReadBuffer, DATA_LEN_OFFSET);
							if (lLength > 0)
							{
								if (this.Read(this.ReadBuffer, lLength) != lLength)
									throw new Exception("Unable to read the wave data");

								// Get wave data length
								TracingBlock block = new TracingBlock { Start = QSTimeToDateTime(displayTime) };
								for (int waveOffset = 0; waveOffset + F_WAVE_CHUNK_OFFSET < lLength; )
								{
									// Read the header
									int type = this.ReadBuffer[waveOffset];
									int numSamples = GetInteger16(this.ReadBuffer, waveOffset + NUM_SAMPLES_OFFSET);
									waveOffset += F_WAVE_CHUNK_OFFSET;

									// Safety first
									if (waveOffset + numSamples > lLength)
										throw new Exception("Incorrect wave data number of sample, does not fit in the data length: " + numSamples);

									if (numSamples > 0)
									{
										// Look for HR1 and UP
										switch (type)
										{
											case WAVE_FHR1:
											case WAVE_FHR2:
												// Note that we need the previous status block to know which HR signal is actually the FHR1 signal
												if (HR1ProbeSignal == type)
												{
													for (int i = 0; i < numSamples; ++i)
													{
														block.HRs.Add(this.ReadBuffer[waveOffset + i]);
													}
												}
												break;

											case WAVE_UA:
												for (int i = 0; i < numSamples; i += 4) // Immediatly downgrade from 4Hz to 1Hz
												{
													byte data = this.ReadBuffer[waveOffset + i];
													block.UPs.Add(data == 128 ? (byte)255 : data);
												}
												break;
										}

										// Next one
										waveOffset += numSamples;
									}
								}

								// Skip empty block (could happened if there is only MHR for instance for that period of time)
								if (block.TotalSeconds > 0)
								{
									// Align signal
									block.AlignSignals();
									results.Add(block);

									this.StartTime = block.End;
								}
							}
							break;
						}

					case PMS_RETURN_CODE:
						{
							if (this.Read(this.ReadBuffer, RETURN_CODE_SIZE) != RETURN_CODE_SIZE)
								throw new Exception("Unable to read the return code");

							var code = GetInteger16(this.ReadBuffer, 0);
							switch (code)
							{
								case C_NORMAL:
									// All good
									break;

								case C_ALL_DATA:
									// Got completly up to date
									break;

								case C_NO_DATA:
									// No data available at this time
									break;

								case C_PT_ID_CHANGE:
									Common.Source.TraceEvent(TraceEventType.Verbose, 6304, "Tracing: (Live) C_PT_ID_CHANGE while reading the live tracing for {0}", this);
									this.NeedChalkboardRefresh = true;
									this.ReleaseResources();
									break;

								case C_CONNECTING:
									Common.Source.TraceEvent(TraceEventType.Verbose, 6305, "Tracing: (Live) C_CONNECTING while reading the live tracing for {0}", this);
									this.NeedChalkboardRefresh = true;
									this.ReleaseResources();
									break;

								case C_NOT_CONNECTED:
									Common.Source.TraceEvent(TraceEventType.Verbose, 6306, "Tracing: (Live) C_NOT_CONNECTED while reading the live tracing for {0}", this);
									this.NeedChalkboardRefresh = true;
									this.ReleaseResources();
									break;

								case C_CONNECT_ERROR:
									Common.Source.TraceEvent(TraceEventType.Verbose, 6307, "Tracing: (Live) C_CONNECT_ERROR while reading the live tracing for {0}", this);
									this.NeedChalkboardRefresh = true;
									this.ReleaseResources();
									break;

								case C_POWER_OFF:
									Common.Source.TraceEvent(TraceEventType.Verbose, 6308, "Tracing: (Live) C_POWER_OFF while reading the live tracing for {0}", this);
									this.NeedChalkboardRefresh = true;
									this.ReleaseResources();
									break;

								default:
									Common.Source.TraceEvent(TraceEventType.Verbose, 6313, "Tracing: (Live) Unknown PMS_RETURN_CODE ({0}) while reading the live tracing for {1}", this, code);
									break;
							}
							break;
						}

					case PMS_FM_NOTE:
						Common.Source.TraceEvent(TraceEventType.Verbose, 6309, "Tracing: (Live) PMS_FM_NOTE while reading the live tracing for {0}. Skipping it.", this);
						Skip(FM_NOTE_LIMIT + 1); //Skip QSString
						break; 

					case PMS_PATIENT_ID:
						Common.Source.TraceEvent(TraceEventType.Verbose, 6310, "Tracing: (Live) PMS_PATIENT_ID while reading the live tracing for {0} (unable to handle)", this);
						this.ReleaseResources();
						return null;

					default:
						Common.Source.TraceEvent(TraceEventType.Verbose, 6311, "Tracing: (Live) Unknown EDATA {0} while reading the live tracing for {1} (unable to handle)", eDataType, this);
						this.ReleaseResources();
						return null;
				}
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Verbose, 6312, "Tracing: (Live) Error while reading the live tracing for {0}!\n{1}", this, e);
				this.ReleaseResources();
			}
			return results;
		}

		/// <summary>
		/// For traces
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"TracingProcessorLive|unique_id='{0}' mrn='{1}' bed_id='{2}'",
				this.PatientUniqueID, 
				Settings_TraceExtendedInformationWithPHIData ? this.PatientID : "n/a",
				this.BedID);
		}
	}
}
