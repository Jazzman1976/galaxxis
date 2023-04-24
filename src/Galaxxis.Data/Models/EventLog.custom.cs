using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxxis.Data.Models
{
    public partial class EventLog
    {
        public EventLog() { }
        public EventLog(Event @event, Func<string> getData)
        {
            EventData = getData();
            EventLocation = @event.EventLocation;
            EventLocationId = @event.EventLocationId;
            EventTick = @event.EventTick;
            EventName = @event.EventName;
            EventType = @event.EventType;
        }
    }
}
