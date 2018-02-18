using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MoveButtonTestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            _btnLeft.DrawArrow(BtnDirection.Left);
            _btnUp.DrawArrow(BtnDirection.Up);
            _btnRight.DrawArrow(BtnDirection.Right);
            _btnDown.DrawArrow(BtnDirection.Down);
        }

        private void _btn_Click(object sender, EventArgs e)
        {
            BtnArrow btn = (BtnArrow)sender;

            listBox1.Items.Add(btn.Direction.ToString());           
        }
    }
}
