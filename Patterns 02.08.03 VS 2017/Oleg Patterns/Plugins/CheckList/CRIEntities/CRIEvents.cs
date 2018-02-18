using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRIEntities
{
    public class CRIEvents
    {
        public EventCounterDouble Variability { get; set; }
        public EventCounterLong Accels { get; set; }
        public EventCounterLong Contractions { get; set; }
        public EventCounterLong LongContractions { get; set; }
        public EventCounterLong LargeDeceles { get; set; }
        public EventCounterLong LateDecels { get; set; }
        public EventCounterLong ProlongedDecels { get; set; }

        public CRIEvents()
        {
            Variability = new EventCounterDouble();
            Accels = new EventCounterLong();
            Contractions = new EventCounterLong();
            LongContractions = new EventCounterLong();
            LargeDeceles = new EventCounterLong();
            LateDecels = new EventCounterLong();
            ProlongedDecels = new EventCounterLong();
        }

        public CRIEvents(CRIEvents that)
        {
            Variability = new EventCounterDouble(that.Variability);
            Accels = new EventCounterLong(that.Accels);
            Contractions = new EventCounterLong(that.Contractions);
            LongContractions = new EventCounterLong(that.LongContractions);
            LargeDeceles = new EventCounterLong(that.LargeDeceles);
            LateDecels = new EventCounterLong(that.LateDecels);
            ProlongedDecels = new EventCounterLong(that.ProlongedDecels);
        }
    }

    public abstract class EventCounter
    {
        public bool IsReason { get; set; }
        public EventCounter()
        {
            IsReason = false;
        }
    }

    public class EventCounterLong : EventCounter
    {
        public long Value { get; set; }
        public EventCounterLong()
        {
            Value = -1;
        }
        public EventCounterLong(EventCounterLong that)
        {
            IsReason = that.IsReason;
            Value = that.Value;
        }
    }

    public class EventCounterDouble: EventCounter
    {
        public double Value { get; set; }
        public EventCounterDouble()
        {
            Value = -1;
        }
        public EventCounterDouble(EventCounterDouble that)
        {
            IsReason = that.IsReason;
            Value = that.Value;
        }

    }
}

