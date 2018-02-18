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
using System.Windows.Shapes;

namespace PeriCALM.Patterns.Curve.UI.Chart
{
    /// <summary>
    /// Interaction logic for WinDialog.xaml
    /// </summary>
    public partial class WinDialog : Window
    {
        public WinDialog()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(WinDialog_Loaded);
            this.btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            this.btnOk.Click += new RoutedEventHandler(btnOk_Click);
        }

        void btnOk_Click(object sender, RoutedEventArgs e)
        {
            IsOk = true;
            this.Hide();
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsOk = false;
            this.Hide();
        }

        void WinDialog_Loaded(object sender, RoutedEventArgs e)
        {
            lblMessage.Text = Message;
            this.Title = MessageTitle;
            IsOk = false;
        }

        public Boolean IsOk { get; private set; }
        public string Message { get; set; }
        public string MessageTitle { get; set; }

    }
}
