using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestGrid
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            TBL = new DataTable();
            FillTBL();
            dataGridView1.DataSource = TBL;
            listBox1.DataSource = TBL.AsEnumerable().Select(c => c["ID2"]).ToList();
        }

        private void FillTBL()
        {
            DataColumn col = new DataColumn();
            col.Caption = "ID";
            col.ColumnName = "ID";
            col.DataType = System.Type.GetType("System.Int32");
            col.ReadOnly = true;
            col.Unique = true;
            TBL.Columns.Add(col);

            col = new DataColumn();
            col.Caption = "ID2";
            col.ColumnName = "ID2";
            col.DataType = System.Type.GetType("System.Int32");
            col.ReadOnly = true;
            col.Unique = false;
            TBL.Columns.Add(col);

            col = new DataColumn();
            col.Caption = "Name";
            col.ColumnName = "Name";
            col.DataType = System.Type.GetType("System.String");
            col.ReadOnly = true;
            col.Unique = false;
            TBL.Columns.Add(col);

            DataRow row;
            for (int i = 0; i <= 2; i++)
            {
                row = TBL.NewRow();
                row["ID"] = i;
                row["ID2"] = 2 * i;
                row["Name"] = "ParentItem " + i;
                TBL.Rows.Add(row);
            }
        }

        public DataTable TBL { get; set; }

        private void dataGridView1_DataSourceChanged(object sender, EventArgs e)
        {
            if (dataGridView1.Columns["ID2"] != null)
            {
                dataGridView1.Columns["ID2"].Visible = false;
            }
        }
    }
}
