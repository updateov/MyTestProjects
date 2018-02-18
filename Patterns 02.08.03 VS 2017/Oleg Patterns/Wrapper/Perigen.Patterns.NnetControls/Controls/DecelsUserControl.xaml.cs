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
    /// Interaction logic for DecelsUserControl.xaml
    /// </summary>
    public partial class DecelsUserControl : UserControl, IBaseExportControl    
    {
        public CalculatedCheckboxGroupConcept ControlData
        {
            get { return (CalculatedCheckboxGroupConcept)GetValue(ControlDataProperty); }
            set { SetValue(ControlDataProperty, value); }
        }
        public static readonly DependencyProperty ControlDataProperty =
            DependencyProperty.Register("ControlData", typeof(CalculatedCheckboxGroupConcept), typeof(DecelsUserControl), new PropertyMetadata(null));

        public PatternsUIManager UiManager
        {
            get { return (PatternsUIManager)GetValue(UiManagerProperty); }
            set { SetValue(UiManagerProperty, value); }
        }
        public static readonly DependencyProperty UiManagerProperty =
            DependencyProperty.Register("UiManager", typeof(PatternsUIManager), typeof(DecelsUserControl), new PropertyMetadata(null));

        public bool IsCanceled
        {
            get { return (bool)GetValue(IsCanceledProperty); }
            set { SetValue(IsCanceledProperty, value); }
        }
        public static readonly DependencyProperty IsCanceledProperty =
            DependencyProperty.Register("IsCanceled", typeof(bool), typeof(DecelsUserControl), new UIPropertyMetadata(false));

        public DecelsUserControl()
        {
            InitializeComponent();
            this.Loaded += DecelsUserControl_Loaded;
        }

        private void DecelsUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.UpdateControlData();
        }

        private void DecelsUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_stackPanel.Children.Count == 0)
            {
                int decelsCount = ControlData.Items.Where(t => t.IsGrouping == false).Sum(f => f.CalculatedValue == String.Empty || f.CalculatedValue == null ? 0 : Int32.Parse(f.CalculatedValue));

                ControlData.Items.ForEach(item =>
                        {
                            CheckBoxWithValueControl control = new CheckBoxWithValueControl();
                            control.IsGrouping = item.IsGrouping;
                            control.ControlName = item.Value;
                            control.Checked += CheckBox_Checked;
                            control.Unchecked += CheckBox_Unchecked;
                            control.Margin = new Thickness(0, 2, 0, 0);
                            control.ControlTabIndex = this.TabIndex + m_stackPanel.Children.Count;

                            if (decelsCount == 0 && control.IsGrouping == true)
                            {
                                control.Value = 1;
                            }
                            else
                            {
                                control.Value = item.CalculatedValue == String.Empty || item.CalculatedValue == null ? 0 : Int32.Parse(item.CalculatedValue);
                            }

                            m_stackPanel.Children.Add(control);
                        });

                UpdateControlData();
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //CheckBoxWithValueControl groupCheckBox = m_stackPanel.Children.OfType<CheckBoxWithValueControl>().ToArray().SingleOrDefault(t => t.IsGrouping == true);                                              

            //if (groupCheckBox != null)
            //{
            //    foreach (CheckBoxWithValueControl control in m_stackPanel.Children)
            //    {
            //        if (groupCheckBox != control && control.IsChecked == true)
            //        {
            //            return;
            //        }
            //    }

            //    groupCheckBox.IsChecked = true;
            //}
            UpdateControlData();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBoxWithValueControl chkBox = (CheckBoxWithValueControl)sender;

            if (chkBox.IsGrouping == true)
            {
                foreach (CheckBoxWithValueControl control in m_stackPanel.Children)
                {
                    if (control.IsGrouping == true)
                        control.IsChecked = true;
                    else
                        control.IsChecked = false;
                }
            }
            else
            {
                foreach (CheckBoxWithValueControl control in m_stackPanel.Children)
                {
                    if (control.IsGrouping == true)
                    {
                        control.IsChecked = false;
                        break;
                    }
                }
            }

            UpdateControlData();
        }

        public void UpdateControlData()
        {
            ControlData.Value = String.Empty;
            bool isNone = false;

            foreach (CheckBoxWithValueControl control in m_stackPanel.Children)
            {
                if (control.IsChecked == true)
                {
                    if (control.IsGrouping == true)
                    {
                        isNone = true;
                        ControlData.Value = control.ControlName;

                        break;
                    }
                    else
                    {
                        ControlData.Value += control.ControlName + ";";
                    }
                }
            }

            if (isNone == false)
            {
                ControlData.Value = ControlData.Value.ToString().TrimEnd(';');
            }    
        }

        public void SetFocus()
        {
            foreach (CheckBoxWithValueControl control in m_stackPanel.Children)
            {
                if (control.IsGrouping == true)
                {
                    control.Focus();
                    break;
                }
            }
        }
    }
}
