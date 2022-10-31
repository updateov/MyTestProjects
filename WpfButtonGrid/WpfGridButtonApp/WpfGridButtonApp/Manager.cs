using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGridButtonApp
{
    internal class Manager
    {
        private static Manager s_instance = null;
        private static Object s_lock = new Object();
        public DataTable PatientsData { get; private set; }

        private Manager()
        {
        }

        public static Manager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                            s_instance = new Manager();
                    }
                } 

                return s_instance;
            }
        }


        public void InitDataTable()
        {
            PatientsData = new DataTable("Patients");
            DataColumn column;

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "FirstName";
            column.Caption = "First Name";
            column.ReadOnly = true;
            column.Unique = false;
            PatientsData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "LastName";
            column.Caption = "Last Name";
            column.ReadOnly = true;
            column.Unique = false;
            PatientsData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Inits";
            column.Caption = "Initials";
            column.ReadOnly = true;
            column.Unique = false;
            PatientsData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "BedName";
            column.Caption = "Bed Name";
            column.ReadOnly = true;
            column.Unique = false;
            PatientsData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "VisitKey";
            column.Caption = "VisitKey";
            column.ReadOnly = true;
            column.Unique = false;
            PatientsData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "VisitId";
            column.Caption = "VisitId";
            column.ReadOnly = true;
            column.Unique = false;
            PatientsData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "FileName";
            column.Caption = "File Name";
            column.ReadOnly = true;
            column.Unique = false;
            PatientsData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "State";
            column.Caption = "State";
            column.ReadOnly = false;
            column.Unique = false;
            PatientsData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Object");
            column.ColumnName = "Start";
            column.Caption = "Start";
            column.ReadOnly = false;
            column.Unique = false;
            PatientsData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Object");
            column.ColumnName = "Stop";
            column.Caption = "Stop";
            column.ReadOnly = true;
            column.Unique = false;
            PatientsData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Discharge";
            column.Caption = "Discharge";
            column.ReadOnly = true;
            column.Unique = false;
            PatientsData.Columns.Add(column);
        }

        internal void AddPatient(String fn, String ln, String bn, String vk, int pId)
        {
            // Add to DataTable and grid
            DataRow row;
            row = PatientsData.NewRow();
            row["FirstName"] = fn;
            row["LastName"] = ln;
            row["Inits"] = fn[0].ToString() + ln[0].ToString();
            row["BedName"] = bn;
            row["VisitKey"] = vk;
            row["VisitId"] = pId;
            row["State"] = "Stopped";
            String files = "files";
            row["FileName"] = files;
            PatientsData.Rows.Add(row);
        }
    }
}
