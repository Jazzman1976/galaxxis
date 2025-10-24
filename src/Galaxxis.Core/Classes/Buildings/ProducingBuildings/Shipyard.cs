using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Classes.Ships;
using Galaxxis.Core.Engine;
using Galaxxis.Core.Provider;
using Galaxxis.Core.Static;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Events;
using Galaxxis.Data.Events.BuildEvents;
using Galaxxis.Data.EventTypeData;
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
            return base.Build<BuildShipyardEvent>(out info);
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

            // init a new explorer ship object on the shipyard's planet
            Explorer explorer = new(_planet);

            // check costs
            if (explorer.CostTools > _planet.PlanetTools)
            {
                info = "Not enough tools!";
                return false;
            }
            if (explorer.CostCrystals > _planet.PlanetCrystals)
            {
                info = "Not enough crystals!";
                return false;
            }
            if (explorer.CostAntimatter > _planet.PlanetAntimatter)
            {
                info = "Not enough antimatter!";
                return false;
            }
            if (explorer.CostElementium > _planet.PlanetElementium)
            {
                info = "Not enough elementium!";
                return false;
            }

            // init produce event type data
            ProduceEventTypeData data = new ProduceEventTypeData(
                explorer.CostTools,
                explorer.CostCrystals,
                explorer.CostAntimatter,
                explorer.CostElementium);

            // init produce ship explorer event
            ProduceShipExplorerEvent @event = new ProduceShipExplorerEvent(
                EventLocationEnum.Planet,
                _planet.PlanetId,
                100,
                data);

            // enqueue event
            Processor.Instance.EventQueue.Enqueue(@event);

            info = "Producing of a new explorer ship has been started.";
            return true;
        }

        public bool ProduceShipCarrier(out string info)
        {
            if (EventProvider
                .GetEvents(_planet.PlanetId, EventLocationEnum.Planet, EventTypeEnum.Produce)
                .Any())
            {
                info = "Producing is still in progress!";
                return false;
            }

            Carrier carrier = new Carrier(_planet); 

            if (carrier.CostTools > _planet.PlanetTools)
            {
                info = "Not enough tools!";
                return false;
            }
            if (carrier.CostCrystals > _planet.PlanetCrystals)
            {
                info = "Not enough crystals!";
                return false;
            }
            if (carrier.CostAntimatter > _planet.PlanetAntimatter)
            {
                info = "Not enough antimatter!";
                return false;
            }
            if (carrier.CostElementium > _planet.PlanetElementium)
            {
                info = "Not enough elementium!";
                return false;
            }

            // get event
            ProduceEventTypeData data = new ProduceEventTypeData(
                carrier.CostTools,
                carrier.CostCrystals,
                carrier.CostAntimatter,
                carrier.CostElementium);
            ProduceShipCarrierEvent @event = new ProduceShipCarrierEvent(
                EventLocationEnum.Planet,
                _planet.PlanetId,
                100,
                data);

            // enqueue event
            Processor.Instance.EventQueue.Enqueue(@event);

            info = "Producing of a new carrier ship has been started.";
            return true;
        }

        public Shipyard(Planet planet) : base(planet) 
        {
            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.ToString(Static.Constants.DEBUG_TIME_FORMAT)}: "
                + "A new Shipyard object has been created.");
        }
    }
}
