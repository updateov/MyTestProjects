using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBUpdate.sqlite;

namespace DBUpdate.ExportPlugin
{
    class Program
    {
        static void Main(string[] args)
        { 
            try
            {
                /*
                 
                 Debug Args:
                 "..\..\dbupdate"  "C:\Program Files (x86)\PeriGen\PeriCALM Patterns\DBs"
                  
                 */
                DBUpdate.sqlite.Process.ProcessUtil.Elevate(args);

                string scriptsPath = "";
                string sqliteDBsPath = "";

                //incase we have paramters use them
                if (args.Length == 2)
                {
                    scriptsPath = args[0];
                    sqliteDBsPath = args[1];
                }

                //if scriptsPath is empty, it means the dbupdate is in the current folder
                if (scriptsPath == "")
                    scriptsPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

                Console.WriteLine("scriptsPath:{0}; sqliteDBsPath:{1};", scriptsPath, sqliteDBsPath);

                DBUpdateUtility.DBUpdateManager(scriptsPath, sqliteDBsPath);
                Environment.ExitCode = 0;
            }
            catch (Exception Exception)
            {
                Console.WriteLine(Exception.Message);
                Environment.ExitCode = 1;
            }
        }
    }
}
