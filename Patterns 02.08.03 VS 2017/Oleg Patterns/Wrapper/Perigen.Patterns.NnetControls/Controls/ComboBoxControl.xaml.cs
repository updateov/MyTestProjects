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
    /// Interaction logic for ComboBoxControl.xaml
    /// </summary>
    public partial class ComboBoxControl : UserControl, IBaseExportControl  
    {
        public List<string> Items
        {
            get { return (List<string>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(List<string>), typeof(ComboBoxControl), new UIPropertyMetadata(null));


        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(ComboBoxControl), new UIPropertyMetadata(null));


        public string OriginValue
        {
            get { return (string)GetValue(OriginValueProperty); }
            set { SetValue(OriginValueProperty, value); }
        }
        public static readonly DependencyProperty OriginValueProperty =
            DependencyProperty.Register("OriginValue", typeof(string), typeof(ComboBoxControl), new UIPropertyMetadata(null));


        public ComboBoxControl()
        {
            InitializeComponent();
            this.Loaded += ComboBoxControl_Loaded;
        }

        private void ComboBoxControl_Loaded(object sender, RoutedEventArgs e)
        {
            m_comboBox.TabIndex = this.TabIndex;
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetTextDecorations();
        }

        private void comboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SetTextDecorations();
        }

        private void SetTextDecorations()
        {
            if (this.Value == this.OriginValue)
            {
                m_textBlock.TextDecorations = null;
            }
            else
            {
                if (this.Value == String.Empty && this.OriginValue != String.Empty)
                {
                    m_textBlock.TextDecorations = TextDecorations.Strikethrough;
                    m_textBlock.Text = this.OriginValue.ToString();
                }
                else
                {
                    m_textBlock.TextDecorations = TextDecorations.Underline;
                }
            }
        }

        public void SetFocus()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                m_comboBox.Focus();
            }));
        }
    }
}
