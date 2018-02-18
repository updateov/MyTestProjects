// This file is intended to be edited manually

using System;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using PeriGen.Patterns.ActiveXInterface;

namespace DataContextEpisode
{
	partial class DataContextEpisode
	{
		/// <summary>
		/// If the DB does not exist, create it now
		/// </summary>
		/// <returns>True is the DB didn't exist yet and was created</returns>
		public bool CreateIfNecessary()
		{
			if (!this.DatabaseExists())
			{
				this.CreateDatabase(false, true);
				this.SubmitChanges();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Change the value of a given parameter
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetParameter(string name, string value)
		{
			Debug.Assert(!string.IsNullOrWhiteSpace(name));
			name = name.ToUpperInvariant();

			// The database does not accept a null value...
			if (value == null)
				value = string.Empty;

			var entry = this.Parameters.FirstOrDefault(p => p.Name == name);
			if (entry == null)
			{
				this.Parameters.InsertOnSubmit(new Parameter { Name = name, Value = value});
			}
			else
			{
				entry.Value = value;
			}
		}

		/// <summary>
		/// Retrieve the value of a given parameter
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public string GetParameter(string name)
		{
			Debug.Assert(!string.IsNullOrWhiteSpace(name));
			name = name.ToUpperInvariant();

			var entry = this.Parameters.FirstOrDefault(p => p.Name == name);
			return (entry == null) ? null : entry.Value;
		}
	}

	partial class UserAction
	{
		/// <summary>
		/// Contructor from a ActiveX user action
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public static UserAction From(XUserAction action)
		{
			return new UserAction
					{
						ActionId = action.Id,
						ActionType = (byte)action.ActionType,
						ArtifactId = action.ArtifactId,
						UserId = action.UserId ?? string.Empty,
						UserName = action.UserName ?? string.Empty,
						PerformedTime = action.PerformedTime.ToEpoch()
					};
		}

		/// <summary>
		/// Return the equivalent activeX user action
		/// </summary>
		/// <returns></returns>
		public XUserAction ToXUserAction()
		{
			return new XUserAction
						{
							Id = this.ActionId,
							ActionType = (ActionTypes)Enum.ToObject(typeof(ActionTypes), this.ActionType),
							ArtifactId = this.ArtifactId,
							UserId = this.UserId,
							UserName = this.UserName,
							PerformedTime = this.PerformedTime.ToDateTime()
						};
		}
	}

	partial class Tracing
	{
		/// <summary>
		/// Contructor from a pattern's engine tracing block
		/// </summary>
		/// <param name="contraction"></param>
		public static Tracing From(PeriGen.Patterns.Engine.Data.TracingBlock block)
		{
			return new Tracing
						{
							TracingId = block.Id,
							StartTime = block.Start.ToEpoch(),
							EndTime = block.End.ToEpoch(),
							HR1 = block.HRs.ToArray(),
							UP = block.UPs.ToArray()
						};
		}

		/// <summary>
		/// Return the equivalent pattern's engine Tracing block
		/// </summary>
		/// <returns></returns>
		public PeriGen.Patterns.Engine.Data.TracingBlock ToTracingBlock()
		{
			return new PeriGen.Patterns.Engine.Data.TracingBlock
			{
				Capacity = this.HR1.Count(),
				Id = this.TracingId,
				HRs = this.HR1.ToList(),
				UPs = this.UP.ToList(),
				Start = this.StartTime.ToDateTime()
			};
		}
	}

	partial class Artifact
	{
		/// <summary>
		/// Is the artifact a contraction?
		/// </summary>
		public bool IsContraction { get { return this.Category == (byte)PeriGen.Patterns.Engine.Data.ArtifactCategories.Contraction; } }

		/// <summary>
		/// Is the artifact a baseline?
		/// </summary>
		public bool IsBaseline { get { return this.Category == (byte)PeriGen.Patterns.Engine.Data.ArtifactCategories.Baseline; } }

		/// <summary>
		/// Is the artifact an acceleration?
		/// </summary>
		public bool IsAcceleration { get { return this.Category == (byte)PeriGen.Patterns.Engine.Data.ArtifactCategories.Acceleration; } }

		/// <summary>
		/// Is the artifact a deceleration?
		/// </summary>
		public bool IsDeceleration { get { return this.Category == (byte)PeriGen.Patterns.Engine.Data.ArtifactCategories.Deceleration; } }

		/// <summary>
		/// Constructor of a sqlite mapping artifact from a pattern's engine artifact
		/// </summary>
		/// <param name="patternArtifact"></param>
		/// <returns></returns>
		public static Artifact From(PeriGen.Patterns.Engine.Data.DetectionArtifact patternArtifact)
		{
			if (patternArtifact.IsContraction)
			{
				PeriGen.Patterns.Engine.Data.Contraction contraction = patternArtifact as PeriGen.Patterns.Engine.Data.Contraction;
				return new Artifact
				{
					ArtifactId = patternArtifact.Id,
					Category = Convert.ToByte(contraction.Category, CultureInfo.InvariantCulture),

					StartTime = contraction.StartTime.ToEpoch(),
					PeakTime = contraction.PeakTime.ToEpoch(),
					EndTime = contraction.EndTime.ToEpoch(),

                    IsStrikedOut = contraction.IsStrikedOut
				};
			}

			if (patternArtifact.IsBaseline)
			{
				PeriGen.Patterns.Engine.Data.Baseline baseline = patternArtifact as PeriGen.Patterns.Engine.Data.Baseline;
				return new Artifact
				{
					ArtifactId = patternArtifact.Id,
					Category = Convert.ToByte(baseline.Category, CultureInfo.InvariantCulture),

					StartTime = baseline.StartTime.ToEpoch(),
					EndTime = baseline.EndTime.ToEpoch(),

					Y1 = baseline.Y1.ToSignalByteValue(),
					Y2 = baseline.Y2.ToSignalByteValue(),

					BaselineVariability = baseline.BaselineVariability.ToSignalByteValue()
				};
			}

			if (patternArtifact.IsAcceleration)
			{
				PeriGen.Patterns.Engine.Data.Acceleration acceleration = patternArtifact as PeriGen.Patterns.Engine.Data.Acceleration;
				return new Artifact
				{
					ArtifactId = patternArtifact.Id,
					Category = Convert.ToByte(acceleration.Category, CultureInfo.InvariantCulture),

					StartTime = acceleration.StartTime.ToEpoch(),
					EndTime = acceleration.EndTime.ToEpoch(),

					PeakTime = acceleration.PeakTime.ToEpoch(),
					PeakValue = acceleration.PeakValue.ToSignalByteValue(),

					Confidence = acceleration.Confidence.ToPercentageByteValue(),
					Repair = acceleration.Repair.ToPercentageByteValue(),
					Height = acceleration.Height.ToSignalByteValue(),

					IsNonInterpretable = acceleration.IsNonInterpretable,

                    IsStrikedOut = acceleration.IsStrikedOut
				};
			}

			if (patternArtifact.IsDeceleration)
			{
				PeriGen.Patterns.Engine.Data.Deceleration deceleration = patternArtifact as PeriGen.Patterns.Engine.Data.Deceleration;
				return new Artifact
				{
					ArtifactId = patternArtifact.Id,
					Category = Convert.ToByte(deceleration.Category, CultureInfo.InvariantCulture),

					StartTime = deceleration.StartTime.ToEpoch(),
					EndTime = deceleration.EndTime.ToEpoch(),

					PeakTime = deceleration.PeakTime.ToEpoch(),
					PeakValue = deceleration.PeakValue.ToSignalByteValue(),

					Confidence = deceleration.Confidence.ToPercentageByteValue(),
					Repair = deceleration.Repair.ToPercentageByteValue(),
					Height = deceleration.Height.ToSignalByteValue(),

					IsNonInterpretable = deceleration.IsNonInterpretable,

                    IsStrikedOut = deceleration.IsStrikedOut,

					DecelerationCategory = Convert.ToByte(deceleration.DecelerationCategory, CultureInfo.InvariantCulture),
					NonReassuringFeatures = Convert.ToByte(deceleration.NonReassuringFeatures, CultureInfo.InvariantCulture),

					ContractionStart = deceleration.ContractionStart.ToEpoch()
				};
			}

			throw new InvalidOperationException("Type of detection artifact unknown to the sqlite mapping data structure");
		}

		/// <summary>
		/// Return the equivalent pattern's engine artifact
		/// </summary>
		/// <returns></returns>
		public PeriGen.Patterns.Engine.Data.DetectionArtifact ToDetectionArtifact()
		{
			if (this.IsContraction)
			{
				return new PeriGen.Patterns.Engine.Data.Contraction
				{
					Id = this.ArtifactId,
					StartTime = this.StartTime.ToDateTime(),
					PeakTime = this.PeakTime.ToDateTime(),
					EndTime = this.EndTime.ToDateTime(),

                    IsStrikedOut = this.IsStrikedOut
				};
			}

			if (this.IsBaseline)
			{
				return new PeriGen.Patterns.Engine.Data.Baseline
				{
					Id = this.ArtifactId,
					StartTime = this.StartTime.ToDateTime(),
					EndTime = this.EndTime.ToDateTime(),

					Y1 = this.Y1,
					Y2 = this.Y2,
					BaselineVariability = this.BaselineVariability
				};
			}

			if (this.IsAcceleration)
			{
				return new PeriGen.Patterns.Engine.Data.Acceleration
				{
					Id = this.ArtifactId,
					StartTime = this.StartTime.ToDateTime(),
					EndTime = this.EndTime.ToDateTime(),

					PeakTime = this.PeakTime.ToDateTime(),
					PeakValue = this.PeakValue,

					Confidence = this.Confidence / 100f,
					Repair = this.Repair / 100f,
					Height = this.Height,

					IsNonInterpretable = this.IsNonInterpretable,

                    IsStrikedOut = this.IsStrikedOut
				};
			}

			if (this.IsDeceleration)
			{
				return new PeriGen.Patterns.Engine.Data.Deceleration
				{
					Id = this.ArtifactId,
					StartTime = this.StartTime.ToDateTime(),
					EndTime = this.EndTime.ToDateTime(),

					PeakTime = this.PeakTime.ToDateTime(),
					PeakValue = this.PeakValue,

					Confidence = this.Confidence / 100f,
					Repair = this.Repair / 100f,
					Height = this.Height,

					IsNonInterpretable = this.IsNonInterpretable,

                    IsStrikedOut = this.IsStrikedOut,

					DecelerationCategory = (PeriGen.Patterns.Engine.Data.DecelerationCategories)Enum.ToObject(typeof(PeriGen.Patterns.Engine.Data.DecelerationCategories), this.DecelerationCategory),
					NonReassuringFeatures = (PeriGen.Patterns.Engine.Data.AtypicalCharacteristics)Enum.ToObject(typeof(PeriGen.Patterns.Engine.Data.AtypicalCharacteristics), this.NonReassuringFeatures),

					ContractionStart = this.ContractionStart.ToDateTime()
				};
			}

			throw new InvalidOperationException("Type of detection artifact unknown to the sqlite mapping data structure");
		}
	}

	partial class PelvicExam
	{
		/// <summary>
		/// Check if the exam is fully strikeout (all values empty)
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return
					string.IsNullOrWhiteSpace(this.Effacement)
					&& string.IsNullOrWhiteSpace(this.Dilatation)
					&& string.IsNullOrWhiteSpace(this.Station)
					&& string.IsNullOrWhiteSpace(this.Presentation)
					&& string.IsNullOrWhiteSpace(this.Position);
			}
		}

		/// <summary>
		/// Return the merged list of pelvic exam, that is to say, for each time, the latest one
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static List<PelvicExam> Merge(List<PelvicExam> list)
		{
			return list.GroupBy(exam => exam.Time).Select(group => group.OrderBy(p => p.ExamId).Last()).ToList();
		}
	}
}