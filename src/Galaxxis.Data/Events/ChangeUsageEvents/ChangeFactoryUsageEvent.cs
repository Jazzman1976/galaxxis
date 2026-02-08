using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.ChangeUsageEvents
{
    public class ChangeFactoryUsageEvent : BaseChangeUsageEvent
    {
        private ChangeFactoryUsageEvent() { }

        public ChangeFactoryUsageEvent(
            EventLocationEnum location,
            int locationId,
            ChangeUsageEventTypeData data)
            : base(location, locationId, data, EventNameEnum.ChangeFactoryUsage)
        { }
    }
}
