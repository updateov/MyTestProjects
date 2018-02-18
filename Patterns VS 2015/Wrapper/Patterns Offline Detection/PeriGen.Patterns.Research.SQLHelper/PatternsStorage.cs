using System;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;
using System.Globalization;

using PeriGen.Patterns.ActiveXInterface;
using System.Collections.Generic;
using PeriGen.Patterns.Engine.Data;

namespace PeriGen.Patterns.Research.SQLHelper
{
	partial class Patient
	{
		/// <summary>
		/// Serialize the given user this
		/// </summary>
		/// <param name="element"></param>
		/// <param name="this"></param>
		public XElement EncodeForActiveX()
		{
			return new XElement("patient",
					new XAttribute("id", this.PatientId),
					new XAttribute("mrn", this.PatientKey),
					new XAttribute("status", this.Status),
					new XAttribute("statusdetails", string.Empty),
					new XAttribute("firstname", string.Empty),
					new XAttribute("lastname", string.Empty),
					new XAttribute("edd", "0"),
					new XAttribute("reset", "0"),
					new XAttribute("fetus", "1"));
		}
	}

	partial class UserAction
	{
		/// <summary>
		/// Serialize the given user this
		/// </summary>
		/// <param name="element"></param>
		/// <param name="this"></param>
		public XElement EncodeForActiveX()
		{
			return new XElement("action",
				new XAttribute("type", this.ActionType.ToString(CultureInfo.InvariantCulture)), 
				new XAttribute("artifact", this.ArtifactId.ToString(CultureInfo.InvariantCulture)),
				new XAttribute("userid", this.UserId),
				new XAttribute("username", this.UserName),
				new XAttribute("performed", this.PerformTime.Value.ToEpoch().ToString(CultureInfo.InvariantCulture)));
		}

		/// <summary>
		/// Serialize the given user actions
		/// </summary>
		public static XElement EncodeForActiveX(IEnumerable<UserAction> actions)
		{
			var element = new XElement("actions");
			if (actions != null)
			{
				foreach (var action in actions)
				{
					element.Add(action.EncodeForActiveX());
				}
			}
			return element;
		}
	}

	partial class Tracing
	{
		/// <summary>
		/// Contructor from a pattern's engine tracing block
		/// </summary>
		/// <param name="contraction"></param>
		public Tracing(TracingBlock block)
		{
			this.StartTime = block.Start;
			this.SignalHR1 = block.HRs.ToArray();
			this.SignalUP = block.UPs.ToArray();
		}

		/// <summary>
		/// Serialize the given tracing
		/// </summary>
		/// <param name="tracing"></param>
		/// <returns></returns>
		public XElement EncodeForActiveX()
		{
			return new XElement("tracing",
				new XAttribute("start", this.StartTime.ToEpoch().ToString(CultureInfo.InvariantCulture)),
				new XAttribute("hr1", this.SignalHR1 != null ? Convert.ToBase64String(this.SignalHR1.ToArray(), 0, this.SignalHR1.Length) : string.Empty),
				new XAttribute("up", (this.SignalUP != null) ? Convert.ToBase64String(this.SignalUP.ToArray(), 0, this.SignalUP.Length) : string.Empty));
		}

		/// <summary>
		/// Serialize the given tracings
		/// </summary>
		public static XElement EncodeForActiveX(IEnumerable<Tracing> tracings)
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
	}

	partial class Artifact
	{
		/// <summary>
		/// Is the artifact a contraction?
		/// </summary>
		public bool IsContraction { get { return this.Category == (byte)ArtifactCategories.Contraction; } }

		/// <summary>
		/// Is the artifact a baseline?
		/// </summary>
		public bool IsBaseline { get { return this.Category == (byte)ArtifactCategories.Baseline; } }

		/// <summary>
		/// Is the artifact an acceleration?
		/// </summary>
		public bool IsAcceleration { get { return this.Category == (byte)ArtifactCategories.Acceleration; } }

