﻿using Export.Entities;
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
    /// Interaction logic for CalculatedComboBoxControlContainer.xaml
    /// </summary>
    public partial class CalculatedComboBoxControlContainer : UserControl, IBaseExportControl
    {
        public CalculatedComboConcept ControlData
        {
            get { return (CalculatedComboConcept)GetValue(ControlDataProperty); }
            set { SetValue(ControlDataProperty, value); }
        }
        public static readonly DependencyProperty ControlDataProperty =
            DependencyProperty.Register("ControlData", typeof(CalculatedComboConcept), typeof(CalculatedComboBoxControlContainer), new PropertyMetadata(null));

        public PatternsUIManager UiManager
        {
            get { return (PatternsUIManager)GetValue(UiManagerProperty); }
            set { SetValue(UiManagerProperty, value); }
        }
        public static readonly DependencyProperty UiManagerProperty =
            DependencyProperty.Register("UiManager", typeof(PatternsUIManager), typeof(CalculatedComboBoxControlContainer), new PropertyMetadata(null));

        public bool IsCanceled
        {
            get { return (bool)GetValue(IsCanceledProperty); }
            set { SetValue(IsCanceledProperty, value); }
        }
        
        public static readonly DependencyProperty IsCanceledProperty =
            DependencyProperty.Register("IsCanceled", typeof(bool), typeof(CalculatedComboBoxControlContainer), new UIPropertyMetadata(false));

        public CalculatedComboBoxControlContainer()
        {
            InitializeComponent();
            this.Loaded += calculatedComboBoxControlContainer_Loaded;
        }
        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ControlData.Value = m_combo.SelectedValue != null ? m_combo.SelectedValue.ToString() : String.Empty;
        }

        private void calculatedComboBoxControlContainer_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= calculatedComboBoxControlContainer_Loaded;

            List<string> variabilityItems = new List<string>();
            double variabilityValue = -10; // impossible value
            string variabilitySelectedValue = String.Empty;

            if (ControlData.Value != null)
            {
                Double.TryParse(ControlData.Value.ToString(), out variabilityValue);
            }

            foreach (CalculatedComboConceptItem item in ControlData.Items)
            {
                variabilityItems.Add(item.Value);

                if (item.Min <= variabilityValue && item.Max >= variabilityValue)
                {
                    variabilitySelectedValue = item.Value;
                }
            }

            m_combo.TabIndex = this.TabIndex;
            m_combo.ItemsSource = variabilityItems;
            m_combo.SelectedItem = variabilitySelectedValue;
            ControlData.OriginalValue = variabilitySelectedValue;
        }

        public void SetFocus()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                m_combo.Focus();
            }));
        }
    }
}