using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using PeriGen.Patterns.GE.Interface;

namespace PeriGen.Patterns.GEInterfaceTest
{
	public partial class frmMain : Form
	{
		Chalkboard<Episode> GEChalkboard = new GE.Interface.Chalkboard<Episode>();
		bool InRefresh = false;
		int LastPatientId = 0;

		/// <summary>
		/// Ctor
		/// </summary>
		public frmMain()
		{
			this.InitializeComponent();
			this.gridCtl.CellFormatting += new DataGridViewCellFormattingEventHandler(OnCellFormatting);
		}

		/// <summary>
		/// To display dates in local time...
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if ((e.Value != null) && (e.Value.GetType() == typeof(DateTime)) && (e.CellStyle.Format == "T"))
			{
				e.Value = ((DateTime)e.Value).ToLocalTime();
			}
		}

		/// <summary>
		/// Refresh the chalkboard
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnRefresh_Click(object sender, EventArgs args)
		{
			// Avoid double entry
			if (this.InRefresh)
				return;
			this.InRefresh = true;

			try
			{
				this.Cursor = Cursors.WaitCursor;

				// Remove all closed episodes
				GEChalkboard.Episodes.RemoveAll((Episode e) => e.EpisodeStatus == EpisodeStatuses.Closed);
				GEChalkboard.Episodes.ForEach(e => e.Updated = false);

				// Refresh!
				if (!GEChalkboard.Refresh())
				{
					MessageBox.Show(this, "Synchronization skipped. Resfresh again.", "Information", MessageBoxButtons.OK);
					return;
				}

				// Assign unique ID to make it easier to track what is happening (for testing only)
				GEChalkboard.Episodes.Where(e => e.PatientUniqueId == 0).ToList().ForEach(e => e.PatientUniqueId = ++LastPatientId);

				// Refresh the display
				bindingSourceCtl.DataSource = null;
				bindingSourceCtl.DataSource = GEChalkboard.Episodes;
				gridCtl.Refresh();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, "Error detected.\n\nDetails:\n" + ex.Message + ".\n\nSource:\n" + ex.StackTrace, this.Text, MessageBoxButtons.OK);
			}
			finally
			{
				this.Cursor = Cursors.Default;
				this.InRefresh = false;
			}
		}

		/// <summary>
		/// F5 refresh!
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void frmMain_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F5)
			{
				this.btnRefresh_Click(this, EventArgs.Empty);
				e.Handled = true;
			}
		}
	}
}
