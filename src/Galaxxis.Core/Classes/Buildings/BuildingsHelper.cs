using Galaxxis.Core.Engine;
using Galaxxis.Core.Provider;
using Galaxxis.Core.Static;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Events.BuildEvents;
using Galaxxis.Data.EventTypeData;
using Galaxxis.Data.Models;
using System;
using System.Linq;

namespace Galaxxis.Core.Classes.Buildings
{
    internal static class BuildingsHelper
    {
        /// <summary>
        /// Check if a build is possible and enqueue a build event
        /// </summary>
        /// <param name="planet">The planet where something should be build</param>
        /// <param name="buildCosts">0:ore, 1:mana, 2:crystals, 3:elementium</param>
        /// <param name="ticksToEventTick">Ticks until the build event is processed (build finished)</param>
        /// <param name="info">Feedback to user</param>
        public static bool BuildOnPlanet<TBuildEvent>(
            Planet planet, double[] buildCosts, long ticksToEventTick, out string info) 
            where TBuildEvent : BaseBuildEvent
        {
            // log build attempt
            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.ToString(Constants.DEBUG_TIME_FORMAT)}: "
                + $"Attempt to build on planet {planet.PlanetName} started.");

            // check conditions
            if (EventProvider
                    .GetEvents(planet.PlanetId, EventLocationEnum.Planet, EventTypeEnum.Build)
                    .Count != 0
                || Processor.Instance.EventQueue
                    .Any(e => e.EventLocation == EventLocationEnum.Planet.ToString()
                           && e.EventLocationId == planet.PlanetId
                           && e.EventType == EventTypeEnum.Build.ToString()))
            {
                info = "A build is still in progress!";
                return false;
            }
            if (buildCosts[0] > planet.PlanetOre)
            {
                info = "Not enough ore!";
                return false;
            }
            if (buildCosts[1] > planet.PlanetMana)
            {
                info = "Not enough mana!";
                return false;
            }
            if (buildCosts[2] > planet.PlanetCrystals)
            {
                info = "Not enough crystals!";
                return false;
            }
            if (buildCosts[3] > planet.PlanetElementium)
            {
                info = "Not enough elementium!";
                return false;
            }

            // get event
            BuildEventTypeData data = new (
                buildCosts[0],
                buildCosts[1],
                buildCosts[2],
                buildCosts[3]);
            int id = planet.PlanetId;
            TBuildEvent @event = (TBuildEvent)Activator.CreateInstance(
                typeof(TBuildEvent),
                [EventLocationEnum.Planet, id, ticksToEventTick, data]);

            // queue event
            if (@event != null)
            {
                Processor.Instance.EventQueue.Enqueue(@event);
                System.Diagnostics.Debug.WriteLine(
                    $"{DateTime.Now.ToString(Constants.DEBUG_TIME_FORMAT)}: "
                    + $"Build on planet {planet.PlanetName} enqueued.");
            }

            // exit
            info = "Construction of the next building level has been started.";
            return true;
        }
    }
}
