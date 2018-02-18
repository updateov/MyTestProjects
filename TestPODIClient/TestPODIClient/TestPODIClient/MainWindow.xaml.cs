using PeriCALMOutboundDataInterfaceAPI;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Xml.Linq;

namespace TestPODIClient
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

        private void buttonGet_Click(object sender, RoutedEventArgs e)
        {
            WSHttpBinding bind = new WSHttpBinding(SecurityMode.None);
            EndpointAddress addr = new EndpointAddress("http://localhost:7100/PeriGenOutbound/");
            ChannelFactory<IPeriGenOutbound> chn = new ChannelFactory<IPeriGenOutbound>(bind, addr);
            IPeriGenOutbound conn = chn.CreateChannel();
            String xml = "<request system_name=\"HUB\" calm_version=\"01.01.00\"><attributes><tracings></tracings></attributes></request>";
            XElement req = XElement.Parse(xml);
            //XElement res = conn.GetData(req);
            XElement res = conn.GetData(null);
            textBoxResponse.Text = res.ToString();
        }
    }
}
