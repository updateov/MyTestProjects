using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using PeriGen.Patterns.ActiveXInterface;
using PeriGen.Patterns.GE.Interface;
using PeriGen.Patterns.Settings;

namespace PeriGen.Patterns.GE.Service
{
	class PatternsTask
	{
		#region Settings

		static int Settings_MaintenanceDelay = SettingsManager.GetInteger("MaintenanceDelay");
		static int Settings_ChalkboardRefreshDelay = SettingsManager.GetInteger("ChalkboardRefreshDelay");
		static int Settings_CollectTracingDelay = SettingsManager.GetInteger("CollectTracingDelay");
		static int Settings_CommitEpisodesDelay = SettingsManager.GetInteger("CommitEpisodesDelay");
		static int Settings_CompressCachedTracingDelay = SettingsManager.GetInteger("CompressCachedTracingDelay");
		static int Settings_PerformanceCountersDelay = SettingsManager.GetInteger("LiveCountersDelay");
		static int Settings_EpisodeOutOfMonitorToCloseDuration = SettingsManager.GetInteger("EpisodeOutOfMonitorToCloseDuration");
		static int Settings_DelayBeforeDeleteEpisodeWhenClose = SettingsManager.GetInteger("DelayBeforeDeleteEpisodeWhenClose");
		static int Settings_ServiceStopTimeout = SettingsManager.GetInteger("ServiceStopTimeout");
		static int Settings_TimeoutToAllowLiveTracingToStop = SettingsManager.GetInteger("TimeoutToAllowLiveTracingToStop");
		static int Settings_CollectTracingMaximumElapse = SettingsManager.GetInteger("CollectTracingMaximumElapse");
		static int Settings_SystemRequirements_Core_Count = SettingsManager.GetInteger("SystemRequirements_Core_Count");
		static int Settings_SystemRequirements_Memory_MB = SettingsManager.GetInteger("SystemRequirements_Memory_MB");

		static string Settings_OBLinkURL = SettingsManager.GetValue("OBLinkURL");
		static string Settings_OBLinkUsername = SettingsManager.GetValue("OBLinkUsername");
		static string Settings_OBLinkPassword = SettingsManager.GetValue("OBLinkPassword");
		static string Settings_PatternsDBPath = SettingsManager.GetValue("PatternsDBPath");
		static string Settings_CatalogDatabaseFilename = SettingsManager.GetValue("CatalogDatabaseFilename");
		static string Settings_DBConnectionStringFormat = SettingsManager.GetValue("DBConnectionStringFormat");

		static bool Settings_DeleteEpisodeWhenClosed = SettingsManager.GetBoolean("DeleteEpisodeWhenClosed");
		static bool Settings_ShuffleEpisodeOnCollectTracings = SettingsManager.GetBoolean("ShuffleEpisodeOnCollectTracings");

		#endregion

		#region Members and Properties

		static DateTime TaskStartTime = DateTime.UtcNow;

		/// <summary>
		/// For thread safe protection
		/// </summary>
		internal object ThreadLock = new object();

		/// <summary>
		/// For thread safe protection
		/// </summary>
		internal object DatabaseLock = new object();

		/// <summary>
		/// The ONE source to use to trace data (Windows event log / DebugView...)
		/// </summary>
		public static TraceSource Source = new TraceSource("PatternsService");

		/// <summary>
		/// The main working thread thread
		/// </summary>
		protected Thread WorkingThread { get; set; }

		/// <summary>
		/// Return the connection string for the catalog database
		/// </summary>
		string CatalogDatabaseSource { get; set; }

		/// <summary>
		/// Task started or not. volatile for thread safety.
		/// </summary>
		bool _IsStarted;

		/// <summary>
		/// Test if the task is started
		/// </summary>
		public bool IsStarted
		{
			get { return this._IsStarted; }
			protected set { this._IsStarted = value; }
		}

		/// <summary>
		/// Task requested to stop or not. volatile for thread safety.
		/// </summary>
		bool _IsStopRequested;

		/// <summary>
		/// Test if the task is supposed to stop
		/// </summary>
		public bool IsStopRequested
		{
			get { return this._IsStopRequested; }
			protected set { this._IsStopRequested = value; }
		}

