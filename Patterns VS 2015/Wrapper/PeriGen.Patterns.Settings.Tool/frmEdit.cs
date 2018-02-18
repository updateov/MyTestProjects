using System;
using System.Windows.Forms;

namespace PeriGen.Patterns.Settings.Tool
{
	internal partial class frmEdit : Form
	{
		public frmEdit()
		{
			InitializeComponent();
			cmbBoolean.Items.Add("TRUE");
			cmbBoolean.Items.Add("FALSE");
			cmbBoolean.SelectedIndex = 0;
			this.Load += new EventHandler(frmEdit_Load);
		}

		void frmEdit_Load(object sender, EventArgs e)
		{

			if (DataToEdit.Type.ToUpper().Trim() == "BOOLEAN")
			{
				cmbBoolean.Visible = true;
				cmbBoolean.SelectedIndex = cmbBoolean.FindString(DataToEdit.Value.ToUpper().Trim());
			}
			else if (DataToEdit.Type.ToUpper().Trim() == "INTEGER")
			{
				numEditor.Visible = true;
				numEditor.Value = int.Parse(DataToEdit.Value);
			}
			else //string by default
			{
				txtValueDetails.Visible = true;
				txtValueDetails.Text = DataToEdit.Value;
			}
			lblCommentDetails.Text = DataToEdit.Info;
			lblValueDetails.Text = DataToEdit.Key;
		}

		public SettingData DataToEdit
		{
			get;
			set;
		}

		private void btnReset_Click(object sender, EventArgs e)
		{
			if (DataToEdit.Type.ToUpper().Trim() == "BOOLEAN")
			{
				cmbBoolean.SelectedIndex = cmbBoolean.FindString(DataToEdit.ResetValue.ToUpper().Trim());
			}
			else if (DataToEdit.Type.ToUpper().Trim() == "INTEGER")
			{
				numEditor.Value = int.Parse(DataToEdit.ResetValue);
			}
			else //string by default
			{
				txtValueDetails.Text = DataToEdit.ResetValue;
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			if (DataToEdit.Type.ToUpper().Trim() == "BOOLEAN")
			{
				DataToEdit.Value = cmbBoolean.SelectedIndex == 0 ? "True" : "False";
				DataToEdit.Edited = true;
				this.Close();
			}
			else if (DataToEdit.Type.ToUpper().Trim() == "INTEGER")
			{
				DataToEdit.Value = numEditor.Value.ToString();
				DataToEdit.Edited = true;
				this.Close();
			}
			else //String by default
			{
				DataToEdit.Value = txtValueDetails.Text.Trim();
				DataToEdit.Edited = true;
				this.Close();
			}
		}
	}
}
