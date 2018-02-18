using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Threading;
using RestSharp;




namespace reader2
{
    public partial class Form1 : Form
    {
        private static System.Windows.Forms.Timer aTimer;

        static int requestInterval, duration;
        static bool bFirstTime = true;
        static bool bFirstTimeCRIStatusEvents = true;
        static float durationCounter = 0;
        string[] arg;

        public Form1()
        {
            InitializeComponent();
            arg = Environment.GetCommandLineArgs();
            if (arg.Count() > 1)
            {         
                requestInterval = Convert.ToInt32(arg[1]);
                RequestInterval.Text = arg[1];

                duration = Convert.ToInt32(arg[2]);
                Duration.Text = arg[2];

                Outputfile.Text = arg[3];
                readCRI();
        }
        }

        private void Start_Click(object sender, EventArgs e)
        {
            readCRI();
        }
        private void readCRI()
        {
            Start.Enabled = false;

            try
            {
                requestInterval = Convert.ToInt32(RequestInterval.Text);
                duration = Convert.ToInt32(Duration.Text);
                progressBar1.Minimum = 0;
                progressBar1.Maximum = duration * 60 / requestInterval;
                progressBar1.Step = 1;

                progressBar1.PerformStep();
                if (requestInterval <= 0 || duration <= 0)
                    throw new Exception();

            }
            catch (Exception)
            {
                MessageBox.Show("Only valid numbers ");
                return;
                
            }


            aTimer = new System.Windows.Forms.Timer();
            aTimer.Interval = requestInterval * 1000;
            aTimer.Tick+= new EventHandler(OnTimedEvent);
            aTimer.Enabled = true;

        }
        private void OnTimedEvent(Object source, EventArgs e)
        {
            //var sb = BuildQuery();

            if (durationCounter < duration * 60)
            {
                
                var sb = BuildQuery();
               
                durationCounter += requestInterval;
                var result = PatternsDataIO.SendRequest("http://localhost:7200/PatternsPlugins/CRIPlugin/", sb);

                WriteToFile(result);

                progressBar1.PerformStep();
            }
            else
            {

            Start.Enabled = true;
                progressBar1.Value = 0;
                aTimer.Enabled = false;
            }
        }
            
        private string BuildQuery()
        {
            // Build the query
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Auto }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("request");

                writer.WriteStartElement("visit");
                writer.WriteAttributeString("key", "1-25-1-1" ?? string.Empty);
                writer.WriteRaw("10");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            return sb.ToString();
        }
        private void CRIack(string visitKey)
        {
            var client = new RestClient("http://localhost:7200/PatternsPlugins/CRIPlugin/");
            var request = new RestRequest("CRIEpisodes/Review/{visitkey}/user/{username}", Method.POST);
            request.AddParameter("visitkey", visitKey, ParameterType.UrlSegment);
            request.AddParameter("username", "ReaderTester", ParameterType.UrlSegment);
            //request.Timeout = 3000;
            var response = client.Execute(request);
            //client.ExecuteAsync(request, response =>
            //{
            //    //Console.WriteLine(response.Content);
            //});
        }
        private void WriteToFile(string result)
        {


            if (result=="")
                return;
            string filePath = Outdir.Text + Outputfile.Text;
            XElement rep = XElement.Parse(result);
            
            if (rep.HasElements == false)
            {
                //MessageBox.Show("No Data");
                //Application.Exit();
                return;
            }
            StringBuilder sb = new StringBuilder();

            if (bFirstTime)
            {
                sb.Append("CurrentTime,");
                sb.Append("Minutes");
                sb.Append(",");
            }

            foreach (var item in rep.Elements())
            {
                if (!bFirstTime)
                {
                    sb.Append(DateTime.Now.ToString("HH:mm:ss"));
                    sb.Append(",");
                    sb.Append(durationCounter / 60);
                    sb.Append(",");

                }

                foreach (var value in item.Elements())
                {
                    if (!value.HasElements)
                    {
                        //if (bFirstTime)// &&value.Name.LocalName!="CRIObject")
                        //{
                        //    sb.Append(value.Name.LocalName);
                        //    sb.Append(",");
                        //}
                        //else //if (value.Name.LocalName != "CRIObject")
                        //{
                        //    sb.Append(value.Value);
                        //    sb.Append(",");
                        //}

                    }
                    else if (value.Name.LocalName == "CurrentDisplayCRI")
                    {
                        bool bACK = false;
                        foreach (var CRIStatus in value.Elements())
                        {
                            if (bFirstTime)// && CRIStatus.Name.LocalName != "CRIObject")
                            {
                                sb.Append(CRIStatus.Name.LocalName);
                                foreach (var CRIStatusEvents in CRIStatus.Elements())
                                {
                                    sb.Append(CRIStatusEvents.Name.LocalName);
                                    sb.Append(",");
                                    foreach (var CRIStatusEventsValues in CRIStatusEvents.Elements())
                                    {
                                        //sb.Append(CRIStatusEvents.);
                                        sb.Append(CRIStatusEventsValues.Name.LocalName);
                                        sb.Append(",");
                                    }
                                }
                            }
                            else //if (CRIStatus.Name.LocalName != "CRIObject")
                            {
                                string CRIStatusEventsChop = CRIStatus.Value;
                                int iDot = 0;
                                if (CRIStatus.Name.LocalName == "CRIStatusEvents")
                                {
                                    CRIStatusEventsChop = "";   
                                    foreach (var CRIStatusEvents in CRIStatus.Elements())
                                    {
                                        sb.Append(CRIStatusEvents.Name.LocalName);
                                        sb.Append(",");
                                        foreach (var CRIStatusEventsValues in CRIStatusEvents.Elements())
                                        {
                                            CRIStatusEventsChop = CRIStatusEventsValues.Value;
                                         iDot = CRIStatusEventsChop.IndexOf('.');
                                         if (iDot > 0)
                                         {
                                             CRIStatusEventsChop = CRIStatusEventsChop.Remove(iDot + 2, (CRIStatusEventsChop.Length - iDot - 2));
                                             sb.Append(CRIStatusEventsChop);
                                         }
                                         else
                                         {
                                             //sb.Append(CRIStatusEvents.);
                                             sb.Append(CRIStatusEventsValues.Value);
                                         }
                                           sb.Append(",");
                                        }
                                    }
                                    bFirstTimeCRIStatusEvents = false;
                                    //iDot = CRIStatusEventsChop.IndexOf('.');
                                    //if (iDot>0)
                                        //CRIStatusEventsChop = CRIStatusEventsChop.Remove(iDot + 2, (CRIStatusEventsChop.Length - iDot - 2));
                                }
                                if (CRIStatus.Name.LocalName == "CRIStatus" && CRIStatus.Value == "PositiveCurrent")
                                {
                                    bACK=true;
                                }

                                if (bACK && CRIStatus.Name.LocalName == "VisitKey")
                                {
                                    CRIack(CRIStatus.Value);
                                    bACK = false;
                                }
                                sb.Append(CRIStatusEventsChop);
                            }
                            
                            sb.Append(",");
                        }
                    }
                }
                sb.AppendLine();
                bFirstTime = false;
            }
            try
            {
                File.AppendAllText(filePath, sb.ToString());
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            Outdir.Text =  folderBrowserDialog1.SelectedPath.ToString();
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void Stop_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
