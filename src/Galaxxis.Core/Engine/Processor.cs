using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Extensions;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
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

            // process db objects (for now only one universe supported)
            using (GalaxxisContext context = new GalaxxisContext())
            {
                // get universe
                Universe universe = context.Universe.First();

                // set current tick
                universe.UniverseTicks = universeTicks;

                // process all ticks gone until last processed tick
                while (universe.UniverseTicksProcessed < universe.UniverseTicks)
                {
                    // log time started processing
                    DateTime tStartTick = DateTime.Now;

                    // get events
                    long tick = universe.UniverseTicksProcessed + 1;
                    List<Event> events = EventProvider.GetEvents(tick);

                    // iterate events, do action and remove
                    foreach(Event @event in events) {
                        Do_EventAction(@event, context);
                        context.Event.Remove(@event);
                    }

                    // raise processed ticks
                    universe.UniverseTicksProcessed = tick;

                    // write log
                    Debug.WriteLine($"{DateStamp}: Tick {tick} done in {tStartTick.MsGone()} ms.");
                }

                // save all changes
                context.SaveChanges();
            }
            // mark not processing, unlock processor
            this.IsWorking = false;

            // write log
            Debug.WriteLine($"{DateStamp}: Processing done in {tStartProcessing.MsGone()} ms.");
        }

        /// <summary>
        /// Main definitions of all code to be processed for every possible event
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
            }
        }

        private void Do_BuildOreMine(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocationType.Trim() == EventLocationTypeEnum.Planet.ToString())
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
            if (@event.EventLocationType.Trim() == EventLocationTypeEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetPowerSupply++;
                Debug.WriteLine($"{DateStamp}: Power supply build on planet {planet.PlanetId}.");   
            }
        }

        private void Do_BuildFactory(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocationType.Trim() == EventLocationTypeEnum.Planet.ToString())
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
            if (@event.EventLocationType.Trim() == EventLocationTypeEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet
                    .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetMageAcademy++;
                Debug.WriteLine($"{DateStamp}: Mage Academy build on planet {planet.PlanetId}.");
            }
        }

        #endregion
    }
}