		#endregion

		#region Initialization methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public PatternsTask()
		{
			// Create the windows event log source entry
			try
			{
				if (!EventLog.SourceExists("PeriGen Patterns Service"))
				{
					EventLog.CreateEventSource("PeriGen Patterns Service", "Application");
				}

				if (!EventLog.SourceExists("PeriGen Patterns Interface"))
				{
					EventLog.CreateEventSource("PeriGen Patterns Interface", "Application");
				}

				if (!EventLog.SourceExists("PeriGen Patterns Engine"))
				{
					EventLog.CreateEventSource("PeriGen Patterns Engine", "Application");
				}
			}
			catch (Exception e)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Warning, 7001, "Warning, unable to create the log source.\n{0}", e);
			}
		}

		/// <summary>
		/// Check that the computer matches the system requirements
		/// </summary>
		/// <returns></returns>
		static bool ValidateRequirements()
		{
			if (Settings_SystemRequirements_Core_Count > 0)
			{
				int cores = 0;
				using (var smm = new System.Management.ManagementObjectSearcher("Select * from Win32_Processor"))
				{
					foreach (var item in smm.Get())
					{
						cores += int.Parse(item["NumberOfCores"].ToString(), CultureInfo.InvariantCulture);
					}
				}

				if (cores < Settings_SystemRequirements_Core_Count)
				{
					PatternsTask.Source.TraceEvent(TraceEventType.Error, 7102, "This computer does not match the number of cores system requirements. Patterns asks for {0} cores and this system has only {1}!", Settings_SystemRequirements_Core_Count, cores);
					return false;
				}
			}

			if (Settings_SystemRequirements_Memory_MB > 0)
			{
				int memory = 0;

				using (var smm = new System.Management.ManagementObjectSearcher("Select TotalPhysicalMemory from Win32_ComputerSystem"))
				{
					foreach (var item in smm.Get())
					{
						memory += (int)(ulong.Parse(item["TotalPhysicalMemory"].ToString(), CultureInfo.InvariantCulture) / 1048576);
					}
				}

				if (memory < Settings_SystemRequirements_Memory_MB)
				{
					PatternsTask.Source.TraceEvent(TraceEventType.Error, 7103, "This computer does not match the physical memory requirements. Patterns asks for {0} MB and this system has only {1}!", Settings_SystemRequirements_Memory_MB, memory);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Make sure there is a license installed and switch to demo mode if it's not the case
		/// </summary>
		static void InitializeLicense()
		{
			// Check the current license and apply a demo one if none found
			if (!PeriGen.Patterns.Engine.LicenseValidation.HasValidLicense)
			{
				PeriGen.Patterns.Engine.LicenseValidation.EnableDemoMode();
			}

			Common.DemoMode = (PeriGen.Patterns.Engine.LicenseValidation.CurrentLicense == PeriGen.Patterns.Engine.LicenseStatus.DemoMode);

			// Log current license
			PatternsTask.Source.TraceEvent(TraceEventType.Information, 7002, "PeriGen Patterns Engine license: {0}", PeriGen.Patterns.Engine.LicenseValidation.CurrentLicenseDescription);
		}

		/// <summary>
		/// Initialize and load the settings
		/// </summary>
		static void InitializeSettings()
		{
			// Encrypt the settings if not already done
			SettingsManager.EncryptSettings();

			// Make sure these are NOT the canned settings!!
			if ((string.IsNullOrEmpty(Settings_OBLinkURL))
				|| (string.IsNullOrEmpty(Settings_OBLinkUsername))
				|| (string.IsNullOrEmpty(Settings_OBLinkPassword)))
				throw new InvalidProgramException("Invalid setting file!");

			if ((Settings_OBLinkURL.IndexOf("ENTER-THE-OBLINK", StringComparison.OrdinalIgnoreCase) != -1)
				|| (Settings_OBLinkUsername.IndexOf("ENTER-THE-OBLINK", StringComparison.OrdinalIgnoreCase) != -1)
				|| (Settings_OBLinkPassword.IndexOf("ENTER-THE-OBLINK", StringComparison.OrdinalIgnoreCase) != -1))
				throw new InvalidProgramException("You must configure the settings before you can start the service!");

			// Log Settings 
			PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7005, "PeriGen Patterns Settings loaded.");
		}

		/// <summary>
		/// Initialization of the database engine
		/// </summary>
		/// <returns></returns>
		bool InitializeDB()
		{
			// Remember the settings
			string path = Path.GetDirectoryName(Settings_PatternsDBPath);
			this.CatalogDatabaseSource = string.Format(CultureInfo.InvariantCulture, Settings_DBConnectionStringFormat, Path.Combine(Settings_PatternsDBPath, Settings_CatalogDatabaseFilename));

			// Ensure the path exists
			if (!Directory.Exists(path))
			{
				try
				{
					Directory.CreateDirectory(path);
				}
				catch (Exception e)
				{
					PatternsTask.Source.TraceEvent(TraceEventType.Error, 7011, "Unable to create the folder containing the PeriGen PeriCALM Patterns database files {0}.\nException: {1}", path, e);
					return false;
				}

				PatternsTask.Source.TraceEvent(TraceEventType.Warning, 7012, "First time creation of the folder containing the PeriGen PeriCALM Patterns database files {0}", path);
			}

			// Create the catalog database if necessary
			lock (this.DatabaseLock)
			{
				using (var db = new DataContextCatalog.DataContextCatalog(this.CatalogDatabaseSource))
				{
					// Make sure the catalog database exists
					if (db.CreateIfNecessary())
					{
						PatternsTask.Source.TraceEvent(TraceEventType.Warning, 7013, "First time creation of the database containing the catalog of all PeriCALM Patterns episodes.");
					}

					if (!db.DatabaseExists())
					{
						PatternsTask.Source.TraceEvent(TraceEventType.Error, 7014, "Unable to create the database containing the catalog of all PeriCALM Patterns episodes.");
						return false;
					}
				}
			}

			//Done
			return true;
		}

		#endregion

		#region Task control methods

		/// <summary>
		/// Start the task
		/// </summary>
		/// <returns></returns>
		public bool Start()
		{
			try
			{
				lock (this.ThreadLock)
				{
					if (this.IsStarted)
					{
						PatternsTask.Source.TraceEvent(TraceEventType.Warning, 7006, "Trying to start the task but it is already started. Stopping it now.");
						this.Stop();
					}

					PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7007, "Starting the task ({0}, Comment={1})",
						Assembly.GetExecutingAssembly().FullName,
						((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute))).Description);

					// Validate requirements
					if (!ValidateRequirements())
						return false;

					// Load settings
					InitializeSettings();

					// Check the pattern's license
					InitializeLicense();

					// Initialize database
					if (!this.InitializeDB())
					{
						PatternsTask.Source.TraceEvent(TraceEventType.Error, 7008, "Error during initialization of the database");
						return false;
					}

					// Purge old episodes from the database if settings ask for it
					this.PurgeClosedEpisodes();

					// Reload episode from last state in database
					this.LoadEpisodes();

					// Initialize members
					this.IsStopRequested = false;

					// Start the thread
					this.WorkingThread = new Thread(() => DoWork()) { Name = "Main service thread" };
					this.WorkingThread.Start();

					// Start service feed
					PatternsDataFeed.StartHost();

					// Started!!
					this.IsStarted = true;

					PatternsTask.Source.TraceEvent(TraceEventType.Information, 7009, "Task started");
					return true;
				}
			}
			catch (Exception e)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Error, 7010, "Error when starting the task!\n{0}", e);
				this.Stop();
			}
			return false;
		}

		/// <summary>
		/// Stop the task
		/// </summary>
		/// <returns></returns>
		public void Stop()
		{
			try
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Information, 7016, "Stopping the task");

				lock (this.ThreadLock)
				{
					// Requesting to stop
					this.IsStopRequested = true;

					// Stop service feed
					PatternsDataFeed.StopHost();

					PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7017, "Task host stopped.");

					// Stopping the thread
					if (this.WorkingThread != null)
					{
						if (!this.WorkingThread.Join(Settings_ServiceStopTimeout * 1000))
						{
							PatternsTask.Source.TraceEvent(TraceEventType.Warning, 7015, "Timeout while waiting for the working thread to stop, aborting it!");
							this.WorkingThread.Abort();
						}
					}

					PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7018, "Task main loop stopped.");

					// Memory cleanup
					if (Common.Chalkboard != null)
					{
						Common.Chalkboard.Dispose();
						Common.Chalkboard = null;
					}

					PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7017, "Chalkboard disposed.");

					GC.Collect();
				}
			}
			catch (Exception e)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Error, 7018, "Error when stopping the task!\n{0}", e);
			}
			finally
			{
				this.IsStarted = false;
			}
		}

		#endregion

		/// <summary>
		/// The main working thread method
		/// </summary>
		void DoWork()
		{
			try
			{
				// Initialize
				var LastMaintenance = DateTime.MinValue;
				var LastChalkboardRefresh = DateTime.MinValue;
				var LastCollectTracings = DateTime.MinValue;
				var LastCommitEpisodes = DateTime.MinValue;
				var LastCompressTracings = DateTime.UtcNow;
				var LastPerformanceCounters = DateTime.UtcNow;
				var ChalkboardRefreshed = false;

				while (!this.IsStopRequested)
				{
					// Maintenance operation
					if (Math.Abs((DateTime.UtcNow - LastMaintenance).TotalHours) >= Settings_MaintenanceDelay)
					{
						LastMaintenance = DateTime.UtcNow;
						this.PerformMaintenance();
					}

					// Chalkboard refresh
					if (Math.Abs((DateTime.UtcNow - LastChalkboardRefresh).TotalSeconds) >= Settings_ChalkboardRefreshDelay)
					{
						LastChalkboardRefresh = DateTime.UtcNow; // Do it BEFORE the actual refresh since we want to try to keep the data actually updated every xx seconds counting the time it takes for the refresh itself
						ChalkboardRefreshed = this.RefreshChalkboard();

						// We want a collect tracing loop right after the chalkboard one since some episodes may now be online and we want the tracing to start collecting ASAP
						LastCollectTracings = DateTime.MinValue;
					}

					if (ChalkboardRefreshed)
					{
						// Collect tracings
						if (Math.Abs((DateTime.UtcNow - LastCollectTracings).TotalSeconds) >= Settings_CollectTracingDelay)
						{
							LastCollectTracings = DateTime.UtcNow;
							this.CollectTracings();
						}

						// Save episodes
						if (Math.Abs((DateTime.UtcNow - LastCommitEpisodes).TotalSeconds) >= Settings_CommitEpisodesDelay)
						{
							LastCommitEpisodes = DateTime.UtcNow;
							SaveEpisodes();
						}

						// Compress tracing in memory
						if (Math.Abs((DateTime.UtcNow - LastCompressTracings).TotalSeconds) >= Settings_CompressCachedTracingDelay)
						{
							LastCompressTracings = DateTime.UtcNow;
							CompressCachedTracings();
						}

						// Update performance counter
						if (Math.Abs((DateTime.UtcNow - LastPerformanceCounters).TotalSeconds) >= Settings_PerformanceCountersDelay)
						{
							LastPerformanceCounters = DateTime.UtcNow;
							RefreshPerformanceCounters();
						}
					}
										
					// Just to avoid abnormal use cases where it will loop empty..
					Thread.Sleep(100);
				}

				// Stop all live tracing collection
				lock (Common.Chalkboard.EpisodesLockObject)
				{
					PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7019, "Stopping the live tracings...");

					// Signal all episode to stop live collection
					foreach (var episode in Common.Chalkboard.Episodes)
					{
						episode.ResetLive = true;
					}

					// Leave them a little bit of time to actually stop
					for (int i = 0; i < Settings_TimeoutToAllowLiveTracingToStop; ++i)
					{
						Thread.Sleep(1000);
						if (!Common.Chalkboard.Episodes.Any(e => e.IsLive))
						{
							PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7020, "All live tracings properly stopped.");
							break;
						}
					}

					// Some didn't stop?
					if (Common.Chalkboard.Episodes.Any(e => e.IsLive))
					{
						PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7021, "Some live tracings are still running, aborting them...");

						// Abort the ones that would still be collecting
						foreach (var episode in Common.Chalkboard.Episodes)
						{
							episode.StopLive();
						}

						PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7022, "All live tracings stopped.");
					}
				}

				// Make sure all data is commited to the database
				PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7023, "Committing all data to database...");
				SaveEpisodes();
			}
			catch (ThreadAbortException)
			{
				// Just ignore
			}
			catch (Exception e)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Error, 7025, "Error in the main thread!\n{0}", e);
				throw;
			}

			PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7024, "Do work loop completed");
		}

		/// <summary>
		/// Perform some maintenance task
		/// </summary>
		void PerformMaintenance()
		{
			try
			{
				var watch = Stopwatch.StartNew();

				// Garbage memory
				GC.Collect();

				// Purge all closed episodes
				this.PurgeClosedEpisodes();

				// Update the license
				Common.DemoMode = (PeriGen.Patterns.Engine.LicenseValidation.CurrentLicense == PeriGen.Patterns.Engine.LicenseStatus.DemoMode);

				// Done
				PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7019, "--- Maintenance done in {0} ms", watch.ElapsedMilliseconds);
			}
			catch (Exception e)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Error, 7020, "Error while performing maintenance operation!\n{0}", e);
			}
		}

		/// <summary>
		/// Refresh the chalkboard
		/// </summary>
		/// <returns>True if updated</returns>
		bool RefreshChalkboard()
		{
			try
			{
				var watch = Stopwatch.StartNew();

				// Refresh chalkboard
				bool result = Common.Chalkboard.Refresh();

				// Commit changes to the catalog database
				this.SaveCatalog();

				lock (Common.Chalkboard.EpisodesLockObject)
				{
					// Remove closed episodes (check for updated to ensure we remove only those that where saved)
					var closed = Common.Chalkboard.Episodes.Where(e => e.EpisodeStatus == EpisodeStatuses.Closed && !e.Updated).ToList();
					closed.ForEach(e =>
						{
							Common.Chalkboard.Episodes.Remove(e);
							e.Dispose();
						});
				}

				if (result)
				{
					PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7021, "--- Chalkboard with {0} episodes updated in {1} ms.", Common.Chalkboard.Episodes.Count(e => e.IsOpen), watch.ElapsedMilliseconds);
				}
				else
				{
					PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7022, "--- Failure to update the chalkboard with {0} episodes in {1} ms.", Common.Chalkboard.Episodes.Count(e => e.IsOpen), watch.ElapsedMilliseconds);
				}

				// Try to garbage all connections...
				GC.Collect();

				return result;
			}
			catch (Exception e)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Error, 7023, "Error while refreshing the chalkboard!\n{0}", e);
				throw;
			}
		}

		/// <summary>
		/// Save uncommited data in database
		/// </summary>
		static void SaveEpisodes()
		{
			try
			{
				var watch = Stopwatch.StartNew();

				// Get all episodes
				List<PatternsEpisode> episodes;
				lock (Common.Chalkboard.EpisodesLockObject)
				{
					// Get all opened episodes that are already with a database ID
					episodes = Common.Chalkboard.Episodes.Where(e => e.EpisodeStatus != EpisodeStatuses.Closed && e.PatientUniqueId > 0).ToList();
				}

				// Process
				foreach (var episode in episodes)
				{
					episode.SaveDataToDatabase();
				}

				PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7026, "--- Episode catalog database saving on {0} episodes in {1} ms", episodes.Count, watch.ElapsedMilliseconds);
			}
			catch (Exception e)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Error, 7027, "Error while saving data to the episode catalog databases!\n{0}", e);
				throw;
			}
		}

		/// <summary>
		/// Compress the memory cache of tracings by merging small blocks
		/// </summary>
		static void CompressCachedTracings()
		{
			try
			{
				var watch = Stopwatch.StartNew();

				// Get all episodes
				List<PatternsEpisode> episodes;
				lock (Common.Chalkboard.EpisodesLockObject)
				{
					// Get all opened episodes that are already with a database ID
					episodes = Common.Chalkboard.Episodes.Where(e => e.EpisodeStatus == EpisodeStatuses.Normal && e.PatientUniqueId > 0).ToList();
				}

				// Process
				int saved = 0;
				double hours = 0;
				foreach (var episode in episodes)
				{
					saved += episode.CompressCachedTracings();
					hours += episode.HoursOfTracings;
				}

				// Round hours to the minute
				hours = ((long)(hours * 60)) / 60;

				GC.Collect();

				PatternsTask.Source.TraceEvent(
					TraceEventType.Verbose, 
					7036, 
					"--- Cached tracings compressed on {0} episodes in {1} ms, compressed {2} blocks. The service uses {3} MB of memory, manages {4} tracings and is running for {5}",
					episodes.Count, 
					watch.ElapsedMilliseconds, 
					saved,
					(Process.GetCurrentProcess().PrivateMemorySize64) / 1048576,
					new TimeSpan((long)(hours * TimeSpan.TicksPerHour)),
					DateTime.UtcNow - TaskStartTime);
			}
			catch (Exception e)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Error, 7037, "Error while compressing cached tracings!\n{0}", e);
				throw;
			}
		}

		/// <summary>
		/// Live tracing performance counters
		/// </summary>
		static void RefreshPerformanceCounters()
		{
			try
			{
				if (!Common.Chalkboard.IsOffline)
				{
					// Get all episodes
					List<long> latencies;
					lock (Common.Chalkboard.EpisodesLockObject)
					{
						// Get all opened episodes that are already with a database ID
						latencies = Common.Chalkboard.Episodes.Where(e => e.EpisodeStatus == EpisodeStatuses.Normal && e.PatientUniqueId > 0 && !e.RecoveryInProgress).Select(e => e.RealTimeLatencyLong).ToList();
					}

					// Remove invalid latencies
					latencies = latencies.Where(l => l >= 0).ToList();
					if (latencies.Count > 0)
					{
						// Update the counters
						PerformanceCounterHelper.SetAverageLatencyLivePatients((long)latencies.Average());
						PerformanceCounterHelper.SetWorstLatencyLivePatients(latencies.Max());
						return;
					}
				}

				// If we reach that point, clear counters
				PerformanceCounterHelper.SetAverageLatencyLivePatients(0);
				PerformanceCounterHelper.SetWorstLatencyLivePatients(0);
			}
			catch (Exception e)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Error, 7037, "Error while refreshing live counters!\n{0}", e);
			}
		}

		/// <summary>
		/// Collect tracings for opened episodes
		/// </summary>
		void CollectTracings()
		{
			try
			{
				var watch = Stopwatch.StartNew();

				// Make sure the tracings are where they are supposed to be
				Common.Chalkboard.ValidateLivePatientLocations();

				// Get all normal episodes that are already with a database ID
				IEnumerable<PatternsEpisode> allEpisodes;
				bool recoveryMode = false;
				lock (Common.Chalkboard.EpisodesLockObject)
				{
					// We shuffle the list just to make sure that a problem with one will no block all other episodes...
					allEpisodes = Common.Chalkboard.Episodes.Where(e => e.EpisodeStatus == EpisodeStatuses.Normal && e.PatientUniqueId > 0).ToList();
					recoveryMode = Common.Chalkboard.RecoveryInProgress;

					if (Settings_ShuffleEpisodeOnCollectTracings)
					{
						allEpisodes = allEpisodes.RandomShuffle();
					}
				}

				// First, make sure all 'live' are started when possible
				foreach (var episode in allEpisodes)
				{
					// To speed up service stop because in case of a lot of recovery, this method could take a long time...
					if (this.IsStopRequested)
						return;

					// Stop live on episodes that are not 'normal'
					episode.RetrieveTracings(recoveryMode);

					// Except when in recovery, do not go on too long collecting tracing
					if (!recoveryMode && watch.ElapsedMilliseconds > Settings_CollectTracingMaximumElapse * 1000)
					{
						PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7039, "Collecting tracings exceeded the allowed time frame of {0} seconds. Ending the loop there. Next iteration(s) will take care of the other episodes.", Settings_CollectTracingMaximumElapse);
						break;
					}
				}

				// Reset the recovery flag if need be
				if (recoveryMode)
				{
					Common.Chalkboard.RecoveryInProgress = false;
				}
			}
			catch (Exception e)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Error, 7029, "Error during the collection of tracings!\n{0}", e);
			}
		}

		/// <summary>
		/// Save the current list episode to database (changes only)
		/// </summary>
		void SaveCatalog()
		{
			lock (this.DatabaseLock)
			{
				using (var db = new DataContextCatalog.DataContextCatalog(this.CatalogDatabaseSource))
				{
					foreach (var episode in Common.Chalkboard.Episodes)
					{
						// The episode in "Normal" status must always be saved since the last monitored date and time is updated every chalkboard refresh
						if (episode.EpisodeStatus != EpisodeStatuses.Normal && !episode.Updated)
							continue;

						// Not yet in database and
						//		... the episode is already close, no need to create it ever
						//		... or the episode is involved in a merge in GE side, no need to create it yet
						//		... or the episode is in conflict with other for reconciliation
						if ((episode.PatientUniqueId == 0) && ((episode.EpisodeStatus == EpisodeStatuses.Closed) || (episode.MergeInProgress) || (!string.IsNullOrEmpty(episode.ReconciliationConflict))))
						{
							episode.Updated = false;
							continue;
						}

						// Look for the patient or create one if new episode
						DataContextCatalog.Patient patient = null;

						if (episode.PatientUniqueId == 0)
						{
							patient = new DataContextCatalog.Patient();
						}
						else
						{
							patient = db.Patients.FirstOrDefault(p => p.PatientId == episode.PatientUniqueId);

							// Missing??
							if (patient == null)
							{
								PatternsTask.Source.TraceEvent(TraceEventType.Error, 7030, "Missing episode for id {0}", episode.PatientUniqueId);
								episode.EpisodeStatus = EpisodeStatuses.Closed;
								episode.Updated = false;

								continue;
							}
						}

						// Update the data
						patient.EpisodeStatus = (int)episode.EpisodeStatus;

						// patient.DatabaseFile... no need, readonly data once the patient is created in the database
						// patient.Created... no need, readonly data once the patient is created in the database
						patient.PatientData = SettingsSecurity.Encrypt(episode.Patient.WriteToXml()); // All PHI and real data about the patient is store in one field, a blob, and the data itself is encrypted
						patient.LastMonitored = episode.LastMonitored.ToEpoch();
						patient.LastUpdated = episode.LastUpdated.ToEpoch();

						// Brand new episode in DB?
						if (episode.PatientUniqueId == 0)
						{
							episode.DatabaseFile = Guid.NewGuid().ToString("N");

							patient.Created = episode.Created.ToEpoch();
							patient.DatabaseFile = episode.DatabaseFile;

							db.Patients.InsertOnSubmit(patient);
							db.SubmitChanges();
							episode.PatientUniqueId = patient.PatientId;

							PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7035, "Creation of episode {0} in database.", episode.FullDescription);
						}

						// Plain update
						else
						{
							db.SubmitChanges();

							if (episode.Updated)
							{
								PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7035, "Update of episode {0} in database.", episode.FullDescription);
							}
						}

						// Done for that one
						episode.Updated = false;
					}
				}
			}
		}

		/// <summary>
		/// Load all open episodes from database
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		void LoadEpisodes()
		{
			var watch = Stopwatch.StartNew();

			// Create the chalkboard instance in Offline mode
			Common.Chalkboard = new PatternsChalkboard();

			lock (this.DatabaseLock)
			{
				using (var db = new DataContextCatalog.DataContextCatalog(this.CatalogDatabaseSource))
				{
					// Read non closed episodes
					var openEpisodes = (from p in db.Patients where p.EpisodeStatus != (int)EpisodeStatuses.Closed select p).ToList();

					// Set current time
					DateTime now = DateTime.UtcNow;

					///check all items to verify if time to close episodes
					foreach (var item in openEpisodes)
					{
						PatternsEpisode episode = null;

						// Deserialize episode 
						try
						{
							var patient = new PeriGen.Patterns.GE.Interface.Demographics.Patient();
							patient.ReadFromXml(SettingsSecurity.Decrypt(item.PatientData)); // Added security to the db

							// Decrypt and deserialize episode 
							episode = new PatternsEpisode
									{
										PatientUniqueId = item.PatientId,
										DatabaseFile = item.DatabaseFile,
										EpisodeStatus = (PeriGen.Patterns.GE.Interface.EpisodeStatuses)Enum.ToObject(typeof(PeriGen.Patterns.GE.Interface.EpisodeStatuses), item.EpisodeStatus),
										Created = item.Created.ToDateTime(),
										LastUpdated = item.LastUpdated.ToDateTime(),
										LastMonitored = item.LastMonitored.ToDateTime(),
										Patient = patient
									};
						}
						catch (Exception ex)
						{
							// Cannot deserialize episode. It must be closed automatically.
							PatternsTask.Source.TraceEvent(TraceEventType.Error, 7031, "Episode for id {0} cannot be deserialized from database. Episode is now closed.\nException: {1}", item.PatientId, ex);

							// Close episode directly in the database
							item.EpisodeStatus = (int)EpisodeStatuses.Closed;
							db.SubmitChanges();

							Debug.Assert(false, "Unable to read the patient catalog data!");
							continue;
						}

						// Load existing data for that patient
						if (!episode.LoadDataFromDatabase())
						{
							// Failed, close episode directly in the database
							item.EpisodeStatus = (int)EpisodeStatuses.Closed;
							db.SubmitChanges();

							Debug.Assert(false, "Unable to read the episode database!");
							continue;
						}

						// Add it to the current list of tracked episodes
						Common.Chalkboard.Episodes.Add(episode);
					}
				}
			}

			PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7049, "--- LoadEpisodes of {0} episodes done in {1} ms", Common.Chalkboard.Episodes.Count, watch.ElapsedMilliseconds);
		}

		/// <summary>
		/// If settings ask for it, delete the entries in the catalog for closed episodes and or delete the episode's files
		/// </summary>
		void PurgeClosedEpisodes()
		{
			// Delete not asked for?
			if (!Settings_DeleteEpisodeWhenClosed)
				return;

			var limit = Settings_DelayBeforeDeleteEpisodeWhenClose <= 0 ? int.MaxValue : (DateTime.UtcNow.AddDays(-Settings_DelayBeforeDeleteEpisodeWhenClose).ToEpoch());

			// To close all open connections on the database files...
			Devart.Data.SQLite.SQLiteConnection.ClearAllPools();

			// Delete closed episodes database if necessary
			lock (this.DatabaseLock)
			{
				using (var db = new DataContextCatalog.DataContextCatalog(this.CatalogDatabaseSource))
				{
					// Read closed episodes
					var episodes = (from p in db.Patients
									where p.EpisodeStatus == (int)EpisodeStatuses.Closed && p.LastUpdated <= limit
									select p).ToList();

					// Delete database
					foreach (var episode in episodes)
					{
						if (!string.IsNullOrEmpty(episode.DatabaseFile))
						{
							// Delete the database
							try
							{
								var file = PatternsEpisode.GetDatabaseFileName(episode.DatabaseFile);
								if (File.Exists(file))
								{
									File.Delete(file);
									PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 7032, "Database {0} deleted for the episode {1}", episode.DatabaseFile, episode);
								}
							}
							catch (Exception e)
							{
								PatternsTask.Source.TraceEvent(TraceEventType.Warning, 7033, "Unable to delete the database {0} for the episode {1}.\nError: {2}", episode.DatabaseFile, episode, e);
								continue;
							}
						}

						// Delete from the catalog only if the database was successfully deleted
						db.Patients.DeleteOnSubmit(db.Patients.Single(e => e.PatientId == episode.PatientId));
						db.SubmitChanges();
					}
				}
			}
		}
	}
}