using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;

namespace Galaxxis.Data.Events
{
    public class ChangeOreMineUsageEvent : Event
    {
        private ChangeOreMineUsageEvent() { }

        public ChangeOreMineUsageEvent(
            EventLocationEnum location,
            int locationId,
            ChangeUsageEventTypeData data)
        {
            this.EventLocation = location.ToString();
            this.EventLocationId = locationId;
            this.EventName = EventNameEnum.ChangeOreMineUsage.ToString();
            this.EventData = data.GetXml();
            this.EventType = EventTypeEnum.ChangeUsage.ToString();

            this.EventTicksToEventTick = 0;
        }
    }
}
