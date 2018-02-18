using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using PeriGen.Patterns.ActiveXInterface;
using PeriGen.Patterns.Engine.Data;
using PeriGen.Patterns.GE.Interface;
using PeriGen.Patterns.GE.Interface.TracingServices;
using PeriGen.Patterns.Settings;

namespace PeriGen.Patterns.GE.Service
{
	[Serializable]
	public class PatternsEpisode : Episode, IDisposable
	{
		#region Configuration

		static int Settings_PatternsEngineMinimumDurationToAppend = SettingsManager.GetInteger("PatternsEngineMinimumDurationToAppend");
		static int Settings_PatternsEngineMaximumBridgeableGap = SettingsManager.GetInteger("PatternsEngineMaximumBridgeableGap");
		static int Settings_PatternsEngineBufferPrimingDelay = Math.Max(0, SettingsManager.GetInteger("PatternsEngineBufferPrimingDelay"));
		static int Settings_CompressCachedTracingMaximumDuration = SettingsManager.GetInteger("CompressCachedTracingMaximumDuration");
		static int Settings_CompressCachedTracingExcludeLastBlocks = SettingsManager.GetInteger("CompressCachedTracingExcludeLastBlocks");
		static int Settings_TracingMergeBridgeableGap = SettingsManager.GetInteger("TracingMergeBridgeableGap");
		static int Settings_LiveTracingMaximumDuration = SettingsManager.GetInteger("LiveTracingMaximumDuration");
		static int Settings_LiveTracingHasMoreTracingThreshold = SettingsManager.GetInteger("LiveTracingHasMoreTracingThreshold");
		static int Settings_SleepTimeAfterEachHistorical = SettingsManager.GetInteger("SleepTimeAfterEachHistorical");
		static int Settings_SleepTimeAfterEachLive = SettingsManager.GetInteger("SleepTimeAfterEachLive");
		static int Settings_MaxDaysOfTracingReturnedToActiveX = SettingsManager.GetInteger("MaxDaysOfTracingReturnedToActiveX");
		static int Settings_TracingLateCollectingLive = SettingsManager.GetInteger("TracingLateCollectingLive");
		static int Settings_LiveTracingReadTimeout = SettingsManager.GetInteger("LiveTracingReadTimeout");

		static string Settings_DBConnectionStringFormat = SettingsManager.GetValue("DBConnectionStringFormat");
		static string Settings_PatternsDBPath = SettingsManager.GetValue("PatternsDBPath");

		#endregion

		#region Members and properties

		/// <summary>
		/// For thread safety on accessing the database of that object
		/// </summary>
		[XmlIgnore]
		object LockDatabase = new object();

		/// <summary>
		/// List of Tracing blocks
		/// </summary>
		[XmlIgnore]
		public List<TracingBlock> TracingBlocks { get; private set; }

		/// <summary>
		/// Hours of tracings
		/// </summary>
		public double HoursOfTracings
		{
			get
			{
				lock (this.TracingBlocks)
				{
					return this.TracingBlocks.Sum(t => t.TotalSeconds) / 3600d;
				}
			}
		}

		/// <summary>
		/// The id of the last tracing block from TracingBlocks that was saved in database
		/// </summary>
		int LastCommittedTracingID { get; set; }

		/// <summary>
		/// The id of the last tracing block from TracingBlocks that was added to the memory cache
		/// </summary>
		int LastMemoryTracingID { get; set; }

		/// <summary>
		/// The id of the last tracing block from TracingBlocks that was compressed
		/// </summary>
		int LastCompressTracingID { get; set; }

		/// <summary>
		/// Return the date and time of the last bit of tracing known for that episode
		/// </summary>
		public DateTime LastTracing { get; set; }

