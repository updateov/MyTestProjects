using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;
using PeriGen.Patterns;
using PeriGen.Patterns.ActiveXInterface;
using PeriGen.Patterns.Engine.Data;
using PeriGen.Patterns.Helper;

namespace TestPaternsActiveX
{
	public partial class frmMain : Form
	{
		public frmMain()
		{
			try
			{
				if (!EventLog.SourceExists("PeriGen Patterns TestActiveX"))
				{
					EventLog.CreateEventSource("PeriGen Patterns TestActiveX", "Application");
				}
				if (!EventLog.SourceExists("PeriGen Patterns Engine"))
				{
					EventLog.CreateEventSource("PeriGen Patterns Engine", "Application");
				}

			}
			catch (Exception e)
			{
				Repository.Source.TraceEvent(TraceEventType.Warning, 9001, "Warning, unable to create the log source.\n{0}", e);
			}

			// Check the current license and apply a demo one if none found
			if (!PeriGen.Patterns.Engine.LicenseValidation.HasValidLicense)
			{
				PeriGen.Patterns.Engine.LicenseValidation.EnableDemoMode();
			}

			InitializeComponent();
			this.txtURLUserPermission.SelectedIndex = 1;
			this.cboViewerBanner.SelectedIndex = 0;
			var date = DateTime.UtcNow.AddDays(14);
			this.dtpPatientEDD.Value = new DateTime(date.Year, date.Month, date.Day);
			this.dtpPatientReset.CustomFormat = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern;
			this.dtpPatientReset.Checked = false;
			this.dtpPatientReset.Value = DateTime.UtcNow;
			Repository.Patient = new XPatient { PatientId = (int)DateTime.UtcNow.ToEpoch() };

			UpdateUI();
		}

		/// <summary>
		/// The data read from the file
		/// </summary>
		TracingBlock TracingData;

		/// <summary>
		/// The tracing simulation data start time
		/// </summary>
		DateTime TracingStart;

		/// <summary>
		/// The last time sent from the tracing data
		/// </summary>
		DateTime TracingTime;

		/// <summary>
		/// The first unsent index from the tracing data
		/// </summary>
		int TracingIndex;

		/// <summary>
		/// Monitor Power OFF
		/// </summary>
		volatile bool MonitorPowerOFF = false;

		/// <summary>
		/// Late processing realtime tracings
		/// </summary>
		volatile bool LateRealTime = false;

		/// <summary>
		/// Recovery mode 0N
		/// </summary>
		volatile bool RecoveryModeON = false;

		/// <summary>
		/// Reset requested
		/// </summary>
		volatile bool ResetRequested = false;

		/// <summary>
		/// Siimulation started
		/// </summary>
		volatile bool SimulationStarted = false;

		/// <summary>
		/// Mom unplugged?
		/// </summary>
		volatile bool ProbesUnplugged = false;

		/// <summary>
		/// Server disconnected?
		/// </summary>
		volatile bool ServerDisconnected = false;

