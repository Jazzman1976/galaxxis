using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;

namespace Galaxxis.Data.Events
{
    public class FleetCommandScanEvent : Event
    {
        private FleetCommandScanEvent() { }

        public FleetCommandScanEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            ScanEventTypeData data)
        {
            this.EventLocation = location.ToString();
            this.EventLocationId = locationId;
            this.EventName = EventNameEnum.FleetCommandScan.ToString();
            this.EventType = EventTypeEnum.Move.ToString();
            this.EventData = data.GetXml();
            this.EventTicksToEventTick = ticksToEventTick;
        }
    }
}
