using Galaxxis.Core.Extensions;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Core.Provider;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Events;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxxis.Core.Engine
{
    public sealed partial class Processor
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


        #region Common Methods --------------------------------------------------------------------

        /// <summary>
        /// Starts processing all ticks not still processed up to universeTicks. 
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
                    foreach (Event @event in events)
                    {
                        Do_EventAction(@event, context);
                        context.Event.Remove(@event);
                    }

                    // save changes the tick has made
                    context.SaveChanges();

                    // write log
                    Debug.WriteLine($"{DateStamp}: Tick {universe.UniverseTicks} done in "
                        + $"{tStartTick.MsGone()} ms.");
                }

                // save all changes
                //context.SaveChanges();
            }

            // mark not processing, unlock processor
            this.IsWorking = false;

            // write log
            Debug.WriteLine($"{DateStamp}: Processing completely done in "
                + $"{tStartProcessing.MsGone()} ms.");
            Debug.WriteLine("");
        }

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

        /// <summary>
        /// Main definitions of all code to be processed for every possible event.
        /// </summary>
        private void Do_EventAction(Event @event, GalaxxisContext context)
        {
            switch (Enum.Parse(typeof(EventNameEnum), @event.EventName))
            {
                #region Build event actions -------------------------------------------------------

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

                case EventNameEnum.BuildElementiumSynthesizer:
                    Do_BuildElementiumSynthesizer(@event, context);
                    break;

                case EventNameEnum.BuildShipyard:
                    Do_BuildShipyard(@event, context);
                    break;

                #endregion

                #region Change event actions ------------------------------------------------------

                case EventNameEnum.ChangeOreMineUsage:
                    Do_ChangeOreMineUsage(@event, context);
                    break;

                case EventNameEnum.ChangePowerSupplyUsage:
                    Do_ChangePowerSupplyUsage(@event, context);
                    break;

                case EventNameEnum.ChangeFactoryUsage:
                    Do_ChangeFactoryUsage(@event, context);
                    break;

                case EventNameEnum.ChangeMageAcademyUsage:
                    Do_ChangeMageAcademyUsage(@event, context);
                    break;

                case EventNameEnum.ChangeCrystalMineUsage:
                    Do_ChangeCrystalMineUsage(@event, context);
                    break;

                case EventNameEnum.ChangeNeutronAcceleratorUsage:
                    Do_ChangeNeutronAcceleratorUsage(@event, context);
                    break;

                case EventNameEnum.ChangeElementiumSynthesizerUsage:
                    Do_ChangeElementiumSynthesizerUsage(@event, context);
                    break;

                #endregion

                #region Produce event actions -----------------------------------------------------

                case EventNameEnum.ProduceShipExplorer:
                    Do_ProduceShipExplorer(@event, context);
                    break;

                case EventNameEnum.ProduceShipCarrier:
                    Do_ProduceShipCarrier(@event, context);
                    break;

                #endregion

                #region Fleet event actions -------------------------------------------------------

                case EventNameEnum.Attack:
                    Do_Attack(@event, context);
                    break;

                case EventNameEnum.Return:
                    Do_Return(@event, context);
                    break;

                #endregion
            }
        }

        #endregion


        #region Do Build Event Methods ------------------------------------------------------------

        private void Do_BuildOreMine(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetOreMine++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Ore Mine build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildPowerSupply(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetPowerSupply++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Power Supply build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildFactory(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetFactory++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Factory build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildMageAcademy(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetMageAcademy++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Mage Academy build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildCrystalMine(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetCrystalMine++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Chrystal Mine build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildNeutronAccelerator(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetNeutronAccelerator++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Neutron Accelerator build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildElementiumSynthesizer(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetElementiumSynthesizer++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Elementium Synthesizer build on planet {planet.PlanetId}.");
            }
        }

        private void Do_BuildShipyard(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetShipyard++;
                planet.ClearCache();
                Debug.WriteLine($"{DateStamp}: Shipyard build on planet {planet.PlanetId}.");
            }
        }

        #endregion --------------------------------------------------------------------------------


        #region Do Change Event Methods -----------------------------------------------------------

        private void Do_ChangeOreMineUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetOreMineUsage
                    = new ChangeUsageEventTypeData(@event.EventData).NewValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Ore Mine usage changed on planet {planet.PlanetId}.");
            }
        }

        private void Do_ChangePowerSupplyUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetPowerSupplyUsage
                    = new ChangeUsageEventTypeData(@event.EventData).NewValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Power Supply usage changed on planet {planet.PlanetId}.");
            }
        }

        private void Do_ChangeFactoryUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetFactoryUsage
                    = new ChangeUsageEventTypeData(@event.EventData).NewValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Power Supply usage changed on planet {planet.PlanetId}.");
            }
        }

        private void Do_ChangeMageAcademyUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetMageAcademyUsage
                    = new ChangeUsageEventTypeData(@event.EventData).NewValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Mage Academy usage changed on planet {planet.PlanetId}.");
            }
        }

        private void Do_ChangeCrystalMineUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetCrystalMineUsage
                    = new ChangeUsageEventTypeData(@event.EventData).NewValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Crystal Mine usage changed on planet {planet.PlanetId}.");
            }
        }

        private void Do_ChangeNeutronAcceleratorUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetNeutronAcceleratorUsage
                    = new ChangeUsageEventTypeData(@event.EventData).NewValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Neutron Accelerator usage changed on planet {planet.PlanetId}.");
            }
        }

        private void Do_ChangeElementiumSynthesizerUsage(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                planet.UpdateRessources();
                planet.PlanetElementiumSynthesizerUsage
                    = new ChangeUsageEventTypeData(@event.EventData).NewValue;
                planet.ClearCache();
                Debug.WriteLine(
                    $"{DateStamp}: Elementium Synthesizer usage changed on planet {planet.PlanetId}.");
            }
        }

        #endregion


        #region Do Produce Event Methods ----------------------------------------------------------

        private void Do_ProduceShipExplorer(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                User user = context.User.FirstOrDefault(u => u.UserId.Equals(planet.PlanetUserId));
                planet.UpdateRessources();
                context.Ship.Add(new Ship
                {
                    ShipUserId = planet.PlanetUserId,
                    ShipPlanetId = planetId,
                    ShipType = ShipTypeEnum.Explorer.ToString(),
                    ShipTactical = user.IsOffensive()
                        ? TacticalEnum.Offensive.ToString()
                        : TacticalEnum.Defensive.ToString(),
                    ShipWeaponSlots = user.IsOffensive() ? 2 : 1,
                    ShipArmorSlots = user.IsOffensive() ? 1 : 2,
                    ShipItemSlots = 2,
                    ShipStructurePoints = 50,
                    ShipDriveStrength = 1000,
                    ShipPowerConsumption = 1,
                    ShipStorageCapacity = 50,
                    ShipCpuPower = 9,
                    ShipAccuracy = user.IsOffensive() ? 45 : 40,
                    ShipShieldPower = user.IsOffensive() ? 20 : 25,
                    ShipAttackPower = user.IsOffensive() ? 40 : 35,
                    ShipArmour = user.IsOffensive() ? 10 : 15
                }); ;
            }
        }

        private void Do_ProduceShipCarrier(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                int planetId = @event.EventLocationId;
                Planet planet = context.Planet.FirstOrDefault(p => p.PlanetId.Equals(planetId));
                User user = context.User.FirstOrDefault(u => u.UserId.Equals(planet.PlanetUserId));
                planet.UpdateRessources();
                context.Ship.Add(new Ship
                {
                    ShipUserId = planet.PlanetUserId,
                    ShipPlanetId = planetId,
                    ShipType = ShipTypeEnum.Carrier.ToString(),
                    ShipTactical = user.IsOffensive()
                        ? TacticalEnum.Offensive.ToString()
                        : TacticalEnum.Defensive.ToString(),
                    ShipWeaponSlots = user.IsOffensive() ? 2 : 1,
                    ShipArmorSlots = user.IsOffensive() ? 2 : 3,
                    ShipItemSlots = 3,
                    ShipStructurePoints = 80,
                    ShipDriveStrength = 800,
                    ShipPowerConsumption = 2,
                    ShipStorageCapacity = 500,
                    ShipCpuPower = 8,
                    ShipAccuracy = user.IsOffensive() ? 40 : 35,
                    ShipShieldPower = user.IsOffensive() ? 20 : 25,
                    ShipAttackPower = user.IsOffensive() ? 64 : 56,
                    ShipArmour = user.IsOffensive() ? 16 : 24
                }); ;
            }
        }

        #endregion


        #region Do Fleet Event Methods ------------------------------------------------------------

        private void Do_Attack(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                // local variables                
                MoveEventTypeData attackMoveData = new MoveEventTypeData(@event.EventData);
                int storageCapacity = context.Ship
                    .Where(s => attackMoveData.MovingShipIds.Contains(s.ShipId))
                    .Sum(s => s.ShipStorageCapacity);
                Planet planetTarget = PlanetProvider
                    .GetPlanet(attackMoveData.ToLocationId, true, context);
                double
                    takeSize = (double)storageCapacity / 7,
                    stolenOre = 0,
                    stolenEnergy = 0,
                    stolenTools = 0,
                    stolenMana = 0,
                    stolenCrystals = 0,
                    stolenAntimatter = 0,
                    stolenElementium = 0;
                bool
                    hasRessourcesLeft = true,
                    hasStorageLeft = true;

                // attack target planet
                while (hasRessourcesLeft && hasStorageLeft)
                {
                    // steal ore
                    if (planetTarget.PlanetOre > takeSize)
                    {
                        stolenOre += takeSize;
                        planetTarget.PlanetOre -= takeSize;
                    }
                    else if (planetTarget.PlanetOre > 0)
                    {
                        stolenOre += planetTarget.PlanetOre;
                        planetTarget.PlanetOre = 0;
                    }

                    // steal energy
                    if (planetTarget.PlanetEnergy > takeSize)
                    {
                        stolenEnergy += takeSize;
                        planetTarget.PlanetEnergy -= takeSize;
                    }
                    else if (planetTarget.PlanetEnergy > 0)
                    {
                        stolenEnergy += planetTarget.PlanetEnergy;
                        planetTarget.PlanetEnergy = 0;
                    }

                    // steal tools
                    if (planetTarget.PlanetTools > takeSize)
                    {
                        stolenTools += takeSize;
                        planetTarget.PlanetTools -= takeSize;
                    }
                    else if (planetTarget.PlanetTools > 0)
                    {
                        stolenTools += planetTarget.PlanetTools;
                        planetTarget.PlanetTools = 0;
                    }

                    // steal mana
                    if (planetTarget.PlanetMana > takeSize)
                    {
                        stolenMana += takeSize;
                        planetTarget.PlanetMana -= takeSize;
                    }
                    else if (planetTarget.PlanetMana > 0)
                    {
                        stolenMana += planetTarget.PlanetMana;
                        planetTarget.PlanetMana = 0;
                    }

                    // steal crystals
                    if (planetTarget.PlanetCrystals > takeSize)
                    {
                        stolenCrystals += takeSize;
                        planetTarget.PlanetCrystals -= takeSize;
                    }
                    else if (planetTarget.PlanetCrystals > 0)
                    {
                        stolenCrystals += planetTarget.PlanetCrystals;
                        planetTarget.PlanetCrystals = 0;
                    }

                    // steal antimatter
                    if (planetTarget.PlanetAntimatter > takeSize)
                    {
                        stolenAntimatter += takeSize;
                        planetTarget.PlanetAntimatter -= takeSize;
                    }
                    else if (planetTarget.PlanetAntimatter > 0)
                    {
                        stolenAntimatter += planetTarget.PlanetAntimatter;
                        planetTarget.PlanetAntimatter = 0;
                    }

                    // steal elementium
                    if (planetTarget.PlanetElementium > takeSize)
                    {
                        stolenElementium += takeSize;
                        planetTarget.PlanetElementium -= takeSize;
                    }
                    else if (planetTarget.PlanetElementium > 0)
                    {
                        stolenElementium += planetTarget.PlanetElementium;
                        planetTarget.PlanetElementium = 0;
                    }

                    // break if nothing left
                    if (planetTarget.PlanetOre
                        + planetTarget.PlanetEnergy
                        + planetTarget.PlanetTools
                        + planetTarget.PlanetMana
                        + planetTarget.PlanetCrystals
                        + planetTarget.PlanetAntimatter
                        + planetTarget.PlanetElementium
                        == 0)
                    {
                        hasRessourcesLeft = false;
                    }

                    // break if storage is full
                    double storageLeft = storageCapacity
                        - stolenOre
                        - stolenEnergy
                        - stolenTools
                        - stolenMana
                        - stolenCrystals
                        - stolenAntimatter
                        - stolenElementium;
                    if (storageLeft <= 1) // (rounding issue fix)
                    {
                        hasStorageLeft = false;
                    }

                    // set new take size and continue stealing
                    takeSize = storageLeft / 7;
                }

                // enqueue return event
                MoveEventTypeData data = new MoveEventTypeData(
                    attackMoveData.ToLocation, attackMoveData.ToLocationId, // from
                    attackMoveData.FromLocation, attackMoveData.FromLocationId, // to
                    attackMoveData.MovingShipIds,
                    stolenOre,
                    stolenEnergy,
                    stolenTools,
                    stolenMana,
                    stolenCrystals,
                    stolenAntimatter,
                    stolenElementium);
                Event returnEvent = new ReturnEvent(
                    EventLocationEnum.Planet, @event.EventLocationId,
                    100,
                    data);
                Instance.EventQueue.Enqueue(returnEvent);
            }
        }

        private void Do_Return(Event @event, GalaxxisContext context)
        {
            if (@event.EventLocation.Trim() == EventLocationEnum.Planet.ToString())
            {
                // local variables
                MoveEventTypeData data = new MoveEventTypeData(@event.EventData);
                Planet planet = PlanetProvider.GetPlanet(data.ToLocationId, false, context);

                // add ressources
                planet.PlanetOre += data.Ore;
                planet.PlanetEnergy += data.Energy;
                planet.PlanetTools += data.Tools;
                planet.PlanetMana += data.Mana;
                planet.PlanetCrystals += data.Crystals;
                planet.PlanetAntimatter += data.Antimatter;
                planet.PlanetElementium += data.Elementium;
            }
        }

        #endregion
    }
}
