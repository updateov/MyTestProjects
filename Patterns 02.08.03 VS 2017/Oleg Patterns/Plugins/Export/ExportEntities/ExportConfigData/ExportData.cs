using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Export.Entities.ExportConfigData
{
    [Serializable]
    public class ExportData
    {
        #region Properties
        [XmlElement("ExportTab", typeof(ExportTab))]
        public List<ExportTab> Tabs { get; set; }
        #endregion

        public ExportData()
        {
        }
    }
}
