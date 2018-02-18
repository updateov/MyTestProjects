using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace PatternsAddOnWinformTestApp
{
    public class ManualStorage
    {
        public ManualStorage()
        {
            FhrList = new List<byte>();
            UpsList = new List<byte>();
            LastTimeStamp = DateTime.MinValue;
        }

        public void AppendTracings(String dataStr)
        {
            var sr = new StringReader(dataStr);
            var contentElement = XElement.Load(sr);
            contentElement = XMLHelper.RemoveAllNamespaces(contentElement);
            var fhr = contentElement.Element("Fhr").Value;
            var fhrTempList = new List<byte>(Convert.FromBase64String(fhr));
            var up = contentElement.Element("Up").Value;
            var upTempList = new List<byte>(Convert.FromBase64String(up));
            var startTime = contentElement.Element("StartTime").Value;
            DateTime start = ArtifactsHelper.GetDateTime(startTime);
            var diff = start - LastTimeStamp;
            if (LastTimeStamp == DateTime.MinValue)
            {
                LastTimeStamp = start.AddSeconds(-1);
            }
            else if (diff.TotalSeconds > 1) // GAP
            {
                int secondsToFill = ((int)Math.Floor(diff.TotalSeconds)) - 1;
                for (int i = 0; i < secondsToFill; i++)
                {
                    FhrList.Add(255);
                    FhrList.Add(255);
                    FhrList.Add(255);
                    FhrList.Add(255);
                    UpsList.Add(255);
                }

                LastTimeStamp = LastTimeStamp.AddSeconds(secondsToFill);
            }
            else if (diff.TotalSeconds <= 0) // OVERLAP
            {
                int secondsToRemove = (Math.Abs((int)Math.Floor(diff.TotalSeconds))) + 1;
                upTempList.RemoveRange(0, secondsToRemove);
                fhrTempList.RemoveRange(0, secondsToRemove * 4);
            }

            FhrList.AddRange(fhrTempList);
            UpsList.AddRange(upTempList);
            LastTimeStamp = LastTimeStamp.AddSeconds(upTempList.Count);
        }

        public List<byte> FhrList { get; private set; }
        public List<byte> UpsList { get; private set; }
        public DateTime LastTimeStamp { get; set; }
    }
}
