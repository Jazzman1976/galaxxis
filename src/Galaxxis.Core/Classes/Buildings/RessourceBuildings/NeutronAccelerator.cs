using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Engine;
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
                    Constants.BASEVALUE_CAT2_GROWTH,
                    Constants.FACTOR_RESSOURCE_GROWTH,
                    _planet.PlanetNeutronAccelerator,
                    Constants.BASEINCOME_CAT2_GROWTH);

        public override GrowthRate BuildTimeRate
            => _buildTimeRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_BUILD_TIME,
                    Constants.FACTOR_BUILD_TIME,
                    _planet.PlanetNeutronAccelerator + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(
                    20, Constants.FACTOR_BUILD_COST, _planet.PlanetNeutronAccelerator + 1);

        public override GrowthRate BuildCostManaRate
            => _buildCostManaRate
            ??= new GrowthRate(
                    10, Constants.FACTOR_BUILD_COST, _planet.PlanetNeutronAccelerator + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(
                    10, Constants.FACTOR_BUILD_COST, _planet.PlanetNeutronAccelerator + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(
                    5, Constants.FACTOR_BUILD_COST, _planet.PlanetNeutronAccelerator + 1);

        public override double RessourceCostsPerTick
        {
            get
            {
                if (_costsPerTick == null)
                {
                    _costsPerTick = GetCostsPerTick(new int[]
                    {
                        0, 5, 5,
                        10, 15, 5,
                        15,
                        10, 20, 25,
                        15, 20, 10
                    });
                }
                return _costsPerTick ?? 0;
            }
        }

        public override double Usage => _planet.PlanetNeutronAcceleratorUsage;

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
