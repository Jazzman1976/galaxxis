using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Engine;
using Galaxxis.Core.Provider;
using Galaxxis.Core.Static;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Events.BuildEvents;
using Galaxxis.Data.Models;
using System;
using System.Linq;

namespace Galaxxis.Core.Classes.Buildings.RessourceBuildings
{
    public class CrystalMine : BaseRessourceBuilding<BuildCrystalMineEvent>
    {
        #region Properties ------------------------------------------------------------------------

        public override GrowthRate RessourceGrowthRate
            => _ressourceGrowthRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_CAT2_RESSOURCE, 
                    Constants.FACTOR_RESSOURCES, 
                    _planet.PlanetCrystalMine, 
                    Constants.BASEINCOME_CAT2_RESSOURCE);

        public override GrowthRate BuildTimeRate
            => _buildTimeRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_MUCH,
                    Constants.FACTOR_BUILD_TIME,
                    _planet.PlanetCrystalMine + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetCrystalMine + 1);


        public override GrowthRate BuildCostManaRate 
            => _buildCostManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetCrystalMine + 1);

        public override GrowthRate BuildCostCrystalsRate 
            => _buildCostCrystalsRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetCrystalMine + 1);

        public override GrowthRate BuildCostElementiumRate 
            => _buildCostElementiumRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetCrystalMine + 1);

        public override double RessourceCostsPerTick => 0;

        public override double Usage => _planet.PlanetCrystalMineUsage;

        public override GrowthRate ConsumptionEnergyRate
            => _consumptionEnergyRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetCrystalMine);

        public override GrowthRate ConsumptionManaRate
            => _consumptionManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetCrystalMine);

        public override GrowthRate ConsupmtionAntimatter
            => _consupmtionAntimatter
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetCrystalMine);

        public override GrowthRate ConsumptionElementium
            => _consumptionElementium
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetCrystalMine);

        #endregion


        #region Methods ---------------------------------------------------------------------------

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
        {
            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.ToString(Constants.DEBUG_TIME_FORMAT)}: "
                + $"A new Crystal Mine object has been created on planet {planet.PlanetName}.");
        }

        #endregion
    }
}
