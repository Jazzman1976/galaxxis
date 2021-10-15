using Galaxxis.Data.Enums;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Galaxxis.Data.EventData
{
    public abstract class BaseEventData : IEventData
    {
        public virtual EventNameEnum EventName { get; set; }

        public string GetXml<TEvent>() where TEvent: IEventData, new()
        {
            XmlSerializer x = new XmlSerializer(typeof(TEvent));

            MemoryStream m = new MemoryStream();
            x.Serialize(m, this);
            string xml = Encoding.UTF8.GetString(m.ToArray());

            return xml;
        }
    }
}
