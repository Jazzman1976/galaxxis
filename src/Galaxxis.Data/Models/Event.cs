using System;
using System.Collections.Generic;

#nullable disable

namespace Galaxxis.Data.Models
{
    public partial class Event
    {
        public long EventId { get; set; }
        public long EventTick { get; set; }
        public string EventLocationType { get; set; }
        public int EventLocationId { get; set; }
        public string EventName { get; set; }
        public string EventData { get; set; }
    }
}
