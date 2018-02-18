using MessagingInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MainServerApplication
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public partial class Form1 : Form, IChildToMainMessages
    {
        public ServiceHost Host { get; private set; }
        private List<Guid> m_registeredClients = new List<Guid>();

        public Form1()
        {
            InitializeComponent();
            labelChildrenCount.Text = m_registeredClients.Count.ToString();

            Host = new ServiceHost(this);
            Host.AddServiceEndpoint((typeof(IChildToMainMessages)), new NetNamedPipeBinding(), "net.pipe://localhost/MainServer");
            Host.Open();
        }

        #region IChildToMainMessages Implementation

        public bool RegisterChild(Guid guid)
        {
            if (m_registeredClients.Contains(guid))
            {
                MessageBox.Show("Client already exists");
                return false;
            }

            m_registeredClients.Add(guid);
            listBoxClients.Items.Add(guid.ToString());
            labelChildrenCount.Text = m_registeredClients.Count.ToString();
            return true;
        }

        #endregion IChildToMainMessages implementation

        private void buttonCreateChild_Click(object sender, EventArgs e)
        {
            Guid guid = Guid.NewGuid();
            try
            {
                Process.Start("ChildClientApplication.exe", guid.ToString());
                m_registeredClients.Add(guid);
                listBoxClients.Items.Add(guid.ToString());
                labelChildrenCount.Text = m_registeredClients.Count.ToString();
            }
            catch (Exception ex)
            {
            }
        }

        private void buttonRemoveSelected_Click(object sender, EventArgs e)
        {
            String guidStr = listBoxClients.SelectedItem.ToString();
            Guid guid = (from c in m_registeredClients
                         where c.ToString().Equals(guidStr)
                         select c).FirstOrDefault();

            if (guid == null)
                return;

            RemoveChild(guidStr);
            m_registeredClients.Remove(guid);
            listBoxClients.Items.Remove(guidStr);
            labelChildrenCount.Text = m_registeredClients.Count.ToString();
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            foreach (var item in m_registeredClients)
            {
                RemoveChild(item.ToString());
            }

            m_registeredClients.Clear();
            listBoxClients.Items.Clear();
            labelChildrenCount.Text = m_registeredClients.Count.ToString();
        }

        private void buttonFill_Click(object sender, EventArgs e)
        {
            String guidStr = listBoxClients.SelectedItem.ToString();
            Guid guid = (from c in m_registeredClients
                         where c.ToString().Equals(guidStr)
                         select c).FirstOrDefault();

            if (guid == null)
                return;

            using (ChannelFactory<IMainToChildMessages> factory = new ChannelFactory<IMainToChildMessages>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/Child_" + guidStr)))
            {
                IMainToChildMessages channel = factory.CreateChannel();
                try
                {
                    String start = textBoxStartIndex.Text;
                    int nStart;
                    Int32.TryParse(start, out nStart);
                    String length = textBoxLength.Text;
                    int nLength;
                    Int32.TryParse(length, out nLength);
                    String val = textBoxValue.Text;
                    double nVal;
                    Double.TryParse(val, out nVal);
                    channel.FillArray(nStart, nLength, nVal);
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    CloseChannel((ICommunicationObject)channel);
                }
            }
        }

        private void buttonGet_Click(object sender, EventArgs e)
        {
            String guidStr = listBoxClients.SelectedItem.ToString();
            Guid guid = (from c in m_registeredClients
                         where c.ToString().Equals(guidStr)
                         select c).FirstOrDefault();

            if (guid == null)
                return;

            using (ChannelFactory<IMainToChildMessages> factory = new ChannelFactory<IMainToChildMessages>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/Child_" + guidStr)))
            {
                IMainToChildMessages channel = factory.CreateChannel();
                try
                {
                    String start = textBoxStartIndex.Text;
                    int nStart;
                    Int32.TryParse(start, out nStart);
                    String length = textBoxLength.Text;
                    int nLength;
                    Int32.TryParse(length, out nLength);
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    CloseChannel((ICommunicationObject)channel);
                }
            }
        }

        private void RemoveChild(string guidStr)
        {
            using (ChannelFactory<IMainToChildMessages> factory = new ChannelFactory<IMainToChildMessages>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/Child_" + guidStr)))
            {
                IMainToChildMessages channel = factory.CreateChannel();
                try
                {
                    channel.TerminateProcess();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    CloseChannel((ICommunicationObject)channel);
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
            }
            finally
            {
                channel.Abort();
            }
        }

        private void buttonGetBuffer_Click(object sender, EventArgs e)
        {
            String guidStr = listBoxClients.SelectedItem.ToString();
            Guid guid = (from c in m_registeredClients
                         where c.ToString().Equals(guidStr)
                         select c).FirstOrDefault();

            if (guid == null)
                return;

            using (ChannelFactory<IMainToChildMessages> factory = new ChannelFactory<IMainToChildMessages>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/Child_" + guidStr)))
            {
                IMainToChildMessages channel = factory.CreateChannel();
                try
                {
                    String str = channel.GetStringFromBuffer();
                    textBoxBuffer.Text = str;
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    CloseChannel((ICommunicationObject)channel);
                }
            }
        }
    }
}