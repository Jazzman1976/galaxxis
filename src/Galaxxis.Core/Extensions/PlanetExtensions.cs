using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Extensions
{
    public static class PlanetExtensions
    {
        #region Growth rates ----------------------------------------------------------------------

        internal static GrowthRate GetGrowthRateOre(this Planet planet)
        {
            return new GrowthRate(200, 1, planet.PlanetOreMine, 50);
        }

        internal static GrowthRate GetGrowthRateEnergy(this Planet planet)
        {
            return new GrowthRate(200, 1, planet.PlanetPowerSupply, 50);
        }

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

        internal static GrowthRate GetGrowthRateMana(this Planet planet)
        {
            return new GrowthRate(175, 1, planet.PlanetMageAcademy, 30);
        }

        internal static GrowthRate GetGrowthRateElementium(this Planet planet)
        {
            return new GrowthRate(75, 1, planet.PlanetElementiumSynthesizer, 10);
        }

        #endregion


        internal static long GetTicksGone(this Planet planet)
        {
            Planet thePlanet = PlanetProvider.GetPlanet(planet.PlanetId);
            long ticksGone
                = thePlanet.PlanetSystem.SystemGalaxy.GalaxyUniverse.UniverseTicks
                - thePlanet.PlanetDataTick;
            return ticksGone;
        }

        internal static void UpdateRessources(this Planet planet)
        {
            // ticks gone since last update
            long ticksGone = planet.GetTicksGone();

            // init growthrates
            GrowthRate
                growthRateEnergy = planet.GetGrowthRateEnergy(),
                growthRateOre = planet.GetGrowthRateOre(),
                growthRateTools = planet.GetGrowthRateTools(),
                growthRateAntimatter = planet.GetGrowthRateAntimatter(),
                growthRateCrystals = planet.GetGrowthRateCrystals(),
                growthRateMana = planet.GetGrowthRateMana(),
                growthRateElementium = planet.GetGrowthRateElementium();

            // add base income regardless of level or crafting ressources on stock
            planet.PlanetEnergy += growthRateEnergy.GetBaseIncomeForTicks(ticksGone);
            planet.PlanetOre += growthRateOre.GetBaseIncomeForTicks(ticksGone);
            planet.PlanetTools += growthRateTools.GetBaseIncomeForTicks(ticksGone);
            planet.PlanetAntimatter += growthRateAntimatter.GetBaseIncomeForTicks(ticksGone);
            planet.PlanetCrystals += growthRateCrystals.GetBaseIncomeForTicks(ticksGone);
            planet.PlanetMana += growthRateMana.GetBaseIncomeForTicks(ticksGone);
            planet.PlanetElementium += growthRateElementium.GetBaseIncomeForTicks(ticksGone);

            // add ore income
            {
                GrowthRate
                    consumptionEnergy = new GrowthRate(5, 2, planet.PlanetOreMine),
                    consumptionMana = new GrowthRate(5, 2, planet.PlanetOreMine),
                    consumptionAntimatter = new GrowthRate(0, 2, planet.PlanetOreMine),
                    consumptionElementium = new GrowthRate(5, 2, planet.PlanetOreMine);

                double
                    costsEnergy = consumptionEnergy.GetValuePerTick(),
                    costsMana = consumptionMana.GetValuePerTick(),
                    costsAntimatter = consumptionAntimatter.GetValuePerTick(),
                    costsElementium = consumptionElementium.GetValuePerTick();

                // add income as long as consumption costs could be paid for all ticks gone
                for (long tickstart = ticksGone; tickstart > 0; tickstart--)
                {
                    bool hasEnergy = planet.PlanetEnergy - costsEnergy > 0;
                    bool hasMana = planet.PlanetMana - costsMana > 0;
                    bool hasAntimatter = planet.PlanetAntimatter - costsAntimatter > 0;
                    bool hasElementium = planet.PlanetElementium - costsElementium > 0;

                    if (hasEnergy && hasMana && hasAntimatter && hasElementium)
                    {
                        // reduce production costs
                        planet.PlanetEnergy -= costsEnergy;
                        planet.PlanetMana -= costsMana;
                        planet.PlanetAntimatter -= costsAntimatter;
                        planet.PlanetElementium -= costsElementium;

                        // add income
                        planet.PlanetOre += growthRateOre.GetValuePerTick();
                    }
                }
            }

            // add energy income
            {
                GrowthRate
                    consumptionEnergy = new GrowthRate(0, 2, planet.PlanetPowerSupply),
                    consumptionMana = new GrowthRate(5, 2, planet.PlanetPowerSupply),
                    consumptionAntimatter = new GrowthRate(5, 2, planet.PlanetPowerSupply),
                    consumptionElementium = new GrowthRate(5, 2, planet.PlanetPowerSupply);

                double
                    costsEnergy = consumptionEnergy.GetValuePerTick(),
                    costsMana = consumptionMana.GetValuePerTick(),
                    costsAntimatter = consumptionAntimatter.GetValuePerTick(),
                    costsElementium = consumptionElementium.GetValuePerTick();

                // add income as long as consumption costs could be paid for all ticks gone
                for (long tickstart = ticksGone; tickstart > 0; tickstart--)
                {
                    bool hasEnergy = planet.PlanetEnergy - costsEnergy > 0;
                    bool hasMana = planet.PlanetMana - costsMana > 0;
                    bool hasAntimatter = planet.PlanetAntimatter - costsAntimatter > 0;
                    bool hasElementium = planet.PlanetElementium - costsElementium > 0;

                    if (hasEnergy && hasMana && hasAntimatter && hasElementium)
                    {
                        // reduce production costs
                        planet.PlanetEnergy -= costsEnergy;
                        planet.PlanetMana -= costsMana;
                        planet.PlanetAntimatter -= costsAntimatter;
                        planet.PlanetElementium -= costsElementium;

                        // add income
                        planet.PlanetEnergy += growthRateEnergy.GetValuePerTick();
                    }
                }
            }

            // add tools income
            {
                GrowthRate
                    consumptionEnergy = new GrowthRate(5, 2, planet.PlanetFactory),
                    consumptionMana = new GrowthRate(0, 2, planet.PlanetFactory),
                    consumptionAntimatter = new GrowthRate(5, 2, planet.PlanetFactory),
                    consumptionElementium = new GrowthRate(5, 2, planet.PlanetFactory);

                double
                    costsEnergy = consumptionEnergy.GetValuePerTick(),
                    costsMana = consumptionMana.GetValuePerTick(),
                    costsAntimatter = consumptionAntimatter.GetValuePerTick(),
                    costsElementium = consumptionElementium.GetValuePerTick();

                // add income as long as consumption costs could be paid for all ticks gone
                for (long tickstart = ticksGone; tickstart > 0; tickstart--)
                {
                    bool hasEnergy = planet.PlanetEnergy - costsEnergy > 0;
                    bool hasMana = planet.PlanetMana - costsMana > 0;
                    bool hasAntimatter = planet.PlanetAntimatter - costsAntimatter > 0;
                    bool hasElementium = planet.PlanetElementium - costsElementium > 0;

                    if (hasEnergy && hasMana && hasAntimatter && hasElementium)
                    {
                        // reduce production costs
                        planet.PlanetEnergy -= costsEnergy;
                        planet.PlanetMana -= costsMana;
                        planet.PlanetAntimatter -= costsAntimatter;
                        planet.PlanetElementium -= costsElementium;

                        // add income
                        planet.PlanetTools += growthRateTools.GetValuePerTick();
                    }
                }
            }

            // update data tick
            planet.PlanetDataTick = PlanetProvider
                .GetPlanet(planet.PlanetId)
                .PlanetSystem
                .SystemGalaxy
                .GalaxyUniverse
                .UniverseTicks;
        }


        #region Ore -------------------------------------------------------------------------------

        /// <summary>
        /// Current ore in stock
        /// </summary>
        public static double GetStockOre(this Planet planet)
        {
            long ticksGone = planet.GetTicksGone();
            GrowthRate growthRateOre = planet.GetGrowthRateOre();
            return planet.PlanetOre
                + growthRateOre.GetBaseIncomeForTicks(ticksGone)
                + growthRateOre.GetValueForTicks(ticksGone);
        }

        /// <summary>
        /// Income in 24 hours
        /// </summary>
        public static double GetIncomeOrePerDay(this Planet planet)
        {
            // init growth rate
            GrowthRate growthRateOre = planet.GetGrowthRateOre();
            return growthRateOre.Value + growthRateOre.BaseIncome;
        }

        public static double GetIncomeOrePerTick(this Planet planet)
        {
            // init growth rate
            GrowthRate growthRateOre = planet.GetGrowthRateOre();
            return growthRateOre.GetValuePerTick() + growthRateOre.GetBaseIncomePerTick();
        }

        #endregion


        #region Energy ----------------------------------------------------------------------------

        public static double GetStockEnergy(this Planet planet)
        {
            long ticksGone = planet.GetTicksGone();
            GrowthRate growthRateEnergy = planet.GetGrowthRateEnergy();
            return planet.PlanetEnergy
                + growthRateEnergy.GetBaseIncomeForTicks(ticksGone)
                + growthRateEnergy.GetValueForTicks(ticksGone);
        }

        public static double GetIncomeEnergyPerDay(this Planet planet)
        {
            // init growth rate
            GrowthRate growthRateEnergy = planet.GetGrowthRateEnergy();
            return growthRateEnergy.Value + growthRateEnergy.BaseIncome;
        }

        public static double GetIncomeEnergyPerTick(this Planet planet)
        {
            // init growth rate
            GrowthRate growthRateEnergy = planet.GetGrowthRateEnergy();
            return growthRateEnergy.GetValuePerTick() + growthRateEnergy.GetBaseIncomePerTick();
        }

        #endregion

        #region Tools -----------------------------------------------------------------------------

        public static double GetStockTools(this Planet planet)
        {
            long ticksGone = planet.GetTicksGone();
            GrowthRate growthRateTools = planet.GetGrowthRateTools();
            return planet.PlanetTools
                + growthRateTools.GetBaseIncomeForTicks(ticksGone)
                + growthRateTools.GetValueForTicks(ticksGone);
        }

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

        #endregion
    }
}
