using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.BuildEvents
{
    public class BuildMageAcademyEvent : BaseBuildEvent
    {
        private BuildMageAcademyEvent() { }

        public BuildMageAcademyEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            BuildEventTypeData data)
            : base(location, locationId, ticksToEventTick, data, EventNameEnum.BuildMageAcademy)
        { }
    }
}
