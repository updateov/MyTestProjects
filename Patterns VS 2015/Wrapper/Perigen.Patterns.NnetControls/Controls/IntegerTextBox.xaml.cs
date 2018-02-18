//Reviewed: 12/01/16
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
using Perigen.Patterns.NnetControls.Screens;
using System.Windows.Interop;

namespace Perigen.Patterns.NnetControls.Controls
{
    /// <summary>
    /// Interaction logic for IntegerTextBox.xaml
    /// </summary>
    public partial class IntegerTextBox : UserControl
    {
        private bool m_isControlLoaded = false;
        private bool m_isDirty = false;
        private bool m_isControlInFocus = false;

        
        public bool IsCanceled
        {
            get { return (bool)GetValue(IsCanceledProperty); }
            set { SetValue(IsCanceledProperty, value); }
        }
        public static readonly DependencyProperty IsCanceledProperty =
            DependencyProperty.Register("IsCanceled", typeof(bool), typeof(IntegerTextBox), new UIPropertyMetadata(false));


        public Style TextStyle
        {
            get { return (Style)GetValue(TextStyleProperty); }
            set { SetValue(TextStyleProperty, value); }
        }
        public static readonly DependencyProperty TextStyleProperty =
            DependencyProperty.Register("TextStyle", typeof(Style), typeof(IntegerTextBox), new UIPropertyMetadata(null));


        public Brush BorderColor
        {
            get { return (Brush)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }
        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register("BorderColor", typeof(Brush), typeof(IntegerTextBox), new UIPropertyMetadata(null));


        public int? Value
        {
            get { return (int?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int?), typeof(IntegerTextBox), new UIPropertyMetadata(null));

        public int? OriginValue
        {
            get { return (int?)GetValue(OriginValueProperty); }
            set { SetValue(OriginValueProperty, value); }
        }
        public static readonly DependencyProperty OriginValueProperty =
            DependencyProperty.Register("OriginValue", typeof(int?), typeof(IntegerTextBox), new UIPropertyMetadata(null));


        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(int), typeof(IntegerTextBox), new UIPropertyMetadata(0));


        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int), typeof(IntegerTextBox), new UIPropertyMetadata(Int32.MaxValue));


        public bool IsInFocus
        {
            get { return (bool)GetValue(IsInFocusProperty); }
            set { SetValue(IsInFocusProperty, value); }
        }
        public static readonly DependencyProperty IsInFocusProperty =
            DependencyProperty.Register("IsInFocus", typeof(bool), typeof(IntegerTextBox), new UIPropertyMetadata(false));


        public int ControlTabIndex
        {
            get { return (int)GetValue(ControlTabIndexProperty); }
            set { SetValue(ControlTabIndexProperty, value); }
        }
        public static readonly DependencyProperty ControlTabIndexProperty =
            DependencyProperty.Register("ControlTabIndex", typeof(int), typeof(IntegerTextBox), new UIPropertyMetadata(10000));


        public string ParameterName
        {
            get { return (string)GetValue(ParameterNameProperty); }
            set { SetValue(ParameterNameProperty, value); }
        }
        public static readonly DependencyProperty ParameterNameProperty =
            DependencyProperty.Register("ParameterName", typeof(string), typeof(IntegerTextBox), new UIPropertyMetadata(String.Empty));

        //Add comments
        public IntegerTextBox()
        {
            InitializeComponent();
        }

        private void integerTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            string strValue = String.Empty;

            if (Value.HasValue && Value >= 0)
            {
                strValue = Value.ToString();
            }

            m_txtValue.Text = strValue;

            Window wnd = Window.GetWindow(this);
            wnd.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(wnd_PreviewMouseLeftButtonDown);

            SetTextDecorations();

            if (this.IsInFocus)
            {
                m_txtValue.Focus();
                m_txtValue.CaretIndex = m_txtValue.Text.Length;
                m_txtValue.SelectAll();
            }

