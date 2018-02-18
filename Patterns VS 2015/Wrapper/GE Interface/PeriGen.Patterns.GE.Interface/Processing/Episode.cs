using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeriGen.Patterns.GE.Interface.Demographics;
using PeriGen.Patterns.Engine.Data;
using System.Diagnostics;
using System.Xml.Linq;
using System.Globalization;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace PeriGen.Patterns.GE.Interface
{
	/// <summary>
	/// The different status of an episode
	/// </summary>
	public enum EpisodeStatuses
	{
		Normal = 0,
		OutOfMonitor = 1,
		OutOfScope = 2,
		Closed = 3,
	}

	[Serializable]
	public class Episode
	{
		/// <summary>
		/// ctor
		/// </summary>
		public Episode()
		{
			this.LockObject = new object();
			this.Created = DateTime.UtcNow;

			this.PatientUniqueId = 0;
			this.EpisodeStatus = EpisodeStatuses.Normal;
		}

		#region Attributes and Properties

		/// <summary>
		/// For thread safety on accessing the data of that object
		/// </summary>
		[XmlIgnore]
		public object LockObject { get; private set; }

		/// <summary>
		/// The unique id of the episode
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
		/// Demographic data and status
		/// </summary>
		public Patient Patient { get; set; }

		/// <summary>
		/// If there is another episode that is conflicting with this one during reconciliation, it's description is added here
		/// </summary>
		public string ReconciliationConflict { get; set; }

		/// <summary>
		/// Date and time when the episode was created
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		/// Date and time when the patient was synchronized from GE data
		/// </summary>
		public DateTime LastUpdated { get; set; }

		/// <summary>
		/// Date and Time when episode went to out of monitor
		/// </summary>
		public DateTime LastMonitored { get; set; }

		volatile bool _Updated;

		/// <summary>
		/// If the last synchronization updated some data in the patient
		/// </summary>
		public bool Updated { get { return this._Updated; } set { this._Updated = value; } }

		/// <summary>
		/// Flag used to reconciliate list
		/// </summary>
		public bool Reconciliated { get; set; }

		#endregion

		#region Accessors for mapping Patient data in a GUI

		[XmlIgnore]
		public string MRN { get { return this.Patient.MRN; } }

		[XmlIgnore]
		public string BedId { get { return this.Patient.BedId; } }

		[XmlIgnore]
		public string Location { get { return string.Format(CultureInfo.InvariantCulture, "{0}-{1}({2})", this.Patient.UnitName, this.Patient.BedName, this.Patient.BedId); } }

		[XmlIgnore]
		public string PatientName { get { return this.Patient.Name; } }

		[XmlIgnore]
		public DateTime? PatientEDD { get { return this.Patient.EDD; } }

		[XmlIgnore]
		public int? PatientFetus { get { return this.Patient.Fetuses; } }

		[XmlIgnore]
		public bool MergeInProgress { get { return this.Patient.IsMergeInProgress; } }

		#endregion

		/// <summary>
		/// For nice traces
		/// </summary>
		/// <returns></returns>
		public string FullDescription
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture, "unique_id='{0}' status='{1}' {2}", this.PatientUniqueId, this.EpisodeStatus, this.Patient.FullDescription);
			}
		}

		/// <summary>
		/// For nice traces
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "unique_id='{0}' {1}", this.PatientUniqueId, this.Patient);
		}

		/// <summary>
		/// Check if the episode is still 
		/// </summary>
		[XmlIgnore]
		public bool IsOpen
		{
			get
			{
				return EpisodeStatus == EpisodeStatuses.Normal || EpisodeStatus == EpisodeStatuses.OutOfMonitor;
			}
		}

		#region For synchronization with GE data

		/// <summary>
		/// If a previous conflict was known on that episode, it does not apply anymore, clear it
		/// </summary>
		public void ResetConflict()
		{
			// A previous conflict?
			if (!string.IsNullOrEmpty(this.ReconciliationConflict))
			{
				this.ReconciliationConflict = string.Empty;
				this.Updated = true;

				Common.Source.TraceEvent(TraceEventType.Warning, 6601, "Chalkboard: Reconciliation conflict resolved for patient {0}.", this);
			}
		}

		/// <summary>
		/// There are conflicts that block the reconciliation!
		/// </summary>
		/// <param name="conflicts"></param>
		public void SetConflict(string conflicts)
		{
			// No conflict?
			if (string.IsNullOrEmpty(conflicts))
			{
				this.ResetConflict();
				return;
			}

			// Conflict already known?
			if ((string.IsNullOrEmpty(this.ReconciliationConflict)) || (string.CompareOrdinal(this.ReconciliationConflict, conflicts) != 0))
			{
				this.ReconciliationConflict = conflicts;
				this.Updated = true;

				Common.Source.TraceEvent(TraceEventType.Warning, 6602, "Chalkboard: Reconciliation conflict detected for patient {0}.", this);
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
					// Close any tracing collection process
					this.OnADTChanged();

					Common.Source.TraceEvent(TraceEventType.Verbose, 6603, "Chalkboard: Episode status change from {0} to {1} for patient {2}.\nReason: {3}", this.EpisodeStatus, status, this, logReason);

					this.EpisodeStatus = status;
					this.Updated = true;
				}
				this.Reconciliated = true;
			}
		}

		/// <summary>
		/// Update the patient information with the ones in the given patient instance
		/// </summary>
		/// <param name="patient"></param>
		/// <param name="logReason">For tracing why it is called</param>
		public void UpdatePatient(Patient patient, string logReason)
		{
			Debug.Assert(!string.IsNullOrEmpty(patient.BedName) && !string.IsNullOrEmpty(patient.UnitName) && !string.IsNullOrEmpty(patient.MRN));

			////////////////////////////////////////////////////////////////////////////////
			// Check for update in order to log proper trace
			if (this.Patient == null)
			{
				// New patient!
				this.EpisodeStatus = EpisodeStatuses.Normal;
				this.LastMonitored = DateTime.UtcNow;
				this.LastUpdated = DateTime.UtcNow;
				this.Updated = true;

				Common.Source.TraceEvent(TraceEventType.Verbose, 6604, "Chalkboard: New patient {0}\nReason: {1}", patient, logReason);
			}
			else
			{
				if (string.CompareOrdinal(patient.MRN, this.MRN) != 0)
				{
					this.OnADTChanged();
					this.Updated = true;
					Common.Source.TraceEvent(TraceEventType.Verbose, 6605, "Chalkboard: Change MRN.\nFrom: {0}\nTo: {1}\nReason: {2}", this, patient, logReason);
				}

				if ((!string.IsNullOrEmpty(patient.BedId))
					&& (!string.IsNullOrEmpty(this.BedId))
					&& (string.CompareOrdinal(patient.BedId, this.Patient.BedId) != 0))
				{
					this.OnADTChanged();
					this.Updated = true;
					Common.Source.TraceEvent(TraceEventType.Verbose, 6606, "Chalkboard: Transfer in another bed.\nFrom: {0}\nTo: {1}\nReason: {2}", this, patient, logReason);
				}

				if ((!string.IsNullOrEmpty(patient.BedId)) && (string.IsNullOrEmpty(this.BedId)))
				{
					this.OnADTChanged();
					this.Updated = true;
					Common.Source.TraceEvent(TraceEventType.Verbose, 6607, "Chalkboard: Patient is back on the monitor.\nFrom: {0}\nTo: {1}\nReason: {2}", this, patient, logReason);
				}

				if ((string.IsNullOrEmpty(patient.BedId)) && (!string.IsNullOrEmpty(this.BedId)))
				{
					this.OnADTChanged();
					this.Updated = true;
					Common.Source.TraceEvent(TraceEventType.Verbose, 6608, "Chalkboard: Patient is now off the monitor or was transfer to a non active bed.\nFrom: {0}\nTo: {1}\nReason: {2}", this, patient, logReason);
				}

				if (this.Patient.Fetuses != patient.Fetuses)
				{
					this.Updated = true;
					Common.Source.TraceEvent(TraceEventType.Verbose, 6609, "Chalkboard: Fetus count changed.\nFrom: {0}\nTo: {1}\nReason: {2}", this, patient, logReason);
				}

				if (this.Patient.EDD != patient.EDD)
				{
					this.Updated = true;
					Common.Source.TraceEvent(TraceEventType.Verbose, 6610, "Chalkboard: EDD changed.\nFrom: {0}\nTo: {1}\nReason: {2}", this, patient, logReason);
				}

				if (string.CompareOrdinal(this.Patient.Name, patient.Name) != 0)
				{
					this.Updated = true;
					Common.Source.TraceEvent(TraceEventType.Verbose, 6611, "Chalkboard: Name changed.\nFrom: {0}\nTo: {1}\nReason: {2}", this, patient, logReason);
				}

				if (!this.Patient.TrailList.Equals(patient.TrailList))
				{
					this.Updated = true;
					Common.Source.TraceEvent(TraceEventType.Verbose, 6612, "Chalkboard: Location trail changed for patient {0}\nReason: {1}", patient, logReason);
				}

				if (this.Patient.IsMergeInProgress != patient.IsMergeInProgress)
				{
					this.Updated = true;
					Common.Source.TraceEvent(TraceEventType.Verbose, 6613, "Chalkboard: Merge {0}.\nFrom: {1}\nTo: {2}\nReason: {3}", patient.IsMergeInProgress ? "starting" : "completed", this, patient, logReason);
				}
			}

			////////////////////////////////////////////////////////////////////////////////
			// Update the data
			lock (this.LockObject)
			{
				this.Patient = patient;
			}

			////////////////////////////////////////////////////////////////////////////////
			// Update the episode status
			if (string.IsNullOrEmpty(this.Patient.BedId))
			{
				if (this.EpisodeStatus == EpisodeStatuses.Normal)
				{
					this.UpdateStatus(EpisodeStatuses.OutOfMonitor, logReason);
				}
			}
			else
			{
				this.LastMonitored = DateTime.UtcNow;
				if (this.EpisodeStatus != EpisodeStatuses.Normal)
				{
					this.UpdateStatus(EpisodeStatuses.Normal, logReason);
				}
			}

			// Remember last update time
			if (this.Updated)
			{
				this.LastUpdated = DateTime.UtcNow;
			}

			this.Reconciliated = true;
		}

		/// <summary>
		/// Some ADT action was performed on that episode
		/// Reset the live tracing processor if necessary
		/// </summary>
		protected virtual void OnADTChanged()
		{
			// Nothing to do here
		}

		#endregion
	}
}
