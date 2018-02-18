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
    /// Interaction logic for RangeDoubleConceptControl.xaml
    /// </summary>
    public partial class RangeDoubleConceptControl : UserControl, IBaseExportControl
    {
        public StringConcept ControlData
        {
            get { return (StringConcept)GetValue(ControlDataProperty); }
            set
            {
                if (value.Value != null && value.Value.ToString() == "-1")
                {
                    value.Value = String.Empty;
                }
                SetValue(ControlDataProperty, value);
            }
        }
        public static readonly DependencyProperty ControlDataProperty =
            DependencyProperty.Register("ControlData", typeof(StringConcept), typeof(RangeDoubleConceptControl), new PropertyMetadata(null));

        public StringConcept ReadOnlyData
        {
            get { return (StringConcept)GetValue(ReadOnlyDataProperty); }
            set
            {
                if (value.Value != null && value.Value.ToString() == "-1")
                {
                    value.Value = String.Empty;
                }
                SetValue(ReadOnlyDataProperty, value);
            }
        }
        public static readonly DependencyProperty ReadOnlyDataProperty =
            DependencyProperty.Register("ReadOnlyData", typeof(StringConcept), typeof(RangeDoubleConceptControl), new PropertyMetadata(null));

        public PatternsUIManager UiManager
        {
            get { return (PatternsUIManager)GetValue(UiManagerProperty); }
            set { SetValue(UiManagerProperty, value); }
        }
        public static readonly DependencyProperty UiManagerProperty =
            DependencyProperty.Register("UiManager", typeof(PatternsUIManager), typeof(RangeDoubleConceptControl), new PropertyMetadata(null));

        public bool IsCanceled
        {
            get { return (bool)GetValue(IsCanceledProperty); }
            set { SetValue(IsCanceledProperty, value); }
        }
        public static readonly DependencyProperty IsCanceledProperty =
            DependencyProperty.Register("IsCanceled", typeof(bool), typeof(RangeDoubleConceptControl), new UIPropertyMetadata(false));
        public RangeDoubleConceptControl()
        {
            InitializeComponent();
            this.Loaded += RangeDoubleConceptControl_Loaded;
        }

        private void RangeDoubleConceptControl_Loaded(object sender, RoutedEventArgs e)
        {
            m_txtValue.TabIndex = this.TabIndex;      
        }

        public void SetFocus()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                m_txtValue.SelectAll();
                m_txtValue.Focus();
            }));
        }
    }
}
