using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System;
using System.Linq;

namespace Galaxxis.Core.Classes.Buildings.ProducingBuildings
{
    public class Shipyard : BaseProducingBuilding
    {
        public override GrowthRate BuildTimeRate
            => _buildTimeRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_VERY_MUCH,
                    Constants.FACTOR_BUILD_TIME,
                    _planet.PlanetShipyard + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL,
                    Constants.FACTOR_BUILD,
                    _planet.PlanetShipyard + 1);

        public override GrowthRate BuildCostManaRate
            => _buildCostManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW,
                    Constants.FACTOR_BUILD,
                    _planet.PlanetShipyard + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL,
                    Constants.FACTOR_BUILD,
                    _planet.PlanetShipyard + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW,
                    Constants.FACTOR_BUILD,
                    _planet.PlanetShipyard + 1);

        public override bool IsProducing
            => !EventProvider
                .GetEvents(_planet.PlanetId, EventLocationEnum.Planet, EventTypeEnum.Produce)
                .Any(e => e.EventName.Trim().Equals(EventNameEnum.ProduceShip));

        public override GrowthRate ConsumptionEnergyRate
            => _consumptionEnergyRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetShipyard);

        public override GrowthRate ConsumptionManaRate
            => _consumptionManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetShipyard);

        public override GrowthRate ConsupmtionAntimatter
            => _consupmtionAntimatter
            ??= new GrowthRate(
                    Constants.BASEVALUE_MUCH,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetShipyard);

        public override GrowthRate ConsumptionElementium
            => _consumptionElementium
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetShipyard);

        public override bool Build(out string info)
        {
            return base.Build(EventNameEnum.BuildShipyard, out info);
        }

        public Shipyard(Planet planet) : base(planet) 
        {
            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.ToString(_timeFormat)}: "
                + "A new Shipyard object has been created.");
        }
    }
}
