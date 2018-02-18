using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
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
    /// Interaction logic for UpgradeVersionWindow.xaml
    /// </summary>
    public partial class UpgradeVersionWindow : Window
    {
        public int CountDown
        {
            get { return (int)GetValue(CountDownProperty); }
            set { SetValue(CountDownProperty, value); }
        }

        public static readonly DependencyProperty CountDownProperty =
            DependencyProperty.Register("CountDown", typeof(int), typeof(UpgradeVersionWindow), new PropertyMetadata(60));

        private DispatcherTimer m_timer;
        private Storyboard m_gridFadeInStoryBoard;

        public UpgradeVersionWindow()
        {
            InitializeComponent();

            m_timer = new DispatcherTimer();
            m_timer.Interval = TimeSpan.FromSeconds(1.0);
            m_timer.Tick += TimerCallback;
            m_timer.IsEnabled = true;
    
            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("gridFadeInMessage");
        }

        private void TimerCallback(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {  
                CountDown--;

                if (CountDown == -1)
                {
                    CloseMessage();
                }
            }));
        }

        private void CloseMessage()
        {
            m_timer.Stop();
            m_timer.Tick -= TimerCallback;

            App.StopProcessIfExist();
            App.Current.Shutdown();
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
            this.Width = 0;
            this.Height = 0;
        }

        private void upgradeVersionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_gridFadeInStoryBoard != null)
            {
                m_gridFadeInStoryBoard.Begin();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseMessage();
        }      
    }
}
