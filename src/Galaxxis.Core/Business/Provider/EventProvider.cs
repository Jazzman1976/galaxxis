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
            Event @event = null;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                @event = context.Event
                .FirstOrDefault(e => e.EventId.Equals(eventId));
            }
            if (@event == null)
            {
                throw new EventNotFoundException();
            }
            return @event;
        }

        public static Event GetEvent(
            long eventTick, int eventLocationId, EventLocationTypeEnum eventLocationType)
        {
            Event @event = null;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                @event = context.Event
                    .FirstOrDefault(e 
                        => e.EventTick.Equals(eventTick)
                        && e.EventLocationId.Equals(eventLocationId)
                        && e.EventLocationType.Equals(eventLocationType.ToString()));
            }
            if (@event == null)
            {
                throw new EventNotFoundException();
            }
            return @event;
        }

        public static List<Event> GetEvents(long eventTick)
        {
            List<Event> events = new List<Event>();
            using (GalaxxisContext context = new GalaxxisContext())
            {
                events = GetEvents(eventTick, context);
            }
            return events;
        }
        public static List<Event> GetEvents(long eventTick, GalaxxisContext context)
        {
            List<Event> events = new List<Event>();            

            events = context.Event
                .Where(e => e.EventTick <= eventTick)?
                .ToList()
                ?? new List<Event>();
            
            return events;
        }

        public static List<Event> GetEvents(int eventLocationId, EventLocationTypeEnum eventLocationType)
        {
            List<Event> events = new List<Event>();
            using (GalaxxisContext context = new GalaxxisContext())
            {
                events = context.Event
                    .Where(e 
                        => e.EventLocationId.Equals(eventLocationId)
                        && e.EventLocationType.Equals(eventLocationType.ToString()))?
                    .ToList()
                    ?? new List<Event>();
            }
            return events;
        }

        public static Event NewEvent(
            EventLocationTypeEnum type, 
            int locationId, 
            EventNameEnum eventName, 
            long executionTick,
            string eventData = "")
        {
            Event @event = new Event
            {
                EventLocationType = type.ToString(),
                EventLocationId = locationId,
                EventTick = executionTick,
                EventName = eventName.ToString(),
                EventData = eventData
            };
            return @event;
        }
    }
}
