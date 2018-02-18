using CurveChartControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CurveNuGetUse
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CurveControl curve = new CurveControl();
            curve.GetCurveURLParameters = GetCurveURLParameters;
            elementHost1.Child = curve;
            checkListControl.SetInitialData("http://calm_dev3:7803/PluginsDataFeed/", "197-118-1-1", "02.06.00");
        }

        public CurveURLParameters GetCurveURLParameters()
        {
            return new CurveURLParameters()
            {
                CurveURL = "http://calm_dev3:7802/PatternsDataFeed/",
                CurveVersion = "02.06.00",
                //UserName = "PeriGen Support",
                //UserId = "PeriGen",
                PatientVisitKey = "197-118-1-1",
                CurveClientRefresh = 5,
                Banner = 0,
                CurveReviewModeEnabled = false,
                PODISendEpidural = false,
                PODISendVBAC = false,
                PODISendVaginalDelivery = false,
                CanModify = false,
                CanPrint = false,
            };
        }
    }
}
