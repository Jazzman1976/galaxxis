using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Classes.Buildings.Enums;
using Galaxxis.Core.Engine;
using Galaxxis.Core.Extensions.PlanetExtensions;
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
                    Constants.BASEVALUE_CAT2_RESSOURCE, 
                    Constants.FACTOR_RESSOURCES, 
                    _planet.PlanetMageAcademy, 
                    Constants.BASEINCOME_CAT2_RESSOURCE);

        public override GrowthRate BuildTimeRate 
            => _buildTimeRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_MUCH, 
                    Constants.FACTOR_BUILD_TIME, 
                    _planet.PlanetMageAcademy + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetMageAcademy + 1);

        public override GrowthRate BuildCostManaRate
            => _buildCostManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetMageAcademy + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetMageAcademy + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetMageAcademy + 1);

        public override double RessourceCostsPerTick
        {
            get
            {
                return _planet.GetOreMine().ConsumptionManaRate.GetValuePerTick()
                    * _planet.PlanetOreMineUsage
                + _planet.GetPowerSupply().ConsumptionManaRate.GetValuePerTick()
                    * _planet.PlanetPowerSupplyUsage
                + _planet.GetFactory().ConsumptionManaRate.GetValuePerTick()
                    * _planet.PlanetFactoryUsage
                + _planet.GetMageAcademy().ConsumptionManaRate.GetValuePerTick()
                    * _planet.PlanetMageAcademyUsage
                + _planet.GetCrystalMine().ConsumptionManaRate.GetValuePerTick()
                    * _planet.PlanetCrystalMineUsage
                + _planet.GetNeutronAccelerator().ConsumptionManaRate.GetValuePerTick()
                    * _planet.PlanetNeutronAcceleratorUsage
                + _planet.GetElementiumSynthesizer().ConsumptionManaRate.GetValuePerTick()
                    * _planet.PlanetElementiumSynthesizerUsage;
            }
        }

        public override double Usage => _planet.PlanetMageAcademyUsage;

        public override GrowthRate ConsumptionEnergyRate
            => _consumptionEnergyRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetMageAcademy);

        public override GrowthRate ConsumptionManaRate
            => _consumptionManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetMageAcademy);

        public override GrowthRate ConsupmtionAntimatter
            => _consupmtionAntimatter
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetMageAcademy);

        public override GrowthRate ConsumptionElementium
            => _consumptionElementium
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetMageAcademy);

        public override EnumConsumingType ConsumingType => EnumConsumingType.continuous;

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public MageAcademy(Planet planet) : base(planet)
        {
            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.ToString(_timeFormat)}: "
                + "A new Mage Academy object has been created.");
        }

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

        public override bool ChangeUsage(double newValue, out string info)
        {
            return base.ChangeUsage(EventNameEnum.ChangeMageAcademyUsage, newValue, out info);
        }

        #endregion
    }
}
