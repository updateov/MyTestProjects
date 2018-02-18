using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Export.Entities.ExportConfigData
{
    [Serializable]
    public class ExportTab
    {
        #region Properties
        [XmlElement("ExportColumn", typeof(ExportColumn))]
        public List<ExportColumn> Columns { get; set; }
        public int OrderId { get; set; }
        public bool IsVisible { get; set; }
        public string TabTitle { get; set; }

        #endregion

        public ExportTab()
        {
        }
    }
}