		/// <summary>
		/// The latency for real time on that episode in seconds
		/// </summary>
		public string RealTimeLatency
		{
			get
			{
				if (this.EpisodeStatus != EpisodeStatuses.Normal)
					return "N/A";

				if (this.LastTracing == DateTime.MinValue)
					return "No tracing";

				return string.Format(CultureInfo.InvariantCulture, "{0:0} ({1})", (DateTime.UtcNow - this.LastTracing).TotalSeconds, this.IsLive ? "Live" : "Off");
			}
		}

		/// <summary>
		/// The latency for real time on that episode in seconds
		/// </summary>
		public long RealTimeLatencyLong
		{
			get
			{
				if(this.EpisodeStatus != EpisodeStatuses.Normal)
					return -1;

				if(this.LastTracing == DateTime.MinValue)
					return 0;

				return Convert.ToInt64((DateTime.UtcNow - this.LastTracing).TotalMilliseconds);
			}
		}

		/// <summary
		/// List of artifacts
		/// </summary>
		[XmlIgnore]
		public List<DetectionArtifact> TracingArtifacts { get; private set; }

		/// <summary>
		/// The id of the last artifact from TracingArtifacts that was saved in database
		/// </summary>
		int LastCommittedArtifactID { get; set; }

		/// <summary>
		/// The id of the last artifact from TracingArtifacts that was added to the memory cache
		/// </summary>
		int LastMemoryArtifactID { get; set; }

		/// <summary>
		/// List of actions
		/// </summary>
		[XmlIgnore]
		public List<XUserAction> TracingActions { get; private set; }

		/// <summary>
		/// The id of the last Actions from TracingActions that was saved in database
		/// </summary>
		int LastCommittedActionID { get; set; }

		/// <summary>
		/// The id of the last Actions from TracingActions that was added to the memory cache
		/// </summary>
		int LastMemoryActionID { get; set; }

		volatile bool _NeedChalkboardRefresh;

		/// <summary>
		/// If some the live tracing cannot be restarted before a chalkboard refresh...
		/// </summary>
		[XmlIgnore]
		public bool NeedChalkboardRefresh { get { return this._NeedChalkboardRefresh; } set { this._NeedChalkboardRefresh = value; } }

		volatile bool _RecoveryInProgress;

		/// <summary>
		/// If some tracing processing or event detection has catch up to do
		/// </summary>
		public bool RecoveryInProgress { get { return this._RecoveryInProgress; } set { this._RecoveryInProgress = value; } }

		volatile bool _ResetLive;

		/// <summary>
		/// To know if the live must be reset
		/// </summary>
		[XmlIgnore]
		public bool ResetLive
		{
			get { return this._ResetLive; }
			set { this._ResetLive = value; }
		}

		/// <summary>
		/// Pattern's engine
		/// </summary>
		[XmlIgnore]
		PeriGen.Patterns.Engine.PatternsEngineWrapper Engine { get; set; }

		/// <summary>
		/// Determine if we primed the engine with past tracings that were already processed
		/// </summary>
		bool EnginePrimed { get; set; }

		/// <summary>
		/// Determine if we primed the engine with past tracings that were already processed
		/// </summary>
		DateTime EnginePrimedContractions { get; set; }

		/// <summary>
		/// Determine if we primed the engine with past tracings that were already processed
		/// </summary>
		DateTime EnginePrimedEvents { get; set; }

		/// <summary>
		/// The thread used to collect live tracings for that particular episode
		/// </summary>
		[XmlIgnore]
		Thread LiveThread { get; set; }

		/// <summary>
		/// Is the current episode currently set to get live tracings
		/// </summary>
		public bool IsLive { get; set; }

		/// <summary>
		/// Indicate a live episode that is late retrieving the realtime tracing
		/// </summary>
		bool IsLate
		{
			get { return (!this.IsLive) || ((DateTime.UtcNow - this.LastTracing).TotalSeconds > Settings_TracingLateCollectingLive); }
		}

		#endregion