		/// <summary>
		/// Enable / Disable the button and adjust their texts
		/// </summary>
		void UpdateUI()
		{
			if (!this.IsDisposed)
			{
				if (this.InvokeRequired)
				{
					this.BeginInvoke((Action)delegate() { this.UpdateUI(); });
				}
				else
				{
					btnSelectFile.Enabled = !this.SimulationStarted;

					btnStartStop.Enabled = (this.TracingData != null);
					btnStartStop.Text = this.SimulationStarted ? "Stop simulation" : "Start simulation";

					btnPlugUnplug.Enabled = this.SimulationStarted;
					btnPlugUnplug.Text = this.ProbesUnplugged ? "Plug the probes" : "Unplug the probes";

					btnPowerOnOff.Enabled = this.SimulationStarted;
					btnPowerOnOff.Text = this.MonitorPowerOFF ? "Power monitor ON" : "Power monitor OFF";

					btnRecovery.Enabled = this.SimulationStarted;
					btnRecovery.Text = this.RecoveryModeON ? "Turn Recovery OFF" : "Turn Recovery ON";

					btnLateRealtimeOnOff.Enabled = this.SimulationStarted;
					btnLateRealtimeOnOff.Text = this.LateRealTime ? "Turn Late OFF" : "Turn Late ON";
					
					btnDisconnect.Enabled = this.SimulationStarted;
					btnDisconnect.Text = this.ServerDisconnected ? "Reconnect server" : "Disconnect server";

					btnReset.Enabled = this.SimulationStarted;

					numHistory.Enabled = !this.SimulationStarted;

					btnUpdate.Enabled = this.SimulationStarted;

					btnOpenURL.Enabled = this.SimulationStarted;
					
					btnDemo.Enabled = this.SimulationStarted;
					btnDemo.Text = Repository.DemoMode ? "Turn Demo OFF" : "Turn Demo ON";

					this.Refresh();
					Application.DoEvents();
				}
			}
		}

