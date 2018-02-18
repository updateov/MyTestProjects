//Review: 27/04/15
using PatternsCALMMediator;
using PatternsCRIClient.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace PatternsCRIClient.Controls
{
    /// <summary>
    /// Interaction logic for TracingControl.xaml
    /// </summary>
    public partial class TracingControl : UserControl
    {
        private const String DateFormat = "HH:mm";

        private PatientData m_currentPatient;

        public TracingControl()
        {
            InitializeComponent();

            App.ClientManager.UpdateDataEvent += ClientManager_DataUpdated;
        }

        void ClientManager_DataUpdated(object sender, UpdateDataEventArgs e)
        {
            if (App.ClientManager.CurrentPatient != null)
            {
                m_currentPatient = new PatientData();
                m_currentPatient.CopyData(App.ClientManager.CurrentPatient);
                Draw();
            }
        }

        private void tracingControl_Loaded(object sender, RoutedEventArgs e)
        {
            Draw();
        }

        private void Draw()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (m_currentPatient == null)
                    return;

                drawingCanvas.Children.Clear();
                DateTime now = DateTime.Now;
                DrawContractility(now);
                DrawTime(now);
            }));
        }

        // TODO: explain the method
        private void DrawContractility(DateTime time)
        {
            double xLeft = 0f;
            DateTime leftBoundTime = time.AddHours(-1);
            var contractilities = from c in m_currentPatient.LastHourContractilities
                                  where c.EndTime > leftBoundTime
                                  orderby c.EndTime
                                  select c;
            foreach (var item in contractilities)
            {
                var rect = new Rectangle();
                // TODO: comment or const
                rect.Height = 18;

                double rectWidth = 0f;
                if (xLeft == 0)
                    rectWidth = ((item.EndTime - (item.StartTime < leftBoundTime ? leftBoundTime : item.StartTime)).TotalSeconds + 1) / 10;
                else
                    rectWidth = ((item.EndTime - item.StartTime).TotalSeconds + 1) / 10;
           
                rect.Width = rectWidth;
                rect.Height = 18;

                Color color;
                switch (item.State)
                {
                    case CRIEntities.ContractilityState.Unknown:
                        color = Color.FromRgb(163, 163, 163);
                        break;
                    case CRIEntities.ContractilityState.Normal:
                        color = Color.FromRgb(234, 234, 234);
                        break;
                    case CRIEntities.ContractilityState.Alert:
                        color = Color.FromRgb(224, 138, 85);
                        break;
                    case CRIEntities.ContractilityState.Danger:
                        color = Color.FromRgb(224, 138, 85);
                        break;
                    default:
                        color = Color.FromRgb(163, 163, 163);
                        break;
                }

                rect.Stroke = new SolidColorBrush(color);
                rect.Fill = new SolidColorBrush(color);
                Canvas.SetLeft(rect, xLeft);
                Canvas.SetTop(rect, 0);
                drawingCanvas.Children.Add(rect);

                xLeft += rectWidth;
            }

            if (xLeft < drawingCanvas.Width)
            {
                var rect = new Rectangle();
                rect.Height = 18;
                rect.Width = drawingCanvas.Width - xLeft;
                rect.Stroke = new SolidColorBrush(Color.FromRgb(163,163,163));
                rect.Fill = new SolidColorBrush(Color.FromRgb(163, 163, 163));
                Canvas.SetLeft(rect, xLeft);
                Canvas.SetTop(rect, 0);
                drawingCanvas.Children.Add(rect);
            }
        }

        private void DrawTime(DateTime time)
        {
            var xPos = GetXPosition(time);
            var line = GetLine(xPos);
            drawingCanvas.Children.Add(line);
            line = GetLine(xPos, false);
            drawingCanvas.Children.Add(line);

            TextBlock textBlock = GetTimeText(time, xPos);
            drawingCanvas.Children.Add(textBlock);

            time = time.AddMinutes(-30);
            xPos -= drawingCanvas.Width / 2; 
            line = GetLine(xPos);
            drawingCanvas.Children.Add(line);
            line = GetLine(xPos, false);
            drawingCanvas.Children.Add(line);

            textBlock = GetTimeText(time, xPos);
            drawingCanvas.Children.Add(textBlock);
        }

        private double GetXPosition(DateTime time)
        {
            int minutes = time.Minute;
            int seconds = time.Second;
            int shift = (minutes % 30 * 6) + (seconds / 10);
            double toRet = drawingCanvas.Width - shift;
            return toRet;
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

        private TextBlock GetTimeText(DateTime now, double x)
        {
            TextBlock textBlock = new TextBlock();
            int yPos = (int)(7.0 - textBlock.FontSize / 2);
            int minutes = now.Minute;

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
            Canvas.SetTop(textBlock, yPos);
            return textBlock;
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
    }
}
