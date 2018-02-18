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
    /// Interaction logic for StringControlContainer.xaml
    /// </summary>
    public partial class StringControlContainer : UserControl, IBaseExportControl
    {
        public StringConcept ControlData
        {
            get { return (StringConcept)GetValue(ControlDataProperty); }
            set { SetValue(ControlDataProperty, value); }
        }
        public static readonly DependencyProperty ControlDataProperty =
            DependencyProperty.Register("ControlData", typeof(StringConcept), typeof(StringControlContainer), new PropertyMetadata(null));

        public PatternsUIManager UiManager
        {
            get { return (PatternsUIManager)GetValue(UiManagerProperty); }
            set { SetValue(UiManagerProperty, value); }
        }
        public static readonly DependencyProperty UiManagerProperty =
            DependencyProperty.Register("UiManager", typeof(PatternsUIManager), typeof(StringControlContainer), new PropertyMetadata(null));

        public bool IsCanceled
        {
            get { return (bool)GetValue(IsCanceledProperty); }
            set { SetValue(IsCanceledProperty, value); }
        }
        public static readonly DependencyProperty IsCanceledProperty =
            DependencyProperty.Register("IsCanceled", typeof(bool), typeof(StringControlContainer), new UIPropertyMetadata(false));

        public int RowsNum { get; set; }

        private bool IsControlLoaded { get; set; }

        public StringControlContainer()
        {
            IsControlLoaded = false;

            InitializeComponent();
        }

        private void StringControlContainer_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsControlLoaded == false)
            {
                txtComment.TabIndex = this.TabIndex;
                IsControlLoaded = true;
                txtComment.Height = txtComment.Height + (this.RowsNum - 1) * 16;
            }
        }

        public void SetFocus()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                txtComment.SelectAll();
                txtComment.Focus();
            }));
        }
    }
}
