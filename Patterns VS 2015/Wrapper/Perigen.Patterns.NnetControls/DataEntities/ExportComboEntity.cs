using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Perigen.Patterns.NnetControls.DataEntities
{
    public class ExportComboEntity : BaseExportEntity
    {
        public new string Value { get; set; }

        public List<string> Items = new List<string>();

        public override void ReadData(System.Xml.XmlReader reader)
        {
            Value = String.Empty;

            string val = reader.GetAttribute("Value");
            if (String.IsNullOrEmpty(val) == false)
            {
                Value = val;
            }

            val = reader.GetAttribute("OriginalValue");
            if (String.IsNullOrEmpty(val) == false)
            {
                OriginalValue = val;
            }

            bool isEmptyElement = reader.IsEmptyElement;

            if (!isEmptyElement)
            {
                while (reader.Read() && reader.IsStartElement("Item"))
                {
                    string propValue = reader["Value"];

                    Items.Add(propValue);
                }
            }
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
