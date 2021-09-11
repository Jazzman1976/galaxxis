using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Classes.Buildings.Enums;
using Galaxxis.Core.Engine;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;

namespace Galaxxis.Core.Classes.Buildings.RessourceBuildings
{
    public class NeutronAccelerator : BaseRessourceBuilding
    {
        #region Properties ------------------------------------------------------------------------

        public override GrowthRate RessourceGrowthRate
            => _ressourceGrowthRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_CAT2_RESSOURCE,
                    Constants.FACTOR_RESSOURCES,
                    _planet.PlanetNeutronAccelerator,
                    Constants.BASEINCOME_CAT2_RESSOURCE);

        public override GrowthRate BuildTimeRate
            => _buildTimeRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_MUCH,
                    Constants.FACTOR_BUILD_TIME,
                    _planet.PlanetNeutronAccelerator + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_MUCH, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetNeutronAccelerator + 1);

        public override GrowthRate BuildCostManaRate
            => _buildCostManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetNeutronAccelerator + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetNeutronAccelerator + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetNeutronAccelerator + 1);

        public override double RessourceCostsPerTick
        {
            get
            {
                return _planet.GetOreMine().ConsupmtionAntimatter.GetValuePerTick()
                    * _planet.PlanetOreMineUsage
                + _planet.GetPowerSupply().ConsupmtionAntimatter.GetValuePerTick()
                    * _planet.PlanetPowerSupplyUsage
                + _planet.GetFactory().ConsupmtionAntimatter.GetValuePerTick()
                    * _planet.PlanetFactoryUsage
                + _planet.GetMageAcademy().ConsupmtionAntimatter.GetValuePerTick()
                    * _planet.PlanetMageAcademyUsage
                + _planet.GetCrystalMine().ConsupmtionAntimatter.GetValuePerTick()
                    * _planet.PlanetCrystalMineUsage
                + _planet.GetNeutronAccelerator().ConsupmtionAntimatter.GetValuePerTick()
                    * _planet.PlanetNeutronAcceleratorUsage
                + _planet.GetElementiumSynthesizer().ConsupmtionAntimatter.GetValuePerTick()
                    * _planet.PlanetElementiumSynthesizerUsage;
            }
        }

        public override double Usage => _planet.PlanetNeutronAcceleratorUsage;

        public override GrowthRate ConsumptionEnergyRate
            => _consumptionEnergyRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetNeutronAccelerator);

        public override GrowthRate ConsumptionManaRate
            => _consumptionManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetNeutronAccelerator);

        public override GrowthRate ConsupmtionAntimatter
            => _consupmtionAntimatter
            ??= new GrowthRate(
                    0,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetNeutronAccelerator);

        public override GrowthRate ConsumptionElementium
            => _consumptionElementium
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetNeutronAccelerator);

        #endregion


        #region Methods ---------------------------------------------------------------------------

        public override bool Build(out string info)
        {
            return base.Build(EventNameEnum.BuildNeutronAccelerator, out info);
        }

        public override double ApplyRaceFactor(double value)
        {
            switch (Enum.Parse(typeof(UserRacesEnum), _owner.UserRace))
            {
                case UserRacesEnum.Human:
                case UserRacesEnum.Troll:
                    value *= 1.5;
                    break;

                case UserRacesEnum.Dwarf:
                case UserRacesEnum.Goblin:
                    value *= 0.75;
                    break;
            }
            return value;
        }

        public override bool ChangeUsage(double newValue, out string info)
        {
            return 
                base.ChangeUsage(EventNameEnum.ChangeNeutronAcceleratorUsage, newValue, out info);
        }

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public NeutronAccelerator(Planet planet) : base(planet)
        {
            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.ToString(_timeFormat)}: "
                + "A new Neutron Accelerator object has been created.");
        }

        #endregion
    }
}
