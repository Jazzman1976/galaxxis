using Galaxxis.Core.Engine;
using Galaxxis.Core.Provider;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Events;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;
using System;
using System.Linq;

namespace Galaxxis.Core.Actions
{
    internal class FleetCommandAttackAction : BaseAction
    {
        public override EventNameEnum TriggerEvent => EventNameEnum.FleetCommandAttack;

        public override bool DoAction(Event @event, GalaxxisContext context, out string feedback)
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
                Event returnEvent = new FleetCommandMoveEvent(
                    EventLocationEnum.Planet, @event.EventLocationId,
                    100,
                    data);
                Processor.Instance.EventQueue.Enqueue(returnEvent);

                feedback = 
                    $"{DateStamp}: Fleet Command Attack: planet {planetTarget.PlanetId} attacked.";
                return true;
            }

            feedback = $"Fleet Command Attack: no conditions for any change found.";
            return false;
        }
    }
}
