using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;

namespace Galaxxis.Core.Classes.Buildings.RessourceBuildings
{
    public class OreMine : BaseRessourceBuilding
    {
        #region Properties ------------------------------------------------------------------------

        public override GrowthRate RessourceGrowthRate
            => _ressourceGrowthRate
            ??= new GrowthRate(200, Constants.FACTOR_RESSOURCE_GROWTH, _planet.PlanetOreMine, 50);

        public override GrowthRate BuildTimeRate 
            => _buildTimeRate
            ??= new GrowthRate(5, Constants.FACTOR_BUILD_TIME, _planet.PlanetOreMine + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(10, Constants.FACTOR_BUILD_COST, _planet.PlanetOreMine + 1);

        public override GrowthRate BuildCostManaRate
            => _buildCostManaRate
            ??= new GrowthRate(10, Constants.FACTOR_BUILD_COST, _planet.PlanetOreMine + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(5, Constants.FACTOR_BUILD_COST, _planet.PlanetOreMine + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(0, Constants.FACTOR_BUILD_COST, _planet.PlanetOreMine + 1);

        public override double RessourceCostsPerTick => 0;

        public override double Usage => _planet.PlanetOreMineUsage;

        #endregion


        #region Methods ---------------------------------------------------------------------------

        public override bool Build(out string info)
        {
            return base.Build(EventNameEnum.BuildOreMine, out info);
        }

        public override double ApplyRaceFactor(double value)
        {
            switch (Enum.Parse(typeof(UserRacesEnum), _owner.UserRace))
            {
                case UserRacesEnum.Dwarf:
                case UserRacesEnum.Goblin:
                    value *= 1.5;
                    break;
            }
            return value;
        }

        public override bool ChangeUsage(double newValue, out string info)
        {
            return base.ChangeUsage(EventNameEnum.ChangeOreMineUsage, newValue, out info);
        }

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public OreMine(Planet planet) : base(planet)
        { }

        #endregion
    }
}
