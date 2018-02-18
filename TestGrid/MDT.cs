using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGrid
{
    public class MDT
    {
        public MDT()
        {
            TBL = new DataTable("Strings");
            FillTBL();
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
                row["Name"] = "ParentItem " + i;
                TBL.Rows.Add(row);
            }
        }

        public DataTable TBL { get; set; }
    }
}
