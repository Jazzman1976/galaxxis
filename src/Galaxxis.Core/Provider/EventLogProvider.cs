using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Galaxxis.Core.Provider
{
    public static class EventLogProvider
    {
        public static List<EventLog> GetEventLogs(EventLocationEnum location, int locationId)
        {
            List<EventLog> logs;
            string locationName = location.ToString();
            using (GalaxxisContext context = new GalaxxisContext())
            {
                logs = context.EventLog
                    .Where(log
                        => locationName.Equals(log.EventLocation)
                        && locationId.Equals(log.EventLocationId))
                    .ToList();
            }
            return logs;
        }
    }
}
