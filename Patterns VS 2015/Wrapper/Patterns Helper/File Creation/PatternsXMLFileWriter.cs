using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;
using PeriGen.Patterns.Engine.Data;
using System.Diagnostics;
using System.Reflection;

namespace PeriGen.Patterns.Helper
{
	public static class PatternsXMLFileWriter
	{
		/// <summary>
		/// Current version of the export tool
		/// </summary>
		static string BackupVersion = "1.0";

		/// <summary>
		/// Current version of the engine tool
		/// </summary>
		static string PatternVersion = "Unknown";

		/// <summary>
		/// Static contructor to initialize static variables
		/// </summary>
		static PatternsXMLFileWriter()
		{
			try
			{
				var info = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
				PatternVersion = info.FileVersion + "-" + info.Comments;
			}
			catch (Exception)
			{
				PatternVersion = "Unknown";
			}
		}

		/// <summary>
		/// Create the proper xml document
		/// </summary>
		/// <param name="patient_id"></param>
		/// <param name="blocks"></param>
		/// <param name="results"></param>
		/// <returns></returns>
		public static XDocument Write(string patient_id, 
										string lastname,
										string firstname,
										string configuration,
										DateTime? edd,
										byte? fetusCount,
										IEnumerable<TracingBlock> blocks, 
										IEnumerable<DetectionArtifact> results)
		{
			var xmlDoc = new XDocument();

			var mainNode = new XElement("patternsarchive",
								new XAttribute("patternengine", PatternVersion),
								new XAttribute("backupengine", BackupVersion),
								new XAttribute("configuration", Convert.ToBase64String(Encoding.UTF8.GetBytes(configuration))));
			xmlDoc.Add(mainNode);

			var visitNode = new XElement("visit",
								new XAttribute("dischargetime", string.Empty),
								new XAttribute("archivetime", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
								new XAttribute("dob", string.Empty),
								new XAttribute("age", string.Empty),
								new XAttribute("edd", edd.HasValue?edd.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture):string.Empty),
								new XAttribute("ga", string.Empty),
								new XAttribute("fetuscount", fetusCount.HasValue?fetusCount.Value.ToString(CultureInfo.InvariantCulture):string.Empty));
			mainNode.Add(visitNode);

			var patientNode = new XElement("patient",
								new XAttribute("patientid", patient_id),
								new XAttribute("accountno", "n/a"),
								new XAttribute("lastname", lastname),
								new XAttribute("firstname", firstname));
			visitNode.Add(patientNode);

			var dataNode = new XElement("data");
			visitNode.Add(dataNode);

			var tracingsNode = new XElement("tracings");

			var firstBlock = blocks.FirstOrDefault();
			DateTime absoluteStartTime = (firstBlock == null) ? DateTime.MinValue : firstBlock.Start;

			tracingsNode.SetAttributeValue("starttime", absoluteStartTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));

			XElement tracingNode;

			// Dump UP
			tracingNode = new XElement("tracing", new XAttribute("type", "up"));

			foreach (var b in blocks)
			{
				var segmentNode = new XElement("segment",
									new XAttribute("start", (b.Start - absoluteStartTime).TotalSeconds.ToString(CultureInfo.InvariantCulture)),
									new XAttribute("compress", false.ToString(CultureInfo.InvariantCulture)),
									new XAttribute("data", Convert.ToBase64String(b.UPs.ToArray(), 0, b.UPs.Count)));
				tracingNode.Add(segmentNode);
			}
			tracingsNode.Add(tracingNode);

			// Dump HR1
			tracingNode = new XElement("tracing", new XAttribute("type", "fhr1"));

			foreach (var b in blocks)
			{
				var segmentNode = new XElement("segment",
										new XAttribute("start", ((b.Start - absoluteStartTime).TotalSeconds * 4).ToString(CultureInfo.InvariantCulture)),
										new XAttribute("compress", false.ToString(CultureInfo.InvariantCulture)),
										new XAttribute("data", Convert.ToBase64String(b.HRs.ToArray(), 0, b.HRs.Count)));
				tracingNode.Add(segmentNode);
			}
			tracingsNode.Add(tracingNode);
			dataNode.Add(tracingsNode);
			
			///////////////
			StringBuilder patternsData = new StringBuilder(10240);
			foreach (var result in results)
			{
				if (patternsData.Length > 0)
				{
					patternsData.Append("\r\n");
				}
				patternsData.Append(ToArchiveCompatibleString(result, absoluteStartTime));
			}

			// Convert to base 64 to ensure it's utf8 compliant and also hardly readable
			var patternsNode = new XElement("patterns", 
									new XAttribute("compress", false.ToString(CultureInfo.InvariantCulture)),
									new XAttribute("data", Convert.ToBase64String(Encoding.UTF8.GetBytes(patternsData.ToString()))));

			dataNode.Add(patternsNode);

			return xmlDoc;
		}

