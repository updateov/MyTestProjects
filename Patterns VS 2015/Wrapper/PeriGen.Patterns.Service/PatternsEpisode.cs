using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using PeriGen.Patterns.ActiveXInterface;
using PeriGen.Patterns.Curve;
using PeriGen.Patterns.Engine.Data;

namespace PeriGen.Patterns.Service
{
	/// <summary>
	/// The class that represents all data and behavior for a visit in Patterns
	/// Patient data, exams, tracings, Patterns data and Curve data
	/// </summary>
	public class PatternsEpisode : IDisposable
	{
		/// <summary>
		/// Version of the program
		/// </summary>
		static string ApplicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4);

		/// <summary>
		/// The different status of an episode
		/// </summary>
		public enum EpisodeStatuses
		{
			New = 0,
			Admitted = 1,
			Discharged = 2,
			Closed = 3,
		}

		/// <summary>
		/// The different status of recovery possible
		/// </summary>
		[Flags]
		public enum RecoveryStatuses
		{
			None = 0x00,
			Podi = 0x01,
			Patterns = 0x02,
			Curve = 0x04
		}

		/// <summary>
		/// The possible patient statuses as per PODI
		/// </summary>
		public enum PODIVisitStatus : int
		{
			Unknown = 0,
			New = 1,
			Admitted = 2
		}

		#region Configuration and constants

		/// <summary>
		/// If you change the following fields, you need to update the class PeriCALM.Patterns.Curve.UI.Chart.Common
		/// </summary>
		public const string ContextDatabaseKeyName = "_Context";
		public const string EpiduralDatabaseKeyName = "_Epidural";
		public const string VBACDatabaseKeyName = "_VBAC";
		public const string PreviousVaginalDatabaseKeyName = "_PreviousVaginal";
		public const string FirstExamDatabaseKeyName = "_FirstExam";

		#endregion

		/// <summary>
		/// ctor
		/// </summary>
		public PatternsEpisode()
		{
			this.LockObject = new object();
			this.Created = DateTime.UtcNow;

			this.FutureExamRefreshDate = DateTime.MaxValue;

			this.PatientUniqueId = 0;
			this.EpisodeStatus = EpisodeStatuses.Discharged;

			this.TracingBlocks = new List<TracingBlock>();
			this.TracingArtifacts = new List<DetectionArtifact>();
			this.TracingActions = new List<XUserAction>();
			this.PelvicExams = new List<DataContextEpisode.PelvicExam>();
			this.DataEntries = new List<DataContextEpisode.DataEntry>();
			this.CurveSnapshots = new List<DataContextEpisode.CurveSnapshot>();
		}

		#region Attributes and Properties

		/// <summary>
		/// For thread safety on accessing the data of that object
		/// </summary>
		public object LockObject { get; private set; }

		/// <summary>
		/// For thread safety on accessing the database of that object
		/// </summary>
		object LockDatabase = new object();

		/// <summary>
		/// For thread safety on processing detection and curve
		/// </summary>
		object LockProcess = new object();

		/// <summary>
		/// The unique id of the episode in the database (auto generated sequential number, does not id the patient in the real world)
		/// </summary>
		public Int32 PatientUniqueId { get; set; }

		/// <summary>
		/// The file for the database that contains the patient tracings and detection events and actions
		/// </summary>
		public string DatabaseFile { get; set; }

		volatile EpisodeStatuses _EpisodeStatus;

		/// <summary>
		/// The status of the episode
		/// </summary>
		public EpisodeStatuses EpisodeStatus { get { return this._EpisodeStatus; } set { this._EpisodeStatus = value; } }

		/// <summary>
		/// Date and time when the episode was created
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		/// Date and time when the patient was synchronized from the external system
		/// </summary>
		public DateTime LastUpdated { get; set; }

		/// <summary>
		/// Date and Time when episode went to out of monitor
		/// </summary>
		public DateTime LastMonitored { get; set; }

		/// <summary>
		/// If the last synchronization updated some data in the patient
		/// </summary>
		public bool DataUpdated { get; set; }

		volatile bool _CurveNeedRefresh;
		object __CurveNeedRefreshLock = new object();

		/// <summary>
		/// It is possible to have exams in the future in which case, when the clock reach one of these exams, the curve has to be refreshed
		/// </summary>
		DateTime FutureExamRefreshDate { get; set; }

		/// <summary>
		/// To track if the curve of that patient need to be refreshed due to data changes
		/// </summary>
		public bool CurveNeedRefresh
		{
			get { return this._CurveNeedRefresh; }
			set
			{
				lock (this.__CurveNeedRefreshLock)
				{
					this._CurveNeedRefresh = value;
				}
			}
		}

		/// <summary>
		/// List of Tracing blocks
		/// </summary>
		public List<TracingBlock> TracingBlocks { get; private set; }

		/// <summary>
		/// Number of hours of tracings
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
				if ((this.EpisodeStatus == EpisodeStatuses.Admitted) && (this.IsLive))
					return string.Format(CultureInfo.InvariantCulture, "{0:0}{1}", (DateTime.UtcNow - this.LastTracing).TotalSeconds, this.IsLate ? " (Late)" : string.Empty);

