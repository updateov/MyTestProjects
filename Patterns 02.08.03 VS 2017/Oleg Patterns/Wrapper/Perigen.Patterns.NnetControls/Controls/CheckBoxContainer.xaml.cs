using Export.Entities;
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
    /// Interaction logic for CheckBoxContainer.xaml
    /// </summary>
    public partial class CheckBoxContainer : UserControl, IBaseExportControl
    {
        public IntConcept ControlData
        {
            get { return (IntConcept)GetValue(ControlDataProperty); }
            set { SetValue(ControlDataProperty, value); }
        }
        public static readonly DependencyProperty ControlDataProperty =
            DependencyProperty.Register("ControlData", typeof(IntConcept), typeof(CheckBoxContainer), new PropertyMetadata(null));

        public PatternsUIManager UiManager
        {
            get { return (PatternsUIManager)GetValue(UiManagerProperty); }
            set { SetValue(UiManagerProperty, value); }
        }
        public static readonly DependencyProperty UiManagerProperty =
            DependencyProperty.Register("UiManager", typeof(PatternsUIManager), typeof(CheckBoxContainer), new PropertyMetadata(null));

        public bool IsCanceled
        {
            get { return (bool)GetValue(IsCanceledProperty); }
            set { SetValue(IsCanceledProperty, value); }
        }
        public static readonly DependencyProperty IsCanceledProperty =
            DependencyProperty.Register("IsCanceled", typeof(bool), typeof(CheckBoxContainer), new UIPropertyMetadata(false));

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(CheckBoxContainer), new UIPropertyMetadata(false));

        public CheckBoxContainer()
        {
            InitializeComponent();
            this.Loaded += CheckBoxContainer_Loaded;
        }

        private void CheckBoxContainer_Loaded(object sender, RoutedEventArgs e)
        {
            m_checkBox.TabIndex = this.TabIndex;

            if (ControlData.Value != null)
            {
                int res = 0;
                if(Int32.TryParse(ControlData.Value.ToString(), out res))
                {
                    IsChecked = res > 0 ? true : false;
                }
                else
                {
                    IsChecked = false;
                }          
            }
            else
            {
                IsChecked = false;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ControlData.Value = 1;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ControlData.Value = 0;
        }

        public void SetFocus()
        {
            m_checkBox.Focus();
        }
    }
}
