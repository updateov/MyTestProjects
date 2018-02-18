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
using PeriGen.Patterns.Engine.Data;

namespace PeriGen.Patterns.Service
{
	[ServiceBehavior(
			Name = "PatternsDataFeed",
			Namespace = "http://www.perigen.com/2011/09/patterns/",
			IncludeExceptionDetailInFaults = true,
			ConfigurationName = "PeriGen.Patterns.PatternsDataFeed",
			InstanceContextMode = InstanceContextMode.Single,
			ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class PatternsDataFeed : ICurveDataFeed
	{
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

			//Avoid issues with certificates
			System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(delegate { return true; });

            string httpAddress = String.Format(PatternsServiceSettings.Instance.WCFServiceBaseAddress, Environment.MachineName);

			//Check Security
			var isSecure = httpAddress.ToUpperInvariant().StartsWith("HTTPS");

			//Binding
			WebHttpBinding wsBinding = new WebHttpBinding();

			if (isSecure)
			{
				wsBinding.Security.Mode = WebHttpSecurityMode.Transport;
				wsBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
			}
			else
			{
				wsBinding.Security.Mode = WebHttpSecurityMode.None;
			}

			wsBinding.MaxReceivedMessageSize = 2147483647;
			wsBinding.ReaderQuotas.MaxArrayLength = 2147483647;
			wsBinding.ReaderQuotas.MaxStringContentLength = 2147483647;
			wsBinding.ReaderQuotas.MaxBytesPerRead = 2147483647;
			wsBinding.CloseTimeout = new TimeSpan(0, 1, 0);
			wsBinding.OpenTimeout = new TimeSpan(0, 1, 0);
			wsBinding.ReceiveTimeout = new TimeSpan(0, 3, 0);
			wsBinding.SendTimeout = new TimeSpan(0, 1, 0);

			PatternsDataFeed.Host = new System.ServiceModel.Web.WebServiceHost(typeof(PatternsDataFeed), new Uri(httpAddress));
			PatternsDataFeed.Host.AddServiceEndpoint(typeof(ICurveDataFeed), wsBinding, httpAddress).Behaviors.Add(new System.ServiceModel.Description.WebHttpBehavior());
			PatternsDataFeed.Host.Open();
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

		/// <summary>
		/// Check if the client version meet the requested requirements
		/// </summary>
		/// <param name="version"></param>
		/// <returns>True if the version is valid</returns>
		static bool ValidatePatternsClientVersion(string version)
		{
			if (string.IsNullOrEmpty(PatternsServiceSettings.Instance.VersionPatterns))
				return true;

			if (string.IsNullOrEmpty(version))
				return false;

			// Easy comparison, we want the EXACT version!
            if (string.CompareOrdinal(version, PatternsServiceSettings.Instance.VersionPatterns) != 0)
			{
                Common.Source.TraceEvent(TraceEventType.Warning, 7400, "An obsolete Patterns client asked for patient data, request rejected!\nClient version: {0} Requested version: {1}", version, PatternsServiceSettings.Instance.VersionPatterns);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Check if the client version meet the requested requirements
		/// </summary>
		/// <param name="version"></param>
		/// <returns>True if the version is valid</returns>
		static bool ValidateCurveClientVersion(string version)
		{
            if (string.IsNullOrEmpty(PatternsServiceSettings.Instance.VersionCurve))
				return true;

			if (string.IsNullOrEmpty(version))
				return false;

			// Easy comparison, we want the EXACT version!
            if (string.CompareOrdinal(version, PatternsServiceSettings.Instance.VersionCurve) != 0)
			{
                Common.Source.TraceEvent(TraceEventType.Warning, 7405, "An obsolete Curve client asked for patient data, request rejected!\nClient version: {0} Requested version: {1}", version, PatternsServiceSettings.Instance.VersionCurve);
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
            if (PatternsServiceSettings.Instance.DisableAdminAPILocalIPValidation)
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

				Common.Source.TraceEvent(TraceEventType.Warning, 7410, "Someone tried to use an admin API of the web service from a remote computer {0}. Request is rejected.", clientAddress);
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 7415, "Error while validating the IP address of the client of an admin API.\n{0}", e);
			}
			return false;
		}

		#region Interface Members

		/// <summary>
		/// Get patient data an return it into an xml that the ActiveX can read
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public XElement GetPatternsData(XElement param)
		{
			// Security
			if (param == null)
				return null;

			// Validate patterns enabled
			if (! PatternsServiceSettings.Instance.PatternsEnabled)
				return null;

			var watch = Stopwatch.StartNew();
			bool full_request = false;

			// Prepare the response
			var root = new XElement("data", new XAttribute("server_time", DateTime.UtcNow.ToEpoch().ToString(CultureInfo.InvariantCulture)));
			if (PeriGen.Patterns.Service.Common.DemoMode)
			{
				root.SetAttributeValue("demo_mode", 1);
			}

			var user = param.Attribute("user") != null ? param.Attribute("user").Value : "n/a";

			var version = param.Attribute("version");
			if ((version != null) && (!ValidatePatternsClientVersion(version.Value)))
			{
				root.SetAttributeValue("invalid_version", 1);
			}
			else
			{
				foreach (var node in param.Descendants("request"))
				{
					// Read the header
					var header = new PatternsRequestHeader(node);
					XElement element = null;

					// PODI not connected?
					if (PeriGen.Patterns.Service.Common.Chalkboard.IsOffline)
					{
						element = new XPatient
										{
											Status = StatusType.Error,
											StatusDetails = "External system cannot be reached."
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
							lock (PeriGen.Patterns.Service.Common.Chalkboard.EpisodesLockObject)
							{
								episode = (from ep in PeriGen.Patterns.Service.Common.Chalkboard.Episodes
										   where ep.PatientUniqueId == unique_id
										   select ep).FirstOrDefault();
							}
						}

						// Got an episode? Make sure it's not closed
						if ((episode != null) && (!episode.IsOpen))
						{
							episode = null;
						}

						if (episode == null)
						{
							// Make sure to indicate this is a full refresh
							header.Reset();
						}
					}

					// Not episode yet?
					if (episode == null)
					{
						// Revert to the Key search
						lock (PeriGen.Patterns.Service.Common.Chalkboard.EpisodesLockObject)
						{
							episode = (from ep in PeriGen.Patterns.Service.Common.Chalkboard.Episodes
									   where (string.CompareOrdinal(ep.Key, header.Key) == 0) && ep.IsOpen
									   select ep).FirstOrDefault();
						}
					}

					// Still no episode?
					if (episode == null)
					{
						element = new XPatient
										{
											Status = StatusType.Invalid,
											StatusDetails = "No data is available for the selected patient."
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
						// If we reach that point, then we have an episode that is open and we can return useful informations to the ActiveX
						header.Id = episode.PatientUniqueId.ToString(CultureInfo.InvariantCulture);
						header.Key = episode.Key;

						// Prepare the response for that patient
						element = episode.EncodeForRequest();
					}

					////////////////////////////////////////////////////////////////////////////////////
					//// TRACINGS 
					DateTime firstTracing = DateTime.MinValue;
					{
						long lastTracingId = long.Parse(header.LastTracing ?? "-1", CultureInfo.InvariantCulture);

						List<PeriGen.Patterns.Engine.Data.TracingBlock> tracings = null;
						lock (episode.TracingBlocks)
						{
							if (lastTracingId > -1)
							{
								// If it is a partial update, then get all new tracings since the last one returned
								tracings = (from t in episode.TracingBlocks where t.Id > lastTracingId orderby t.Id select t).ToList();
							}
							else
							{
								full_request = true;

								// If it is a complete refresh, then get all the tracings but for a maximum duration
								if (episode.TracingBlocks.Count > 0)
								{
                                    var timeLimit = episode.TracingBlocks.Max(b => b.End).AddDays(-PatternsServiceSettings.Instance.MaxDaysOfTracingReturnedToActiveX);
									tracings = (from t in episode.TracingBlocks where t.End >= timeLimit orderby t.Id select t).ToList();

									// User activity log...
									Common.Source.TraceEvent(TraceEventType.Verbose, 7420, "User '{0}' view pattern's tracing for episode ({1})", user, episode);

									// PerformanceCounters 
									PerformanceCounterHelper.AddPatientsOpened(1);
								}
							}
						}

						if (tracings != null && tracings.Count > 0)
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

						var actions = new List<XUserAction>();
						lock (episode.TracingActions)
						{
							actions = (from a in episode.TracingActions where a.Id > lastActionId orderby a.Id select a).ToList();
						}

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

						lock (episode.TracingArtifacts)
						{
                            if (lastArtifactId > -1)
							{
								// If it is a partial update, then get all new artifacts since the last one returned
                                artifacts = (from a in episode.TracingArtifacts where a.Id > lastArtifactId orderby a.Id select a).ToList();
							}
                            else 
                            {
                                string traceStr = String.Format("Start get First Arrifacts for ID = {0}", header.Id);
                                // If it is a complete refresh, then get all the artifacts for the returned tracings
                                long resetTime;
                                var resetTimeStr = element.Attribute("reset").Value;
                                Int64.TryParse(resetTimeStr, out resetTime);
                                DateTime absoluteStart = resetTime.ToDateTime();
                                DateTime artifactsStartTime = (absoluteStart < firstTracing) ? absoluteStart : firstTracing;
                                artifacts = (from a in episode.TracingArtifacts where a.StartTime >= artifactsStartTime orderby a.Id select a).ToList();
                                //artifacts = (from a in episode.TracingArtifacts where a.StartTime >= absoluteStart orderby a.Id select a).ToList();
                                //artifacts = (from a in episode.TracingArtifacts where a.StartTime >= firstTracing orderby a.Id select a).ToList();
                            }
                   
						}

						if (artifacts.Count > 0)
						{
							element.Add(DataEncoder.EncodeForActiveX(artifacts));
							header.LastArtifact = artifacts.Last().Id.ToString(CultureInfo.InvariantCulture);
						}
					}

                    ////////////////////////////////////////////////////////////////////////////////////
                    //// HEADER (to remember current request, used for next incremental updates
                    element.Add(header.SerializeXml());
                    root.Add(element);
                }
			}

			// Trace
            if (PatternsServiceSettings.Instance.TraceExtendedPatternsData)
			{
				Common.Source.TraceEvent(TraceEventType.Verbose, 7425, "GetPatternsData ({0} ms)\n-- From:\n{1}\n-- Request:\n{2}\n-- Answer:\n{3}",
					watch.ElapsedMilliseconds,
					((System.ServiceModel.Channels.RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name]).Address,
					param,
					full_request ? "Full refresh, too big for dump!" : root.ToString());
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
				lock (PeriGen.Patterns.Service.Common.Chalkboard.EpisodesLockObject)
				{
					episode = (from ep in PeriGen.Patterns.Service.Common.Chalkboard.Episodes
							   where ep.PatientUniqueId == patient_id
							   select ep).FirstOrDefault();
				}

				if (episode != null)
				{
					episode.AddActions(patientActions);

					// User activity log...
					Common.Source.TraceEvent(TraceEventType.Verbose, 7430, "Patterns user actions '{0}' for episode ({1})", param, episode);

					// Performance counter
					PerformanceCounterHelper.AddUserActions(patientActions.Count());
				}
			}
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
			lock (PeriGen.Patterns.Service.Common.Chalkboard.EpisodesLockObject)
			{
				if (!PeriGen.Patterns.Service.Common.Chalkboard.IsOffline)
				{
					episodes = PeriGen.Patterns.Service.Common.Chalkboard.Episodes
									.Where(e => e.EpisodeStatus != PatternsEpisode.EpisodeStatuses.New && e.EpisodeStatus != PatternsEpisode.EpisodeStatuses.Closed && e.PatientUniqueId > 0)
									.OrderBy(e => e.EpisodeStatus)
									.ThenBy(e => e.UnitName)
									.ThenBy(e => e.BedName)
									.ThenBy(e => e.BedId)
									.ThenBy(e => e.PatientUniqueId)
									.ToList();
				}
			}

			var sb = new StringBuilder();
			using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true }))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("Episodes");
				writer.WriteAttributeString("LastRefreshed", PeriGen.Patterns.Service.Common.Chalkboard.LastRefreshed.ToString("s", CultureInfo.InvariantCulture));
				writer.WriteAttributeString("IsOffline", PeriGen.Patterns.Service.Common.Chalkboard.IsOffline.ToString());

				if (episodes != null)
				{
					foreach (var episode in episodes)
					{
						writer.WriteStartElement("Episode");
						writer.WriteAttributeString("PatientUniqueId", episode.PatientUniqueId.ToString(CultureInfo.InvariantCulture));
						writer.WriteAttributeString("DatabaseFile", episode.DatabaseFile ?? string.Empty);
						writer.WriteAttributeString("EpisodeStatus", episode.EpisodeStatus.ToString());
						writer.WriteAttributeString("RecoveryInProgress", episode.RecoveryInProgress.ToString(CultureInfo.InvariantCulture));
						writer.WriteAttributeString("Created", episode.Created.ToString("s", CultureInfo.InvariantCulture));
						writer.WriteAttributeString("LastUpdated", episode.LastUpdated.ToString("s", CultureInfo.InvariantCulture));
						writer.WriteAttributeString("LastMonitored", episode.LastMonitored.ToString("s", CultureInfo.InvariantCulture));
						writer.WriteAttributeString("RealTimeLatency", episode.RealTimeLatency.ToString(CultureInfo.InvariantCulture));
						episode.WritePatientDataToXml(writer);
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
				Common.Source.TraceEvent(TraceEventType.Error, 7435, "Unable to close a patient, the given parameter is not an integer.\n{0}", id);
				return;
			}

			if (unique_id <= 0)
			{
				Common.Source.TraceEvent(TraceEventType.Warning, 7440, "Unable to close a patient, the given parameter is not a valid database id.\n{0}", unique_id);
				return;
			}

			lock (PeriGen.Patterns.Service.Common.Chalkboard.EpisodesLockObject)
			{
				var episode = (from ep in PeriGen.Patterns.Service.Common.Chalkboard.Episodes
							   where ep.PatientUniqueId == unique_id
							   select ep).FirstOrDefault();

				if (episode == null)
				{
					Common.Source.TraceEvent(TraceEventType.Warning, 7445, "Unable to close a patient, the given id is not a valid episode.\n{0}", unique_id);
					return;
				}

				if (episode.EpisodeStatus == PatternsEpisode.EpisodeStatuses.Closed)
				{
					Common.Source.TraceEvent(TraceEventType.Warning, 7450, "Unable to close a patient, it is already closed.\n{0}", unique_id);
					return;
				}

				episode.UpdateStatus(PatternsEpisode.EpisodeStatuses.Closed, "Closing episode as per requested by user");
				Common.Source.TraceEvent(TraceEventType.Information, 7455, "Closing episode ({0}) as per requested by user.", episode);
			}
		}

		/// <summary>
		/// Return the patient's curve data
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public XElement GetCurveData(XElement param)
		{
			// Security
			if (param == null)
				return null;

			// Validate curve enabled
			if (!PatternsServiceSettings.Instance.CurveEnabled)
				return null;

			var watch = Stopwatch.StartNew();

			// Prepare the response
			var root = new XElement("data", new XAttribute("server_time", DateTime.UtcNow.ToEpoch().ToString(CultureInfo.InvariantCulture)));
			if (PeriGen.Patterns.Service.Common.DemoMode)
			{
				root.SetAttributeValue("demo_mode", 1);
			}

			var user = param.Attribute("user") != null ? param.Attribute("user").Value : "n/a";

			var version = param.Attribute("version");
			if ((version != null) && (!ValidateCurveClientVersion(version.Value)))
			{
				root.SetAttributeValue("invalid_version", 1);
			}
			else
			{
				foreach (var node in param.Descendants("request"))
				{
					// Read the header
					var header = new CurveRequestHeader(node);
					XElement element = null;

					// PODI not connected?
					if (PeriGen.Patterns.Service.Common.Chalkboard.IsOffline)
					{
						element = new XPatient
						{
							Status = StatusType.Error,
							StatusDetails = "External system cannot be reached."
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
							lock (PeriGen.Patterns.Service.Common.Chalkboard.EpisodesLockObject)
							{
								episode = (from ep in PeriGen.Patterns.Service.Common.Chalkboard.Episodes
										   where ep.PatientUniqueId == unique_id
										   select ep).FirstOrDefault();
							}
						}

						// Got an episode? Make sure it's not closed
						if ((episode != null) && (!episode.IsOpen))
						{
							episode = null;
						}

						if (episode == null)
						{
							// Make sure to indicate this is a full refresh
							header.Reset();
						}
					}

					// Not episode yet?
					if (episode == null)
					{
						// Revert to the Key search
						lock (PeriGen.Patterns.Service.Common.Chalkboard.EpisodesLockObject)
						{
							episode = (from ep in PeriGen.Patterns.Service.Common.Chalkboard.Episodes
									   where (string.CompareOrdinal(ep.Key, header.Key) == 0) && ep.IsOpen
									   select ep).FirstOrDefault();
						}
					}

					// Still no episode?
					if (episode == null)
					{
						element = new XPatient
						{
							Status = StatusType.Invalid,
							StatusDetails = "No data is available for the selected patient."
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
						// If we reach that point, then we have an episode that is open and we can return useful informations to the ActiveX
						header.Id = episode.PatientUniqueId.ToString(CultureInfo.InvariantCulture);
						header.Key = episode.Key;

						// Prepare the response for that patient
						element = episode.EncodeForRequest();

						/// CURVE
						{
							int snapshotId;
							if (string.IsNullOrWhiteSpace(header.Snapshot) || (!int.TryParse(header.Snapshot, NumberStyles.Integer, CultureInfo.InvariantCulture, out snapshotId)))
							{
								snapshotId = -1;
							}

							int lastSnapshotId;
							if (string.IsNullOrWhiteSpace(header.LastSnapshot) || (!int.TryParse(header.LastSnapshot, NumberStyles.Integer, CultureInfo.InvariantCulture, out lastSnapshotId)))
							{
								lastSnapshotId = -1;
							}

							DataContextEpisode.CurveSnapshot snapshot;
							lock (episode.CurveSnapshots)
							{
								// Specific snapshot requested?
								if (snapshotId >= 0)
								{
									snapshot = episode.CurveSnapshots.FirstOrDefault(item => item.SnapshotId == snapshotId);
								}

								// Most recent snapshot
								else
								{
									snapshot = episode.CurveSnapshots.Where(item => item.SnapshotId >= lastSnapshotId).OrderBy(item => item.SnapshotId).LastOrDefault();
								}
							}

							// No snapshot found?
							if (snapshot == null)
							{
								header.LastSnapshot = "-1";
							}

							// Already sent?
							else if (snapshot.SnapshotId == lastSnapshotId)
							{
								// Nothing to do there...
							}

							// New snapshot or first sent
							else
							{
								// Specific snapshot asked...
								if (snapshotId != -1)
								{
									// User activity log...
									Common.Source.TraceEvent(TraceEventType.Verbose, 7460, "User '{0}' view a review curve ({1:s}) for episode ({2})", user, snapshot.UpdateTime.ToDateTime(), episode);
								}

								// Latest snapshot asked for the first time...
								else if (lastSnapshotId == -1)
								{
									// User activity log...
									Common.Source.TraceEvent(TraceEventType.Verbose, 7465, "User '{0}' view the curve for episode ({1})", user, episode);

									// PerformanceCounters 
									PerformanceCounterHelper.AddPatientsOpened(1);
								}

								header.LastSnapshot = snapshot.SnapshotId.ToString(CultureInfo.InvariantCulture);
								element.Add(
										new XElement("snapshot",
													new XAttribute("id", snapshot.SnapshotId.ToString(CultureInfo.InvariantCulture)),
													new XAttribute("update", snapshot.UpdateTime.ToDateTime().ToString("s")),
													XElement.Parse(snapshot.Data)));
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
            if (PatternsServiceSettings.Instance.TraceExtendedCurveData)
			{
				Common.Source.TraceEvent(TraceEventType.Verbose, 7470, "GetCurveData ({0} ms)\n-- From:\n{1}\n-- Request:\n{2}\n-- Answer:\n{3}",
					watch.ElapsedMilliseconds,
					((System.ServiceModel.Channels.RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name]).Address,
					param,
					root.ToString());
			}

			return root;
		}

		static DateTime AlignOnBlockTime(DateTime time, int blockDuration)
		{
			return new DateTime(((time.Ticks / (TimeSpan.TicksPerMinute * blockDuration)) + 1) * (TimeSpan.TicksPerMinute * blockDuration));
		}

		/// <summary>
		/// Returns decision support information
		/// Right now, it only returns the contraction counts...
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public XElement GetDecisionSupportInformation(XElement param)
		{
			// Duration of a block. Defaulted to 10 minutes.
			int _ctr_cnt_block_duration = 10;

			// Contraction detection is always a little bit late which mean that a block that just ended cannot be send right away since the last contraction of that block may not be calculated yet. This adds a 3 minutes delay before sending the contraction count for the most recent block
			int _ctr_cnt_block_realtime_offset = 3;

			// Percentage of the block duration (_ctr_cnt_block_duration) with UP tracing necessary to send the block. Any block with less up than that is not send.
			int _ctr_cnt_block_minimum_up = 90;

			// Security
			if (param == null)
				return null;

			var watch = Stopwatch.StartNew();
			var visitsCount = 0;

			// Prepare the response
			var answer = new XElement("data", new XAttribute("server_time", DateTime.UtcNow.ToEpoch().ToString(CultureInfo.InvariantCulture)));
			if (PeriGen.Patterns.Service.Common.Chalkboard.IsOffline)
			{
				answer.SetAttributeValue("offline", "1");
				return answer;
			}

			foreach (var node in param.Descendants("visit"))
			{
				// Find the requested episode of tracings
				PatternsEpisode episode = null;

				if (node.Attribute("key") == null)
				{
					Debug.Assert(false);
					continue;
				}
				
				var visit_key = node.Attribute("key").Value;

				// Revert to the Key search
				lock (PeriGen.Patterns.Service.Common.Chalkboard.EpisodesLockObject)
				{
					episode = (from ep in PeriGen.Patterns.Service.Common.Chalkboard.Episodes where (string.CompareOrdinal(ep.Key, visit_key) == 0) && ep.IsOpen select ep).FirstOrDefault();
				}

				if (episode == null)
				{
					continue;
				}

				var visit_node = new XElement("visit", new XAttribute("key", episode.Key), new XAttribute("id", episode.PatientUniqueId));
				answer.Add(visit_node);
				++visitsCount;

				// Handle the contraction counts
				var ctr_cnt_request = node.Element("ctr_cnt");
				if (ctr_cnt_request != null)
				{
					var ctr_answer = new XElement(ctr_cnt_request);

					// Safety in case the caller did not clean previous data sent back under the "ctr_cnt" node
					ctr_answer.RemoveNodes();
					visit_node.Add(ctr_answer);

					// Recovery or not saved yet? Not sending anything yet...
					if (episode.RecoveryInProgress || (episode.PatientUniqueId <= 0))
					{
						continue;
					}

					var value = ctr_answer.AttributeToInteger("block_duration");
					if (value.HasValue)
					{
						_ctr_cnt_block_duration = value.Value;
					}
					value = ctr_answer.AttributeToInteger("block_minimum_up");
					if (value.HasValue)
					{
						_ctr_cnt_block_minimum_up = value.Value;
					}

					DateTime first_tracings;
					DateTime last_tracings;

					lock (episode.TracingBlocks)
					{
						first_tracings = episode.TracingBlocks.Count == 0 ? DateTime.MinValue : episode.TracingBlocks.Min(b => b.Start);
						last_tracings = episode.TracingBlocks.Count == 0 ? DateTime.MinValue : episode.TracingBlocks.Max(b => b.End);
					}

					// Visit with tracings only...
					if (first_tracings != DateTime.MinValue)
					{
						long ticks;

						var last_update = DateTime.MinValue;
						if ((ctr_answer.Attribute("last_update") != null) && (long.TryParse(ctr_answer.Attribute("last_update").Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ticks)))
							last_update = new DateTime(ticks);

						var last_sent = DateTime.MinValue;
						if ((ctr_answer.Attribute("last_sent") != null) && (long.TryParse(ctr_answer.Attribute("last_sent").Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ticks)))
							last_sent = new DateTime(ticks);

						// Build the list of time block to send
						var listOfTime = new List<DateTime>();

						// ...with modified contractions...
						if (last_sent != DateTime.MinValue)
						{
							// Get the list of strikeout-contraction actions that where not undo later on
							List<XUserAction> actions;
							lock (episode.TracingActions)
							{
								actions = episode.TracingActions.Where(ua => (ua.ActionType == ActionTypes.StrikeoutContraction || ua.ActionType == ActionTypes.UndoStrikeoutContraction) && (ua.PerformedTime > last_update)).ToList();
							}
							if (actions.Count > 0)
							{
								last_update = actions.Max(ua => ua.PerformedTime);
							}

							lock (episode.TracingArtifacts)
							{
								listOfTime.AddRange(actions
                                        .Select(ua => episode.TracingArtifacts.FirstOrDefault(a => (a.Id == ua.ArtifactId) && a.IsContraction))
										.Where(a => a != null)
										.Select(a => (a as PeriGen.Patterns.Engine.Data.Contraction).PeakTime));
							}
						}
						else
						{
							lock (episode.TracingActions)
							{
								if (episode.TracingActions.Count > 0)
								{
									last_update = episode.TracingActions.Max(ua => ua.PerformedTime);
								}
							}
						}

						// ...and with time passing...
						DateTime start_block = first_tracings;
						if (last_sent > first_tracings)
						{
							start_block = last_sent;
						}

						var now = DateTime.UtcNow;
						while (start_block < now)
						{
							listOfTime.Add(start_block);
							start_block = start_block.AddMinutes(_ctr_cnt_block_duration);
						}

						// Remove block that is too close to present time
						var limit = now.AddMinutes(-_ctr_cnt_block_realtime_offset);

						// Align the times on the block start...
						listOfTime = listOfTime.Select(dt => AlignOnBlockTime(dt, _ctr_cnt_block_duration)).Distinct().Where(dt => dt <= limit).ToList();

						// Now, for all these times, count the contractions...
						var contractions = episode.ActiveContractions;
						var blocks = listOfTime.ToDictionary(dt => dt, dt => contractions.Count(c => (c.PeakTime < dt) && (c.PeakTime >= dt.AddMinutes(-_ctr_cnt_block_duration))));

						if (blocks.Count > 0)
						{
							// From these blocks, remove those with too little tracings...
							List<PeriGen.Patterns.Engine.Data.TracingBlock> tracings;
							lock (episode.TracingBlocks)
							{
								var start = blocks.Min(b => b.Key).AddMinutes(-_ctr_cnt_block_duration);
								var end = blocks.Max(b => b.Key);

								tracings = PeriGen.Patterns.Engine.Data.TracingBlock.Merge(episode.TracingBlocks.Where(t => (t.Start < end) && (t.End > start)), _ctr_cnt_block_duration * 5).ToList();
							}

							var latestBlock = blocks.Max(b => b.Key);
							if (latestBlock > last_sent)
								last_sent = latestBlock;

							ctr_answer.SetAttributeValue("last_update", last_update.Ticks);
							ctr_answer.SetAttributeValue("last_sent", last_sent.Ticks);

							foreach (var kv in blocks)
							{
								var start = kv.Key.AddMinutes(-_ctr_cnt_block_duration);
								var end = kv.Key;

								var tracing = tracings.FirstOrDefault(t => (t.Start <= start) && (t.End >= start));
								if (tracing != null)
								{
									// How many seconds of real UP tracings in that block between start
									int seconds = 0;
									if (_ctr_cnt_block_minimum_up >= 0)
									{
										var itr = tracing.GetUPEnumerator(start, end);
										while (itr.MoveNext())
										{
											if (!PeriGen.Patterns.Engine.Data.TracingBlock.IsNoDataUP(itr.Current))
											{
												++seconds;
											}
										}
									}
									if (100 * seconds >= _ctr_cnt_block_minimum_up * _ctr_cnt_block_duration * 60)
									{
										// Ok, enough tracings...
										ctr_answer.Add(new XElement("ctr_block", new XAttribute("time", kv.Key.ToString("s")), new XAttribute("value", kv.Value)));
									}
								}
							}
						}
					}
				}
			}

			// Trace!
            if (PatternsServiceSettings.Instance.TraceExtendedPatternsData)
			{
				Common.Source.TraceEvent(TraceEventType.Verbose, 7475, "GetDecisionSupportInformation in {0} ms for {1} visit(s)\nparam:\n{2}\nanswer:\n{3}", watch.ElapsedMilliseconds, visitsCount, param.ToString(SaveOptions.None), answer.ToString(SaveOptions.None));
			}

			return answer;
		}

		/// <summary>
		/// Update some Curve related patient's data
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public XElement UpdateFields(XElement param)
		{
			var xml = UTF8Encoding.UTF8.GetString(Convert.FromBase64String(System.Web.HttpUtility.UrlDecode(param.Value)));
			XElement dataEdited = XElement.Parse(xml);

			Common.Source.TraceEvent(TraceEventType.Verbose, 7480, "Curve UpdateFields command: " + xml);

			var userName = dataEdited.Attribute("userName").Value;
			var userId = dataEdited.Attribute("userId").Value;
			var updateTime = DateTime.ParseExact(dataEdited.Attribute("updateTime").Value, "s", CultureInfo.InvariantCulture);

			// Get data from parameters
			var request = dataEdited.Element("request");
			var examEdited = dataEdited.Element("exam");
			var fieldsEdited = dataEdited.Element("fields");

			// Read the header
			var header = new CurveRequestHeader(request);

			// Find the requested episode of tracings
			PatternsEpisode episode = null;

			// If there is a unique ID in the header, use if and make sure it's still a good one
			int unique_id;
			if ((int.TryParse(header.Id, out unique_id)) && (unique_id > 0))
			{
				lock (PeriGen.Patterns.Service.Common.Chalkboard.EpisodesLockObject)
				{
					episode = (from ep in PeriGen.Patterns.Service.Common.Chalkboard.Episodes
							   where ep.PatientUniqueId == unique_id
							   select ep).FirstOrDefault();
				}
			}

			//do something if episode exists
			if (episode != null)
			{
				// Check first exam edited
				if (examEdited != null)
				{
					// Set First Exam
					episode.AddDataEntry(
							new List<DataContextEpisode.DataEntry>() 
								{ 
									new DataContextEpisode.DataEntry 
									{ 
										Name=PatternsEpisode.FirstExamDatabaseKeyName,
										Value=examEdited.Attribute("dateTime").Value,
										UserId=userId,
										UserName=userName,
										UpdateTime= updateTime.ToEpoch()
									} 
								});
				}

				//check fields edited
				if (fieldsEdited != null)
				{
					//Fields edited
					var edited = dataEdited.Descendants("field");
					List<DataContextEpisode.DataEntry> lde = new List<DataContextEpisode.DataEntry>();
					foreach (XElement item in edited)
					{
						//field
						var name = item.Attribute("name").Value;
						var value = item.Attribute("value").Value;

						//add new data entry to list
						lde.Add(
							new DataContextEpisode.DataEntry()
							{
								Name = name,
								Value = string.IsNullOrEmpty(value) ? "" : value,
								UserId = userId,
								UserName = userName,
								UpdateTime = updateTime.ToEpoch()
							}
							);
					}
					//add list to episode if there are some values
					if (lde.Count > 0) episode.AddDataEntry(lde);

				}

				// Process data right away...
				episode.ProcessCurve();

			}

			/// remove data from edited XML
			if (examEdited != null) dataEdited.Element("exam").Remove();
			if (fieldsEdited != null) dataEdited.Element("fields").Remove();

			/// return updated data with request information provided in xml
			return GetCurveData(dataEdited);

		}

		/// <summary>
		/// Returns list of snapshots for an episode
		/// </summary>
		/// <returns></returns>
		public XElement GetSnapshotsList(XElement param)
		{
			var watch = Stopwatch.StartNew();

			//No request , no data
			if (param == null) return null;

			// Prepare the response
			var root = new XElement("data", new XAttribute("server_time", DateTime.UtcNow.ToEpoch().ToString(CultureInfo.InvariantCulture)));
			if (PeriGen.Patterns.Service.Common.DemoMode)
			{
				root.SetAttributeValue("demo_mode", 1);
			}

			var version = param.Attribute("version");
			if ((version != null) && (!ValidateCurveClientVersion(version.Value)))
			{
				root.SetAttributeValue("invalid_version", 1);
			}
			else
			{
				foreach (var node in param.Descendants("request"))
				{
					// Read the header
					var header = new CurveRequestHeader(node);
					XElement element = null;

					// PODI not connected?
					if (PeriGen.Patterns.Service.Common.Chalkboard.IsOffline)
					{
						element = new XPatient
						{
							Status = StatusType.Error,
							StatusDetails = "External system cannot be reached."
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
							lock (PeriGen.Patterns.Service.Common.Chalkboard.EpisodesLockObject)
							{
								episode = (from ep in PeriGen.Patterns.Service.Common.Chalkboard.Episodes
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

						// Revert to the Key search
						lock (PeriGen.Patterns.Service.Common.Chalkboard.EpisodesLockObject)
						{
							episode = (from ep in PeriGen.Patterns.Service.Common.Chalkboard.Episodes
									   where (string.CompareOrdinal(ep.Key, header.Key) == 0) && ep.IsOpen
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
						// If we reach that point, then we have an episode that is open and we can return useful informations to the ActiveX
						header.Id = episode.PatientUniqueId.ToString(CultureInfo.InvariantCulture);
						header.Key = episode.Key;

						// Prepare the response for that patient
						element = episode.EncodeForRequest();

						/// CURVE Snapshots
						{

							lock (episode.CurveSnapshots)
							{
								//write xml with snapshots
								StringBuilder sb = new StringBuilder();
								XmlWriterSettings xs = new XmlWriterSettings() { OmitXmlDeclaration = true };
								XmlWriter writer = XmlWriter.Create(sb);
								writer.WriteStartDocument();
								writer.WriteStartElement("snapshots");

								//get list of snapshots from current episode
								var list = episode.CurveSnapshots.OrderByDescending(item => item.SnapshotId);
								foreach (var item in list)
								{
									writer.WriteStartElement("snapshot");
									writer.WriteAttributeString("id", item.SnapshotId.ToString(CultureInfo.InvariantCulture));
									writer.WriteAttributeString("updateTime", item.UpdateTime.ToDateTime().ToString("s"));
									writer.WriteEndElement();
								}

								writer.WriteEndElement();
								writer.WriteEndDocument();
								writer.Flush();
								writer.Close();
								writer = null;

								//add snapshots to return data
								element.Add(XElement.Parse(sb.ToString()));
							}
						}
					}

					////////////////////////////////////////////////////////////////////////////////////
					/// HEADER (to remember current request, used for next incremental updates
					element.Add(header.SerializeXml());
					root.Add(element);
				}
			}

			return root;

		}

        public XElement GetArtifactByID(XElement param)
        {
            var actions = param.DescendantsAndSelf("action").Select(a => XUserAction.FromActiveX(a)).ToList();
            var validActions = from c in actions
                         where c.ActionType != ActionTypes.None && c.ActionType != ActionTypes.ConfirmEvent
                         select c;

            if (validActions == null || validActions.Count() <= 0)
                return null;

            var action = validActions.First();

            var toRet = new XElement("data", new XAttribute("server_time", DateTime.UtcNow.ToEpoch().ToString(CultureInfo.InvariantCulture)));

            // Find the requested episode of tracings
            PatternsEpisode episode = null;
            lock (PeriGen.Patterns.Service.Common.Chalkboard.EpisodesLockObject)
            {
                episode = (from ep in PeriGen.Patterns.Service.Common.Chalkboard.Episodes
                           where ep.PatientUniqueId == action.PatientId
                           select ep).FirstOrDefault();
            }

            DetectionArtifact artifact = null;
            if (episode != null)
            {
                artifact = (from c in episode.TracingArtifacts
                            where c.Id == action.ArtifactId
                            select c).First();

                if (artifact == null)
                    return null;
            }

            var element = episode.EncodeForRequest();
            var artifactsNode = new XElement("artifacts");
            artifactsNode.SetAttributeValue("basetime", artifact.StartTime.ToEpoch());
            artifactsNode.Add(DataEncoder.EncodeForActiveX(artifact, artifact.StartTime));
            element.Add(artifactsNode);
            toRet.Add(element);

            return toRet;
        }

        public XElement GetPatternsDataByPeriod(XElement param)
        {
            XElement element = null;
            var actions = param.DescendantsAndSelf("action").Select(a => XExtendedUserAction.FromActiveX(a)).ToList();
            var action = (from c in actions
                          where c.ActionType != ActionTypes.None && c.ActionType != ActionTypes.ConfirmEvent
                          select c).First();

            var toRet = new XElement("data", new XAttribute("server_time", DateTime.UtcNow.ToEpoch().ToString(CultureInfo.InvariantCulture)));

            // Find the requested episode of tracings
            PatternsEpisode episode = null;
            lock (PeriGen.Patterns.Service.Common.Chalkboard.EpisodesLockObject)
            {
                episode = (from ep in PeriGen.Patterns.Service.Common.Chalkboard.Episodes
                           where ep.PatientUniqueId == action.PatientId
                           select ep).FirstOrDefault();
                element = episode.EncodeForRequest();
           }

            if (episode != null)
            { 
                ////////////////////////
                ////    TRACINGS   /////
                ////////////////////////
                List<TracingBlock> tracings = null;
                tracings = (from c in episode.TracingBlocks
                            where c.End > action.StartPeriod && c.Start < action.EndPeriod
                            orderby c.Id
                            select c).ToList();

                if (tracings != null && tracings.Count > 0)
                    element.Add(DataEncoder.EncodeForActiveX(TracingBlock.Merge(tracings, 60)));

                ////////////////////////
                ////   ARTIFACTS   /////
                ////////////////////////
                List<DetectionArtifact> artifacts = null;
                artifacts = (from c in episode.TracingArtifacts
                             where c.EndTime > action.StartPeriod && c.StartTime < action.EndPeriod
                             orderby c.Id
                             select c).ToList();

                if (artifacts.Count() > 0)
                    element.Add(DataEncoder.EncodeForActiveX(artifacts));
            }

            XElement request = new XElement("request",
                                            new XAttribute("key", episode.Key),
				                            new XAttribute("id", episode.PatientUniqueId),
                                            new XAttribute("action", episode.TracingActions.Count));

            element.Add(request);
            toRet.Add(element);

            return toRet;
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
