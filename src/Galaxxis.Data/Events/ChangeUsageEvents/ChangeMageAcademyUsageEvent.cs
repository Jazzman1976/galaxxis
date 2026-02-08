using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.ChangeUsageEvents
{
    public class ChangeMageAcademyUsageEvent : BaseChangeUsageEvent
    {
        private ChangeMageAcademyUsageEvent() { }

        public ChangeMageAcademyUsageEvent(
            EventLocationEnum location,
            int locationId,
            ChangeUsageEventTypeData data)
            : base(location, locationId, data, EventNameEnum.ChangeMageAcademyUsage)
        { }
    }
}
