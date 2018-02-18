using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using PeriGen.Patterns.GE.Interface.Demographics;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;
using PeriGen.Patterns.Settings;

namespace PeriGen.Patterns.GE.Interface
{
	public class Chalkboard<T>
		where T : Episode, new()
	{
		#region Settings

		static int Settings_OBLinkChalkboardRefreshRetries = SettingsManager.GetInteger("OBLinkChalkboardRefreshRetries");
		static int Settings_EpisodeOutOfMonitorToOutOfScopeDuration = SettingsManager.GetInteger("EpisodeOutOfMonitorToOutOfScopeDuration");
		static int Settings_EpisodeOutOfMonitorToCloseDuration = SettingsManager.GetInteger("EpisodeOutOfMonitorToCloseDuration");
		static int Settings_OBLinkChalkboardRequestTimeout = SettingsManager.GetInteger("OBLinkChalkboardRequestTimeout");
		static int Settings_OBLinkPatientRequestTimeout = SettingsManager.GetInteger("OBLinkPatientRequestTimeout");

		static bool Settings_TraceExtendedInformationWithPHIData = SettingsManager.GetBoolean("TraceExtendedInformationWithPHIData");
		static bool Settings_TraceExtendedOBLinkData = SettingsManager.GetBoolean("TraceExtendedOBLinkData");
		static bool Settings_TraceExtendedReconciliationData = SettingsManager.GetBoolean("TraceExtendedReconciliationData");
		static bool Settings_OBLinkChalkboardCheckForOBLinkPatientMismatch = SettingsManager.GetBoolean("OBLinkChalkboardCheckForOBLinkPatientMismatch");

		static string Settings_OBLinkURL = SettingsManager.GetValue("OBLinkURL");
		static string Settings_OBLinkUsername = SettingsManager.GetValue("OBLinkUsername");
		static string Settings_OBLinkPassword = SettingsManager.GetValue("OBLinkPassword");
		static string Settings_OBLinkChalkboardRequestUrl = SettingsManager.GetValue("OBLinkChalkboardRequestUrl");
		static string Settings_OBLinkPatientDataRequestUrl = SettingsManager.GetValue("OBLinkPatientDataRequestUrl");
		static string Settings_OBLinkPatientTrailRequestUrl = SettingsManager.GetValue("OBLinkPatientTrailRequestUrl");
		static string Settings_OBLinkPatientADTRequestUrl = SettingsManager.GetValue("OBLinkPatientADTRequestUrl");
		
		#endregion

		/// <summary>
		/// For thread safety
		/// </summary>
		public object EpisodesLockObject { get; protected set; }

		/// <summary>
		/// Date&time of the last refresh
		/// </summary>
		public DateTime LastRefreshed { get; protected set; }

		volatile bool _IsOffline = true;

		/// <summary>
		/// Offline if unable to reach OBLink
		/// </summary>
		public bool IsOffline
		{
			get { return this._IsOffline; }
			protected set 
			{
				// Flag as recovery in progress if the chalkboard just turned online
				if ((this._IsOffline != value) && (!value))
				{
					this.RecoveryInProgress = true;
				}

				this._IsOffline = value; 
			}
		}

		volatile bool _RecoveryInProgress = true;

		/// <summary>
		/// Indicates if the chalkboard was just turned online
		/// </summary>
		public bool RecoveryInProgress
		{
			get { return this._RecoveryInProgress; }
			set { this._RecoveryInProgress = value; }
		}

		/// <summary>
		/// List of episodes managed within the chalkboard
		/// </summary>
		public List<T> Episodes
		{
			get;
			protected set;
		}

		/// <summary>
		/// Simple constructor
		/// </summary>
		public Chalkboard()
		{
			this.EpisodesLockObject = new object();
			this.Episodes = new List<T>();			
		}

		/// <summary>
		/// Updates data and reconciliate. This is the main function that keeps the Chalkboard instance in sync with the GE state
		/// </summary>
		/// <returns>True if successfully refreshed</returns>
		public virtual bool Refresh()
		{
			var now = DateTime.UtcNow;

			#region Retrieving all necessary data from OBLink

			/////////////////////////////////////////////////////////////////
			// Retrieve the list of patients that are live in GE system and their demographic data from GE
			XElement patientList1 = null;
			var gepatients = new Dictionary<string, Patient>();
			int tryCount = 1;

			bool suspiciousTrail = false;

			while (true)
			{
				if (tryCount > Settings_OBLinkChalkboardRefreshRetries)
				{
					// We reached the maximum number of retries here... Leave now
					return false;
				}

				// Retrieve the list of patients
				if (patientList1 == null)
				{
					patientList1 = RetrieveOBLinkPatientList();
					if (patientList1 == null)
					{
						return false;
					}
				}

				// Build a list of interesting patients
				gepatients.Clear();
				foreach (var node in patientList1.Descendants("patient"))
				{
					var mrn = node.Attribute("mrn").Value;
					var bedid = node.Attribute("bedid").Value;

					Patient patient;
					if (gepatients.TryGetValue(mrn, out patient))
					{
						if (string.CompareOrdinal(patient.BedId, bedid) != 0)
						{
							Common.Source.TraceEvent(TraceEventType.Warning, 6401, "Chalkboard: At least 2 beds pretend to contain the same patient.\nBed (bed_id='{0}') and bed (bed_id='{1}').\nPatient: {2}", patient.BedId, bedid, patient);
							return false;
						}
					}
					else
					{
						gepatients[mrn] = new Patient { MRN = mrn, BedId = bedid };
					}
				}

				// Add the mrn that could be out of monitor patients...
				foreach (var episode in this.Episodes.Where(e => e.IsOpen))
				{
					if (!gepatients.ContainsKey(episode.MRN))
					{
						gepatients[episode.MRN] = new Patient { MRN = episode.MRN };
					}
				}

				// Retrieve individual information for all of these patients
				foreach (var mrn in gepatients.Keys.ToList())
				{
					var patient = gepatients[mrn];
					if (!RetrieveOBLinkPatientData(patient))
					{
						if (!string.IsNullOrEmpty(patient.BedId))
						{
							// If that patient is actually in the current chalkboard of GE... leave!
							Common.Source.TraceEvent(TraceEventType.Verbose, 6402, "Chalkboard: Retrieving information for patient {0} failed. Reconciliation skipped!", patient);

							// Retry again
							patientList1 = null;
							break;
						}

						// Then it's an out of scope patient, just remove it from the Dictionary
						gepatients.Remove(mrn);
					}
				}

				// Retry?
				if (patientList1 == null)
				{
					++tryCount;
					continue;
				}

				/////////////////////////////////////////////////////////////////
				// It happened that the OBLink send us the wrong information for the trail so check for that rare occasion (if the previous call already flag as suspicious, don't do it again)
				if (Settings_OBLinkChalkboardCheckForOBLinkPatientMismatch && !suspiciousTrail)
				{
					lock (this.EpisodesLockObject)
					{
						foreach (var oblinkPatient in gepatients.Values)
						{
							var possibleMatch = this.Episodes.FirstOrDefault(p => string.CompareOrdinal(p.MRN, oblinkPatient.MRN) == 0);
							if ((possibleMatch != null)
									&& (string.CompareOrdinal(possibleMatch.PatientName.ToUpperInvariant(), oblinkPatient.Name.ToUpperInvariant()) == 0)
									&& (!oblinkPatient.ComparePartialTrail(possibleMatch.Patient)))
							{
								// Highly unlikely that two patient with the same MRNs and names have complete different trails!
								Common.Source.TraceEvent(TraceEventType.Verbose, 6418, "Chalkboard: Patient {0} appears to be a conflict on the trail with {1}! OBLink mismatch on response suspected. Reconciliation skipped!", oblinkPatient, possibleMatch);

								// Flag as suspicious trail
								suspiciousTrail = true;

								// Redo the get data
								patientList1 = null;
								continue;
							}
						}
					}
				}

				// Retry?
				if (patientList1 == null)
				{
					// We don't decrement the try count for a 'suspicious' trail but we allow for only ONE saving grace
					continue;
				}
					
				/////////////////////////////////////////////////////////////////
				// Make sure the data is consistent and the GE chalkboard did not significally change since it was retrieved at the begining of this method
				var patientList2 = RetrieveOBLinkPatientList();
				if (patientList2 == null)
					return false;

				var table = new Dictionary<string, string>();
				foreach (var p in patientList2.Descendants("patient"))
				{
					table[p.Attribute("mrn").Value] = p.Attribute("bedid").Value;
				}

				// Scan that all patient in the first snapshot are still in the second one and in the same bed and that the RetrieveOBLinkPatientData worked for them
				foreach (var p in patientList1.Descendants("patient"))
				{
					var bedId = string.Empty;
					if ((!table.TryGetValue(p.Attribute("mrn").Value, out bedId)) || (string.CompareOrdinal(bedId, p.Attribute("bedid").Value) != 0))
					{
						Common.Source.TraceEvent(TraceEventType.Verbose, 6403, "Chalkboard: Patient in bed (bed_id='{0}') was moved while retrieving the data from OBLink. Reconciliation skipped!", p.Attribute("bedid").Value);

						// Redo the get data
						patientList1 = patientList2;
						patientList2 = null;
						break;
					}
				}

				if (patientList2 == null)
				{
					++tryCount;
					continue;
				}

				// If we reach that point, it means that we have a successful refresh
				break;
			}

			// Do a copy of the gepatients for dump later on
			var patientsAsReturnedByOBLink = gepatients.Values.ToList();

			#endregion

			#region Matching the current in memory data with the data retrieved from OBLink

			/////////////////////////////////////////////////////////////////
			// Match the list of patient from GE with the current list of episode in memory

			// Now that we are about to update the episode list, lock the complete chalkboard
			lock (this.EpisodesLockObject)
			{
				// Reset the reconciliation flag
				foreach (var episode in this.Episodes)
				{
					episode.Reconciliated = (episode.EpisodeStatus == EpisodeStatuses.Closed);
				}

				List<T> candidates;
				Patient gepatient;

				// Reconciliation of 'merge in progress' episodes
				// The MRN of a patient that is actually 'merge in progress' cannot be changed in GE so in that case, the MRN is a reliable primary key (for once)
				candidates = this.Episodes.Where(e => !e.Reconciliated && e.MergeInProgress).ToList();
				foreach (var episode in candidates)
				{
					// Find a corresponding entry in the GE list of patient, based ONLY on the id if the merge is still in progress
					if ((gepatients.TryGetValue(episode.MRN, out gepatient)) && (gepatient.IsMergeInProgress))
					{
						// The episode is still 'merge in progress', reconciliation done
						episode.UpdatePatient(gepatient, "Reconciliation of 'merge in progress' episodes");
						gepatients.Remove(gepatient.MRN);
					}
				}

				// Reconciliation of LIVE episodes with same MRN, same bed and exact same trail
				candidates = this.Episodes.Where(e => !e.Reconciliated && e.EpisodeStatus == EpisodeStatuses.Normal).ToList();
				foreach (var episode in candidates)
				{
					Debug.Assert(!string.IsNullOrEmpty(episode.BedId), "An episode that is in a normal state must have a bed id");

					if ((gepatients.TryGetValue(episode.MRN, out gepatient))
						&& (!string.IsNullOrEmpty(gepatient.BedId)) && (!string.IsNullOrEmpty(episode.BedId)) && (string.CompareOrdinal(gepatient.BedId, episode.BedId) == 0)
						&& (gepatient.CompareFullTrail(episode.Patient)))
					{
						episode.UpdatePatient(gepatient, "Reconciliation of LIVE episodes with same MRN, same bed and exact same trail");
						gepatients.Remove(gepatient.MRN);
					}
				}

				// Reconciliation of LIVE episodes with same MRN and just a monitor turned off
				candidates = candidates.Where(e => !e.Reconciliated).ToList();
				foreach (var episode in candidates)
				{
					if ((gepatients.TryGetValue(episode.MRN, out gepatient))
						&& (string.IsNullOrEmpty(gepatient.BedId))
						&& (gepatient.CompareFullTrail(episode.Patient)))
					{
						episode.UpdatePatient(gepatient, "Reconciliation of LIVE episodes with same MRN and just a monitor turned off");
						gepatients.Remove(gepatient.MRN);
					}
				}

				// Reconciliation of LIVE episodes with just a change MRN but same bed and exact same trail
				candidates = candidates.Where(e => !e.Reconciliated).ToList();
				foreach (var episode in candidates)
				{
					gepatient = gepatients.Values.FirstOrDefault(p =>
														(!string.IsNullOrEmpty(p.BedId)) && (!string.IsNullOrEmpty(episode.BedId)) && (string.CompareOrdinal(p.BedId, episode.BedId) == 0)
														&& (p.CompareFullTrail(episode.Patient)));
					if (gepatient != null)
					{
						episode.UpdatePatient(gepatient, "Reconciliation of LIVE episodes with just a change MRN but same bed and exact same trail");
						gepatients.Remove(gepatient.MRN);

						continue;
					}
				}

				// Reconciliation of LIVE episodes with same MRN just a transfer
				candidates = candidates.Where(e => !e.Reconciliated).ToList();
				foreach (var episode in candidates)
				{
					if ((gepatients.TryGetValue(episode.MRN, out gepatient)) && (gepatient.ComparePartialTrail(episode.Patient)))
					{
						episode.UpdatePatient(gepatient, "Reconciliation of LIVE episodes with same MRN just a transfer");
						gepatients.Remove(gepatient.MRN);
					}
				}

				// Reconciliation of ANY episodes with same MRN and exact same trail
				candidates = this.Episodes.Where(e => !e.Reconciliated).ToList();
				foreach (var episode in candidates)
				{
					if ((gepatients.TryGetValue(episode.MRN, out gepatient)) && (gepatient.CompareFullTrail(episode.Patient)))
					{
						episode.UpdatePatient(gepatient, "Reconciliation of ANY episodes with same MRN and exact same trail");
						gepatients.Remove(gepatient.MRN);
					}
				}

				// Reconciliation of ANY episodes with same MRN and partial trail
				candidates = this.Episodes.Where(e => !e.Reconciliated).ToList();
				foreach (var episode in candidates)
				{
					if ((gepatients.TryGetValue(episode.MRN, out gepatient)) && (gepatient.ComparePartialTrail(episode.Patient)))
					{
						episode.UpdatePatient(gepatient, "Reconciliation of ANY episodes with same MRN and partial trail");
						gepatients.Remove(gepatient.MRN);
					}
				}

				// Reconciliation of ANY episodes with a combinaison of transfer and change MRN (exact match on the trail)
				candidates = this.Episodes.Where(e => !e.Reconciliated).ToList();
				foreach (var episode in candidates)
				{
					var possibles = gepatients.Values.Where(p => p.CompareFullTrail(episode.Patient)).ToList();

					if (possibles.Count > 1)
					{
						var msg = new StringBuilder();
						possibles.ForEach(p => msg.AppendLine(string.Format(CultureInfo.InvariantCulture, "Possible: {0}", p)));

						Common.Source.TraceEvent(TraceEventType.Warning, 6404, "Chalkboard: Multiple matches in reconciliation for patient {0}. Closing the episode!\n{1}", episode, msg.ToString());

						// We have a problem here... Drop/restart
						episode.UpdateStatus(EpisodeStatuses.Closed, "Multiple matches in reconciliation for patient - ErrorCode 2036");
					}
					else if (possibles.Count == 1)
					{
						gepatient = possibles.First();

						// TODO Maybe add some extra security here with a check on other items like tracing or...
						// ...

						// The episode is a match, but the id was changed and at least one transfer
						episode.UpdatePatient(gepatient, "Reconciliation of ANY episodes with a combinaison of transfer and change MRN (exact match on the trail)");
						gepatients.Remove(gepatient.MRN);
					}
				}

				// Reconciliation of ANY episodes with a combinaison of transfer and change MRN (partial match on the trail)
				candidates = this.Episodes.Where(e => !e.Reconciliated).ToList();
				foreach (var episode in candidates)
				{
					var possibles = gepatients.Values.Where(p => p.ComparePartialTrail(episode.Patient)).ToList();

					if (possibles.Count > 1)
					{
						var msg = new StringBuilder();
						possibles.ForEach(p => msg.AppendLine(string.Format(CultureInfo.InvariantCulture, "Possible: {0}", p)));

						Common.Source.TraceEvent(TraceEventType.Warning, 6405, "Chalkboard: Multiple matches in reconciliation for patient {0}. Closing the episode!\n{1}", episode, msg.ToString());

						// We have a problem here... Drop/restart
						episode.UpdateStatus(EpisodeStatuses.Closed, "Multiple matches in reconciliation for patient - ErrorCode 2037");
					}
					else if (possibles.Count == 1)
					{
						gepatient = possibles.First();

						// TODO Maybe add some extra security here with a check on other items like tracing or...
						// ...

						// The episode is a match, but the id was changed and at least one transfer
						episode.UpdatePatient(gepatient, "Reconciliation of ANY episodes with a combinaison of transfer and change MRN (partial match on the trail)");
						gepatients.Remove(gepatient.MRN);
					}
				}

				// Check for episode still not reconciliated and mark them as out of scope
				candidates = this.Episodes.Where(e => !e.Reconciliated).ToList();
				foreach (var episode in candidates)
				{
					episode.UpdateStatus(EpisodeStatuses.OutOfScope, "No match at all for that episode found");
				}

				// Reconciliation of new patient in GE
				foreach (var patient in gepatients.Values.Where(p => !string.IsNullOrEmpty(p.BedId)))
				{
					var episode = new T();
					episode.UpdatePatient(patient, "Reconciliation of new patient in GE");
					this.Episodes.Add(episode);
				}

				// Check for patient off the monitor for too long
				candidates = this.Episodes.Where(e => e.EpisodeStatus == EpisodeStatuses.OutOfMonitor).ToList();
				foreach (var episode in candidates)
				{
					if (episode.LastMonitored == DateTime.MinValue)
					{
						Debug.Assert(false, "Out of monitor patient should have a 'last tracing' date");
					}
					else if (now.Subtract(episode.LastMonitored).TotalHours >= Settings_EpisodeOutOfMonitorToOutOfScopeDuration)
					{
						episode.UpdateStatus(EpisodeStatuses.OutOfScope, "That episode is out of monitor for a long time and reached the maximum time limit for that status");
					}
				}

				// Check for patient out of scope for too long
				candidates = this.Episodes.Where(e => e.EpisodeStatus == EpisodeStatuses.OutOfScope).ToList();
				foreach (var episode in candidates)
				{
					if (episode.LastMonitored == DateTime.MinValue)
					{
						Debug.Assert(false, "Out of scope patient should have a 'last tracing' date");
					}
					else if (now.Subtract(episode.LastMonitored).TotalHours >= Settings_EpisodeOutOfMonitorToCloseDuration)
					{
						episode.UpdateStatus(EpisodeStatuses.Closed, "That episode is out of scope for a long time and reached the maximum time limit for that status");
					}
				}

				// Check for Out of scope episodes that are now also in scope!
				candidates = this.Episodes.Where(e => e.EpisodeStatus == EpisodeStatuses.OutOfScope).ToList();
				foreach (var episode in candidates)
				{
					var conflict = this.Episodes.FirstOrDefault(e => e.IsOpen && (string.CompareOrdinal(e.MRN, episode.MRN) == 0));
					if (conflict != null)
					{
						Common.Source.TraceEvent(TraceEventType.Verbose, 6406, "Chalkboard: An out of scope patient {0} has the same MRN as an other valid patient {1}. Closing the episode!", episode, conflict);
						episode.EpisodeStatus = EpisodeStatuses.Closed;
						episode.Updated = true;
					}
				}

				// Check for Out of scope episodes that have a trail matching with another open episode
				candidates = this.Episodes.Where(e => e.EpisodeStatus == EpisodeStatuses.OutOfScope).ToList();
				foreach (var episode in candidates)
				{
					var conflict = this.Episodes.FirstOrDefault(e => e.IsOpen && e.Patient.ComparePartialTrail(episode.Patient));
					if (conflict != null)
					{
						Common.Source.TraceEvent(TraceEventType.Verbose, 6407, "Chalkboard: An out of scope patient {0} has the same MRN as an other valid patient {1}. Closing the episode!", episode, conflict);
						episode.EpisodeStatus = EpisodeStatuses.Closed;
						episode.Updated = true;
					}
				}

				// Security check: check for open episodes that have the exact same trails!
				candidates = this.Episodes.Where(e => e.EpisodeStatus != EpisodeStatuses.Closed).ToList();
				while (candidates.Count > 0)
				{
					var episode = candidates.First();
					var matches = candidates.Where(e => (e == episode)			// The partial trail comparison is not a symetrical function
						|| e.Patient.ComparePartialTrail(episode.Patient)		// so it must be check both ways...
						|| episode.Patient.ComparePartialTrail(e.Patient)).OrderBy(e => e.MRN).ToList();

					Debug.Assert(matches.Count > 0 && matches.Contains(episode));

					// If there is a conflict, directly close the out of scope. It's a typical case when there is a merge.
					if (matches.Count > 1)
					{
						var outOfScopes = matches.Where(e => e.EpisodeStatus == EpisodeStatuses.OutOfScope).ToList();
						outOfScopes.ForEach(e =>
							{
								Common.Source.TraceEvent(TraceEventType.Verbose, 6408, "Chalkboard: Multiple matches in reconciliation for out of scope patient {0}. Closing the episode!", e);
								e.UpdateStatus(EpisodeStatuses.Closed, "Multiple matches in reconciliation for out of scope patient - ErrorCode 2038");
								matches.Remove(e);
							});
					}

					// Is there still a conflict?
					if (matches.Count() > 1)
					{
						// Build the MRN lists
						var mrns = new StringBuilder();
						foreach (var e in matches)
						{
							if (mrns.Length > 0) mrns.Append(", ");
							mrns.Append(e.MRN);
						}

						matches.ForEach(e =>
							{
								e.SetConflict(mrns.ToString());
								candidates.Remove(e);
							});
					}

					// There is no conflict
					else
					{
						episode.ResetConflict();
						candidates.Remove(episode);
					}

					Debug.Assert(!candidates.Contains(episode));
				}
			}
	
			// Dump if required
			if ((Settings_TraceExtendedReconciliationData) && (this.Episodes.Any(e => e.Updated)))
			{
				var sbDump = new StringBuilder();
				using (var writer = XmlWriter.Create(sbDump, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Auto }))
				{
					writer.WriteStartDocument();
					writer.WriteStartElement("reconciliation");

					writer.WriteStartElement("gecensus");
					writer.WriteRaw(patientList1.ToString());
					writer.WriteEndElement();

					{
						writer.WriteStartElement("gedata");
						var ns = new System.Xml.Serialization.XmlSerializerNamespaces();
						ns.Add(string.Empty, string.Empty);

						var xs = new System.Xml.Serialization.XmlSerializer(typeof(Patient));
						foreach (var p in patientsAsReturnedByOBLink)
						{
							xs.Serialize(writer, p, ns);
						}
						writer.WriteEndElement();
					}

					{
						writer.WriteStartElement("chalkboard");
						var ns = new System.Xml.Serialization.XmlSerializerNamespaces();
						ns.Add(string.Empty, string.Empty);

						var xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
						foreach (var e in this.Episodes)
						{
							xs.Serialize(writer, e, ns);
						}
						writer.WriteEndElement();
					}

					writer.WriteEndElement();
					writer.WriteEndDocument();
					writer.Flush();
				}
				Common.Source.TraceData(TraceEventType.Verbose, 6409, sbDump.ToString());
			}

			#endregion

			this.LastRefreshed = now;

			return true;
		}

