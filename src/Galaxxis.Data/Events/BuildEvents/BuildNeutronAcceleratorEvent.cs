using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.BuildEvents
{
    public class BuildNeutronAcceleratorEvent : BaseBuildEvent
    {
        private BuildNeutronAcceleratorEvent() { }

        public BuildNeutronAcceleratorEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            BuildEventTypeData data)
            : base(
                  location, 
                  locationId, 
                  ticksToEventTick, 
                  data, 
                  EventNameEnum.BuildNeutronAccelerator)
        { }
    }
}
