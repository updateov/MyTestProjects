using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeriGen.Patterns.GE.Interface.TracingServices;
using System.Diagnostics;
using PeriGen.Patterns.Engine.Data;
using System.Globalization;

namespace PeriGen.Patterns.GE.Interface.Processing
{
	public class GETracingReader
	{
		TracingProcessorLive ProcessorLive { get; set; }

		public bool IsResetted { get { return this.ProcessorLive == null || !this.ProcessorLive.IsCreated; } }

		Dictionary<DateTime, List<byte>> HRList;
		Dictionary<DateTime, List<byte>> UPList;

		/// <summary>
		/// The patient ID
		/// </summary>
		public string PatientId { get; protected set; }

		/// <summary>
		/// The Bed ID
		/// </summary>
		public string BedId { get; protected set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="patientId"></param>
		/// <param name="bedId"></param>
		public GETracingReader(string patientId, string bedId)
		{
			this.PatientId = patientId;
			this.BedId = bedId;
			this.HRList = new Dictionary<DateTime, List<byte>>();
			this.UPList = new Dictionary<DateTime, List<byte>>();
		}

		/// <summary>
		/// Wave data type
		/// </summary>        
		enum WaveTypeEnum
		{
			WAVE_FHR1 = 0,
			WAVE_FHR2 = 1,
			WAVE_MHR = 2,
			WAVE_UA = 3,
			N_WAVES = 4,
			WAVE_UNKNOWN = 99
		}

		/// <summary>
		/// Hooks delegate from Tracing processor and stores wave data comming from web server
		/// </summary>
		/// <param name="data">wave data (bytes values)</param>
		/// <param name="dataType">Type of wave</param>
		/// <param name="dateTime">time</param>
		private void WaveProcessed(byte[] data, int dataType, DateTime dateTime)
		{
			switch (dataType)
			{
				case (int)WaveTypeEnum.WAVE_FHR1:
					if (HRList.ContainsKey(dateTime))
					{
						HRList[dateTime].AddRange(data);
					}
					else
					{
						HRList.Add(dateTime, new List<byte>(data));
					}
					break;

				case (int)WaveTypeEnum.WAVE_UA:
					if (UPList.ContainsKey(dateTime))
					{
						UPList[dateTime].AddRange(data);
					}
					else
					{
						UPList.Add(dateTime, new List<byte>(data));
					}
					break;
			}
		}

		/// <summary>
		/// Returns live tracing
		/// </summary>
		/// <returns>Tracing list</returns>
		public List<TracingBlock> GetLiveTracing()
		{
			if (ProcessorLive == null)
			{
				//create new processor
				ProcessorLive = new TracingProcessorLive();
				ProcessorLive.ReceivedBytesWithTime += this.WaveProcessed;

				var args = new TracingArgs();
				args.BedId = BedId;
				args.PatientId = PatientId;

				args.HostServiceAddress = Settings.SettingsManager.GetValue("urlTracing"); ///TODO Check Configuration File
				args.TimeAgo = 180; ///TODO check overlap value
				ProcessorLive.Initialize(args);
				ProcessorLive.IsCreated = false;
				ProcessorLive.FlagStop = false;
			}

			/// Collect tracing
			ProcessorLive.Process();

			///Merge blocks
			List<TracingBlock> blockList = new List<TracingBlock>();
			DateTime minHrTime = DateTime.MaxValue;
			DateTime minUpTime = DateTime.MaxValue;
			if (HRList.Count > 0) minHrTime = HRList.Keys.Min();
			if (UPList.Count > 0) minUpTime = UPList.Keys.Min();
			if (minHrTime == DateTime.MinValue && minUpTime == DateTime.MinValue) return blockList;

			//Hr first
			if (minHrTime <= minUpTime)
			{
				foreach (var item in HRList.Keys)
				{
					//if block does not exist with key time, create a new one
					if (!blockList.Exists(tb => tb.Start == item))
					{
						TracingBlock tb = new TracingBlock();
						tb.Start = item;
						blockList.Add(tb);
					}
					//add hrs
					blockList.First(tb => tb.Start == item).HRs.AddRange(HRList[item]);

					//add ups
					if (UPList.ContainsKey(item))
					{
						blockList.First(tb => tb.Start == item).UPs.AddRange(UPList[item]);
					}
					else
					{
						blockList.First(tb => tb.Start == item).UPs.AddRange(new byte[HRList[item].Count]);
					}
				}
			}
			else
			{
				foreach (var item in UPList.Keys)
				{
					//if block does not exist with key time, create a new one
					if (!blockList.Exists(tb => tb.Start == item))
					{
						TracingBlock tb = new TracingBlock();
						tb.Start = item;
						blockList.Add(tb);
					}
					//add Ups
					blockList.First(tb => tb.Start == item).UPs.AddRange(UPList[item]);

					//add Hrs
					if (HRList.ContainsKey(item))
					{
						blockList.First(tb => tb.Start == item).HRs.AddRange(HRList[item]);
					}
					else
					{
						blockList.First(tb => tb.Start == item).HRs.AddRange(new byte[UPList[item].Count]);
					}
				}
			}

			foreach (var tb in blockList)
			{
				//pad with 0
				var HRCount = tb.HRs.Count;
				var UPCount = tb.UPs.Count;
				if (HRCount < UPCount)
				{
					tb.HRs.AddRange(new byte[UPCount - HRCount]);
				}
				else if (HRCount > UPCount)
				{
					tb.UPs.AddRange(new byte[HRCount - UPCount]);
				}

				//list to fix up
				List<byte> Ups = new List<byte>();

				//now remove extra up
				for (int i = 0; i < tb.UPs.Count; i += 4)
				{
					Ups.Add(tb.UPs[i]);
				}
				//clear ups...
				tb.UPs.Clear();

				//.. and add fixed ups
				tb.UPs.AddRange(Ups);
			}

			//list of tracings
			return blockList.OrderBy(b => b.Start).ToList();
		}

		/// <summary>
		/// Destroy processor and free resources
		/// </summary>
		public void ResetLiveProcessor()
		{
			if (ProcessorLive != null)
			{
				try
				{
					///set flag to close connection
					ProcessorLive.FlagStop = true;

					///make last call to close connection
					ProcessorLive.Process();

					///remove handlers
					ProcessorLive.ReceivedBytesWithTime -= this.WaveProcessed;

					///destroy processors and set reseting flags
					ProcessorLive = null;
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.ToString());
					Common.Source.TraceEvent(TraceEventType.Warning, 1004, "Warning, exception ResetLiveProcessor.\n{0}", ex);
				}
			}
		}
	}
}