		/// <summary>
		/// Ctor
		/// </summary>
		public PatternsEpisode()
		{
			// All new episodes are flag as recovery in progress...
			this.RecoveryInProgress = true;

			this.TracingBlocks = new List<TracingBlock>();
			this.TracingArtifacts = new List<DetectionArtifact>();
			this.TracingActions = new List<XUserAction>();
		}

		/// <summary>
		/// Some ADT action was performed on that episode
		/// Reset the live tracing processor if necessary
		/// </summary>
		protected override void OnADTChanged()
		{
			base.OnADTChanged();
			this.ResetLive = true;
		}

		/// <summary>
		/// Add some more tracings to the list
		/// </summary>
		/// <param name="list"></param>
		public void AddTracings(IEnumerable<TracingBlock> list)
		{
			if ((list == null) || (list.Count() == 0))
				return;

			// First, cut any overlap between the current in memory tracings and the ones in the list
			var last_time = this.LastTracing;
			var filtered = new List<TracingBlock>();
			foreach (var b in list)
			{
				// Skip empty block (in case...)
				if (b.TotalSeconds == 0)
					continue;

				// Some validations
				Debug.Assert(b.End.Millisecond == 0);
				Debug.Assert(4 * b.UPs.Count == b.HRs.Count);
				Debug.Assert(b.Start < b.End);

				// Skip complete overlap
				if (b.End <= last_time)
					continue;

				// Cut partial overlap
				if (b.Start < last_time)
				{
					int overlap = (int)((last_time - b.Start).TotalSeconds);
					b.HRs.RemoveRange(0, 4 * overlap);
					b.UPs.RemoveRange(0, overlap);
					b.Start = last_time;
				}

				Debug.Assert(b.TotalSeconds > 0);
				Debug.Assert(b.Start >= last_time);

				// Done with that one
				filtered.Add(b);
				last_time = b.End;
			}

			if (filtered.Count == 0)
			{
				// Nothing to do
				return;
			}

			// Then add them to the memory buffer
			lock (this.TracingBlocks)
			{
				// Set new ID (in memory ID)...
				foreach (var b in filtered)
				{
					b.Id = ++this.LastMemoryTracingID;
				}

				// Add all...
				this.TracingBlocks.AddRange(filtered);

				// Update the last tracing date&time
				this.LastTracing = last_time;
			}
		}

		/// <summary>
		/// Add some more artifacts to the list
		/// </summary>
		/// <param name="list"></param>
		public void AddArtifacts(IEnumerable<DetectionArtifact> list)
		{
			if ((list == null) || (list.Count() == 0))
				return;

			lock (this.TracingArtifacts)
			{
				// Set new ID (in memory ID)...
				foreach (var item in list)
				{
					item.Id = ++this.LastMemoryArtifactID;
				}

				// Add all...
				this.TracingArtifacts.AddRange(list);
			}
		}

		/// <summary>
		/// Add some more actions to the list
		/// </summary>
		/// <param name="list"></param>
		public void AddActions(IEnumerable<XUserAction> list)
		{
			if ((list == null) || (list.Count() == 0))
				return;

			lock (this.TracingActions)
			{
				// Set new ID (in memory ID)...
				foreach (var item in list)
				{
					item.Id = ++this.LastMemoryActionID;
				}

				// Add all...
				this.TracingActions.AddRange(list);
			}
		}

		#region For the ActiveX data exchange

		/// <summary>
		/// Encode a Patient into an XElement that can be sent to the patterns activex
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public XElement EncodeForActiveX()
		{
			lock (this.LockObject)
			{
				return new XElement("patient",
						new XAttribute("id", this.PatientUniqueId),
						new XAttribute("mrn", this.MRN ?? string.Empty),
						new XAttribute("status", (int)this.PatientStatus),
						new XAttribute("statusdetails", string.Empty),
						new XAttribute("firstname", string.Empty),
						new XAttribute("lastname", this.Patient.Name ?? string.Empty),
						new XAttribute("edd", this.Patient.EDD.ToEpoch() ?? 0),
						new XAttribute("reset", this.Created.ToEpoch()),
						new XAttribute("fetus", this.Patient.Fetuses ?? 0));
			}
		}

