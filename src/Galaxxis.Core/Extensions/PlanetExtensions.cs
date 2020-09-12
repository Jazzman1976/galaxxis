using Galaxxis.Core.Engine;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Extensions
{
    public static class PlanetExtensions
    {
        internal static void UpdateRessources(this Planet planet)
        {
            // ticks gone since last update
            long ticksGone 
                = planet.PlanetSystem.SystemGalaxy.GalaxyUniverse.UniverseTicks 
                - planet.PlanetDataTick;

            // init growthrates
            GrowthRate
                growthRateEnergy = new GrowthRate(200, 1, planet.PlanetPowerSupply, 50),
                growthRateOre = new GrowthRate(200, 1, planet.PlanetOreMine, 50),
                growthRateTools = new GrowthRate(200, 1, planet.PlanetFactory, 50),
                growthRateAntimatter = new GrowthRate(175, 1, planet.PlanetNeutronAccelerator, 30),
                growthRateCrystals = new GrowthRate(175, 1, planet.PlanetCrystalMine, 30),
                growthRateMana = new GrowthRate(175, 1, planet.PlanetMageAcademy, 30),
                growthRateElementium = new GrowthRate(75, 1, planet.PlanetElementiumSynthesizer, 10);

            // add base income regardless of level or crafting ressources on stock
            planet.PlanetEnergy += growthRateEnergy.GetBaseIncomeForTicks(ticksGone);
            planet.PlanetOre += growthRateOre.GetBaseIncomeForTicks(ticksGone);
            planet.PlanetTools += growthRateTools.GetBaseIncomeForTicks(ticksGone);
            planet.PlanetAntimatter += growthRateAntimatter.GetBaseIncomeForTicks(ticksGone);
            planet.PlanetCrystals += growthRateCrystals.GetBaseIncomeForTicks(ticksGone);
            planet.PlanetMana += growthRateMana.GetBaseIncomeForTicks(ticksGone);
            planet.PlanetElementium += growthRateElementium.GetBaseIncomeForTicks(ticksGone);

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
        }
    }
}
