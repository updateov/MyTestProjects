using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PatternsCRIClient.Downloader
{
    public class DownloadAction : BaseAction
    {
        public string FileName { get; set; }
        public string Destination { get; set; }
        public string Version { get; set; }
        public RegisterAction RegisterAction { get; set; }

        public override void ReadData(System.Xml.XmlReader reader) 
        {
            FileName = reader.GetAttribute("Source");
            Destination = reader.GetAttribute("Destination");
            Version = reader.GetAttribute("Version");

            if(String.IsNullOrEmpty(Destination))
            {
                Destination = DownloadsManager.Instance.AppLocation;
            }

            bool isEmptyElement = reader.IsEmptyElement;

            if (!isEmptyElement)
            {
                while (reader.Read() && reader.IsStartElement("RegisterAction"))
                {
                    RegisterAction = new RegisterAction();
                    RegisterAction.ReadData(reader);
                }
            }
        }

        public override void WriteData(System.Xml.XmlWriter writer) 
        { 
        
        }
    }
}
