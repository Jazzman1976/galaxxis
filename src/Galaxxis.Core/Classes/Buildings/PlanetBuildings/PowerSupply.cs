using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Classes.Buildings.PlanetBuildings
{
    public class PowerSupply : BasePlanetRessourceBuilding
    {
        public override GrowthRate RessourceGrowthRate 
            => _ressourceGrowthRate
            ??= new GrowthRate(200, 1, _planet.PlanetPowerSupply, 50);

        public override GrowthRate BuildTimeRate 
            => _buildTimeRate
            ??= new GrowthRate(5, Constants.FACTOR_BUILD_TIME, _planet.PlanetPowerSupply + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(15, Constants.FACTOR_BUILD_COST, _planet.PlanetPowerSupply + 1);

        public override GrowthRate BuildCostManaRate 
            => _buildCostManaRate
            ??= new GrowthRate(5, Constants.FACTOR_BUILD_COST, _planet.PlanetPowerSupply + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(5, Constants.FACTOR_BUILD_COST, _planet.PlanetPowerSupply + 1);

        public override bool Build(out string info)
        {
            return base.Build(EventNameEnum.BuildPowerSupply, out info);
        }

        private double _costsPerTick = -1;
        private double GetCostsPerTick()
        {
            GrowthRate
                decreaseRateOreMine = new GrowthRate(5, 2, _planet.PlanetOreMine),
                decreaseRateFactory = new GrowthRate(5, 2, _planet.PlanetFactory),
                decreaseRateNeutronAccelerator
                    = new GrowthRate(10, 2, _planet.PlanetNeutronAccelerator),
                decreaseRateCrystalMine = new GrowthRate(5, 2, _planet.PlanetCrystalMine),
                decreaseRateMageAcademy = new GrowthRate(15, 2, _planet.PlanetMageAcademy),
                decreaseRateElementiumSynthesizer
                    = new GrowthRate(15, 2, _planet.PlanetElementiumSynthesizer);

            double energyCostPerTick
                = decreaseRateOreMine.GetValuePerTick()
                + decreaseRateFactory.GetValuePerTick()
                + decreaseRateNeutronAccelerator.GetValuePerTick()
                + decreaseRateCrystalMine.GetValuePerTick()
                + decreaseRateMageAcademy.GetValuePerTick()
                + decreaseRateElementiumSynthesizer.GetValuePerTick();

            return energyCostPerTick;
        }
        public override double CostsPerTick
            => _costsPerTick > 0
                ? _costsPerTick
                : (_costsPerTick = GetCostsPerTick());

        #region Constructors ----------------------------------------------------------------------

        public PowerSupply(Planet planet) : base(planet)
        { }

        #endregion
    }
}
