using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using PeriGen.Patterns.WebServiceManagement.Factory;
using PeriGen.Patterns.Engine.Data;
using PeriGen.Patterns.ActiveXInterface;
using System.Text;
using System.Xml;

namespace PeriGen.Patterns.Service
{
	public class PatternsChalkboard : IDisposable
	{
		#region Members and Properties

		/// <summary>
		/// The data sent to PODI as a request
		/// </summary>
		XElement PODIData { get; set; }

		/// <summary>
		/// For thread safety
		/// </summary>
		public object EpisodesLockObject { get; private set; }

		/// <summary>
		/// Date and time of the last refresh
		/// </summary>
		public DateTime LastRefreshed { get; private set; }

		volatile bool _IsOffline = true;

		/// <summary>
		/// Offline if unable to get data updates from the CIS
		/// </summary>
		public bool IsOffline
		{
			get { return this._IsOffline; }
			private set { this._IsOffline = value; }
		}

		/// <summary>
		/// List of episodes managed within the chalkboard
		/// </summary>
		public List<PatternsEpisode> Episodes
		{
			get;
			private set;
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Destructor
		/// </summary>
		~PatternsChalkboard()
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
					foreach (var e in this.Episodes)
					{
						e.Dispose();
					}
					this.Episodes.Clear();
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
		/// Simple constructor
		/// </summary>
		public PatternsChalkboard()
		{
			this.IsOffline = true;
			this.EpisodesLockObject = new object();
			this.Episodes = new List<PatternsEpisode>();
		}

		#region PODI data updates

		/// <summary>
		/// Build the initial PODI request based on the currently loaded episodes
		/// </summary>
		void BuildInitialPodiRequest()
		{
			var sb = new StringBuilder();
			using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true }))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("visits");

