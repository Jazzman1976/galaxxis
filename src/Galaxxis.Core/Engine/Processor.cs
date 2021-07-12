using Galaxxis.Core.Business;
using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Extensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;
using System.Collections.Concurrent;
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

        private bool IsWorking { get; set; }

        //private bool HasChanges { get; set; }

        private ConcurrentDictionary<int, Universe> ProcessedUniverses { get; set; }
            = new ConcurrentDictionary<int, Universe>();

        private ConcurrentDictionary<int, Planet> ProcessedPlanets { get; set; }
            = new ConcurrentDictionary<int, Planet>();

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

        /// <summary>
        /// Calculates entity changes for all entities in the database. 
        /// This is done for every single tick gone until last tick. 
        /// Changes are dropped to Instance dictionaries instead saving them to db. 
        /// Can't save them in async task due to access errors in other tasks on same entities.
        /// </summary>
        public async Task ProcessTask()
        {
            // start processing
            await Task.Run(Instance.Process);
        }

        /// <summary>
        /// Uses an own context to be threatsave if executed in async tasks
        /// </summary>
        private void Process()
        {
            // skip if task is still running
            if (this.IsWorking)
            {
                return;
            }

            // write debug info
            // Debug.WriteLine("" + DateTime.Now.ToString(_dateFormat) + " : Running 'Processor Process'.");

            // mark processing
            this.IsWorking = true;

            List<Universe> universes = Context.Instance.Database.Universe.ToList();
            foreach (Universe universe in universes)
            {
                DateTime TStart = DateTime.Now;

                while (universe.UniverseTicksProcessed < universe.UniverseTicks)
                {
                    // get events
                    long tick = universe.UniverseTicksProcessed + 1;
                    List<Event> events = EventProvider.GetEvents(tick);

                    // collect tasks
                    List<Task> eventTasks = new List<Task>();
                    foreach (Event @event in events)
                    {
                        switch (Enum.Parse(typeof(EventNameEnum), @event.EventName))
                        {
                            case EventNameEnum.BuildOreMine:
                                {
                                    eventTasks.Add(new Task(() =>
                                    {
                                        int planetId = @event.EventLocationId;
                                        Planet planet = Context.Instance.Database.Planet
                                            .FirstOrDefault(p => p.PlanetId.Equals(planetId));
                                        planet.PlanetOreMine++;
                                    }));

                                    Debug.WriteLine(
                                        ""
                                        + DateTime.Now.ToString(_dateFormat)
                                        + $" : {EventNameEnum.BuildOreMine} task added.");
                                }
                                break;
                        }
                    }

                    // execute tasks
                    if (eventTasks.Any())
                    {
                        // start tasks
                        foreach (Task task in eventTasks)
                        {
                            task.Start();
                        }

                        // wait until every task has been finished                    
                        Task.WaitAll(eventTasks.ToArray());
                    }

                    // raise processed ticks
                    universe.UniverseTicksProcessed++;
                    Debug.WriteLine(
                        ""
                        + DateTime.Now.ToString(_dateFormat)
                        + $" : Tick {tick} in universe {universe.UniverseName} processed in "
                        + $"{new TimeSpan(DateTime.Now.Ticks - TStart.Ticks).Milliseconds} ms.");
                }
            }

                // init new context, modify entities and drop them to dictionaries
                /*
                using (GalaxxisContext asyncContext = new GalaxxisContext())
                {
                    // get universe
                    Universe universe = asyncContext.Universe.First();

                    // process all ticks until current tick is reached
                    while (universe.UniverseTicksProcessed < universe.UniverseTicks)
                    {
                        // raise tick + 1
                        universe.UniverseTicksProcessed++;

                        // Process every 120 ticks (2 Minutes)
                        if (universe.UniverseTicksProcessed % 120 == 0)
                        {
                            foreach (Planet planet in asyncContext.Planet)
                            {
                                // Save current ressources to db.
                                // Ressources could be calculated for each tick instead 
                                // reading from db. The field 'PlanetDataTick' shows which 
                                // tick is stored to db.
                                planet.UpdateRessources();
                            }
                        }
                    }

                    // drop changed universe entitiy
                    Universe[] modifiedUniverses =
                        asyncContext.ChangeTracker.Entries<Universe>()
                        .Where(u => u.State == Microsoft.EntityFrameworkCore.EntityState.Modified)
                        .Select(u => u.Entity)
                        .ToArray();
                    foreach (Universe modifiedUniverse in modifiedUniverses)
                    {
                        Instance.ProcessedUniverses[modifiedUniverse.UniverseId] = modifiedUniverse;
                    }

                    // drop changed planet entities
                    Planet[] modifiedPlanets =
                        asyncContext.ChangeTracker.Entries<Planet>()
                        .Where(p => p.State == Microsoft.EntityFrameworkCore.EntityState.Modified)
                        .Select(p => p.Entity)
                        .ToArray();
                    foreach (Planet modifiedPlanet in modifiedPlanets)
                    {
                        Instance.ProcessedPlanets[modifiedPlanet.PlanetId] = modifiedPlanet;
                    }
                }
                */

                // mark HasChanges
                //this.HasChanges = true;

                // update database
                Context.Instance.Database.SaveChanges();
            
            // mark not processing, unlock processor
            this.IsWorking = false;

            // write debug info
            // Debug.WriteLine("" + DateTime.Now.ToString(_dateFormat) + " : 'Processor Process' finished.");
        }

        /*

        /// <summary>
        /// Sync processed objects to the db context instance
        /// </summary>
        public void SyncChanges()
        {
            // skip if Process task is still running or no changes were made
            if (this.IsWorking || !this.HasChanges)
            {
                return;
            }

            // write debug info
            Debug.WriteLine("" + DateTime.Now.ToString(_dateFormat) + " : Running 'Processor SyncChanges'.");

            // mark processing
            this.IsWorking = true;

            // sync universe changes from dictionaries to context
            while (this.ProcessedUniverses.Any())
            {
                // get modified universe
                int key = this.ProcessedUniverses.First().Key;
                if (!this.ProcessedUniverses.Remove(key, out Universe modifiedUniverse))
                { 
                    continue; 
                }

                // get context db universe
                Universe contextUniverse
                    = Context.Instance.Database.Universe.Find(modifiedUniverse.UniverseId);
                if (contextUniverse == null)
                {
                    continue;
                }

                // write changes to context
                contextUniverse.UniverseTicksProcessed = modifiedUniverse.UniverseTicksProcessed;
            }

            // sync planet changes
            while (this.ProcessedPlanets.Any())
            {
                // get modified planet
                int key = this.ProcessedPlanets.First().Key;
                if (!this.ProcessedPlanets.Remove(key, out Planet modifiedPlanet))
                {
                    continue;
                }

                // get context db planet
                Planet contextPlanet
                    = Context.Instance.Database.Planet.Find(modifiedPlanet.PlanetId);
                if (contextPlanet == null)
                {
                    continue;
                }

                // write changes to context
                contextPlanet.PlanetDataTick = modifiedPlanet.PlanetDataTick;
                contextPlanet.PlanetOre = modifiedPlanet.PlanetOre;
                contextPlanet.PlanetEnergy = modifiedPlanet.PlanetEnergy;
                contextPlanet.PlanetTools = modifiedPlanet.PlanetTools;
                contextPlanet.PlanetMana = modifiedPlanet.PlanetMana;
                contextPlanet.PlanetCrystals = modifiedPlanet.PlanetCrystals;
                contextPlanet.PlanetAntimatter = modifiedPlanet.PlanetAntimatter;
                contextPlanet.PlanetElementium = modifiedPlanet.PlanetElementium;
                contextPlanet.PlanetPowerSupply = modifiedPlanet.PlanetPowerSupply;
                contextPlanet.PlanetOreMine = modifiedPlanet.PlanetOreMine;
                contextPlanet.PlanetFactory = modifiedPlanet.PlanetFactory;
                contextPlanet.PlanetNeutronAccelerator = modifiedPlanet.PlanetNeutronAccelerator;
                contextPlanet.PlanetCrystalMine = modifiedPlanet.PlanetCrystalMine;
                contextPlanet.PlanetMageAcademy = modifiedPlanet.PlanetMageAcademy;
                contextPlanet.PlanetElementiumSynthesizer = modifiedPlanet.PlanetElementiumSynthesizer;
            }

            // reset HasChanges
            this.HasChanges = false;

            // mark not processing, unlock processor
            this.IsWorking = false;

            // write debug info
            Debug.WriteLine("" + DateTime.Now.ToString(_dateFormat) + " : 'Processor SyncChanges' finished.");
        }

        /// <summary>
        /// Save all changes of the db context instance to the database.
        /// Skip if currently changes are in progress to the db context instance.
        /// </summary>
        public void SaveChanges()
        {
            // skip if Process task is still running
            if (this.IsWorking)
            {
                return;
            }

            // write debug info
            Debug.WriteLine("" + DateTime.Now.ToString(_dateFormat) + " : Running 'Processor SaveChanges'.");

            // mark processing
            this.IsWorking = true;

            // write changes to db
            Context.Instance.Database.SaveChanges();

            // mark not processing, unlock processor
            this.IsWorking = false;

            // write debug info
            Debug.WriteLine("" + DateTime.Now.ToString(_dateFormat) + " : 'Processor SaveChanges' finished.");
        }

        */

        #endregion
    }
}
