using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Configuration;

namespace HostCmdLineApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Type serviceType = typeof(MyWCFServices.HelloWorldService);

            string httpBaseAddress = ConfigurationManager.AppSettings["HTTPBaseAddress"];
            Uri[] baseAddress = new Uri[] { new Uri(httpBaseAddress) };

            ServiceHost host = new ServiceHost(serviceType, baseAddress);
            host.Open();
            Console.WriteLine("HelloWorldService is now running. ");
            Console.WriteLine("Press any key to stop it ...");
            Console.ReadKey();
            host.Close();
        }
    }
}
