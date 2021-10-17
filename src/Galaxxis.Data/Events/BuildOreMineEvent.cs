using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;

namespace Galaxxis.Data.Events
{
    public class BuildOreMineEvent : Event
    {
        private BuildOreMineEvent() { }

        public BuildOreMineEvent(
            EventLocationEnum location, 
            int locationId,
            long ticksToEventTick,
            BuildEventTypeData data)
        {
            // entity properties
            this.EventLocation = location.ToString();
            this.EventLocationId = locationId;
            this.EventName = EventNameEnum.BuildOreMine.ToString();
            this.EventData = data.GetXml();
            this.EventType = EventTypeEnum.Build.ToString();

            // custom properties
            this.EventTicksToEventTick = ticksToEventTick;
        }
    }
}
