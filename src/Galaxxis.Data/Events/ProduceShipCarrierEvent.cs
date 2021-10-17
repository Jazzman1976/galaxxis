using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxxis.Data.Events
{
    public class ProduceShipCarrierEvent : Event
    {
        private ProduceShipCarrierEvent() { }

        public ProduceShipCarrierEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            ProduceEventTypeData data)
        {
            this.EventLocation = location.ToString();
            this.EventLocationId = locationId;
            this.EventName = EventNameEnum.ProduceShipCarrier.ToString();
            this.EventData = data.GetXml();
            this.EventType = EventTypeEnum.Produce.ToString();

            this.EventTicksToEventTick = ticksToEventTick;
        }
    }
}
