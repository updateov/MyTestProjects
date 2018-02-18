using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostNameRegex
{
    class Program
    {
        static void Main(string[] args)
        {
            String siteURL = "net.tcp://calm_dev3:8201/PatternsDataFeed/";
            String systemURL = "http://localhost:7802/PatternsDataFeed/";
            String res = SetRemoteHost(siteURL, systemURL);
        }
        private static String SetRemoteHost(string siteURL, string systemURL)
        {
            String toRet = String.Empty;
            toRet = systemURL.Remove(systemURL.IndexOf(":"));
            siteURL = siteURL.Remove(siteURL.LastIndexOf(":"));
            siteURL = siteURL.Remove(0, siteURL.IndexOf(":"));
            systemURL = systemURL.Remove(0, systemURL.LastIndexOf(":"));
            toRet += siteURL + systemURL;
            return toRet;
        }
    }
}
