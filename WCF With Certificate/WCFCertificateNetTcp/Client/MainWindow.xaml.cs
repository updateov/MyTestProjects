using Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonGetStartTime_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                NetTcpBinding bind = new NetTcpBinding(SecurityMode.Transport);
                //String url = "net.tcp://localhost:7103/WCFTestService";
                bind.Security.Mode = SecurityMode.Transport;
                bind.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
                String url = ConfigurationManager.AppSettings["ServerDataURL"];
                EndpointAddress addr = new EndpointAddress(url);
                ChannelFactory<ITestInterface> chn = new ChannelFactory<ITestInterface>(bind, addr);
                chn.Credentials.ClientCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.TrustedPeople,
                    X509FindType.FindBySubjectName, "OlegClient");
                ITestInterface conn = chn.CreateChannel();
                var res = conn.GetStartTime();
                TextBoxStartTime.Text = res.StartTime.ToString();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