		/// <summary>
		/// Encode a Patterns artifact into an XElement that can be sent to the patterns activex
		/// </summary>
		static string ToArchiveCompatibleString(PeriGen.Patterns.Engine.Data.DetectionArtifact artifact, DateTime basetime)
		{
			if (artifact == null)
				throw new ArgumentNullException("artifact");

			// Contractions
			if (artifact.IsContraction)
			{
				return ToArchiveCompatibleString(artifact as PeriGen.Patterns.Engine.Data.Contraction, basetime);
			}

			// Baselines
			if (artifact.IsBaseline)
			{
				return ToArchiveCompatibleString(artifact as PeriGen.Patterns.Engine.Data.Baseline, basetime);
			}

			// Accelerations
			if (artifact.IsAcceleration)
			{
				return ToArchiveCompatibleString(artifact as PeriGen.Patterns.Engine.Data.Acceleration, basetime);
			}

			// Deceleration
			if (artifact.IsDeceleration)
			{
				return ToArchiveCompatibleString(artifact as PeriGen.Patterns.Engine.Data.Deceleration, basetime);
			}

			// Unknown type
			throw new InvalidOperationException("Type of detection artifact unknown to the patterns engine mapping data structure");
		}

		/// <summary>
		/// Encode the artifact as a contraction
		/// </summary>
		/// <param name="artifact"></param>
		/// <param name="baseTime"></param>
		/// <returns></returns>
		static string ToArchiveCompatibleString(PeriGen.Patterns.Engine.Data.Contraction artifact, DateTime basetime)
		{
			StringBuilder value = new StringBuilder(255);

			value.Append("CTR|");
			value.Append(((int)((artifact.StartTime - basetime).TotalSeconds)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			value.Append(((int)((artifact.PeakTime - basetime).TotalSeconds)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			value.Append(((int)((artifact.EndTime - basetime).TotalSeconds)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			value.Append("y"); // Is final
			value.Append("|");
			value.Append("n"); // Is Strikeout
			value.Append("|");
			value.Append(artifact.Id.ToString(CultureInfo.InvariantCulture));

			return value.ToString();
		}

		/// <summary>
		/// Encode the artifact as a baseline
		/// </summary>
		/// <returns></returns>
		static string ToArchiveCompatibleString(PeriGen.Patterns.Engine.Data.Baseline artifact, DateTime basetime)
		{
			StringBuilder value = new StringBuilder(255);

			value.Append("EVT|");

			/* 00 */
			value.Append("9");  // event::tbaseline
			value.Append("|");
			/* 01 */
			value.Append(((int)((artifact.StartTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 02 */
			value.Append(string.Empty); // Peak time
			value.Append("|");
			/* 03 */
			value.Append(((int)((artifact.EndTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 04 */
			value.Append(artifact.Y1.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 05 */
			value.Append(artifact.Y2.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 06 */
			value.Append(string.Empty); // Contraction start
			value.Append("|");
			/* 07 */
			value.Append("y"); // Final
			value.Append("|");
			/* 08 */
			value.Append(string.Empty); // Strikeout
			value.Append("|");
			/* 09 */
			value.Append(string.Empty); // Confidence
			value.Append("|");
			/* 10 */
			value.Append(string.Empty); // Repair
			value.Append("|");
			/* 11 */
			value.Append(string.Empty); // Height
			value.Append("|");
			/* 12 */
			value.Append(artifact.BaselineVariability.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 13 */
			value.Append(string.Empty); // Peak value
			value.Append("|");
			/* 14 */
			value.Append(string.Empty); // Non reassuring features ?
			value.Append("|");
			/* 15 */
			value.Append(string.Empty); // Variable decel
			value.Append("|");
			/* 16 */
			value.Append(string.Empty); // Lag
			value.Append("|");
			/* 17 */
			value.Append(string.Empty); // Non reassuring features
			value.Append("|");
			/* 18 */
			value.Append(string.Empty); // Non Interpretable
			value.Append("|");
			/* 19 */
			value.Append(string.Empty); // Confirmed
			value.Append("|");
			/* 20 */
			value.Append(artifact.Id.ToString(CultureInfo.InvariantCulture));

			return value.ToString();
		}

		/// <summary>
		/// Encode the artifact as an acceleration
		/// </summary>
		/// <returns></returns>
		static string ToArchiveCompatibleString(PeriGen.Patterns.Engine.Data.Acceleration artifact, DateTime basetime)
		{
			StringBuilder value = new StringBuilder(255);

			value.Append("EVT|");

			/* 00 */
			value.Append("1"); // event::tacceleration
			value.Append("|");
			/* 01 */
			value.Append(((int)((artifact.StartTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 02 */
			value.Append(((int)((artifact.PeakTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 03 */
			value.Append(((int)((artifact.EndTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 04 */
			value.Append(string.Empty); // Y1
			value.Append("|");
			/* 05 */
			value.Append(string.Empty); // Y2
			value.Append("|");
			/* 06 */
			value.Append(string.Empty); // Contraction start
			value.Append("|");
			/* 07 */
			value.Append("y"); // Final
			value.Append("|");
			/* 08 */
			value.Append(string.Empty); // Strikeout
			value.Append("|");
			/* 09 */
			value.Append(artifact.Confidence.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 10 */
			value.Append(artifact.Repair.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 11 */
			value.Append(artifact.Height.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 12 */
			value.Append(string.Empty); // Baseline variability
			value.Append("|");
			/* 13 */
			value.Append(artifact.PeakValue.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 14 */
			value.Append(string.Empty); // Non reassuring features ?
			value.Append("|");
			/* 15 */
			value.Append(string.Empty); // Variable decel
			value.Append("|");
			/* 16 */
			value.Append(string.Empty); // Lag
			value.Append("|");
			/* 17 */
			value.Append(string.Empty); // Non reassuring features
			value.Append("|");
			/* 18 */
			if (artifact.IsNonInterpretable) value.Append("y");
			value.Append("|");
			/* 19 */
			value.Append(string.Empty); // Confirmed
			value.Append("|");
			/* 20 */
			value.Append(artifact.Id.ToString(CultureInfo.InvariantCulture));

			return value.ToString();
		}

		/// <summary>
		/// Encode the artifact as a deceleration
		/// </summary>
		/// <returns></returns>
		static string ToArchiveCompatibleString(PeriGen.Patterns.Engine.Data.Deceleration artifact, DateTime basetime)
		{
			int eventType = 0;

			if (artifact.IsNonAssociatedDeceleration)
			{
				eventType = 7; // event::tnadeceleration  ***
			}
			else if (artifact.IsEarlyDeceleration)
			{
				eventType = 3; // event::tearly ***
			}
			else if (artifact.IsLateDeceleration)
			{
				eventType = 6; // event::tlate ***	
			}
			else if (artifact.IsVariableDeceleration)
			{
				if (artifact.HasProlongedNonReassuringFeature)
				{
					eventType = 14; // event::tprolonged ***
				}
				else if (artifact.HasNonReassuringFeature)
				{
					eventType = 5; // event::tatypical ***
				}
				else
				{
					eventType = 4; // event::ttypical ***
				}
			}
			else
			{
				throw new InvalidOperationException("Unable to match the deceleration type to an engine type");
			}

			int atypicalValue = 0;
			if (artifact.HasBiphasicNonReassuringFeature)
			{
				atypicalValue |= 1;
			}
			if (artifact.HasLossRiseNonReassuringFeature)
			{
				atypicalValue |= 2;
			}
			if (artifact.HasLossVariabilityNonReassuringFeature)
			{
				atypicalValue |= 4;
			}
			if (artifact.HasLowerBaselineNonReassuringFeature)
			{
				atypicalValue |= 8;
			}
			if (artifact.HasProlongedSecondRiseNonReassuringFeature)
			{
				atypicalValue |= 16;
			}
			if (artifact.HasSixtiesNonReassuringFeature)
			{
				atypicalValue |= 32;
			}
			if (artifact.HasSlowReturnNonReassuringFeature)
			{
				atypicalValue |= 64;
			}

			StringBuilder value = new StringBuilder(255);

			value.Append("EVT|");

			/* 00 */
			value.Append(eventType.ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 01 */
			value.Append(((int)((artifact.StartTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 02 */
			value.Append(((int)((artifact.PeakTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 03 */
			value.Append(((int)((artifact.EndTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 04 */
			value.Append(string.Empty); // Y1
			value.Append("|");
			/* 05 */
			value.Append(string.Empty); // Y2
			value.Append("|");
			/* 06 */
			value.Append(artifact.ContractionStart.HasValue ? ((int)((artifact.ContractionStart.Value - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture) : string.Empty);
			value.Append("|");
			/* 07 */
			value.Append("y"); // Final
			value.Append("|");
			/* 08 */
			value.Append(string.Empty); // Strikeout
			value.Append("|");
			/* 09 */
			value.Append(artifact.Confidence.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 10 */
			value.Append(artifact.Repair.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 11 */
			value.Append(artifact.Height.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 12 */
			value.Append(string.Empty); // Baseline variability
			value.Append("|");
			/* 13 */
			value.Append(artifact.PeakValue.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 14 */
			value.Append("|");
			/* 15 */
			if ((PeriGen.Patterns.Engine.Data.DecelerationCategories)artifact.DecelerationCategory == PeriGen.Patterns.Engine.Data.DecelerationCategories.Variable) value.Append("y");
			value.Append("|");
			/* 16 */
			value.Append("-1"); // Lag
			value.Append("|");
			/* 17 */
			value.Append(atypicalValue.ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 18 */
			if (artifact.IsNonInterpretable) value.Append("y");
			value.Append("|");
			/* 19 */
			value.Append(string.Empty); // Confirmed
			value.Append("|");
			/* 20 */
			value.Append(artifact.Id.ToString(CultureInfo.InvariantCulture));

			return value.ToString();
		}
	}
}
