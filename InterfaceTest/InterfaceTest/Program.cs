using System;

namespace InterfaceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Artifact a1 = new Deceleration();
            PrintData(a1 as IEvent);

            IEvent a2 = new Acceleration();
            PrintData(a2);

            IEvent a3 = new Baseline();
            PrintData(a3);

            //Artifact a4 = new Contraction();
            //PrintData(a4);

            //Artifact a5 = new Contractility();
            //PrintData(a5);

            Console.ReadLine();
        }

        static void PrintData(IEvent art)
        {
            Console.WriteLine("Event Type = " + art.EventType.ToString());
            Console.WriteLine("Type = " + art.GetType().ToString());
            Console.WriteLine();
        }
    }
}
