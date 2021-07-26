using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;

namespace Galaxxis.Core.Engine
{
    public sealed partial class Processor
    {
        /// <summary>
        /// Add events to the database to be processed when they hit the execution tick.
        /// </summary>
        private void Add_EventAction(Event @event, GalaxxisContext context, long universeTickEventIsAdded)
        {
            // events in queue store ticks until it is executed
            // events in db store exact tick when it is executed
            // => a queued event has to add the current tick to it's eventTick
            @event.EventTick += universeTickEventIsAdded;

            // get planet to edit
            // currently only planet events!
            Planet editPlanet = context.Planet
                .Include(planet => planet.PlanetSystem)
                .ThenInclude(system => system.SystemGalaxy)
                .ThenInclude(galaxy => galaxy.GalaxyUniverse)
                .First(p => p.PlanetId == @event.EventLocationId);
            editPlanet.UpdateRessources();

            // pay build costs
            string[] eventDataArray;
            switch (Enum.Parse(typeof(EventTypeEnum), @event.EventType))
            {
                case EventTypeEnum.Build:
                    eventDataArray = @event.EventData.Split('|');
                    editPlanet.PlanetOre -= double.Parse(eventDataArray[0]);
                    editPlanet.PlanetMana -= double.Parse(eventDataArray[1]);
                    editPlanet.PlanetCrystals -= double.Parse(eventDataArray[2]);
                    editPlanet.PlanetElementium -= double.Parse(eventDataArray[3]);
                    break;
            }

            // add event to db context
            Debug.WriteLine($"{DateStamp}: Event added: "
                + $"{@event.EventLocation}, {@event.EventLocationId}, {@event.EventName}.");
            context.Event.Add(@event);
        }
    }
}
