using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace PeriGen.Patterns.Research.OfflineDetection
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;
        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        static bool working = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            try
            {
				if (!EventLog.SourceExists("PeriGen Patterns Offline Detection"))
                {
                    // Create the source name / event log association
					EventLog.CreateEventSource("PeriGen Patterns Offline Detection", "Application");
                }
				if (!EventLog.SourceExists("PeriGen Patterns Engine"))
				{
					EventLog.CreateEventSource("PeriGen Patterns Engine", "Application");
				}
            }
			catch (Exception e)
			{
				ProcessorEngine.Trace.TraceEvent(TraceEventType.Warning, 9001, "Warning, unable to create the log source.\n{0}", e);
			}

            //No parameters, launch UI
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new DetectionWindow());
            }
            else
            {
                //Parameters show console
                //Attach console
                AttachConsole(ATTACH_PARENT_PROCESS);

                String strInputFolder = string.Empty;
                String strOutputXML = string.Empty;
                String strOutputSQL = string.Empty;
                String strRecursive = string.Empty;

                //Ok check parameters
                List<String> argsList = args.ToList();

                #region help /?

                //Check for help first
                if (argsList.Count(help => help == "/?") > 0)
                {
                    ShowCmdLineHelp(false);
                }

                #endregion

                #region input folder /i

                //check Input Folder
                var count = (from input in argsList
                             where input.Trim().ToLowerInvariant() == "/i"
                             select input).Count();

                if (count == 0 || count > 1)
                {
                    //error - input folder missing or more than one                
                    ShowCmdLineHelp(true);
                }

                //Input exist, check if folder is there
                var index = argsList.IndexOf((from input in argsList
                                              where input.Trim().ToLowerInvariant() == "/i"
                                              select input).SingleOrDefault());

                if (index == argsList.Count - 1)
                {
                    //folder missing
                    ShowCmdLineHelp(true);
                }


                //Ok value exist but... is another option
                strInputFolder = argsList[index + 1].ToLowerInvariant().Trim();
                if (strInputFolder == "/xml" ||
                   strInputFolder == "/sql" ||
                   strInputFolder == "/recursive") ShowCmdLineHelp(true);

                //Is a folder Ok...

                #endregion

                #region output xml /xml

                //check output xml option
                count = (from input in argsList
                         where input.Trim().ToLowerInvariant() == "/xml"
                         select input).Count();
                //skip option?
                if (count > 0)
                {
                    //more than one...?
                    if (count > 1)
                    {
                        //too many options                
                        ShowCmdLineHelp(true);
                    }
                    //only 1, ok
                    //check output folder for xml
                    index = argsList.IndexOf((from input in argsList
                                              where input.Trim().ToLowerInvariant() == "/xml"
                                              select input).SingleOrDefault());

                    if (index == argsList.Count - 1)
                    {
                        //folder missing
                        ShowCmdLineHelp(true);
                    }


                    //Ok value exist but... is another option
                    strOutputXML = argsList[index + 1].ToLowerInvariant().Trim();
                    if (strOutputXML == "/i" ||
                       strOutputXML == "/sql" ||
                       strOutputXML == "/recursive") ShowCmdLineHelp(true);
                    //Is a folder Ok...                
                }



                #endregion

                #region output sql /sql

                //Check sql
                count = argsList.Count(sql => sql.ToLowerInvariant().Trim() == "/sql");

                if (count == 1)
                {
                    strOutputSQL = "sql";
                    try
                    {
                        if (!DataUtils.CheckIfDatabaseExist())
                        {
                            DataUtils.CreateDatabase();
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error while creating database. Details:" + ex.Message + "\nOperation cancelled.");
                        SendKeys.SendWait("{ENTER}");
                        FreeConsole();
                        Application.Exit();
                    }
                }
                else
                {
                    if (count > 1)
                    {
                        ShowCmdLineHelp(true);
                    }
                }

                #endregion

                #region recursive /recursive

                //Check recursive
                count = argsList.Count(sql => sql.ToLowerInvariant().Trim() == "/recursive");

                if (count == 1)
                {
                    strRecursive = "recursive";
                }
                else
                {
                    if (count > 1)
                    {
                        ShowCmdLineHelp(true);
                    }
                }

                #endregion

                //check basic options
                if (string.IsNullOrEmpty(strInputFolder))
                {
                    //I need input folder
                    ShowCmdLineHelp(true);
                }

                //ok, do I have output??
                if (string.IsNullOrEmpty(strOutputSQL) && string.IsNullOrEmpty(strOutputXML))
                {
                    //I need output
                    ShowCmdLineHelp(true);
                }

                //Initialize Engine
                ProcessorEngine Processor = new ProcessorEngine();
                Processor.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(Processor_RunWorkerCompleted);
                Processor.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(Processor_ProgressChanged);


                //Prepare task
                ProcessorEngine.ProcessFilesArgs arguments = new ProcessorEngine.ProcessFilesArgs();
                arguments.ProcessOneFile = false;
                arguments.FileToProcess = string.Empty;
                arguments.SourceFolder = strInputFolder;
                arguments.IsRecursive = !string.IsNullOrEmpty(strRecursive);
                arguments.SaveToSQL = !string.IsNullOrEmpty(strOutputSQL);
                arguments.SaveToXML = !string.IsNullOrEmpty(strOutputXML);
                arguments.TargetFolder = strOutputXML;
                Processor.Args = arguments;
                working = true;
                //Run engine
                Processor.RunWorkerAsync();
                //wait until everything is finished
                while (working)
                {
                    Thread.Sleep(500);
                }
                //close application
                SendKeys.SendWait("{ENTER}");
                FreeConsole();
                Application.Exit();
            }
        }

        static void Processor_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
			// Display info from engine when task is finished
			if (e.UserState != null)
			{
				Console.WriteLine(e.UserState.ToString());
			}
        }

        static void Processor_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            // Display info from engine when task is finished
			if (e.UserState != null)
			{
				Console.WriteLine(e.UserState.ToString());
            }
            working = false;
        }       

        //Display command line help
        private static void ShowCmdLineHelp(bool includeErrorLine)
        {
            Console.WriteLine();
            //Switch help enabled
            if (!includeErrorLine)
            {
                Console.WriteLine("Patterns Offline Detection Help");
            }
            else
            {
                Console.WriteLine("Invalid arguments");
            }
            Console.WriteLine();
            Console.WriteLine("Usage: Patterns Offline Detection.exe [Options]");
            Console.WriteLine("Options");
            Console.WriteLine();
            Console.WriteLine("/?                 Display this help");
            Console.WriteLine("/i   <folder path> Folder that contains the files to process.");
            Console.WriteLine("                   This option is mandatory");
            Console.WriteLine("/xml <folder path> Process the files to xml and write them");
            Console.WriteLine("                   into the specified folder");
            Console.WriteLine("/sql               Process the files and write results into the database");
            Console.WriteLine("/recursive         Process the files in the specified folder and in its");
            Console.WriteLine("                   children folders. This parameter is optional.");
            Console.WriteLine();
            SendKeys.SendWait("{ENTER}");
            FreeConsole();
            System.Environment.Exit(1);
            Application.Exit();
        }

    }
}
