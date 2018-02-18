using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using PeriGen.Patterns.Research.SQLHelper;
using Microsoft.Win32;

namespace PeriGen.Patterns.Research.OfflineDetection
{
	public partial class DetectionWindow : Form
	{
		//Used to close window
		bool MustClose = false;
		bool IsWorking = false;
		bool WaitingClose = false;

		//Main file processor
		ProcessorEngine Processor;

		public DetectionWindow()
		{
			InitializeComponent();

			//Initialize engine
			Processor = new ProcessorEngine();
			Processor.ProgressChanged += new ProgressChangedEventHandler(Processor_ProgressChanged);
			Processor.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Processor_RunWorkerCompleted);

			//initialize UI
			//Source Folder
			try
			{
				this.txtFolderSource.Text = Registry.CurrentUser.GetValue("Software\\Pattern's offline detection\\LastOpenFolder", "").ToString();
				if (!string.IsNullOrEmpty(this.txtFolderSource.Text))
				{
					folderBrowserDialog.SelectedPath = this.txtFolderSource.Text;
				}
			}
			catch { }

			//Target Folder
			try
			{
				this.txtFolderTarget.Text = Registry.CurrentUser.GetValue("Software\\Pattern's offline detection\\LastTargetFolder", "").ToString();
				if (!string.IsNullOrEmpty(this.txtFolderTarget.Text))
				{
					folderBrowserTarget.SelectedPath = this.txtFolderTarget.Text;
				}
			}
			catch { }

			//LastFile
			try
			{
				var txt = Registry.CurrentUser.GetValue("Software\\Pattern's offline detection\\LastOpenFile", "").ToString();
				if (!string.IsNullOrEmpty(txt))
				{
					this.ofdSelectFiles.InitialDirectory = Path.GetDirectoryName(txt);
				}
			}
			catch { }

		}

		#region Processor Events

		void Processor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			// Display engine messages
			MessageBox.Show(this, e.Result.ToString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

			// Update UI
			tsbBatch.Enabled = true;
			tsbConvert.Enabled = true;
			tsbStop.Enabled = false;
			btnFolder.Enabled = true;
			cbToXml.Enabled = true;
			cbToSQL.Enabled = true;
			chkRecursive.Enabled = true;
			btnTarget.Enabled = true;
			tsProgress.Value = 0;
			tsProgress.Visible = false;
			tssStatus.Text = "";
			IsWorking = false;

			if (MustClose)
			{
				//close for x
				this.Close();
			}
		}

		void Processor_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			// Update progress
			if (e.ProgressPercentage > 100)
			{
				tsProgress.Value = 1;
			}
			else
			{
				tsProgress.Value = e.ProgressPercentage;
			}

