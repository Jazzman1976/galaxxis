using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.BuildEvents
{
    public class BuildPowerSupplyEvent : BaseBuildEvent
    {
        private BuildPowerSupplyEvent() { }

        public BuildPowerSupplyEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            BuildEventTypeData data)
            : base(location, locationId, ticksToEventTick, data, EventNameEnum.BuildPowerSupply)
        { }
    }
}
