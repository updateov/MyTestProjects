using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MedianTest
{
    public partial class Form1 : Form
    {
        public DataTable MedianDataTable { get; private set; }

        public Form1()
        {
            InitializeComponent();
        }
    }
}
