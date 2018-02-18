namespace InterfaceTest
{
    public class Baseline : Artifact, IEvent
    {
        public int Y1 { get; set; }
        public int Y2 { get; set; }
        public double Variability { get; set; }
        public EventClassification EventType
        {
            get => EventClassification.Baseline;
        }
    }
}
