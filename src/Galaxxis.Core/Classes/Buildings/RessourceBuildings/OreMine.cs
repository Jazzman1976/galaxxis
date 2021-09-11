using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Classes.Buildings.Enums;
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
            ??= new GrowthRate(
                    Constants.BASEVALUE_CAT1_RESSOURCE, 
                    Constants.FACTOR_RESSOURCES, 
                    _planet.PlanetOreMine, 
                    Constants.BASEINCOME_CAT1_RESSOURCE);

        public override GrowthRate BuildTimeRate 
            => _buildTimeRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_MUCH, 
                    Constants.FACTOR_BUILD_TIME, 
                    _planet.PlanetOreMine + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetOreMine + 1);

        public override GrowthRate BuildCostManaRate
            => _buildCostManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetOreMine + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetOreMine + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(
                    0, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetOreMine + 1);

        public override double RessourceCostsPerTick => 0;

        public override double Usage => _planet.PlanetOreMineUsage;

        public override GrowthRate ConsumptionEnergyRate
            => _consumptionEnergyRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetOreMine);

        public override GrowthRate ConsumptionManaRate
            => _consumptionManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetOreMine);

        public override GrowthRate ConsupmtionAntimatter
            => _consupmtionAntimatter
            ??= new GrowthRate(
                    0,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetOreMine);

        public override GrowthRate ConsumptionElementium
            => _consumptionElementium
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetOreMine);

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
        {
            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.ToString(_timeFormat)}: "
                + "A new Ore Mine object has been created.");
        }

        #endregion
    }
}
