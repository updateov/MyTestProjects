using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnumInc
{
    public enum EventClassification
    {
        Baseline = 0,
        Accel = 1,
        Decel = 2
    };

    class Program
    {
        static void Main(string[] args)
        {
            var eventType = EventClassification.Baseline;

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("EventClassification = " + eventType.ToString());
                eventType++;
                eventType = (EventClassification)(((int)eventType) % 3);
            }

            Console.ReadLine();
        }
    }
}