				lock (this.EpisodesLockObject)
				{
					foreach (var episode in this.Episodes)
					{
						writer.WriteStartElement("visit");
						writer.WriteAttributeString("key", episode.Key ?? string.Empty);
						writer.WriteRaw(episode.Context ?? string.Empty);
						writer.WriteEndElement();
					}
				}

				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
			}
			this.PODIData = XElement.Parse(sb.ToString());
		}

		/// <summary>
		/// Get the data from PODI
		/// The result is put in the variable PODIData if success
		/// </summary>
		/// <returns>True if data properly retrieved</returns>
		bool QueryPODI()
		{
			try
			{
				// First call? Build the initial PODIData request
				if (this.PODIData == null)
				{
					this.BuildInitialPodiRequest();
					Debug.Assert(this.PODIData != null);
				}
				// Not first call? Remove all closed entries from PODI
				else
				{
					this.PODIData.Descendants("visit").Where(visit => this.FindEpisode(visit.Attribute("key").Value) == null).ToList().ForEach(visit => visit.Remove());
				}

				// Prepare Prepare the data to send
				XElement result = null;

				try
				{
					// Prepare service
                    var args = new ServiceProxyArgs { Address = PatternsServiceSettings.Instance.PODILinkURL, CertificateHash = PatternsServiceSettings.Instance.PODICertificateHash };

					// Execute query
					ServiceProxy<IPeriGenOutbound>.Execute(args, gs => result = gs.GetData(this.PODIData));
				}
				catch (Exception e)
				{
					// Exception while calling PODI
					Common.Source.TraceEvent(TraceEventType.Verbose, 6400, "Unable to retrieve the data from PODI, error:\n{0}", e);
					return false;
				}

				if (result == null)
				{
					// No answer
					Common.Source.TraceEvent(TraceEventType.Verbose, 6405, "Unable to retrieve the data from PODI, no answer");
					return false;
				}

				if (result.DescendantsAndSelf("error").Count() > 0)
				{
					// PODI returned an error
					Common.Source.TraceEvent(TraceEventType.Verbose, 6410, "PODI returned an error code:\n{0}", result.ToString(SaveOptions.None));
					return false;
				}

				// Good answer!
				this.PODIData = result;
				return true;
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 6415, "Error while querying PODI.\n{0}", e);
				throw;
			}
		}

		/// <summary>
		/// Update known patient with PODI data
		/// </summary>
		/// <param name="visit"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		void UpdateEpisode(XElement visit)
		{
			try
			{
				var key = visit.Attribute("key").Value;
			 
				var status = visit.Attribute("status").Value.ToPODIVisitStatus();

				// Look for the episode (excluding closed ones)
				var episode = this.FindEpisode(key);

				// Not found?
				if (episode == null)
				{
					episode = new PatternsEpisode 
								{ 
									Key = key, 
									EpisodeStatus = PatternsEpisode.EpisodeStatuses.New, 
									RecoveryStatus = PatternsEpisode.RecoveryStatuses.Curve | PatternsEpisode.RecoveryStatuses.Patterns | PatternsEpisode.RecoveryStatuses.Podi,
									LastMonitored = DateTime.UtcNow,
								};

					lock (this.EpisodesLockObject)
					{
						this.Episodes.Add(episode);
					}

					Common.Source.TraceEvent(TraceEventType.Verbose, 6420, "New episode from PODI {0}", episode);

					// If the status sent back by PODI IS NOT for a new episode, then flush the context to make sure it will retrieve all data and not just an incremental
					if (status != PatternsEpisode.PODIVisitStatus.New)
					{
						visit.RemoveNodes();
						return;
					}
				}

				// No need to go further for a new episode
				if (status == PatternsEpisode.PODIVisitStatus.New)
				{
					return;
				}

				lock (episode.LockObject)
				{
					// Unknown?
					if (status == PatternsEpisode.PODIVisitStatus.Unknown)
					{
						// Flush the PODI recovery flag if it was applicable
						episode.RecoveryStatus &= ~PatternsEpisode.RecoveryStatuses.Podi;

						// The visit is not admitted anymore...
						episode.UpdateStatus(PatternsEpisode.EpisodeStatuses.Discharged, "CIS data update");
						return;
					}

					var response = visit.Element("response");
					if (response == null)
					{
						Debug.Assert(false);
						return;
					}

					// Partial?
					var partial = response.AttributeToBoolean("partial");

					// Patient
					var data = response.Element("data");
					if (data != null)
					{
						episode.IsLive = (data.AttributeToBoolean("live") ?? (Nullable<bool>)false).Value;

						episode.BedId = data.Attribute("bed_id").Value ?? string.Empty;
						episode.BedName = data.Attribute("bed_name").Value ?? string.Empty;
						episode.UnitName = data.Attribute("unit_name").Value ?? string.Empty;

						episode.MRN = data.Attribute("mrn").Value ?? string.Empty;
						episode.AccountNo = data.Attribute("account").Value ?? string.Empty;

						episode.FirstName = data.Attribute("first_name").Value ?? string.Empty;
						episode.LastName = data.Attribute("last_name").Value ?? string.Empty;

						episode.EDD = data.AttributeToDateTime("edd");
						episode.GA = data.Attribute("ga").Value ?? string.Empty;

						episode.Fetuses = data.AttributeToInteger("fetuses");
						episode.Parity = data.AttributeToInteger("parity");
						episode.Epidural = data.AttributeToDateTime("epidural");
						episode.VBAC = data.AttributeToBoolean("vbac_attempt");
						episode.PreviousVaginal = data.AttributeToBoolean("previous_vaginal");

						episode.MembraneStatus = data.Attribute("membrane_status").Value ?? string.Empty;
						episode.MembraneRupture = data.AttributeToDateTime("membrane_rupture");

						episode.LastMonitored = DateTime.UtcNow;
						episode.UpdateStatus(PatternsEpisode.EpisodeStatuses.Admitted, "CIS data update");
					}

					// Exams
					var exams = response.Element("exams");
					if (exams != null)
					{
						var list = new List<DataContextEpisode.PelvicExam>();
						foreach (var examElement in exams.Elements("exam"))
						{
							var time = examElement.AttributeToDateTime("time");
							if (time.HasValue)
							{
								list.Add(
									new DataContextEpisode.PelvicExam
									{
										Time = time.Value.ToEpoch(),

										Dilatation = examElement.Attribute("dilatation").Value,
										Effacement = examElement.Attribute("effacement").Value,
										Station = examElement.Attribute("station").Value,

										Presentation = examElement.Attribute("presentation").Value,
										Position = examElement.Attribute("position").Value,

										UpdateTime = DateTime.UtcNow.ToEpoch()
									});
							}
						}
						episode.AddPelvicExam(list);
					}

					// Tracings
					var tracings = response.Element("tracings");
					if (tracings != null)
					{
						var list = new List<TracingBlock>();
						foreach (XElement tracingElement in tracings.Elements("tracing"))
						{
							var start = tracingElement.AttributeToDateTime("start");
							if (start.HasValue)
							{
								var tb = new TracingBlock { Start = start.Value };
								tb.HRs.AddRange(Convert.FromBase64String(tracingElement.Attribute("hr1").Value));
								tb.UPs.AddRange(Convert.FromBase64String(tracingElement.Attribute("up").Value));
                                list.AddRange(tb.Split(PatternsServiceSettings.Instance.TracingSplitMaximumBlock));
							}
						}
						episode.AddTracings(list);
					}

					// Context 
					var context = visit.Element("context");
					episode.Context = context == null ? string.Empty : context.ToString(SaveOptions.DisableFormatting);

					// It the PODI did not sent all data for that visit then flag it in recovery or else flush the PODI recovery flag if it was set
					if (partial.HasValue && partial.Value)
					{
						episode.RecoveryStatus = PatternsEpisode.RecoveryStatuses.Podi | PatternsEpisode.RecoveryStatuses.Curve | PatternsEpisode.RecoveryStatuses.Patterns;
					}
					else
					{
						episode.RecoveryStatus &= ~PatternsEpisode.RecoveryStatuses.Podi;
					}
				}
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 6425, "Error while updating the episodes with the PODI data.\n{0}", e);
				throw;
			}
		}

		/// <summary>
		/// Find the episode with the given key
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		internal PatternsEpisode FindEpisode(string key)
		{
			lock (this.EpisodesLockObject)
			{
				return this.Episodes.FirstOrDefault(e => (e.EpisodeStatus != PatternsEpisode.EpisodeStatuses.Closed) && (string.CompareOrdinal(e.Key, key) == 0));
			}
		}

		#endregion

		/// <summary>
		/// Updates data from the external system
		/// </summary>
		/// <returns>True if successfully refreshed</returns>
		public bool Refresh()
		{
			try
			{
				var now = DateTime.UtcNow;

				// Get the PODI update
				if (!this.QueryPODI())
				{
					// If we can't refresh for too long, turn offline
                    if (!this.IsOffline && (Math.Abs((DateTime.UtcNow - this.LastRefreshed).TotalSeconds) > PatternsServiceSettings.Instance.MaximumDurationWithoutRefreshToGoOffline))
					{
						this.IsOffline = true;

						// Clear counters
						PerformanceCounterHelper.SetAverageLatencyLivePatients(0);
						PerformanceCounterHelper.SetWorstLatencyLivePatients(0);
						PerformanceCounterHelper.SetPatientsLive(0);
						PerformanceCounterHelper.SetPatientsOutOfMonitor(0);
						PerformanceCounterHelper.SetPatientsTotal(0);

						Common.Source.TraceEvent(TraceEventType.Warning, 6430, "Chalkboard cannot be updated for {0} seconds, going in Offline mode", (int)Math.Abs((DateTime.UtcNow - this.LastRefreshed).TotalSeconds));

						// Mark all admitted episodes in recovery
						lock (this.EpisodesLockObject)
						{
							foreach (var episode in this.Episodes)
							{
								if (episode.IsOpen)
								{
									episode.RecoveryStatus = PatternsEpisode.RecoveryStatuses.Podi | PatternsEpisode.RecoveryStatuses.Patterns | PatternsEpisode.RecoveryStatuses.Curve;
								}
							}
						}
					}
					return false;
				}

				lock (this.EpisodesLockObject)
				{
					// Update the episodes
					foreach (XElement patientElement in this.PODIData.Descendants("visit"))
					{
						this.UpdateEpisode(patientElement);
					}

					// Update performance Counters
					PerformanceCounterHelper.SetPatientsLive(Common.Chalkboard.Episodes.Count(e => e.EpisodeStatus == PatternsEpisode.EpisodeStatuses.Admitted && e.IsLive));
					PerformanceCounterHelper.SetPatientsOutOfMonitor(Common.Chalkboard.Episodes.Count(e => e.EpisodeStatus == PatternsEpisode.EpisodeStatuses.Admitted && !e.IsLive));
					PerformanceCounterHelper.SetPatientsTotal(Common.Chalkboard.Episodes.Count);

					this.LastRefreshed = now;
				}

				// Remove the response from previous call
				this.PODIData.XPathSelectElements("visit/response").ToList().ForEach(elt => elt.Remove());

				if (this.IsOffline)
				{
					this.IsOffline = false;
					Common.Source.TraceEvent(TraceEventType.Information, 6435, "Chalkboard is now online with {0} episodes", this.Episodes.Count);
				}

				// Check for error
				this.LastRefreshed = now;

				return true;
			}
			catch (Exception e)
			{
				Common.Source.TraceEvent(TraceEventType.Error, 6440, "Error while refreshing the chalkboard.\n{0}", e);
				throw;
			}
		}
	}
}
