using MessagingInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.ServiceModel;
using ChildClientBridge;
namespace ChildClientApplication
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            String guid = String.Empty;
            if (!args[0].Equals(String.Empty))
            {
                guid = args[0];
            }

            MyApplicationContext context = new MyApplicationContext(guid);
            Application.Run(context);
        }
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public class MyApplicationContext : ApplicationContext, IMainToChildMessages
    {
        ServiceHost Host { get; set; }

        EngineBridge Engine { get; set; }

        public MyApplicationContext(String guid)
        {
            Host = new ServiceHost(this);

            NetNamedPipeBinding binding = new NetNamedPipeBinding()
            {
                MaxReceivedMessageSize = 2147483647,
                CloseTimeout = new TimeSpan(0, 1, 0),
                OpenTimeout = new TimeSpan(0, 1, 0),
                ReceiveTimeout = new TimeSpan(0, 3, 0),
                SendTimeout = new TimeSpan(0, 1, 0)
            };

            Host.AddServiceEndpoint((typeof(IMainToChildMessages)), binding, "net.pipe://localhost/Child_" + guid);
            Host.Open();

            Engine = new EngineBridge();

            //RegisterClient();
        }

        //private void RegisterClient()
        //{
        //    using (ChannelFactory<IChildToMainMessages> channelFactory = new ChannelFactory<IChildToMainMessages>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/MainServer")))
        //    {
        //        IChildToMainMessages channel = channelFactory.CreateChannel();
        //        try
        //        {
        //            channel.RegisterChild(GUID);
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //        finally
        //        {
        //            CloseChannel((ICommunicationObject)channel);
        //        }
        //    }

        //}

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

        public int EngineProcessHR(string visitKey, byte[] ups, int position, int blockSize)
        {
            throw new NotImplementedException();
        }

        public int EngineProcessUP(string visitKey, byte[] ups, int position, int blockSize)
        {
            throw new NotImplementedException();
        }

        public bool EngineReadResults(string visitKey, StringBuilder data, int bufferSize)
        {
            throw new NotImplementedException();
        }

        public void TerminateProcess()
        {
            ExitThread();
        }

        public void FillArray(int startIndex, int length, double val)
        {
            Engine.LoadArrayNumbers(startIndex, length, val);
        }

        public List<double> GetSubArray(int start, int length)
        {
            return Engine.GetSubArray(start, length);
        }

        public string GetStringFromBuffer()
        {
            return Engine.StringBuilderTest();
        }
    }
}
