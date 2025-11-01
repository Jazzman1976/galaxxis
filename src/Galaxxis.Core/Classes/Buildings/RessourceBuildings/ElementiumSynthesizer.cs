using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Engine;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Core.Provider;
using Galaxxis.Core.Static;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Events.BuildEvents;
using Galaxxis.Data.Models;
using System;
using System.Linq;

namespace Galaxxis.Core.Classes.Buildings.RessourceBuildings
{
    public class ElementiumSynthesizer : BaseRessourceBuilding<BuildElementiumSynthesizerEvent>
    {
        #region Properties ------------------------------------------------------------------------

        public override GrowthRate RessourceGrowthRate
            => _ressourceGrowthRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_CAT3_RESSOURCE,
                    Constants.FACTOR_RESSOURCES,
                    _planet.PlanetElementiumSynthesizer,
                    Constants.BASEINCOME_CAT3_RESSOURCE);

        public override GrowthRate BuildTimeRate
            => _buildTimeRate
            ??= new GrowthRate(
                Constants.BASEVALUE_VERY_MUCH,
                Constants.FACTOR_BUILD_TIME,
                _planet.PlanetElementiumSynthesizer + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_MUCH, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetElementiumSynthesizer + 1);

        public override GrowthRate BuildCostManaRate
            => _buildCostManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_MUCH, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetElementiumSynthesizer + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_MUCH, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetElementiumSynthesizer + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW, 
                    Constants.FACTOR_BUILD, 
                    _planet.PlanetElementiumSynthesizer + 1);

        public override double RessourceCostsPerTick
        {
            get
            {
                return _planet.GetOreMine().ConsumptionElementium.GetValuePerTick()
                    * _planet.PlanetOreMineUsage
                + _planet.GetPowerSupply().ConsumptionElementium.GetValuePerTick()
                    * _planet.PlanetPowerSupplyUsage
                + _planet.GetFactory().ConsumptionElementium.GetValuePerTick()
                    * _planet.PlanetFactoryUsage
                + _planet.GetMageAcademy().ConsumptionElementium.GetValuePerTick()
                    * _planet.PlanetMageAcademyUsage
                + _planet.GetCrystalMine().ConsumptionElementium.GetValuePerTick()
                    * _planet.PlanetCrystalMineUsage
                + _planet.GetNeutronAccelerator().ConsumptionElementium.GetValuePerTick()
                    * _planet.PlanetNeutronAcceleratorUsage
                + _planet.GetElementiumSynthesizer().ConsumptionElementium.GetValuePerTick()
                    * _planet.PlanetElementiumSynthesizerUsage;
            }
        }

        public override double Usage => _planet.PlanetElementiumSynthesizerUsage;

        public override GrowthRate ConsumptionEnergyRate
            => _consumptionEnergyRate
            ??= new GrowthRate(
                Constants.BASEVALUE_NORMAL,
                Constants.FACTOR_SUPPLY,
                _planet.PlanetElementiumSynthesizer);

        public override GrowthRate ConsumptionManaRate
            => _consumptionManaRate
            ??= new GrowthRate(
                Constants.BASEVALUE_NORMAL,
                Constants.FACTOR_SUPPLY,
                _planet.PlanetElementiumSynthesizer);

        public override GrowthRate ConsupmtionAntimatter
            => _consupmtionAntimatter
            ??= new GrowthRate(
                Constants.BASEVALUE_NORMAL,
                Constants.FACTOR_SUPPLY,
                _planet.PlanetElementiumSynthesizer);

        public override GrowthRate ConsumptionElementium
            => _consumptionElementium
            ??= new GrowthRate(
                0,
                Constants.FACTOR_SUPPLY,
                _planet.PlanetElementiumSynthesizer);

        #endregion


        #region Constructors ----------------------------------------------------------------------

        public ElementiumSynthesizer(Planet planet) : base(planet)
        {
            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.ToString(Static.Constants.DEBUG_TIME_FORMAT)}: "
                + $"A new Elementium Synthesizer object has been created on planet {planet.PlanetName}.");
        }

        #endregion


        #region Methods ---------------------------------------------------------------------------

        public override double ApplyRaceFactor(double value)
        {
            return value;
        }

        public override bool ChangeUsage(double newValue, out string info)
        {
            return base.ChangeUsage(
                EventNameEnum.ChangeElementiumSynthesizerUsage, newValue, out info);
        }

        #endregion
    }
}
