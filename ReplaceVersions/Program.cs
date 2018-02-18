using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplaceVersions
{
    class Program
    {
        static void Main(string[] args)
        {
            String path = @"g:\Develop\TFS\HUB\01.00.00\Private\OlegU Main HUB 01.00.00";
            DirectoryInfo di = new DirectoryInfo(path);
            var enDir = di.EnumerateDirectories("Properies", SearchOption.AllDirectories);
            var getDir = di.GetDirectories("Properties", SearchOption.AllDirectories);
            //foreach (var item in getDir)
            //{
            //    var files = item.GetFiles("AssemblyInfo.cs");
            //}
            var getFiles = di.GetFiles("AssemblyInfo.cs", SearchOption.AllDirectories);
            foreach (var item in getFiles)
            {
                String filePath = item.FullName;
                var fileStr = File.ReadAllLines(filePath);
                for (var i = 0; i < fileStr.Length; i++)
                {
                    if (fileStr[i].IndexOf("AssemblyVersion(\"1.0.0.0\")") >= 0)
                        fileStr[i] = fileStr[i].Replace("(\"1.0.0.0\")", "(\"01.00.00\")");
                    if (fileStr[i].IndexOf("AssemblyFileVersion(\"1.0.0.0\")") >= 0)
                        fileStr[i] = fileStr[i].Replace("(\"1.0.0.0\")", "(\"01.00.00\")");
                    if (fileStr[i].IndexOf("[assembly: AssemblyDescription(\"\")]") >= 0)
                        fileStr[i] = fileStr[i].Replace("(\"\")", "(\"320." + DateTime.Now.Day.ToString("D2") + DateTime.Now.Month.ToString("D2") + DateTime.Now.Year.ToString() + "\")");
                }
                //var version = (from c in fileStr where c.IndexOf("AssemblyVersion(\"1.0.0.0\")") >= 0 select c).First();
                //var fileVersion = (from c in fileStr where c.IndexOf("AssemblyFileVersion(\"1.0.0.0\")") >= 0 select c).First();
                //var description = (from c in fileStr where c.IndexOf("[assembly: AssemblyDescription(\"\")]") >= 0 select c).First();
                //version = version.Replace("(\"1.0.0.0\")", "(\"01.00.00\")");
                //fileVersion = fileVersion.Replace("(\"1.0.0.0\")", "(\"01.00.00.320\")");
                //description = description.Replace("(\"\")", "(\"320." + DateTime.Now.Day.ToString("D2") + DateTime.Now.Month.ToString("D2") + DateTime.Now.Year.ToString() + "\")");
                File.WriteAllLines(filePath, fileStr);
            }
        }
    }
}
