using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Perigen.Patterns.NnetControls.DataEntities
{
    public class ExportDoubleEntity : BaseExportEntity
    {
        public new double? Value { get; set; }
        public double? OriginValue { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }

        public override void ReadData(System.Xml.XmlReader reader)
        {
            string val = reader.GetAttribute("Value");
            if (String.IsNullOrEmpty(val))
            {
                Value = null;
            }
            else
            {
                Value = Math.Round(Double.Parse(reader.GetAttribute("Value")), 1);
            }

            val = reader.GetAttribute("OriginalValue");
            if (String.IsNullOrEmpty(val))
            {
                OriginValue = null;
            }
            else
            {
                OriginValue = Math.Round(Double.Parse(reader.GetAttribute("OriginalValue")), 1);
            }

            Min = Double.Parse(reader.GetAttribute("Min"));
            Max = Double.Parse(reader.GetAttribute("Max"));
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
