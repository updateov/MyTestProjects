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

namespace MultipleClientsThatAreAlsoWCFServers
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public partial class ClientForm : Form, IFromServerToClientMessages
    {
        Guid _clientID;
        ServiceHost _clientHost;

        public ClientForm()
        {
            InitializeComponent();

            _clientID = Guid.NewGuid();
            _clientHost = new ServiceHost(this);

            _clientHost.AddServiceEndpoint((typeof(IFromServerToClientMessages)), new NetNamedPipeBinding(), "net.pipe://localhost/Client_" + _clientID.ToString());
            _clientHost.Open();
        }

        private void unregister_Click(object sender, EventArgs e)
        {
            // deliberately unimplmented
            MessageBox.Show("This can't be done sometimes, for some clients, so better to have the server handling it");
        }

        private void Register_btn_Click(object sender, EventArgs e)
        {
            Register(_clientID);
        }

        public void Register(Guid clientID)
        {
            using (ChannelFactory<IFromClientToServerMessages> factory = new ChannelFactory<IFromClientToServerMessages>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/Server")))
            {
                IFromClientToServerMessages clientToServerChannel = factory.CreateChannel();
                try
                {
                    clientToServerChannel.Register(clientID);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    CloseChannel((ICommunicationObject)clientToServerChannel);
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

        private void SendText_btn_Click(object sender, EventArgs e)
        {
            string text = textToSend_tb.Text;
            using (ChannelFactory<IFromClientToServerMessages> factory = new ChannelFactory<IFromClientToServerMessages>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/Server")))
            {
                IFromClientToServerMessages clientToServerChannel = factory.CreateChannel();
                try
                {
                    clientToServerChannel.DisplayTextOnServerAsFromThisClient(_clientID, text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    CloseChannel((ICommunicationObject)clientToServerChannel);
                }
            }
        }

        #region IFromServerToClientMessages Members

        public void DisplayTextInClient(string text)
        {
            lastMessage_tb.Text = text;
        }

        #endregion

        private void sendAnonymously_btn_Click(object sender, EventArgs e)
        {
            string text = textToSend_tb.Text;
            using (ChannelFactory<IFromClientToServerMessages> factory = new ChannelFactory<IFromClientToServerMessages>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/Server")))
            {
                IFromClientToServerMessages clientToServerChannel = factory.CreateChannel();
                try
                {
                    clientToServerChannel.DisplayTextOnServer(text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    CloseChannel((ICommunicationObject)clientToServerChannel);
                }
            }
        }

        private void getLastAnon_btn_Click(object sender, EventArgs e)
        {
            lastAnon_tb.Text = getLastAnon();
        }

        private string getLastAnon()
        {
            using (ChannelFactory<IFromClientToServerMessages> factory = new ChannelFactory<IFromClientToServerMessages>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/Server")))
            {
                IFromClientToServerMessages clientToServerChannel = factory.CreateChannel();
                try
                {
                    return clientToServerChannel.GetLastAnonMessage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    CloseChannel((ICommunicationObject)clientToServerChannel);
                }
            }

            return "";
        }
    }
}
