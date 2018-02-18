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

namespace PatternsCRIClient.Screens
{
    /// <summary>
    /// Interaction logic for AutomaticMessageWindow.xaml
    /// </summary>
    public partial class AutomaticMessageWindow : Window
    {
        private Storyboard m_gridFadeInStoryBoard;
        private Timer m_timer;

        public AutomaticMessageWindow()
        {
            InitializeComponent();
            m_timer = new Timer(5000);
        }

        private void btnClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void automaticMessageWindow_Loaded(object sender, RoutedEventArgs e)
        {
            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("gridFadeInMessage");
            m_gridFadeInStoryBoard.Begin();

            m_timer.Elapsed += timer_Elapsed;
            m_timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {            
                this.Close();
            }));
        }

        private void automaticMessageWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
