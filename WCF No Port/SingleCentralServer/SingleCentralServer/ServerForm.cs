// Lance Roberts 04-Mar-2010
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MessagingInterfaces;
using System.ServiceModel;

namespace SingleCentralServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public partial class ServerForm : Form, IFromClientToServerMessages
    {
        ServiceHost _serverHost;
        List<Guid> _registeredClients = new List<Guid>();
        public ServerForm()
        {
            InitializeComponent();

            _serverHost = new ServiceHost(this);

            _serverHost.AddServiceEndpoint((typeof(IFromClientToServerMessages)), new NetNamedPipeBinding(), "net.pipe://localhost/Server");
            _serverHost.Open();
        }

        private void broadcast_btn_Click(object sender, EventArgs e)
        {
            foreach (Guid client in _registeredClients)
            {
                SendText(client, textToSendToClient_tb.Text);
            }
        }

        private void SendText(Guid client, string text)
        {
            using (ChannelFactory<IFromServerToClientMessages> factory = new ChannelFactory<IFromServerToClientMessages>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/Client_" + client.ToString())))
            {
                IFromServerToClientMessages serverToClientChannel = factory.CreateChannel();
                try
                {
                    serverToClientChannel.DisplayTextInClient(text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    CloseChannel((ICommunicationObject)serverToClientChannel);
                }
            }
        }

        private void CloseChannel(ICommunicationObject channel)
        {
            try
            {
                channel.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                channel.Abort();
            }
        }



        private void unregisterClient_btn_Click(object sender, EventArgs e)
        {
            Guid client = new Guid(clients_lb.SelectedItem.ToString());
            _registeredClients.Remove(client);
            clients_lb.Items.Remove(clients_lb.SelectedItem.ToString());
        }

        private void selectClientSend_btn_Click(object sender, EventArgs e)
        {
            Guid client = new Guid(clients_lb.SelectedItem.ToString());
            SendText(client, textToSendToClient_tb.Text);
        }


        #region IFromClientToServerMessages Members

        public void Register(Guid clientID)
        {
            if (!_registeredClients.Contains(clientID))
                _registeredClients.Add(clientID);
            clients_lb.Items.Add(clientID.ToString());
        }

        public void DisplayTextOnServer(string text)
        {
            anon_tb.Text = text;
        }

        public void DisplayTextOnServerAsFromThisClient(Guid clientID, string text)
        {
            clients_lb.Items.Add(clientID.ToString() + " - " + text);
        }

        public string GetLastAnonMessage()
        {
            return anon_tb.Text;
        }

        #endregion
    }
}