            m_isControlLoaded = true;
        }

        void wnd_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lock (PatternsUIManager.Instance.ValidationLock)
            {
                if (e.Source is Button)
                {
                    Button btn = e.Source as Button;

                    if (btn != null)
                    {
                        m_isDirty = !btn.IsCancel;
                    }
                }
                else if (m_isControlInFocus == true && this.m_txtValue.IsMouseOver == false)
                {
                    m_isDirty = true;
                }
            }
        }

        private int? GetCurrentValue()
        {
            int? curValue = null;
            int iValue;

            if (Int32.TryParse(m_txtValue.Text, out iValue) == false)
            {
                if (String.IsNullOrEmpty(m_txtValue.Text))
                {
                    curValue = null;
                }
            }
            else
            {
                curValue = iValue;
            }

            return curValue;
        }

        private void integerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == System.Windows.Visibility.Visible && m_isDirty == true && IsCanceled == false)
            {
                int? curValue = GetCurrentValue();

                bool isValid = curValue.HasValue == false ? true : this.MinValue <= curValue && curValue <= this.MaxValue;

                if (isValid == false)
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#D43F06");
                    m_border.BorderBrush = new SolidColorBrush(color);

                    MessageBoxWindow msgBox = new MessageBoxWindow();
                    msgBox.MessageTitle.Text = "Warning";
                    msgBox.MessageDescription.Text = ParameterName + " must be a number beetween " +
                                                     MinValue.ToString() + " and " +
                                                     MaxValue.ToString() + ".";
                    msgBox.ShowDialog();

                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                        new Action(delegate()
                        {
                            m_txtValue.Focus();
                            m_txtValue.CaretIndex = m_txtValue.Text.Length;
                            m_txtValue.SelectAll();
                        }));

                    e.Handled = true;
                }
                else
                {
                    m_border.BorderBrush = this.BorderColor;
                }

                TextBox textBox = sender as TextBox;
                int iValue = -1;

                if (String.IsNullOrEmpty(textBox.Text))
                {
                    Value = null;
                }
                else
                {
                    if (Int32.TryParse(textBox.Text, out iValue))
                    {
                        Value = iValue;
                    }
                    else
                    {
                        Value = null;
                    }
                }

                m_isDirty = false;
                m_isControlInFocus = false;

                SetTextDecorations();
            }
        }

        private void SetTextDecorations()
        {
            // Not for drop 2

            //if (this.Value == this.OriginValue)
            //{
            //    m_txtValue.TextDecorations = null;
            //}
            //else
            //{
            //    if (this.Value == null && this.OriginValue != null)
            //    {
            //        m_txtValue.TextDecorations = TextDecorations.Strikethrough;
            //        m_txtValue.Text = this.OriginValue.ToString();
            //    }
            //    else
            //    {
            //        m_txtValue.TextDecorations = TextDecorations.Underline;
            //    }
            //}
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_isControlLoaded == false)
                return;

            TextBox textBox = sender as TextBox;
            int iValue = -1;

            if (Int32.TryParse(textBox.Text, out iValue) == false)
            {
                TextChange textChange = e.Changes.ElementAt<TextChange>(0);
                int iAddedLength = textChange.AddedLength;
                int iOffset = textChange.Offset;

                textBox.Text = textBox.Text.Remove(iOffset, iAddedLength);
                textBox.CaretIndex = textBox.Text.Length;
            }        
        }

        private void _txtValue_GotFocus(object sender, RoutedEventArgs e)
        {
            SetTextDecorations();

            if (m_txtValue.TextDecorations == TextDecorations.Strikethrough)
            {
                m_txtValue.Text = String.Empty;
                m_txtValue.TextDecorations = null;
            }

            m_txtValue.CaretIndex = m_txtValue.Text.Length;
            m_txtValue.SelectAll();
            m_isControlInFocus = true;
        }

        private void _txtValue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            lock (PatternsUIManager.Instance.ValidationLock)
            {
                m_isDirty = PatternsUIManager.Instance.IsValidKey(e.Key);
            }
        }
    }
}
