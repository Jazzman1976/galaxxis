using Galaxxis.Data.Enums;

namespace Galaxxis.Data.EventTypeData
{
    public class MoveEventTypeData : BaseEventTypeData, IEventTypeData
    {
        public EventLocationEnum FromLocation { get; set; }
        public EventLocationEnum ToLocation { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public int[] MovingShipIds { get; set; }
        public double Ore { get; set; }
        public double Energy { get; set; }
        public double Tools { get; set; }
        public double Mana { get; set; }
        public double Crystals { get; set; }
        public double Antimatter { get; set; }
        public double Elementium { get; set; }

        private MoveEventTypeData() { }

        public MoveEventTypeData(
            EventLocationEnum fromLocation,
            int fromLocationId,
            EventLocationEnum toLocation,            
            int toLocationId,
            int[] movingShipIds,
            double ore = 0,
            double energy = 0,
            double tools = 0,
            double mana = 0,
            double crystals = 0,
            double antimatter = 0,
            double elementium = 0)
        {
            FromLocation = fromLocation;
            ToLocation = toLocation;
            FromLocationId = fromLocationId;
            ToLocationId = toLocationId;
            MovingShipIds = movingShipIds;
            Ore = ore;
            Energy = energy;
            Tools = tools;
            Mana = mana;
            Crystals = crystals;
            Antimatter = antimatter;
            Elementium = elementium;
        }

        public MoveEventTypeData(string xml)
        {
            ReadData(xml);
        }

        public override string GetXml()
        {
            return base.GetXml<MoveEventTypeData>(this);
        }

        public override void ReadData(string xml)
        {
            base.ReadData<MoveEventTypeData>(xml, this);
        }
    }
}
