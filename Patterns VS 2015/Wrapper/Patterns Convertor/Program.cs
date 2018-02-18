using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PeriGen.Patterns.Engine.Data;

namespace PeriGen.Patterns.ConvertTracings
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{

			int count = args.GetLength(0);
			if (count != 3)
			{
				ShowHelp("Incorrect number of arguments.");
				return;
			}

			var format = args[0].ToUpperInvariant().Trim();
			if ((string.CompareOrdinal(format, "IN") != 0) && (string.CompareOrdinal(format, "XML") != 0))
			{
				ShowHelp("Unknow target format, must be either IN or XML.");
				return;
			}

			var input = args[1];
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

			var output = args[2];
			try
			{
				output = new FileInfo(output).FullName;
			}
			catch (Exception)
			{
				ShowHelp("Invalid output file/path.");
				return;
			}


			// Process!
			Process(input, output, format);

			// Done
			System.Environment.ExitCode = 0;
		}

		static void Process(string input, string output, string format)
		{
			// Call on a directory?
			if ((File.GetAttributes(input) & FileAttributes.Directory) == FileAttributes.Directory)
			{
				Console.Out.WriteLine(string.Format(CultureInfo.InvariantCulture, "- {0} --> {1}", input, output));

				// Collect current input directory data
				var files = new List<string>();
				files.AddRange(Directory.GetFiles(input, "*.v01", SearchOption.TopDirectoryOnly));
				files.AddRange(Directory.GetFiles(input, "*.xml", SearchOption.TopDirectoryOnly));
				files.AddRange(Directory.GetFiles(input, "*.in", SearchOption.TopDirectoryOnly));

				var directories = Directory.GetDirectories(input);

				// Process all files
				foreach (var file in files)
				{
					Process(file, Path.Combine(output, Path.GetFileNameWithoutExtension(file) + "." + format), format);
				}

				// Recursive call on subdirectory
				foreach (var directory in directories)
				{
					Process(directory, Path.Combine(output, Path.GetFileName(directory)), format);
				}
			}

			// Call on a specific file
			else
			{
				Console.Out.Write(string.Format(CultureInfo.InvariantCulture, "---- {0} --> {1}", Path.GetFileNameWithoutExtension(input), Path.GetFileNameWithoutExtension(output)));

				// Read the data from the file
				var blocks = PeriGen.Patterns.Helper.TracingFileReader.Read(input);
				if (blocks == null)
				{
					Console.Out.WriteLine(" --> EMPTY or INVALID file");
					return;
				}

				// Merge small blocks to optimize storage
				blocks = TracingBlock.Merge(blocks, 60);
				
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
				{
					Console.Out.WriteLine(" --> EMPTY or INVALID file");
					return;
				}

				Directory.CreateDirectory(Path.GetDirectoryName(output));
				if (string.CompareOrdinal(format, "XML") == 0)
				{
					// Encode as XML and save in the target file
					PeriGen.Patterns.Helper.PatternsXMLFileWriter.Write(Path.GetFileNameWithoutExtension(output), "n/a", "n/a", string.Empty, blocks.Count > 0 ? blocks.Min(b => b.Start) : (Nullable<DateTime>)null, 1, blocks, new List<DetectionArtifact>()).Save(output);
				}
				else if (string.CompareOrdinal(format, "IN") == 0)
				{
					File.WriteAllText(output, PeriGen.Patterns.Helper.PatternsINFileWriter.Write(blocks), System.Text.Encoding.ASCII);
				}

				Console.Out.WriteLine(" --> {0} hours of tracings", blocks.Sum(p => p.TotalSeconds) / 3600);
			}
		}

		/// <summary>
		/// Display help on how to use that tool
		/// </summary>
		/// <param name="msg"></param>
		static void ShowHelp(string msg)
		{
			Console.Out.WriteLine(msg);
			Console.Out.WriteLine("\nTo convert a single file, you must specify a target format \n(IN or XML), a source file and a destination file: \nPeriGen.Patterns.ConvertTracings.exe XML c:\\Data\\1772615.in c:\\Data\\1772615.xml\n");
			Console.Out.WriteLine("To convert recursively process a directory, you must specify\na target format (IN or XML), a source directory and\na destination directory: \nPeriGen.Patterns.ConvertTracings.exe IN c:\\Data\\ c:\\Test\\\n");
			Console.Out.WriteLine("\nPress any key to continue...");
			Console.ReadKey(true);

			System.Environment.ExitCode = -1;
		}
	}
}
