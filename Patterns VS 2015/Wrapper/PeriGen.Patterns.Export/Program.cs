using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;
using System.IO;

namespace PeriGen.Patterns.Export
{
	class Program
	{
		internal static TraceSource Source = new TraceSource("ExportTool");

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
				if(!EventLog.SourceExists("PeriGen Patterns Export"))
				{
					EventLog.CreateEventSource("PeriGen Patterns Export", "Application");
				}
			}
			catch(Exception e)
			{
				Source.TraceEvent(TraceEventType.Warning, 9001, "Warning, unable to create the log source.\n{0}", e);
			}

			var input = string.Empty;
			var output = string.Empty;

			int count = args.GetLength(0);

			if(count != 2)
			{
				ShowHelp("Invalid command line parameters. Incorrect number of arguments.");
				return;
			}

			input = args[0];
			output = args[1];

			if(string.CompareOrdinal(input.ToUpperInvariant(), output.ToUpperInvariant()) == 0)
			{
				ShowHelp("Input and output files cannot be the same");
				return;
			}

			var inputFile = new System.IO.FileInfo(input);
			if(string.CompareOrdinal(inputFile.Extension.ToUpperInvariant(), ".DB3") != 0)
			{
				ShowHelp("Valid input file extension is DB3");
				return;
			}

			if(!inputFile.Exists)
			{
				ShowHelp("Input does not exist");
				return;
			}

			var ouputFile = new System.IO.FileInfo(output);
			if((string.CompareOrdinal(ouputFile.Extension.ToUpperInvariant(), ".XML") != 0)
				&& (string.CompareOrdinal(ouputFile.Extension.ToUpperInvariant(), ".IN") != 0))
			{
				ShowHelp("Valid output file extensions are XML or IN");
				return;
			}

			Process(input, output);
		}

		/// <summary>
		/// Display error message and how to use the console
		/// </summary>
		/// <param name="error"></param>
		static void ShowHelp(string error)
		{
			Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Error: {0}\n", error));

			Source.TraceEvent(TraceEventType.Error, 9011, "Error: {0}", error);

			Console.Out.WriteLine("Valid parameters looks like\n\t\"C:\\SourceFolder\\xxx.db3\" \"C:\\DestinationFolder\\data.xml\"\"");
			Console.WriteLine("\nPress any key to continue...");
			Console.ReadKey(true);
		}

		/// <summary>
		/// The reference date for the EPOCH Unix time (http://en.wikipedia.org/wiki/Unix_time) used by the ActiveX for date/time encoding
		/// </summary>
		static DateTime EpochReferenceDateTime = new DateTime(1970, 1, 1);

		/// <summary>
		/// Convert an Epoch encoded datetime to a datetime
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		static DateTime ToDateTime(long date)
		{
			return EpochReferenceDateTime.AddSeconds(date);
		}

		/// <summary>
		/// Process the input file and save it as the output file
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <param name="realtime"></param>
		static void Process(string input, string output)
		{
			try
			{
				List<PeriGen.Patterns.Engine.Data.TracingBlock> tracings = null;
				List<PeriGen.Patterns.Engine.Data.DetectionArtifact> artifacts = null;
				Dictionary<DateTime, string> snapshots = null;
				using(var db = new DataContextEpisode.DataContextEpisode(string.Format(CultureInfo.InvariantCulture, "Data Source={0};Pooling=True;Max Pool Size=100;", input)))
				{
					tracings = PeriGen.Patterns.Engine.Data.TracingBlock.Merge(db.Tracings.Select(item => item.ToTracingBlock()).ToList(), 300);
					artifacts = db.Artifacts.Select(item => item.ToDetectionArtifact()).ToList();
					snapshots = db.CurveSnapshots.ToDictionary(item => ToDateTime(item.UpdateTime), item => item.Data);
				}

				// Generate the document and save it for Patterns
				if (string.CompareOrdinal(System.IO.Path.GetExtension(output).ToUpperInvariant(), ".IN") == 0)
				{
					System.IO.File.WriteAllText(output, PeriGen.Patterns.Helper.PatternsINFileWriter.Write(tracings));
				}
				else
				{
					var document = PeriGen.Patterns.Helper.PatternsXMLFileWriter.Write(input, "N/A", "N/A", Configuration, tracings.Min(b => b.Start), 1, tracings, artifacts);
					document.Save(output);

					if (snapshots.Count > 0)
					{
						var curves = new XElement("curves");
						foreach (var kp in snapshots.OrderByDescending(i => i.Key))
						{
							var curve = XElement.Parse(kp.Value);
							curve.Add(new XAttribute("generated", kp.Key.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));
							curves.Add(curve);
						}
						curves.Save(Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + ".curves." + ".xml"), SaveOptions.None);
					}
				}
			}
			catch(Exception e)
			{
				string error = string.Format(CultureInfo.InvariantCulture, "Unknow error while processing the data!\nError: {0}", e);

				Source.TraceEvent(TraceEventType.Error, 9012, error);

				Console.Out.WriteLine(error);
				Console.WriteLine("\nPress any key to continue...");
				Console.ReadKey(true);
			}
		}

	}
}
