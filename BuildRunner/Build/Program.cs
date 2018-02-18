using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Build
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Process proc = new Process())
            {
                ProcessStartInfo psi = new ProcessStartInfo("CVS -Q rtag -r PATTERN-020200-BRANCH Build-PATTERNS-020600-179-02242016  PATTERN");
                psi.WorkingDirectory = "D:\\Test";
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardError = true;
                psi.ErrorDialog = false;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo = psi;
                proc.Start();
                System.IO.StreamReader myOutput = proc.StandardOutput;
                String currentBuildingLog = String.Empty;
                using (StreamWriter wr = new StreamWriter("BuildLog666.log"))
                {
                    while (!proc.HasExited)
                    {
                        String output = myOutput.ReadToEnd();
                        currentBuildingLog += output;
                        wr.WriteLine(output);
                    }
                }

                if (proc.HasExited)
                {
                    var logStrList = currentBuildingLog.Split('\n').ToList();
                    var buildNum = (from c in logStrList
                                    where c.Contains("Build=")
                                    select c).FirstOrDefault();

                    buildNum = buildNum.Remove(0, buildNum.IndexOf("=") + 1).Trim();
                    Console.Out.WriteLine(buildNum);
                }
            }

            using (Process proc = new Process())
            {
                ProcessStartInfo psi = new ProcessStartInfo("cvs -Q checkout -r Build-PATTERNS-020600-179-02242016 -P -d PATTERN PATTERN");
                psi.WorkingDirectory = "D:\\Test";
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardError = true;
                psi.ErrorDialog = false;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo = psi;
                proc.Start();
                System.IO.StreamReader myOutput = proc.StandardOutput;
                String currentBuildingLog = String.Empty;
                using (StreamWriter wr = new StreamWriter("BuildLog666.log"))
                {
                    while (!proc.HasExited)
                    {
                        String output = myOutput.ReadToEnd();
                        currentBuildingLog += output;
                        wr.WriteLine(output);
                    }
                }

                if (proc.HasExited)
                {
                    var logStrList = currentBuildingLog.Split('\n').ToList();
                    var buildNum = (from c in logStrList
                                    where c.Contains("Build=")
                                    select c).FirstOrDefault();

                    buildNum = buildNum.Remove(0, buildNum.IndexOf("=") + 1).Trim();
                    Console.Out.WriteLine(buildNum);
                }
            }
        }
    }
}
