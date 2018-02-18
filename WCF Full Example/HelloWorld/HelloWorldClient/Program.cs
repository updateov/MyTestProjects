using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorldClient
{
    class Program
    {
        static void Main(string[] args)
        {
            HelloWorldServiceClient client = new HelloWorldServiceClient();
            Console.WriteLine(client.GetMessage("Mike Liu")); 
        }
    }
}
