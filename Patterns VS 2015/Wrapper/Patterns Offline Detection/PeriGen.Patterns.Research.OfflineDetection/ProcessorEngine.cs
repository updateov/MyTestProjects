using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Reflection;
using PeriGen.Patterns.Engine.Data;
using PeriGen.Patterns.Helper;

namespace PeriGen.Patterns.Research.OfflineDetection
{
	public class ProcessorEngine : BackgroundWorker
	{
		internal static TraceSource Trace = new TraceSource("OfflineProcessor");

		//variables for progress
		int ProcessedFilesCount { get; set; }
		int TotalFilesCount { get; set; }
		int ErrorFilesCount { get; set; }

		// Used to lock multithread access
		private readonly object LockObject = new object();

		//dictionary of threds
		Dictionary<String, BackgroundWorker> RunningWorkers = new Dictionary<string, BackgroundWorker>();

		/// <summary>
		/// Parameters used to process files
		/// </summary>
		public ProcessFilesArgs Args { get; set; }

		public ProcessorEngine()
		{
			this.WorkerReportsProgress = true;
			this.WorkerSupportsCancellation = true;
		}

		public ProcessorEngine(ProcessFilesArgs args)
			: this()
		{
			Args = args;
		}

		/// <summary>
		/// Start processing files when RunWorkerAsync is called
		/// </summary>
		/// <param name="e"></param>
		protected override void OnDoWork(DoWorkEventArgs e)
		{
			try
			{
				//Check parameters
				if (Args == null)
				{
					throw new ArgumentException("Parameters missing");
				}

				//initialize counters
				ProcessedFilesCount = 0;
				ErrorFilesCount = 0;
				TotalFilesCount = 0;

				//make room for files
				FileInfo[] files;

				//check kind of task
				if (!Args.ProcessOneFile)
				{
					//Process folder
					var dir = new DirectoryInfo(Args.SourceFolder);

					// Recursive?
					if (!Args.IsRecursive)
					{
						//Single folder
						files = dir.GetFiles("*.in", SearchOption.TopDirectoryOnly)
									.Union(dir.GetFiles("*.xml", SearchOption.TopDirectoryOnly))
									.Union(dir.GetFiles("*.v01", SearchOption.TopDirectoryOnly)).ToArray();
					}
					else
					{
						//Recursive Folders
						files = dir.GetFiles("*.in", SearchOption.AllDirectories)
									.Union(dir.GetFiles("*.xml", SearchOption.AllDirectories))
									.Union(dir.GetFiles("*.v01", SearchOption.AllDirectories)).ToArray();
					}

					// Check files availables
					if (files.Count() == 0)
					{
						throw new ArgumentException("There are no compatible files in that directory, supported types are xml, v01 and in.");
					}
				}
				else
				{
					//only one file
					files = new FileInfo[] { new FileInfo(Args.FileToProcess) };
				}

				//Create target folder for XML options if is necessary
				if (Args.SaveToXML && !Directory.Exists(Args.TargetFolder))
				{
					Directory.CreateDirectory(Args.TargetFolder);
				}

				//Transfer files in queue before start processing them
				Queue<FileInfo> queue = new Queue<FileInfo>(files);
				TotalFilesCount = queue.Count;

				// report start
				this.ReportProgress(0, "Starting to process files...");
				ProcessorEngine.Trace.TraceEvent(TraceEventType.Information, 1001, "Detection starting...");

				var numberOfThreads = System.Environment.ProcessorCount;

				//process while there are files in the queue
				while ((queue.Count > 0) && !this.CancellationPending)
				{
					if (RunningWorkers.Count < numberOfThreads)
					{
						// Create new worker
						BackgroundWorker worker = new BackgroundWorker();
						worker.WorkerReportsProgress = true;
						worker.WorkerSupportsCancellation = true;
						worker.RunWorkerCompleted += worker_RunWorkerCompleted;
						worker.DoWork += worker_DoWork;

						// Create parameters
						WorkerParameters wp = new WorkerParameters();
						wp.FileToProcess = queue.Dequeue();
						wp.SaveToSQL = Args.SaveToSQL;
						wp.SaveToXML = Args.SaveToXML;
						wp.SourceFolder = Args.SourceFolder;
						wp.TargetFolder = Args.TargetFolder;

						lock (RunningWorkers)
						{
							// Add to list
							RunningWorkers.Add(wp.FileToProcess.FullName, worker);
						}

						// Start processing file
						worker.RunWorkerAsync(wp);
					}
					else
					{
						Thread.Sleep(50);
					}
				}

				//check if user ask for cancellation
				if (this.CancellationPending)
				{
					// Stop Threads Working
					var items = RunningWorkers.ToArray();
					foreach (var item in items)
					{
						item.Value.CancelAsync();
					}
					while (RunningWorkers.Count > 0)
					{
						Thread.Sleep(100);
					}

					// Cancelled
					ProcessorEngine.Trace.TraceEvent(TraceEventType.Warning, 1002, "Operation cancelled by user.");
					e.Result = "Operation cancelled by user.";
				}
				else
				{
					// Wait for threads before terminate
					while (RunningWorkers.Count > 0)
					{
						Thread.Sleep(100);
					}

					// Completed
					string msg = string.Format(CultureInfo.InvariantCulture, "Processing of {0} files completed. {1} error(s).", this.TotalFilesCount, this.ErrorFilesCount);
					ProcessorEngine.Trace.TraceEvent(TraceEventType.Information, 1003, msg);
					e.Result = msg;
				}
			}
			catch (Exception ex)
			{
				// Before raise error finish threads still working                
				var items = RunningWorkers.ToArray();
				foreach (var item in items)
				{
					item.Value.CancelAsync();
				}
				while (RunningWorkers.Count > 0)
				{
					Thread.Sleep(100);
				}

				ProcessorEngine.Trace.TraceEvent(TraceEventType.Error, 1004, "Error while processing files. Operation cancelled.\nError details: {0}", ex);
				e.Result = "Error while processing files. Operation cancelled.\nError details: " + ex.Message;
			}
		}

