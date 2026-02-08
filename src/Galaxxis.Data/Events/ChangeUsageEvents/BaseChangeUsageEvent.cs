using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;

namespace Galaxxis.Data.Events.ChangeUsageEvents
{
    public abstract class BaseChangeUsageEvent : Event
    {
        protected BaseChangeUsageEvent() { }

        public BaseChangeUsageEvent(
            EventLocationEnum location,
            int locationId,
            ChangeUsageEventTypeData data,
            EventNameEnum eventName)
        {
            // entity properties
            EventLocation = location.ToString();
            EventLocationId = locationId;
            EventName = eventName.ToString();
            EventData = data.GetXml();
            EventType = EventTypeEnum.ChangeUsage.ToString();

            // custom properties
            EventTicksToEventTick = 0;
        }
    }
}
