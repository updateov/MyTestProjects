using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace PeriGen.Patterns.Research.SQLHelper
{
	public static class DataMapping
	{
		/// <summary>
		/// Convert a double that represents a HR/UP value to a simple byte from 0 to 255
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		static byte ToSignalByteValue(this double value)
		{
			return Convert.ToByte(Math.Min(255, Math.Max(0, value)), CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Convert a double that represents a percentage value (0.12) to a simple byte from 0 to 100
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		static byte ToPercentageByteValue(this double value)
		{
			return Convert.ToByte(Math.Min(100, Math.Max(0, value * 100)), CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Convert an artifact from the engine to a NEW SQL record in the Artifact table
		/// It does not populate the ArtifactId column NOR the EpisodeId column
		/// </summary>
		/// <param name="artifact"></param>
		/// <returns></returns>
		public static PeriGen.Patterns.Research.SQLHelper.Artifact ToDatabaseDetectionArtifact(this PeriGen.Patterns.Engine.Data.DetectionArtifact artifact)
		{
			if (artifact == null)
				throw new ArgumentNullException("artifact");

			if (artifact.IsContraction)
			{
				PeriGen.Patterns.Engine.Data.Contraction contraction = artifact as PeriGen.Patterns.Engine.Data.Contraction;
				return new PeriGen.Patterns.Research.SQLHelper.Artifact
					{
						Category = Convert.ToByte(artifact.Category, CultureInfo.InvariantCulture),

						StartTime = contraction.StartTime,
						EndTime = contraction.EndTime,

						PeakTime = contraction.PeakTime
					};
			}
			if (artifact.IsBaseline)
			{
				PeriGen.Patterns.Engine.Data.Baseline baseline = artifact as PeriGen.Patterns.Engine.Data.Baseline;
				return new PeriGen.Patterns.Research.SQLHelper.Artifact
					{
						Category = Convert.ToByte(artifact.Category, CultureInfo.InvariantCulture),

						StartTime = baseline.StartTime,
						EndTime = baseline.EndTime,

						Y1 = baseline.Y1.ToSignalByteValue(),
						Y2 = baseline.Y2.ToSignalByteValue(),

						BaselineVariability = baseline.BaselineVariability.ToSignalByteValue(),
					};
			}
			if (artifact.IsAcceleration)
			{
				PeriGen.Patterns.Engine.Data.Acceleration acceleration = artifact as PeriGen.Patterns.Engine.Data.Acceleration;
				return new PeriGen.Patterns.Research.SQLHelper.Artifact
					{
						Category = Convert.ToByte(acceleration.Category, CultureInfo.InvariantCulture),

						StartTime = acceleration.StartTime,
						EndTime = acceleration.EndTime,

						PeakTime = acceleration.PeakTime,
						PeakValue = acceleration.PeakValue.ToSignalByteValue(),

						Confidence = acceleration.Confidence.ToPercentageByteValue(),
						Repair = acceleration.Repair.ToPercentageByteValue(),
						Height = acceleration.Height.ToSignalByteValue(),

						IsNonInterpretable = acceleration.IsNonInterpretable
					};
			}
			if (artifact.IsDeceleration)
			{
				PeriGen.Patterns.Engine.Data.Deceleration deceleration = artifact as PeriGen.Patterns.Engine.Data.Deceleration;
				return new PeriGen.Patterns.Research.SQLHelper.Artifact
					{
						Category = Convert.ToByte(deceleration.Category, CultureInfo.InvariantCulture),

						StartTime = deceleration.StartTime,
						EndTime = deceleration.EndTime,

						PeakTime = deceleration.PeakTime,
						PeakValue = deceleration.PeakValue.ToSignalByteValue(),

						Confidence = deceleration.Confidence.ToPercentageByteValue(),
						Repair = deceleration.Repair.ToPercentageByteValue(),
						Height = deceleration.Height.ToSignalByteValue(),

						IsNonInterpretable = deceleration.IsNonInterpretable,

						DecelerationCategory = Convert.ToByte(deceleration.DecelerationCategory, CultureInfo.InvariantCulture),
						NonReassuringFeatures = Convert.ToByte(deceleration.NonReassuringFeatures, CultureInfo.InvariantCulture),

						ContractionStart = deceleration.ContractionStart
					};
			}

			throw new InvalidOperationException("Type of detection artifact unknown to the sql mapping data structure");
		}

		/// <summary>
		/// Convert a database record of the Artifact table into an engine artifact
		/// </summary>
		/// <param name="artifact"></param>
		/// <returns></returns>
		public static PeriGen.Patterns.Engine.Data.DetectionArtifact ToEngineDetectionArtifact(this PeriGen.Patterns.Research.SQLHelper.Artifact artifact)
		{
			if (artifact == null)
				throw new ArgumentNullException("artifact");

			if (((PeriGen.Patterns.Engine.Data.ArtifactCategories)artifact.Category) == PeriGen.Patterns.Engine.Data.ArtifactCategories.Contraction)
			{
				return new PeriGen.Patterns.Engine.Data.Contraction 
					{
						Id = artifact.ArtifactId,

						StartTime = artifact.StartTime, 
						PeakTime = artifact.PeakTime.Value, 
						EndTime = artifact.EndTime
					};
			}
			if (((PeriGen.Patterns.Engine.Data.ArtifactCategories)artifact.Category) == PeriGen.Patterns.Engine.Data.ArtifactCategories.Baseline)
			{
				return new PeriGen.Patterns.Engine.Data.Baseline 
					{
						Id = artifact.ArtifactId,

						StartTime = artifact.StartTime, 
						EndTime = artifact.EndTime,

						Y1 = artifact.Y1.Value,
						Y2 = artifact.Y2.Value,

						BaselineVariability = artifact.BaselineVariability.Value
					};
			}
			if (((PeriGen.Patterns.Engine.Data.ArtifactCategories)artifact.Category) == PeriGen.Patterns.Engine.Data.ArtifactCategories.Acceleration)
			{
				return new PeriGen.Patterns.Engine.Data.Acceleration 
					{
						Id = artifact.ArtifactId,

						StartTime = artifact.StartTime, 
						EndTime = artifact.EndTime,

						PeakTime = artifact.PeakTime.Value,
						PeakValue = artifact.PeakValue.Value,

						Confidence = artifact.Confidence.Value / 100f,
						Repair = artifact.Repair.Value / 100f,
						Height = artifact.Height.Value,
						
						IsNonInterpretable = artifact.IsNonInterpretable.Value
					};
			}
			if (((PeriGen.Patterns.Engine.Data.ArtifactCategories)artifact.Category) == PeriGen.Patterns.Engine.Data.ArtifactCategories.Deceleration)
			{
				return 
					new PeriGen.Patterns.Engine.Data.Deceleration 
					{
						Id = artifact.ArtifactId,

						StartTime = artifact.StartTime, 
						EndTime = artifact.EndTime,

						PeakTime = artifact.PeakTime.Value,
						PeakValue = artifact.PeakValue.Value,

						Confidence = artifact.Confidence.Value / 100f,
						Repair = artifact.Repair.Value / 100f,
						Height = artifact.Height.Value,
						
						IsNonInterpretable = artifact.IsNonInterpretable.Value,
						
						NonReassuringFeatures = (PeriGen.Patterns.Engine.Data.AtypicalCharacteristics)artifact.NonReassuringFeatures.Value,
						DecelerationCategory = (PeriGen.Patterns.Engine.Data.DecelerationCategories)artifact.DecelerationCategory,

						ContractionStart = artifact.ContractionStart
					};
			}

			throw new InvalidOperationException("Type of detection artifact unknown to the patterns engine mapping data structure");
		}
	}
}
