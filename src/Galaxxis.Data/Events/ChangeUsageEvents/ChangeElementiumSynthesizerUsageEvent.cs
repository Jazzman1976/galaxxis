using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.ChangeUsageEvents
{
    public class ChangeElementiumSynthesizerUsageEvent : BaseChangeUsageEvent
    {
        private ChangeElementiumSynthesizerUsageEvent() { }

        public ChangeElementiumSynthesizerUsageEvent(
            EventLocationEnum location,
            int locationId,
            ChangeUsageEventTypeData data)
            : base(location, locationId, data, EventNameEnum.ChangeElementiumSynthesizerUsage)
        { }
    }
}
