
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
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections;


namespace CRIReaderCompareTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
   
    public partial class MainWindow : Window
    {
        static string[] files;
        string[] arg;
        List<string>[] CRIStatus;
        List<string>[,] CRIStatusEvents;
        float[] eventsSum;
        float[] threshEventsSum;

        public MainWindow()
        {
            arg = Environment.GetCommandLineArgs();
            InitializeComponent();
            
           
            if (arg.Count() > 2)
            {
                files = new string[2];
                for (int i = 0; i < 2; i++)
                {
                    files[i] = arg[i + 1];
                }

                compare();
            }
        }

        private void selectFiles_Click(object sender, RoutedEventArgs e)
        {
            
            //System.Data.DataTable data = new System.Data.DataTable();

            OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            System.Data.DataTable[] data;// = new System.Data.DataTable();
            string[]CRI;
            CRI = new string[2];
           

            openFileDialog1.Multiselect = true;
            openFileDialog1.Filter = "xml files (*.csv)|*.csv|All Files (*.*)|*.*";

            DialogResult result = openFileDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                int filesCounter = openFileDialog1.FileNames.Count();
                if (filesCounter != 2)
                {
                    System.Windows.MessageBox.Show("Choose 2 Files to compare");
                    return;
                }
                files = new string[filesCounter];
                data = new System.Data.DataTable[filesCounter];

                int counter = 0;

                foreach (string strfile in openFileDialog1.FileNames)
                {
                    files[counter] = strfile;
                    counter++;
                }

                txtStatus.Text = "Comparing";
                compare();
            }     
             
        }
        private void compare()
        {
            var stream = File.ReadAllLines(files[0]);
            var headers = stream.First().Split(',');
            float CriStatusMismatch=0;
            int counterRec = 0;
            CRIStatus = new List<String>[2];
            
            List <int> values = new List <int>();
            var CRIStatusEventsindex = 0;

            eventsSum = new float[headers.Count()];
            threshEventsSum = new float[headers.Count()];

            //foreach (var elem in headers)
            while (CRIStatusEventsindex > -1 )
            {
                CRIStatusEventsindex = Array.IndexOf(headers, "Value", CRIStatusEventsindex + 1);
                if (CRIStatusEventsindex > -1)
                    values.Add(CRIStatusEventsindex);
            }
            CRIStatusEvents = new List<String>[2,stream.Count()];
            try
            {


                foreach (string strfile in files)
                {
                    filesSelected.Text += strfile + '\n';
                    //data[counter] = new System.Data.DataTable();
                    CRIStatus[counterRec] = new List<string>();
                   // CRIStatusEvents[counter,10] = new List<string>();

                    stream = File.ReadAllLines(files[counterRec]);

                    //CRI = File.ReadAllLines(files[counter]);

                    //var headers = stream.First().Split(',');

                    var CRIStatusindex = Array.IndexOf(headers, "CRIStatus");
                    //var CRIStatusEventsindex = Array.IndexOf(headers, "CRIStatusEvents");
                    //var CRIStatusEventsindex = 0;



                    //foreach (var elem in headers)
                    //{
                    //    CRIStatusEventsindex = Array.IndexOf(headers, "Value", CRIStatusEventsindex + 1);
                    //    values.Add(CRIStatusEventsindex);
                    //}

                    //if (CRIStatusindex < 0 || CRIStatusEventsindex < 0)
                    if (CRIStatusindex < 0)
                    {
                        System.Windows.MessageBox.Show("Files not compareble");
                        return;
                    }

                    //data[counter].Columns.Add(headers[CRIStatusindex]);


                    var records = stream.Skip(1);
                    int iCRIStatusEvents = 0;
                    foreach (var record in records)
                    {
                        //data[counter].Rows.Add(record.Split(','));
                        CRIStatusEvents[counterRec, iCRIStatusEvents] = new List<string>();
                        CRIStatus[counterRec].Add(record.Split(',').ToArray().ElementAt(CRIStatusindex));
                        for (int j = 0; j < values.Count; j++)
                        {

                            CRIStatusEvents[counterRec, iCRIStatusEvents].Add(record.Split(',').ToArray().ElementAt(values.ElementAt(j)));
                            //CRIStatusEvents[counter].Add(record.Split(',').ToArray().ElementAt(CRIStatusEventsindex));
                        }
                        //var CRIStatusEvent = new List<String>[2, values.Count()];
                        //CRIStatusEvents[counter,iCRIStatusEvents].Add(CRIStatusEvent);
                        iCRIStatusEvents++;
                    }

                    counterRec++;

                }
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
            
            try
            {


                string[] names1 = File.ReadAllLines(files[0]);
                string[] names2 = File.ReadAllLines(files[1]);

                //IEnumerable<string> differenceQuery;

                bool eq = names1.SequenceEqual(names2);

                //var CRIStatusstr0= CRIStatus[0].ToArray();
                //var CRIStatusstr1 = CRIStatus[1].ToArray();
                string strReport = "";

                bool bCRIStatus = CRIStatus[0].SequenceEqual(CRIStatus[1]);
                bool bCRIStatusEvents = true;
                for (int iCRIStatusEvents = 0; iCRIStatusEvents < CRIStatus[0].Count(); iCRIStatusEvents++)
                {
                   // for (int j = 0; j < values.Count; j++)
                    {
                        bCRIStatusEvents = CRIStatusEvents[0, iCRIStatusEvents].SequenceEqual(CRIStatusEvents[1, iCRIStatusEvents]);
                    }
                }
                bool eq1 = names1.SequenceEqual(names2);

                int arrSize = (CRIStatus[0].Count() == CRIStatus[1].Count() ? CRIStatus[0].Count() : 0);

                if (bCRIStatus && bCRIStatusEvents)
                {
                    strReport += "Pass";

                    txtStatus.Text = "Pass";

                    txtStatus.Background = Brushes.Green;

                    //System.Windows.MessageBox.Show("CRIStatus & CRIStatusEvents match");

                }
                else if (bCRIStatus && !bCRIStatusEvents)
                {
                    strReport += "CRISEventsReport";

                    txtStatus.Text = "CRIStatus match only";

                    txtStatus.Background = Brushes.Yellow;
                    //System.Windows.MessageBox.Show("CRIStatus match only");
                }
                else if (!bCRIStatus && bCRIStatusEvents)
                {
                    //strReport += "CRISEventsReport";

                    txtStatus.Text = "CRIStatusEvents match only";

                    txtStatus.Background = Brushes.Yellow;
                    //System.Windows.MessageBox.Show("CRIStatus match only");
                }

                else
                {
                    strReport += "CRIAndEventsReport";

                    txtStatus.Text = "CRIStatus and Events don't match";

                    txtStatus.Background = Brushes.Red;

                    //System.Windows.MessageBox.Show("CRIStatus doesn't match");
                }
                if (!bCRIStatus || !bCRIStatusEvents)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("NO MATCH REPORT");
                    sb.Append(DateTime.Now.ToString(@" dd/M/yyyy HH:mm:ss"));
                    sb.AppendLine();
                    sb.Append("------------------");
                    sb.AppendLine();
                    sb.AppendLine();

                    for (int i = 0; i < arrSize; i++)
                    {
                        string diff;

                        if ((CRIStatus[0][i] != CRIStatus[1][i]))
                        {
                            diff = GetDiff(CRIStatus[0][i],CRIStatus[1][i]);

                            sb.Append("No match CRIStatus at line : ");
                            sb.Append(i + 2);
                            sb.Append(" ");
                            sb.Append(CRIStatus[0][i]); sb.Append(" "); sb.Append(CRIStatus[1][i]); sb.Append(" ");
                            sb.AppendFormat(diff);
                            sb.AppendLine(); sb.AppendLine();
                            CriStatusMismatch++;
                        }
                        var itrList0 = CRIStatusEvents[0, i].GetEnumerator();
                        var itrList1 = CRIStatusEvents[1, i].GetEnumerator();
                        for (int j = 0; j < values.Count; j++)
                        {
 
                           // if (CRIStatusEvents[0,i].ToList().ToString() != CRIStatusEvents[1,i].ToString())
                            if (CRIStatusEvents[0, i].SequenceEqual(CRIStatusEvents[1, i])==false)
                            {
                                itrList0.MoveNext();
                                itrList1.MoveNext();
                                //diff = GetDiff(CRIStatusEvents[0, i], CRIStatusEvents[1,i]);
                                //diff = GetDiff(itrList0.MoveNext().ToString(), itrList1.MoveNext().ToString());
                                if (itrList0.Current != itrList1.Current)
                                {
                                    eventsSum[j]++;
                                }

                                diff = GetDiff(itrList0.Current, itrList1.Current);

                                if (diff != "")
                                {
                                    try
                                    {
                                        var delta = Convert.ToDouble(itrList0.Current) - Convert.ToDouble(itrList1.Current);

                                        if ((delta > 1 && j < 6) || (delta > 0.5 && j==6))
                                            threshEventsSum[j]++;
                                    }
                                    catch (OverflowException)
                                    {

                                    }
                                }
                                sb.Append("No match CRIStatusEvents at line : ");
                                sb.Append(i + 2);
                                sb.Append(" ");
                                sb.AppendFormat(itrList0.Current); sb.Append(" "); sb.Append(itrList1.Current); sb.Append(" ");
                                sb.AppendFormat(diff);
                                sb.AppendLine(); sb.AppendLine();
                            }

                        }

                    }
                    if (arrSize == 0)
                    {
                        sb.Append("No match in SIZE");
                    }
                    else
                    {
                        sb.AppendLine(); sb.AppendLine();

                        float percent = CriStatusMismatch / arrSize * 100;
                        sb.Append("CRIStatus don't match: ");
                        sb.AppendLine();
                        sb.Append(CriStatusMismatch);
                        sb.Append(" Out of ");
                        sb.Append(arrSize);
                        sb.Append("    ");

                        //sb.AppendLine(); //sb.AppendLine();
                        sb.Append(percent); sb.Append("%");
                        sb.AppendLine();
                        sb.AppendLine();
                        for (int j = 0; j < values.Count; j++)
                        {
                            sb.Append(headers[values[j]-2]);
                            sb.Append(": ");
                            sb.Append(eventsSum[j]);
                            sb.Append(" Out of ");
                            sb.Append(arrSize);
                            float percentE = (eventsSum[j]) / arrSize * 100;
                            sb.Append("    ");
                            sb.Append(percentE); sb.Append("%");
                            sb.Append("   Threshold  ");
                            sb.Append(threshEventsSum[j]);
                            sb.Append(" Out of ");
                            sb.Append(arrSize);
                            percentE = (threshEventsSum[j]) / arrSize * 100;
                            sb.Append("    ");
                            sb.Append(percentE); sb.Append("%");
                            sb.AppendLine();
                            sb.AppendLine();
                        }

                        if (!bCRIStatus)
                        {
                            txtStatus.Text += '\n';
                            txtStatus.Text += "CRIStatus mismatch: ";
                            txtStatus.Text += (percent.ToString());
                            txtStatus.Text += "%";
                        }
                    }

                    string filesChecked0 = files[0].Remove(files[0].LastIndexOf("."), 4);
                    string filesChecked1 = files[1].Remove(0, files[1].LastIndexOf("\\") + 1);
                    strReport = String.Format("{0}_{1}_{2}.txt", filesChecked0, filesChecked1, strReport);

                    File.AppendAllText(strReport, sb.ToString());
                }
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }

        }
        private string GetDiff(string s1, string s2)
        {

            string diff;
            IEnumerable<string> set1 = s1.Split(' ').Distinct();
            IEnumerable<string> set2 = s2.Split(' ').Distinct();

            if (set2.Count() > set1.Count())
            {
                diff = s2.Replace(s1, "");
                //diff = s2.s(s1.Length);
            }
            else
            {
                diff = s1.Replace(s2, "");
            }

            
            return diff.ToString();
        }
    }
}
