using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;

namespace Galaxxis.Data.Events
{
    public class AttackEvent : Event
    {
        private AttackEvent() { }

        public AttackEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            MoveEventTypeData data)
        {
            this.EventLocation = location.ToString();
            this.EventLocationId = locationId;
            this.EventName = EventNameEnum.Attack.ToString();
            this.EventType = EventTypeEnum.Move.ToString();
            this.EventData = data.GetXml();

            this.EventTicksToEventTick = ticksToEventTick;
        }
    }
}