		/// <summary>
		/// Execute the query and return the response as plain text
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		static string GetOBLinkData(Uri uri, int timeout)
		{
			var request = Common.CreateRequest(uri);
			request.Timeout = timeout;

			try
			{
				string result = null;

				using (var response = request.GetResponse() as HttpWebResponse)
				using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
				{
					Debug.Assert(!response.IsFromCache, "The response came from the cache!!");

					result = reader.ReadToEnd();
					reader.Close();
					response.Close();
				}

				#region Dump

				if (Settings_TraceExtendedOBLinkData)
				{
					Common.Source.TraceData(TraceEventType.Verbose, 6410, string.Format(CultureInfo.InstalledUICulture, "GetOBLinkData\nUri={0}\nTimeout={1}\n\n{2}", uri, timeout, result.ToString()));
				}

				#endregion

				return result;
			}
			catch (Exception e)
			{
				if (Settings_TraceExtendedInformationWithPHIData)
				{
					Common.Source.TraceEvent(TraceEventType.Error, 6411, "Chalkboard: Error, unable to perform web call to OBLINK {0}.\n{1}", uri, e);
				}
				else
				{
					Common.Source.TraceEvent(TraceEventType.Error, 6412, "Chalkboard: Error, unable to perform web call to OBLINK.\n{0}", e);
				}

				try
				{
					if (request != null)
					{
						request.Abort();
					}
				}
				catch (Exception) { }
			}

			return null;
		}

