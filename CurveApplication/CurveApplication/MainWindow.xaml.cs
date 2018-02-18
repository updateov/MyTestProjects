using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CurveChartControl;

namespace CurveApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeUserControl();
        }

        private void InitializeUserControl()
        {
            CurveControl.GetCurveURLParameters = GetCurveURLParameters;
        }

        public CurveURLParameters GetCurveURLParameters()
        {
            return new CurveURLParameters()
            {
                CurveHostName = "calm_dev2",
                CurvePort = 7802,
                CurveVersion = "02.06.00",
                //UserName = "PeriGen Support",
                //UserId = "PeriGen",
                PatientVisitKey = "47-5-1-1",
                CurveClientRefresh = 5,
                Banner = 0,
                CurveReviewModeEnabled = false,
                PODISendEpidural = false,
                PODISendVBAC = false,
                PODISendVaginalDelivery = false,
                CanModify = false,
                CanPrint = false,
            };

            //return new CurveURLParameters()
            //{
            //    CurveHostName = "cristian_win7",
            //    CurvePort = 7802,
            //    CurveVersion = "01.00.00.00",
            //    //UserName = "PeriGen Support",
            //    //UserId = "PeriGen",
            //    PatientVisitKey = "1-14-1-1",
            //    CurveClientRefresh = 5,
            //    Banner = 0,
            //    CurveReviewModeEnabled = false,
            //    PODISendEpidural = false,
            //    PODISendVBAC = false,
            //    PODISendVaginalDelivery = false,
            //    CanModify = false,
            //    CanPrint = false,
            //};
        }
    }
}
