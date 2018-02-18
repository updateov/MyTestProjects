using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Export.Entities.ExportControlConfig
{
    [Serializable]
    public class ExportColumn
    {
        #region Properties
        [XmlElement("ExportEntity", typeof(ExportEntity))]
        public List<ExportEntity> Entities { get; set; }
        public int OrderId { get; set; }
        public string Width { get; set; }
        #endregion

        public ExportColumn()
        {
        }
    }
}