		/// <summary>
		/// Query patient in chalkboard.
		/// </summary>
		/// <returns>XElement that contains the information from the GE list of patients.</returns>
		protected static XElement RetrieveOBLinkPatientList()
		{
			var uri =
				new Uri(
					new Uri(Settings_OBLinkURL),
					string.Format(
						CultureInfo.InvariantCulture,
						Settings_OBLinkChalkboardRequestUrl,
						Settings_OBLinkUsername,
						Settings_OBLinkPassword));

			// Retrieve the patient list from the OBLink web service
			var response = GetOBLinkData(uri, Settings_OBLinkChalkboardRequestTimeout);
			if (string.IsNullOrEmpty(response))
				return null;

			// Process the answer
			var rx = new Regex("(OBLink_FetalWave.asp.*?>)");
			var matches = rx.Matches(response);

			var root = new XElement("patients");

			// Scan each line
			foreach (Match match in matches)
			{
				string value = match.Value;
				if (value.Contains("b=") && value.Contains("pid="))
				{
					value = value.Replace(">", string.Empty).Replace("OBLink_FetalWave.asp?", string.Empty);
					value = System.Web.HttpUtility.UrlDecode(value);

					string[] BedPatient = value.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
					string[] bed = BedPatient[0].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
					string[] patient = BedPatient[1].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

					///Add new patient and bed if it is not exist
					root.Add(new XElement("patient", new XAttribute("bedid", bed[1]), new XAttribute("mrn", patient[1])));
				}
			}

			return root;
		}

