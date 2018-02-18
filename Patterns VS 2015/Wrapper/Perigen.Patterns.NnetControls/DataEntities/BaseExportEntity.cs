using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Perigen.Patterns.NnetControls.DataEntities
{
    public class BaseExportEntity : IXmlSerializable
    {
        public int Id { get; set; }
        public int OOC { get; set; }
        public int OrderNumber { get; set; }
        public string Name { get; set; }

        protected object m_value;
        public object Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        protected object m_originalValue;
        public object OriginalValue
        {
            get
            {
                return m_originalValue;
            }
            set
            {
                m_originalValue = value;
            }
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            reader.MoveToContent();

            Id = Int32.Parse(reader.GetAttribute("Id"));
            OOC = Int32.Parse(reader.GetAttribute("OOC"));
            OrderNumber = Int32.Parse(reader.GetAttribute("OrderNumber"));
            Name = reader.GetAttribute("Name");

            ReadData(reader);

            if (reader.IsStartElement())
            {
                reader.ReadStartElement();
            }
            else
            {
                reader.ReadEndElement();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Id", Id.ToString());
            writer.WriteAttributeString("OOC", OOC.ToString());
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("OrderNumber", OrderNumber.ToString());

            WriteData(writer);
        }

        #endregion

        public virtual void ReadData(System.Xml.XmlReader reader) { }

        public virtual void WriteData(System.Xml.XmlWriter writer) { }
    }
}
