using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace PeriGen.Patterns.Settings.Tool
{
	public partial class frmMain : Form
	{

		/// <summary>
		/// The ONE source to use to trace data (Windows event log / DebugView...)
		/// </summary>
		static TraceSource Source = new TraceSource("PatternsSettings");

		SettingsModel Model = null;

		/// <summary>
		/// images for the editor
		/// </summary>
		Image imageBlack = null;
		Image imageRed = null;



		/// <summary>
		/// Ctr
		/// </summary>
		public frmMain()
		{
			InitializeComponent();

			// Create the windows event log source entry
			try
			{
				if (!EventLog.SourceExists("PeriGen Settings Editor"))
				{
					EventLog.CreateEventSource("PeriGen Settings Editor", "Application");
				}
			}
			catch (Exception e)
			{
				Source.TraceEvent(TraceEventType.Warning, 1001, "Warning, unable to create the log source.\n{0}", e);
			}

			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PeriGen.Patterns.Settings.Tool.Resources.bullet_black.png");
			imageBlack = Bitmap.FromStream(stream);
			stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PeriGen.Patterns.Settings.Tool.Resources.bullet_red.png");
			imageRed = Bitmap.FromStream(stream);

			this.Load += frmMain_Load;
			this.FormClosing += frmMain_FormClosing;
			this.lstItems.DrawMode = DrawMode.OwnerDrawFixed;
			this.lstItems.DrawItem += lstItems_DrawItem;
			this.lstLevels.DrawMode = DrawMode.OwnerDrawFixed;
			this.lstLevels.DrawItem += lstLevels_DrawItem;

			//Model to host settings file
			Model = new SettingsModel();
			Model.Source = Source;
		}

		/// <summary>
		/// Overrides list item drawing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void lstItems_DrawItem(object sender, DrawItemEventArgs e)
		{
			//no selection, return
			if (-1 == e.Index) return;

			//draw background
			bool edited = false;
			e.DrawBackground();

			//stored default item values
			Brush myBrush = Brushes.Black;
			Image img = imageBlack;

			//check if item has been edited
			if ((lstItems.Items[e.Index] as SettingData).Edited)
			{
				edited = true;
				myBrush = Brushes.Red;
				img = imageRed;
			}

			//check if item is selected
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) myBrush = edited ? Brushes.Yellow : Brushes.White;

			//draw bullet
			e.Graphics.DrawImage(img, e.Bounds.X, e.Bounds.Y, 16, 16);

			//draw text
			e.Graphics.DrawString(lstItems.Items[e.Index].ToString(), e.Font, myBrush, e.Bounds.X + 16, e.Bounds.Y, StringFormat.GenericDefault);

			//draw focus
			e.DrawFocusRectangle();
		}

		/// <summary>
		/// Overrides list item drawing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void lstLevels_DrawItem(object sender, DrawItemEventArgs e)
		{
			//no selection, return
			if (-1 == e.Index) return;

			//draw background
			bool edited = false;
			e.DrawBackground();

			//set default values
			Brush myBrush = Brushes.Black;
			Image img = imageBlack;

			//check if item has been edited
			if ((lstLevels.Items[e.Index] as SettingsSection).Settings.Count(s => s.Edited) > 0)
			{
				myBrush = Brushes.Red;
				img = imageRed;
				edited = true;
			}

			//check if item is selected
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) myBrush = edited ? Brushes.Yellow : Brushes.White;

			//draw bullet
			e.Graphics.DrawImage(img, e.Bounds.X, e.Bounds.Y, 16, 16);

			//draw text
			e.Graphics.DrawString(lstLevels.Items[e.Index].ToString(), e.Font, myBrush, e.Bounds.X + 16, e.Bounds.Y, StringFormat.GenericDefault);

			//draw focus
			e.DrawFocusRectangle();
		}

		/// <summary>
		/// Check closing reason and saving
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void frmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			///close only by code
			if (e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;

				///close app checks if there are changes in the settings without saving
				CloseApplication();
			}
		}

		/// <summary>
		/// Close application
		/// </summary>
		private void CloseApplication()
		{
			//check for changes...no changes... exit
			if (!Model.HasBeenChanged) Environment.Exit(0);

			//there are some changes... ask for saving...
			var result = MessageBox.Show(this, "The content of the settings file has been changed.\rDo you want to save those changes?", this.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

			//check user's answer
			//cancel... keep app open...
			if (result == DialogResult.Cancel) return;

			//do not save changes...exit
			if (result == DialogResult.No) Environment.Exit(0);

			//save changes...exit if save true, keep app open if saving fails...
			if (SaveSettings()) Environment.Exit(0);

			//it cannot save settings. Keep app open.
		}

		/// <summary>
		/// Save settings and returns result of operation
		/// </summary>
		/// <returns></returns>
		private bool SaveSettings()
		{

			bool mustReload = false;

			try
			{
				//Save settings
				Model.SaveSettingsToFile(txtNewThumbprint.Text, out mustReload);

				//must reload?
				if (mustReload)
				{
					MessageBox.Show("The file was saved successfully!\r\nBecause you have changed the thumbprint, you will need to reopen the file again.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

					//Reset content because thumbprint was changed
					ResetFields();

				}
				else
				{
					MessageBox.Show("The file was saved successfully!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				return true;
			}
			catch (Exception ex)
			{
				Source.TraceEvent(TraceEventType.Error, 1007, "Error while trying to save the file.\r\nDetails: {0}\r\nOperation is cancelled.", ex);
				MessageBox.Show(string.Format("Error while trying to save the file.\r\nDetails: {0}\r\nOperation is cancelled.", ex.Message), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		void frmMain_Load(object sender, EventArgs e)
		{
			//initialize UI
			ResetFields();

			//add handler for custom drawing
			lstLevels.SelectedIndexChanged += lstLevels_SelectedIndexChanged;
			lstItems.SelectedIndexChanged += lstItems_SelectedIndexChanged;

			//Load command line file
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length > 1)
			{
				if (File.Exists(args[1]))
				{
					//Loads settings file
					OpenSettingsFile(args[1]);
				}
			}

		}

		void lstItems_SelectedIndexChanged(object sender, EventArgs e)
		{
			//selected setting item changed
			//clear bindings
			lblCommentDetails.Text = "";
			txtValueDetails.Text = "";

			//add new binding if there is a new item selected
			if (lstItems.SelectedIndex > -1)
			{
				lblCommentDetails.Text = (lstItems.SelectedItem as SettingData).Info;
				txtValueDetails.Text = (lstItems.SelectedItem as SettingData).Value;
			}
		}

		void lstLevels_SelectedIndexChanged(object sender, EventArgs e)
		{
			//level item (section) has changed
			//reload item for new section
			FilterItems();
		}

		/// <summary>
		/// Propulate Items based in level
		/// </summary>
		private void FilterItems()
		{
			//load settings for specific selected section
			if (Model.Sections != null && lstLevels.SelectedIndex > -1)
			{
				lstItems.DataSource = Model.Sections.Where(l => l.Level == lstLevels.SelectedIndex).First().Settings.ToList();
				lstItems.SelectedIndex = 0;
			}
			else
			{
				//no section selected
				lstItems.DataSource = null;
			}
		}

		private void btnAddLevel_Click(object sender, EventArgs e)
		{
			//show new level (section)
			AddRemoveLevel(false);
		}

		/// <summary>
		/// Add or remove level from list
		/// </summary>
		/// <param name="remove">indicates if must remove a level</param>
		private void AddRemoveLevel(bool remove)
		{
			if (remove)
			{
				//remove section
				if (lstLevels.Items.Count > 1) lstLevels.Items.RemoveAt(lstLevels.Items.Count - 1);
			}
			else
			{
				//add section
				if (lstLevels.Items.Count < Model.Sections.Count) lstLevels.Items.Add(Model.Sections.First(s => s.Level == lstLevels.Items.Count));
			}

			//Update UI controls
			lstLevels.SelectedIndex = lstLevels.Items.Count - 1;
			btnAddLevel.Enabled = (lstLevels.Items.Count < Model.Sections.Count);
			btnRemoveLevel.Enabled = (lstLevels.Items.Count > 1);
		}

		private void btnRemoveLevel_Click(object sender, EventArgs e)
		{
			//remove level
			AddRemoveLevel(true);
		}

		private void btnEdit_Click(object sender, EventArgs e)
		{
			//Edit Selected Item
			EditItem();
		}

		/// <summary>
		/// Edit a value
		/// </summary>
		private void EditItem()
		{
			if (lstItems.SelectedItem != null)
			{
				//store current selected indexes
				int index = lstItems.SelectedIndex;
				int indexLevel = lstLevels.SelectedIndex;

				//new editor instance
				using (frmEdit editor = new frmEdit())
				{
					//set item to edit
					editor.DataToEdit = lstItems.SelectedItem as SettingData;

					//show editor and wait
					editor.ShowDialog();
				}

				//reset level index
				lstLevels.SelectedIndex = -1;
				lstLevels.SelectedIndex = indexLevel;

				//Filter items (renew bindings)
				FilterItems();

				//reset item index
				lstItems.SelectedIndex = -1;
				lstItems.SelectedIndex = index;

				//Update UI with message if item has been modified
				CheckItemsModified();
			}
		}

		/// <summary>
		/// Update UI showing a message if any item has beed modified
		/// </summary>
		void CheckItemsModified()
		{
			//add or removed text in group if any item has been modified or not
			grpSettings.Text = (Model.HasBeenChanged) ? "Setting value/s were modified!" : "";
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//open file to edit
			OpenFile();
		}

		/// <summary>
		/// Open file to edit
		/// </summary>
		private void OpenFile()
		{
			//Clear Fields 
			ResetFields();

			//Clear Fields
			using (var diag = new OpenFileDialog())
			{
				diag.DefaultExt = ".xml";
				diag.Filter = "Settings File (*.xml)|*.xml|*.*|*.*";
				diag.FilterIndex = 1;
				diag.Multiselect = false;
				diag.Title = "Select settings file to edit";
				if (diag.ShowDialog() == DialogResult.OK)
				{
					OpenSettingsFile(diag.FileName);
				}
			}
		}

		/// <summary>
		/// Open settings file 
		/// </summary>
		/// <param name="fileName"></param>
		private void OpenSettingsFile(string fileName)
		{
			try
			{
				//Load Settings
				Model.OpenSettingsFromFile(fileName);

				//Get thumbprint
				txtThumbprint.Text = Model.Thumbprint;

				//select first level
				lstLevels.Items.Add(Model.Sections.First(s => s.Level == 0));
				lstLevels.SelectedIndex = 0;

				//Update UI controls
				toolReset.Enabled = true;
				saveToolStripMenuItem.Enabled = true;
				btnAddLevel.Enabled = true;
				btnRemoveLevel.Enabled = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error processing settings file. Operation is cancelled.\r\nDetails: " + ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				ResetFields();
			}
		}

		/// <summary>
		/// Initialize fields to default values
		/// </summary>
		private void ResetFields()
		{
			//Reset data in model also
			Model.Reset();

			grpSettings.Text = "";
			FilterItems();
			lblCommentDetails.Text = "";
			txtValueDetails.Text = "";
			lblCommentDetails.Text = "";
			txtValueDetails.Text = "";
			lstLevels.Items.Clear();
			btnAddLevel.Enabled = false;
			btnRemoveLevel.Enabled = false;
			txtThumbprint.Text = "";
			txtNewThumbprint.Text = "";
			saveToolStripMenuItem.Enabled = false;
			toolReset.Enabled = false;
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//Close app
			CloseApplication();
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int index = lstItems.SelectedIndex;
			int indexLevel = lstLevels.SelectedIndex;

			//Save settings
			SaveSettings();

			//Refresh UI
			if (lstLevels.Items.Count > 0)
			{
				//Level
				lstLevels.SelectedIndex = -1;
				lstLevels.SelectedIndex = indexLevel;
				lstLevels.Refresh();

				//Items for level
				lstItems.SelectedIndex = -1;
				lstItems.SelectedIndex = index;
				lstItems.Refresh();

				CheckItemsModified();
			}
			else
			{
				lstLevels.SelectedIndex = -1;
				lstItems.SelectedIndex = -1;
			}
		}

		private void toolReset_Click(object sender, EventArgs e)
		{
			//Clear UI and reset values
			ResetFields();
		}

		private void lstItems_DoubleClick(object sender, EventArgs e)
		{
			EditItem();
		}
	}
}
