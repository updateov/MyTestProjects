using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using PeriGen.Patterns.Engine.Data;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace PeriGen.Patterns.GE.Interface.TracingServices
{
	public class TracingProcessorHistoric : TracingProcessorBase
	{
		#region Constants for GE OBLink data

		// PT_DATA
		const int DISPLAY_TIME_OFFSET = 8;
		const int LC_IMAGE_LEN_OFFSET = 41;
		const int PT_DATA_LENGTH = LC_IMAGE_LEN_OFFSET + 4;

		// WAVE_DATA
		const int N_WAVES = 4;
		const int NUM_SAMPLES_OFFSET = 1;
		const int F_WAVE_CHUNK_OFFSET = 3;
		const int MAXIMUM_NUM_SAMPLES = 240;
		const int MAXIMUM_R_WAVE_DATA = F_WAVE_CHUNK_OFFSET + MAXIMUM_NUM_SAMPLES;

		// WAVE_DATA types (at TYPE_OFFSET)
		const int WAVE_FHR1 = 0;
		const int WAVE_FHR2 = 1;
		const int WAVE_MHR = 2;
		const int WAVE_UA = 3;

		// IMAGE_DATA_HEADER
		const int E_TYPE_OFFSET = 0; // eType (see below)
		const int LENGTH_OFFSET = 1; // lLength
		const int IMAGE_DATA_HEADER_LENGTH = 11;
		const int FWAVE_IMAGE = 5;
		const int FMSTAT_IMAGE = 6;
		const int IMAGE_DATA_TRAILER_LENGTH = 3;

		// FM_STATUS
		const int STAT_SET = 0;
		const int HR1_MODE = 3;
		const int HR2_MODE = 4;
		const int SP_DATA = 7;
		const int FM_STATUS_SIZE = 9;

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

		byte[] HeaderBuffer = new byte[PT_DATA_LENGTH];
		byte[] ImageDataHeaderBuffer = new byte[IMAGE_DATA_HEADER_LENGTH];
		byte[] FMStatusBuffer = new byte[FM_STATUS_SIZE];
		byte[] WaveDataBuffer = new byte[MAXIMUM_R_WAVE_DATA * N_WAVES];

		#endregion

		/// <summary>
		/// Ctr
		/// </summary>
		/// <param name="patient_uniqueid"></param>
		/// <param name="patient_id"></param>
		/// <param name="bed_id"></param>
		/// <param name="start"></param>
		public TracingProcessorHistoric(int patient_uniqueid, string patient_id, string bed_id, DateTime start)
			: base(patient_uniqueid, patient_id, bed_id, start)
		{
			// We can only go back a certain duration...
			if ((start == DateTime.MinValue) || (DateTime.UtcNow - start).TotalHours > Settings_HistoricalTracingMaximumDuration)
			{
				this.StartTime = DateTime.UtcNow.AddHours(-Settings_HistoricalTracingMaximumDuration);
			}
			else
			{
				// We need a small overlap to be sure we don't have a problem with gap
				this.StartTime = start.AddSeconds(-Settings_HistoricalTracingOverlapDuration);
			}
			this.LastTracingTime = this.StartTime;
		}

		/// <summary>
		/// Check if there is more tracing to retrieve
		/// </summary>
		public bool HasMoreTracing
		{
			get
			{
				return
					(this.ErrorCount < Settings_HistoricalTracingRetryLimit)
						&& (!this.OBLinkError)
						&& (!string.IsNullOrEmpty(this.BedID))
						&& (DateTime.UtcNow - this.StartTime).TotalSeconds > Settings_HistoricalTracingHasMoreTracingThreshold;
			}
		}

		/// <summary>
		/// For traces
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"TracingProcessorHistoric|unique_id='{0}' mrn='{1}' bed_id='{2}'",
				this.PatientUniqueID, 
				Settings_TraceExtendedInformationWithPHIData ? this.PatientID : "n/a",
				this.BedID);
		}

		/// <summary>
		/// To track error reading the tracings
		/// </summary>
		int ErrorCount { get; set; }

		/// <summary>
		/// To track OBLink errors
		/// </summary>
		bool OBLinkError { get; set; }

		/// <summary>
		/// Read some the next chunk of tracing if any
		/// </summary>
		/// <returns></returns>
		public List<TracingBlock> ReadTracing()
		{
			Stopwatch watch = Stopwatch.StartNew();

			// Safety first
			if (string.IsNullOrEmpty(this.BedID))
				return null;

			/////////////////////// Build the url for the request
			long block_duration = Math.Min((long)(DateTime.UtcNow - this.StartTime).TotalSeconds, 3600 * Settings_HistoricalTracingBlockDuration);
			if (block_duration <= 0)
				return null;

			long block_start = DateTimeToQSTime(this.StartTime) + block_duration; // For OBLink, the start time is actually the end time of the block you want

			string relative_url = string.Format(CultureInfo.InvariantCulture,
													Settings_OBLinkPatientTracingsRequestUrl,
													this.BedID, "TRUE", block_duration, this.PatientID, block_start);

			bool error = false;
			var results = new List<TracingBlock>();
			try
			{
				/////////////////////// Connect to OBLink and ask the data
				if (!this.OpenConnection(new Uri(new Uri(Settings_OBLinkURL), relative_url), Settings_HistoricalTracingOpenTimeout, Settings_HistoricalTracingReadTimeout))
				{
					// No tracings at all for that block
					Common.Source.TraceEvent(TraceEventType.Verbose, 6201, "Tracing: (History) No tracing for {0} from {1:s} to {2:s}", this, this.StartTime, this.StartTime.AddSeconds(block_duration));

					// Do not count that as an error... Skip the block.
					this.StartTime = this.StartTime.AddSeconds(block_duration);
					this.ErrorCount = 0;
					return null;
				}

				/////////////////////// Read and process OBLink response
				// The FHR1 signal can be in HR1 or HR2 data block depending on the probe...
				int HR1ProbeSignal = -1;

				// Loop as long as there are some data block to read
				while (this.Read(this.HeaderBuffer, this.HeaderBuffer.Length) != 0)
				{
					// Check for error
					if (IsDataHeader(HeaderBuffer))
						throw new Exception("Server returned an error!");

					// Extract the fields we need.
					int displayTime = GetInteger32(HeaderBuffer, DISPLAY_TIME_OFFSET);
					int lcImageLen = GetInteger16(HeaderBuffer, LC_IMAGE_LEN_OFFSET);
					if (lcImageLen < ImageDataHeaderBuffer.Length)
						throw new Exception("Incorrect image header length, too small to even fit a header: " + lcImageLen);

					// Read the rest of the header now that we know its exact type and therefore size
					if (this.Read(this.ImageDataHeaderBuffer, this.ImageDataHeaderBuffer.Length) != ImageDataHeaderBuffer.Length)
						throw new Exception("Unable to read the image data header!");

					// Extract the fields we need
					int imageType = GetInteger8(ImageDataHeaderBuffer, E_TYPE_OFFSET);
					int lLength = GetInteger32(ImageDataHeaderBuffer, LENGTH_OFFSET);

					// The image is composed of a header, some variable length payload and a trailer
					if (lcImageLen != IMAGE_DATA_HEADER_LENGTH + lLength + IMAGE_DATA_TRAILER_LENGTH)
						throw new Exception("Incorrect image data length, too big to fit in the whole image length: " + lLength);

					if (lLength > 0)
					{
						// Depending on the type, read the appropriate data
						switch (imageType)
						{
							case FWAVE_IMAGE:
								if (this.Read(this.WaveDataBuffer, lLength) != lLength)
									throw new Exception("Unable to read the wave data!");

								TracingBlock block = new TracingBlock { Start = QSTimeToDateTime(displayTime), Capacity = lLength / 4 };
								for (int waveOffset = 0; waveOffset + F_WAVE_CHUNK_OFFSET < lLength; )
								{
									// Read the header
									int type = WaveDataBuffer[waveOffset];
									int numSamples = GetInteger16(WaveDataBuffer, waveOffset + NUM_SAMPLES_OFFSET);
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
														block.HRs.Add(WaveDataBuffer[waveOffset + i]);
													}
												}
												break;

											case WAVE_UA:
												for (int i = 0; i < numSamples; i += 4) // Immediatly downgrade from 4Hz to 1Hz
												{
													byte data = WaveDataBuffer[waveOffset + i];
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

									// Don't add block that are a complete overlap with the previous one and don't add block only containing no data
									if (block.End > this.LastTracingTime)
									{
										// Take care of partial overlap here!
										if (block.Start < this.LastTracingTime)
										{
											int overlap = (int)((this.LastTracingTime - block.Start).TotalSeconds);
											block.HRs.RemoveRange(0, 4 * overlap);
											block.UPs.RemoveRange(0, overlap);
											block.Start = this.LastTracingTime;
										}
										results.Add(block);
										this.LastTracingTime = block.End;
									}
								}
								break;

							case FMSTAT_IMAGE:
								if (lLength != FM_STATUS_SIZE)
								{
									this.Skip(lLength);
									Debug.Assert(false, "The received status block length is invalid!");
								}
								else
								{
									if (this.Read(this.FMStatusBuffer, this.FMStatusBuffer.Length) != FMStatusBuffer.Length)
										throw new Exception("Unable to read the FM status data!");

									// Extract useful info from the status
									int statSet = GetInteger8(FMStatusBuffer, STAT_SET);
									int hr1Mode = GetInteger8(FMStatusBuffer, HR1_MODE);
									int hr2Mode = GetInteger8(FMStatusBuffer, HR2_MODE);
									int spData = GetInteger8(FMStatusBuffer, SP_DATA);
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
								}
								break;

							default:
								// Skip the whole image
								this.Skip(lLength);
								break;
						}
					}

					// Ignore the trailer.
					this.Skip(IMAGE_DATA_TRAILER_LENGTH);
				}

				// Move to the end of the current block
				this.StartTime = this.StartTime.AddSeconds(block_duration);
			}
			catch (Exception e)
			{
				// If OBLink timed out, no need to overstress it, leave now
				if ((e.Message.Contains("operation has timeout")) || (e.Message.Contains("(500) Internal Server Error.")))
				{
					this.OBLinkError = true;
					Common.Source.TraceEvent(TraceEventType.Warning, 6203, "Tracing: (History) OBlink error while reading historical tracing for {0} from {1:s} to {2:s}.\n{3}", this, this.StartTime, this.StartTime.AddSeconds(block_duration), e);
				}

				// If OBLink closed the connection on us (typical of no tracing for the time range)
				else if (e.Message.Contains("server committed a protocol violation"))
				{
					// Skip the block
					Common.Source.TraceEvent(TraceEventType.Verbose, 6204, "Tracing: (History) OBlink protocol violation while reading historical tracing for {0} from {1:s} to {2:s}.\n{3}", this, this.StartTime, this.StartTime.AddSeconds(block_duration), e);
					this.StartTime = this.StartTime.AddSeconds(block_duration);
				}

				// If we managed to read some...
				else if (this.LastTracingTime > this.StartTime)
				{
					// Skip to the end of what was read
					Common.Source.TraceEvent(TraceEventType.Verbose, 6202, "Tracing: (History) Error while reading historical tracing for {0} from {1:s} to {2:s} but some tracing read so continue.\n{3}", this, this.StartTime, this.StartTime.AddSeconds(block_duration), e);
					this.StartTime = this.LastTracingTime;
				}
				
				// If we reach the limit of retries...
				else if (this.ErrorCount >= Settings_HistoricalTracingRetryLimit)
				{
					// Skip up to an hour of the block...
					Common.Source.TraceEvent(TraceEventType.Error, 6205, "Tracing: (History) Error while reading historical tracing for {0} from {1:s} to {2:s}.\n{3}", this, this.StartTime, this.StartTime.AddSeconds(Math.Min(3600, block_duration)), e);
					this.StartTime = this.StartTime.AddSeconds(Math.Min(3600, block_duration));
				}

				// Flag as error
				else
				{
					Common.Source.TraceEvent(TraceEventType.Verbose, 6206, "Tracing: (History) Error while reading historical tracing for {0} from {1:s} to {2:s}.\nWill try again!\n{3}", this, this.StartTime, this.StartTime.AddSeconds(Math.Min(3600, block_duration)), e);
					error = true;
				}
			}
			finally
			{
				// Clean up
				this.ReleaseResources();

				// Count the error numbers...
				this.ErrorCount = error ? this.ErrorCount + 1 : 0;
			}

			// Done
			if ((results != null) && (results.Count > 0))
			{
				// Merge blocks
				results = TracingBlock.Merge(results, Settings_TracingMergeBridgeableGap);

				if (results.Count > 0)
				{
					Common.Source.TraceEvent(TraceEventType.Verbose, 6207, "Tracing: (History) Historical tracing for {0}: {1} minute(s) retrieved in {2} ms.", this, results.Sum(b => b.TotalSeconds) / 60, watch.ElapsedMilliseconds);
				}
			}

			return results;
		}
	}
}
