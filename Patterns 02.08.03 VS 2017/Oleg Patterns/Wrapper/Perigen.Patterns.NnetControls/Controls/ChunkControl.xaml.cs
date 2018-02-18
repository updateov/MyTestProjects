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

namespace Perigen.Patterns.NnetControls.Controls
{
    /// <summary>
    /// Interaction logic for ChunkControl.xaml
    /// </summary>
    public partial class ChunkControl : UserControl
    {
        private int m_intervalID = -1;
        private PatternsUIManager UiManager { get; set; }

        public DateTime StartTime
        {
            get { return (DateTime)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }
        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register("StartTime", typeof(DateTime), typeof(ChunkControl), new UIPropertyMetadata(DateTime.MinValue));

        public int TimeRange
        {
            get { return (int)GetValue(TimeRangeProperty); }
            set { SetValue(TimeRangeProperty, value); }
        }
        public static readonly DependencyProperty TimeRangeProperty =
            DependencyProperty.Register("TimeRange", typeof(int), typeof(ChunkControl), new UIPropertyMetadata(30));

        public bool IsExported
        {
            get { return (bool)GetValue(IsExportedProperty); }
            set { SetValue(IsExportedProperty, value); }
        }
        public static readonly DependencyProperty IsExportedProperty =
            DependencyProperty.Register("IsExported", typeof(bool), typeof(ChunkControl), new UIPropertyMetadata(false));


        public ChunkControl(PatternsUIManager uiManager, DateTime startTime, int timeRange, bool isExported, int intervalID)
        {
            UiManager = uiManager;
            StartTime = startTime;
            TimeRange = timeRange;
            IsExported = isExported;
            m_intervalID = intervalID;

            InitializeComponent();

            this.LblStartTime.Text = startTime.ToString("H:mm");
            this.Width = Math.Ceiling(UiManager.PixelsInMinute * timeRange);

            if (IsExported == true)
            {
                Color color = (Color)ColorConverter.ConvertFromString("#f2f2f2");
                this.mainGrid.Background = new SolidColorBrush(color);
            }
            else
            {
                Color color = (Color)ColorConverter.ConvertFromString("#e7f4fe");
                this.mainGrid.Background = new SolidColorBrush(color);
            }

            bool isSelect = StartTime == UiManager.SelectedChunkStartTime;
            SetSelected(isSelect);
        }

        private void chunkControl_MouseEnter(object sender, MouseEventArgs e)
        {
            SetSelected(true);

            UiManager.RaiseMouseOverEvent(StartTime, StartTime.AddMinutes(TimeRange));
        }

        private void chunkControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.IsExported == false)
            {
                SetSelected(true);
                UiManager.StartExport(this.StartTime, this.TimeRange);
            }
            else
            {
                UiManager.RaiseBtnPressedEvent(this.StartTime, this.StartTime.AddMinutes(this.TimeRange), m_intervalID);
            }
        }

        private void chunkControl_MouseLeave(object sender, MouseEventArgs e)
        {
            SetSelected(false);

            UiManager.RaiseMouseLeaveEvent(StartTime, StartTime.AddMinutes(TimeRange));
        }

        public void SetSelected(bool isSelect)
        {
            if (isSelect == true)
            {
                Color color = (Color)ColorConverter.ConvertFromString("#dfdfdf");
                this.line1.Stroke = new SolidColorBrush(color);
                this.line2.Stroke = new SolidColorBrush(color);
                this.mainGrid.Background = new SolidColorBrush(color);

                if (IsExported == false)
                {
                    this.mainGrid.ToolTip = "Ready to Export";
                }
            }
            else
            {
                this.line1.Stroke = new SolidColorBrush(Colors.White);
                this.line2.Stroke = new SolidColorBrush(Colors.White);

                if (IsExported == true)
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#f2f2f2");
                    this.mainGrid.Background = new SolidColorBrush(color);
                    this.mainGrid.ToolTip = "Export done";
                }
                else
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#e7f4fe");
                    this.mainGrid.Background = new SolidColorBrush(color);
                }
            }
        }
    }
}
