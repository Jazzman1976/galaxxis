using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;

namespace Galaxxis.Data.Events.BuildEvents
{
    public class BuildElementiumSynthesizerEvent : BaseBuildEvent
    {
        private BuildElementiumSynthesizerEvent() { }

        public BuildElementiumSynthesizerEvent(
            EventLocationEnum location,
            int locationId,
            long ticksToEventTick,
            BuildEventTypeData data)
            : base(
                  location, 
                  locationId, 
                  ticksToEventTick, 
                  data, 
                  EventNameEnum.BuildElementiumSynthesizer)
        { }
    }
}
