using System;

namespace InterfaceTest
{
    public interface IEvent
    {
        EventClassification EventType { get; }
    }

    public abstract class Artifact
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int ID{ get; set; }
    }

    public abstract class DeleteableArtifact : Artifact
    {
        public bool IsStrikedOut { get; set; }
        public DateTime PeakTime { get; set; }
    }
}
