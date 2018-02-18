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
using System.Windows.Shapes;

namespace ColorPickerSample
{
    /// <summary>
    /// Interaction logic for ColorCanvasAndPicker.xaml
    /// </summary>
    public partial class ColorCanvasAndPicker : Window
    {
        public ColorCanvasAndPicker()
        {
            InitializeComponent();
        }

        private void _colorCanvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            _colorPicker.SelectedColor = _colorCanvas.SelectedColor;

        }

        private void _colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            _colorCanvas.SelectedColor = _colorPicker.SelectedColor;
        }
    }
}
