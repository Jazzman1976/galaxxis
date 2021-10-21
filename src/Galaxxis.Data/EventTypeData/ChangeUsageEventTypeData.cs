namespace Galaxxis.Data.EventTypeData
{
    public class ChangeUsageEventTypeData : BaseEventTypeData, IEventTypeData
    {
        public double NewValue { get; set; }

        private ChangeUsageEventTypeData() { }

        public ChangeUsageEventTypeData(string xml) 
        {
            ReadData(xml);
        }

        public ChangeUsageEventTypeData(double newValue)
        {
            NewValue = newValue;
        }

        public override string GetXml()
        {
            return base.GetXml<ChangeUsageEventTypeData>(this);
        }

        public override void ReadData(string xml)
        {
            base.ReadData<ChangeUsageEventTypeData>(xml, this);
        }
    }
}
