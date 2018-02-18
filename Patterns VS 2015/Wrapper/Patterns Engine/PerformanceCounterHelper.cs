using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace PeriGen.Patterns.Engine
{
	public static class PerformanceCounterHelper
	{
		#region Constants for names

		const string CategoryName = "PatternsEngine";

		const string PatternsNumberOfArtifactsFoundName = "PeriCALM Patterns Engine - Number of artifacts found";
		const string PatternsSecondsOfTracingProcessedName = "PeriCALM Patterns Engine - Seconds of tracing processed";

		#endregion

		#region Members
		  
		static bool PerformanceCounterDisabled { get; set; }

		public static PerformanceCounter PatternsSecondsOfTracingProcessed { get; set; }
		public static PerformanceCounter PatternsNumberOfArtifactsFound { get; set; }

		#endregion

		#region Public initialization methods

		/// <summary>
		/// Create the performance counters
		/// </summary>
		public static void InstallPerformanceCounters()
		{
			PatternsEngineWrapper.Source.TraceEvent(TraceEventType.Verbose, 2000, "Installing PeriCALM Patterns Engine performance counters");

			// Delete if already created
			if (PerformanceCounterCategory.Exists(PerformanceCounterHelper.CategoryName))
			{
				PerformanceCounterCategory.Delete(PerformanceCounterHelper.CategoryName);
			}

			CounterCreationDataCollection list = new CounterCreationDataCollection();

			list.Add(new CounterCreationData(PerformanceCounterHelper.PatternsSecondsOfTracingProcessedName, "Seconds of tracing processed", PerformanceCounterType.NumberOfItems64));
			list.Add(new CounterCreationData(PerformanceCounterHelper.PatternsNumberOfArtifactsFoundName, "Number of artifacts found", PerformanceCounterType.NumberOfItems64));

			PerformanceCounterCategory.Create(PerformanceCounterHelper.CategoryName, PerformanceCounterHelper.CategoryName, PerformanceCounterCategoryType.SingleInstance, list);

			PatternsEngineWrapper.Source.TraceEvent(TraceEventType.Verbose, 2001, "First time PeriCALM Patterns Engine performance installed");
		}

		/// <summary>
		/// Initialize the performance counters, must be called by whoever is using this server to activate 
		/// </summary>
		public static void InitializePerformanceCounters()
		{
			try
			{
				// Make sure we don't call that method twice
				if (PerformanceCounterHelper.PatternsSecondsOfTracingProcessed != null)
				{
					return;
				}

				PatternsEngineWrapper.Source.TraceEvent(TraceEventType.Verbose, 2004, "Initialization of PeriCALM Patterns Engine performance");

				// Install them if necessary
				if (!PerformanceCounterCategory.Exists(PerformanceCounterHelper.CategoryName))
				{
					PerformanceCounterHelper.InstallPerformanceCounters();
				}

				PerformanceCounterHelper.PatternsNumberOfArtifactsFound = new PerformanceCounter(PerformanceCounterHelper.CategoryName, PerformanceCounterHelper.PatternsNumberOfArtifactsFoundName, false);
				PerformanceCounterHelper.PatternsSecondsOfTracingProcessed = new PerformanceCounter(PerformanceCounterHelper.CategoryName, PerformanceCounterHelper.PatternsSecondsOfTracingProcessedName, false);

				PatternsEngineWrapper.Source.TraceEvent(TraceEventType.Verbose, 2005, "PeriCALM Patterns Engine performance initialized");

				PerformanceCounterHelper.PerformanceCounterDisabled = false;
			}
			catch (Exception e)
			{
				PatternsEngineWrapper.Source.TraceEvent(TraceEventType.Error, 2012, "PeriCALM Patterns Engine performance cannot be initialized. Performance counter feature turned off.\nException: {0}", e);
				PerformanceCounterHelper.PerformanceCounterDisabled = true;
			}
		}

		#endregion

		#region Public Counter helpers

		/// <summary>
		/// Monitor seconds of tracings
		/// </summary>
		/// <param name="secondsProcessed">Number of seconds of tracing processed</param>
		public static void AddSecondsOfTracingProcessed(long secondsProcessed) 		
		{
			if (!PerformanceCounterHelper.PerformanceCounterDisabled)
			{
				PerformanceCounterHelper.PatternsSecondsOfTracingProcessed.IncrementBy(secondsProcessed);
			}
		}
		/// <summary>
		/// Monitor artifacts
		/// </summary>
		/// <param name="number"></param>
		public static void AddArtifactsFound(long number)
		{
			if (!PerformanceCounterHelper.PerformanceCounterDisabled)
			{
				PerformanceCounterHelper.PatternsNumberOfArtifactsFound.IncrementBy(number);
			}
		}

		#endregion
	}
}
