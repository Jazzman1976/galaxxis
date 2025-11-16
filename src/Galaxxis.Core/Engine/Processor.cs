using Galaxxis.Core.Actions;
using Galaxxis.Core.Extensions;
using Galaxxis.Core.Provider;
using Galaxxis.Data.Enums;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Galaxxis.Core.Engine
{
    public sealed partial class Processor
    {
        #region Fields ----------------------------------------------------------------------------

        private static Processor _instance = null;

        #endregion


        #region Properties ------------------------------------------------------------------------

        // make IsWorking atomic via an int flag
        private int _isWorkingFlag = 0;
        public bool IsWorking => Interlocked.CompareExchange(ref _isWorkingFlag, 0, 0) == 1;
        public ConcurrentQueue<Event> EventQueue { get; set; } = new ConcurrentQueue<Event>();

        private static string DateStamp => DateTime.Now.ToString(Static.Constants.DEBUG_TIME_FORMAT);


        /****************************************************************************
         * The _actionStore is important! Actions not registered here are not found!
         * Change this software architecture to use Dependency Injection instead!
         ****************************************************************************/

        /// <summary>
        /// Register all event actions that should be processed by the processor
        /// </summary>
        private static readonly List<IAction> _actionStore = new()
        {
            // register build actions of producing buildings
            new BuildOreMineAction(),                       // Ore
            new BuildPowerSupplyAction(),                   // Tools
            new BuildFactoryAction(),                       // Energy
            new BuildMageAcademyAction(),                   // Mana
            new BuildCrystalMineAction(),                   // Crystals
            new BuildNeutronAcceleratorAction(),            // Antimatter
            new BuildElementiumSynthesizerAction(),         // Elementium

            // register build actions of special buildings
            new BuildShipyardAction(),                      // Ships
            new BuildDefenseSystemAction(),                 // Defense

            // register change usage actions of producing buildings
            new ChangeOreMineUsageAction(),                 // Ore
            new ChangeFactoryUsageAction(),                 // Tools
            new ChangePowerSupplyUsageAction(),             // Energy
            new ChangeMageAcademyUsageAction(),             // Mana
            new ChangeCrystalMineUsageAction(),             // Crystals
            new ChangeNeutronAcceleratorUsageAction(),      // Antimatter
            new ChangeElementiumSynthesizerUsageAction(),   // Elementium

            // register produce ship actions
            new ProduceShipExplorerAction(),                // Explorer
            new ProduceShipCarrierAction(),                 // Carrier

            // register produce defense action
            new ProduceDefenseLightManualCannonAction(),    // Light Manual Cannon

            // register fleet commands
            new FleetCommandAttackAction(),                 // Attack
            new FleetCommandMoveAction(),                   // Move
            new FleetCommandScanAction(),                   // Scan
        };

        /// <summary>
        /// Singelton design pattern instance
        /// </summary>
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


        #region Common Methods --------------------------------------------------------------------

        /// <summary>
        /// Starts processing all ticks not still processed up to latest universeTicks. 
        /// If processor is still running, processing is skipped and the tick is queued. 
        /// </summary>
        /// <param name="universeTicks"></param>
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
            // mark processing, block another run simultaniously
            // try to atomically set working flag; if already 1 -> skip
            if (Interlocked.Exchange(ref _isWorkingFlag, 1) == 1)
            {
                Debug.WriteLine(
                    $"{DateStamp}: Still processing... Tick {universeTicks} is queued.");
                return;
            }

            // log time started processing all ticks
            DateTime tStartProcessing = DateTime.Now;

            try
            {

                // process db objects
                using (GalaxxisContext context = new ())
                {
                    // get universe (for now only one universe supported)
                    Universe universe = context.Universe.First();

                    // process all ticks gone until last processed tick
                    while (universe.UniverseTicks < universeTicks)
                    {
                        // log time started processing a single tick
                        DateTime tStartTick = DateTime.Now;

                        // raise universe ticks for next tick to be processed
                        universe.UniverseTicks++;

                        // read queued events and add new Events to Database
                        while (EventQueue.TryDequeue(out Event queuedEvent))
                        {
                            Add_EventAction(queuedEvent, context, universe.UniverseTicks);
                        }

                        // process all events in the database for the current tick
                        List<Event> events = EventProvider
                            .GetEvents(universe.UniverseTicks, context);
                        foreach (Event @event in events)
                        {
                            Do_EventAction(@event, context);
                            context.Event.Remove(@event);
                        }

                        // save changes processing the tick has made to the context to database
                        context.SaveChanges();

                        // write log for the single tick processed
                        Debug.WriteLine($"{DateStamp}: Tick {universe.UniverseTicks} done in "
                            + $"{tStartTick.MsGone()} ms.");
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: replace with injected logging, for now Debug.Write
                Debug.WriteLine($"{DateStamp}: ERROR processing ticks: {ex}");
            }
            finally
            {
                // release working flag
                // mark not processing, unlock processor
                Interlocked.Exchange(ref _isWorkingFlag, 0);
                // write log for all ticks processed
                Debug.WriteLine(
                    $"{DateStamp}: Processing completely done in {tStartProcessing.MsGone()} ms.");
                Debug.WriteLine("");
            }
        }

        /// <summary>
        /// Add events to the database to be processed when they hit the execution tick.
        /// </summary>
        private static void Add_EventAction(
            Event @event, GalaxxisContext context, long universeTickEventIsAdded)
        {
            // events in queue store ticks until it is executed
            // events in db store exact tick when it is executed
            // => a queued event has to add the current tick to it's eventTick
            @event.EventTick = @event.EventTicksToEventTick + universeTickEventIsAdded;

            // get planet to edit
            // currently only planet events!
            Planet editPlanet = PlanetProvider.GetPlanet(@event.EventLocationId, true, context);

            // pay costs when event is added to database (build|production started)
            // depending on type
            // XML is deserialized depending on type to other classes
            // other XML deserialize classes may be added in the future for other purposes
            switch (Enum.Parse(typeof(EventTypeEnum), @event.EventType))
            {
                case EventTypeEnum.Build:
                    BuildEventTypeData buildData = new (@event.EventData);
                    editPlanet.PlanetOre -= buildData.CostsOre;
                    editPlanet.PlanetMana -= buildData.CostsMana;
                    editPlanet.PlanetCrystals -= buildData.CostsCrystals;
                    editPlanet.PlanetElementium -= buildData.CostsElementium;
                    break;

                case EventTypeEnum.Produce:
                    ProduceEventTypeData prodData = new (@event.EventData);
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

        /// <summary>
        /// Main definitions of all code to be processed for every possible event.
        /// </summary>
        private static void Do_EventAction(Event @event, GalaxxisContext context)
        {
            /****************************************************************
             * This is using the instance's _actionStore!
             * Actions need to be registered there to be found!
             ****************************************************************/

            // find first action in action store by trigger event
            IAction triggeredAction = _actionStore
                .FirstOrDefault(
                    a => a.TriggerEvent == (EventNameEnum)Enum.Parse(
                        typeof(EventNameEnum), 
                        @event.EventName));

            // if found, execute it
            if (triggeredAction != null)
            {
                bool success = triggeredAction.DoAction(@event, context, out string feedback);
                Debug.WriteLine((success ? "Action done: " : "Action failed: ") + feedback);
                return;
            }

            // if not found, warn missing event action!
            Debug.WriteLine($"WARNING: Action for Event '{@event.EventName}' missing!");
        }

        #endregion
    }
}