		/// <summary>
		/// Return the patient Status for the active X according the the state of the episode
		/// </summary>
		StatusType PatientStatus
		{
			get
			{
				switch (this.EpisodeStatus)
				{
					case EpisodeStatuses.OutOfMonitor:
						return StatusType.Unplugged;

					case EpisodeStatuses.Normal:
						{
							if (this.RecoveryInProgress)
								return StatusType.Recovery;

							if (this.IsLate)
								return StatusType.Late;

							return StatusType.Live;
						}

					case EpisodeStatuses.Closed:
					case EpisodeStatuses.OutOfScope:
					default:
						return StatusType.Invalid;
				}
			}
		}

		#endregion

		/// <summary>
		/// Process the new tracings using the Pattern's engine
		/// </summary>
		public void ProcessPatterns()
		{
			// No tracing at all, leave!
			lock (this.TracingBlocks)
			{
				if (this.TracingBlocks.Count == 0)
				{
					return;
				}
			}

			// No current engine? Create a new one
			if (this.Engine == null)
			{
				// Reset the priming values
				this.EnginePrimed = false;

				if (this.TracingArtifacts.Count > 0)
				{
					lock (this.TracingArtifacts)
					{
						// If we already have artifacts, restart the engine after the last one
						var startime = this.TracingArtifacts.Max(a => a.EndTime);

						// But for a little 'priming' period to ensure the buffers inside the engine are primed
						this.Engine = new PeriGen.Patterns.Engine.PatternsEngineWrapper(startime.AddMinutes(-Settings_PatternsEngineBufferPrimingDelay), this.ToString());

						// Remember we are processing some tracing that was already processed
						if (Settings_PatternsEngineBufferPrimingDelay > 0)
						{
							this.EnginePrimed = true;

							var lastContraction = this.TracingArtifacts.LastOrDefault(a => a.IsContraction);
							this.EnginePrimedContractions = (lastContraction == null) ? DateTime.MinValue : lastContraction.EndTime;

							var lastEvent = this.TracingArtifacts.LastOrDefault(a => !a.IsContraction);
							this.EnginePrimedEvents = (lastEvent == null) ? DateTime.MinValue : lastEvent.EndTime;
						}
					}
				}
				else
				{
					lock (this.TracingBlocks)
					{
						// If we don't already have artifact, restart the engine at the very beginning ot the tracings
						this.Engine = new PeriGen.Patterns.Engine.PatternsEngineWrapper(this.TracingBlocks.Min(b => b.Start), this.ToString());
					}
				}
			}

			// Get the new block since last time we did a patterns detection...
			IEnumerable<TracingBlock> tracings;
			lock (this.TracingBlocks)
			{
				tracings = this.TracingBlocks.Where(b => b.End > this.Engine.CurrentTime).ToList();
			}

			// Do not trigger a detection unless there is a significant amount of tracing
			if ((tracings.Count() == 0) || ((tracings.Last().End - Engine.CurrentTime).TotalSeconds < Settings_PatternsEngineMinimumDurationToAppend))
				return;

			// Do a merge to reduce the number of blocks
			tracings = TracingBlock.Merge(tracings, Settings_PatternsEngineMaximumBridgeableGap);

			// One block at a time (most likely, there is only one block there. There may be more if the episode is in recovery and if there is a lot of tracings to process
			foreach (var b in tracings)
			{
				// Complete overlap?
				if (b.End <= this.Engine.CurrentTime)
					continue;

				Debug.Assert(b.TotalSeconds > 0);

				var artifacts = new List<PeriGen.Patterns.Engine.Data.DetectionArtifact>();
                var arts = new List<PeriGen.Patterns.Engine.Data.DetectedObject>();

				// Partial overlap?
				if (b.Start < this.Engine.CurrentTime)
				{
					int overlap = (int)((this.Engine.CurrentTime - b.Start).TotalSeconds);
                    var candidatesToAdd = Engine.Process(b.HRs.ToArray(), b.UPs.ToArray(), overlap, b.TotalSeconds - overlap);
                    var toAdd = from c in candidatesToAdd
                                where c is DetectionArtifact
                                select c as DetectionArtifact;

					artifacts.AddRange(toAdd);
				}

				// No overlap...
				else
				{
					// A gap?
					if (b.Start > this.Engine.CurrentTime)
					{
						int gap = (int)((b.Start - this.Engine.CurrentTime).TotalSeconds);

						// Too big a gap?
						if (gap > Settings_PatternsEngineMaximumBridgeableGap)
						{
							// Drop and recreate the engine at the start of the block
							this.Engine.Dispose();
							this.Engine = new PeriGen.Patterns.Engine.PatternsEngineWrapper(b.Start, this.ToString());
						}
						// A small gap...
						else
						{
							// Patch the gap
							arts.AddRange(this.Engine.Process((byte[])Array.CreateInstance(typeof(byte), 4 * gap), (byte[])Array.CreateInstance(typeof(byte), gap)));
                            artifacts.AddRange(from c in arts where c is DetectionArtifact select c as DetectionArtifact);
						}
					}

					// Process the block
					Debug.Assert(b.Start == this.Engine.CurrentTime);
                    arts.AddRange(this.Engine.Process(b.HRs.ToArray(), b.UPs.ToArray()));
                    artifacts.AddRange(from c in arts where c is DetectionArtifact select c as DetectionArtifact);
				}

				// If the engine was primed... make sure we don't add twice some events/contractions
				if (this.EnginePrimed && artifacts.Count > 0)
				{
					artifacts.RemoveAll(a => (a.IsContraction && a.StartTime < this.EnginePrimedContractions) || (!a.IsContraction && a.StartTime < this.EnginePrimedEvents));

					// Are we past the primed buffer? It is the case if we have at least one 
					if (artifacts.Any(a => a.IsContraction))
						this.EnginePrimedContractions = DateTime.MinValue;
					if (artifacts.Any(a => !a.IsContraction))
						this.EnginePrimedEvents = DateTime.MinValue;

					this.EnginePrimed = (this.EnginePrimedContractions != DateTime.MinValue) || (this.EnginePrimedEvents != DateTime.MinValue);
				}

				// Add the artifacts to the list
				this.AddArtifacts(artifacts);
			}
		}

