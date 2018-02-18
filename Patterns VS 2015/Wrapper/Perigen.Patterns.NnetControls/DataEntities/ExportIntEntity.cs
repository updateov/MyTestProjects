using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Perigen.Patterns.NnetControls.DataEntities
{
    public class ExportIntEntity : BaseExportEntity
    {
        public new int? Value { get; set; }
        public int? OriginValue { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }

        public override void ReadData(System.Xml.XmlReader reader)
        {
            string val = reader.GetAttribute("Value");
            if (String.IsNullOrEmpty(val))
            {
                Value = null;
            }
            else
            {
                Value = Int32.Parse(reader.GetAttribute("Value"));
            }

            val = reader.GetAttribute("OriginalValue");
            if (String.IsNullOrEmpty(val))
            {
                OriginValue = null;
            }
            else
            {
                OriginValue = Int32.Parse(reader.GetAttribute("OriginalValue"));
            }

            Min = Int32.Parse(reader.GetAttribute("Min"));
            Max = Int32.Parse(reader.GetAttribute("Max"));
        }

        public override void WriteData(System.Xml.XmlWriter writer)
        {
            if (Value.HasValue)
            {
                writer.WriteAttributeString("Value", Value.ToString());
            }

            if (OriginValue.HasValue)
            {
                writer.WriteAttributeString("OriginalValue", OriginValue.ToString());
            }

            writer.WriteAttributeString("Min", Min.ToString());
            writer.WriteAttributeString("Max", Max.ToString());
        }
    }
}
