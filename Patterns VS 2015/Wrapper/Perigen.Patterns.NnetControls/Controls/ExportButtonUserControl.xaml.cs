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

namespace Perigen.Patterns.NnetControls.Controls
{
    /// <summary>
    /// Interaction logic for ExportButtonUserControl.xaml
    /// </summary>
    public partial class ExportButtonUserControl : UserControl
    {
        public bool Is30MinChecked
        {
            get { return (bool)GetValue(Is30MinCheckedProperty); }
            set { SetValue(Is30MinCheckedProperty, value); }
        }
        public static readonly DependencyProperty Is30MinCheckedProperty =
            DependencyProperty.Register("Is30MinChecked", typeof(bool), typeof(ExportButtonUserControl), new UIPropertyMetadata(true));


        public bool Is15MinChecked
        {
            get { return (bool)GetValue(Is15MinCheckedProperty); }
            set { SetValue(Is15MinCheckedProperty, value); }
        }
        public static readonly DependencyProperty Is15MinCheckedProperty =
            DependencyProperty.Register("Is15MinChecked", typeof(bool), typeof(ExportButtonUserControl), new UIPropertyMetadata(false));


        public ExportButtonUserControl()
        {
            InitializeComponent();
        }

        public void SetTimeRange(bool is30MinChecked)
        {
            if (is30MinChecked == true)
            {
                Is30MinChecked = true;
                Is15MinChecked = false;
            }
            else
            {             
                Is30MinChecked = false;
                Is15MinChecked = true;             
            }
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border menuItem = sender as Border;

            if (menuItem != null)
            {
                int timeRange;

                if (Int32.TryParse(menuItem.Tag.ToString(), out timeRange))
                {                  
                    // Enable/Disable menu items
                    Is30MinChecked = timeRange == 15 ? true : false;
                    Is15MinChecked = timeRange == 30 ? true : false;

                    PatternsUIManager.Instance.RaiseTimeRangeChangedEvent(timeRange);
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {         
            PatternsUIManager.Instance.RaiseTimeRangeChangedEvent(Is30MinChecked == true ? 30 : 15);
        }
    }
}
