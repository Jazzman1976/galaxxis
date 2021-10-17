using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;

namespace Galaxxis.Data.Events.BuildEvents
{
    public abstract class BaseBuildEvent : Event
    {
        protected BaseBuildEvent() { }

        public BaseBuildEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            BuildEventTypeData data,
            EventNameEnum eventName)
        {
            // entity properties
            this.EventLocation = location.ToString();
            this.EventLocationId = locationId;
            this.EventName = eventName.ToString();
            this.EventData = data.GetXml();
            this.EventType = EventTypeEnum.Build.ToString();

            // custom properties
            this.EventTicksToEventTick = ticksToEventTick;
        }
    }
}
