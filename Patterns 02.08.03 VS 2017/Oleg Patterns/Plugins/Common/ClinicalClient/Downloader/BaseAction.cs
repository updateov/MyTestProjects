using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PatternsCRIClient.Downloader
{
    public class BaseAction : IXmlSerializable
    {
        public System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader == null) 
                throw new ArgumentNullException("reader");

            reader.MoveToContent();

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
            WriteData(writer);
        }

        public virtual void ReadData(System.Xml.XmlReader reader) { }

        public virtual void WriteData(System.Xml.XmlWriter writer) { }
    }
}
