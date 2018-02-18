using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perigen.Patterns.NnetControls.DataEntities
{
    public class ExportCalculatedCheckboxGroupEntity : BaseExportEntity
    {
        public List<CalculatedCheckboxGroupConceptItem> Items { get; set; }

        public ExportCalculatedCheckboxGroupEntity()
        {
            Items = new List<CalculatedCheckboxGroupConceptItem>();
            m_value = null;
            m_originalValue = null;
        }

        public override void ReadData(System.Xml.XmlReader reader)
        {
            var value = reader.GetAttribute("Value");
            if (value != null)
            {
                m_value = Convert.ToString(value);
            }

            var originalValue = reader.GetAttribute("OriginalValue");
            if (originalValue != null)
            {
                m_originalValue = Convert.ToString(originalValue);
            }

            bool isEmptyElement = reader.IsEmptyElement;

            if (!isEmptyElement)
            {
                while (reader.Read() && reader.IsStartElement("Item"))
                {
                    CalculatedCheckboxGroupConceptItem item = new CalculatedCheckboxGroupConceptItem();
                    item.Id = Int32.Parse(reader.GetAttribute("Id"));
                    item.Value = reader.GetAttribute("Value");
                    item.CalculatedValue = reader.GetAttribute("CalculatedValue");
                    item.OrderNumber = Int32.Parse(reader.GetAttribute("OrderNumber"));

                    var IsGrouping = reader.GetAttribute("IsGrouping");
                    if (IsGrouping != null)
                    {
                        item.IsGrouping = true;
                    }

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
            //    writer.WriteAttributeString("CalculatedValue", item.CalculatedValue);
            //    writer.WriteAttributeString("OrderNumber", item.OrderNumber.ToString());
            //    if (item.IsGrouping == true)
            //    {
            //        writer.WriteAttributeString("IsGrouping", item.IsGrouping.ToString());
            //    }
            //    writer.WriteEndElement();
            //}
        }
    }


    public class CalculatedCheckboxGroupConceptItem
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string CalculatedValue { get; set; }
        public int OrderNumber { get; set; }
        public bool IsGrouping { get; set; }
    }
}
