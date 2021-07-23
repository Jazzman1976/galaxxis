using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Classes.Buildings.PlanetBuildings;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Extensions.PlanetExtensions
{
    public static class MainPlanetExtensions
    {
        /// <summary>
        /// Returns a new OreMine instance
        /// </summary>
        public static OreMine GetOreMine(this Planet planet)
        {
            return new OreMine(planet);
        }

        /// <summary>
        /// Returns a new PowerSupply instance
        /// </summary>
        public static PowerSupply GetPowerSupply(this Planet planet)
        {
            return new PowerSupply(planet);
        }

        /// <summary>
        /// Returns a new Factory instance
        /// </summary>
        public static Factory GetFactory(this Planet planet)
        {
            return new Factory(planet);
        }

        /// <summary>
        /// Returns a new MageAcademy instance
        /// </summary>
        public static MageAcademy GetMageAcademy(this Planet planet)
        {
            return new MageAcademy(planet);
        }

        /// <summary>
        /// Get amount of ticks gone since last UpdateRessources
        /// </summary>
        public static long GetTicksGone(this Planet planet)
        {
            Planet thePlanet = PlanetProvider.GetPlanet(planet.PlanetId);
            long ticksGone
                = thePlanet.PlanetSystem.SystemGalaxy.GalaxyUniverse.UniverseTicks
                - thePlanet.PlanetDataTick;
            return ticksGone;
        }

        /// <summary>
        /// Calculates all ressources on the planet depending on
        /// income and consumption. Does not store calculated planet to db.
        /// </summary>
        public static void UpdateRessources(this Planet planet)
        {
            // ticks gone since last update
            long ticksGone = planet.GetTicksGone();

            // for each tick process growth of ressources
            for (long tickstart = ticksGone; tickstart > 0; tickstart--)
            {
                #region Ore -----------------------------------------------------------------------

                // set calculated stock
                double stockOre = planet.PlanetOre + planet.GetOreMine().AmountPerTick;
                if (stockOre >= 0)
                {
                    planet.PlanetOre = stockOre;
                }

                #endregion

                #region Energy --------------------------------------------------------------------

                // set calculated stock
                double stockEnergy = planet.PlanetEnergy + planet.GetPowerSupply().AmountPerTick;
                if (stockEnergy >= 0)
                {
                    planet.PlanetEnergy = stockEnergy;
                }

                #endregion

                #region Tools ---------------------------------------------------------------------

                // set calculated stock
                double stockTools = planet.PlanetTools + planet.GetFactory().AmountPerTick;
                if (stockTools >= 0)
                {
                    planet.PlanetTools = stockTools;
                }
                #endregion

                #region Mana ----------------------------------------------------------------------

                // set calculated stock
                double stockMana = planet.PlanetMana + planet.GetMageAcademy().AmountPerTick;
                if (stockMana >= 0)
                {
                    planet.PlanetMana = stockMana;
                }

                #endregion

                // increase data tick
                planet.PlanetDataTick++;
            }
        }
    }
}
