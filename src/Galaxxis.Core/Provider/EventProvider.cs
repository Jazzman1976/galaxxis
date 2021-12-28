using Galaxxis.Data.Enums;
using Galaxxis.Core.Exceptions;
using Galaxxis.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Galaxxis.Core.Provider
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
            long eventTick, int eventLocationId, EventLocationEnum eventLocation)
        {
            Event @event = null;
            using (GalaxxisContext context = new GalaxxisContext())
            {
                @event = context.Event
                    .FirstOrDefault(e 
                        => e.EventTick.Equals(eventTick)
                        && e.EventLocationId.Equals(eventLocationId)
                        && e.EventLocation.Trim().Equals(eventLocation.ToString()));
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

        public static List<Event> GetEvents(int eventLocationId, EventLocationEnum eventLocation)
        {
            List<Event> events = new List<Event>();
            using (GalaxxisContext context = new GalaxxisContext())
            {
                events = context.Event
                    .Where(e 
                        => e.EventLocationId.Equals(eventLocationId)
                        && e.EventLocation.Trim().Equals(eventLocation.ToString()))?
                    .ToList()
                    ?? new List<Event>();
            }
            return events;
        }

        public static List<Event> GetEvents(
            int eventLocationId, EventLocationEnum eventLocation, EventTypeEnum eventType)
        {
            List<Event> events = GetEvents(eventLocationId, eventLocation)
                .Where(e => e.EventType.Trim().Equals(eventType.ToString()))?
                .ToList() 
                ?? new List<Event>();
            return events;
        }
    }
}
