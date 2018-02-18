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

namespace PatternsCRIClient.Controls
{
    /// <summary>
    /// Interaction logic for EddDateControl.xaml
    /// </summary>
    public partial class EddDateControl : UserControl
    {
        public string GestationalAge
        {
            get { return (string)GetValue(GestationalAgeProperty); }
            set { SetValue(GestationalAgeProperty, value); }
        }

        public static readonly DependencyProperty GestationalAgeProperty =
            DependencyProperty.Register("GestationalAge", typeof(string), typeof(EddDateControl), new PropertyMetadata(String.Empty));


        public DateTime EDD
        {
            get { return (DateTime)GetValue(EDDProperty); }
            set { SetValue(EDDProperty, value); }
        }

        public static readonly DependencyProperty EDDProperty =
            DependencyProperty.Register("EDD", typeof(DateTime), typeof(EddDateControl), new PropertyMetadata(DateTime.Now));

        
        public EddDateControl()
        {
            InitializeComponent();
        }

        //public string CalcGestationalAgeInDays(DateTime edd)
        //{
        //    DateTime delveryDate = edd.ToUniversalTime();
        //    DateTime startDate = edd.AddDays(-280).ToUniversalTime();
        //    DateTime nowDate = DateTime.UtcNow;

        //    double nDays = (nowDate - startDate).TotalDays;

        //    int weeks = Convert.ToInt32(nDays) / 7;
        //    int days = Convert.ToInt32(nDays) % 7;

        //    string ga = String.Format("{0} + {1}", weeks, days);
            
        //    return ga;
        //}
    }
}
