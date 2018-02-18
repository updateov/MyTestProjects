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
    /// Interaction logic for ComboBoxControlContainer.xaml
    /// </summary>
    public partial class ComboBoxControlContainer : UserControl, IBaseExportControl
    {
        public ComboConcept ControlData
        {
            get { return (ComboConcept)GetValue(ControlDataProperty); }
            set { SetValue(ControlDataProperty, value); }
        }
        public static readonly DependencyProperty ControlDataProperty =
            DependencyProperty.Register("ControlData", typeof(ComboConcept), typeof(ComboBoxControlContainer), new PropertyMetadata(null));


        public PatternsUIManager UiManager
        {
            get { return (PatternsUIManager)GetValue(UiManagerProperty); }
            set { SetValue(UiManagerProperty, value); }
        }
        public static readonly DependencyProperty UiManagerProperty =
            DependencyProperty.Register("UiManager", typeof(PatternsUIManager), typeof(ComboBoxControlContainer), new PropertyMetadata(null));

        public bool IsCanceled
        {
            get { return (bool)GetValue(IsCanceledProperty); }
            set { SetValue(IsCanceledProperty, value); }
        }
        public static readonly DependencyProperty IsCanceledProperty =
            DependencyProperty.Register("IsCanceled", typeof(bool), typeof(ComboBoxControlContainer), new UIPropertyMetadata(false));
        public ComboBoxControlContainer()
        {
            InitializeComponent();
            this.Loaded += ComboBoxControlContainer_Loaded;
        }

        private void ComboBoxControlContainer_Loaded(object sender, RoutedEventArgs e)
        {
            m_combo.TabIndex = this.TabIndex;
        }

        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ControlData.Value = m_combo.SelectedValue != null ? m_combo.SelectedValue.ToString() : String.Empty;
        }

        public void SetFocus()
        {
            m_combo.Focus();
        }
    }
}
