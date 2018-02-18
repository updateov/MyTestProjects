using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestClient.NetTCPService;

namespace TestClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonGetData_Click(object sender, EventArgs e)
        {
            var client = new NetTCPServiceClient();
            MessageBox.Show(client.GetData(123), "My Service");
            client.Close();
        }

        private void buttonGetContextData_Click(object sender, EventArgs e)
        {
            var client = new NetTCPServiceClient();
            CompositeType t = new CompositeType() { BoolValue = true, StringValue = "Bla " };
            var res = client.GetDataUsingDataContract(t);
            MessageBox.Show(res.StringValue, "My Service");
            client.Close();
        }
    }
}
