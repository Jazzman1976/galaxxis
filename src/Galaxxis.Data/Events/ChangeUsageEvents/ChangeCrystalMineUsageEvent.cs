using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.ChangeUsageEvents
{
    public class ChangeCrystalMineUsageEvent : BaseChangeUsageEvent
    {
        private ChangeCrystalMineUsageEvent() { }

        public ChangeCrystalMineUsageEvent(
            EventLocationEnum location,
            int locationId,
            ChangeUsageEventTypeData data)
            : base(location, locationId, data, EventNameEnum.ChangeCrystalMineUsage)
        { }
    }
}
