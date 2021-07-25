using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Engine;
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
                    Constants.BASEVALUE_CAT1_GROWTH, 
                    Constants.FACTOR_RESSOURCE_GROWTH, 
                    _planet.PlanetPowerSupply, 
                    Constants.BASEINCOME_CAT1_GROWTH);

        public override GrowthRate BuildTimeRate
            => _buildTimeRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_BUILD_TIME, 
                    Constants.FACTOR_BUILD_TIME, 
                    _planet.PlanetPowerSupply + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(15, Constants.FACTOR_BUILD_COST, _planet.PlanetPowerSupply + 1);

        public override GrowthRate BuildCostManaRate 
            => _buildCostManaRate
            ??= new GrowthRate(5, Constants.FACTOR_BUILD_COST, _planet.PlanetPowerSupply + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(5, Constants.FACTOR_BUILD_COST, _planet.PlanetPowerSupply + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(0, Constants.FACTOR_BUILD_COST, _planet.PlanetPowerSupply + 1);

        public override double RessourceCostsPerTick
        {
            get
            {
                if (_costsPerTick == null)
                {
                    _costsPerTick = GetCostsPerTick(new int[]
                    {
                        5, 0, 5,
                        15, 5, 10,
                        15,
                        15, 20, 20,
                        15, 15, 15
                    });
                }
                return _costsPerTick ?? 0;
            }
        }

        public override double Usage => _planet.PlanetPowerSupplyUsage;

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
