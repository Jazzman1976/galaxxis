using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Classes.Buildings.RessourceBuildings
{
    public class ElementiumSynthesizer : BaseRessourceBuilding
    {
        #region Properties ------------------------------------------------------------------------

        public override GrowthRate RessourceGrowthRate
            => _ressourceGrowthRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_CAT3_GROWTH,
                    Constants.FACTOR_RESSOURCE_GROWTH,
                    _planet.PlanetElementiumSynthesizer,
                    Constants.BASEINCOME_CAT3_GROWTH);

        public override GrowthRate BuildTimeRate
            => _buildTimeRate
            ??= new GrowthRate(
                Constants.BASEVALUE_BUILD_TIME,
                Constants.FACTOR_BUILD_TIME,
                _planet.PlanetElementiumSynthesizer + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(
                    25, Constants.FACTOR_BUILD_COST, _planet.PlanetElementiumSynthesizer + 1);

        public override GrowthRate BuildCostManaRate
            => _buildCostManaRate
            ??= new GrowthRate(
                    20, Constants.FACTOR_BUILD_COST, _planet.PlanetElementiumSynthesizer + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(
                    20, Constants.FACTOR_BUILD_COST, _planet.PlanetElementiumSynthesizer + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(
                    10, Constants.FACTOR_BUILD_COST, _planet.PlanetElementiumSynthesizer + 1);

        public override double RessourceCostsPerTick
            => _costsPerTick > 0 ? _costsPerTick : (_costsPerTick = GetCostsPerTick(new int[]
            {
                5, 5, 5,
                5, 5, 5,
                0
            }));

        public override double Usage => _planet.PlanetElementiumSynthesizerUsage;

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public ElementiumSynthesizer(Planet planet) : base(planet)
        { }

        #endregion


        #region Methods ---------------------------------------------------------------------------

        public override bool Build(out string info)
        {
            return base.Build(EventNameEnum.BuildElementiumSynthesizer, out info);
        }

        public override double ApplyRaceFactor(double value)
        {
            return value;
        }

        public override bool ChangeUsage(double newValue, out string info)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
