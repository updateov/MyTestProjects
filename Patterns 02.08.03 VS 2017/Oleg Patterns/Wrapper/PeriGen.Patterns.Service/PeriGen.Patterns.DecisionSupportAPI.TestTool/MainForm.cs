using System;
using System.Net;
using System.Windows.Forms;
using System.Globalization;

namespace PeriGen.Patterns.DecisionSupportAPI.TestTool
{
	public partial class MainForm : Form
	{
		object LockObject = new object();
		
		VisitDataModel Data { get; set; }
		BindingSource ContractionsTableBindingSource { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public MainForm()
		{
			// Avoid issues with certificates
			ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(delegate { return true; });
			this.Data = new VisitDataModel();

			this.InitializeComponent();
			this.Data.SetKey(txtVisitKey.Text);

			this.ContractionsTableBindingSource = new BindingSource { DataSource = this.Data.ContractionsTable };
			this.dataGridView.AutoGenerateColumns = false;
			this.dataGridView.ScrollBars = ScrollBars.Vertical;
			this.dataGridView.DataSource = this.ContractionsTableBindingSource;

			this.cboDuration.SelectedIndex = 1;
			this.cboMinimumUP.SelectedIndex = 9;

			this.AutomaticQuery(false);
			this.UpdateDataFilter();
		}

		void btnQuery_Click(object sender, EventArgs e)
		{
			this.Query();
		}

		void btnClear_Click(object sender, EventArgs e)
		{
			lock (this.LockObject)
			{
				this.Data.ContractionsTable.Clear();
			}
		}

		/// <summary>
		/// Selected block duration
		/// </summary>
		int BlockDuration
		{
			get
			{
				return int.Parse(this.cboDuration.Text.Split(" ".ToCharArray(), StringSplitOptions.None)[0], CultureInfo.InvariantCulture);
			}
		}

		/// <summary>
		/// Selected minimum UP
		/// </summary>
		int MinimumUP
		{
			get
			{
				return int.Parse(this.cboMinimumUP.Text.Substring(0, this.cboMinimumUP.Text.Length - 1), CultureInfo.InvariantCulture);
			}
		}

		void txtVisitKey_Leave(object sender, EventArgs e)
		{
			lock (this.LockObject)
			{
				if (string.CompareOrdinal(txtVisitKey.Text, this.Data.VisitKey) != 0)
				{
					this.Data.SetKey(txtVisitKey.Text);
				}
			}
		}

		void AutomaticQueryTimer_Tick(object sender, EventArgs e)
		{
			this.Query();
		}

		void btnReset_Click(object sender, EventArgs e)
		{
			lock (this.LockObject)
			{
				this.AutomaticQuery(false);
				this.Data.Clear();
			}
		}

		void chkShowOnlyUpdated_CheckedChanged(object sender, EventArgs e)
		{
			this.UpdateDataFilter();
		}

		void btnAuto_Click(object sender, EventArgs e)
		{
			this.AutomaticQuery(!this.AutomaticQueryTimer.Enabled);
		}

		/// <summary>
		/// Query the data from Patterns
		/// </summary>
		void Query()
		{
			lock (this.LockObject)
			{
				try
				{
					this.Data.Query(this.txtURL.Text, this.BlockDuration, this.MinimumUP);
					this.panelSettings.Enabled = false;
				}
				catch (Exception e)
				{
					this.AutomaticQuery(false);
					MessageBox.Show(this, e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		/// <summary>
		/// Automatically query every so xxx second
		/// </summary>
		/// <param name="start"></param>
		void AutomaticQuery(bool start)
		{
			lock (this.LockObject)
			{
				this.panelSettings.Enabled = !start;
				this.numDelay.Enabled = !start;

				this.btnAuto.Text = start ? "Stop auto-query" : "Start auto-query";
				if (start)
				{
					this.AutomaticQueryTimer.Interval = (int)(this.numDelay.Value * 1000);
				}
				this.AutomaticQueryTimer.Enabled = start;
			}
		}

		/// <summary>
		/// Adjust the filtering on the list
		/// </summary>
		void UpdateDataFilter()
		{
			lock (this.LockObject)
			{
				this.ContractionsTableBindingSource.Filter = (this.chkShowOnlyUpdated.Checked ? "Updated = True" : null);
			}
		}
	}
}
