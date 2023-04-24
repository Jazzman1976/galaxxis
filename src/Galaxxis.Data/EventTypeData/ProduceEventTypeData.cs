namespace Galaxxis.Data.EventTypeData
{
    public class ProduceEventTypeData : BaseEventTypeData, IEventTypeData
    {
        public double CostsTools { get; set; }
        public double CostsCrystals { get; set; }
        public double CostsAntimatter { get; set; }
        public double CostsElementium { get; set; }

        private ProduceEventTypeData() { }

        public ProduceEventTypeData(
            double costsTools,
            double costsCrystals,
            double costsAntimatter,
            double costsElementium)
        {
            CostsTools = costsTools;
            CostsCrystals = costsCrystals;
            CostsAntimatter = costsAntimatter;
            CostsElementium = costsElementium;
        }

        public ProduceEventTypeData(string xml) 
        {
            ReadData(xml);
        }

        public override string GetXml()
        {
            return base.GetXml<ProduceEventTypeData>(this);
        }

        public override void ReadData(string xml)
        {
            base.ReadData<ProduceEventTypeData>(xml, this);
        }
    }
}
