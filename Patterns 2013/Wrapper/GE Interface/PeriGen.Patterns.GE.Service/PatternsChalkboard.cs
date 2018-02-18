using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeriGen.Patterns.GE.Interface;
using PeriGen.Patterns.Settings;

namespace PeriGen.Patterns.GE.Service
{
	public class PatternsChalkboard : PeriGen.Patterns.GE.Interface.Chalkboard<PatternsEpisode>, IDisposable
	{
		#region Settings

		static int Settings_OBLinkMaximumDurationWithoutRefreshToGoOffline = SettingsManager.GetInteger("OBLinkMaximumDurationWithoutRefreshToGoOffline");

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Destructor
		/// </summary>
		~PatternsChalkboard()
		{
			this.Dispose(false);
		}

		/// <summary>
		/// To remember if it was already disposed
		/// </summary>
		private bool IsDisposed;

		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this.IsDisposed)
			{
				if (disposing)
				{
					foreach (var e in this.Episodes)
					{
						e.Dispose();
					}
					this.Episodes.Clear();
				}

				this.IsDisposed = true;
			}
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		/// <summary>
		/// Check for all live episodes that in GE, their MRN is still associated with the proper bed
		/// It allows Patterns to detect a transfer faster.
		/// Without that check, it can take up to one minute to the GE side to notify us that there was a transfer and
		/// during that time, if there is tracing for that bed, we do attach that tracing to the patient that is
		/// not in the bed anymore!
		/// </summary>
		public void ValidateLivePatientLocations()
		{
			// Get the list of active patients from GE
			var patientList = RetrieveOBLinkPatientList();

			// In case it failed, just ignore it. It's already logged anyway and this is just a validation.
			if (patientList == null)
				return;

			// For fast access, map the patients from GE to a table
			var table = new Dictionary<string, string>();
			foreach (var p in patientList.Descendants("patient"))
			{
				table[p.Attribute("mrn").Value] = p.Attribute("bedid").Value;
			}

			List<PatternsEpisode> episodes;
			lock (this.EpisodesLockObject)
			{
				episodes = this.Episodes.Where(e => e.EpisodeStatus == EpisodeStatuses.Normal && !e.NeedChalkboardRefresh).ToList();
			}
			
			foreach (var e in episodes)
			{
				string bedId;
				if ((!table.TryGetValue(e.MRN, out bedId)) || (string.CompareOrdinal(bedId, e.BedId) != 0))
				{
					PatternsTask.Source.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 7350, "Episode {0} may have being transfer since the last chalkboard refresh. Suspending live until next chalkboard refresh!", e);
					e.ResetLive = true;
					e.NeedChalkboardRefresh = true;
				}
			}
		}

		/// <summary>
		/// Updates data and reconciliate. This is the main function that keeps the Chalkboard instance in sync with the GE state
		/// </summary>
		/// <returns>True if successfully refreshed</returns>
		public override bool Refresh()
		{
			if (base.Refresh())
			{
				lock (this.EpisodesLockObject)
				{
					// Close all MergeInProgress episodes
					Common.Chalkboard.Episodes.
						Where(e => (e.PatientUniqueId != 0) && e.MergeInProgress).ToList().
						ForEach(e => e.UpdateStatus(EpisodeStatuses.Closed, "A merge was detected for that episode"));

					// Close all ReconciliationConflict episodes
					Common.Chalkboard.Episodes.
						Where(e => (e.PatientUniqueId != 0) && !string.IsNullOrEmpty(e.ReconciliationConflict)).ToList().
						ForEach(e => e.UpdateStatus(EpisodeStatuses.Closed, "A conflict was detected for that episode"));

					// Reset the 'Need chalkbord refresh' since we just did a full refresh...
					Common.Chalkboard.Episodes.ForEach(e => e.NeedChalkboardRefresh = false);

					// Update performance Counters
					PerformanceCounterHelper.SetPatientsLive(Common.Chalkboard.Episodes.Count(e => e.EpisodeStatus == EpisodeStatuses.Normal));
					PerformanceCounterHelper.SetPatientsOutOfMonitor(Common.Chalkboard.Episodes.Count(e => e.EpisodeStatus == EpisodeStatuses.OutOfMonitor));
					PerformanceCounterHelper.SetPatientsTotal(Common.Chalkboard.Episodes.Count);
				}

				if (this.IsOffline)
				{
					this.IsOffline = false; 
					PatternsTask.Source.TraceEvent(System.Diagnostics.TraceEventType.Information, 7310, "Chalkboard is now online with {0} episodes", Common.Chalkboard.Episodes.Count);
				}

				return true;
			}
			
			// If we can't refresh for too long, turn offline
			if (!this.IsOffline && (Math.Abs((DateTime.UtcNow - this.LastRefreshed).TotalSeconds) > Settings_OBLinkMaximumDurationWithoutRefreshToGoOffline))
			{
				this.IsOffline = true;

				// Clear counters
				PerformanceCounterHelper.SetAverageLatencyLivePatients(0);
				PerformanceCounterHelper.SetWorstLatencyLivePatients(0);
				PerformanceCounterHelper.SetPatientsLive(0);
				PerformanceCounterHelper.SetPatientsOutOfMonitor(0);
				PerformanceCounterHelper.SetPatientsTotal(0);

				PatternsTask.Source.TraceEvent(System.Diagnostics.TraceEventType.Warning, 7311, "Chalkboard cannot be updated from OBLink for {0} seconds, going in Offline mode", (int)Math.Abs((DateTime.UtcNow - this.LastRefreshed).TotalSeconds));
			}
			return false;
		}
	}
}