		/// <summary>
		/// Compress the cached tracings by merging small blocks
		/// </summary>
		/// <param name="startIndex"></param>
		/// <param name="count"></param>
		/// <returns>Number of blocks saved by compression</returns>
		int CompressCachedTracings(int startIndex, int count)
		{
			if ((startIndex < 0) || (startIndex >= this.TracingBlocks.Count))
				return 0;

			if ((count < 0) || (startIndex + count > this.TracingBlocks.Count))
				return 0;
			
			int initialCount = count;
			TracingBlock last = null;
			for (int i = startIndex; i < startIndex + count; )
			{
				TracingBlock block = this.TracingBlocks[i];

				// No current block or a very big block already or a gap that's not bridgeable
				if ((last == null)
					|| (last.TotalSeconds > Settings_CompressCachedTracingMaximumDuration)
					|| ((int)(block.Start - last.End).TotalSeconds > Settings_TracingMergeBridgeableGap))
				{
					this.LastCompressTracingID = block.Id;
					last = block;
					++i;
					continue;
				}

				// This method should NEVER be called on blocks that are not saved in database yet
				Debug.Assert(last.Id <= this.LastCommittedTracingID);
				Debug.Assert(block.Id <= this.LastCommittedTracingID);

				// Merge...
				last.Append(block);
				last.Id = block.Id;
				this.TracingBlocks.RemoveAt(i);

				--count;
			}

			return initialCount - count;
		}