		/// <summary>
		/// Is the artifact a deceleration?
		/// </summary>
		public bool IsDeceleration { get { return this.Category == (byte)ArtifactCategories.Deceleration; } }

		/// <summary>
		/// Constructor of a sqlite mapping artifact from a pattern's engine artifact
		/// </summary>
		/// <param name="patternArtifact"></param>
		/// <returns></returns>
		public Artifact(DetectionArtifact patternArtifact)
		{
			if (patternArtifact.IsContraction)
			{
				Contraction contraction = patternArtifact as Contraction;

				Category = Convert.ToByte(contraction.Category, CultureInfo.InvariantCulture);

				StartTime = contraction.StartTime;
				EndTime = contraction.StartTime;

				PeakTime = contraction.PeakTime;
			}

			else if (patternArtifact.IsBaseline)
			{
				Baseline baseline = patternArtifact as Baseline;

				Category = Convert.ToByte(baseline.Category, CultureInfo.InvariantCulture);

				StartTime = baseline.StartTime;
				EndTime = baseline.EndTime;

				Y1 = baseline.Y1.ToSignalByteValue();
				Y2 = baseline.Y2.ToSignalByteValue();

				BaselineVariability = baseline.BaselineVariability.ToSignalByteValue();
			}

			else if (patternArtifact.IsAcceleration)
			{
				Acceleration acceleration = patternArtifact as Acceleration;

				Category = Convert.ToByte(acceleration.Category, CultureInfo.InvariantCulture);

				StartTime = acceleration.StartTime;
				EndTime = acceleration.EndTime;

				PeakTime = acceleration.PeakTime;
				PeakValue = acceleration.PeakValue.ToSignalByteValue();

				Confidence = acceleration.Confidence.ToPercentageByteValue();
				Repair = acceleration.Repair.ToPercentageByteValue();
				Height = acceleration.Height.ToSignalByteValue();

				IsNonInterpretable = acceleration.IsNonInterpretable;
			}

			else if (patternArtifact.IsDeceleration)
			{
				Deceleration deceleration = patternArtifact as Deceleration;

				Category = Convert.ToByte(deceleration.Category, CultureInfo.InvariantCulture);

				StartTime = deceleration.StartTime;
				EndTime = deceleration.EndTime;

				PeakTime = deceleration.PeakTime;
				PeakValue = deceleration.PeakValue.ToSignalByteValue();

				Confidence = deceleration.Confidence.ToPercentageByteValue();
				Repair = deceleration.Repair.ToPercentageByteValue();
				Height = deceleration.Height.ToSignalByteValue();

				IsNonInterpretable = deceleration.IsNonInterpretable;

				DecelerationCategory = Convert.ToByte(deceleration.DecelerationCategory, CultureInfo.InvariantCulture);
				NonReassuringFeatures = Convert.ToByte(deceleration.NonReassuringFeatures, CultureInfo.InvariantCulture);

				ContractionStart = deceleration.ContractionStart;
			}
			else
			{
				throw new InvalidOperationException("Type of detection artifact unknown to the sqlite mapping data structure");
			}
		}

