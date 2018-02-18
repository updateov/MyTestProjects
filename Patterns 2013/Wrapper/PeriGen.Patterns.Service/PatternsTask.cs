using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using PeriGen.Patterns.ActiveXInterface;
using PeriGen.Patterns.Engine;

namespace PeriGen.Patterns.Service
{
	/// <summary>
	/// The class that actually does the Patterns and Curve work
	/// - Collect data from the CIS
	/// - Process Patterns and Curve
	/// - Load and save data to database
	/// </summary>
	class PatternsTask
	{
        #region Members and Properties

		/// <summary>
		/// Date and time of the task startup to track for how long it's running
		/// </summary>
		static DateTime StartTime = DateTime.UtcNow;

		/// <summary>
		/// For thread safe protection
		/// </summary>
		object ThreadLock = new object();

		/// <summary>
		/// The threads used by the task
		/// </summary>
		List<Thread> WorkingThreads { get; set; }

        /// <summary>
        /// Return the connection string for the catalog database
        /// </summary>
        static string CatalogDatabaseSource { get; set; }

        /// <summary>
        /// Return the connection string for the archive catalog database
        /// </summary>
        static string ArchiveDatabaseSource { get; set; }

        volatile bool _IsStarted;

		/// <summary>
		/// Test if the task is started
		/// </summary>
		public bool IsStarted
		{
			get { return this._IsStarted; }
			protected set { this._IsStarted = value; }
		}

		volatile bool _IsInitialized;

		/// <summary>
		/// Test if the task is started
		/// </summary>
		public bool IsInitialized
		{
			get { return this._IsInitialized; }
			protected set { this._IsInitialized = value; }
		}

		volatile bool _IsStopRequested;

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

