using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using PeriGen.Patterns.ActiveXInterface;
using PeriGen.Patterns.GE.Interface;
using PeriGen.Patterns.GE.Service;
using PeriGen.Patterns.Settings;

namespace PeriGen.Patterns
{
	[ServiceBehavior(
			Name = "PatternsDataFeed",
			Namespace = "http://www.perigen.com/2011/09/patterns/",
			IncludeExceptionDetailInFaults = true,
			ConfigurationName = "PeriGen.Patterns.PatternsDataFeed",
			InstanceContextMode = InstanceContextMode.Single,
			ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class PatternsDataFeed : IPatternsDataFeed
	{
		#region Settings

		static bool Settings_TraceExtendedActiveXData = SettingsManager.GetBoolean("TraceExtendedActiveXData");
		static bool Settings_DisableAdminAPILocalIPValidation = SettingsManager.GetBoolean("DisableAdminAPILocalIPValidation");

		static string Settings_ValidationVersionActiveX = SettingsManager.GetValue("ValidationVersionActiveX");

		static int Settings_MaxDaysOfTracingReturnedToActiveX = SettingsManager.GetInteger("MaxDaysOfTracingReturnedToActiveX");

		#endregion

		#region WCF host

		/// <summary>
		/// The WCF service host
		/// </summary>
		static ServiceHost Host { get; set; }

		/// <summary>
		/// Start the WCF host
		/// </summary>
		public static void StartHost()
		{
			if (Host != null)
			{
				StopHost();
			}

			Host = new ServiceHost(typeof(PatternsDataFeed));
			Host.Open();
		}

		/// <summary>
		/// Stop the WCF host
		/// </summary>
		public static void StopHost()
		{
			if (Host != null)
			{
				Host.Close(TimeSpan.FromSeconds(5));
				Host = null;
			}
		}

		#endregion

		#region IPatternsDataFeed Members

		/// <summary>
		/// Get patient data an return it into an xml that the ActiveX can read
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public XElement GetPatternsData(XElement param)
		{
			var watch = Stopwatch.StartNew();
			bool full_request = false;

			// Prepare the response
			var root = new XElement("data", new XAttribute("server_time", DateTime.UtcNow.ToEpoch().ToString(CultureInfo.InvariantCulture)));
			if (PeriGen.Patterns.GE.Service.Common.DemoMode)
			{
				root.SetAttributeValue("demo_mode", 1);
			}

			var user = param.Attribute("user") != null ? param.Attribute("user").Value : "n/a";
			
			var version = param.Attribute("version");
			if ((version != null) && (!ValidateActiveXVersion(version.Value)))
			{
				root.SetAttributeValue("invalid_version", 1);
			}
			else
			{
				foreach (var node in param.Descendants("request"))
				{
					// Read the header
					PatternsRequestHeader header = new PatternsRequestHeader(node);
					XElement element = null;

					// OBLink not connected?
					if (PeriGen.Patterns.GE.Service.Common.Chalkboard.IsOffline)
					{
						element = new XPatient
						{
							Status = StatusType.Error,
							StatusDetails = "OBLink cannot be reached."
						}.EncodeForActiveX();

						// Flush the header
						header.Reset();
						element.Add(header.SerializeXml());

						root.Add(element);
						continue;
					}

					// Find the requested episode of tracings
					PatternsEpisode episode = null;

					// If there is a unique ID in the header, use if and make sure it's still a good one
					if (!string.IsNullOrEmpty(header.Id))
					{
						int unique_id;
						if ((int.TryParse(header.Id, out unique_id)) && (unique_id > 0))
						{
							lock (PeriGen.Patterns.GE.Service.Common.Chalkboard.EpisodesLockObject)
							{
								episode = (from ep in PeriGen.Patterns.GE.Service.Common.Chalkboard.Episodes
										   where ep.PatientUniqueId == unique_id
										   select ep).FirstOrDefault();
							}
						}

						// Got an episode? Make sure it's not closed
						if ((episode != null) && (!episode.IsOpen))
						{
							episode = null;
						}
					}

					// Not episode yet?
					if (episode == null)
					{
						// Make sure to indicate this is a full refresh
						header.Reset();

						// Revert to the MRN search
						lock (PeriGen.Patterns.GE.Service.Common.Chalkboard.EpisodesLockObject)
						{
							episode = (from ep in PeriGen.Patterns.GE.Service.Common.Chalkboard.Episodes
									   where (string.CompareOrdinal(ep.MRN, header.Key) == 0) && ep.IsOpen
									   select ep).FirstOrDefault();
						}
					}

					// Still no episode?
					if (episode == null)
					{
						element = new XPatient
										{
											Status = StatusType.Invalid,
											StatusDetails = "The selected patient is not actively monitored and does not have an active episode of tracing."
										}.EncodeForActiveX();

						// Flush the header
						header.Reset();
						element.Add(header.SerializeXml());

						root.Add(element);
						continue;
					}

					// We did find a valid episode
					lock (episode.LockObject)
					{
						// We have an episode BUT there is a merge in progress.
						if (episode.MergeInProgress)
						{
							// Send a skeleton of a patient just to inform the user of what is happening
							element = new XPatient
											{
												Status = StatusType.Invalid,
												StatusDetails = "Merge in progress"
											}.EncodeForActiveX();

							// Flush the header
							header.Reset();
							header.Key = episode.MRN; // Make sure we track the MRN. No need to track the id since a 'MergeInProgress' episode has no unique id yet, it does not exist in db yet
							element.Add(header.SerializeXml());

							root.Add(element);
							continue;
						}

						// We have an episode BUT there is a reconciliation conflict
						if (!string.IsNullOrEmpty(episode.ReconciliationConflict))
						{
							// Send a skeleton of a patient just to inform the user of what is happening
							element = new XPatient
											{
												Status = StatusType.Invalid,
												StatusDetails = "Patient conflict detected. To resolve the conflict, transfer the affected patients (" + episode.ReconciliationConflict + ") to different beds and then back."
											}.EncodeForActiveX();

							// Flush the header
							header.Reset();
							header.Key = episode.MRN; // Make sure we track the MRN. No need to track the id since a 'MergeInProgress' episode has no unique id yet, it does not exist in db yet
							element.Add(header.SerializeXml());

							root.Add(element);
							continue;
						}

						// If we reach that point, then we have an episode that is open and we can return useful informations to the ActiveX
						header.Id = episode.PatientUniqueId.ToString(CultureInfo.InvariantCulture);
						header.Key = episode.MRN;

						// Prepare the response for that patient
						element = episode.EncodeForActiveX();

						////////////////////////////////////////////////////////////////////////////////////
						//// TRACINGS 
						DateTime firstTracing = DateTime.MinValue;
						{
							long lastTracingId = long.Parse(header.LastTracing ?? "-1", CultureInfo.InvariantCulture);

							List<PeriGen.Patterns.Engine.Data.TracingBlock> tracings = null;
							if (lastTracingId > -1)
							{
								// If it is a partial update, then get all new tracings since the last one returned
								tracings = (from t in episode.TracingBlocks where t.Id > lastTracingId orderby t.Id select t).ToList();
							}
							else
							{
								full_request = true;

								// If it is a complete refresh, then get all the tracings but for a maximum duration
								DateTime timeLimit = DateTime.UtcNow.AddDays(-Settings_MaxDaysOfTracingReturnedToActiveX).RoundToTheSecond();
								tracings = (from t in episode.TracingBlocks where t.End >= timeLimit orderby t.Id select t).ToList();

								// User activity log...
								PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 9916, "User '{0}' view pattern's tracing for episode ({1})", user, episode);
						
								// PerformanceCounters 
								PerformanceCounterHelper.AddPatientsOpened(1);
							}

							if (tracings.Count > 0)
							{
								element.Add(DataEncoder.EncodeForActiveX(PeriGen.Patterns.Engine.Data.TracingBlock.Merge(tracings, 60)));
								header.LastTracing = tracings.Last().Id.ToString(CultureInfo.InvariantCulture);
								firstTracing = tracings.First().Start; // Remember the start time of the oldest tracing block returned, used later on for the artifacts							
							}
						}

						////////////////////////////////////////////////////////////////////////////////////
						//// USER ACTIONS 
						{
							long lastActionId = long.Parse(header.LastAction ?? "-1", CultureInfo.InvariantCulture);
							var actions = (from a in episode.TracingActions where a.Id > lastActionId orderby a.Id select a).ToList();
							if (actions.Count > 0)
							{
								element.Add(XUserAction.EncodeForActiveX(actions));
								header.LastAction = actions.Last().Id.ToString(CultureInfo.InvariantCulture);
							}
						}

						////////////////////////////////////////////////////////////////////////////////////
						//// ARTIFACTS
						{
							long lastArtifactId = long.Parse(header.LastArtifact ?? "-1", CultureInfo.InvariantCulture);

							List<PeriGen.Patterns.Engine.Data.DetectionArtifact> artifacts = null;
							if (lastArtifactId > -1)
							{
								// If it is a partial update, then get all new artifacts since the last one returned
								artifacts = (from a in episode.TracingArtifacts where a.Id > lastArtifactId orderby a.Id select a).ToList();
							}
							else
							{
								// If it is a complete refresh, then get all the artifacts for the returned tracings
								artifacts = (from a in episode.TracingArtifacts where a.StartTime >= firstTracing orderby a.Id select a).ToList();
							}
							if (artifacts.Count > 0)
							{
								element.Add(DataEncoder.EncodeForActiveX(artifacts));
								header.LastArtifact = artifacts.Last().Id.ToString(CultureInfo.InvariantCulture);
							}
						}
					}

					////////////////////////////////////////////////////////////////////////////////////
					/// HEADER (to remember current request, used for next incremental updates
					element.Add(header.SerializeXml());
					root.Add(element);
				}
			}

			// Trace
			if (Settings_TraceExtendedActiveXData)
			{
				PatternsTask.Source.TraceEvent(
					TraceEventType.Verbose, 
					9910,
					string.Format(CultureInfo.InvariantCulture, "GetPatternsData ({0} ms)\n-- From:\n{1}\n-- Request:\n{2}\n-- Answer:\n{3}", 
					watch.ElapsedMilliseconds, 
					((System.ServiceModel.Channels.RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name]).Address,
					param, 
					full_request ? "Full refresh, too big for dump!" : root.ToString()));
			}

			return root;
		}

		/// <summary>
		/// Perform a user action and save it in the database
		/// </summary>
		/// <param name="param"></param>
		public void PerformUserAction(XElement param)
		{
			// The request may concern multiple actions
			var actions = param.DescendantsAndSelf("action").Select(a => XUserAction.FromActiveX(a)).ToList();

			foreach (var patient_id in actions.Select(a => a.PatientId).Distinct())
			{
				var patientActions = actions.Where(a => a.PatientId == patient_id);

				PatternsEpisode episode = null;
				lock (PeriGen.Patterns.GE.Service.Common.Chalkboard.EpisodesLockObject)
				{
					episode = (from ep in PeriGen.Patterns.GE.Service.Common.Chalkboard.Episodes
							   where ep.PatientUniqueId == patient_id
							   select ep).FirstOrDefault();
				}

				if (episode != null)
				{
					episode.AddActions(patientActions);
		
					// User activity log...
					PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 9917, "User actions '{0}' for episode ({1})", param, episode);

					// Performance counter
					PerformanceCounterHelper.AddUserActions(patientActions.Count());
				}
			}
		}

		/// <summary>
		/// Check if the active X version meet the requested requirements
		/// </summary>
		/// <param name="activeXVersion"></param>
		/// <returns>True if the version is valid</returns>
		static bool ValidateActiveXVersion(string activeXVersion)
		{
			if (string.IsNullOrEmpty(Settings_ValidationVersionActiveX))
				return true;

			if (string.IsNullOrEmpty(activeXVersion))
				return false;

			// Easy comparison, we want the EXACT version!
			if (string.CompareOrdinal(activeXVersion.ToUpperInvariant(), Settings_ValidationVersionActiveX.ToUpperInvariant()) != 0)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Warning, 7213, "An obsolete client asked for patient data, request rejected!\nClient version: {0} Requested version: {1}", activeXVersion, Settings_ValidationVersionActiveX);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Check if the current client IP address is local
		/// </summary>
		/// <returns></returns>
		static bool ValidateClientAddressIsLocalIP()
		{
			if (Settings_DisableAdminAPILocalIPValidation)
			{
				return true;
			}

			try
			{ 
				// Get the client IP address
				var clientAddress = ((System.ServiceModel.Channels.RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name]).Address;

				// Get corresponding IP addresses
				var clientIPs = System.Net.Dns.GetHostAddresses(clientAddress);
				var localIPs = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());

				foreach (var clientIP in clientIPs)
				{
					// A loopback?
					if (System.Net.IPAddress.IsLoopback(clientIP))
						return true;

					if (localIPs.Any(ip => clientIP.Equals(ip)))
						return true;
				}

				PatternsTask.Source.TraceEvent(TraceEventType.Warning, 7211, "Someone tried to use an admin API of the web service from a remote computer {0}. Request is rejected.", clientAddress);
			}
			catch (Exception e)
			{ 
				PatternsTask.Source.TraceEvent(TraceEventType.Error, 7212, "Error while validating the IP address of the client of an admin API.\n{0}", e);
			}
			return false;
		}

