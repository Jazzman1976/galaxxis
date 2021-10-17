using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Core.Provider;
using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
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
        private void Add_EventAction(
            Event @event, GalaxxisContext context, long universeTickEventIsAdded)
        {
            // events in queue store ticks until it is executed
            // events in db store exact tick when it is executed
            // => a queued event has to add the current tick to it's eventTick
            @event.EventTick = @event.EventTicksToEventTick + universeTickEventIsAdded;

            // get planet to edit
            // currently only planet events!
            //Planet editPlanet = context.Planet
            //    .Include(planet => planet.PlanetSystem)
            //    .ThenInclude(system => system.SystemGalaxy)
            //    .ThenInclude(galaxy => galaxy.GalaxyUniverse)
            //    .First(p => p.PlanetId == @event.EventLocationId);
            //editPlanet.UpdateRessources();
            Planet editPlanet = PlanetProvider.GetPlanet(@event.EventLocationId, true, context);

            // pay costs when event is added to database (build|production started)
            switch (Enum.Parse(typeof(EventTypeEnum), @event.EventType))
            {
                case EventTypeEnum.Build:
                    BuildEventTypeData buildData = new BuildEventTypeData(@event.EventData);
                    editPlanet.PlanetOre -= buildData.CostsOre;
                    editPlanet.PlanetMana -= buildData.CostsMana;
                    editPlanet.PlanetCrystals -= buildData.CostsCrystals;
                    editPlanet.PlanetElementium -= buildData.CostsElementium;
                    break;

                case EventTypeEnum.Produce:
                    ProduceEventTypeData prodData = new ProduceEventTypeData(@event.EventData);
                    editPlanet.PlanetTools -= prodData.CostsTools;
                    editPlanet.PlanetCrystals -= prodData.CostsCrystals;
                    editPlanet.PlanetAntimatter -= prodData.CostsAntimatter;
                    editPlanet.PlanetElementium -= prodData.CostsElementium;
                    break;
            }

            // add event to db context
            Debug.WriteLine($"{DateStamp}: Event added: "
                + $"{@event.EventLocation}, {@event.EventLocationId}, {@event.EventName}.");
            context.Event.Add(@event);
        }
    }
}
