using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChartControl1
{
    public partial class Form1 : Form
    {
        public List<byte> FHR { get; set; }
        public List<byte> UP { get; set; }
        private int m_lastFHR = 0;
        private int m_lastUP = 0;

        private Timer m_timer = new Timer();
        
        public Form1()
        {
            InitializeComponent();
            FHR = new List<byte>();
            UP = new List<byte>();

            FillFHRAndUP();
            m_timer.Interval = 2000;
            m_timer.Tick += m_timer_Tick;
        }

        void m_timer_Tick(object sender, EventArgs e)
        {
            if (m_lastFHR >= 19999)
                m_timer.Stop();

            List<byte> up = new List<byte>();
            //for (int i = 0; i < 2; i++)
            //{
            //    UPToDraw.Add(UP[m_lastUP++]);
            //}

            var fhr = m_lastFHR <= 0 ? FHR.GetRange(0, 7000) : FHR.GetRange(m_lastFHR, 8);
            m_lastFHR += m_lastFHR <= 0 ? 7000 : 8;
            //var fhr = FHR.GetRange(m_lastFHR, 8);
            //m_lastFHR += 8;
            //for (int i = 0; i < 8; i++)
            //{
            //    fhr.Add(FHR[m_lastFHR++]);
            //}

            chartCtrl1.AddDataToStrip(fhr, up);

            Refresh();
        }

        private void FillFHRAndUP()
        {
            //var rand1 = new Random();
            //rand1.NextBytes(fhrs);
            FHR.AddRange(fhrs);

            //var rand2 = new Random();
            //for (int i = 0; i < 2000; i++)
            //{
            //    int toAdd = rand2.Next(127);
            //    UP.Add((byte)toAdd);
            //}
        }

        private void buttonStartStop_Click(object sender, EventArgs e)
        {
            m_timer.Start();
        }


    }
}
