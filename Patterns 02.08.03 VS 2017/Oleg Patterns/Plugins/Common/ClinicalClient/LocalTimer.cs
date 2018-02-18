using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace PatternsCRIClient
{
    public class LocalTimer : INotifyPropertyChanged
    {
        private DispatcherTimer m_timer;

        public LocalTimer()
        {
            m_timer = new DispatcherTimer();
            m_timer.Interval = TimeSpan.FromSeconds(1.0);
            m_timer.Tick += new EventHandler(TimerCallback);
            this.TimeFormat = "HH:mm";
        }

        private void TimerCallback(object sender, EventArgs e)
        {
            PropertyChanged(this, new PropertyChangedEventArgs("FormattedTime"));
        }

        public bool Enabled
        {
            get { return this.m_timer.IsEnabled; }
            set { if (value) this.m_timer.Start(); else this.m_timer.Stop(); }
        }

        public string FormattedTime 
        { 
            get 
            { 
                return DateTime.Now.ToString(this.TimeFormat); 
            } 
            set { } 
        }

        public string TimeFormat { get; set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
