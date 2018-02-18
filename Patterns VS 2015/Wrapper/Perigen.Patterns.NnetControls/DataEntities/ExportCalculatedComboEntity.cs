using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perigen.Patterns.NnetControls.DataEntities
{
    public class ExportCalculatedComboEntity : BaseExportEntity
    {
        public List<CalculatedComboConceptItem> Items { get; set; }
        public new string Value { get; set; }
        public new string OriginalValue { get; set; }

        public ExportCalculatedComboEntity()
        {
            Items = new List<CalculatedComboConceptItem>();
            m_value = null;
            m_originalValue = null;
        }   

        public override void ReadData(System.Xml.XmlReader reader)
        {
            var value = reader.GetAttribute("Value");
            if (value != null)
            {
                Value = Convert.ToString(value);
            }

            var originalValue = reader.GetAttribute("OriginalValue");
            if (originalValue != null)
            {
                OriginalValue = Convert.ToString(originalValue);
            }

            bool isEmptyElement = reader.IsEmptyElement;

            if (!isEmptyElement)
            {
                while (reader.Read() && reader.IsStartElement("Item"))
                {
                    CalculatedComboConceptItem item = new CalculatedComboConceptItem();
                    item.Id = Int32.Parse(reader.GetAttribute("Id"));
                    item.Value = reader.GetAttribute("Value");
                    item.Min = Double.Parse(reader.GetAttribute("Min"));
                    item.Max = Double.Parse(reader.GetAttribute("Max"));
                    item.OrderNumber = Int32.Parse(reader.GetAttribute("OrderNumber"));

                    Items.Add(item);
                }
            }
        }

        public override void WriteData(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Value", Value != null ? Value.ToString() : String.Empty);
            writer.WriteAttributeString("OriginalValue", OriginalValue != null ? OriginalValue.ToString() : String.Empty);
 
            //foreach (var item in Items)
            //{
            //    writer.WriteStartElement("Item");
            //    writer.WriteAttributeString("Id", item.Id.ToString());
            //    writer.WriteAttributeString("Value", item.Value);
            //    writer.WriteAttributeString("Min", item.Min.ToString());
            //    writer.WriteAttributeString("Max", item.Max.ToString());
            //    writer.WriteAttributeString("OrderNumber", item.OrderNumber.ToString());
            //    writer.WriteEndElement();
            //}
        }
    }

    public class CalculatedComboConceptItem
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public int OrderNumber { get; set; }
    }
}
