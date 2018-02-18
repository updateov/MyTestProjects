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
    /// Interaction logic for CheckBoxWithValueControl.xaml
    /// </summary>
    public partial class CheckBoxWithValueControl : UserControl
    {
        public int? Value
        {
            get { return (int?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int?), typeof(CheckBoxWithValueControl), new UIPropertyMetadata(null));


        public string ControlName
        {
            get { return (string)GetValue(ControlNameProperty); }
            set { SetValue(ControlNameProperty, value); }
        }
        public static readonly DependencyProperty ControlNameProperty =
            DependencyProperty.Register("ControlName", typeof(string), typeof(CheckBoxWithValueControl), new UIPropertyMetadata(String.Empty));


        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(CheckBoxWithValueControl), new UIPropertyMetadata(false));


        public bool IsGrouping
        {
            get { return (bool)GetValue(IsGroupingProperty); }
            set { SetValue(IsGroupingProperty, value); }
        }
        public static readonly DependencyProperty IsGroupingProperty =
            DependencyProperty.Register("IsGrouping", typeof(bool), typeof(CheckBoxWithValueControl), new UIPropertyMetadata(false));

        public int ControlTabIndex
        {
            get { return (int)GetValue(ControlTabIndexProperty); }
            set { SetValue(ControlTabIndexProperty, value); }
        }
        public static readonly DependencyProperty ControlTabIndexProperty =
            DependencyProperty.Register("ControlTabIndex", typeof(int), typeof(CheckBoxWithValueControl), new UIPropertyMetadata(10000));

        public event EventHandler<RoutedEventArgs> Checked;
        public void FireEventChecked()
        {
            var tempHandler = Checked;
            if (tempHandler != null)
            {
                tempHandler(this, new RoutedEventArgs());
            }
        }

        void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            FireEventChecked();
        }       

        public CheckBoxWithValueControl()
        {
            InitializeComponent();          
        }

        private void checkBoxWithValueControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Value.HasValue && Value.Value > 0)
            {
                IsChecked = true;
            }
        }
    }
}
