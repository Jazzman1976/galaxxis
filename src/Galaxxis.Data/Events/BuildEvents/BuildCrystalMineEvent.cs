using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.BuildEvents
{
    public class BuildCrystalMineEvent : BaseBuildEvent
    {
        private BuildCrystalMineEvent() { }

        public BuildCrystalMineEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            BuildEventTypeData data)
            : base(location, locationId, ticksToEventTick, data, EventNameEnum.BuildCrystalMine)
        { }
    }
}