		/// <summary>
		/// Return the list of active episodes of tracings as an xml
		/// </summary>
		/// <returns></returns>
		public XElement GetPatientList()
		{
			if (!ValidateClientAddressIsLocalIP())
			{
				return null;
			}

			List<PatternsEpisode> episodes = null;
			lock (PeriGen.Patterns.GE.Service.Common.Chalkboard.EpisodesLockObject)
			{
				if (!PeriGen.Patterns.GE.Service.Common.Chalkboard.IsOffline)
				{
					episodes = PeriGen.Patterns.GE.Service.Common.Chalkboard.Episodes.OrderBy(e => e.EpisodeStatus).ThenBy(e => e.PatientUniqueId).ThenBy(e => e.Created).ToList();
				}
			}

			var sb = new StringBuilder();
			using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true }))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("Episodes");
				writer.WriteAttributeString("LastRefreshed", PeriGen.Patterns.GE.Service.Common.Chalkboard.LastRefreshed.ToString("s", CultureInfo.InvariantCulture));
				writer.WriteAttributeString("IsOffline", PeriGen.Patterns.GE.Service.Common.Chalkboard.IsOffline.ToString());

				if (episodes != null)
				{
					foreach (var episode in episodes)
					{
						writer.WriteStartElement("Episode");
						writer.WriteAttributeString("PatientUniqueId", episode.PatientUniqueId.ToString(CultureInfo.InvariantCulture));
						writer.WriteAttributeString("DatabaseFile", episode.DatabaseFile ?? string.Empty);
						writer.WriteAttributeString("EpisodeStatus", episode.EpisodeStatus.ToString());
						writer.WriteAttributeString("RecoveryInProgress", (episode.EpisodeStatus == EpisodeStatuses.Normal ? episode.RecoveryInProgress : false).ToString(CultureInfo.InvariantCulture));
						writer.WriteAttributeString("MergeInProgress", episode.MergeInProgress.ToString(CultureInfo.InvariantCulture));
						writer.WriteAttributeString("ReconciliationConflict", episode.ReconciliationConflict ?? string.Empty);
						writer.WriteAttributeString("Created", episode.Created.ToString("s", CultureInfo.InvariantCulture));
						writer.WriteAttributeString("LastUpdated", episode.LastUpdated.ToString("s", CultureInfo.InvariantCulture));
						writer.WriteAttributeString("LastMonitored", episode.LastMonitored.ToString("s", CultureInfo.InvariantCulture));
						writer.WriteAttributeString("RealTimeLatency", episode.RealTimeLatency.ToString(CultureInfo.InvariantCulture));
						episode.Patient.WriteToXml(writer);
						writer.WriteEndElement();
					}
				}
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
			}

			return XElement.Parse(sb.ToString());
		}

		/// <summary>
		/// Close the active patient matching the given id
		/// </summary>
		/// <param name="param"></param>
		public void ClosePatient(string id)
		{
			if (!ValidateClientAddressIsLocalIP())
			{
				return;
			}

			int unique_id;
			if (!int.TryParse(id, NumberStyles.Number, CultureInfo.InvariantCulture, out unique_id))
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Error, 7201, "Unable to close a patient, the given parameter is not an integer.\n{0}", id);
				return;
			}

			if (unique_id <= 0)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Warning, 7202, "Unable to close a patient, the given parameter is not a valid database id.\n{0}", unique_id);
				return;
			}

			lock (PeriGen.Patterns.GE.Service.Common.Chalkboard.EpisodesLockObject)
			{
				var episode = (from ep in PeriGen.Patterns.GE.Service.Common.Chalkboard.Episodes
							   where ep.PatientUniqueId == unique_id
							   select ep).FirstOrDefault();

				if (episode == null)
				{
					PatternsTask.Source.TraceEvent(TraceEventType.Warning, 7203, "Unable to close a patient, the given id is not a valid episode.\n{0}", unique_id);
					return;
				}

				if (episode.EpisodeStatus == EpisodeStatuses.Closed)
				{
					PatternsTask.Source.TraceEvent(TraceEventType.Information, 7204, "Unable to close a patient, it is already closed.\n{0}", unique_id);
					return;
				}

				lock (PeriGen.Patterns.GE.Service.Common.Chalkboard.EpisodesLockObject)
				{
					episode.UpdateStatus(EpisodeStatuses.Closed, "Closing episode as per requested by user");
				}
				PatternsTask.Source.TraceEvent(TraceEventType.Information, 7205, "Closing episode ({0}) as per requested by user.", episode);
			}
		}

        /// <summary>
        /// Get plugins and return it into an xml that the ActiveX can read
        /// </summary>       
        /// <returns></returns>
        public XElement GetPlugins()
        {
            // Prepare the response
            XElement toRet = new XElement("Plugins", new XAttribute("name", string.Empty));
            return toRet;
        }
		#endregion
    }
}
