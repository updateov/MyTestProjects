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
    /// Interaction logic for RoundingControlContainer.xaml
    /// </summary>
    public partial class RoundingControlContainer : UserControl, IBaseExportControl
    {
        public IntConcept ControlData
        {
            get { return (IntConcept)GetValue(ControlDataProperty); }
            set { SetValue(ControlDataProperty, value); }
        }
        public static readonly DependencyProperty ControlDataProperty =
            DependencyProperty.Register("ControlData", typeof(IntConcept), typeof(RoundingControlContainer), new PropertyMetadata(null));

        public PatternsUIManager UiManager
        {
            get { return (PatternsUIManager)GetValue(UiManagerProperty); }
            set { SetValue(UiManagerProperty, value); }
        }
        public static readonly DependencyProperty UiManagerProperty =
            DependencyProperty.Register("UiManager", typeof(PatternsUIManager), typeof(RoundingControlContainer), new PropertyMetadata(null));

        public bool IsCanceled
        {
            get { return (bool)GetValue(IsCanceledProperty); }
            set { SetValue(IsCanceledProperty, value); }
        }
        public static readonly DependencyProperty IsCanceledProperty =
            DependencyProperty.Register("IsCanceled", typeof(bool), typeof(RoundingControlContainer), new UIPropertyMetadata(false));

        public int RoundBy
        {
            get { return (int)GetValue(RoundByProperty); }
            set { SetValue(RoundByProperty, value); }
        }
        public static readonly DependencyProperty RoundByProperty =
            DependencyProperty.Register("RoundBy", typeof(int), typeof(RoundingControlContainer), new PropertyMetadata(1));

        public int? RoundedValue
        {
            get { return (int)GetValue(RoundedValueProperty); }
            set { SetValue(RoundedValueProperty, value); }
        }
        public static readonly DependencyProperty RoundedValueProperty =
            DependencyProperty.Register("RoundedValue", typeof(int?), typeof(RoundingControlContainer), new PropertyMetadata(null));


        public RoundingControlContainer()
        {
            InitializeComponent();
            this.Loaded += RoundingControlContainer_Loaded;
            this.integerTextBox.m_txtValue.TextChanged += Value_TextChanged;
        }

        private void Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            ControlData.Value = ((TextBox)sender).Text;
        }

        private void RoundingControlContainer_Loaded(object sender, RoutedEventArgs e)
        {
            if(ControlData != null)
            {
                if(RoundBy < 1)
                {
                    RoundBy = 1;
                }

                if (ControlData.Value != null)
                {
                    int val = Int32.Parse(ControlData.Value.ToString());

                    RoundedValue = (int)Math.Round((double)val / RoundBy, MidpointRounding.AwayFromZero) * RoundBy;

                    if (RoundBy != 1)
                    {
                        m_txtRoundedValue.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public void SetFocus()
        {
            integerTextBox.SetFocus();
        }
    }
}
