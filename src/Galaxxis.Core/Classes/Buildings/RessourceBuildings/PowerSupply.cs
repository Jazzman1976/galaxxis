using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Classes.Buildings.Enums;
using Galaxxis.Core.Engine;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;

namespace Galaxxis.Core.Classes.Buildings.RessourceBuildings
{
    public class PowerSupply : BaseRessourceBuilding
    {
        #region Properties ------------------------------------------------------------------------

        public override GrowthRate RessourceGrowthRate 
            => _ressourceGrowthRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_CAT1_RESSOURCE, 
                    Constants.FACTOR_RESSOURCES, 
                    _planet.PlanetPowerSupply, 
                    Constants.BASEINCOME_CAT1_RESSOURCE);

        public override GrowthRate BuildTimeRate
            => _buildTimeRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_MUCH, 
                    Constants.FACTOR_BUILD_TIME, 
                    _planet.PlanetPowerSupply + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetPowerSupply + 1);

        public override GrowthRate BuildCostManaRate 
            => _buildCostManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetPowerSupply + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetPowerSupply + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(
                    0, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetPowerSupply + 1);

        public override double RessourceCostsPerTick
        {
            get
            {
                return _planet.GetOreMine().ConsumptionEnergyRate.GetValuePerTick()
                    * _planet.PlanetOreMineUsage
                + _planet.GetPowerSupply().ConsumptionEnergyRate.GetValuePerTick()
                    * _planet.PlanetPowerSupplyUsage
                + _planet.GetFactory().ConsumptionEnergyRate.GetValuePerTick()
                    * _planet.PlanetFactoryUsage
                + _planet.GetMageAcademy().ConsumptionEnergyRate.GetValuePerTick()
                    * _planet.PlanetMageAcademyUsage
                + _planet.GetCrystalMine().ConsumptionEnergyRate.GetValuePerTick()
                    * _planet.PlanetCrystalMineUsage
                + _planet.GetNeutronAccelerator().ConsumptionEnergyRate.GetValuePerTick()
                    * _planet.PlanetNeutronAcceleratorUsage
                + _planet.GetElementiumSynthesizer().ConsumptionEnergyRate.GetValuePerTick()
                    * _planet.PlanetElementiumSynthesizerUsage;
            }
        }

        public override double Usage => _planet.PlanetPowerSupplyUsage;

        public override GrowthRate ConsumptionEnergyRate
            => _consumptionEnergyRate
            ??= new GrowthRate(
                    0,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetPowerSupply);

        public override GrowthRate ConsumptionManaRate
            => _consumptionManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetPowerSupply);

        public override GrowthRate ConsupmtionAntimatter
            => _consupmtionAntimatter
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetPowerSupply);

        public override GrowthRate ConsumptionElementium
            => _consumptionElementium
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetPowerSupply);

        #endregion


        #region Methods ---------------------------------------------------------------------------

        public override bool Build(out string info)
        {
            return base.Build(EventNameEnum.BuildPowerSupply, out info);
        }

        public override double ApplyRaceFactor(double value)
        {
            switch (Enum.Parse(typeof(UserRacesEnum), _owner.UserRace))
            {
                case UserRacesEnum.Human:
                case UserRacesEnum.Elf:
                    value *= 1.5;
                    break;
            }
            return value;
        }

        public override bool ChangeUsage(double newValue, out string info)
        {
            return base.ChangeUsage(EventNameEnum.ChangePowerSupplyUsage, newValue, out info);
        }

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public PowerSupply(Planet planet) : base(planet)
        {
            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.ToString(_timeFormat)}: "
                + "A new Power Supply object has been created.");
        }

        #endregion
    }
}
