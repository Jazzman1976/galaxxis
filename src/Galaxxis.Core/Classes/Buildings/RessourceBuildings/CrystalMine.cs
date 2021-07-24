using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;

namespace Galaxxis.Core.Classes.Buildings.RessourceBuildings
{
    public class CrystalMine : BaseRessourceBuilding
    {
        #region Properties ------------------------------------------------------------------------

        public override GrowthRate RessourceGrowthRate
            => _ressourceGrowthRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_CAT2_GROWTH, 
                    Constants.FACTOR_RESSOURCE_GROWTH, 
                    _planet.PlanetCrystalMine, 
                    Constants.BASEINCOME_CAT2_GROWTH);

        public override GrowthRate BuildTimeRate
            => _buildTimeRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_BUILD_TIME,
                    Constants.FACTOR_BUILD_TIME,
                    _planet.PlanetCrystalMine + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(15, Constants.FACTOR_BUILD_COST, _planet.PlanetCrystalMine + 1);


        public override GrowthRate BuildCostManaRate 
            => _buildCostManaRate
            ??= new GrowthRate(15, Constants.FACTOR_BUILD_COST, _planet.PlanetCrystalMine + 1);

        public override GrowthRate BuildCostCrystalsRate 
            => _buildCostCrystalsRate
            ??= new GrowthRate(10, Constants.FACTOR_BUILD_COST, _planet.PlanetCrystalMine + 1);

        public override GrowthRate BuildCostElementiumRate 
            => _buildCostElementiumRate
            ??= new GrowthRate(5, Constants.FACTOR_BUILD_COST, _planet.PlanetCrystalMine + 1);

        public override double RessourceCostsPerTick => 0;

        public override double Usage => _planet.PlanetCrystalMineUsage;

        #endregion


        #region Methods ---------------------------------------------------------------------------

        public override bool Build(out string info)
        {
            return base.Build(EventNameEnum.BuildCrystalMine, out info);
        }

        public override double ApplyRaceFactor(double value)
        {
            switch (Enum.Parse(typeof(UserRacesEnum), _owner.UserRace))
            {
                case UserRacesEnum.Dwarf:
                case UserRacesEnum.Ork:
                    value *= 1.5;
                    break;

                case UserRacesEnum.Human:
                case UserRacesEnum.Elf:
                    value *= 0.75;
                    break;
            }
            return value;
        }

        public override bool ChangeUsage(double newValue, out string info)
        {
            return base.ChangeUsage(EventNameEnum.ChangeCrystalMineUsage, newValue, out info);
        }

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public CrystalMine(Planet planet) : base(planet)
        { }

        #endregion
    }
}
