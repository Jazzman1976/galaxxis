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
            ??= new GrowthRate(
                    200, Constants.FACTOR_RESSOURCE_GROWTH, _planet.PlanetPowerSupply, 50);

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

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(0, Constants.FACTOR_BUILD_COST, _planet.PlanetPowerSupply + 1);

        public override bool Build(out string info)
        {
            return base.Build(EventNameEnum.BuildPowerSupply, out info);
        }

        public override double CostsPerTick
            => _costsPerTick > 0 ? _costsPerTick : (_costsPerTick = GetCostsPerTick(new int[] 
            {
                5, 0, 5, 
                15, 5, 10,
                15
            }));

        #region Constructors ----------------------------------------------------------------------

        public PowerSupply(Planet planet) : base(planet)
        { }

        #endregion
    }
}
