using System;
using System.Collections.Generic;
using System.Text;
using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Classes.Buildings.RessourceBuildings
{
    public class Factory : BaseRessourceBuilding
    {
        #region Properties ------------------------------------------------------------------------

        public override GrowthRate RessourceGrowthRate 
            => _ressourceGrowthRate
            ??= new GrowthRate(200, Constants.FACTOR_RESSOURCE_GROWTH, _planet.PlanetFactory, 50);

        public override GrowthRate BuildTimeRate 
            => _buildTimeRate
            ??= new GrowthRate(5, Constants.FACTOR_BUILD_TIME, _planet.PlanetFactory + 1);

        public override GrowthRate BuildCostOreRate 
            => _buildCostOreRate
            ??= new GrowthRate(10, Constants.FACTOR_BUILD_COST, _planet.PlanetFactory + 1);

        public override GrowthRate BuildCostManaRate
            => _buildCostManaRate
            ??= new GrowthRate(5, Constants.FACTOR_BUILD_COST, _planet.PlanetFactory + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(10, Constants.FACTOR_BUILD_COST, _planet.PlanetFactory + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(0, Constants.FACTOR_BUILD_COST, _planet.PlanetFactory + 1);

        public override double RessourceCostsPerTick => 0;

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public Factory(Planet planet) : base(planet)
        { }

        #endregion


        #region Methods ---------------------------------------------------------------------------

        public override bool Build(out string info)
        {
            return base.Build(EventNameEnum.BuildFactory, out info);
        }

        #endregion
    }
}