		/// <summary>
		/// Process file and put items in queue
		/// </summary>
		private void ReadTracing(string filename)
		{
			// Reset current data
			this.TracingData = null;

			// Disable controls...
			this.Enabled = false;
			this.Cursor = Cursors.WaitCursor;

			try
			{
				// Make sure there is a filename...
				if (string.IsNullOrEmpty(filename))
					throw new Exception("No file selected");

				// Read the file and merge blocks to improve detection performances
				var block = TracingBlock.Merge(TracingFileReader.Read(filename), 3600).LastOrDefault();
				if ((block == null) || (block.TotalSeconds == 0))
					throw new Exception("Invalid or empty tracing file!");

				// Make sure UP & HR are the exact same length
				block.AlignSignals();

				// Position the time cursor to present time minus requested history tracing
				this.TracingData = block;

				// File opened...
				txtFile.Text = filename;
			}
			catch (Exception ex)
			{
				this.TracingData = null;

				// Error processing, message and clean
				MessageBox.Show(this, string.Format("Error: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}
			finally
			{
				this.Enabled = true;
				this.Cursor = Cursors.Default;
			}
		}

		/// <summary>
		/// The actual worker method
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void workerCtl_DoWork(object sender, DoWorkEventArgs e)
		{
			PeriGen.Patterns.Engine.PatternsEngineWrapper engine = null;
			this.SimulationStarted = true;

			this.MonitorPowerOFF = false;
			this.RecoveryModeON = false;
			this.ResetRequested = false;
			this.ProbesUnplugged = false;
			this.ServerDisconnected = false;
			this.LateRealTime = false;

			this.UpdateUI();

			try
			{
				while (true)
				{
					// Start/Stop server as per required
					if (PatternsDataFeed.IsStarted && this.ServerDisconnected)
					{
						PatternsDataFeed.StopHost();
					}
					else if (!PatternsDataFeed.IsStarted && !ServerDisconnected)
					{
						PatternsDataFeed.StartHost();
					}

					// Small pause here and there
					System.Threading.Thread.Sleep(10);

					// Cancel?
					if (workerCtl.CancellationPending)
					{
						e.Cancel = true;
						break;
					};

					// Reset requested or first call?
					if (this.ResetRequested || engine == null)
					{
						lock (Repository.LockObject)
						{
							Repository.Reset();
							Repository.Patient.PatientId += 1;
						}

						this.TracingTime = this.TracingStart;
						this.TracingIndex = 0;

						engine = new PeriGen.Patterns.Engine.PatternsEngineWrapper(this.TracingStart);

						this.ResetRequested = false;
						this.UpdateUI();
					}

					DateTime now = DateTime.UtcNow;

					var elapsed = (int)(now - this.TracingTime).TotalSeconds;
					if (elapsed < 1)
					{
						continue;
					}

					byte[] ups;
					byte[] hrs;

					// Monitor off or Unplugged or at the end of the tracing data?
					if ((this.MonitorPowerOFF) || (this.ProbesUnplugged) || (this.TracingData == null) || (this.TracingIndex >= this.TracingData.TotalSeconds))
					{
						// No-data arrays...
						ups = Enumerable.Repeat(TracingBlock.NoData, elapsed).ToArray();
						hrs = Enumerable.Repeat(TracingBlock.NoData, elapsed * 4).ToArray();
					}
					else
					{
						elapsed = Math.Min(elapsed, this.TracingData.TotalSeconds - this.TracingIndex);

						ups = (byte[])Array.CreateInstance(typeof(byte), elapsed);
						hrs = (byte[])Array.CreateInstance(typeof(byte), elapsed * 4);
						
						this.TracingData.UPs.CopyTo(this.TracingIndex, ups, 0, elapsed);
						this.TracingData.HRs.CopyTo(this.TracingIndex * 4, hrs, 0, 4 * elapsed);
					}

					// If the monitor is OFF we don't actually save the data
					if (!this.MonitorPowerOFF)
					{
						// Save the tracings
						Repository.AddTracings(new TracingBlock[] { new TracingBlock { Start = this.TracingTime, HRs = hrs.ToList(), UPs = ups.ToList() } });
					}

					// Calculate the pattern's event
					var arts = engine.Process(hrs, ups);
                    var artifacts = from c in arts
                                    where c is DetectionArtifact
                                    select c as DetectionArtifact;

					if (!this.MonitorPowerOFF)
					{
						// Save the artifacts
						Repository.AddArtifacts(artifacts);
					}

					// Adjust patient status
					lock (Repository.LockObject)
					{
						if (this.RecoveryModeON)
							Repository.Patient.Status = StatusType.Recovery;
						else if (this.MonitorPowerOFF)
							Repository.Patient.Status = StatusType.Unplugged;
						else if (this.LateRealTime)
							Repository.Patient.Status = StatusType.Late;
						else
							Repository.Patient.Status = StatusType.Live;
					}

					// Adjust pointers
					this.TracingTime = this.TracingTime.AddSeconds(elapsed);
					this.TracingIndex += elapsed;
				}
			}
			finally
			{
				if (engine != null)
				{
					engine.Dispose();
				}

				PatternsDataFeed.StopHost();
				this.SimulationStarted = false;
				Repository.Reset();

				this.UpdateUI();
			}
		}

		private void btnSelectFile_Click(object sender, EventArgs e)
		{
			// Select file to process
			if (ofdSelectFiles.ShowDialog(this) == DialogResult.OK)
			{
				// Read it
				ReadTracing(ofdSelectFiles.FileName);

				if (this.TracingData != null)
				{
					var hours = (int)(this.TracingData.TotalSeconds / 3600);
					lblOutOf.Text = string.Format("out of {0} hour{1} of tracings loaded", this.TracingData == null ? 0 : hours, hours > 1 ? "s" : string.Empty);
				}
			}
			UpdateUI();
		}

		private void btnStartStop_Click(object sender, EventArgs e)
		{
			btnStartStop.Enabled = false;

			if (this.SimulationStarted)
			{
				workerCtl.CancelAsync();
			}
			else
			{
				this.TracingStart = DateTime.UtcNow.AddHours(-(double)numHistory.Value).RoundToTheSecond();
				this.UpdatePatientModel();
				workerCtl.RunWorkerAsync();
			}
		}

		/// <summary>
		/// Update the patient's data in the repository
		/// </summary>
		void UpdatePatientModel()
		{
			lock (Repository.LockObject)
			{
				Repository.Patient.MRN = this.txtPatientMRN.Text;
				Repository.Patient.FirstName = this.txtPatientFirstName.Text;
				Repository.Patient.LastName = this.txtPatientLastName.Text;
				Repository.Patient.EDD = this.dtpPatientEDD.Checked ? new Nullable<long>(this.dtpPatientEDD.Value.ToEpoch()) : null;
				Repository.Patient.Reset = this.dtpPatientReset.Checked ? new Nullable<long>(this.dtpPatientReset.Value.ToUniversalTime().ToEpoch()) : null;

				byte fetus;
				Repository.Patient.FetusCount = byte.TryParse(this.txtPatientFetus.Text, out fetus) ? new Nullable<byte>(fetus) : null;
			}
		}

		private void btnPlugUnplug_Click(object sender, EventArgs e)
		{
			this.ProbesUnplugged = !this.ProbesUnplugged;
			this.UpdateUI();
		}


		private void btnDemo_Click(object sender, EventArgs e)
		{
			lock (Repository.LockObject)
			{
				Repository.DemoMode = !Repository.DemoMode;
			}
			this.UpdateUI();
		}

		private void btnRecovery_Click(object sender, EventArgs e)
		{
			this.RecoveryModeON = !this.RecoveryModeON;
			this.UpdateUI();
		}

		private void btnPowerOnOff_Click(object sender, EventArgs e)
		{
			this.MonitorPowerOFF = !this.MonitorPowerOFF;
			this.UpdateUI();
		}

		private void btnLateRealtimeOnOff_Click(object sender, EventArgs e)
		{
			this.LateRealTime = !this.LateRealTime;
			this.UpdateUI();
		}

		private void btnReset_Click(object sender, EventArgs e)
		{
			this.btnReset.Enabled = false;
			this.ResetRequested = true;
		}

		private void btnUpdate_Click(object sender, EventArgs e)
		{
			this.UpdatePatientModel();
		}

		private void btnDisconnect_Click(object sender, EventArgs e)
		{
			this.ServerDisconnected = !this.ServerDisconnected;
			this.UpdateUI();
		}

		private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.SimulationStarted)
			{
				workerCtl.CancelAsync();

				// Wait for worker to stop if not already
				for (int i = 0; i < 100; ++i)
				{
					System.Threading.Thread.Sleep(100);
					if (!this.SimulationStarted)
						break;
				}
			}
		}

		/// <summary>
		///  Generate the proper URL for accessing the patterns ActiveX
		/// </summary>
		/// <returns></returns>
		string BuildURL()
		{
			var builder = new StringBuilder();

#if DEBUG
			builder.Append(HttpUtility.UrlPathEncode("http://localhost:63997/PeriGenPatternsWeb/common/Default.aspx?"));
#else
			builder.Append(HttpUtility.UrlPathEncode("http://localhost/TestActiveX/common/Default.aspx?"));
#endif

			builder.Append("server_url=");		builder.Append("http://localhost:7801/XPatternsDataFeed/");

			builder.Append("&patient_id="); builder.Append(HttpUtility.UrlEncode(Repository.Patient.MRN));
			builder.Append("&user_id="); builder.Append(HttpUtility.UrlEncode(this.txtURLUserID.Text));
			builder.Append("&user_name="); builder.Append(HttpUtility.UrlEncode(this.txtURLUserName.Text));

			builder.Append("&permissions="); builder.Append(this.txtURLUserPermission.Text);
			builder.Append("&refresh="); builder.Append(this.numURLRefresh.Value.ToString());
			builder.Append("&cr_limit="); builder.Append(this.numCRLimit.Value.ToString());
			builder.Append("&cr_window="); builder.Append(this.numCRWindow.Value.ToString());
			builder.Append("&cr_stage1="); builder.Append(this.numCRStage1.Value.ToString());
			builder.Append("&cr_stage2="); builder.Append(this.numCRStage2.Value.ToString());
			builder.Append("&banner="); builder.Append(this.cboViewerBanner.SelectedItem.ToString()[0]);
			builder.Append("&timeout_dlg="); builder.Append(this.numTimeoutDlg.Value.ToString());
				
			return builder.ToString();
		}

		private void btnOpenURL_Click(object sender, EventArgs e)
		{
			Process.Start("IExplore.exe", BuildURL());
		}

	}
}