		#region Worker Methods

		/// <summary>
		/// The main worker method, called in thread. Do the Patterns detection!
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			//Parameters
			BackgroundWorker worker = (BackgroundWorker)sender;
			WorkerParameters args = (WorkerParameters)e.Argument;
			try
			{
				// Read the file and merge blocks to improve detection performances
				var blocks = TracingBlock.Merge(TracingFileReader.Read(args.FileToProcess.FullName), 600);

				// Detection now !
				var results = new List<DetectionArtifact>();

				foreach (TracingBlock block in blocks)
				{
					// Check for cancellation
					if (worker.CancellationPending)
					{
						e.Result = new ProcessWorkerResult { Filename = args.FileToProcess.FullName, Status = ProcessWorkerResult.ProcessStatuses.Cancelled, Message = "Operation cancelled"};
						return;
					}

					// Skip very small blocks
					if (block.TotalSeconds < 30)
					{
						continue;
					}

					using (var engine = new PeriGen.Patterns.Engine.PatternsEngineWrapper(block.Start))
					{
                        var arts = new List<PeriGen.Patterns.Engine.Data.DetectedObject>();
                        arts.AddRange(engine.Process(block.HRs.ToArray(), block.UPs.ToArray()));
                        results.AddRange(from c in arts where c is DetectionArtifact select c as DetectionArtifact);
					}
				}

				var patientID = args.FileToProcess.FullName;
				if (args.SaveToSQL)
				{
					lock (LockObject)
					{
						/// Save in the DB                                
						DataUtils.PurgePatient(patientID);
						DataUtils.SaveData(patientID, blocks, results);
					}
				}

				if (args.SaveToXML)
				{
					var outputFolder = Path.GetDirectoryName(args.FileToProcess.FullName).Replace(args.SourceFolder, args.TargetFolder);

					lock (LockObject)
					{
						if (!Directory.Exists(outputFolder))
						{
							Directory.CreateDirectory(outputFolder);
						}
					}
					string outputFile = Path.ChangeExtension(Path.Combine(outputFolder, args.FileToProcess.Name), "xml");

					/// Save in an XML file
					var doc = PeriGen.Patterns.Helper.PatternsXMLFileWriter.Write(patientID, "n/a", "n/a", Properties.Resources.patterns, blocks.Min(b => b.Start), 1, blocks, results);
					doc.Save(outputFile);
				}

				// Success
				e.Result = new ProcessWorkerResult { Filename = args.FileToProcess.FullName, Status = ProcessWorkerResult.ProcessStatuses.Success};
			}
			catch (Exception ex)
			{
				// Catch error and cancel operation
				e.Result = new ProcessWorkerResult { Filename = args.FileToProcess.FullName, Status = ProcessWorkerResult.ProcessStatuses.Error, Message = ex.ToString() };
			}
		}

