using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace TestPatternsControlApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Properties.Settings config = Properties.Settings.Default;
            bool exportSupport = Convert.ToBoolean(config.export);

            checklistControl1.SetInitialData(config.URL, config.patient);

            //checklistControl1.OnIntervalPressedEvent += new CLRPatternsUserControls.BasePatternsUserControl.IntervalPressedEventHandler(checklistControl1_OnIntervalPressedEvent);
            
            
            //checklistControl2.SetInitialData(config.URL2, config.patient2, config.user2_id, config.user2_name, config.permission2);
            //checklistControl3.SetInitialData(config.URL3, config.patient3, config.user3_id, config.user3_name, config.permission3);
            //checklistControl4.SetInitialData(config.URL4, config.patient4, config.user4_id, config.user4_name, config.permission4);
        }

        void checklistControl1_OnIntervalPressedEvent(DateTime from, DateTime to, int intervalID)
        {
        }
    }
}
