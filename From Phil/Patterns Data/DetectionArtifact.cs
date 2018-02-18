using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriGen.Patterns.Engine.Data
{
	public enum ArtifactCategories : byte
	{
		None = 0,
		Baseline = 1,
		Acceleration = 2,
		Deceleration = 3,
		Contraction = 4,
	}

	public enum DecelerationCategories : byte
	{
		None = 0,
		Early = 1,
		Variable = 2,
		Late = 4,
		NonAssociated = 5
	}

	[Flags]
	public enum AtypicalCharacteristics : byte
	{
		None = 0,
		LossRise = 1,
		LossVariability = 2,
		Sixties = 4,
		Biphasic = 8,
		Prolonged = 16,
		LowerBaseline = 32,
		ProlongedSecondRise = 64,
		SlowReturn = 128
	}

    public abstract class DetectedObject
    {
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public int Id { get; set; }

        public abstract String WriteDAT(DateTime absoluteStart);
    }

    public abstract class DetectionArtifact : DetectedObject
	{
		public abstract ArtifactCategories Category { get; }


		public bool IsContraction { get { return this.Category == ArtifactCategories.Contraction; } }
		public bool IsBaseline { get { return this.Category == ArtifactCategories.Baseline; } }
		public bool IsAcceleration { get { return this.Category == ArtifactCategories.Acceleration; } }
		public bool IsDeceleration { get { return this.Category == ArtifactCategories.Deceleration; } }
	}

    public abstract class DeletableDetectedArtifact : DetectionArtifact
    {
        public bool IsStrikedOut { get; set; }
    }

    public class Contraction : DeletableDetectedArtifact
	{
		public override ArtifactCategories Category { get { return ArtifactCategories.Contraction; } }

		public DateTime PeakTime { get; set; }

        public override string WriteDAT(DateTime absoluteStart)
        {
            return DATWriter.WriteContractionDAT(this, absoluteStart);
        }

		public override string ToString()
		{
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "Contraction| S:{0:s} P:{1:s} E:{2:s}", this.StartTime, this.PeakTime, this.EndTime);
		}
	}

	public class Baseline : DetectionArtifact
	{
		public override ArtifactCategories Category { get { return ArtifactCategories.Baseline; } }

		public double Y1 { get; set; }
		public double Y2 { get; set; }
		public double BaselineVariability { get; set; }

        public override string WriteDAT(DateTime absoluteStart)
        {
            return DATWriter.WriteBaselineDAT(this, absoluteStart);
        }

        public override string ToString()
		{
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "Baseline| S:{0:s} E:{1:s} Variability:{2:0.0}", this.StartTime, this.EndTime, this.BaselineVariability);
		}
	}

    public abstract class PatternEvent : DeletableDetectedArtifact
	{
		public DateTime PeakTime { get; set; }
		public double PeakValue { get; set; }

		public double Confidence { get; set; }
		public double Repair { get; set; }
		public double Height { get; set; }

		public bool IsNonInterpretable { get; set; }
	}

	public class Acceleration : PatternEvent
	{
		public override ArtifactCategories Category { get { return ArtifactCategories.Acceleration; } }

        public override string WriteDAT(DateTime absoluteStart)
        {
            return DATWriter.WriteAccelerationDAT(this, absoluteStart);
        }

        public override string ToString()
		{
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "Acceleration| S:{0:s} P:{1:s} E:{2:s}", this.StartTime, this.PeakTime, this.EndTime);
		}
	}

	public class Deceleration : PatternEvent
	{
		public override ArtifactCategories Category { get { return ArtifactCategories.Deceleration; } }

		public DecelerationCategories DecelerationCategory { get; set; }

		#region DecelerationCategory properties

		public bool IsEarlyDeceleration
		{
			get { return this.DecelerationCategory == DecelerationCategories.Early; }
			set { if (value) { this.DecelerationCategory = DecelerationCategories.Early; } else { this.DecelerationCategory = DecelerationCategories.None; } }
		}
		public bool IsVariableDeceleration
		{
			get { return this.DecelerationCategory == DecelerationCategories.Variable; }
			set { if (value) { this.DecelerationCategory = DecelerationCategories.Variable; } else { this.DecelerationCategory = DecelerationCategories.None; } }
		}
		public bool IsLateDeceleration
		{
			get { return this.DecelerationCategory == DecelerationCategories.Late; }
			set { if (value) { this.DecelerationCategory = DecelerationCategories.Late; } else { this.DecelerationCategory = DecelerationCategories.None; } }
		}
		public bool IsNonAssociatedDeceleration
		{
			get { return this.DecelerationCategory == DecelerationCategories.NonAssociated; }
			set { if (value) { this.DecelerationCategory = DecelerationCategories.NonAssociated; } else { this.DecelerationCategory = DecelerationCategories.None; } }
		}

		#endregion

		public AtypicalCharacteristics NonReassuringFeatures { get; set; }

		#region NonReassuringFeatures properties

		public bool HasNonReassuringFeature
		{
			get { return this.IsVariableDeceleration && (this.NonReassuringFeatures != AtypicalCharacteristics.None); }
		}

		public bool HasLossVariabilityNonReassuringFeature
		{
			get { return this.IsVariableDeceleration && (this.NonReassuringFeatures & AtypicalCharacteristics.LossVariability) == AtypicalCharacteristics.LossVariability; }
			set
			{
				if (value)
					this.NonReassuringFeatures |= AtypicalCharacteristics.LossVariability;
				else
					this.NonReassuringFeatures &= ~AtypicalCharacteristics.LossVariability;
			}
		}
		public bool HasSixtiesNonReassuringFeature
		{
			get { return this.IsVariableDeceleration && (this.NonReassuringFeatures & AtypicalCharacteristics.Sixties) == AtypicalCharacteristics.Sixties; }
			set
			{
				if (value)
					this.NonReassuringFeatures |= AtypicalCharacteristics.Sixties;
				else
					this.NonReassuringFeatures &= ~AtypicalCharacteristics.Sixties;
			}
		}
		public bool HasLossRiseNonReassuringFeature
		{
			get { return this.IsVariableDeceleration && (this.NonReassuringFeatures & AtypicalCharacteristics.LossRise) == AtypicalCharacteristics.LossRise; }
			set
			{
				if (value)
					this.NonReassuringFeatures |= AtypicalCharacteristics.LossRise;
				else
					this.NonReassuringFeatures &= ~AtypicalCharacteristics.LossRise;
			}
		}
		public bool HasLowerBaselineNonReassuringFeature
		{
			get { return this.IsVariableDeceleration && (this.NonReassuringFeatures & AtypicalCharacteristics.LowerBaseline) == AtypicalCharacteristics.LowerBaseline; }
			set
			{
				if (value)
					this.NonReassuringFeatures |= AtypicalCharacteristics.LowerBaseline;
				else
					this.NonReassuringFeatures &= ~AtypicalCharacteristics.LowerBaseline;
			}
		}
		public bool HasProlongedSecondRiseNonReassuringFeature
		{
			get { return this.IsVariableDeceleration && (this.NonReassuringFeatures & AtypicalCharacteristics.ProlongedSecondRise) == AtypicalCharacteristics.ProlongedSecondRise; }
			set
			{
				if (value)
					this.NonReassuringFeatures |= AtypicalCharacteristics.ProlongedSecondRise;
				else
					this.NonReassuringFeatures &= ~AtypicalCharacteristics.ProlongedSecondRise;
			}
		}
		public bool HasSlowReturnNonReassuringFeature
		{
			get { return this.IsVariableDeceleration && (this.NonReassuringFeatures & AtypicalCharacteristics.SlowReturn) == AtypicalCharacteristics.SlowReturn; }
			set
			{
				if (value)
					this.NonReassuringFeatures |= AtypicalCharacteristics.SlowReturn;
				else
					this.NonReassuringFeatures &= ~AtypicalCharacteristics.SlowReturn;
			}
		}
		public bool HasBiphasicNonReassuringFeature
		{
			get { return this.IsVariableDeceleration && (this.NonReassuringFeatures & AtypicalCharacteristics.Biphasic) == AtypicalCharacteristics.Biphasic; }
			set
			{
				if (value)
					this.NonReassuringFeatures |= AtypicalCharacteristics.Biphasic;
				else
					this.NonReassuringFeatures &= ~AtypicalCharacteristics.Biphasic;
			}
		}
		public bool HasProlongedNonReassuringFeature
		{
			get { return this.IsVariableDeceleration && (this.NonReassuringFeatures & AtypicalCharacteristics.Prolonged) == AtypicalCharacteristics.Prolonged; }
			set
			{
				if (value)
					this.NonReassuringFeatures |= AtypicalCharacteristics.Prolonged;
				else
					this.NonReassuringFeatures &= ~AtypicalCharacteristics.Prolonged;
			}
		}
		#endregion

		public DateTime? ContractionStart { get; set; }

        public override string WriteDAT(DateTime absoluteStart)
        {
            return DATWriter.WriteDecelerationDAT(this, absoluteStart);
        }

        public override string ToString()
		{
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "Deceleration| S:{0:s} P:{1:s} E:{2:s}", this.StartTime, this.PeakTime, this.EndTime);
		}
	}
}
