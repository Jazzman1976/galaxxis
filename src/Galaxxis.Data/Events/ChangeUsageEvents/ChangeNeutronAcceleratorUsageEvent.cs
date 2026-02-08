using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.ChangeUsageEvents
{
    public class ChangeNeutronAcceleratorUsageEvent : BaseChangeUsageEvent
    {
        private ChangeNeutronAcceleratorUsageEvent() { }

        public ChangeNeutronAcceleratorUsageEvent(
            EventLocationEnum location,
            int locationId,
            ChangeUsageEventTypeData data)
            : base(location, locationId, data, EventNameEnum.ChangeNeutronAcceleratorUsage)
        { }
    }
}
