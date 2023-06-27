using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;

namespace Galaxxis.Data.Events
{
    public class ProduceLightManualCannonEvent : Event
    {
        private ProduceLightManualCannonEvent() { }

        public ProduceLightManualCannonEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            ProduceEventTypeData data)
        {
            this.EventLocation = location.ToString();
            this.EventLocationId = locationId;
            this.EventName = EventNameEnum.ProduceDefenseLightManualCannon.ToString();
            this.EventData = data.GetXml();
            this.EventType = EventTypeEnum.Produce.ToString();

            this.EventTicksToEventTick = ticksToEventTick;
        }
    }
}