				if (!EventLog.SourceExists("PeriGen Patterns Engine"))
				{
					EventLog.CreateEventSource("PeriGen Patterns Engine", "Application");
				}
			}
			catch (Exception e)
			{
				// Log a warning but don't stop for that
				Common.Source.TraceEvent(TraceEventType.Warning, 7100, "Warning, unable to create the log source.\n{0}", e);
			}
		}

		/// <summary>
		/// Check that the computer matches the system requirements
		/// </summary>
		/// <returns></returns>
		static bool ValidateRequirements()
		{
			var error = new System.Text.StringBuilder();

			// A required core count lower or equal to 0 means no validation
            if (PatternsServiceSettings.Instance.SystemRequirements_Core_Count > 0)
			{
				int cores = ComputerInfoEx.GetCoreCount();

                if (cores < PatternsServiceSettings.Instance.SystemRequirements_Core_Count)
				{
                    error.AppendFormat("Patterns demands for {0} physical cores and this system has only {1}", PatternsServiceSettings.Instance.SystemRequirements_Core_Count, cores);
				}
			}

			// A required memory size lower or equal to 0 means no validation
            if (PatternsServiceSettings.Instance.SystemRequirements_Memory_MB > 0)
			{
				int memory = 0;
				using (var smm = new System.Management.ManagementObjectSearcher("Select TotalPhysicalMemory from Win32_ComputerSystem"))
				{
					foreach (var item in smm.Get())
					{
						memory += (int)(ulong.Parse(item["TotalPhysicalMemory"].ToString(), CultureInfo.InvariantCulture) / 1048576);
					}
				}

				if (memory < PatternsServiceSettings.Instance.SystemRequirements_Memory_MB)
				{
					if (error.Length > 0)
						error.AppendLine();

                    error.AppendFormat("Patterns demands for {0} MB of physical memory and this system has only {1} MB", PatternsServiceSettings.Instance.SystemRequirements_Memory_MB, memory);
				}
			}

			if (error.Length > 0)
			{
				Common.Source.TraceEvent(TraceEventType.Critical, 7105, "This computer does not match the minimum hardware requirements.\n{0}", error.ToString());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Make sure there is a license installed and switch to demo mode if it's not the case
		/// </summary>
		static void InitializeLicense()
		{
			// No permament license in place? Look in the registry
			if ((PeriGen.Patterns.Engine.LicenseValidation.CurrentLicense != PeriGen.Patterns.Engine.LicenseStatus.Registered)
				&& (PeriGen.Patterns.Engine.LicenseValidation.CurrentLicense != PeriGen.Patterns.Engine.LicenseStatus.TimeLimited))
			{
				//check registry to see if we need to register the engine
				try 
				{
					Microsoft.Win32.RegistryKey key;
					key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\PeriGen\PeriCALM Patterns");
					if (key != null)
					{
						//get value or default
						string result = key.GetValue("Key", string.Empty).ToString();

						//close key
						key.Close();

						//check value returned
						if (!string.IsNullOrEmpty(result))
						{
							//try to register
							var error = string.Empty;
							var success = LicenseValidation.Register(LicenseValidation.SerialID, result, out error);

							// Check if it did work
							if (success)
							{
								Common.Source.TraceEvent(TraceEventType.Information, 7118, "Successful registration.\n\n{0}", LicenseValidation.CurrentLicenseDescription);
							}
							else
							{
								Common.Source.TraceEvent(TraceEventType.Warning, 7117, "Registration failed.\n\n{0}", error);
							}
						}
					}
				}                
				catch(Exception ex)
				{                    
					Common.Source.TraceEvent(TraceEventType.Warning, 7116, "PeriGen Patterns Engine cannot read/write license from registry.\n\n{0}",ex.Message);
				}                
			}

			// Check the current license and apply a demo one if none found
			if (!PeriGen.Patterns.Engine.LicenseValidation.HasValidLicense)
			{
				PeriGen.Patterns.Engine.LicenseValidation.EnableDemoMode();
			}

			Common.DemoMode = (PeriGen.Patterns.Engine.LicenseValidation.CurrentLicense == PeriGen.Patterns.Engine.LicenseStatus.DemoMode);

			// Log current license
			Common.Source.TraceEvent(TraceEventType.Information, 7115, "PeriGen Patterns Engine license: {0}", PeriGen.Patterns.Engine.LicenseValidation.CurrentLicenseDescription);
		}

		/// <summary>
		/// Initialize and load the settings
		/// </summary>
        //static void InitializeSettings()
        //{
        //    // Encrypt the settings if not already done
        //    SettingsManager.EncryptSettings();

        //    // Make sure these are NOT the canned settings!!
        //    if ((string.IsNullOrEmpty(PatternsServiceSettings.Instance.PODILinkURL)) || (PatternsServiceSettings.Instance.PODILinkURL.IndexOf("http", StringComparison.OrdinalIgnoreCase) == -1))
        //        throw new InvalidProgramException("You must configure the settings before you can start the service!");

        //    // Log Settings 
        //    Common.Source.TraceEvent(TraceEventType.Verbose, 7120, "Settings loaded");
        //}

        /// <summary>
        /// Initialization of the database engine
        /// </summary>
        static void InitializeDB()
        {
            // Remember the settings
            string path = Path.GetDirectoryName(PatternsServiceSettings.Instance.PatternsDBPath);
            CatalogDatabaseSource = string.Format(CultureInfo.InvariantCulture, PatternsServiceSettings.Instance.DBConnectionStringFormat, Path.Combine(PatternsServiceSettings.Instance.PatternsDBPath, PatternsServiceSettings.Instance.CatalogDatabaseFilename));

            // Ensure the path exists
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to create the folder containing the database files", e);
                }

                Common.Source.TraceEvent(TraceEventType.Warning, 7130, "First time creation of the folder containing the database files {0}", path);
            }

            // Create the catalog database if necessary
            using (var db = new DataContextCatalog.DataContextCatalog(CatalogDatabaseSource))
            {
                // Make sure the catalog database exists
                if (db.CreateIfNecessary())
                {
                    Common.Source.TraceEvent(TraceEventType.Warning, 7135, "First time creation of the database containing the catalog of all episodes.");
                }

                if (!db.DatabaseExists())
                {
                    throw new Exception("Unable to create the database containing the catalog of all episodes.");
                }
            }
        }

        /// <summary>
        /// Initialization of the database engine
        /// </summary>
        static void InitializeArchiveDB()
        {
            if (!PatternsServiceSettings.Instance.PatientsDiscoverability)
                return;

            // Remember the settings
            string path = Path.GetDirectoryName(PatternsServiceSettings.Instance.ArchiveDBPath);
            ArchiveDatabaseSource = string.Format(CultureInfo.InvariantCulture, PatternsServiceSettings.Instance.DBConnectionStringFormat, Path.Combine(PatternsServiceSettings.Instance.ArchiveDBPath, PatternsServiceSettings.Instance.ArchiveCatalogDatabaseFilename));

            // Ensure the path exists
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to create the folder containing the database files", e);
                }

                Common.Source.TraceEvent(TraceEventType.Warning, 7130, "First time creation of the folder containing the database files {0}", path);
            }

            // Create the catalog database if necessary
            using (var db = new ArchiveContextCatalog.ArchiveContextCatalog(ArchiveDatabaseSource))
            {
                // Make sure the catalog database exists
                if (db.CreateIfNecessary())
                {
                    Common.Source.TraceEvent(TraceEventType.Warning, 7135, "First time creation of the archive database containing the catalog of all discharged episodes.");
                }

                if (!db.DatabaseExists())
                {
                    throw new Exception("Unable to create the archive database containing the catalog of all discharged episodes.");
                }
            }
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
					// Stop if already started (with a warning, this is not a normal workflow)
					if (this.IsStarted)
					{
						Common.Source.TraceEvent(TraceEventType.Warning, 7145, "Trying to start the task but it is already started. Stopping it now.");
						this.Stop();
					}

					this.IsInitialized = false;

					// Log starting...
					Common.Source.TraceEvent(TraceEventType.Verbose, 7150, "Starting the task");

					// Validate requirements
					if (!PatternsTask.ValidateRequirements())
						return false;

					// Load settings
					//PatternsTask.InitializeSettings();

					// Check the pattern's license
					PatternsTask.InitializeLicense();

					// Initialize database
					PatternsTask.InitializeDB();

					// Initialize archive database
                    PatternsTask.InitializeArchiveDB();

					// Initialize members
					this.IsStopRequested = false;
					this.WorkingThreads = new List<Thread>();

					// Start the threads
					var collectThread = new Thread(() => DoCollectData()) { Name = "collecting data thread" };
					collectThread.Start();
					this.WorkingThreads.Add(collectThread);

					var processThread = new Thread(() => DoProcessData()) { Name = "processing data thread" };
					processThread.Start();
					this.WorkingThreads.Add(processThread);
					
					// Started!!
					this.IsStarted = true;

					Common.Source.TraceEvent(TraceEventType.Information, 7160, "Task started ({0}, {1})",
						Assembly.GetExecutingAssembly().FullName,
						((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute))).Description);

					return true;
				}
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Critical, 7165, "Error when starting the task!\n{0}", e);
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
				Common.Source.TraceEvent(TraceEventType.Verbose, 7170, "Stopping the task");

				lock (this.ThreadLock)
				{
					// Requesting to stop
					this.IsStopRequested = true;

					// Stop service feed
					PatternsDataFeed.StopHost();

					Common.Source.TraceEvent(TraceEventType.Verbose, 7175, "WCF host stopped.");

					// Stopping the thread
					if (this.WorkingThreads != null)
					{
						foreach (var thread in this.WorkingThreads)
						{
                            if (!thread.Join(PatternsServiceSettings.Instance.ServiceStopTimeout * 1000))
							{
								Common.Source.TraceEvent(TraceEventType.Warning, 7180, "Timeout while waiting for the thread '{0}' to stop, aborting it!", thread.Name);
								thread.Abort();
							}
						}
						this.WorkingThreads.Clear();
					}

					// Make sure all data is commited to the database
					Common.Source.TraceEvent(TraceEventType.Verbose, 7185, "Committing all data to database...");
					SaveCatalog();
					SaveEpisodes();

					Common.Source.TraceEvent(TraceEventType.Verbose, 7190, "Task main loop stopped.");

					// Memory cleanup
					if (Common.Chalkboard != null)
					{
						Common.Chalkboard.Dispose();
						Common.Chalkboard = null;
					}

					GC.Collect();
					GC.WaitForPendingFinalizers();

					Common.Source.TraceEvent(TraceEventType.Information, 7195, "Task stopped ({0}, {1})",
						Assembly.GetExecutingAssembly().FullName,
						((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute))).Description);
				}
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 7200, "Error when stopping the task!\n{0}", e);
			}
			finally
			{
				this.IsStarted = false;
			}
		}

		#endregion

		/// <summary>
		/// The method for the collecting data thread
		/// </summary>
		void DoCollectData()
		{
			try
			{
				// Initialize
				var LastChalkboardRefresh = DateTime.MinValue;
				var LastMaintenance = DateTime.MinValue;
				var LastCompressTracings = DateTime.UtcNow;

				// Purge old episodes from the database if settings ask for it
				PatternsTask.PurgeClosedEpisodes();

				// Reload episode from last state in database
				PatternsTask.LoadEpisodes();

				// Start service feed
				PatternsDataFeed.StartHost();

				// Start the process thread
				this.IsInitialized = true;

				while (!this.IsStopRequested)
				{
					// Chalkboard refresh
                    if (Math.Abs((DateTime.UtcNow - LastChalkboardRefresh).TotalSeconds) >= PatternsServiceSettings.Instance.ChalkboardRefreshDelay)
					{
						LastChalkboardRefresh = DateTime.UtcNow;
						PatternsTask.RefreshChalkboard();
						PatternsTask.SaveCatalog();
						PatternsTask.RefreshPerformanceCounters();
					}

					// Compress tracing in memory
                    if (Math.Abs((DateTime.UtcNow - LastCompressTracings).TotalSeconds) >= PatternsServiceSettings.Instance.CompressCachedTracingDelay)
					{
						LastCompressTracings = DateTime.UtcNow;
						PatternsTask.CompressCachedTracings();
					}

					// Maintenance operation
                    if (Math.Abs((DateTime.UtcNow - LastMaintenance).TotalHours) >= PatternsServiceSettings.Instance.MaintenanceDelay)
					{
						LastMaintenance = DateTime.UtcNow;
						PatternsTask.PerformMaintenance();
					}

					// Just to avoid abnormal use cases where it will loop empty..
					Thread.Sleep(10);
				}
			}
			catch (ThreadAbortException)
			{
				// Just ignore
				Common.Source.TraceEvent(TraceEventType.Warning, 7205, "Thread abort in the collecting data thread. Stopping...");
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Critical, 7210, "Error in the collecting data thread!\n{0}", e);
				throw;
			}

			Common.Source.TraceEvent(TraceEventType.Verbose, 7215, "DoCollectData completed");
		}

		/// <summary>
		/// The method for the processing data thread
		/// </summary>
		void DoProcessData()
		{
			try
			{
				// Wait until the data is initialized
				while ((!this.IsStopRequested) && (!this.IsInitialized))
				{
					Thread.Sleep(250);
					continue;
				}

				var LastProcessData = DateTime.UtcNow;
				var LastCommitEpisodes = DateTime.MinValue;

				while (!this.IsStopRequested)
				{
					// Process calculations
                    if (Math.Abs((DateTime.UtcNow - LastProcessData).TotalSeconds) >= PatternsServiceSettings.Instance.ProcessDataDelay)
					{
						LastProcessData = DateTime.UtcNow;
						PatternsTask.ProcessData();
					}

					// Save episodes
                    if (Math.Abs((DateTime.UtcNow - LastCommitEpisodes).TotalSeconds) >= PatternsServiceSettings.Instance.CommitEpisodesDelay)
					{
						LastCommitEpisodes = DateTime.UtcNow;
						PatternsTask.SaveEpisodes();
					}

					// Just to avoid abnormal use cases where it will loop empty..
					Thread.Sleep(10);
				}

			}
			catch (ThreadAbortException)
			{
				// Just ignore
				Common.Source.TraceEvent(TraceEventType.Warning, 7220, "Thread abort in the processing data thread. Stopping...");
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Critical, 7225, "Error in the processing data thread!\n{0}", e);
				throw;
			}

			Common.Source.TraceEvent(TraceEventType.Verbose, 7230, "DoProcessData completed");
		}

		/// <summary>
		/// Perform some maintenance task
		/// </summary>
		static void PerformMaintenance()
		{
			try
			{
				var watch = Stopwatch.StartNew();

				// Garbage memory
				GC.Collect();
				GC.WaitForPendingFinalizers();

				// Purge all closed episodes
				PatternsTask.PurgeClosedEpisodes();

				// Update the license
				Common.DemoMode = (PeriGen.Patterns.Engine.LicenseValidation.CurrentLicense == PeriGen.Patterns.Engine.LicenseStatus.DemoMode);
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 7240, "Error while performing maintenance operation!\n{0}", e);
				throw;
			}
		}

		/// <summary>
		/// Refresh the chalkboard
		/// </summary>
		/// <returns>True if updated</returns>
		static bool RefreshChalkboard()
		{
			try
			{
				var watch = Stopwatch.StartNew();

				// Refresh chalkboard
				if (Common.Chalkboard.Refresh())
				{
					return true;
				}

				Common.Source.TraceEvent(TraceEventType.Verbose, 7250, "--- Failure to update the chalkboard.");
				return false;
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 7255, "Error while refreshing the chalkboard!\n{0}", e);
				throw;
			}
		}

		/// <summary>
		/// Save uncommited data in the episode's databases
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
					foreach (var episode in Common.Chalkboard.Episodes)
					{
                        if ((episode.EpisodeStatus == PatternsEpisode.EpisodeStatuses.Discharged) && ((DateTime.UtcNow - episode.LastMonitored).TotalHours > PatternsServiceSettings.Instance.DelayBeforeDeleteDischargedVisits))
						{
							// Close the discharged episode that are no in PODI for a while
                            episode.UpdateStatus(PatternsEpisode.EpisodeStatuses.Closed, string.Format(CultureInfo.InvariantCulture, "Discharged for more than {0} hours", PatternsServiceSettings.Instance.DelayBeforeDeleteDischargedVisits));
						}
					}

					// Get all opened episodes that are already with a database ID and not closed
					episodes = Common.Chalkboard.Episodes.Where(e => e.PatientUniqueId > 0 && e.EpisodeStatus != PatternsEpisode.EpisodeStatuses.Closed).ToList();
				}

				// Process
				foreach (var episode in episodes)
				{
					episode.SaveDataToDatabase();
				}
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 7265, "Error while saving data to the episode databases!\n{0}", e);
				throw;
			}
		}

		/// <summary>
		/// Save uncommited data in the episode catalog databases
		/// </summary>
		static void SaveCatalog()
		{
			try
			{
				var watch = Stopwatch.StartNew();

				// Get all episodes
				List<PatternsEpisode> episodes;
				lock (Common.Chalkboard.EpisodesLockObject)
				{
					// Do not save episode that are in the 'new' state, they will be saved when they 'graduate' to normal status
					episodes = Common.Chalkboard.Episodes.Where(e => (e.PatientUniqueId == 0 || e.DataUpdated) && (e.EpisodeStatus != PatternsEpisode.EpisodeStatuses.New)).ToList();
				}

				// Then, save the catalog information on episodes
				using (var db = new DataContextCatalog.DataContextCatalog(CatalogDatabaseSource))
				{
					foreach (var episode in episodes)
					{
						// Look for the patient or create one if new episode
						DataContextCatalog.Patient patient = null;

						lock (episode.LockObject)
						{
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
									Debug.Assert(false);

									Common.Source.TraceEvent(TraceEventType.Error, 7270, "Missing episode for id {0}, closing it!", episode.PatientUniqueId);

									episode.UpdateStatus(PatternsEpisode.EpisodeStatuses.Closed, "Missing episode in the database!");
									episode.DataUpdated = false; // Flag as not updated so it's just remove from the chalkboard!

									continue;
								}
							}

							// Flag the last update datetime to now
							episode.LastUpdated = DateTime.UtcNow;

							// Update the data
							patient.EpisodeStatus = (int)episode.EpisodeStatus;

							// All PHI and real data about the patient is store in one field, a blob, and the data itself is encrypted
							patient.PatientData = PatternsSecurity.Encrypt(episode.WritePatientDataToXml());

							patient.LastMonitored = episode.LastMonitored.ToEpoch();
							patient.LastUpdated = episode.LastUpdated.ToEpoch();

                            patient.VisitKey = PatternsServiceSettings.Instance.PatientsDiscoverability ? episode.Key : String.Empty;

							// Reset the updated flag
							episode.DataUpdated = false;
						}

						// Brand new episode in DB?
						if (episode.PatientUniqueId == 0)
						{
							patient.Created = DateTime.UtcNow.ToEpoch();
							patient.DatabaseFile = Guid.NewGuid().ToString("N");

							db.Patients.InsertOnSubmit(patient);
							db.SubmitChanges();

							// Retrieve the assigned patient data
							episode.PatientUniqueId = patient.PatientId;
							episode.DatabaseFile = patient.DatabaseFile;
							episode.Created = patient.Created.ToDateTime();

							Common.Source.TraceEvent(TraceEventType.Verbose, 7275, "DB '{0}' created for episode ({1})", episode.DatabaseFile, episode);
						}

						// Plain update
						else
						{
							db.SubmitChanges();
							Common.Source.TraceEvent(TraceEventType.Verbose, 7280, "DB '{0}' updated for episode ({1}).", episode.DatabaseFile, episode);
						}
					}
				}
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 7290, "Error while saving data to the episode catalog!\n{0}", e);
				throw;
			}
		}

		static int CompressCachedTracingsCalled = 0;

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
					episodes = Common.Chalkboard.Episodes.Where(e => e.IsOpen && e.PatientUniqueId > 0).ToList();
				}

				// Process
				int saved = 0;
				double hours = 0;
				foreach (var episode in episodes)
				{
					saved += episode.CompressCachedTracings();
					hours += episode.HoursOfTracings;
				}

				// Every 10 times, memory cleanup and logs
				if (++CompressCachedTracingsCalled >= 10)
				{
					CompressCachedTracingsCalled = 0;
					
					GC.Collect();
					GC.WaitForPendingFinalizers();

					// Round hours to the minute
					hours = ((long)(hours * 60)) / 60;

					Common.Source.TraceEvent(TraceEventType.Verbose, 7295,
						"Cached tracings compressed on {0} episodes in {1} ms, compressed {2} blocks. The service uses {3} MB of memory, manages {4} tracings and is running for {5}",
						episodes.Count,
						watch.ElapsedMilliseconds,
						saved,
						(Process.GetCurrentProcess().PrivateMemorySize64) / 1048576,
						new TimeSpan((long)(hours * TimeSpan.TicksPerHour)),
						DateTime.UtcNow - StartTime);
				}
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 7300, "Error while compressing cached tracings!\n{0}", e);
				throw;
			}
		}

		/// <summary>
		/// Trigger the algorithms
		/// </summary>
		static void ProcessData()
		{
			try
			{
				var watch = Stopwatch.StartNew();

				// Get all episodes
				List<PatternsEpisode> episodes;
				lock (Common.Chalkboard.EpisodesLockObject)
				{
					// Get all opened episodes that are already with a database ID
					episodes = Common.Chalkboard.Episodes.Where(e => e.IsOpen && e.PatientUniqueId > 0).ToList();
				}

				// Process
				var refreshWatch = Stopwatch.StartNew();
				foreach (var episode in episodes)
				{
					episode.ProcessData();

					if (refreshWatch.ElapsedMilliseconds > 5000)
					{
						episodes.ForEach(e => e.ProcessCurve());
						refreshWatch.Restart();
					}
				}
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 7310, "Error while processing data!\n{0}", e);
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
						// Get all live episodes that are already with a database ID and are not in recovery mode
						latencies = Common.Chalkboard.Episodes.Where(e => e.EpisodeStatus == PatternsEpisode.EpisodeStatuses.Admitted && e.IsLive && e.PatientUniqueId > 0 && !e.RecoveryInProgress).Select(e => e.RealTimeLatencyLong).ToList();
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
				Common.Source.TraceEvent(TraceEventType.Warning, 7315, "Error while refreshing live counters!\n{0}", e);
			}
		}

		/// <summary>
		/// Load all open episodes from database
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		static void LoadEpisodes()
		{
			var watch = Stopwatch.StartNew();

			// Create the chalkboard instance in Offline mode
			Common.Chalkboard = new PatternsChalkboard();

			using (var db = new DataContextCatalog.DataContextCatalog(CatalogDatabaseSource))
			{
				// Read non closed episodes
				var openEpisodes = (from p in db.Patients where p.EpisodeStatus != (int)PatternsEpisode.EpisodeStatuses.Closed select p).ToList();

				// Check all items to verify if time to close episodes
				foreach (var item in openEpisodes)
				{
					var episode = new PatternsEpisode();
					if (!episode.LoadFromDatabase(item))
					{
						// Cannot load episode. It must be closed automatically.
						Common.Source.TraceEvent(TraceEventType.Error, 7320, "Episode for id {0} cannot be loaded from database. Episode is now closed.\n{0}", item.PatientId);

						// Close episode directly in the database
						item.EpisodeStatus = (int)PatternsEpisode.EpisodeStatuses.Closed;
						db.SubmitChanges();

						Debug.Assert(false, "Unable to read the patient catalog data!");
						continue;
					}

					episode.RecoveryStatus = PatternsEpisode.RecoveryStatuses.Podi | PatternsEpisode.RecoveryStatuses.Patterns | PatternsEpisode.RecoveryStatuses.Curve;

					// Add it to the current list of tracked episodes
					Common.Chalkboard.Episodes.Add(episode);
				}
			}

			Common.Source.TraceEvent(TraceEventType.Verbose, 7325, "--- LoadEpisodes of {0} episodes done in {1} ms", Common.Chalkboard.Episodes.Count, watch.ElapsedMilliseconds);
		}

		/// <summary>
		/// If settings ask for it, delete the entries in the catalog for closed episodes and or delete the episode's files
		/// </summary>
        static void PurgeClosedEpisodes()
        {
            // Remove all closed episode from the chalkboard
            if (Common.Chalkboard != null) // The chalkboard is null the first time this method is called since it's not loaded yet
            {
                lock (Common.Chalkboard.EpisodesLockObject)
                {
                    Common.Chalkboard.Episodes.RemoveAll(e => e.EpisodeStatus == PatternsEpisode.EpisodeStatuses.Closed);
                }
            }

            // To close all open connections on the database files...
            Devart.Data.SQLite.SQLiteConnection.ClearAllPools();

            // Delete closed episodes database if necessary
            using (var db = new DataContextCatalog.DataContextCatalog(CatalogDatabaseSource))
            {
                // Oleg - in case Discoverability flag is on, don't delete the episode DB's, add the episodes to archive DB and delete from catalog
                if (!PatternsServiceSettings.Instance.PatientsDiscoverability)
                    PurgeClosedEpisodesInternal(db);
                else
                    ArchiveClosedEpisodes(db);
            }
        }

        private static void PurgeClosedEpisodesInternal(DataContextCatalog.DataContextCatalog db)
        {
            // Read closed episodes
            var episodes = (from p in db.Patients
                            where p.EpisodeStatus == (int)PatternsEpisode.EpisodeStatuses.Closed
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
                            Common.Source.TraceEvent(TraceEventType.Verbose, 7330, "Database {0} deleted for the episode {1}", episode.DatabaseFile, episode);
                        }
                    }
                    catch (Exception e)
                    {
                        Common.Source.TraceEvent(TraceEventType.Verbose, 7335, "Unable to delete the database {0} for the episode {1}.\nError: {2}", episode.DatabaseFile, episode, e);
                        continue;
                    }
                }

                // Delete from the catalog only if the database was successfully deleted
                db.Patients.DeleteOnSubmit(db.Patients.Single(e => e.PatientId == episode.PatientId));
                db.SubmitChanges();
            }
        }

        private static void ArchiveClosedEpisodes(DataContextCatalog.DataContextCatalog db)
        {
            // Read closed episodes
            var patients = (from p in db.Patients
                            where p.EpisodeStatus == (int)PatternsEpisode.EpisodeStatuses.Closed
                            select p).ToList();

            using (var arch = new ArchiveContextCatalog.ArchiveContextCatalog(ArchiveDatabaseSource))
            {
                foreach (var patient in patients)
                {
                    // Look for the patient or create one if new episode
                    ArchiveContextCatalog.Patient archPatient = arch.Patients.FirstOrDefault(p => p.VisitKey.Equals(patient.VisitKey));
                    bool bNewPatient = false;
                    if (archPatient == null)
                    {
                        archPatient = new ArchiveContextCatalog.Patient();
                        bNewPatient = true;
                    }
                    else
                    {
                        var archFile = PatternsEpisode.GetDatabaseFileName(archPatient.DatabaseFile);
                        if (File.Exists(archFile))
                            File.Delete(archFile);

                    }

                    archPatient.Created = patient.Created;
                    archPatient.DatabaseFile = patient.DatabaseFile;
                    archPatient.EpisodeStatus = patient.EpisodeStatus;
                    archPatient.LastMonitored = patient.LastMonitored;
                    archPatient.LastUpdated = patient.LastUpdated;
                    archPatient.PatientData = patient.PatientData;
                    archPatient.VisitKey = patient.VisitKey;

                    var file = PatternsEpisode.GetDatabaseFileName(patient.DatabaseFile);
                    string path = Path.GetDirectoryName(PatternsServiceSettings.Instance.ArchiveDBPath);
                    String dest = path + "\\" + patient.DatabaseFile + ".db3";
                    if (File.Exists(file))
                        File.Move(file, dest);

                    if (bNewPatient)
                        arch.Patients.InsertOnSubmit(archPatient);

                    arch.SubmitChanges();

                    // Delete from the catalog only if the database was successfully deleted
                    db.Patients.DeleteOnSubmit(db.Patients.Single(e => e.PatientId == patient.PatientId));
                    db.SubmitChanges();

                }
            }
        }
	}
}