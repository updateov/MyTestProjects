using System;

namespace InterfaceTest
{
    public class Deceleration : Acceleration
    {
        public DateTime ContractionStart { get; set; }
        public DecelerationCategory DecelCategory { get; set; }
        public bool IsVariableDecel { get; set; }
        public int AtypicalValue { get; set; }

        public new EventClassification EventType
        {
            get => EventClassification.Deceleration;
        }
    }
}
