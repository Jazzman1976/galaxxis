namespace Galaxxis.Data.EventTypeData
{
    public interface IEventTypeData
    {
        string GetXml();
        void ReadData(string xml);
    }
}
