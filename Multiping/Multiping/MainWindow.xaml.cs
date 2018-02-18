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
using System.Net.NetworkInformation;
using Microsoft.Practices.Prism.ViewModel;
using System.Collections.ObjectModel;

namespace Multiping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PingObjects = new PingObject();
            this.DataContext = PingObjects;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var pingSender = new Ping();
            var options = new PingOptions();
            options.DontFragment = false;

            // Create a buffer of 32 bytes of data to be transmitted. 
            string data = "0123456789";
            String toAdd = "abcdef";
            for (int i = 0; i < 12; i++)
            {
                data += data + toAdd;
            }

            data = data.Remove(65500);
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 180;
            //for (int i = 0; i < 20; i++)
            //{
            //    Task.Factory.StartNew(() =>
            //    {
            //        PingReply asyncReply = pingSender.Send(PingObjects.Host, timeout, buffer, options);
            //    });
            //}

            for (; ; )
            {
                PingReply reply = pingSender.Send(PingObjects.Host, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
                    String toList = String.Format("Address: {0}, RoundTrip time: {1}, Time to live: {2}, Buffer size: {3}",
                        reply.Address.ToString(), reply.RoundtripTime, reply.Options.Ttl, reply.Buffer.Length);
                    PingObj thisObj = new PingObj()
                    {
                        Response = toList
                    };

                    PingObjects.PingObjs.Add(thisObj);
                }
            }
        }

        public PingObject PingObjects { get; set; }
    }

    public class PingObject
    {
        public PingObject()
        {
            PingObjs = new ObservableCollection<PingObj>();
        }

        public String Host { get; set; }
        public ObservableCollection<PingObj> PingObjs { get; set; }
    }

    public class PingObj
    {
        public PingObj()
        {
        }

        private String m_response;
        public String Response
        {
            get 
            {
                return m_response;
            }
            set 
            {
                m_response = value;
            }
        }
    }
}