		/// <summary>
		/// A worker reported completion
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//Get operation result
			var result = (ProcessWorkerResult)e.Result;

			lock (RunningWorkers)
			{
				++ProcessedFilesCount;

				// Cancelled?
				string msg;
				if (result.Status == ProcessWorkerResult.ProcessStatuses.Cancelled)
				{
					msg = string.Format(CultureInfo.InvariantCulture, "Processing of file {0} cancelled", result.Filename);
					ProcessorEngine.Trace.TraceEvent(TraceEventType.Verbose, 1005, msg);
				}
				else if (result.Status == ProcessWorkerResult.ProcessStatuses.Error)
				{
					msg = string.Format(CultureInfo.InvariantCulture, ">>Error while processing file {0}<<", result.Filename);
					ProcessorEngine.Trace.TraceEvent(TraceEventType.Error, 1006, msg);
					++ErrorFilesCount;
				}
				else if (result.Status == ProcessWorkerResult.ProcessStatuses.Success)
				{
					msg = string.Format(CultureInfo.InvariantCulture, "File {0} successfully processed", result.Filename);
					ProcessorEngine.Trace.TraceEvent(TraceEventType.Verbose, 1007, msg);
				}
				else
				{
					msg = string.Format(CultureInfo.InvariantCulture, "Unknown status for file {0}", result.Filename);
					ProcessorEngine.Trace.TraceEvent(TraceEventType.Error, 1006, msg);
					++ErrorFilesCount;
				}

				this.ReportProgress(ProcessedFilesCount * 100 / TotalFilesCount, msg);

				RunningWorkers.Remove(result.Filename);
			}
		}

		#endregion

		/// <summary>
		/// Result for internal workers
		/// </summary>
		private class ProcessWorkerResult
		{
			public enum ProcessStatuses { None = 0, Success, Error, Cancelled };

			public string Message { get; set; }
			public string Filename { get; set; }
			public ProcessStatuses Status { get; set; }
		}

		/// <summary>
		/// Used as parameters to process the files
		/// </summary>
		public class ProcessFilesArgs
		{
			//one file?
			public Boolean ProcessOneFile { get; set; }
			//File to process if is oneFile
			public String FileToProcess { get; set; }
			//Source folder where files are stored
			public String SourceFolder { get; set; }
			//must save in xml?
			public Boolean SaveToXML { get; set; }
			//Target folder used if must save as xml
			public String TargetFolder { get; set; }
			//Must save to database?
			public Boolean SaveToSQL { get; set; }
			//must process subfolders?
			public Boolean IsRecursive { get; set; }
		}

		/// <summary>
		/// used to pass information to internal workers
		/// </summary>
		private class WorkerParameters
		{
			public string SourceFolder { get; set; }
			public string TargetFolder { get; set; }
			public FileInfo FileToProcess { get; set; }
			public Boolean SaveToSQL { get; set; }
			public Boolean SaveToXML { get; set; }
		}
	}
}
