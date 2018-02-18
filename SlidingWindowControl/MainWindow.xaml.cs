using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace SlidingWindowControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const String DateFormat = "HH:mm";

        private int m_rect1Start = 10;
        private int m_rect2Start = 40;
        private int m_rect3Start = 60;
        private int m_rect4Start = 85;
        private int m_rect5Start = 107;
        private int m_rect6Start = 170;
        private int m_rect7Start = 200;
        private int m_rect8Start = 216;
        private int m_rect9Start = 230;
        private int m_rect10Start = 250;
        private int m_rect11Start = 270;
        private int m_rect12Start = 280;
        private int m_rect13Start = 290;
        private int m_rect14Start = 310;
        private int m_rect15Start = 320;
        private int m_rect16Start = 330;
        private int m_rect17Start = 340;

        private int m_rect1Width = 10;
        private int m_rect2Width = 10;
        private int m_rect3Width = 10;
        private int m_rect4Width = 10;
        private int m_rect5Width = 50;
        private int m_rect6Width = 10;
        private int m_rect7Width = 10;
        private int m_rect8Width = 10;
        private int m_rect9Width = 10;
        private int m_rect10Width = 10;
        private int m_rect11Width = 8;
        private int m_rect12Width = 6;
        private int m_rect13Width = 7;
        private int m_rect14Width = 7;
        private int m_rect15Width = 7;
        private int m_rect16Width = 7;
        private int m_rect17Width = 7;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_DrawRectangle(object sender, RoutedEventArgs e)
        {
            drawingCanvas.Children.Clear();
            var rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect1Start < 0)
            {
                m_rect1Width -= 6;
                m_rect1Start = 0;
            }

            rect.Width = Math.Max(0, m_rect1Width);
            Canvas.SetLeft(rect, m_rect1Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect2Start < 0)
            {
                m_rect2Width -= 6;
                m_rect2Start = 0;
            }

            rect.Width = Math.Max(0, m_rect2Width);
            Canvas.SetLeft(rect, m_rect2Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect3Start < 0)
            {
                m_rect3Width -= 6;
                m_rect3Start = 0;
            }

            rect.Width = Math.Max(0, m_rect3Width);
            Canvas.SetLeft(rect, m_rect3Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect4Start < 0)
            {
                m_rect4Width -= 6;
                m_rect4Start = 0;
            }

            rect.Width = Math.Max(0, m_rect4Width);
            Canvas.SetLeft(rect, m_rect4Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);
            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect5Start < 0)
            {
                m_rect5Width -= 6;
                m_rect5Start = 0;
            }

            rect.Width = Math.Max(0, m_rect5Width);
            Canvas.SetLeft(rect, m_rect5Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect6Start < 0)
            {
                m_rect6Width -= 6;
                m_rect6Start = 0;
            }

            rect.Width = Math.Max(0, m_rect6Width);
            Canvas.SetLeft(rect, m_rect6Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect7Start < 0)
            {
                m_rect7Width -= 6;
                m_rect7Start = 0;
            }

            rect.Width = Math.Max(0, m_rect7Width);
            Canvas.SetLeft(rect, m_rect7Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect8Start < 0)
            {
                m_rect8Width -= 6;
                m_rect8Start = 0;
            }

            rect.Width = Math.Max(0, m_rect8Width);
            Canvas.SetLeft(rect, m_rect8Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect9Start < 0)
            {
                m_rect9Width -= 6;
                m_rect9Start = 0;
            }

            rect.Width = Math.Max(0, m_rect9Width);
            Canvas.SetLeft(rect, m_rect9Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect10Start < 0)
            {
                m_rect10Width -= 6;
                m_rect10Start = 0;
            }

            rect.Width = Math.Max(0, m_rect10Width);
            Canvas.SetLeft(rect, m_rect10Start);

            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);
            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect11Start < 0)
            {
                m_rect11Width -= 6;
                m_rect11Start = 0;
            }

            rect.Width = Math.Max(0, m_rect11Width);
            Canvas.SetLeft(rect, m_rect11Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect12Start < 0)
            {
                m_rect12Width -= 6;
                m_rect12Start = 0;
            }

            rect.Width = Math.Max(0, m_rect12Width);
            Canvas.SetLeft(rect, m_rect12Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect13Start < 0)
            {
                m_rect13Width -= 6;
                m_rect13Start = 0;
            }

            rect.Width = Math.Max(0, m_rect13Width);
            Canvas.SetLeft(rect, m_rect13Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect14Start < 0)
            {
                m_rect14Width -= 6;
                m_rect14Start = 0;
            }

            rect.Width = Math.Max(0, m_rect14Width);
            Canvas.SetLeft(rect, m_rect14Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect15Start < 0)
            {
                m_rect15Width -= 6;
                m_rect15Start = 0;
            }

            rect.Width = Math.Max(0, m_rect15Width);
            Canvas.SetLeft(rect, m_rect15Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect16Start < 0)
            {
                m_rect16Width -= 6;
                m_rect16Start = 0;
            }

            rect.Width = Math.Max(0, m_rect16Width);
            Canvas.SetLeft(rect, m_rect16Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Fill = new SolidColorBrush(Color.FromRgb(224, 138, 85));
            rect.Height = 38;
            if (m_rect17Start < 0)
            {
                m_rect17Width -= 6;
                m_rect17Start = 0;
            }

            rect.Width = Math.Max(0, m_rect17Width);
            Canvas.SetLeft(rect, m_rect17Start);
            Canvas.SetTop(rect, 0);
            drawingCanvas.Children.Add(rect);

            m_rect1Start -= 6;
            m_rect2Start -= 6;
            m_rect3Start -= 6;
            m_rect4Start -= 6;
            m_rect5Start -= 6;
            m_rect6Start -= 6;
            m_rect7Start -= 6;
            m_rect8Start -= 6;
            m_rect9Start -= 6;
            m_rect10Start -= 6;
            m_rect11Start -= 6;
            m_rect12Start -= 6;
            m_rect13Start -= 6;
            m_rect14Start -= 6;
            m_rect15Start -= 6;
            m_rect16Start -= 6;
            m_rect17Start -= 6;
        }

        private void Button_DrawText(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = new TextBlock();
            DateTime now = DateTime.Now;
            int minutes = now.Minute;
            int shift = (minutes * 6) + (now.Second / 10);
            var wid = drawingCanvas.Width;
            var x = wid - shift;
            int y = (int)(17f - textBlock.FontSize / 2);

            now = now.AddMinutes(-minutes);
            textBlock.Text = now.ToString(DateFormat);
            var size = MeasureString(textBlock);
            textBlock.Foreground = new SolidColorBrush(Colors.Black);
            Canvas.SetLeft(textBlock, x - size.Width / 2);
            Canvas.SetTop(textBlock, y);
            drawingCanvas.Children.Add(textBlock);

            double nextShift = 0f;
            if (minutes > 30)
            {
                nextShift = x + 180;
                now = now.AddMinutes(30);
            }
            else
            {
                nextShift = x - 180;
                now = now.AddMinutes(-30);
            }

            x = nextShift - size.Width / 2;
            if (x < 0)
                x = 0;

            if (x > drawingCanvas.Width - size.Width)
                x = drawingCanvas.Width - size.Width;

            textBlock = new TextBlock();
            textBlock.Text = now.ToString(DateFormat);
            textBlock.Foreground = new SolidColorBrush(Colors.Black);
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            drawingCanvas.Children.Add(textBlock);
        }

        private Size MeasureString(TextBlock textBlock)
        {
            var formattedText = new FormattedText(
                textBlock.Text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }

        private void Button_DrawLine(object sender, RoutedEventArgs e)
        {
            DateTime now = DateTime.Now;
            int minutes = now.Minute;
            int seconds = now.Second;
            int shift = (minutes * 6) + (seconds / 10);
            var wid = (int)drawingCanvas.Width;
            int x = wid - shift;

            Line line = new Line();
            line.Stroke = new SolidColorBrush(Colors.Black);
            line.StrokeThickness = 1f;
            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            line.Opacity = 1;
            line.X1 = x;
            line.X2 = x;
            line.Y1 = 0;
            line.Y2 = 13;
            drawingCanvas.Children.Add(line);

            line = new Line();
            line.Stroke = new SolidColorBrush(Colors.Black);
            line.StrokeThickness = 1f;
            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            line.Opacity = 1;
            line.X1 = x;
            line.X2 = x;
            line.Y1 = 26;
            line.Y2 = 39;
            drawingCanvas.Children.Add(line);

            int nextShift = 0;
            if (minutes > 30)
                nextShift = x + 180;
            else
                nextShift = x - 180;

            if (nextShift >= 0)
            {
                line = new Line();
                line.Stroke = new SolidColorBrush(Colors.Black);
                line.StrokeThickness = 1f;
                line.SnapsToDevicePixels = true;
                line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                line.Opacity = 1;
                line.X1 = nextShift;
                line.X2 = nextShift;
                line.Y1 = 0;
                line.Y2 = 13;
                drawingCanvas.Children.Add(line);

                line = new Line();
                line.Stroke = new SolidColorBrush(Colors.Black);
                line.StrokeThickness = 1f;
                line.SnapsToDevicePixels = true;
                line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                line.Opacity = 1;
                line.X1 = nextShift;
                line.X2 = nextShift;
                line.Y1 = 26;
                line.Y2 = 39;
                drawingCanvas.Children.Add(line);
            }
        }

        private void Button_All(object sender, RoutedEventArgs e)
        {
            drawingCanvas.Children.Clear();
            Button_DrawLine(sender, e);
            Button_DrawText(sender, e);
        }

        private Line GetLine(double xValue, bool bUpper = true)
        {
            Line line = new Line();
            line.Stroke = new SolidColorBrush(Colors.Black);
            line.StrokeThickness = 1f;
            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            line.Opacity = 1;
            line.X1 = xValue;
            line.X2 = xValue;
            line.Y1 = bUpper ? 0 : 15;
            line.Y2 = bUpper ? 3 : 19;
            return line;
        }

        private void Button_DrawTime(object sender, RoutedEventArgs e)
        {
            //var lst1 = new List<A>();
            //lst1.Add(new A());
            //lst1.Add(new A());
            //lst1.Add(new A());
            //lst1.Add(new A());
            //lst1.Add(new A());
            //lst1.Add(new A());
            //lst1.Add(new A());
            //lst1.Add(new A());
            //lst1.Add(new A());

            //List<A> lst2 = new List<A>(lst1);

            //lst1.RemoveAt(0);
            //lst1.RemoveAt(0);
            //lst1.RemoveAt(0);

            drawingCanvas.Children.Clear();
            DateTime now = DateTime.Now;
            int minutes = now.Minute;
            int seconds = now.Second;
            int shift = (minutes % 30 * 6) + (seconds / 10);
            var wid = drawingCanvas.Width;
            var x = wid - shift;
            var line = GetLine(x);
            drawingCanvas.Children.Add(line);
            line = GetLine(x, false);
            drawingCanvas.Children.Add(line);

            TextBlock textBlock = GetTimeText(now, minutes, x);
            drawingCanvas.Children.Add(textBlock);

            now = now.AddMinutes(-30);
            minutes = now.Minute;
            seconds = now.Second;
            shift = (minutes * 6) + (seconds / 10);
            //wid = drawingCanvas.Width;
            x -= drawingCanvas.Width / 2;// wid - shift;
            line = GetLine(x);
            drawingCanvas.Children.Add(line);
            line = GetLine(x, false);
            drawingCanvas.Children.Add(line);

            textBlock = GetTimeText(now, minutes, x);
            drawingCanvas.Children.Add(textBlock);
        }

        private TextBlock GetTimeText(DateTime now, int minutes, double x)
        {
            TextBlock textBlock = new TextBlock();
            int y = (int)(6f - textBlock.FontSize / 2);

            if (minutes >= 30)
            {
                int minsToSub = minutes - 30;
                now = now.AddMinutes(-minsToSub);
            }
            else
            {
                now = now.AddMinutes(-minutes);
            }

            textBlock.Text = now.ToString(DateFormat);
            var size = MeasureString(textBlock);
            textBlock.Foreground = new SolidColorBrush(Colors.Black);
            Canvas.SetLeft(textBlock, x - size.Width / 2);
            Canvas.SetTop(textBlock, y);
            return textBlock;
        }
    }

    public class A
    {
        public A()
        {
            Str = DateTime.Now.ToShortTimeString();
        }

        public String Str { get; set; }
    }
}