		/// <summary>
		/// Serialize the given artifact
		/// </summary>
		public static XElement EncodeForActiveX(IEnumerable<Artifact> artifacts)
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
		/// Encode the artifact as a contraction
		/// </summary>
		/// <param name="basetime"></param>
		/// <returns></returns>
		protected string EncodeAsContraction(DateTime basetime)
		{
			Debug.Assert(this.Category == (int)ArtifactCategories.Contraction);

			StringBuilder value = new StringBuilder(255);

			value.Append("CTR|");
			value.Append(((int)((this.StartTime - basetime).TotalSeconds)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			value.Append(((int)((this.PeakTime.Value - basetime).TotalSeconds)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			value.Append(((int)((this.EndTime - basetime).TotalSeconds)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			value.Append("y"); // Is final
			value.Append("|");
			value.Append("n"); // Is Strikeout
			value.Append("|");
			value.Append(this.ArtifactId.ToString(CultureInfo.InvariantCulture));

			return value.ToString();
		}

		/// <summary>
		/// Encode the artifact as a baseline
		/// </summary>
		/// <param name="basetime"></param>
		/// <returns></returns>
		protected string EncodeAsBaseline(DateTime basetime)
		{
			Debug.Assert(this.Category == (int)ArtifactCategories.Baseline);

			StringBuilder value = new StringBuilder(255);

			value.Append("EVT|");

			/* 00 */
			value.Append("9");  // event::tbaseline
			value.Append("|");
			/* 01 */
			value.Append(((int)((this.StartTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 02 */
			value.Append(string.Empty); // Peak time
			value.Append("|");
			/* 03 */
			value.Append(((int)((this.EndTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 04 */
			value.Append(this.Y1.Value.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 05 */
			value.Append(this.Y2.Value.ToString("0.000000", CultureInfo.InvariantCulture));
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
			value.Append(this.BaselineVariability.Value.ToString("0.000000", CultureInfo.InvariantCulture));
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
			value.Append(this.ArtifactId.ToString(CultureInfo.InvariantCulture));

			return value.ToString();
		}

		/// <summary>
		/// Encode the artifact as an acceleration
		/// </summary>
		/// <param name="basetime"></param>
		/// <returns></returns>
		protected string EncodeAsAcceleration(DateTime basetime)
		{
			Debug.Assert(this.Category == (int)ArtifactCategories.Acceleration);

			StringBuilder value = new StringBuilder(255);

			value.Append("EVT|");

			/* 00 */
			value.Append("1"); // event::tacceleration
			value.Append("|");
			/* 01 */
			value.Append(((int)((this.StartTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 02 */
			value.Append(((int)((this.PeakTime.Value - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 03 */
			value.Append(((int)((this.EndTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
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
			value.Append(this.Confidence.Value.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 10 */
			value.Append(this.Repair.Value.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 11 */
			value.Append(this.Height.Value.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 12 */
			value.Append(string.Empty); // Baseline variability
			value.Append("|");
			/* 13 */
			value.Append(this.PeakValue.Value.ToString("0.000000", CultureInfo.InvariantCulture));
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
			if (this.IsNonInterpretable.Value) value.Append("y");
			value.Append("|");
			/* 19 */
			value.Append(string.Empty); // Confirmed
			value.Append("|");
			/* 20 */
			value.Append(this.ArtifactId.ToString(CultureInfo.InvariantCulture));

			return value.ToString();
		}

		/// <summary>
		/// Encode the artifact as a deceleration
		/// </summary>
		/// <param name="basetime"></param>
		/// <returns></returns>
		protected string EncodeAsDeceleration(DateTime basetime)
		{
			Debug.Assert(this.Category == (int)ArtifactCategories.Deceleration);

			int eventType = 0;

			if ((DecelerationCategories)this.DecelerationCategory == DecelerationCategories.NonAssociated)
			{
				eventType = 7; // event::tnadeceleration  ***
			}
			else if ((DecelerationCategories)this.DecelerationCategory == DecelerationCategories.Early)
			{
				eventType = 3; // event::tearly ***
			}
			else if ((DecelerationCategories)this.DecelerationCategory == DecelerationCategories.Late)
			{
				eventType = 6; // event::tlate ***	
			}
			else if ((DecelerationCategories)this.DecelerationCategory == DecelerationCategories.Variable)
			{
				if (((AtypicalCharacteristics)this.NonReassuringFeatures & AtypicalCharacteristics.Prolonged) == AtypicalCharacteristics.Prolonged)
				{
					eventType = 14; // event::tprolonged ***
				}
				if ((AtypicalCharacteristics)this.NonReassuringFeatures != AtypicalCharacteristics.None)
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
			if (((AtypicalCharacteristics)this.NonReassuringFeatures & AtypicalCharacteristics.Biphasic) == AtypicalCharacteristics.Biphasic)
			{
				atypicalValue |= 1;
			}
			if (((AtypicalCharacteristics)this.NonReassuringFeatures & AtypicalCharacteristics.LossRise) == AtypicalCharacteristics.LossRise)
			{
				atypicalValue |= 2;
			}
			if (((AtypicalCharacteristics)this.NonReassuringFeatures & AtypicalCharacteristics.LossVariability) == AtypicalCharacteristics.LossVariability)
			{
				atypicalValue |= 4;
			}
			if (((AtypicalCharacteristics)this.NonReassuringFeatures & AtypicalCharacteristics.LowerBaseline) == AtypicalCharacteristics.LowerBaseline)
			{
				atypicalValue |= 8;
			}
			if (((AtypicalCharacteristics)this.NonReassuringFeatures & AtypicalCharacteristics.ProlongedSecondRise) == AtypicalCharacteristics.ProlongedSecondRise)
			{
				atypicalValue |= 16;
			}
			if (((AtypicalCharacteristics)this.NonReassuringFeatures & AtypicalCharacteristics.Sixties) == AtypicalCharacteristics.Sixties)
			{
				atypicalValue |= 32;
			}
			if (((AtypicalCharacteristics)this.NonReassuringFeatures & AtypicalCharacteristics.SlowReturn) == AtypicalCharacteristics.SlowReturn)
			{
				atypicalValue |= 64;
			}

			StringBuilder value = new StringBuilder(255);

			value.Append("EVT|");

			/* 00 */
			value.Append(eventType.ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 01 */
			value.Append(((int)((this.StartTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 02 */
			value.Append(((int)((this.PeakTime.Value - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 03 */
			value.Append(((int)((this.EndTime - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 04 */
			value.Append(string.Empty); // Y1
			value.Append("|");
			/* 05 */
			value.Append(string.Empty); // Y2
			value.Append("|");
			/* 06 */
			value.Append(this.ContractionStart.HasValue ? ((int)((ContractionStart.Value - basetime).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture) : string.Empty);
			value.Append("|");
			/* 07 */
			value.Append("y"); // Final
			value.Append("|");
			/* 08 */
			value.Append(string.Empty); // Strikeout
			value.Append("|");
			/* 09 */
			value.Append(this.Confidence.Value.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 10 */
			value.Append(this.Repair.Value.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 11 */
			value.Append(this.Height.Value.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 12 */
			value.Append(string.Empty); // Baseline variability
			value.Append("|");
			/* 13 */
			value.Append(this.PeakValue.Value.ToString("0.000000", CultureInfo.InvariantCulture));
			value.Append("|");
			/* 14 */
			value.Append("|");
			/* 15 */
			if ((DecelerationCategories)this.DecelerationCategory == DecelerationCategories.Variable) value.Append("y");
			value.Append("|");
			/* 16 */
			value.Append("-1"); // Lag
			value.Append("|");
			/* 17 */
			value.Append(atypicalValue.ToString(CultureInfo.InvariantCulture));
			value.Append("|");
			/* 18 */
			if (this.IsNonInterpretable.Value) value.Append("y");
			value.Append("|");
			/* 19 */
			value.Append(string.Empty); // Confirmed
			value.Append("|");
			/* 20 */
			value.Append(this.ArtifactId.ToString(CultureInfo.InvariantCulture));

			return value.ToString();
		}

		/// <summary>
		/// Serialize the given artifacts
		/// </summary>
		/// <returns></returns>
		public string EncodeForActiveX(DateTime basetime)
		{
			// Contractions
			if (this.Category == (int)ArtifactCategories.Contraction)
			{
				return this.EncodeAsContraction(basetime);
			}

			// Baselines
			if (this.Category == (int)ArtifactCategories.Baseline)
			{
				return this.EncodeAsBaseline(basetime);
			}

			// Accelerations
			if (this.Category == (int)ArtifactCategories.Acceleration)
			{
				return this.EncodeAsAcceleration(basetime);
			}

			if (this.Category == (int)ArtifactCategories.Deceleration)
			{
				return this.EncodeAsDeceleration(basetime);
			}

			throw new InvalidOperationException("Type of detection artifact unknown to the patterns engine mapping data structure");
		}
	}
}
