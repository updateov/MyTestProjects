using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PatternsApplication
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void patientListCtrl_CollapseClicked(object sender, EventArgs e)
        {
            splitContainerPatientWeb.Panel1Collapsed = true;
        }
    }
}
