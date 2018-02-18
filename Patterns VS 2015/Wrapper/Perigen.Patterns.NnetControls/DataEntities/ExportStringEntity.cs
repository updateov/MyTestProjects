using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Perigen.Patterns.NnetControls.DataEntities
{
    public class ExportStringEntity : BaseExportEntity
    {
        public new string Value { get; set; }

        public override void ReadData(System.Xml.XmlReader reader)
        {
            Value = reader.GetAttribute("Value");
            OriginalValue = reader.GetAttribute("OriginalValue");
        }

        public override void WriteData(System.Xml.XmlWriter writer)
        {
            if (Value != null)
            {
                writer.WriteAttributeString("Value", Value);
            }

            if (OriginalValue != null)
            {
                writer.WriteAttributeString("OriginalValue", OriginalValue.ToString());
            }
        }
    }
}