		/// <summary>
		/// Compress the cached tracings by merging small blocks
		/// </summary>
		/// <returns>Number of blocks saved by compression</returns>
		public int CompressCachedTracings()
		{
			// All needs to be done under lock...
			lock (this.TracingBlocks)
			{
				// Drop blocks that are too old and will not be useful anymore
				DateTime timeLimit = DateTime.UtcNow.AddDays(-Settings_MaxDaysOfTracingReturnedToActiveX).RoundToTheSecond();
				this.TracingBlocks.RemoveAll(b => (b.Id <= this.LastCommittedTracingID) && (b.End <= timeLimit));

				// We start were the last compress ended
				int startIndex = this.TracingBlocks.FindIndex(b => b.Id >= this.LastCompressTracingID);

				// We don't compress blocks that are not yet saved in database AND 
				// We don't compress the last xxx blocks just to make sure the partial refresh from the activex are not impacted
				int endIndex = Math.Min(
									this.TracingBlocks.Count - Settings_CompressCachedTracingExcludeLastBlocks, 
									this.TracingBlocks.FindLastIndex(b => b.Id <= this.LastCommittedTracingID));

				// Nothing?
				if ((startIndex < 0) || (endIndex <= startIndex))
					return 0;

				// Compress now
				return this.CompressCachedTracings(startIndex, (endIndex - startIndex) + 1);
			}
		}

		/// <summary>
		/// Save all pending data to database
		/// </summary>
		public void SaveDataToDatabase()
		{
			// Not already mapped in db, skip!
			if (this.PatientUniqueId <= 0)
				return;

			if (string.IsNullOrEmpty(this.DatabaseFile))
			{
				Debug.Assert(false, "At that stage, the database file should be set!");
				return;
			}

			lock (this.LockDatabase)
			{
				List<TracingBlock> tracings;
				List<XUserAction> actions;
				List<DetectionArtifact> artifacts;

				lock (this.TracingBlocks)
				{
					tracings = this.TracingBlocks.Where(item => item.Id > this.LastCommittedTracingID).ToList();
				}
				lock (this.TracingActions)
				{
					actions = this.TracingActions.Where(item => item.Id > this.LastCommittedActionID).ToList();
				}
				lock (this.TracingArtifacts)
				{
					artifacts = this.TracingArtifacts.Where(item => item.Id > this.LastCommittedArtifactID).ToList();
				}

				if ((tracings.Count == 0) && (actions.Count == 0) && (artifacts.Count == 0))
				{
					// Nothing to do
					return;
				}

				using (var db = new DataContextEpisode.DataContextEpisode(GetDatabaseConnectionString(this.DatabaseFile)))
				{
					db.CreateIfNecessary();
					if (tracings.Count > 0)
					{
						this.LastCommittedTracingID = tracings.Max(item => item.Id);

						// Merge tracings for database saving in order to achieve better performances
						tracings = TracingBlock.Merge(tracings, Settings_TracingMergeBridgeableGap);
						db.Tracings.InsertAllOnSubmit(tracings.Select(item => DataContextEpisode.Tracing.From(item)));
					}
					if (actions.Count > 0)
					{
						this.LastCommittedActionID = actions.Max(item => item.Id);
						db.UserActions.InsertAllOnSubmit(actions.Select(item => DataContextEpisode.UserAction.From(item)));
					}
					if (artifacts.Count > 0)
					{
						this.LastCommittedArtifactID = artifacts.Max(item => item.Id);
						db.Artifacts.InsertAllOnSubmit(artifacts.Select(item => DataContextEpisode.Artifact.From(item)));
					}

					db.SubmitChanges();
				}
			}
		}

