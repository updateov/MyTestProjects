using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGrid
{
    public class VMDT
    {
        public VMDT()
        {
            TBL = new DataTable("Strings");
            InitTBL();
        }

        private void InitTBL()
        {
            DataColumn col = new DataColumn();
            col.Caption = "ID";
            col.ColumnName = "ID";
            col.DataType = System.Type.GetType("System.Int32");
            col.ReadOnly = true;
            col.Unique = true;
            TBL.Columns.Add(col);

            col = new DataColumn();
            col.Caption = "Name";
            col.ColumnName = "Name";
            col.DataType = System.Type.GetType("System.String");
            col.ReadOnly = true;
            col.Unique = false;
            TBL.Columns.Add(col);
        }


        DataTable TBL { get; set; }
    }
}
