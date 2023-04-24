using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxxis.Data.Events
{
    public class ScanEvent : Event
    {
        private ScanEvent() { }

        public ScanEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            ScanEventTypeData data)
        {
            this.EventLocation = location.ToString();
            this.EventLocationId = locationId;
            this.EventName = EventNameEnum.Scan.ToString();
            this.EventType = EventTypeEnum.Move.ToString();
            this.EventData = data.GetXml();
            this.EventTicksToEventTick = ticksToEventTick;
        }
    }
}
