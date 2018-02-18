using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PatternsApplication
{
    public partial class PatientList : UserControl
    {
        public PatientList()
        {
            InitializeComponent();
        }

        private void buttonCollapse_Click(object sender, EventArgs e)
        {
            if (CollapseClicked != null)
                CollapseClicked(this, EventArgs.Empty);
        }

        public event EventHandler CollapseClicked;
    }
}
