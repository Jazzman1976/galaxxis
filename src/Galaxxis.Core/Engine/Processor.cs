using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Extensions;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxxis.Core.Engine
{
    public sealed class Processor
    {
        #region Fields ----------------------------------------------------------------------------

        private static Processor _instance = null;
        private static readonly string _dateFormat = "HH.mm.ss.fffffff";

        #endregion


        #region Properties ------------------------------------------------------------------------

        public bool IsWorking { get; set; }
        public Queue<Event> EventQueue { get; set; } = new Queue<Event>();
        private string DateStamp => DateTime.Now.ToString(_dateFormat);
        public static Processor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Processor();
                }
                return _instance;
            }
        }

        #endregion


        #region Methods ---------------------------------------------------------------------------

        public Task ProcessAsync(long universeTicks)
        {
            return Task.Run(() => Process(universeTicks));
        }

        /// <summary>
        /// This is done for every single tick gone until last processed tick.
        /// </summary>
        private void Process(long universeTicks)
        {
            // skip if task is still running
            if (this.IsWorking)
            {
                Debug.WriteLine($"{DateStamp}: Still processing... Tick {universeTicks} is queued.");
                return;
            }

            // mark processing, block another run simultaniously
            this.IsWorking = true;

            // log time started processing
            DateTime tStartProcessing = DateTime.Now;

            // for debugging purposes
            //System.Threading.Thread.Sleep(5000);

            // process db objects (for now only one universe supported)
            using (GalaxxisContext context = new GalaxxisContext())
            {
                // get universe
                Universe universe = context.Universe.First();

                // process all ticks gone until last processed tick
                while (universe.UniverseTicks < universeTicks)
                {
                    // log time started processing
                    DateTime tStartTick = DateTime.Now;

                    // raise tick to be processed
                    universe.UniverseTicks++;

                    // add new Events to Database
                    while (EventQueue.Any())
                    {
                        Event @event = EventQueue.Dequeue();
                        Add_EventAction(@event, context, universe.UniverseTicks);
                    }

                    // iterate events, do action and remove
                    List<Event> events = EventProvider.GetEvents(universe.UniverseTicks, context);
                    foreach (Event @event in events) {
                        Do_EventAction(@event, context);
                        context.Event.Remove(@event);
                    }

                    // write log
                    Debug.WriteLine($"{DateStamp}: Tick {universe.UniverseTicks} done in {tStartTick.MsGone()} ms.");
                }

                // save all changes
                context.SaveChanges();
            }
            // mark not processing, unlock processor
            this.IsWorking = false;

            // write log
            Debug.WriteLine($"{DateStamp}: Processing completely done in {tStartProcessing.MsGone()} ms.");
        }

        /// <summary>
        /// Main definitions of all code to be processed for every possible event.
        /// </summary>
        private void Do_EventAction(Event @event, GalaxxisContext context)
        {
            switch (Enum.Parse(typeof(EventNameEnum), @event.EventName))
            {
                case EventNameEnum.BuildOreMine:
                    Do_BuildOreMine(@event, context);
                    break;

                case EventNameEnum.BuildPowerSupply:
                    Do_BuildPowerSupply(@event, context);                    
                    break;

                case EventNameEnum.BuildFactory:
                    Do_BuildFactory(@event, context);
                    break;

                case EventNameEnum.BuildMageAcademy:
                    Do_BuildMageAcademy(@event, context);
                    break;

                case EventNameEnum.BuildCrystalMine:
                    Do_BuildCrystalMine(@event, context);
                    break;

                case EventNameEnum.BuildNeutronAccelerator:
                    Do_BuildNeutronAccelerator(@event, context);
                    break;
            }
        }

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

            string[] eventDataArray;
            switch (Enum.Parse(typeof(EventTypeEnum), @event.EventType))
            {
                case EventTypeEnum.Build:
                    // pay build costs
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

        private void Do_BuildOreMine(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetOreMine++;
                Debug.WriteLine($"{DateStamp}: Ore Mine build on planet {planet.PlanetId}.");   
            }
        }

        private void Do_BuildPowerSupply(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetPowerSupply++;
                Debug.WriteLine($"{DateStamp}: Power Supply build on planet {planet.PlanetId}.");   
            }
        }

        private void Do_BuildFactory(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetFactory++;
                Debug.WriteLine($"{DateStamp}: Factory build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildMageAcademy(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetMageAcademy++;
                Debug.WriteLine($"{DateStamp}: Mage Academy build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildCrystalMine(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetCrystalMine++;
                Debug.WriteLine($"{DateStamp}: Chrystal Mine build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildNeutronAccelerator(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetNeutronAccelerator++;
                Debug.WriteLine($"{DateStamp}: Neutron Accelerator build on planet {planet.PlanetId}.");
            }
        }

        #endregion
    }
}