		/// <summary>
		/// Load all existing data from the database
		/// </summary>
		/// <returns></returns>
		public bool LoadDataFromDatabase()
		{
			Debug.Assert(!string.IsNullOrEmpty(this.DatabaseFile) && this.PatientUniqueId > 0);

			try
			{
				lock (this.LockDatabase)
				{
					if (string.IsNullOrEmpty(this.DatabaseFile))
					{
						// No db file yet
						return false;
					}

					using (var db = new DataContextEpisode.DataContextEpisode(GetDatabaseConnectionString(this.DatabaseFile)))
					{
						if (!db.DatabaseExists())
						{
							// No db file yet, so no problem there...
							return true;
						}

						this.TracingActions = db.UserActions.Select(item => item.ToXUserAction()).ToList();
						this.TracingArtifacts = db.Artifacts.Select(item => item.ToDetectionArtifact()).ToList();
						this.TracingBlocks = db.Tracings.Select(item => item.ToTracingBlock()).ToList();

						this.LastCommittedActionID = this.TracingActions.Count == 0 ? 0 : this.TracingActions.Max(item => item.Id);
						this.LastMemoryActionID = this.LastCommittedActionID;

						this.LastCommittedArtifactID = this.TracingArtifacts.Count == 0 ? 0 : this.TracingArtifacts.Max(item => item.Id);
						this.LastMemoryArtifactID = this.LastCommittedArtifactID;

						this.LastCommittedTracingID = this.TracingBlocks.Count == 0 ? 0 : this.TracingBlocks.Max(item => item.Id);
						this.LastMemoryTracingID = this.LastCommittedTracingID;

						// Update the last tracing date&time
						this.LastTracing = this.TracingBlocks.Count == 0 ? DateTime.MinValue : this.TracingBlocks.Max(b => b.End);

						// Compress ALL tracings in memory to reduce load
						this.CompressCachedTracings(0, this.TracingBlocks.Count);

						PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7301, "Episode ({0}) loaded from the database with {1:0} hours of tracings and {2} artifacts", this.FullDescription, this.HoursOfTracings, this.TracingArtifacts.Count);
					}
				}

				return true;
			}
			catch (Exception e)
			{
				// Cannot read the episode database. It must be closed automatically.
				PatternsTask.Source.TraceEvent(TraceEventType.Error, 7302, "Episode ({0}) cannot be read from the database. Episode will be closed.\nException: {1}", this, e);
				return false;
			}
		}

		/// <summary>
		/// Returns the file name associated to the given database name
		/// </summary>
		/// <param name="database"></param>
		/// <returns></returns>
		public static string GetDatabaseFileName(string database)
		{
			return Path.Combine(
						Path.GetDirectoryName(Settings_PatternsDBPath),
						string.Format(CultureInfo.InvariantCulture, "{0}.db3", database));
		}

		/// <summary>
		/// Returns the connection string associated to the given database name
		/// </summary>
		/// <param name="database"></param>
		/// <returns></returns>
		public static string GetDatabaseConnectionString(string database)
		{
			///get connection string format and options from settings
			var connectionStringFormat = Settings_DBConnectionStringFormat;
			return string.Format(CultureInfo.InvariantCulture, connectionStringFormat, GetDatabaseFileName(database));

		}

		#region IDisposable Members

		/// <summary>
		/// Destructor
		/// </summary>
		~PatternsEpisode()
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
					// Make sure there is no live thread running here
					this.StopLive();

					// Cleanup the engine
					if (this.Engine != null)
					{
						this.Engine.Dispose();
						this.Engine = null;
					}
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

		#endregion

