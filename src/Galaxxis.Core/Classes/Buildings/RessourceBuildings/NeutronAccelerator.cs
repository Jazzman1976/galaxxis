using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Classes.Buildings.RessourceBuildings
{
    public class NeutronAccelerator : BaseRessourceBuilding
    {
        #region Properties ------------------------------------------------------------------------

        public override GrowthRate RessourceGrowthRate
            => _ressourceGrowthRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_CAT2_GROWTH,
                    Constants.FACTOR_RESSOURCE_GROWTH,
                    _planet.PlanetNeutronAccelerator,
                    Constants.BASEINCOME_CAT2_GROWTH);

        public override GrowthRate BuildTimeRate
            => _buildTimeRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_BUILD_TIME,
                    Constants.FACTOR_BUILD_TIME,
                    _planet.PlanetNeutronAccelerator + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(
                    20, Constants.FACTOR_BUILD_COST, _planet.PlanetNeutronAccelerator + 1);

        public override GrowthRate BuildCostManaRate
            => _buildCostManaRate
            ??= new GrowthRate(
                    10, Constants.FACTOR_BUILD_COST, _planet.PlanetNeutronAccelerator + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(
                    10, Constants.FACTOR_BUILD_COST, _planet.PlanetNeutronAccelerator + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(
                    5, Constants.FACTOR_BUILD_COST, _planet.PlanetNeutronAccelerator + 1);

        public override double RessourceCostsPerTick
            => _costsPerTick > 0 ? _costsPerTick : (_costsPerTick = GetCostsPerTick(new int[]
            {
                0, 5, 5,
                10, 15, 5,
                15
            }));

        #endregion


        #region Methods ---------------------------------------------------------------------------

        public override bool Build(out string info)
        {
            return base.Build(EventNameEnum.BuildNeutronAccelerator, out info);
        }

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public NeutronAccelerator(Planet planet) : base(planet)
        { }

        #endregion
    }
}
