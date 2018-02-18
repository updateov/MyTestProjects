using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PeriGen.Patterns.ActiveXInterface;
using PeriGen.Patterns.Engine.Data;

namespace PeriGen.Patterns
{
	/// <summary>
	/// A very simple "repository" class that simulate a database and contains ONE patient with tracings, artifacts and user actions
	/// Security or performances are not the main target of that class
	/// </summary>
	public static class Repository
	{
		/// <summary>
		/// For trace
		/// </summary>
		internal static TraceSource Source = new TraceSource("TestActiveX");

		/// <summary>
		/// For thread safety
		/// </summary>
		public static object LockObject = new object();

		/// <summary>
		/// Statis constructor
		/// </summary>
		static Repository()
		{
			Reset();
		}

		/// <summary>
		/// To switch between demo and registered version
		/// </summary>
		public static bool DemoMode { get; set; }

		/// <summary>
		/// The patient's data
		/// </summary>
		public static XPatient Patient { get; set; }

		/// <summary>
		/// List of Tracing blocks
		/// </summary>
		public static List<TracingBlock> TracingBlocks { get; private set; }

		/// <summary>
		/// List of artifacts
		/// </summary>
		public static List<DetectionArtifact> TracingArtifacts { get; private set; }

		/// <summary>
		/// List of actions
		/// </summary>
		public static List<XUserAction> TracingActions { get; private set; }

		/// <summary>
		/// Add some more tracings to the repository
		/// </summary>
		/// <param name="list"></param>
		public static void AddTracings(IEnumerable<TracingBlock> list)
		{
			if ((list == null) || (list.Count() == 0))
				return;

			lock (LockObject)
			{
				Source.TraceEvent(TraceEventType.Verbose, 9421, "Repository: storing {0} blocks of tracings for a total of {1} seconds of data", list.Count(), list.Sum(b => b.TotalSeconds));

				// Set new ID...
				var lastId = TracingBlocks.Count() == 0 ? 0 : TracingBlocks.Max(a => a.Id);
				foreach (var item in list.OrderBy(t => t.Start))
				{
					item.Id = ++lastId;
				}

				// Add all...
				TracingBlocks.AddRange(list);
			}
		}

		/// <summary>
		/// Add some more artifacts to the repository
		/// </summary>
		/// <param name="list"></param>
		public static void AddArtifacts(IEnumerable<DetectionArtifact> list)
		{
			if ((list == null) || (list.Count() == 0))
				return;

			lock (LockObject)
			{
				Source.TraceEvent(TraceEventType.Verbose, 9422, "Repository: storing {0} artifacts (B:{1} A:{2} D:{3} C:{4})", list.Count(), list.Count(a => a.IsBaseline), list.Count(a => a.IsAcceleration), list.Count(a => a.IsDeceleration), list.Count(a => a.IsContraction));

				// Set new ID...
				var lastId = TracingArtifacts.Count() == 0 ? 0 : TracingArtifacts.Max(a => a.Id);
				foreach (var item in list)
				{
					item.Id = ++lastId;
				}

				// Add all...
				TracingArtifacts.AddRange(list);
			}
		}

		/// <summary>
		/// Add some more actions to the repository
		/// </summary>
		/// <param name="list"></param>
		public static void AddActions(IEnumerable<XUserAction> list)
		{
			if ((list == null) || (list.Count() == 0))
				return;

			lock (LockObject)
			{
				Source.TraceEvent(TraceEventType.Verbose, 9423, "Repository: storing {0} user actions", list.Count());

				// Set new ID...
				var lastId = TracingActions.Count() == 0 ? 0 : TracingActions.Max(a => a.Id);
				foreach (var item in list)
				{
					item.Id = ++lastId;
				}

				// Add all...
				TracingActions.AddRange(list);
			}
		}

		/// <summary>
		/// Reset the repository to empty content
		/// </summary>
		public static void Reset()
		{
			lock (LockObject)
			{
				Source.TraceEvent(TraceEventType.Verbose, 9424, "Repository: reset");

				TracingBlocks = new List<TracingBlock>();
				TracingArtifacts = new List<DetectionArtifact>();
				TracingActions = new List<XUserAction>();
			}
		}
	}
}
