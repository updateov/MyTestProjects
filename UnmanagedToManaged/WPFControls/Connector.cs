using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace WPFControls
{

    class Globals
    {
        public static HwndSource gHwndSource;
        public static ControlData gwcControlDAta;
    }

    public class Connector
    {
        public Connector(HwndSourceParameters param)
        {
            m_sourceParam = param;
        }

        public HwndSource InitCtrl()
        //public void InitCtrl()
        {
            //Thread t = new Thread(() =>
            //    {
                    Globals.gHwndSource = new HwndSource(m_sourceParam);
                    Globals.gwcControlDAta = new ControlData();
	                FrameworkElement myPage = Globals.gwcControlDAta;
                    Globals.gHwndSource.RootVisual = myPage;

            //    });

            //t.SetApartmentState(ApartmentState.STA);
            //t.Start();
            //t.Join(5000);
            //Thread.CurrentThread.Join();           
                    return Globals.gHwndSource;
        }

        private HwndSourceParameters m_sourceParam;
    }
}
