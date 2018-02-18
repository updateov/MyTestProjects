using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace PeriGen.Patterns.GE.Service
{
	public static class PerformanceCounterHelper
	{
		#region Constants for names

		const string CategoryName = "PatternsService";

		const string PatternsNumberOfLivePatients = "PeriCALM Patterns Service - Number of patients live";
		const string PatternsNumberOfOutOfMonitorPatients = "PeriCALM Patterns Service - Number of patients out of monitor";
		const string PatternsTotalPatients = "PeriCALM Patterns Service - Patients total";

		const string PatternsTotalPatientsOpened = "PeriCALM Patterns Service - Patients total opened";
		const string PatternsTotalUserActions = "PeriCALM Patterns Service - User actions total";

		const string PatternsAverageLatencyLivePatients = "PeriCALM Patterns Service - Average latency live patients in milliseconds";
		const string PatternsLatencyLivePatientsWorst = "PeriCALM Patterns Service - Latency live patients in milliseconds (worst)";

		#endregion

		#region Members

		static bool PerformanceCounterDisabled { get; set; }

		public static PerformanceCounter PatternsNumberOfLivePatientsCounter { get; set; }
		public static PerformanceCounter PatternsNumberOfOutOfMonitorPatientsCounter { get; set; }
		public static PerformanceCounter PatternsTotalPatientsCounter { get; set; }

		public static PerformanceCounter PatternsTotalPatientsOpenedCounter { get; set; }
		public static PerformanceCounter PatternsTotalUserActionsCounter { get; set; }

		public static PerformanceCounter PatternsAverageLatencyLivePatientsCounter { get; set; }
		public static PerformanceCounter PatternsLatencyLivePatientsWorstCounter { get; set; }

		#endregion

		#region Public initialization methods

		/// <summary>
		/// Create the performance counters
		/// </summary>
		public static void InstallPerformanceCounters()
		{
			PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 2000, "Installing PeriCALM Patterns Service performance counters");

			// Delete if already created
			if (PerformanceCounterCategory.Exists(PerformanceCounterHelper.CategoryName))
			{
				PerformanceCounterCategory.Delete(PerformanceCounterHelper.CategoryName);
			}

			CounterCreationDataCollection list = new CounterCreationDataCollection();

			list.Add(new CounterCreationData(PerformanceCounterHelper.PatternsNumberOfLivePatients, "Number of live patients", PerformanceCounterType.NumberOfItems64));
			list.Add(new CounterCreationData(PerformanceCounterHelper.PatternsNumberOfOutOfMonitorPatients, "Number of out of monitor patients", PerformanceCounterType.NumberOfItems64));
			list.Add(new CounterCreationData(PerformanceCounterHelper.PatternsTotalPatients, "Number of patients", PerformanceCounterType.NumberOfItems64));

			list.Add(new CounterCreationData(PerformanceCounterHelper.PatternsTotalPatientsOpened, "Number of time Patterns was opened", PerformanceCounterType.NumberOfItems64));
			list.Add(new CounterCreationData(PerformanceCounterHelper.PatternsTotalUserActions, "Number of actions performed", PerformanceCounterType.NumberOfItems64));

			list.Add(new CounterCreationData(PerformanceCounterHelper.PatternsAverageLatencyLivePatients, "Average live tracing latency", PerformanceCounterType.NumberOfItems64));
			list.Add(new CounterCreationData(PerformanceCounterHelper.PatternsLatencyLivePatientsWorst, "Worst live tracing latency", PerformanceCounterType.NumberOfItems64));

			PerformanceCounterCategory.Create(PerformanceCounterHelper.CategoryName, PerformanceCounterHelper.CategoryName, PerformanceCounterCategoryType.SingleInstance, list);

			PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 2001, "First time PeriCALM Patterns Service performance installation");
		}

		/// <summary>
		/// Initialize the performance counters, must be called by whoever is using this server to activate 
		/// </summary>
		public static void InitializePerformanceCounters()
		{
			try
			{	
				// Make sure we don't call that method twice
				if (PerformanceCounterHelper.PatternsNumberOfLivePatientsCounter != null)
				{
					return;
				}

				PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 2004, "Initialization of PeriCALM Patterns Service performance");

				// Install them if necessary
				if (!PerformanceCounterCategory.Exists(PerformanceCounterHelper.CategoryName))
				{
					PerformanceCounterHelper.InstallPerformanceCounters();
				}

				PerformanceCounterHelper.PatternsNumberOfLivePatientsCounter = new PerformanceCounter(PerformanceCounterHelper.CategoryName, PerformanceCounterHelper.PatternsNumberOfLivePatients, false);
				PerformanceCounterHelper.PatternsNumberOfOutOfMonitorPatientsCounter = new PerformanceCounter(PerformanceCounterHelper.CategoryName, PerformanceCounterHelper.PatternsNumberOfOutOfMonitorPatients, false);
				PerformanceCounterHelper.PatternsTotalPatientsCounter = new PerformanceCounter(PerformanceCounterHelper.CategoryName, PerformanceCounterHelper.PatternsTotalPatients, false);

				PerformanceCounterHelper.PatternsTotalPatientsOpenedCounter = new PerformanceCounter(PerformanceCounterHelper.CategoryName, PerformanceCounterHelper.PatternsTotalPatientsOpened, false);
				PerformanceCounterHelper.PatternsTotalUserActionsCounter = new PerformanceCounter(PerformanceCounterHelper.CategoryName, PerformanceCounterHelper.PatternsTotalUserActions, false);

				PerformanceCounterHelper.PatternsAverageLatencyLivePatientsCounter = new PerformanceCounter(PerformanceCounterHelper.CategoryName, PerformanceCounterHelper.PatternsAverageLatencyLivePatients, false);
				PerformanceCounterHelper.PatternsLatencyLivePatientsWorstCounter = new PerformanceCounter(PerformanceCounterHelper.CategoryName, PerformanceCounterHelper.PatternsLatencyLivePatientsWorst, false);

				PatternsTask.Source.TraceEvent(TraceEventType.Verbose, 2005, "PeriCALM Patterns Service performance initialized");

				PerformanceCounterHelper.PerformanceCounterDisabled = false;
			}
			catch (Exception e)
			{
				PatternsTask.Source.TraceEvent(TraceEventType.Error, 2012, "PeriCALM Patterns Service performance cannot be initialized. Performance counter feature turned off.\nException: {0}", e);
				PerformanceCounterHelper.PerformanceCounterDisabled = true;
			}
		}

		#endregion

		#region Public Counter helpers


		/// <summary>
		/// Monitor live patients
		/// </summary>
		/// <param name="number"></param>
		public static void SetPatientsLive(long number)
		{
			if (!PerformanceCounterHelper.PerformanceCounterDisabled)
			{
				PerformanceCounterHelper.PatternsNumberOfLivePatientsCounter.RawValue = number;
			}
		}

		/// <summary>
		/// Monitor out of monitor
		/// </summary>
		/// <param name="number"></param>
		public static void SetPatientsOutOfMonitor(long number)
		{
			if (!PerformanceCounterHelper.PerformanceCounterDisabled)
			{
				PerformanceCounterHelper.PatternsNumberOfOutOfMonitorPatientsCounter.RawValue = number;
			}
		}

		/// <summary>
		/// Monitor Patient Total
		/// </summary>
		/// <param name="number"></param>
		public static void SetPatientsTotal(long number)
		{
			if (!PerformanceCounterHelper.PerformanceCounterDisabled)
			{
				PerformanceCounterHelper.PatternsTotalPatientsCounter.RawValue = number;
			}
		}

		/// <summary>
		/// Monitor Patients opened
		/// </summary>
		/// <param name="number"></param>
		public static void AddPatientsOpened(long number)
		{
			if (!PerformanceCounterHelper.PerformanceCounterDisabled)
			{
				PerformanceCounterHelper.PatternsTotalPatientsOpenedCounter.IncrementBy(number);
			}
		}

		/// <summary>
		/// Monitor user actions
		/// </summary>
		/// <param name="number"></param>
		public static void AddUserActions(long number)
		{
			if (!PerformanceCounterHelper.PerformanceCounterDisabled)
			{
				PerformanceCounterHelper.PatternsTotalUserActionsCounter.IncrementBy(number);
			}
		}

		/// <summary>
		/// Monitor Average Latency patients
		/// </summary>
		/// <param name="number"></param>
		public static void SetAverageLatencyLivePatients(long number)
		{
			if (!PerformanceCounterHelper.PerformanceCounterDisabled)
			{
				PerformanceCounterHelper.PatternsAverageLatencyLivePatientsCounter.RawValue = number;
			}
		}

		/// <summary>
		/// Monitor worst Latency patients
		/// </summary>
		/// <param name="number"></param>
		public static void SetWorstLatencyLivePatients(long number)
		{
			if (!PerformanceCounterHelper.PerformanceCounterDisabled)
			{
				PerformanceCounterHelper.PatternsLatencyLivePatientsWorstCounter.RawValue = number;
			}
		}


		#endregion
	}
}
