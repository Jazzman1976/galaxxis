using Galaxxis.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxxis.Data.EventTypeData
{
    public class ScanEventTypeData : BaseEventTypeData, IEventTypeData
    {
        public EventLocationEnum FromLocation { get; set; }
        public EventLocationEnum ScanLocation { get; set; }
        public int[] ScanningShipIds { get; set; }
        public int FromLocationId { get; set; }
        public int ScanLocationId { get; set; }
        public double ScannedOre { get; set; }
        public double ScannedEnergy { get; set; }
        public double ScannedTools { get; set; }
        public double ScannedMana { get; set; }
        public double ScannedCrystals { get; set; }
        public double ScannedAntimatter { get; set; }
        public double ScannedElementium { get; set; }

        private ScanEventTypeData() { }

        public ScanEventTypeData(
            EventLocationEnum fromLocation,
            int fromLocationId,
            EventLocationEnum toLocation,
            int toLocationId,
            int[] movingShipIds)
        {
            FromLocation = fromLocation;
            ScanLocation = toLocation;
            FromLocationId = fromLocationId;
            ScanLocationId = toLocationId;
            ScanningShipIds = movingShipIds;
        }

        public ScanEventTypeData(string xml)
        {
            ReadData(xml);
        }

        public override string GetXml()
        {
            return base.GetXml<ScanEventTypeData>(this);
        }

        public override void ReadData(string xml)
        {
            base.ReadData<ScanEventTypeData>(xml, this);
        }
    }
}
