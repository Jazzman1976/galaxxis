using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.ChangeUsageEvents
{
    public class ChangePowerSupplyUsageEvent : BaseChangeUsageEvent
    {
        private ChangePowerSupplyUsageEvent() { }

        public ChangePowerSupplyUsageEvent(
            EventLocationEnum location,
            int locationId,
            ChangeUsageEventTypeData data)
            : base(location, locationId, data, EventNameEnum.ChangePowerSupplyUsage)
        { }
    }
}
