using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Classes.Buildings.PlanetBuildings;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Extensions.PlanetExtensions
{
    public static class MainPlanetExtensions
    {
        #region Growth rates ----------------------------------------------------------------------

        internal static GrowthRate GetGrowthRateTools(this Planet planet)
        {
            return new GrowthRate(200, 1, planet.PlanetFactory, 50);
        }

        internal static GrowthRate GetGrowthRateAntimatter(this Planet planet)
        {
            return new GrowthRate(175, 1, planet.PlanetNeutronAccelerator, 30);
        }

        internal static GrowthRate GetGrowthRateCrystals(this Planet planet)
        {
            return new GrowthRate(175, 1, planet.PlanetCrystalMine, 30);
        }

        internal static GrowthRate GetGrowthRateElementium(this Planet planet)
        {
            return new GrowthRate(75, 1, planet.PlanetElementiumSynthesizer, 10);
        }

        #endregion


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

                // init oreMine
                OreMine oreMine = new OreMine(planet);

                // set calculated stock
                double stockOre = planet.PlanetOre + oreMine.AmountPerTick;
                if (stockOre >= 0)
                {
                    planet.PlanetOre = stockOre;
                }

                #endregion

                #region Energy --------------------------------------------------------------------

                // add energy income
                double amountEnergy = planet.GetIncomeEnergyPerTick();

                // subscruct production costs
                amountEnergy -= planet.GetCostsEnergyPerTick();

                // set calculated stock
                double stockEnergy = planet.PlanetEnergy + amountEnergy;
                if (stockEnergy >= 0)
                {
                    planet.PlanetEnergy = stockEnergy;
                }

                #endregion

                #region Tools ---------------------------------------------------------------------

                // add tools income
                double amountTools = planet.GetIncomeToolsPerTick();

                // subscruct production costs
                amountTools -= planet.GetCostsToolsPerTick();

                // set calculated stock
                double stockTools = planet.PlanetTools + amountTools;
                if (stockTools >= 0)
                {
                    planet.PlanetTools = stockTools;
                }
                #endregion

                #region Mana ----------------------------------------------------------------------

                // init mageAcademy
                MageAcademy mageAcademy = new MageAcademy(planet);

                // set calculated stock
                double stockMana = planet.PlanetMana + mageAcademy.AmountPerTick;
                if (stockMana >= 0)
                {
                    planet.PlanetMana = stockMana;
                }

                #endregion

                // increase data tick
                planet.PlanetDataTick++;
            }
        }


        #region Tools -----------------------------------------------------------------------------

        public static double GetIncomeToolsPerDay(this Planet planet)
        {
            GrowthRate growthRateTools = planet.GetGrowthRateTools();
            return growthRateTools.Value + growthRateTools.BaseIncome;
        }

        public static double GetIncomeToolsPerTick(this Planet planet)
        {
            GrowthRate growthRateTools = planet.GetGrowthRateTools();
            return growthRateTools.GetValuePerTick() + growthRateTools.GetBaseIncomePerTick();
        }

        /// <summary>
        /// Amount of tools decreased each tick due to consuption in production
        /// </summary>
        public static double GetCostsToolsPerTick(this Planet planet)
        {
            // tools are consumed if producing items in item producing buildings
            // currently items are not implemented
            // TODO: as soon as items are implemented, add logic here
            return 0;
        }

        #endregion
    }
}
