namespace InterfaceTest
{
    public class Acceleration : DeleteableArtifact, IEvent
    {
        public double Confidence { get; set; }
        public double Repair { get; set; }
        public double Height { get; set; }
        public double PeakValue { get; set; }
        public bool IsNonInterpretable { get; set; }

        public EventClassification EventType
        {
            get => EventClassification.Acceleration;
        }
    }
}
