using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Galaxxis.Data.EventTypeData
{
    public abstract class BaseEventTypeData : IEventTypeData
    {
        public abstract string GetXml();
        public abstract void ReadData(string xml);

        protected string GetXml<TEventData>(TEventData obj)
        {
            XmlSerializer x = new XmlSerializer(typeof(TEventData));
            MemoryStream m = new MemoryStream();
            x.Serialize(m, obj);
            string xml = Encoding.UTF8.GetString(m.ToArray());
            return xml;
        }

        protected void ReadData<TEventData>(string xml, TEventData obj)
        {
            XmlSerializer x = new XmlSerializer(typeof(TEventData));
            using TextReader tr = new StringReader(xml);
            TEventData data = (TEventData)x.Deserialize(tr);

            // copy properties from xml to current object
            PropertyInfo[] properties = typeof(TEventData).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object value = data.GetType().GetProperty(property.Name).GetValue(data);
                property.SetValue(obj, value);
            }
        }
    }
}
