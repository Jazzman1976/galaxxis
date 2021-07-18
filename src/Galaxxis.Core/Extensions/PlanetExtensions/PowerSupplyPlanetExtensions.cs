using Galaxxis.Core.Engine;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Extensions.PlanetExtensions
{
    public static class PowerSupplyPlanetExtensions
    {
        internal static GrowthRate GetGrowthRateEnergy(this Planet planet)
        {
            return new GrowthRate(200, 1, planet.PlanetPowerSupply, 50);
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

        /// <summary>
        /// Amount of energy decreased each tick due to consuption in production
        /// </summary>
        public static double GetCostsEnergyPerTick(this Planet planet)
        {
            GrowthRate
                decreaseRateOreMine = new GrowthRate(5, 2, planet.PlanetOreMine),
                decreaseRateFactory = new GrowthRate(5, 2, planet.PlanetFactory),
                decreaseRateNeutronAccelerator
                    = new GrowthRate(10, 2, planet.PlanetNeutronAccelerator),
                decreaseRateCrystalMine = new GrowthRate(5, 2, planet.PlanetCrystalMine),
                decreaseRateMageAcademy = new GrowthRate(15, 2, planet.PlanetMageAcademy),
                decreaseRateElementiumSynthesizer
                    = new GrowthRate(15, 2, planet.PlanetElementiumSynthesizer);

            double energyCostPerTick
                = decreaseRateOreMine.GetValuePerTick()
                + decreaseRateFactory.GetValuePerTick()
                + decreaseRateNeutronAccelerator.GetValuePerTick()
                + decreaseRateCrystalMine.GetValuePerTick()
                + decreaseRateMageAcademy.GetValuePerTick()
                + decreaseRateElementiumSynthesizer.GetValuePerTick();

            return energyCostPerTick;
        }
    }
}