		/// <summary>
		/// Fill patient object with data from OBLink
		/// </summary>
		/// <param name="patient">Patient object to be filled with data. This object must provides MRN and Bed Id.</param>
		/// <returns>False if it fails</returns>
		static bool RetrieveOBLinkPatientData(Patient patient)
		{
			try
			{
				string response;

				// Retrieve the patient data
				var uriData =
					new Uri(
						new Uri(Settings_OBLinkURL),
						string.Format(
							CultureInfo.InvariantCulture,
							Settings_OBLinkPatientDataRequestUrl,
							Settings_OBLinkUsername,
							Settings_OBLinkPassword,
							patient.MRN));

				response = GetOBLinkData(uriData, Settings_OBLinkPatientRequestTimeout);
				if ((string.IsNullOrEmpty(response)) || (!patient.PopulateData(response)))
					return false;

				// Retrieve the patient location trail
				var uriTrail =
					new Uri(
						new Uri(Settings_OBLinkURL),
						string.Format(
							CultureInfo.InvariantCulture,
							Settings_OBLinkPatientTrailRequestUrl,
							Settings_OBLinkUsername,
							Settings_OBLinkPassword,
							patient.MRN));

				response = GetOBLinkData(uriTrail, Settings_OBLinkPatientRequestTimeout);
				if ((string.IsNullOrEmpty(response)) || (!patient.PopulateTrailList(response)))
					return false;

				// Retrieve the patient ADL log
				var uriADT =
					new Uri(
						new Uri(Settings_OBLinkURL),
						string.Format(
							CultureInfo.InvariantCulture,
							Settings_OBLinkPatientADTRequestUrl,
							Settings_OBLinkUsername,
							Settings_OBLinkPassword,
							patient.MRN));

				response = GetOBLinkData(uriADT, Settings_OBLinkPatientRequestTimeout);
				if ((string.IsNullOrEmpty(response)) || (!patient.PopulateADTLogList(response)))
					return false;

				return true;
			}
			catch (Exception ex)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 6413, "Chalkboard: Error, exception retrieving patient data for {0}.\n{1}", patient, ex);
				return false;
			}
		}
	}
}
