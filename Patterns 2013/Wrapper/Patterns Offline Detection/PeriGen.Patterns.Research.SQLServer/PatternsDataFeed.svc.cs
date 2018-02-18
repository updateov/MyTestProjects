using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Globalization;
using PeriGen.Patterns.Research.SQLHelper;
using System.Xml.Linq;
using PeriGen.Patterns.ActiveXInterface;
using System.IO;

namespace PeriGen.Patterns.Research.SQLServer
{
	[ServiceBehavior(
		Name = "PatternsDataFeed",
		Namespace = "http://www.perigen.com/2011/09/patterns/",
		IncludeExceptionDetailInFaults = true,
		ConfigurationName = "PeriGen.Patterns.Research.SQLServer.PatternsDataFeed",
		InstanceContextMode = InstanceContextMode.Single,
		ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class PatternsDataFeed : IPatternsDataFeed
	{
		#region IPatternsDataFeed Members

		/// <summary>
		/// Return all available data for the given patient's
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public XElement GetPatientData(XElement param)
		{
			// Prepare the response
			var root = new XElement("data", new XAttribute("server_time", DateTime.UtcNow.ToEpoch().ToString(CultureInfo.InvariantCulture)));

			// The request may concern multiple patients
			using (PatternsDataContext db = new PatternsDataContext(Properties.Settings.Default.DatabaseConnectionString))
			{
				foreach (var request in param.Descendants("request"))
				{
					// Read requested key
					var key = request.Attribute("key").Value.ToUpperInvariant();
					var id = request.Attribute("id") == null ? -1 : Convert.ToInt32(request.Attribute("id").Value, CultureInfo.InvariantCulture);

					// Look for the requested patient
					Patient patient = null;
					if (id > 0)
					{
						patient = (from p in db.Patients where p.PatientId == id select p).FirstOrDefault();
					}
					else
					{
						patient = (from p in db.Patients where p.PatientKey == key select p).FirstOrDefault();
						if (patient != null)
						{
							request.SetAttributeValue("id", patient.PatientId);
						}
					}
					// Patient just not found...
					XElement patientElement = null;
					if (patient == null)
					{
						var p = new XPatient{Status = StatusType.Invalid, StatusDetails = "Unknown patient"};
						patientElement = p.EncodeForActiveX();
						root.Add(patientElement);
						continue;
					}

					patientElement = patient.EncodeForActiveX();
					root.Add(patientElement);

					// Read the request
					int lastTracing = request.Attribute("tracing") != null ? Convert.ToInt32(request.Attribute("tracing").Value, CultureInfo.InvariantCulture) : -1;
					int lastArtifact = request.Attribute("artifact") != null ? Convert.ToInt32(request.Attribute("artifact").Value, CultureInfo.InvariantCulture) : -1;
					int lastAction = request.Attribute("action") != null ? Convert.ToInt32(request.Attribute("action").Value, CultureInfo.InvariantCulture) : -1;

					// Get the data
					var tracings = (from t in patient.Tracings where t.TracingId > lastTracing orderby t.TracingId select t);
					if (tracings.Count() > 0)
					{
						patientElement.Add(Tracing.EncodeForActiveX(tracings));
						request.SetAttributeValue("tracing", tracings.Max(t => t.TracingId));
					}
					
					
					var artifacts = (from a in patient.Artifacts where a.ArtifactId > lastArtifact orderby a.ArtifactId select a);
					if (artifacts.Count() > 0)
					{
						patientElement.Add(Artifact.EncodeForActiveX(artifacts));
						request.SetAttributeValue("artifact", artifacts.Max(t => t.ArtifactId));
					}

					var actions = (from a in patient.UserActions where a.ActionId > lastAction orderby a.ActionId select a);
					if (actions.Count() > 0)
					{
						patientElement.Add(UserAction.EncodeForActiveX(actions));
						request.SetAttributeValue("action", actions.Max(t => t.ActionId));
					}

					/// Remember current request, used for next incremental updates for next calls
					patientElement.Add(request);
				}
			}

			return root;
		}

		/// <summary>
		/// Perform the requested user action
		/// </summary>
		/// <param name="param"></param>
		public void PerformUserAction(XElement param)
		{
			var actions = param.DescendantsAndSelf("action");
			using (PatternsDataContext db = new PatternsDataContext(Properties.Settings.Default.DatabaseConnectionString))
			{
				foreach (var element in actions)
				{
					UserAction action =
						new UserAction
							{
								ActionType = Convert.ToByte(element.Attribute("type")),
								ArtifactId = Convert.ToInt32(element.Attribute("artifact")),
								PatientId = Convert.ToInt32(element.Attribute("patient")),
								UserId = element.Attribute("userid").Value,
								UserName = element.Attribute("username").Value
							};
					db.UserActions.InsertOnSubmit(action);
				}
				db.SubmitChanges();
			}
		}

		/// <summary>
		/// Return the list of patients
		/// </summary>
		/// <returns></returns>
		public XElement GetPatientList()
		{
			using (PatternsDataContext db = new PatternsDataContext(Properties.Settings.Default.DatabaseConnectionString))
			{
				var node = new XElement("patients");
				var patients = db.Patients.OrderBy(p => p.PatientId).Take(100).ToList();
				foreach (var item in patients)
				{
					node.Add(item.EncodeForActiveX());
				}
				return node;
			}
		}

		/// <summary>
		/// Close the active patient matching the given id
		/// </summary>
		/// <param name="param"></param>
		public void ClosePatient(string id)
		{
			// Not applicable to that test app
			throw new NotImplementedException();
		}

		#endregion

	}
}
