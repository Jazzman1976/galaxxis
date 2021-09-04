using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Engine;
using Galaxxis.Core.Extensions;
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
                .Any();

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

        public bool ProduceShipExplorer(out string info)
        {
            if (EventProvider
                .GetEvents(_planet.PlanetId, EventLocationEnum.Planet, EventTypeEnum.Produce)
                .Any())
            {
                info = "Producing is still in progress!";
                return false;
            }

            double[] produceCosts = new double[] 
            {
                500,
                _owner.IsOffensive() ? 750 : 1500,
                _owner.IsOffensive() ? 1500 : 750,
                250
            };

            if (produceCosts[0] > _planet.PlanetTools)
            {
                info = "Not enough tools!";
                return false;
            }
            if (produceCosts[1] > _planet.PlanetCrystals)
            {
                info = "Not enough crystals!";
                return false;
            }
            if (produceCosts[2] > _planet.PlanetAntimatter)
            {
                info = "Not enough antimatter!";
                return false;
            }
            if (produceCosts[3] > _planet.PlanetElementium)
            {
                info = "Not enough elementium!";
                return false;
            }

            Processor.Instance.EventQueue
                .Enqueue(EventProvider.NewEvent(
                    EventLocationEnum.Planet,
                    _planet.PlanetId,
                    EventNameEnum.ProduceShipExplorer,
                    100,
                    EventTypeEnum.Produce,
                    produceCosts.ToValueString()));

            info = "Producing of a new explorer ship has been started.";
            return true;
        }

        public Shipyard(Planet planet) : base(planet) 
        {
            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.ToString(_timeFormat)}: "
                + "A new Shipyard object has been created.");
        }
    }
}
