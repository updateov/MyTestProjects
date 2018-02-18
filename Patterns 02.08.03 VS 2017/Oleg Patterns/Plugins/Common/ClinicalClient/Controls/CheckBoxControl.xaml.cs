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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PatternsCRIClient.Controls
{
    /// <summary>
    /// Interaction logic for CheckBoxControl.xaml
    /// </summary>
    public partial class CheckBoxControl : UserControl
    {
        private Storyboard m_gridFadeInStoryBoard;
        private Storyboard m_gridFadeOutStoryBoard;
        private Timer m_timer;

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CheckBoxControl), new PropertyMetadata(String.Empty));

        public bool IsChecked 
        { 
            get
            {
                return (bool)chkReviewed.IsChecked;
            }
            set
            {
                chkReviewed.IsChecked = value;
            }
        }
       
        public CheckBoxControl()
        {
            InitializeComponent();

            m_timer = new Timer(5000);
            m_timer.Elapsed += timer_Elapsed;

            chkReviewed.Checked += chkReviewed_Checked;

            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("gridFadeIn");
            m_gridFadeOutStoryBoard = (Storyboard)this.TryFindResource("gridFadeOut");
        }

        public void Init()
        {
            if (m_timer.Enabled == true)
            {
                m_timer.Stop();
            }

            m_gridFadeOutStoryBoard.Stop();

            if (this.Opacity == 0)
            {
                m_gridFadeInStoryBoard.Begin();
            }

            chkReviewed.IsChecked = false;
            chkReviewed.IsEnabled = true;
        }

        public void CheckAndDisappear()
        {
            if (chkReviewed.IsChecked == false)
            {
                chkReviewed.IsChecked = true;
            }
            chkReviewed.IsEnabled = false;

            m_timer.Start();         
        }

        public void Disappear()
        {
            if (this.Opacity != 0)
            {
                chkReviewed.IsEnabled = false;
                m_timer.Start();
            }
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                m_gridFadeOutStoryBoard.Begin();
                m_timer.Stop();
            }));
        }

        public event EventHandler<RoutedEventArgs> Checked;
        public void FireEventChecked()
        {
            var tempHandler = Checked;
            if (tempHandler != null)
            {
                tempHandler(this, new RoutedEventArgs());
            }
        }

        void chkReviewed_Checked(object sender, RoutedEventArgs e)
        {
            FireEventChecked();
        }       
    }
}
