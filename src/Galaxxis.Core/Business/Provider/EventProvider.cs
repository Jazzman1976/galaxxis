using Galaxxis.Data.Enums;
using Galaxxis.Core.Exceptions;
using Galaxxis.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Galaxxis.Core.Business.Provider
{
    public static class EventProvider
    {
        public static Event GetEvent(long eventId)
        {
            Event @event = Context.Instance.Database.Event
                .FirstOrDefault(e => e.EventId.Equals(eventId));

            if (@event == null)
            {
                throw new EventNotFoundException();
            }

            return @event;
        }

        public static Event GetEvent(long eventTick, int eventLocationId, EventLocationTypeEnum eventLocationType)
        {
            Event @event = Context.Instance.Database.Event
                .FirstOrDefault(e
                    => e.EventTick.Equals(eventTick)
                    && e.EventLocationId.Equals(eventLocationId)
                    && e.EventLocationType.Equals(eventLocationType.ToString()));

            if (@event == null)
            {
                throw new EventNotFoundException();
            }

            return @event;
        }

        public static List<Event> GetEvents(long eventTick)
        {
            List<Event> events = Context.Instance.Database.Event
                .Where(e => e.EventTick.Equals(eventTick))?
                .ToList()
                ?? new List<Event>();

            return events;
        }

        public static List<Event> GetEvents(int eventLocationId, EventLocationTypeEnum eventLocationType)
        {
            List<Event> events = Context.Instance.Database.Event
                .Where(e
                    => e.EventLocationId.Equals(eventLocationId)
                    && e.EventLocationType.Equals(eventLocationType.ToString()))?
                .ToList()
                ?? new List<Event>();

            return events;
        }

        public static void AddEvent()
        {
            Event buildOreMineEvent = new Event
            {
                EventLocationType = EventLocationTypeEnum.Planet.ToString(),
                EventLocationId = 1,
                EventTick = 3400,
                EventName = EventNameEnum.BuildOreMine.ToString()
            };

            Context.Instance.Database.Event.Add(buildOreMineEvent);

            Context.Instance.Database.SaveChanges();
        }
    }
}
