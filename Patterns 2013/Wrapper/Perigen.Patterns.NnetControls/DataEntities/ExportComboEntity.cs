using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Perigen.Patterns.NnetControls.DataEntities
{
    public class ExportComboEntity : BaseExportEntity
    {
        public string Value { get; set; }

        public List<string> Categories = new List<string>();

        public override void ReadData(System.Xml.XmlReader reader)
        {
            Value = String.Empty;
            string val = reader.GetAttribute("Value");

            if (String.IsNullOrEmpty(val) == false)
            {
                Value = reader.GetAttribute("Value");
            }

            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();

            if (!isEmptyElement)
            {
                while (reader.IsStartElement("Item"))
                {
                    string propValue = reader["Value"];

                    Categories.Add(propValue);

                    reader.ReadStartElement();
                }
                reader.ReadEndElement();
            }
        }

        public override void WriteData(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Value", Value.ToString());
        }
    }
}
