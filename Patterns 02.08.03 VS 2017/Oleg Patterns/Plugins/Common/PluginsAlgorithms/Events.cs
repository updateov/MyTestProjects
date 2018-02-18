using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginsAlgorithms
{
    public enum DecelerationCategories : byte
    {
        None = 0,
        Early = 1,
        Variable = 2,
        Late = 4,
        NonAssociated = 5,
        Prolonged = 14
    }

    public enum ArtifactType
    {
        Contraction,
        Baseline,
        Acceleration,
        Deceleration
    }

    public abstract class PluginDetectionArtifact : PluginDetectedObject
    {
        public ArtifactType EventType { get; set; }
    }

    public abstract class PluginDeleteableDetectionArtifact : PluginDetectionArtifact
    {
        public bool IsStrikedOut { get; set; }
        public long PeakTime { get; set; }
    }

    public class PluginContraction : PluginDeleteableDetectionArtifact
    {
    }

    public class PluginBaseline : PluginDetectionArtifact
    {
        public double Y1 { get; set; }
        public double Y2 { get; set; }

        public double BaselineVariability { get; set; }
    }

    public class PluginAcceleration : PluginDeleteableDetectionArtifact
    {
        public double PeakValue { get; set; }

        public double Confidence { get; set; }
        public double Repair { get; set; }
        public double Height { get; set; }

        public bool IsNonInterpretable { get; set; }
    }

    public class PluginDeceleration : PluginAcceleration
    {
        private const int LONG_DECEL_DURATION = 60;
        private const int LARGE_DECEL_HEIGHT = 60;

        public DecelerationCategories DecelerationCategory { get; set; }

        public bool LateTiming { get; set; }

        public bool LongAndLarge
        {
            get
            {
                return EndTime - StartTime > LONG_DECEL_DURATION && Height > LARGE_DECEL_HEIGHT;
            }
        }

        #region DecelerationCategory properties

        public bool IsEarlyDeceleration
        {
            get { return this.DecelerationCategory == DecelerationCategories.Early; }
            set
            {
                if (value)
                    this.DecelerationCategory = DecelerationCategories.Early;
                else
                    this.DecelerationCategory = DecelerationCategories.None;
            }
        }

        public bool IsVariableDeceleration
        {
            get { return this.DecelerationCategory == DecelerationCategories.Variable; }
            set
            {
                if (value)
                    this.DecelerationCategory = DecelerationCategories.Variable;
                else
                    this.DecelerationCategory = DecelerationCategories.None;
            }
        }

        public bool IsLateDeceleration
        {
            get { return this.DecelerationCategory == DecelerationCategories.Late; }
            set
            {
                if (value)
                    this.DecelerationCategory = DecelerationCategories.Late;
                else
                    this.DecelerationCategory = DecelerationCategories.None;
            }
        }

        public bool IsNonAssociatedDeceleration
        {
            get { return this.DecelerationCategory == DecelerationCategories.NonAssociated; }
            set
            {
                if (value)
                    this.DecelerationCategory = DecelerationCategories.NonAssociated;
                else
                    this.DecelerationCategory = DecelerationCategories.None;
            }
        }

        #endregion
    }
}
