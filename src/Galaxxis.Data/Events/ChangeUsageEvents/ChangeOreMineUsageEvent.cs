using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.ChangeUsageEvents
{
    public class ChangeOreMineUsageEvent : BaseChangeUsageEvent
    {
        private ChangeOreMineUsageEvent() { }

        public ChangeOreMineUsageEvent(
            EventLocationEnum location,
            int locationId,
            ChangeUsageEventTypeData data)
            : base(location, locationId, data, EventNameEnum.ChangeOreMineUsage)
        { }
    }
}
