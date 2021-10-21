using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.BuildEvents
{
    public class BuildFactoryEvent : BaseBuildEvent
    {
        private BuildFactoryEvent() { }

        public BuildFactoryEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            BuildEventTypeData data)
            : base(location, locationId, ticksToEventTick, data, EventNameEnum.BuildFactory)
        { }
    }
}
