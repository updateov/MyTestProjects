using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PatternsCRIClient.Downloader
{
    public class RegisterAction : BaseAction
    {
        public string FileName { get; set; }
        public string Command { get; set; }

        public override void ReadData(System.Xml.XmlReader reader)
        {
            FileName = reader.GetAttribute("FileName");
            Command = reader.GetAttribute("Command");

            if(!reader.IsEmptyElement)
            {
                reader.Read();
            }
        }

        public override void WriteData(System.Xml.XmlWriter writer)
        {
        }
    }
}
