using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HelloWorldClient
{
    class Program
    {
        static void Main(string[] args)
        {
            HelloWorldServiceClient client = new HelloWorldServiceClient();
            Console.WriteLine(client.GetMessage("Mike Liu"));
            Console.ReadKey();
        }
    }
}
