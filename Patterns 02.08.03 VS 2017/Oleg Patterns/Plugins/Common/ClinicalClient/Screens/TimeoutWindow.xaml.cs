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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PatternsCRIClient.Screens
{
    /// <summary>
    /// Interaction logic for TimeoutWindow.xaml
    /// </summary>
    public partial class TimeoutWindow : Window
    {
        public int CountDown
        {
            get { return (int)GetValue(CountDownProperty); }
            set { SetValue(CountDownProperty, value); }
        }

        public static readonly DependencyProperty CountDownProperty =
            DependencyProperty.Register("CountDown", typeof(int), typeof(TimeoutWindow), new PropertyMetadata(0));

        private DispatcherTimer m_timer;
        private Storyboard m_gridFadeInStoryBoard;
        private Point m_LastPoint;

        public TimeoutWindow(Rect dimensions)
        {
            CountDown = 30;

            InitializeComponent();

            this.Top = dimensions.Top;
            this.Left = dimensions.Left;
            this.Width = dimensions.Width;
            this.Height = dimensions.Height;

            Color color = Color.FromArgb(130, 63, 63, 63);
            this.Background = new SolidColorBrush(color);

            m_timer = new DispatcherTimer();
            m_timer.Interval = TimeSpan.FromSeconds(1.0);
            m_timer.Tick += TimerCallback;
            m_timer.IsEnabled = true;

            var transform = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformFromDevice;
            m_LastPoint = transform.Transform(GetMousePosition());

            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("gridFadeInMessage");
        }

        private void TimerCallback(object sender, EventArgs e)
        {
            CountDown--;

            if(CountDown == -1)
            {
                CloseMessage(true);
            }
        }

        public System.Windows.Point GetMousePosition()
        {
            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
            return new System.Windows.Point(point.X, point.Y);
        }

        private void CloseMessage(bool result)
        {
            m_timer.Stop();
            this.DialogResult = result;
            this.Close();
        }

        private void timeoutWindow_Loaded(object sender, RoutedEventArgs e)
        {
            m_gridFadeInStoryBoard.Begin();

            this.MouseMove += timeoutWindow_MouseMove;
            this.KeyDown += timeoutWindow_KeyDown;
        }

        private void btnClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseMessage(false);
        }

        private void timeoutWindow_MouseMove(object sender, MouseEventArgs e)
        {
            var transform = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformFromDevice;
            var mouse = transform.Transform(GetMousePosition());

            if (m_LastPoint != mouse)
            {
                m_LastPoint = mouse;
                CloseMessage(false);
            }
        }

        private void timeoutWindow_KeyDown(object sender, KeyEventArgs e)
        {
            CloseMessage(false);
        }
    }
}
