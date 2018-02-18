using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ManagedGUI
{
    public partial class ManagedWindow : Form
    {
        public ManagedWindow()
        {
            InitializeComponent();
        }

        public String SetText
        {
            set { textBoxFromUnmanaged.Text = value; }
        }
    }
}
