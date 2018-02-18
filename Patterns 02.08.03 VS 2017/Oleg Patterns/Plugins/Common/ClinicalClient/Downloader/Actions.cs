using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PatternsCRIClient.Downloader
{
    [Serializable]
    [XmlType("Actions")]
    public class Actions
    {
        public Actions() { Items = new List<BaseAction>(); }

        [XmlElement("DownloadAction", typeof(DownloadAction))]
        [XmlElement("RegisterAction", typeof(RegisterAction))]
        public List<BaseAction> Items { get; set; }    
    }
}
