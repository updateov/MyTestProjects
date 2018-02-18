using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextBlockCollectionTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public String TT { get { return "svsdf"; } }
    }
    public class TextBlockUtils
    {
        /// <summary>
        /// Gets the value of the AutoTooltipProperty dependency property
        /// </summary>
        public static bool GetAutoTooltip(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoTooltipProperty);
        }

        /// <summary>
        /// Sets the value of the AutoTooltipProperty dependency property
        /// </summary>
        public static void SetAutoTooltip(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoTooltipProperty, value);
        }

        /// <summary>
        /// Identified the attached AutoTooltip property. When true, this will set the TextBlock TextTrimming
        /// property to WordEllipsis, and display a tooltip with the full text whenever the text is trimmed.
        /// </summary>
        public static readonly DependencyProperty AutoTooltipProperty = DependencyProperty.RegisterAttached(
                                                                            "AutoTooltip",
                                                                            typeof(bool), 
                                                                            typeof(TextBlockUtils), 
                                                                            new PropertyMetadata(false, OnAutoTooltipPropertyChanged));

        private static void OnAutoTooltipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBlock textBlock = d as TextBlock;
            if (textBlock == null)
                return;

            if (e.NewValue.Equals(true))
            {
                textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                ComputeAutoTooltip(textBlock);
                textBlock.SizeChanged += TextBlock_SizeChanged;
            }
            else
                textBlock.SizeChanged -= TextBlock_SizeChanged;
        }

        private static void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            ComputeAutoTooltip(textBlock);
        }

        /// <summary>
        /// Assigns the ToolTip for the given TextBlock based on whether the text is trimmed
        /// </summary>
        private static void ComputeAutoTooltip(TextBlock textBlock)
        {
            textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            var width = textBlock.DesiredSize.Width;
            if (textBlock.ActualWidth < width)
                ToolTipService.SetToolTip(textBlock, "bla");
                //ToolTipService.SetToolTip(textBlock, textBlock.Text);
            else
                ToolTipService.SetToolTip(textBlock, null);
        }
    }
}
