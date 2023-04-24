using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.BuildEvents
{
    public class BuildDefenseSystemEvent : BaseBuildEvent
    {
        private BuildDefenseSystemEvent() { }

        public BuildDefenseSystemEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            BuildEventTypeData data)
            : base(location, locationId, ticksToEventTick, data, EventNameEnum.BuildDefenseSystem)
        { }
    }
}
