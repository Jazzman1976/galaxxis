using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;

namespace Galaxxis.Core.Classes.Buildings.RessourceBuildings
{
    public class MageAcademy : BaseRessourceBuilding
    {
        #region Properties ------------------------------------------------------------------------

        public override GrowthRate RessourceGrowthRate
            => _ressourceGrowthRate 
            ??= new GrowthRate(
                    Constants.BASEVALUE_CAT2_GROWTH, 
                    Constants.FACTOR_RESSOURCE_GROWTH, 
                    _planet.PlanetMageAcademy, 
                    Constants.BASEINCOME_CAT2_GROWTH);

        public override GrowthRate BuildTimeRate 
            => _buildTimeRate
            ??= new GrowthRate(
                Constants.BASEVALUE_BUILD_TIME, 
                Constants.FACTOR_BUILD_TIME, 
                _planet.PlanetMageAcademy + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(
                    15, Constants.FACTOR_BUILD_COST, _planet.PlanetMageAcademy + 1);

        public override GrowthRate BuildCostManaRate
            => _buildCostManaRate
            ??= new GrowthRate(
                    10, Constants.FACTOR_BUILD_COST, _planet.PlanetMageAcademy + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(
                    15, Constants.FACTOR_BUILD_COST, _planet.PlanetMageAcademy + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(
                    5, Constants.FACTOR_BUILD_COST, _planet.PlanetMageAcademy + 1);

        public override double RessourceCostsPerTick
            => _costsPerTick > 0 ? _costsPerTick : (_costsPerTick = GetCostsPerTick(new int[]
            {
                5, 5, 0,
                5, 10, 15,
                15
            }));

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public MageAcademy(Planet planet) : base(planet)
        { }

        #endregion


        #region Methods ---------------------------------------------------------------------------

        public override bool Build(out string info)
        {
            return base.Build(EventNameEnum.BuildMageAcademy, out info);
        }

        public override double ApplyRaceFactor(double value)
        {
            switch (Enum.Parse(typeof(UserRacesEnum), _owner.UserRace))
            {
                case UserRacesEnum.Elf:
                case UserRacesEnum.Goblin:
                    value *= 1.5;
                    break;

                case UserRacesEnum.Troll:
                case UserRacesEnum.Ork:
                    value *= 0.75;
                    break;
            }
            return value;
        }

        #endregion
    }
}
