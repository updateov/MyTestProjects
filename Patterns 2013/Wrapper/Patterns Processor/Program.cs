using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using PeriGen.Patterns.Engine.Data;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace PeriGen.Patterns.Engine.Processor
{
	class Program
	{
		internal static TraceSource Source = new TraceSource("TestTool");

		static int CoreCount { get; set; }

		/// <summary>
		/// The patterns config that is embedded inside the produced XML
		/// </summary>
		const string Configuration = "contraction_rate_stage1_threshold=15\ncontraction_rate_stage2_delay=40\ncontraction_rate_upper_limit=30\ncontraction_rate_window=30\ndisplay_baseline_variability=true\ndisplay_classification=true\ndisplay_events=true\ndisplay_mhr=false\ndisplay_wcr_tracing=true\nenable_ADT=false\nenable_montevideo=true\nevent_detection_purchased=true";

		/// <summary>
		/// Main function
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			try
			{
				using (var smm = new System.Management.ManagementObjectSearcher("Select * from Win32_Processor"))
				{
					foreach (var item in smm.Get())
					{
						CoreCount += int.Parse(item["NumberOfCores"].ToString(), CultureInfo.InvariantCulture);
					}
				}
			}
			catch (Exception)
			{
				CoreCount = 1;
			}

			try
			{
				if (!EventLog.SourceExists("PeriGen Patterns Processor"))
				{
					EventLog.CreateEventSource("PeriGen Patterns Processor", "Application");
				}
				if (!EventLog.SourceExists("PeriGen Patterns Engine"))
				{
					EventLog.CreateEventSource("PeriGen Patterns Engine", "Application");
				}
			}
			catch (Exception e)
			{
				Source.TraceEvent(TraceEventType.Warning, 9001, "Warning, unable to create the log source.\n{0}", e);
			}

			var input = string.Empty;
			var output = string.Empty;
			var realtime = false;

			int count = args.GetLength(0);

			if ((count != 2) && (count != 3))
			{
				ShowHelp("Incorrect number of arguments.");
				return;
			}

			if (count == 3)
			{
				string cmd = args[0].ToUpperInvariant().Trim();
				if ((string.CompareOrdinal(cmd, "/REALTIME") != 0)
					&& (string.CompareOrdinal(cmd, "-REALTIME") != 0)
					&& (string.CompareOrdinal(cmd, "REALTIME") != 0))
				{
					ShowHelp("Invalid command line parameters. Unknown argument.");
					return;
				}
				realtime = true;
				input = args[1];
				output = args[2];
			}
			else
			{
				input = args[0];
				output = args[1];
			}

			try
			{
				input = new FileInfo(input).FullName;
			}
			catch (Exception)
			{
				ShowHelp("Invalid input file/path!");
				return;
			}
			if ((!File.Exists(input)) && (!Directory.Exists(input)))
			{
				ShowHelp("Input file/path does not exist.");
				return;
			}
			try
			{
				output = new FileInfo(output).FullName;
			}
			catch (Exception)
			{
				ShowHelp("Invalid output file/path.");
				return;
			}

			if (string.CompareOrdinal(input.ToUpperInvariant(), output.ToUpperInvariant()) == 0)
			{
				ShowHelp("Input and output files cannot be the same");
				return;
			}

			Process(input, output, realtime);

			// Done
			System.Environment.ExitCode = 0;
		}

		/// <summary>
		/// Display error message and how to use the console
		/// </summary>
		/// <param name="msg"></param>
		static void ShowHelp(string msg)
		{
			Source.TraceEvent(TraceEventType.Error, 9011, "Error: {0}", msg);

			Console.Out.WriteLine(msg);
			Console.Out.WriteLine("\nTo process a single file, you must specify a a source file and a destination file: \nPeriGen.Patterns.Engine.Processor.exe c:\\Data\\1772615.in c:\\Data\\1772615.xml\n");
			Console.Out.WriteLine("To convert recursively process a directory, you must specify\na source directory and\na destination directory: \nPeriGen.Patterns.Processor.exe c:\\Data\\ c:\\Test\\\n");
			Console.Out.WriteLine("\nIn both cases, you can ask for realtime patterns processing\nusing the REALITIME, -REALTIME or /REALTIME parameter like:\nPeriGen.Patterns.Processor.exe REALTIME c:\\Data\\ c:\\Test\\\n");
			Console.Out.WriteLine("\nPress any key to continue...");
			Console.ReadKey(true);

			System.Environment.ExitCode = -1;
		}

		// For thread safety
		static object ThreadLock = new object();

		/// <summary>
		/// Process the input file and save it as the output file
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <param name="realtime"></param>
		static void Process(string input, string output, bool realtime)
		{
			// Call on a directory?
			if ((File.GetAttributes(input) & FileAttributes.Directory) == FileAttributes.Directory)
			{
				Console.Out.WriteLine(string.Format(CultureInfo.InvariantCulture, "- {0}", input));

				// Collect current input directory data
				var files = new List<string>();
				files.AddRange(Directory.GetFiles(input, "*.v01", SearchOption.TopDirectoryOnly));
				files.AddRange(Directory.GetFiles(input, "*.xml", SearchOption.TopDirectoryOnly));
				files.AddRange(Directory.GetFiles(input, "*.in", SearchOption.TopDirectoryOnly));

				var directories = Directory.GetDirectories(input);

				// Process all files
				var queue = new Queue<string>(files);
				var threads = new List<Thread>();
				for (int i = 0; i < Math.Max(1, Math.Min(queue.Count, CoreCount)); ++i)
				{
					var thread =
						new Thread(() =>
						{
							while (true)
							{
								string file;
								lock (queue)
								{
									if (queue.Count == 0)
										break;
									file = queue.Dequeue();
								}
								Process(file, Path.Combine(output, Path.GetFileNameWithoutExtension(file) + ".xml"), realtime);
							}
						}) { Name = string.Format("Processing thread #{0}", i + 1) };
					threads.Add(thread);
				}

				foreach (var thread in threads) { thread.Start(); }
				foreach (var thread in threads) { thread.Join(); }

				// Recursive call on subdirectory
				foreach (var directory in directories)
				{
					Process(directory, Path.Combine(output, Path.GetFileName(directory)), realtime);
				}
			}

			// Call on a specific file
			else
			{
				try
				{
					// Read the data from the file
					var blocks = PeriGen.Patterns.Helper.TracingFileReader.Read(input);
					if (blocks == null)
						return;

					// Merge small blocks to optimize storage
					blocks = TracingBlock.Merge(blocks, 600);

					// Trim no data
					blocks.ForEach(b => b.Trim());

					// Clean "fake" signal at start and end
					while (blocks.Count > 0)
					{
						if (blocks[0].TotalSeconds < 10)
						{
							blocks.RemoveAt(0);
						}
						else if (blocks[blocks.Count - 1].TotalSeconds < 10)
						{
							blocks.RemoveAt(blocks.Count - 1);
						}
						else
						{
							break;
						}
					}

					if (blocks.Count == 0)
						return;

					// Process the data
					var artifacts = new List<DetectedObject>();

					foreach (var b in blocks)
					{
						b.AlignSignals();
						using (var engine = new PatternsEngineWrapper(b.Start))
						{
							var hr = b.HRs.ToArray();
							var up = b.UPs.ToArray();

							// In real time, fetch 60 seconds at a time to the engine
							if (realtime)
							{
								int index = 0;
								while (index < b.TotalSeconds)
								{
                                    var candidatesToAdd = engine.Process(hr, up, index, Math.Min(60, b.TotalSeconds - index));
                                    var toAdd = from c in candidatesToAdd
                                                where c is DetectionArtifact
                                                select c as DetectionArtifact;

									artifacts.AddRange(toAdd);
									index += 60;
								}
							}

							// In batch mode, all at once
							else
							{
								artifacts.AddRange(engine.Process(hr, up));
							}
						}
					}

					Directory.CreateDirectory(Path.GetDirectoryName(output));

					// Generate the document and save it
                    var arts = from c in artifacts
                               where c is DetectionArtifact
                               select c as DetectionArtifact;

                    PeriGen.Patterns.Helper.PatternsXMLFileWriter.Write(input, "N/A", "N/A", Configuration, blocks.Min(b => b.Start), 1, blocks, arts).Save(output);

					lock (ThreadLock)
					{
						Console.Out.WriteLine(
							"---- {0}, {1} hours of tracings, {2} baselines, {3} accelerations, {4} deceleration, {5} contractions",
							Path.GetFileName(input),
							blocks.Sum(p => p.TotalSeconds) / 3600,
                            arts.Count(a => a.IsBaseline),
                            arts.Count(a => a.IsAcceleration),
                            arts.Count(a => a.IsDeceleration),
                            arts.Count(a => a.IsContraction));
						Console.Out.Flush();
					}
				}
				catch (Exception e)
				{
					string error = string.Format(CultureInfo.InvariantCulture, "Unknow error while processing the data!\nError: {0}", e);

					lock (ThreadLock)
					{
						Source.TraceEvent(TraceEventType.Error, 9012, error);
						Console.Out.WriteLine(error);
					}
				}
			}
		}
	}
}
