namespace Galaxxis.Data.EventTypeData
{
    public class BuildEventTypeData : BaseEventTypeData, IEventTypeData
    {
        public double CostsOre { get; set; }
        public double CostsMana { get; set; }
        public double CostsCrystals { get; set; }
        public double CostsElementium { get; set; }        

        public BuildEventTypeData(
            double costsOre, double costsMana, double costsCrystals, double costsElementium)
        {
            CostsOre = costsOre;
            CostsMana = costsMana;
            CostsCrystals = costsCrystals;
            CostsElementium = costsElementium;
        }

        public BuildEventTypeData(string xml)
        {
            ReadData(xml);
        }

        private BuildEventTypeData() { }

        public override string GetXml()
        {
            return base.GetXml<BuildEventTypeData>(this);
        }

        public override void ReadData(string xml)
        {
            base.ReadData<BuildEventTypeData>(xml, this);
        }
    }
}
