using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxxis.Data.Events
{
    public class ProduceShipExplorerEvent : Event
    {
        private ProduceShipExplorerEvent() { }

        public ProduceShipExplorerEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            ProduceEventTypeData data)
        {
            this.EventLocation = location.ToString();
            this.EventLocationId = locationId;
            this.EventName = EventNameEnum.ProduceShipExplorer.ToString();
            this.EventData = data.GetXml();
            this.EventType = EventTypeEnum.Produce.ToString();

            this.EventTicksToEventTick = ticksToEventTick;
        }
    }
}
