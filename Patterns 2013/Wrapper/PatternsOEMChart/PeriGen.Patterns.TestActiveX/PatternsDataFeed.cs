using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Xml.Linq;
using PeriGen.Patterns.ActiveXInterface;

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
		#region WCF host

		/// <summary>
		/// The WCF service host
		/// </summary>
		static ServiceHost Host { get; set; }

		/// <summary>
		/// Check if host is started
		/// </summary>
		public static bool IsStarted { get { return Host != null; } }

		/// <summary>
		/// Start the WCF host
		/// </summary>
		public static void StartHost()
		{
			Repository.Source.TraceEvent(TraceEventType.Information, 9431, "Starting Data WCF host");

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
			Repository.Source.TraceEvent(TraceEventType.Information, 9432, "Stopping Data WCF host");

			if (Host != null)
			{
				Host.Close(TimeSpan.FromSeconds(5));
				Host = null;
			}
		}

		#endregion

		#region IPatternsDataFeed Members

		/// <summary>
		/// Get some data
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public XElement GetPatternsData(XElement param)
		{
			// Prepare the response
			var root = new XElement("data", new XAttribute("server_time", DateTime.UtcNow.ToEpoch().ToString(CultureInfo.InvariantCulture)));

			foreach (var node in param.Descendants("request"))
			{
				lock (Repository.LockObject)
				{
					Repository.Source.TraceEvent(TraceEventType.Verbose, 9433, "Requesting patient {0}", node.ToString());

					if (Repository.DemoMode)
					{
						root.SetAttributeValue("demo_mode", 1);
					}

					XElement patientElement = null;

					// Read the header
					PatternsRequestHeader header = new PatternsRequestHeader(node);
					if (string.IsNullOrEmpty(header.Key))
					{
						// No key in the header!!
						header.Reset();
						patientElement = new XPatient { Status = StatusType.Error, StatusDetails = "The data request is invalid and the server cannot process it" }.EncodeForActiveX();
						patientElement.Add(header.SerializeXml()); 
						root.Add(patientElement);

						continue;
					}

					// Prepare the response for that patient
					patientElement = Repository.Patient.EncodeForActiveX();

					// If there is an id, make sure it the one that match the current repository id
					if ((!string.IsNullOrEmpty(header.Id)) && (Repository.Patient.PatientId != int.Parse(header.Id)))
					{
						Repository.Source.TraceEvent(TraceEventType.Verbose, 9434, "Requesting a complete refresh of the patient's data");

						// Reset all data and header ids
						header.Reset();
					}

					// Updating the ID & MRN
					header.Id = Repository.Patient.PatientId.ToString();
					header.Key = Repository.Patient.MRN;

					////////////////////////////////////////////////////////////////////////////////////
					//// TRACINGS 
					{
						var tracings = (from t in Repository.TracingBlocks where t.Id > long.Parse(header.LastTracing ?? "-1") orderby t.Id select t).ToList();
						if (tracings.Count() > 0)
						{
							patientElement.Add(DataEncoder.EncodeForActiveX(PeriGen.Patterns.Engine.Data.TracingBlock.Merge(tracings, 60)));
							header.LastTracing = tracings.Max(t => t.Id).ToString(CultureInfo.InvariantCulture);
						}
					}

					////////////////////////////////////////////////////////////////////////////////////
					//// USER ACTIONS 
					{
						var actions = (from a in Repository.TracingActions where a.Id > int.Parse(header.LastAction ?? "-1") orderby a.Id select a).ToList();
						if (actions.Count() > 0)
						{
							patientElement.Add(XUserAction.EncodeForActiveX(actions));
							header.LastAction = actions.Max(a => a.Id).ToString(CultureInfo.InvariantCulture);
						}
					}

					////////////////////////////////////////////////////////////////////////////////////
					//// ARTIFACTS
					{
						var artifacts = (from a in Repository.TracingArtifacts where a.Id > int.Parse(header.LastArtifact ?? "-1") orderby a.Id select a).ToList();
						if (artifacts.Count() > 0)
						{
							patientElement.Add(DataEncoder.EncodeForActiveX(artifacts));
							header.LastArtifact = artifacts.Max(a => a.Id).ToString(CultureInfo.InvariantCulture);
						}
					}

					////////////////////////////////////////////////////////////////////////////////////
					/// HEADER (to remember current request, used for next incremental updates
					patientElement.Add(header.SerializeXml());
					root.Add(patientElement);
				}
			}

			Repository.Source.TraceEvent(TraceEventType.Verbose, 9438, "Returning data:\n{0}", root.ToString());

			return root;
		}

		/// <summary>
		/// User action!
		/// </summary>
		/// <param name="param"></param>
		public void PerformUserAction(XElement param)
		{
			Repository.Source.TraceEvent(TraceEventType.Verbose, 9435, "Performing some user actions ({0})", param.DescendantsAndSelf("actions").Count());

			// The request may concern multiple actions
			Repository.AddActions(param.DescendantsAndSelf("action").Select(a => XUserAction.FromActiveX(a)).ToList());
		}

		/// <summary>
		/// Return list of patients
		/// In the case of the test application, that is not relevant
		/// </summary>
		/// <returns></returns>
		public XElement GetPatientList()
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Close the active patient matching the given id
		/// </summary>
		/// <param name="param"></param>
		public void ClosePatient(string id) 
		{
			throw new NotImplementedException();
		}
        public XElement GetPlugins()
        {
            throw new NotImplementedException();
        }
		#endregion

    }
}