		/// <summary>
		/// Start the collection of live tracings
		/// </summary>
		/// <param name="downtimeRecovering">Recovering from a long downtime?</param>
		public void RetrieveTracings(bool downtimeRecovering)
		{
			// Need to be in a bed to start collecting tracing...
			if ((this.EpisodeStatus != EpisodeStatuses.Normal) || (string.IsNullOrEmpty(this.BedId)) || (string.IsNullOrEmpty(this.MRN)))
			{
				this.RecoveryInProgress = false;
				this.StopLive();
				return;
			}

			// If the episode is already live, just leave now
			if (this.IsLive)
				return;

			// If the episode needs a chalkboard refresh before resume live...
			if (this.NeedChalkboardRefresh)
				return;

			this.ResetLive = false;

			// Do we need an historical call first?
			if ((DateTime.UtcNow - this.LastTracing).TotalSeconds >= Settings_LiveTracingHasMoreTracingThreshold)
			{
				var watch = Stopwatch.StartNew();

				// Retrieve historical tracing using the proper API
				using (var processor = new TracingProcessorHistoric(this.PatientUniqueId, this.Patient.MRN, this.Patient.BedId, this.LastTracing))
				{
					do
					{
						this.AddTracings(processor.ReadTracing());
						this.ProcessPatterns();

						Thread.Sleep(Settings_SleepTimeAfterEachHistorical);
					}
					while (processor.HasMoreTracing);
				}

				if (downtimeRecovering || this.RecoveryInProgress)
				{
					this.SaveDataToDatabase();
					PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7303, "Episode ({0}) recovery completed in {1} ms.", this, watch.ElapsedMilliseconds);
				}
			}

			// If this is downtime recovery, don't start the live tracing yet
			if (downtimeRecovering)
			{
				return;
			}

			// Good to go for starting a live?
			if ((DateTime.UtcNow - this.LastTracing).TotalSeconds < Settings_LiveTracingMaximumDuration)
			{
				var processor = new TracingProcessorLive(this.PatientUniqueId, this.MRN, this.BedId, this.LastTracing);
				if (processor.Open())
				{
					// Successfully open connection on live...
					this.RecoveryInProgress = false;
					this.IsLive = true;

					this.LiveThread = new Thread(() => this.DoLive(processor)) { Name = string.Format(CultureInfo.InvariantCulture, "Live tracing thread for {0}", this.PatientUniqueId) };
					this.LiveThread.Start();
				}
				else
				{
					// Flush the processor
					processor.Dispose();
				}

				Thread.Sleep(Settings_SleepTimeAfterEachLive);
			}
		}

		/// <summary>
		/// Stop the collection of live tracings if applicable
		/// </summary>
		public void StopLive()
		{
			if (this.LiveThread != null)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7304, "Stopping live tracing for '{0}'.", this);
				this.ResetLive = true;

				// Leave a little time to the thread to stop properly
				for (int i = 0; i < Settings_LiveTracingReadTimeout; ++i)
				{
					Thread.Sleep(1000);
					if (this.LiveThread == null)
					{
						break;
					}
				}
			}

			// Abort the thread if it failed to stop properly
			lock (this)
			{
				if (this.LiveThread != null)
				{
					PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7305, "Aborting live tracing for '{0}'.", this);
					this.LiveThread.Abort();
				}
			}

			// Done!
			this.ResetLive = false;
			this.IsLive = false;
			this.LiveThread = null;
		}

		/// <summary>
		/// The main method running in a secondary thread that collect the tracing
		/// </summary>
		/// <param name="processor"></param>
		void DoLive(TracingProcessorLive processor)
		{
			try
			{
				// As long as the processor is connected...
				while (processor.IsConnected && !this.ResetLive)
				{
					// Read some tracings
					this.AddTracings(processor.ReadTracing());

					// Detection of patterns
					this.ProcessPatterns();
				}

				// If the connection was close because of the need of a chalkboard refresh, flag it on the episode
				this.NeedChalkboardRefresh |= processor.NeedChalkboardRefresh;

				processor.Dispose();
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception e)
			{
				Debug.Assert(false, e.Message);
			}
			finally
			{
				this.ResetLive = false;
				this.IsLive = false;
				lock (this)
				{
					this.LiveThread = null;
				}
			}
		}
	}
}