			// Display information from engine
			if (e.UserState != null && e.UserState is String)
			{
				tssStatus.Text = e.UserState.ToString();
			}
		}

		#endregion

		void OnBatchModeClick(object sender, EventArgs e)
		{
			//Check parameters
			if (string.IsNullOrEmpty(this.txtFolderSource.Text))
			{
				MessageBox.Show(this, "Please, select the folder containing the data files", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
				return;
			}

			if (cbToXml.Checked)
			{
				if (string.IsNullOrEmpty(txtFolderTarget.Text))
				{
					MessageBox.Show(this, "Please, select the target folder to save XML data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
					return;
				}
			}

			//Prepare task
			ProcessorEngine.ProcessFilesArgs args = new ProcessorEngine.ProcessFilesArgs();
			args.SourceFolder = txtFolderSource.Text;
			args.SaveToSQL = cbToSQL.Checked;
			args.SaveToXML = cbToXml.Checked;
			args.TargetFolder = txtFolderTarget.Text;
			args.IsRecursive = chkRecursive.Checked;
			Processor.Args = args;

			//Update UI
			tsbBatch.Enabled = false;
			tsbConvert.Enabled = false;
			tsbStop.Enabled = true;
			btnFolder.Enabled = false;
			cbToXml.Enabled = false;
			cbToSQL.Enabled = false;
			chkRecursive.Enabled = false;
			btnTarget.Enabled = false;
			tsProgress.Value = 0;
			tssStatus.Text = "";
			tsProgress.Visible = true;
			IsWorking = true;
			txtFolderSource.Enabled = true;
			txtFolderTarget.Enabled = true;

			//Start working
			Processor.RunWorkerAsync();
		}

		void OnProcessOneFileClick(object sender, EventArgs e)
		{
			//ask for file to process
			if (this.ofdSelectFiles.ShowDialog(this) == DialogResult.OK)
			{
				//check parameters
				if (cbToXml.Checked)
				{
					if (string.IsNullOrEmpty(txtFolderTarget.Text))
					{
						MessageBox.Show(this, "Please, select the target folder to save XML data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
						return;
					}
				}
				if (cbToXml.Checked == false && cbToSQL.Checked == false)
				{
					MessageBox.Show(this, "Please, select convertion option (SQL, XML or both)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
					return;
				}
				Registry.CurrentUser.SetValue("Software\\Pattern's offline detection\\LastOpenFile", this.ofdSelectFiles.FileName);

				//Prepare task
				ProcessorEngine.ProcessFilesArgs args = new ProcessorEngine.ProcessFilesArgs();
				args.SourceFolder = Path.GetDirectoryName(this.ofdSelectFiles.FileName);
				args.FileToProcess = this.ofdSelectFiles.FileName;
				args.ProcessOneFile = true;
				args.SaveToSQL = cbToSQL.Checked;
				args.SaveToXML = cbToXml.Checked;
				args.TargetFolder = txtFolderTarget.Text;
				args.IsRecursive = false;
				Processor.Args = args;

				//Update UI
				tsbBatch.Enabled = false;
				tsbConvert.Enabled = false;
				tsbStop.Enabled = true;
				btnFolder.Enabled = false;
				cbToXml.Enabled = false;
				cbToSQL.Enabled = false;
				chkRecursive.Enabled = false;
				btnTarget.Enabled = false;
				tsProgress.Value = 0;
				tssStatus.Text = "";
				tsProgress.Visible = true;
				IsWorking = true;
				txtFolderSource.Enabled = false;
				txtFolderTarget.Enabled = false;

				//Start working
				Processor.RunWorkerAsync();
			}
		}

		void OnStopClick(object sender, EventArgs e)
		{
			//cancel process
			tsbStop.Enabled = false;
			Processor.CancelAsync();
		}

		void OnFolderClick(object sender, EventArgs e)
		{
			//Select source folder
			if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
			{
				this.txtFolderSource.Text = folderBrowserDialog.SelectedPath;
			}
		}

		void btnTarget_Click(object sender, EventArgs e)
		{

			//select target folder for XML
			if (folderBrowserTarget.ShowDialog(this) == DialogResult.OK)
			{
				this.txtFolderTarget.Text = folderBrowserTarget.SelectedPath;
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			//Validate form closing
			//If threads are working, must be stopped first
			if (IsWorking)
			{
				if (MustClose)
				{
					base.OnFormClosing(e);
					return;
				}
				else
				{
					if (!WaitingClose)
					{
						WaitingClose = true;
						MustClose = true;
						this.OnStopClick(this, EventArgs.Empty);
						e.Cancel = true;
					}
					else
					{
						MustClose = true;
						e.Cancel = true;
					}
				}
			}
			else
			{
				base.OnFormClosing(e);
				return;
			}
		}

		void cbToSQL_CheckedChanged(object sender, EventArgs e)
		{
			//Check database 
			if (this.cbToSQL.Checked && !DataUtils.CheckIfDatabaseExist())
			{
				if (MessageBox.Show(this, "The database does not exist, do you want to create it?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
				{
					this.cbToSQL.Checked = false;
					return;
				}
				//create database
				try
				{
					DataUtils.CreateDatabase();
				}
				catch (Exception ex)
				{
					MessageBox.Show(this, "Error while creating database. Details: " + ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void cbToXml_CheckedChanged(object sender, EventArgs e)
		{
			btnTarget.Enabled = cbToXml.Checked;
		}

		private void DetectionWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			Registry.CurrentUser.SetValue("Software\\Pattern's offline detection\\LastTargetFolder", this.txtFolderTarget.Text);
			Registry.CurrentUser.SetValue("Software\\Pattern's offline detection\\LastOpenFolder", this.txtFolderSource.Text);
		}
	}
}