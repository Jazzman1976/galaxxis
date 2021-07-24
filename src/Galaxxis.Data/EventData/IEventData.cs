namespace Galaxxis.Data.EventData
{
    public interface IEventData
    {
        public string GetXml<TEvent>() where TEvent : IEventData, new();
    }
}