				return string.Empty;
			}
		}

		/// <summary>
		/// The latency for real time on that episode in seconds
		/// </summary>
		public long RealTimeLatencyLong
		{
			get
			{
				if (this.EpisodeStatus != EpisodeStatuses.Admitted)
					return -1;

				if (this.LastTracing == DateTime.MinValue)
					return 0;

				return Convert.ToInt64((DateTime.UtcNow - this.LastTracing).TotalMilliseconds);
			}
		}

		/// <summary
		/// List of artifacts
		/// </summary>
		public List<DetectionArtifact> TracingArtifacts { get; private set; }

		/// <summary>
		/// The id of the last artifact from TracingArtifacts that was saved in database
		/// </summary>
		int LastCommittedArtifactID { get; set; }

		/// <summary>
		/// The id of the last artifact from TracingArtifacts that was added to the memory cache
		/// </summary>
		int LastMemoryArtifactID { get; set; }

        ///// <summary>
        ///// The id of the last contractility from TracingContractilities that was saved in database
        ///// </summary>
        //int LastCommittedContractilityID { get; set; }

        ///// <summary>
        ///// The id of the last contractility from TracingContractilities that was added to the memory cache
        ///// </summary>
        //int LastMemoryContractilityID { get; set; }

        /// <summary>
		/// List of actions
		/// </summary>
		public List<XUserAction> TracingActions { get; private set; }

		/// <summary>
		/// The id of the last Actions from TracingActions that was saved in database
		/// </summary>
		int LastCommittedActionID { get; set; }

		/// <summary>
		/// The id of the last Actions from TracingActions that was added to the memory cache
		/// </summary>
		int LastMemoryActionID { get; set; }

		/// <summary>
		/// List of Pelvic exams
		/// </summary>
		public List<DataContextEpisode.PelvicExam> PelvicExams { get; private set; }

		/// <summary>
		/// The id of the last exam from PelvicExams that was saved in database
		/// </summary>
		int LastCommittedPelvicExamID { get; set; }

		/// <summary>
		/// The id of the last exam from PelvicExams that was added to the memory cache
		/// </summary>
		int LastMemoryPelvicExamID { get; set; }

		/// <summary>
		/// List of data entries
		/// </summary>
		public List<DataContextEpisode.DataEntry> DataEntries { get; private set; }

		/// <summary>
		/// The id of the last data entry from DataEntries that was saved in database
		/// </summary>
		int LastCommittedDataEntryID { get; set; }

		/// <summary>
		/// The id of the last data entry from DataEntries that was added to the memory cache
		/// </summary>
		int LastMemoryDataEntryID { get; set; }

		/// <summary>
		/// The list of curve snapshots
		/// </summary>
		public List<DataContextEpisode.CurveSnapshot> CurveSnapshots { get; private set; }

		/// <summary>
		/// The id of the last data entry from CurveSnapshots that was saved in database
		/// </summary>
		int LastCommittedSnapshotID { get; set; }

		/// <summary>
		/// The id of the last data entry from CurveSnapshots that was added to the memory cache
		/// </summary>
		int LastMemorySnapshotID { get; set; }

		/// <summary>
		/// Pattern's engine
		/// </summary>
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
		/// If some tracing processing or event detection has catch up to do
		/// </summary>
		public bool RecoveryInProgress { get { return this.RecoveryStatus != RecoveryStatuses.None; } }

		volatile RecoveryStatuses _RecoveryStatus;

		/// <summary>
		/// Recovery information for the episode
		/// </summary>
		public RecoveryStatuses RecoveryStatus
		{
			get
			{
				return this._RecoveryStatus;
			}
			set
			{
				if (this._RecoveryStatus != value)
				{
					this._RecoveryStatus = value;
					Common.Source.TraceEvent(TraceEventType.Verbose, 7600, "Recovery set to '{0}' for episode ({1})", Enum.Format(typeof(RecoveryStatuses), this.RecoveryStatus, "g"), this);
				}
			}
		}

		/// <summary>
		/// Indicate a live episode that is late retrieving the realtime tracing
		/// </summary>
		bool IsLate
		{
			get { return (this.IsLive) && ((DateTime.UtcNow - this.LastTracing).TotalSeconds > PatternsServiceSettings.Instance.TracingLateCollectingLive); }
		}

		/// <summary>
		/// Check if the episode is open
		/// </summary>
		public bool IsOpen
		{
			get
			{
				return (this.EpisodeStatus == EpisodeStatuses.Admitted || EpisodeStatus == EpisodeStatuses.Discharged) && (this.PatientUniqueId > 0);
			}
		}
       

		#endregion

		#region Patient data attributes

		/// <summary>
		/// Primary key in the external system. Cannot be changed!
		/// This cannot be PHI since it is displayed in the logs as a way to track what's happening to a given visit
		/// </summary>
		public string Key { get; set; }

		string _MRN;

		/// <summary>
		/// MRN
		/// </summary>
		public string MRN
		{
			get { return this._MRN; }
			set
			{
				if (string.CompareOrdinal(this._MRN, value) != 0)
				{
					this._MRN = value;
					this.DataUpdated = true;
				}
			}
		}

		string _AccountNo;

		/// <summary>
		/// AccountNo
		/// </summary>
		public string AccountNo
		{
			get { return this._AccountNo; }
			set
			{
				if (string.CompareOrdinal(this._AccountNo, value) != 0)
				{
					this._AccountNo = value;
					this.DataUpdated = true;
				}
			}
		}

		string _BedId;

		/// <summary>
		/// Bed id
		/// </summary>
		public string BedId
		{
			get { return this._BedId; }
			set
			{
				if (string.CompareOrdinal(this._BedId, value) != 0)
				{
					this._BedId = value;
					this.DataUpdated = true;
				}
			}
		}

		string _BedName;

		/// <summary>
		/// Bed name
		/// </summary>
		public String BedName
		{
			get { return this._BedName; }
			set
			{
				if (string.CompareOrdinal(this._BedName, value) != 0)
				{
					this._BedName = value;
					this.DataUpdated = true;
				}
			}
		}

		string _UnitName;

		/// <summary>
		/// Unit name
		/// </summary>
		public String UnitName
		{
			get { return this._UnitName; }
			set
			{
				if (string.CompareOrdinal(this._UnitName, value) != 0)
				{
					this._UnitName = value;
					this.DataUpdated = true;
				}
			}
		}

		string _FirstName;

		/// <summary>
		/// FirstName of the patient
		/// </summary>
		public string FirstName
		{
			get { return this._FirstName; }
			set
			{
				if (string.CompareOrdinal(this._FirstName, value) != 0)
				{
					this._FirstName = value;
					this.DataUpdated = true;
				}
			}
		}

		string _LastName;

		/// <summary>
		/// LastName of the patient
		/// </summary>
		public string LastName
		{
			get { return this._LastName; }
			set
			{
				if (string.CompareOrdinal(this._LastName, value) != 0)
				{
					this._LastName = value;
					this.DataUpdated = true;
				}
			}
		}

		int? _Parity;

		/// <summary>
		/// Parity
		/// </summary>
		public int? Parity
		{
			get { return this._Parity; }
			set
			{
				if (!this._Parity.Equals(value))
				{
					this._Parity = value;
					this.DataUpdated = true;
					this.CurveNeedRefresh = true;
				}
			}
		}

		int? _Fetuses;

		/// <summary>
		/// Number of fetuses
		/// </summary>
		public int? Fetuses
		{
			get { return this._Fetuses; }
			set
			{
				if (!this._Fetuses.Equals(value))
				{
					this._Fetuses = value;
					this.DataUpdated = true;
					this.CurveNeedRefresh = true;
				}
			}
		}

		string _GA;

		/// <summary>
		/// The gestational age (35+1)
		/// We get that as a string since calculation of the EDD after delivery is complex, required rules and other data like delivery datetime...
		/// </summary>
		public string GA
		{
			get { return this._GA; }
			set
			{
				if (string.CompareOrdinal(this._GA, value) != 0)
				{
					this._GA = value;
					this.DataUpdated = true;
					this.CurveNeedRefresh = true;
				}
			}
		}

		DateTime? _EDD;

		/// <summary>
		/// The EDD (estimated date of delivery) as a string, directly from the external system
		/// </summary>
		public DateTime? EDD
		{
			get { return this._EDD; }
			set
			{
				if (!this._EDD.Equals(value))
				{
					this._EDD = value;
					this.DataUpdated = true;
					this.CurveNeedRefresh = true;
				}
			}
		}

		DateTime? _Epidural;

		/// <summary>
		/// The Epidural date and time
		/// </summary>
		public DateTime? Epidural
		{
			get { return this._Epidural; }
			set
			{
				if (!this._Epidural.Equals(value))
				{
					this._Epidural = value;
					this.DataUpdated = true;
					this.CurveNeedRefresh = true;
				}
			}
		}

		bool? _VBAC;

		/// <summary>
		/// VBAC attempt?
		/// </summary>
		public bool? VBAC
		{
			get { return this._VBAC; }
			set
			{
				if (!this._VBAC.Equals(value))
				{
					this._VBAC = value;
					this.DataUpdated = true;
					this.CurveNeedRefresh = true;
				}
			}
		}

		bool? _PreviousVaginal;

		/// <summary>
		/// Previous vaginal attempt?
		/// </summary>
		public bool? PreviousVaginal
		{
			get { return this._PreviousVaginal; }
			set
			{
				if (!this._PreviousVaginal.Equals(value))
				{
					this._PreviousVaginal = value;
					this.DataUpdated = true;
					this.CurveNeedRefresh = true;
				}
			}
		}

		string _MembraneStatus;

		/// <summary>
		/// The membrane status
		/// </summary>
		public string MembraneStatus
		{
			get { return this._MembraneStatus; }
			set
			{
				if (string.CompareOrdinal(this._MembraneStatus, value) != 0)
				{
					this._MembraneStatus = value;
					this.DataUpdated = true;
					this.CurveNeedRefresh = true;
				}
			}
		}

		DateTime? _MembraneRupture;

		/// <summary>
		/// The membrane rupture datetime
		/// </summary>
		public DateTime? MembraneRupture
		{
			get { return this._MembraneRupture; }
			set
			{
				if (!this._MembraneRupture.Equals(value))
				{
					this._MembraneRupture = value;
					this.DataUpdated = true;
					this.CurveNeedRefresh = true;
				}
			}
		}

		/// <summary>
		/// Is the current episode currently set to get live tracings
		/// </summary>
		public bool IsLive { get; set; }

		/// <summary>
		/// The PODI context
		/// </summary>
		public string Context { get; set; }

		#endregion

		#region Adding data to the episode

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
					this.CurveNeedRefresh |= item.IsContraction;
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
					this.CurveNeedRefresh |= item.ActionType == ActionTypes.StrikeoutContraction || item.ActionType == ActionTypes.UndoStrikeoutContraction;

                    if (item.ActionType != ActionTypes.ConfirmEvent && item.ActionType != ActionTypes.None)
                    {
                        var artifact = (from c in TracingArtifacts
                                        where c.Id == item.ArtifactId
                                        select c).First();
                    
                        if (artifact != null && artifact is DeletableDetectedArtifact)
                            (artifact as DeletableDetectedArtifact).IsStrikedOut = item.ActionType == ActionTypes.StrikeoutContraction || item.ActionType == ActionTypes.StrikeoutEvent;
                    }
				}

				// Add all...
				this.TracingActions.AddRange(list);
			}
		}

		/// <summary>
		/// Add some more pelvic exam to the list
		/// </summary>
		/// <param name="list"></param>
		public void AddPelvicExam(IEnumerable<DataContextEpisode.PelvicExam> list)
		{
			if ((list == null) || (list.Count() == 0))
				return;

			lock (this.PelvicExams)
			{
				// Set new ID (in memory ID)...
				foreach (var item in list)
				{
					item.ExamId = ++this.LastMemoryPelvicExamID;
				}
				// Add all...
				this.PelvicExams.AddRange(list);
				this.CurveNeedRefresh = true;
			}
		}

		/// <summary>
		/// Add some more data entries to the list
		/// </summary>
		/// <param name="list"></param>
		public void AddDataEntry(IEnumerable<DataContextEpisode.DataEntry> list)
		{
			if ((list == null) || (list.Count() == 0))
				return;

			lock (this.DataEntries)
			{
				// Set new ID (in memory ID)...
				foreach (var item in list)
				{
					item.EntryId = ++this.LastMemoryDataEntryID;
				}
				// Add all...
				this.DataEntries.AddRange(list);
				this.CurveNeedRefresh = true;
			}
		}

		/// <summary>
		/// Add a new curve snapshot to the list
		/// </summary>
		/// <param name="item"></param>
		public void AddSnapshot(DataContextEpisode.CurveSnapshot item)
		{
			lock (this.CurveSnapshots)
			{
				// Set new ID (in memory ID)...
				item.SnapshotId = ++this.LastMemorySnapshotID;

				// Add all...
				this.CurveSnapshots.Add(item);
			}
		}

		#endregion

		#region For the Patterns and Curve data exchange

		/// <summary>
		/// Encode a Patient into an XElement that can be sent to the patterns client
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public XElement EncodeForRequest()
		{
			lock (this.LockObject)
			{
				return new XElement("patient",
						new XAttribute("id", this.PatientUniqueId),
						new XAttribute("mrn", this.MRN ?? string.Empty),
						new XAttribute("status", (int)this.PatientStatus),
						new XAttribute("readonly", this.EpisodeStatus != EpisodeStatuses.Admitted),
						new XAttribute("statusdetails", this.PatientStatusDetails),
						new XAttribute("firstname", this.FirstName ?? string.Empty),
						new XAttribute("lastname", this.LastName ?? string.Empty),
						new XAttribute("edd", this.EDD.ToEpoch() ?? 0),
						new XAttribute("ga", this.GA ?? string.Empty),
						new XAttribute("reset", this.Created.ToEpoch()),
						new XAttribute("fetus", this.Fetuses ?? 0));
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
					case EpisodeStatuses.Admitted:
						{
							if (this.RecoveryInProgress)
								return StatusType.Recovery;

							if (this.IsLate)
								return StatusType.Late;

							if (this.IsLive)
								return StatusType.Live;

							return StatusType.Unplugged;
						}

					case EpisodeStatuses.Discharged:
						{
							if (this.RecoveryInProgress)
								return StatusType.Recovery;

							return StatusType.Unplugged;
						}

					case EpisodeStatuses.Closed:

					default:
						return StatusType.Invalid;
				}
			}
		}

		/// <summary>
		/// Return the patient Status explanation for the active X according the the state of the episode
		/// </summary>
		string PatientStatusDetails
		{
			get
			{
				switch (this.EpisodeStatus)
				{
					case EpisodeStatuses.Admitted:
					case EpisodeStatuses.Discharged:
						if (this.RecoveryInProgress)
							return "Data is being recovered. Please wait...";
						return string.Empty;

					default:
						return "No data is available for the selected patient.";
				}
			}
		}

		#endregion

		#region Curve calculation

		#region Curve Formula

		/// <summary>
		/// Calculate the percentile and the expected dilation
		/// </summary>
		/// <param name="dilatation">Exam Dilatation value</param>
		/// <param name="effacement">Exam Effacement value</param>
		/// <param name="station">Exam Station value</param>
		/// <param name="dilatation">Next Exam Dilatation value</param>
		/// <param name="contractions">Number of contractions since first exam</param>
		/// <param name="epidural">Epidural administered</param>
		/// <param name="previous_vaginal">Previous vaginal deliveries</param>
		/// <returns></returns>
		static double CalculateExpectedDilatation(double dilatation, double effacement, double station, double contractions, bool epidural, bool previous_vaginal)
		{
			// Multiple vaginal delivery
			if (previous_vaginal)
			{
				return 1.9914 + (0.015503 * effacement) + (0.00766 * contractions) + (0.777 * dilatation) - (0.20669 * (3 - station)) + (epidural ? 0 : 0.23484);
			}

			// First vaginal delivery
			return 0.24952 + (0.024109 * effacement) + (0.00447 * contractions) + (0.80515 * dilatation) - (0.03909 * (3 - station)) + (epidural ? 0 : 0.14627);
		}

		/// <summary>
		/// Calculate the percentile
		/// </summary>
		/// <param name="expectedDilatation">Expected dilatation calculated</param>
		/// <param name="dilatation">Next exam dilatation</param>
		/// <param name="previous_vaginal"></param>
		/// <returns></returns>
		static double CalculatePercentile(double expectedDilatation, double dilatation, bool previous_vaginal)
		{
			double standardError = previous_vaginal ? 1.3451 : 1.3014;
			double value = (dilatation - expectedDilatation) / standardError;

			if ((value < -4.0) || (value > 4.0))
			{
				return 0.0;
			}

			return CalculateCumulativeNormalDistribution(value) * 100;
		}

		/// <summary>
		/// Get lower/upper percentile factor
		/// </summary>
		/// <param name="limit">The upper/lower percentile limit (3, 5 or 10)</param>
		/// <param name="previous_vaginal"></param>
		/// <returns></returns>
		static double GetPercentileFactor(int limit, bool previous_vaginal)
		{
			double dStandardError = previous_vaginal ? 1.3451 : 1.3014;

			switch (limit)
			{
				case 3:
					return 1.8810 * dStandardError;

				case 5:
					return 1.6450 * dStandardError;

				case 10:
					return 1.2800 * dStandardError;
			}

			System.Diagnostics.Debug.Assert(false, "Unknown limit: " + limit.ToString());
			return 0;
		}

		/// <summary>
		/// Standard normal cumulative distribution function
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		static double CalculateCumulativeNormalDistribution(double value)
		{
			double result;
			if (value < -7)
			{
				result = CalculateNormalDistribution(value) / Math.Sqrt(1 + value * value);
			}
			else if (value > 7)
			{
				result = 1 - CalculateCumulativeNormalDistribution(-value);
			}
			else
			{
				result = 1 / (1 + 0.2316419 * Math.Abs(value));
				result = 1 - CalculateNormalDistribution(value) * (result * (0.31938153 + result * (-0.356563782 + result * (1.781477937 + result * (-1.821255978 + result * 1.330274429)))));
				if (value <= 0)
				{
					result = 1 - result;
				}
			}

			return result;
		}

		/// <summary>
		/// Standard normal density function
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		static double CalculateNormalDistribution(double t)
		{
			return 0.398942280401433 * Math.Exp(-t * t / 2);
		}

		#endregion

		/// <summary>
		/// Generate the curve error message that matches the given error reason(s)
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		static string BuildCurveMessage(CurveCalculationReasons reason)
		{
			if (reason == CurveCalculationReasons.None)
				return string.Empty;

			var sb = new StringBuilder(1000);
			sb.AppendLine("The comparison curves will not be calculated until these issues have been resolved.");

			if ((reason & CurveCalculationReasons.MissingEDD) != 0)
				sb.AppendLine("The EDD has not been entered.");

			if ((reason & CurveCalculationReasons.NotSingleton) != 0)
				sb.AppendLine("The Number of Fetuses is not set to Singleton.");

			if ((reason & CurveCalculationReasons.MissingParity) != 0)
				sb.AppendLine("The patient's parity has not been entered.");

			if ((reason & CurveCalculationReasons.InvalidParity) != 0)
				sb.AppendLine("The patient’s parity is invalid.");

			if ((reason & CurveCalculationReasons.MissingVBAC) != 0)
				sb.AppendLine("The Attempted VBAC option has not been entered.");

			if ((reason & CurveCalculationReasons.MissingPreviousVaginal) != 0)
				sb.AppendLine("The First Vaginal Delivery option has not been entered.");

			if ((reason & CurveCalculationReasons.ParityNoFirstVaginalNoVBAC) != 0)
				sb.AppendLine("The patient’s parity must be at least 1 if this is not a VBAC attempt and the patient has had at least one previous vaginal delivery.");

			if ((reason & CurveCalculationReasons.ParityFirstVaginalVBAC) != 0)
				sb.AppendLine("The patient’s parity must be at least 1 if this is a VBAC attempt and the patient has never had previous vaginal deliveries.");

			if ((reason & CurveCalculationReasons.ParityFirstVaginalNoVBAC) != 0)
				sb.AppendLine("The patient’s parity must be 0 if this is not a VBAC attempt and the patient has never had previous vaginal deliveries.");

			if ((reason & CurveCalculationReasons.ParityNoFirstVaginalVBAC) != 0)
				sb.AppendLine("The patient’s parity must be at least 2 if this is a VBAC attempt and the patient has had at least one previous vaginal delivery.");

			if ((reason & CurveCalculationReasons.NotEnoughExams) != 0)
				sb.AppendLine("Insufficient number of applicable vaginal exams.");

			if ((reason & CurveCalculationReasons.NoContractions) != 0)
				sb.AppendLine("No contractions have been detected since the first exam.");

			if ((reason & CurveCalculationReasons.Below35week) != 0)
				sb.AppendLine("Gestational Age is below 35 weeks.");

			if ((reason & CurveCalculationReasons.MissingDilatation) != 0)
				sb.AppendLine("Dilation is missing from one or more exams.");

			if ((reason & CurveCalculationReasons.MissingEffacement) != 0)
				sb.AppendLine("Effacement is missing from one or more exams.");

			if ((reason & CurveCalculationReasons.MissingStation) != 0)
				sb.AppendLine("Station is missing from one or more exams.");

			if ((reason & CurveCalculationReasons.MissingPresentation) != 0)
				sb.AppendLine("Presentation is missing from one or more exams.");

			if ((reason & CurveCalculationReasons.InvalidDilatation) != 0)
				sb.AppendLine("Dilation is invalid in one or more exams.");

			if ((reason & CurveCalculationReasons.InvalidEffacement) != 0)
				sb.AppendLine("Effacement is invalid in one or more exams.");

			if ((reason & CurveCalculationReasons.InvalidStation) != 0)
				sb.AppendLine("Station is invalid in one or more exams.");

			if ((reason & CurveCalculationReasons.DescendingDilatation) != 0)
				sb.AppendLine("Dilation has descended.");

			if ((reason & CurveCalculationReasons.NotCephalic) != 0)
				sb.AppendLine("Not all Presentations are Cephalic.");

			if ((reason & CurveCalculationReasons.ExamInFarFuture) != 0)
				sb.AppendLine("At least one exam is time stamped more than 5 minutes in the future.");

			if (sb.Length == 0)
			{
				Debug.Assert(false);
				sb.AppendLine("Unknown error.");
			}

			// Remove empty lines...
			return sb.ToString(0, Math.Max(0, sb.Length - Environment.NewLine.Length));
		}

		/// <summary>
		/// Generate the curve error message that matches the given error reason(s)
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		static string BuildExamMessage(CurveCalculationReasons reason)
		{
			if (reason == CurveCalculationReasons.None)
				return string.Empty;

			var sb = new StringBuilder(1000);

			if ((reason & CurveCalculationReasons.Below35week) != 0)
				sb.AppendLine("Gestational Age is below 35 weeks.");

			if ((reason & CurveCalculationReasons.MissingDilatation) != 0)
				sb.AppendLine("Dilation is missing.");

			if ((reason & CurveCalculationReasons.MissingEffacement) != 0)
				sb.AppendLine("Effacement is missing.");

			if ((reason & CurveCalculationReasons.MissingStation) != 0)
				sb.AppendLine("Station is missing.");

			if ((reason & CurveCalculationReasons.MissingPresentation) != 0)
				sb.AppendLine("Presentation is missing.");

			if ((reason & CurveCalculationReasons.InvalidDilatation) != 0)
				sb.AppendLine("Dilation must be between 0 cm and 10.0 cm.");

			if ((reason & CurveCalculationReasons.InvalidEffacement) != 0)
				sb.AppendLine("Effacement must be between 0% and 100%.");

			if ((reason & CurveCalculationReasons.InvalidStation) != 0)
				sb.AppendLine("Station must be between -5 and +5.");

			if ((reason & CurveCalculationReasons.DescendingDilatation) != 0)
				sb.AppendLine("Dilation has descended.");

			if ((reason & CurveCalculationReasons.NotCephalic) != 0)
				sb.AppendLine("Presentation is not Cephalic.");

			if ((reason & CurveCalculationReasons.DilationBelow3cm) != 0)
				sb.AppendLine("Dilation is below 3.0 cm.");

			if ((reason & CurveCalculationReasons.ExamInNearFuture) != 0)
				sb.AppendLine("Exam is time stamped in the future.");

			if ((reason & CurveCalculationReasons.ExamInFarFuture) != 0)
				sb.AppendLine("Exam is time stamped in the future.");

			// Remove empty lines...
			return sb.ToString(0, Math.Max(0, sb.Length - Environment.NewLine.Length));
		}

		/// <summary>
		/// Helper to write a dataentry in the xml for the curve
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="elementName"></param>
		/// <param name="entryName"></param>
		void WriteDataEntry(XmlWriter writer, string elementName, string entryName)
		{
			writer.WriteStartElement(elementName);

			var entry = this.GetDataEntry(entryName);
			if (entry == null)
			{
				writer.WriteAttributeString("value", string.Empty);
				writer.WriteAttributeString("lastUpdate", string.Empty);
				writer.WriteAttributeString("userId", string.Empty);
				writer.WriteAttributeString("userName", string.Empty);
			}
			else
			{
				writer.WriteAttributeString("value", entry.Value ?? string.Empty);
				writer.WriteAttributeString("lastUpdate", entry.UpdateTime.ToDateTime().ToString("s", CultureInfo.InvariantCulture));
				writer.WriteAttributeString("userId", entry.UserId ?? string.Empty);
				writer.WriteAttributeString("userName", entry.UserName ?? string.Empty);
			}

			writer.WriteEndElement();
		}

		/// <summary>
		/// Suffix to have a nice english sentence... 1st ... 2nd...
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		static string SuffixForNumber(int number)
		{
			if (number <= 0)
				return string.Empty;
			if (number == 1)
				return "st";
			if (number == 2)
				return "nd";
			if (number == 3)
				return "rd";
			return "th";
		}

		/// <summary>
		/// Small helper...
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		static DateTime Min(DateTime a, DateTime b)
		{
			return a > b ? b : a;
		}

		object __ProcessCurveLock = new object();

		/// <summary>
		/// Process the Curve algorithm
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void ProcessCurve()
		{
			lock (__ProcessCurveLock)
			{
				var now = DateTime.UtcNow;

				try
				{
					// If the data is still in recovery from PODI or Patterns, just leave now...
					if ((this.RecoveryStatus & (RecoveryStatuses.Podi | RecoveryStatuses.Patterns)) != RecoveryStatuses.None)
					{
						return;
					}

					// The update of the curve need refresh must be done under lock protection so as to not loose a 'invalidation' that would occur between the check and reset...
					lock (this.__CurveNeedRefreshLock)
					{
						// If there is an exam in the future, the curve needs to be refreshed when it falls back in the present...
						this.CurveNeedRefresh |= (now >= this.FutureExamRefreshDate);

						// If the curve is marked as in recovery, the curve needs to be refreshed
						this.CurveNeedRefresh |= ((this.RecoveryStatus & RecoveryStatuses.Curve) != RecoveryStatuses.None);

						// If the curve does not need refresh, leave now... 
						if (!this.CurveNeedRefresh)
						{
							return;
						}

						// Reset the flag
						this.CurveNeedRefresh = false;

						// Reset future exams invalidation date
						this.FutureExamRefreshDate = DateTime.MaxValue;
					}

					#region Populate the data and validate it

					// Grab some useful data
					var contractions = this.ActiveContractions;
					var curveExams = this.ActiveExams.Select(e => new CurveExam(e)).OrderBy(e => e.Time).ToList();

					DataContextEpisode.DataEntry entry = null;

					#region VBAC

					bool? vbac = null;
                    if (PatternsServiceSettings.Instance.PODISendVBAC)
					{
						vbac = this.VBAC;
					}
					else
					{
						entry = this.GetDataEntry(VBACDatabaseKeyName);
						if (entry != null)
						{
							bool value;
							if (bool.TryParse(entry.Value, out value))
							{
								vbac = value;
							}
						}
					}

					#endregion

					#region Previous Vaginal Delivery

					bool? previousVaginal = null;
                    if (PatternsServiceSettings.Instance.PODISendVaginalDelivery)
					{
						previousVaginal = this.PreviousVaginal;
					}
					else
					{
						entry = this.GetDataEntry(PreviousVaginalDatabaseKeyName);
						if ((entry != null) && (!string.IsNullOrEmpty(entry.Value)))
						{
							bool value;
							if (bool.TryParse(entry.Value, out value))
							{
								previousVaginal = value;
							}
						}
					}

					#endregion

					#region Epidural

					DateTime? epidural = null;
                    if (PatternsServiceSettings.Instance.PODISendEpidural)
					{
						epidural = this.Epidural;
					}
					else
					{
						entry = this.GetDataEntry(EpiduralDatabaseKeyName);
						if ((entry != null) && (!string.IsNullOrEmpty(entry.Value)))
						{
							DateTime value;
							if (DateTime.TryParseExact(entry.Value, "s", CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
							{
								epidural = value;
							}
						}
					}

					#endregion

					#region First Exam

					var firstExam = DateTime.MinValue;
					entry = this.GetDataEntry(FirstExamDatabaseKeyName);
					if ((entry != null) && (!string.IsNullOrEmpty(entry.Value)))
					{
						DateTime value;
						if (DateTime.TryParseExact(entry.Value, "s", CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
						{
							firstExam = value;
						}
					}

					// Validate first exam still exists in curve exams. //Remove it from entry if does not exist anymore.
					if (firstExam > DateTime.MinValue)
					{
						var fe = curveExams.FirstOrDefault(f => f.Time == firstExam);
						if (fe == null)
						{
							// Set First Exam to null
							this.AddDataEntry(new List<DataContextEpisode.DataEntry>() 
							{ 
								new DataContextEpisode.DataEntry 
								{ 
									Name=PatternsEpisode.FirstExamDatabaseKeyName,
									Value=string.Empty,
									UserId="SYSTEM",
									UserName="SYSTEM (First exam was deleted)",
									UpdateTime= now.ToEpoch()                                        
								} 
							});

							// Reset first exam value
							firstExam = DateTime.MinValue;
						}
					}

					// No explicit first exam means the first exam is the actually the FIRST one
					if ((firstExam == DateTime.MinValue) && (curveExams.Count > 0))
					{
						firstExam = curveExams.First().Time;
					}

					#endregion

					// Validate the 'curve' level rules
					var curveStatus = CurveCalculationStatuses.Valid;
					var curveReason = CurveCalculationReasons.None;

					if ((!this.Fetuses.HasValue) || (this.Fetuses.Value != 1))
					{
						curveStatus = CurveCalculationStatuses.Error;
						curveReason |= CurveCalculationReasons.NotSingleton;
					}

					if ((!EDD.HasValue) || (EDD.Value == DateTime.MinValue))
					{
						curveStatus = CurveCalculationStatuses.Error;
						curveReason |= CurveCalculationReasons.MissingEDD;
					}

					if (!this.Parity.HasValue)
					{
						curveStatus = CurveCalculationStatuses.Error;
						curveReason |= CurveCalculationReasons.MissingParity;
					}
					else if (this.Parity.Value < 0)
					{
						curveStatus = CurveCalculationStatuses.Error;
						curveReason |= CurveCalculationReasons.InvalidParity;
					}

					if (!vbac.HasValue)
					{
						curveStatus = CurveCalculationStatuses.Error;
						curveReason |= CurveCalculationReasons.MissingVBAC;
					}

					if (!previousVaginal.HasValue)
					{
						curveStatus = CurveCalculationStatuses.Error;
						curveReason |= CurveCalculationReasons.MissingPreviousVaginal;
					}

					if (this.Parity.HasValue && vbac.HasValue && previousVaginal.HasValue && this.Parity.Value >= 0)
					{
						// At least one previous vaginal birth and another previous cesarienne birth means ==> Minimum 2 previous birth total
						if (previousVaginal.Value && vbac.Value)
						{
							if (this.Parity.Value < 2)
							{
								curveStatus = CurveCalculationStatuses.Error;
								curveReason |= CurveCalculationReasons.ParityNoFirstVaginalVBAC;
							}
						}

						// At least one previous vaginal birth ==> Minimum 1 previous birth total
						else if (previousVaginal.Value)
						{
							if (this.Parity.Value < 1)
							{
								curveStatus = CurveCalculationStatuses.Error;
								curveReason |= CurveCalculationReasons.ParityNoFirstVaginalNoVBAC;
							}
						}

						// At least one previous cesarienne birth ==> Minimum 1 previous birth total
						else if (vbac.Value)
						{
							if (this.Parity.Value < 1)
							{
								curveStatus = CurveCalculationStatuses.Error;
								curveReason |= CurveCalculationReasons.ParityFirstVaginalVBAC;
							}
						}

						// No previous vaginal nor cesarienne birth ==> Parity must be 0
						else if (this.Parity.Value != 0)
						{
							curveStatus = CurveCalculationStatuses.Error;
							curveReason |= CurveCalculationReasons.ParityFirstVaginalNoVBAC;
						}
					}

					if (!contractions.Any(c => c.PeakTime >= firstExam))
					{
						curveStatus = CurveCalculationStatuses.Error;
						curveReason |= CurveCalculationReasons.NoContractions;
					}

					long eddLimit = 0;
					if ((this.EDD.HasValue) && (this.EDD.Value != DateTime.MinValue))
					{
						var day35Weeks = this.EDD.Value.AddDays(-35);
						if (day35Weeks.Hour > 12)
						{
							day35Weeks = day35Weeks.AddDays(1);
						}
						day35Weeks = new DateTime(day35Weeks.Year, day35Weeks.Month, day35Weeks.Day);
						eddLimit = day35Weeks.ToUniversalTime().ToEpoch(); // EDD change at midnight LOCAL TIME!
					}

					// Validate the exam level rules
					var previousDilatation = double.MinValue;
					var previousPresentation = string.Empty;

					foreach (var e in curveExams)
					{
						// If the presentation is not populated, carry forward the last entered presentation
						var presentation = e.Presentation;
						if ((string.IsNullOrWhiteSpace(presentation)) && (!string.IsNullOrWhiteSpace(previousPresentation)))
						{
							e.Presentation = "(" + previousPresentation + ")";
							presentation = previousPresentation;
						}
						previousPresentation = presentation;

						// Ignore all exams before the one marked as 'first exam'
						if (e.Time < firstExam)
						{
							e.Status = CurveCalculationStatuses.Ignored;
							e.StatusReason |= CurveCalculationReasons.BeforeFirstExam;
							continue;
						}

						// Ignore all exams once a dilatation of 10 is reached
						if (previousDilatation >= 10)
						{
							e.Status = CurveCalculationStatuses.Ignored;
							e.StatusReason |= CurveCalculationReasons.AlreadyReach10cm;
							continue;
						}

						// Exams more than 5 minutes in future is an error
						if ((e.Time - now).TotalMinutes > 5)
						{
							e.Status = CurveCalculationStatuses.Error;
							e.StatusReason |= CurveCalculationReasons.ExamInFarFuture;

							this.FutureExamRefreshDate = PatternsEpisode.Min(this.FutureExamRefreshDate, e.Time.AddMinutes(-5));
							continue;
						}

						// Exams less than 5 minutes but more than 1 in future is a warning (the exam is not used in the calculation but that does not block the curve calculation)
						if ((e.Time - now).TotalMinutes > 1)
						{
							e.Status = CurveCalculationStatuses.Info;
							e.StatusReason |= CurveCalculationReasons.ExamInNearFuture;

							this.FutureExamRefreshDate = PatternsEpisode.Min(this.FutureExamRefreshDate, e.Time.AddMinutes(-1));
							continue;
						}

						// Check dilatation (first check since we need to know if we reach 3 cm...)
						if (string.IsNullOrEmpty(e.Exam.Dilatation))
						{
							e.Status = CurveCalculationStatuses.Error;
							e.StatusReason |= CurveCalculationReasons.MissingDilatation;
						}
						else
						{
							double value;
							if ((!double.TryParse(e.Exam.Dilatation, out value)) || (value < 0) || (value > 10))
							{
								e.Status = CurveCalculationStatuses.Error;
								e.StatusReason |= CurveCalculationReasons.InvalidDilatation;
							}
							else
							{
								e.Dilatation = value;

								if (value < 3)
								{
									e.Status = CurveCalculationStatuses.Info; // Dilatation below 3cm is just a warning, does not block the curve rendering
									e.StatusReason |= CurveCalculationReasons.DilationBelow3cm;
								}

								if (value < previousDilatation)
								{
									e.Status = CurveCalculationStatuses.Error;
									e.StatusReason |= CurveCalculationReasons.DescendingDilatation;
								}
								previousDilatation = value;
							}
						}

						// Check presentation
						if (string.IsNullOrWhiteSpace(presentation))
						{
							e.Status = CurveCalculationStatuses.Error;
							e.StatusReason |= CurveCalculationReasons.MissingPresentation;
						}
						else
						{
							// Check if cephalic...
                            if (!PatternsServiceSettings.Instance.CephalicCodes.Any(code => string.CompareOrdinal(presentation.ToUpperInvariant(), code) == 0))
							{
								e.Status = CurveCalculationStatuses.Error;
								e.StatusReason |= CurveCalculationReasons.NotCephalic;
							}
						}

						// Check effacement
						if (string.IsNullOrEmpty(e.Exam.Effacement))
						{
							e.Status = CurveCalculationStatuses.Error;
							e.StatusReason |= CurveCalculationReasons.MissingEffacement;
						}
						else
						{
							int value;
							if ((!int.TryParse(e.Exam.Effacement, out value)) || (value < 0) || (value > 100))
							{
								e.Status = CurveCalculationStatuses.Error;
								e.StatusReason |= CurveCalculationReasons.InvalidEffacement;
							}
							else
							{
								e.Effacement = value;
							}
						}

						// Check station
						if (string.IsNullOrEmpty(e.Exam.Station))
						{
							e.Status = CurveCalculationStatuses.Error;
							e.StatusReason |= CurveCalculationReasons.MissingStation;
						}
						else
						{
							int value;
							if ((!int.TryParse(e.Exam.Station, out value)) || (value < -5) || (value > +5))
							{
								e.Status = CurveCalculationStatuses.Error;
								e.StatusReason |= CurveCalculationReasons.InvalidStation;
							}
							else
							{
								e.Station = value;
							}
						}

						if (e.Exam.Time < eddLimit)
						{
							e.Status = CurveCalculationStatuses.Error;
							e.StatusReason |= CurveCalculationReasons.Below35week;
						}
					}

					// Need at least ONE point on the curve
					var curvePoints = curveExams.Count(e =>
						(e.Status != CurveCalculationStatuses.Ignored)								// Not counting those before the first exam
							&& (e.Dilatation >= 3)													// Not counting those with dilatation below 3
							&& ((e.StatusReason & CurveCalculationReasons.ExamInNearFuture) == 0));	// Not counting those in the future

					if ((curvePoints == 1) && (curveExams.Any(e => (e.Status != CurveCalculationStatuses.Ignored) && (e.Dilatation < 3))))
					{
						curvePoints = 2;
					}

					if (curvePoints < 2)
					{
						curveStatus = CurveCalculationStatuses.Error;
						curveReason |= CurveCalculationReasons.NotEnoughExams;
					}

					// Update the global curve status with the sum of the exam statuses
					foreach (var e in curveExams)
					{
						if (e.Status == CurveCalculationStatuses.Error)
						{
							curveStatus = CurveCalculationStatuses.Error;
							curveReason |= e.StatusReason;
						}
					}

					#endregion

					#region Populate contractions count and interval in exams (all exams)

					foreach (var e in curveExams.Where(e => e.Status == CurveCalculationStatuses.Valid))
					{
						e.ContractionsSinceFirstExam = contractions.Count(c => (c.PeakTime >= firstExam) && (c.PeakTime < e.Time));

						var startAverage = e.Time.AddMinutes(-30); // 30 minutes
						var recentContractionCount = contractions.Count(c => (c.PeakTime >= startAverage) && (c.PeakTime < e.Time));

						if (recentContractionCount > 0)
						{
							e.AverageContractionsInterval = 30 / (double)recentContractionCount;
						}
					}

					#endregion

					#region Perform the expected dilatations and percentiles calculations if the curve validation was successful

					if (curveStatus == CurveCalculationStatuses.Valid)
					{
                        var limit = vbac.Value ? PatternsServiceSettings.Instance.VBACPercentileLimits : PatternsServiceSettings.Instance.NonVBACPercentileLimits;

						CurveExam previous = null;
						foreach (var e in curveExams.Where(e => e.Status != CurveCalculationStatuses.Ignored))
						{
							if ((previous != null) && (e.Dilatation >= 3))
							{
								// Calculate the expected dilatation
								e.ExpectedDilatation = PatternsEpisode.CalculateExpectedDilatation(
															previous.Dilatation.Value,
															previous.Effacement.Value,
															previous.Station.Value,
															e.ContractionsSinceFirstExam.HasValue ? e.ContractionsSinceFirstExam.Value : 0,
															(epidural.HasValue && e.Time > epidural.Value),
															previousVaginal.Value);

								// Calculate the percentiles
								e.Percentile = PatternsEpisode.CalculatePercentile(e.ExpectedDilatation.Value, e.Dilatation.Value, previousVaginal.Value);

								// Calculate upper and lower expected dilatation
								var percentileFactor = PatternsEpisode.GetPercentileFactor(limit, previousVaginal.Value);

								e.UpperExpectedDilatation = e.ExpectedDilatation + percentileFactor;
								e.LowerExpectedDilatation = e.ExpectedDilatation - percentileFactor;
							}
							previous = e;
						}
					}

					#endregion

					#region Generate the Curve XML

					var sb = new StringBuilder(2000);
					using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Auto }))
					{
						writer.WriteStartElement("curve");

						// Some information about what and when exactly...
						writer.WriteAttributeString("serverName", ComputerInfoEx.FDQN);
						writer.WriteAttributeString("version", ApplicationVersion);

						var comparisonProfile = "Compared to: Undefined.";
						var curveReferenceRange = "Selected reference range is undefined.";
						if (previousVaginal.HasValue)
						{
							comparisonProfile = previousVaginal.Value ? "Comparison to women with a previous vaginal delivery." : "Comparison to women with a first vaginal delivery.";
						}
						if (vbac.HasValue)
						{
                            var limit = vbac.Value ? PatternsServiceSettings.Instance.VBACPercentileLimits : PatternsServiceSettings.Instance.NonVBACPercentileLimits;
							curveReferenceRange = string.Format(CultureInfo.InvariantCulture, "Selected reference range is {0}{1}, 50th, {2}th percentile.", limit, SuffixForNumber(limit), 100 - limit);
						}

						if (curveStatus == CurveCalculationStatuses.Error)
						{
							writer.WriteAttributeString("valid", false.ToString());
							writer.WriteAttributeString("laborReferenceMessage", string.Empty);
							writer.WriteAttributeString("message", BuildCurveMessage(curveReason));
						}
						else
						{
							writer.WriteAttributeString("valid", true.ToString());
							writer.WriteAttributeString("laborReferenceMessage", comparisonProfile + " " + curveReferenceRange);

							// The real curve messages...
							var lastExam = curveExams.Where(e => e.Status == CurveCalculationStatuses.Valid).Last();

							// Dilatation unchanged for...
							double hoursSinceDilatationChanged = 0;
							for (int i = curveExams.Count - 1; (i > 0) && (lastExam.Dilatation == curveExams[i].Dilatation); --i)
							{
								if (curveExams[i].Status == CurveCalculationStatuses.Valid)
								{
									hoursSinceDilatationChanged = (lastExam.Time - curveExams[i].Time).TotalHours;
								}
							}

							// Within distocia zone?
                            var limit = vbac.Value ? PatternsServiceSettings.Instance.VBACPercentileLimits : PatternsServiceSettings.Instance.NonVBACPercentileLimits;
                            var duration = vbac.Value ? PatternsServiceSettings.Instance.VBACDilatationUnchanged : PatternsServiceSettings.Instance.NonVBACDilatationUnchanged;

							if (lastExam.Dilatation >= 10)
							{
								writer.WriteAttributeString("message", "Dilation pattern is within expected range.");
							}
							else if (lastExam.Dilatation < lastExam.LowerExpectedDilatation)
							{
								if (hoursSinceDilatationChanged >= duration)
								{
									writer.WriteAttributeString("message", string.Format(CultureInfo.InvariantCulture, "Dilation pattern is within the dystocia zone. Dilation remains at {0:0.#} cm for {1:0.#} hours. Last percentile ranking is {2:0.#}%.", lastExam.Dilatation, hoursSinceDilatationChanged, lastExam.Percentile));
								}
								else
								{
									writer.WriteAttributeString("message", string.Format(CultureInfo.InvariantCulture, "Labor progress is slow. However the patient has not reached the duration criteria for dystocia. Last percentile ranking is {0:0.#}%.", lastExam.Percentile));
								}
							}
							else if (lastExam.Dilatation > lastExam.UpperExpectedDilatation)
							{
								writer.WriteAttributeString("message", "Dilation is over the upper percentile limit.");
							}
							else if (hoursSinceDilatationChanged >= duration)
							{
								writer.WriteAttributeString("message", string.Format(CultureInfo.InvariantCulture, "Dilation pattern demonstrates a lack of progress, remaining unchanged for {0:0.#} hours. However the patient has not reached the percentile criteria for dystocia. Last percentile ranking is {1:0.#}%.", hoursSinceDilatationChanged, lastExam.Percentile));
							}
							else
							{
								writer.WriteAttributeString("message", "Dilation pattern is within expected range.");
							}
						}

						writer.WriteAttributeString("membraneStatus", this.MembraneStatus ?? string.Empty);
						writer.WriteAttributeString("membraneRupture", this.MembraneRupture.HasValue ? this.MembraneRupture.Value.ToString("s", CultureInfo.InvariantCulture) : string.Empty);
						writer.WriteAttributeString("parity", this.Parity.HasValue ? this.Parity.Value.ToString() : string.Empty);
						writer.WriteAttributeString("ga", this.GA ?? string.Empty);

                        writer.WriteAttributeString("display50Percentile", PatternsServiceSettings.Instance.Display50PercentileCurve.ToString());

                        if (PatternsServiceSettings.Instance.PODISendEpidural)
						{
							writer.WriteStartElement("epidural");
							writer.WriteAttributeString("value", this.Epidural.HasValue ? this.Epidural.Value.ToString("s", CultureInfo.InvariantCulture) : string.Empty);
							writer.WriteEndElement();
						}
						else
						{
							WriteDataEntry(writer, "epidural", EpiduralDatabaseKeyName);
						}

                        if (PatternsServiceSettings.Instance.PODISendVBAC)
						{
							writer.WriteStartElement("vbac");
							writer.WriteAttributeString("value", this.VBAC.HasValue ? this.VBAC.Value.ToString() : string.Empty);
							writer.WriteEndElement();
						}
						else
						{
							WriteDataEntry(writer, "vbac", VBACDatabaseKeyName);
						}

                        if (PatternsServiceSettings.Instance.PODISendVaginalDelivery)
						{
							writer.WriteStartElement("previousVaginal");
							writer.WriteAttributeString("value", this.PreviousVaginal.HasValue ? this.PreviousVaginal.Value.ToString() : string.Empty);
							writer.WriteEndElement();
						}
						else
						{
							WriteDataEntry(writer, "previousVaginal", PreviousVaginalDatabaseKeyName);
						}

						WriteDataEntry(writer, "firstExam", FirstExamDatabaseKeyName);

						writer.WriteStartElement("exams");

						foreach (var exam in curveExams)
						{
							writer.WriteStartElement("exam");

							writer.WriteAttributeString("time", exam.Time.ToString("s", CultureInfo.InvariantCulture));

							writer.WriteAttributeString("status", exam.Status.ToString());

							if (exam.StatusReason != CurveCalculationReasons.None)
							{
								writer.WriteAttributeString("message", BuildExamMessage(exam.StatusReason));
							}

							writer.WriteAttributeString("dilatation", exam.Exam.Dilatation ?? string.Empty);
							writer.WriteAttributeString("effacement", exam.Exam.Effacement ?? string.Empty);
							writer.WriteAttributeString("station", exam.Exam.Station ?? string.Empty);

							writer.WriteAttributeString("presentation", exam.Presentation ?? string.Empty);
							writer.WriteAttributeString("position", exam.Exam.Position ?? string.Empty);
							writer.WriteAttributeString("positionCode", exam.FetalPosition.ToString());

							writer.WriteAttributeString("contractionCount", exam.ContractionsSinceFirstExam.HasValue ? exam.ContractionsSinceFirstExam.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
							writer.WriteAttributeString("contractionInterval", exam.AverageContractionsInterval.HasValue ? exam.AverageContractionsInterval.Value.ToString("0.#", CultureInfo.InvariantCulture) : string.Empty);

							if (exam.Status == CurveCalculationStatuses.Valid)
							{
								writer.WriteAttributeString("expectedDilatation", exam.ExpectedDilatation.HasValue ? exam.ExpectedDilatation.Value.ToString("0.#", CultureInfo.InvariantCulture) : string.Empty);
								writer.WriteAttributeString("lowerExpectedDilatation", exam.LowerExpectedDilatation.HasValue ? exam.LowerExpectedDilatation.Value.ToString("0.#", CultureInfo.InvariantCulture) : string.Empty);
								writer.WriteAttributeString("upperExpectedDilatation", exam.UpperExpectedDilatation.HasValue ? exam.UpperExpectedDilatation.Value.ToString("0.#", CultureInfo.InvariantCulture) : string.Empty);
								writer.WriteAttributeString("percentile", exam.Percentile.HasValue ? exam.Percentile.Value.ToString("0.#", CultureInfo.InvariantCulture) : string.Empty);
							}

							writer.WriteEndElement(); // exam
						}

						writer.WriteEndElement(); // exams
						writer.WriteEndElement(); // curve

						writer.Flush();
						writer.Close();
					}

					#endregion

					#region Add the snapshot to the episode

					lock (this.CurveSnapshots)
					{
						// Snapshot to add...or not
						var snapshotProcessedData = sb.ToString();

						// Do not add the snapshot if it is an exact replica of the last one (no real change)
						var previousSnapshot = this.CurveSnapshots.LastOrDefault();
						if ((previousSnapshot == null) || (string.CompareOrdinal(snapshotProcessedData, previousSnapshot.Data) != 0))
						{
							Common.Source.TraceEvent(TraceEventType.Verbose, 7605, "Updated curve for episode ({0})", this);

							// Curve has changed. Add the new one...
							this.AddSnapshot(new DataContextEpisode.CurveSnapshot { Data = snapshotProcessedData, UpdateTime = now.ToEpoch() });
						}
					}

					#endregion

					// Reset the curve recovery if applicable
					this.RecoveryStatus &= ~RecoveryStatuses.Curve;
				}
				catch (Exception e)
				{
					Common.Source.TraceEvent(TraceEventType.Error, 7615, "Error while processing the Curve of the episode ({0}).\nException: {1}", this, e);
					throw;
				}
			}
		}

		#endregion

		#region Patterns detection

		/// <summary>
		/// For traces
		/// </summary>
		internal string EngineName
		{
			get { return string.Format(CultureInfo.InvariantCulture, "for episode ({0})", this.ToString()); }
		}

		/// <summary>
		/// Process the new tracings using the Pattern's engine
		/// </summary>
		public void ProcessPatterns()
		{
			try
			{
				// If the data is still in recovery from PODI, just leave now...
				if ((this.RecoveryStatus & RecoveryStatuses.Podi) != RecoveryStatuses.None)
				{
					return;
				}

				// No tracing at all, leave!
				lock (this.TracingBlocks)
				{
					if (this.TracingBlocks.Count == 0)
					{
						// Patterns is up to date
						this.RecoveryStatus &= ~RecoveryStatuses.Patterns;

						// Done, that was easy!
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
                            this.Engine = new PeriGen.Patterns.Engine.PatternsEngineWrapper(startime.AddMinutes(-PatternsServiceSettings.Instance.PatternsEngineBufferPrimingDelay), this.EngineName);

							// Remember we are processing some tracing that was already processed
                            if (PatternsServiceSettings.Instance.PatternsEngineBufferPrimingDelay > 0)
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
							this.Engine = new PeriGen.Patterns.Engine.PatternsEngineWrapper(this.TracingBlocks.Min(b => b.Start), this.EngineName);
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
                if ((tracings.Count() > 0) && ((tracings.Last().End - Engine.CurrentTime).TotalSeconds >= PatternsServiceSettings.Instance.PatternsEngineMinimumDurationToAppend))
				{
					// Do a merge to reduce the number of blocks
                    tracings = TracingBlock.Merge(tracings, PatternsServiceSettings.Instance.PatternsEngineMaximumBridgeableGap, PatternsServiceSettings.Instance.PatternsEngineMaximumMergeBlockSize);

					// One block at a time (most likely, there is only one block there. There may be more if the episode is in recovery and if there is a lot of tracings to process
					foreach (var b in tracings)
					{
						// Complete overlap?
						if (b.End <= this.Engine.CurrentTime)
							continue;

						Debug.Assert(b.TotalSeconds > 0);

						var artifacts = new List<PeriGen.Patterns.Engine.Data.DetectedObject>();

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
                                if (gap > PatternsServiceSettings.Instance.PatternsEngineMaximumBridgeableGap)
								{
									// Drop and recreate the engine at the start of the block
									this.Engine.Dispose();
									this.Engine = new PeriGen.Patterns.Engine.PatternsEngineWrapper(b.Start, this.EngineName);
								}
								// A small gap...
								else
								{
									// Patch the gap
									artifacts.AddRange(this.Engine.Process((byte[])Array.CreateInstance(typeof(byte), 4 * gap), (byte[])Array.CreateInstance(typeof(byte), gap)));
								}
							}

							// Process the block
							Debug.Assert(b.Start == this.Engine.CurrentTime);
							artifacts.AddRange(this.Engine.Process(b.HRs.ToArray(), b.UPs.ToArray()));
						}

                        var arts = (from c in artifacts
                                    where c is DetectionArtifact
                                    select c as DetectionArtifact).ToList();

						// If the engine was primed... make sure we don't add twice some events/contractions
                        if (this.EnginePrimed && arts.Count > 0)
						{
                            arts.RemoveAll(a => (a.IsContraction && a.StartTime < this.EnginePrimedContractions) || (!a.IsContraction && a.StartTime < this.EnginePrimedEvents));

							// Are we past the primed buffer? It is the case if we have at least one 
                            if (arts.Any(a => a.IsContraction))
								this.EnginePrimedContractions = DateTime.MinValue;

                            if (arts.Any(a => !a.IsContraction))
								this.EnginePrimedEvents = DateTime.MinValue;

                            this.EnginePrimed = (this.EnginePrimedContractions != DateTime.MinValue) || 
                                                (this.EnginePrimedEvents != DateTime.MinValue);
						}

						// Add the artifacts to the list
						this.AddArtifacts(arts);
					}
				}

				// Patterns is up to date
				this.RecoveryStatus &= ~RecoveryStatuses.Patterns;
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 7620, "Error while processing the Patterns of the episode ({0}).\nException: {1}", this, e);
				throw;
			}
		}

        private void WriteToLog(String logPath, DateTime today, byte[] hrs, byte[] ups, int overlap = 0)
        {
            if (!PatternsServiceSettings.Instance.WriteLogOfTracingsDataToPatternsEngine)
                return;

            if (!File.Exists(logPath))
            {
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(logPath))
                {
                    sw.WriteLine("Log file for date: " + today.ToShortDateString());
                }
            }

            using (StreamWriter sw = File.AppendText(logPath))
            {
                sw.WriteLine("Data sent to engine from processing, episode key: " + Key);
                sw.WriteLine("UP Stream:");
                for (int i = overlap; i < ups.Length; i++)
                {
                    sw.Write(((int)ups[i]).ToString() + "  ");
                }

                sw.WriteLine();
                sw.WriteLine("FHR Stream:");
                for (int i = overlap * 4; i < hrs.Length; i++)
                {
                    sw.Write(((int)hrs[i]).ToString() + "  ");
                }

                sw.WriteLine();
            }
        }

		#endregion

		/// <summary>
		/// The real process of data: Patterns detection and curve calculations
		/// It is call on the primary thread
		/// </summary>
		public void ProcessData()
		{
			// Do the pattern detection
			this.ProcessPatterns();

			// Calculate the curve 
			this.ProcessCurve();
		}

		/// <summary>
		/// Compress the cached tracings by merging small blocks
		/// </summary>
		/// <returns>Number of blocks saved by compression</returns>
		public int CompressCachedTracings()
		{
			try
			{
				// All needs to be done under lock...
				lock (this.TracingBlocks)
				{
					// Drop blocks that are too old and will not be useful anymore
                    DateTime timeLimit = DateTime.UtcNow.AddDays(-PatternsServiceSettings.Instance.MaxDaysOfTracingReturnedToActiveX).RoundToTheSecond();
					this.TracingBlocks.RemoveAll(b => (b.Id <= this.LastCommittedTracingID) && (b.End <= timeLimit));

					// We start were the last compress ended
					int startIndex = this.TracingBlocks.FindIndex(b => b.Id >= this.LastCompressTracingID);

					// We don't compress blocks that are not yet saved in database AND 
					// We don't compress the last xxx blocks just to make sure the partial refresh from the activex are not impacted
					int endIndex = Math.Min(
                                        this.TracingBlocks.Count - PatternsServiceSettings.Instance.CompressCachedTracingExcludeLastBlocks,
										this.TracingBlocks.FindLastIndex(b => b.Id <= this.LastCommittedTracingID));

					// Nothing?
					if ((startIndex < 0) || (endIndex <= startIndex))
						return 0;

					// Compress now
					return this.CompressCachedTracings(startIndex, (endIndex - startIndex) + 1);
				}
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 7625, "Error while compressing the tracings of the episode ({0}).\nException: {1}", this, e);
				throw;
			}
		}

		/// <summary>
		/// Update the episode status
		/// </summary>
		/// <param name="status"></param>
		/// <param name="logReason">For tracing why it is called</param>
		public void UpdateStatus(EpisodeStatuses status, string logReason)
		{
			lock (this.LockObject)
			{
				if (this.EpisodeStatus != status)
				{
					Common.Source.TraceEvent(TraceEventType.Verbose, 7630, "Episode status changed from {0} to {1} for episode ({2}).\nReason: {3}", this.EpisodeStatus, status, this, logReason);

					this.EpisodeStatus = status;
					this.DataUpdated = true;
				}
			}
		}

		#region Database management

		/// <summary>
		/// Save all pending data to database
		/// </summary>
		public void SaveDataToDatabase()
		{
			try
			{
				lock (this.LockDatabase)
				{
					// Not already mapped in db, skip!
					if (this.PatientUniqueId <= 0)
						return;

					if (string.IsNullOrEmpty(this.DatabaseFile))
					{
						Debug.Assert(false, "At that stage, the database file should be set!");
						return;
					}

					List<TracingBlock> tracings;
					List<XUserAction> actions;
					List<DetectionArtifact> artifacts;
					List<DataContextEpisode.PelvicExam> exams;
					List<DataContextEpisode.DataEntry> entries;
					List<DataContextEpisode.CurveSnapshot> snapshots;

					lock (this.LockObject)
					{
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
						
                        lock (this.PelvicExams)
						{
							exams = this.PelvicExams.Where(item => item.ExamId > this.LastCommittedPelvicExamID).ToList();
						}
						
                        lock (this.DataEntries)
						{
							entries = this.DataEntries.Where(item => item.EntryId > this.LastCommittedDataEntryID).ToList();
						}
						
                        lock (this.CurveSnapshots)
						{
							snapshots = this.CurveSnapshots.Where(item => item.SnapshotId > this.LastCommittedSnapshotID).ToList();
						}

                        if ((tracings.Count == 0) && (actions.Count == 0) && (artifacts.Count == 0) && (exams.Count == 0) && (entries.Count == 0) && (snapshots.Count == 0))
						{
							// Nothing to do
							return;
						}
					}

					using (var db = new DataContextEpisode.DataContextEpisode(GetDatabaseConnectionString(this.DatabaseFile)))
					{
						db.CreateIfNecessary();
						if (tracings.Count > 0)
						{
							this.LastCommittedTracingID = tracings.Max(item => item.Id);

							// Merge tracings for database saving in order to achieve better performances
                            tracings = TracingBlock.Merge(tracings, PatternsServiceSettings.Instance.DatabaseMergeBridgeableGap, PatternsServiceSettings.Instance.DatabaseMergeMaximumBlock);

							// Create the databases blocks
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
						
                        if (exams.Count > 0)
						{
							this.LastCommittedPelvicExamID = exams.Max(item => item.ExamId);
							db.PelvicExams.InsertAllOnSubmit(exams);
						}
						
                        if (entries.Count > 0)
						{
							this.LastCommittedDataEntryID = entries.Max(item => item.EntryId);
							db.DataEntries.InsertAllOnSubmit(entries);
						}
						
                        if (snapshots.Count > 0)
						{
							this.LastCommittedSnapshotID = snapshots.Max(item => item.SnapshotId);
							db.CurveSnapshots.InsertAllOnSubmit(snapshots);
						}
                        
						// Save the context
						db.SetParameter(ContextDatabaseKeyName, this.Context);

						// Commit
						db.SubmitChanges();
					}
				}
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 7635, "Error while saving the episode ({0}).\nException: {1}", this, e);
			}
		}

		/// <summary>
		/// Load the episode content from the given db record
		/// </summary>
		/// <param name="dbPatient"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public bool LoadFromDatabase(DataContextCatalog.Patient dbPatient)
		{
			try
			{
				// Decrypt and deserialize episode 
				this.PatientUniqueId = dbPatient.PatientId;
				this.DatabaseFile = dbPatient.DatabaseFile;

				this.EpisodeStatus = (EpisodeStatuses)Enum.ToObject(typeof(EpisodeStatuses), dbPatient.EpisodeStatus);

				this.Created = dbPatient.Created.ToDateTime();
				this.LastUpdated = dbPatient.LastUpdated.ToDateTime();
				this.LastMonitored = dbPatient.LastMonitored.ToDateTime();

				this.ReadPatientDataFromXml(PatternsSecurity.Decrypt(dbPatient.PatientData));

				// Some validation
				if (string.IsNullOrEmpty(this.Key))
					throw new InvalidOperationException("Episode key cannot be empty in the database");

				Debug.Assert(!string.IsNullOrEmpty(this.DatabaseFile) && this.PatientUniqueId > 0);

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
						this.PelvicExams = db.PelvicExams.ToList();
						this.DataEntries = db.DataEntries.ToList();
						this.CurveSnapshots = db.CurveSnapshots.ToList();

						this.LastCommittedActionID = this.TracingActions.Count == 0 ? 0 : this.TracingActions.Max(item => item.Id);
						this.LastMemoryActionID = this.LastCommittedActionID;

                        this.LastCommittedArtifactID = TracingArtifacts.Count() == 0 ? 0 : TracingArtifacts.Max(item => item.Id);
						this.LastMemoryArtifactID = this.LastCommittedArtifactID;

						this.LastCommittedTracingID = this.TracingBlocks.Count == 0 ? 0 : this.TracingBlocks.Max(item => item.Id);
						this.LastMemoryTracingID = this.LastCommittedTracingID;

						this.LastCommittedPelvicExamID = this.PelvicExams.Count == 0 ? 0 : this.PelvicExams.Max(item => item.ExamId);
						this.LastMemoryPelvicExamID = this.LastCommittedPelvicExamID;

						this.LastCommittedDataEntryID = this.DataEntries.Count == 0 ? 0 : this.DataEntries.Max(item => item.EntryId);
						this.LastMemoryDataEntryID = this.LastCommittedDataEntryID;

						this.LastCommittedSnapshotID = this.CurveSnapshots.Count == 0 ? 0 : this.CurveSnapshots.Max(item => item.SnapshotId);
						this.LastMemorySnapshotID = this.LastCommittedSnapshotID;

						// Update the last tracing date&time
						this.LastTracing = this.TracingBlocks.Count == 0 ? DateTime.MinValue : this.TracingBlocks.Max(b => b.End);

						// Compress ALL tracings in memory to reduce load
						this.CompressCachedTracings(0, this.TracingBlocks.Count);

						// Read the context
						this.Context = db.GetParameter(ContextDatabaseKeyName);

						// Flag curve as needing refresh
						this.CurveNeedRefresh = true;

						Common.Source.TraceEvent(TraceEventType.Verbose, 7640, "Episode ({0}) loaded with {1:0} hours of tracings and {2} artifacts", this, this.HoursOfTracings, this.TracingArtifacts.Count);
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				// Cannot deserialize episode. It must be closed automatically.
				Common.Source.TraceEvent(TraceEventType.Error, 7645, "Episode for id {0} in database {1} cannot be deserialized from database. Episode is now closed.\nException: {2}", dbPatient.PatientId, dbPatient.DatabaseFile, ex);
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
                        Path.GetDirectoryName(PatternsServiceSettings.Instance.PatternsDBPath),
						string.Format(CultureInfo.InvariantCulture, "{0}.db3", database));
		}

		#endregion

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

		#region Custom Serialization

		/// <summary>
		/// Serialize episode in XML string
		/// </summary>
		/// <returns></returns>
		public string WritePatientDataToXml()
		{
			var sb = new StringBuilder();
			using (var xw = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Auto }))
			{
				xw.WriteStartDocument();

				// call writer
				this.WritePatientDataToXml(xw);

				// End document
				xw.WriteEndDocument();
				xw.Flush();
			}

			return sb.ToString();
		}

		/// <summary>
		/// Serialize episode in writer
		/// </summary>
		/// <param name="xw"></param>
		public void WritePatientDataToXml(XmlWriter xw)
		{
			// Start patient element
			xw.WriteStartElement("Patient");

			// Attributes
			xw.WriteAttributeString("Key", this.Key);
			xw.WriteAttributeString("MRN", this.MRN);
			xw.WriteAttributeString("AccountNo", this.AccountNo);
			xw.WriteAttributeString("BedId", this.BedId);
			xw.WriteAttributeString("BedName", this.BedName);
			xw.WriteAttributeString("UnitName", this.UnitName);
			xw.WriteAttributeString("FirstName", this.FirstName);
			xw.WriteAttributeString("LastName", this.LastName);
			if (this.Fetuses.HasValue) xw.WriteAttributeString("Fetuses", this.Fetuses.Value.ToString(CultureInfo.InvariantCulture));
			if (this.EDD.HasValue) xw.WriteAttributeString("EDD", this.EDD.Value.ToString("s", CultureInfo.InvariantCulture));
			xw.WriteAttributeString("GA", this.GA);
			if (this.Parity.HasValue) xw.WriteAttributeString("Parity", this.Parity.Value.ToString(CultureInfo.InvariantCulture));
			if (this.Epidural.HasValue) xw.WriteAttributeString("Epidural", this.Epidural.Value.ToString("s", CultureInfo.InvariantCulture));
			if (this.VBAC.HasValue) xw.WriteAttributeString("VBAC", this.VBAC.Value.ToString(CultureInfo.InvariantCulture));
			if (this.PreviousVaginal.HasValue) xw.WriteAttributeString("PreviousVaginal", this.PreviousVaginal.Value.ToString(CultureInfo.InvariantCulture));
			xw.WriteAttributeString("MembraneStatus", this.MembraneStatus);
			if (this.MembraneRupture.HasValue) xw.WriteAttributeString("MembraneRupture", this.MembraneRupture.Value.ToString("s", CultureInfo.InvariantCulture));

			// End Patient element
			xw.WriteEndElement();
			xw.Flush();
		}

		/// <summary>
		/// initialize episode from XElement
		/// </summary>
		/// <param name="element"></param>
		public void ReadPatientDataFromXml(XElement element)
		{
			// Attributes
			this.Key = element.Attribute("Key").Value;
			this.MRN = element.Attribute("MRN").Value;
			this.AccountNo = element.Attribute("AccountNo").Value;
			this.BedId = element.Attribute("BedId").Value;
			this.BedName = element.Attribute("BedName").Value;
			this.UnitName = element.Attribute("UnitName").Value;
			this.FirstName = element.Attribute("FirstName").Value;
			this.LastName = element.Attribute("LastName").Value;

			this.Fetuses = element.Attribute("Fetuses") == null ? null : (int?)(int.Parse(element.Attribute("Fetuses").Value, CultureInfo.InvariantCulture));
			this.EDD = element.Attribute("EDD") == null ? null : (DateTime?)(DateTime.ParseExact(element.Attribute("EDD").Value, "s", CultureInfo.InvariantCulture));
			this.GA = element.Attribute("GA").Value;
			this.Parity = element.Attribute("Parity") == null ? null : (int?)(int.Parse(element.Attribute("Parity").Value, CultureInfo.InvariantCulture));
			this.Epidural = element.Attribute("Epidural") == null ? null : (DateTime?)(DateTime.ParseExact(element.Attribute("Epidural").Value, "s", CultureInfo.InvariantCulture));
			this.VBAC = element.Attribute("VBAC") == null ? null : (bool?)(bool.Parse(element.Attribute("VBAC").Value));
			this.PreviousVaginal = element.Attribute("PreviousVaginal") == null ? null : (bool?)(bool.Parse(element.Attribute("PreviousVaginal").Value));
			this.MembraneStatus = element.Attribute("MembraneStatus").Value;
			this.MembraneRupture = element.Attribute("MembraneRupture") == null ? null : (DateTime?)(DateTime.ParseExact(element.Attribute("MembraneRupture").Value, "s", CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Initialize episode from xmlstring
		/// </summary>
		/// <param name="data"></param>
		public void ReadPatientDataFromXml(string data)
		{
			this.ReadPatientDataFromXml(XElement.Parse(data));
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Returns the connection string associated to the given database name
		/// </summary>
		/// <param name="database"></param>
		/// <returns></returns>
		static string GetDatabaseConnectionString(string database)
		{
			///get connection string format and options from settings
            var connectionStringFormat = PatternsServiceSettings.Instance.DBConnectionStringFormat;
			return string.Format(CultureInfo.InvariantCulture, connectionStringFormat, GetDatabaseFileName(database));

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
                    || (last.TotalSeconds > PatternsServiceSettings.Instance.CompressCachedTracingMaximumDuration)
                    || ((int)(block.Start - last.End).TotalSeconds > PatternsServiceSettings.Instance.CompressCachedTracingMaximumGap))
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
		/// For nice traces
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "pid='{0}' key='{1}'", this.PatientUniqueId, this.Key);
		}

		/// <summary>
		/// Add some more data entries to the list
		/// </summary>
		/// <param name="list"></param>
		public DataContextEpisode.DataEntry GetDataEntry(string name)
		{
			lock (this.DataEntries)
			{
				return this.DataEntries.Where(de => de.Name == name).OrderBy(de => de.EntryId).LastOrDefault();
			}
		}

		/// <summary>
		/// Return the list of contractions that are NOT deleted
		/// </summary>
		public List<Contraction> ActiveContractions
		{
			get
			{
				// Get the list of all contractions
				List<Contraction> contractions;
				lock (this.TracingArtifacts)
				{
                    contractions = TracingArtifacts.Where(t => t.IsContraction).Cast<Contraction>().ToList();
				}

				// Get the list of strikeout-contraction actions that where not undo later on
				List<XUserAction> actions;
				lock (this.TracingActions)
				{
					actions = this.TracingActions.Where(u => u.ActionType == ActionTypes.StrikeoutContraction || u.ActionType == ActionTypes.UndoStrikeoutContraction).ToList();
				}
				actions = actions.GroupBy(u => u.ArtifactId).Select(group => group.OrderBy(p => p.Id).Last()).Where(u => u.ActionType == ActionTypes.StrikeoutContraction).ToList();

				// Filter out the contractions that are deleted
				return contractions.Where(c => !actions.Any(u => u.ArtifactId == c.Id)).ToList();
			}
		}

		/// <summary>
		/// Return the list of pelvic exams that are not deleted
		/// </summary>
		public List<DataContextEpisode.PelvicExam> ActiveExams
		{
			get
			{
				// Get the actual non strikeout exams (merge all the different examx modifications
				List<DataContextEpisode.PelvicExam> exams;
				lock (this.PelvicExams)
				{
					exams = DataContextEpisode.PelvicExam.Merge(this.PelvicExams).Where(e => !e.IsEmpty).ToList();
				}
				return exams;
			}
		}


        private bool IsGAValidForCRI()
        {            
            int weeks = 0;
            int plus = this.GA.IndexOf("+");
            string weeksPart = (plus > 0)? this.GA.Substring(0, plus) : this.GA;
            weeksPart.Trim();
            weeks = Convert.ToInt32(weeksPart);
            if (weeks >= 36 && plus < this.GA.Length)
            {
                string daysPart = this.GA.Substring(plus + 1);
                daysPart.Trim();
                if (Convert.ToInt32(daysPart) > 0)
                {
                    weeks++;
                }
            }           

            return (weeks >= 36 && weeks <= 43);
        }

		#endregion

        #region get for curve

        public bool? GetVBAC()
        {
            bool? vbac = null;
            if (PatternsServiceSettings.Instance.PODISendVBAC)
            {
                vbac = VBAC;
            }
            else
            {
                DataContextEpisode.DataEntry entry = GetDataEntry(VBACDatabaseKeyName);
                if (entry != null)
                {
                    bool value;
                    if (bool.TryParse(entry.Value, out value))
                    {
                        vbac = value;
                    }
                }
            }
            return vbac;
        }

        public bool? GetPreviousVaginalDelivery()
        {
            bool? previousVaginal = null;
            if (PatternsServiceSettings.Instance.PODISendVaginalDelivery)
            {
                previousVaginal = PreviousVaginal;
            }
            else
            {
                DataContextEpisode.DataEntry entry = GetDataEntry(PreviousVaginalDatabaseKeyName);
                if ((entry != null) && (!string.IsNullOrEmpty(entry.Value)))
                {
                    bool value;
                    if (bool.TryParse(entry.Value, out value))
                    {
                        previousVaginal = value;
                    }
                }
            }
            return previousVaginal;
        }
        #endregion
    }

}
