using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace PeriGen.Patterns.ActiveXInterface
{
	/// <summary>
	/// Write the data as an XML file that can be read by the patterns viewer

	/// </summary>
	public static class DataEncoder
	{
		/// <summary>
		/// The reference date for the EPOCH Unix time (http://en.wikipedia.org/wiki/Unix_time) used by the ActiveX for date/time encoding
		/// </summary>
		static DateTime EpochReferenceDateTime = new DateTime(1970, 1, 1);

		/// <summary>
		/// Convert a double that represents a HR/UP value to a simple byte from 0 to 255
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static byte ToSignalByteValue(this double value)
		{
			return Convert.ToByte(Math.Min(255, Math.Max(0, value)), CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Convert a double that represents a percentage value (0.12) to a simple byte from 0 to 100
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static byte ToPercentageByteValue(this double value)
		{
			return Convert.ToByte(Math.Min(100, Math.Max(0, value * 100)), CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Convert an Epoch encoded datetime to a datetime
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static DateTime ToDateTime(this long date)
		{
			return EpochReferenceDateTime.AddSeconds(date);
		}

		/// <summary>
		/// Convert an Epoch encoded datetime to a datetime
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static System.Nullable<DateTime> ToDateTime(this System.Nullable<long> date)
		{
			if (date.HasValue)
			{
				return EpochReferenceDateTime.AddSeconds(date.Value);
			}
			return null;
		}

		/// <summary>
		/// Convert a datetime to the Epoch value
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static long ToEpoch(this DateTime date)
		{
			return (long)((date - EpochReferenceDateTime).TotalSeconds);
		}

		/// <summary>
		/// Convert a datetime to the Epoch value
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static System.Nullable<long> ToEpoch(this System.Nullable<DateTime> date)
		{
			if (date.HasValue)
			{
				return (long)((date.Value - EpochReferenceDateTime).TotalSeconds);
			}

			return null;
		}

		/// <summary>
		/// Convert an Epoch encoded datetime to a datetime
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		static int ToOffset4Hz(this DateTime date, DateTime basetime)
		{
			return (int)((date - basetime).TotalSeconds * 4);
		}

		/// <summary>
		/// Convert an Epoch encoded datetime to a datetime
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		static int ToOffset1Hz(this DateTime date, DateTime basetime)
		{
			return (int)((date - basetime).TotalSeconds);
		}

		/// <summary>
		/// Convert an Epoch encoded datetime to a datetime
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static DateTime RoundToTheSecond(this DateTime date)
		{
			return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
		}

		/// <summary>
		/// Serialize the given tracings
		/// </summary>
		public static XElement EncodeForActiveX(IEnumerable<PeriGen.Patterns.Engine.Data.TracingBlock> tracings)
		{
			var element = new XElement("tracings");

			if (tracings != null)
			{
				foreach (var tracing in tracings)
				{
					element.Add(tracing.EncodeForActiveX());
				}
			}
			return element;
		}

		/// <summary>
		/// Encode a Patterns tracing block into an XElement that can be sent to the patterns activex
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public static XElement EncodeForActiveX(this PeriGen.Patterns.Engine.Data.TracingBlock block)
		{
			if (block == null)
				throw new ArgumentNullException("block");

			return new XElement("tracing",
				new XAttribute("start", block.Start.ToEpoch().ToString(CultureInfo.InvariantCulture)),
				new XAttribute("hr1", block.HRs != null ? Convert.ToBase64String(block.HRs.ToArray(), 0, block.HRs.Count) : string.Empty),
				new XAttribute("up", (block.UPs != null) ? Convert.ToBase64String(block.UPs.ToArray(), 0, block.UPs.Count) : string.Empty));
		}

		/// <summary>
		/// Serialize the given tracings
		/// </summary>
		/// <param name="artifacts"></param>
		/// <returns></returns>
		public static XElement EncodeForActiveX(IEnumerable<PeriGen.Patterns.Engine.Data.DetectionArtifact> artifacts)
		{
			var element = new XElement("artifacts");

			if (artifacts != null)
			{
				var basetime = artifacts.Min(a => a.StartTime);
				element.SetAttributeValue("basetime", basetime.ToEpoch());

				foreach (var artifact in artifacts)
				{
					element.Add(artifact.EncodeForActiveX(basetime));
				}
			}
			return element;
		}

		/// <summary>
		/// Encode a Patterns artifact into an XElement that can be sent to the patterns activex
		/// </summary>
		/// <param name="artifact"></param>
		/// <param name="basetime"></param>
		/// <returns></returns>
		public static XElement EncodeForActiveX(this PeriGen.Patterns.Engine.Data.DetectionArtifact artifact, DateTime basetime)
		{
			if (artifact == null)
				throw new ArgumentNullException("artifact");

			// Contractions
			if (artifact.IsContraction)
			{
				return new XElement("artifact", new XAttribute("data", EncodeForActiveX(artifact as PeriGen.Patterns.Engine.Data.Contraction, basetime)));
			}

			// Baselines
			if (artifact.IsBaseline)
			{
				return new XElement("artifact", new XAttribute("data", EncodeForActiveX(artifact as PeriGen.Patterns.Engine.Data.Baseline, basetime)));
			}

			// Accelerations
			if (artifact.IsAcceleration)
			{
				return new XElement("artifact", new XAttribute("data", EncodeForActiveX(artifact as PeriGen.Patterns.Engine.Data.Acceleration, basetime)));
			}

			// Deceleration
			if (artifact.IsDeceleration)
			{
				return new XElement("artifact", new XAttribute("data", EncodeForActiveX(artifact as PeriGen.Patterns.Engine.Data.Deceleration, basetime)));
			}

			// Unknown type
			throw new InvalidOperationException("Type of detection artifact unknown to the patterns engine mapping data structure");
		}

		/// <summary>
		/// Encode a contraction artifact into an XElement that can be sent to the patterns activex
		/// </summary>
		/// <param name="artifact"></param>
		/// <param name="basetime"></param>
		/// <returns></returns>
		static string EncodeForActiveX(PeriGen.Patterns.Engine.Data.Contraction artifact, DateTime basetime)
		{
			return
				string.Format(
					CultureInfo.InvariantCulture,
					"CTR|{0}|{1}|{2}|y|{3}|{4}",
					artifact.StartTime.ToOffset1Hz(basetime),
					artifact.PeakTime.ToOffset1Hz(basetime),
					artifact.EndTime.ToOffset1Hz(basetime),
                    artifact.IsStrikedOut ? "y" : "n",
					artifact.Id);
		}

		/// <summary>
		/// Encode a baseline artifact into an XElement that can be sent to the patterns activex
		/// </summary>
		/// <param name="artifact"></param>
		/// <param name="basetime"></param>
		/// <returns></returns>
		static string EncodeForActiveX(PeriGen.Patterns.Engine.Data.Baseline artifact, DateTime basetime)
		{
			return
				string.Format(
					CultureInfo.InvariantCulture,
					"EVT|9|{0}||{1}|{2:0.0}|{3:0.0}||y|||||{4:0.0}||||||||{5}",
					artifact.StartTime.ToOffset4Hz(basetime),
					artifact.EndTime.ToOffset4Hz(basetime),
					artifact.Y1,
					artifact.Y2,
					artifact.BaselineVariability,
					artifact.Id);
		}

		/// <summary>
		/// Encode an acceleration artifact into an XElement that can be sent to the patterns activex
		/// </summary>
		/// <param name="artifact"></param>
		/// <param name="basetime"></param>
		/// <returns></returns>
		static string EncodeForActiveX(PeriGen.Patterns.Engine.Data.Acceleration artifact, DateTime basetime)
		{
			return
				string.Format(
					CultureInfo.InvariantCulture,
					"EVT|1|{0}|{1}|{2}||||y|{3}|{4:0.000}|{5:0.000}|{6:0.000}||{7:0.0}|||||{8}||{9}",
					artifact.StartTime.ToOffset4Hz(basetime),
					artifact.PeakTime.ToOffset4Hz(basetime),
					artifact.EndTime.ToOffset4Hz(basetime),
                    artifact.IsStrikedOut ? "y" : "n",
                    artifact.Confidence,
					artifact.Repair,
					artifact.Height,
					artifact.PeakValue,
					artifact.IsNonInterpretable ? "y" : string.Empty,
					artifact.Id);
		}

		/// <summary>
		/// Encode a deceleration artifact into an XElement that can be sent to the patterns activex
		/// </summary>
		/// <param name="artifact"></param>
		/// <param name="basetime"></param>
		/// <returns></returns>
		static string EncodeForActiveX(PeriGen.Patterns.Engine.Data.Deceleration artifact, DateTime basetime)
		{
			int eventType = 0;

			if (artifact.IsNonAssociatedDeceleration)
			{
				eventType = 7; // event::tnadeceleration ***
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

			return
				string.Format(
					CultureInfo.InvariantCulture,
					"EVT|{0}|{1}|{2}|{3}|||{4}|y|{5}|{6:0.000}|{7:0.000}|{8:0.0}||{9:0.0}||{10}|-1|{11}|{12}||{13}",
					eventType,
					artifact.StartTime.ToOffset4Hz(basetime),
					artifact.PeakTime.ToOffset4Hz(basetime),
					artifact.EndTime.ToOffset4Hz(basetime),
					artifact.ContractionStart.HasValue ? artifact.StartTime.ToOffset4Hz(basetime).ToString(CultureInfo.InvariantCulture) : string.Empty,
                    artifact.IsStrikedOut ? "y" : "n",
                    artifact.Confidence,
					artifact.Repair,
					artifact.Height,
					artifact.PeakValue,
					artifact.IsVariableDeceleration ? "y" : string.Empty,
					atypicalValue,
					artifact.IsNonInterpretable ? "y" : string.Empty,
					artifact.Id);
		}
	}
}