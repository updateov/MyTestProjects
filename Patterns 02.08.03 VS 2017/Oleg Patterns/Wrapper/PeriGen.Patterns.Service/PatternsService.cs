using System.ServiceProcess;

namespace PeriGen.Patterns.Service
{
	partial class PatternsService : ServiceBase
	{
		/// <summary>
		/// The name of the service
		/// </summary>
		public const string PatternsServiceName = "PeriGenPatternsService";

		/// <summary>
		/// For thread safety
		/// </summary>
		object ThreadLock = new object();

		/// <summary>
		/// Constructor
		/// </summary>
		public PatternsService()
		{
			this.InitializeComponent();
			this.CanPauseAndContinue = false;
			this.CanShutdown = true;
			this.Task = new PatternsTask();
			
			// Initialize performance counters
			PerformanceCounterHelper.InitializePerformanceCounters();
		}

		/// <summary>
		/// The task instance
		/// </summary>
		PatternsTask Task { get; set; }

		/// <summary>
		/// Start the service
		/// </summary>
		/// <param name="args"></param>
		protected override void OnStart(string[] args)
		{
			lock (this.ThreadLock)
			{
				if (!this.StartTask())
				{
					this.Stop();
				}
			}
		}

		/// <summary>
		/// Stop the service
		/// </summary>
		protected override void OnStop()
		{
			lock (this.ThreadLock)
			{
				this.StopTask();
			}
		}

		/// <summary>
		/// System is shutting down
		/// </summary>
		protected override void OnShutdown()
		{
			base.OnShutdown();
			this.StopTask();
		}

		/// <summary>
		/// Start the task
		/// </summary>
		/// <returns>True if successful</returns>
		public bool StartTask()
		{
			if (this.Task.IsStarted)
			{
				this.StopTask();
			}
			return this.Task.Start();
		}

		/// <summary>
		/// Stop the task
		/// </summary>
		public void StopTask()
		{
			if (this.Task.IsStarted)
			{
				this.Task.Stop();
			}
		}
	}
}
