using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Perigen.Patterns.NnetControls.DataEntities
{
    public class ExportStringEntity : BaseExportEntity
    {
        public string Value { get; set; }

        public override void ReadData(System.Xml.XmlReader reader)
        {
            Value = reader.GetAttribute("Value");
        }

        public override void WriteData(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Value", Value);
        }
    }
}
