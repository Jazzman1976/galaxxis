using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Classes.Defense;
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
    public class DefenseSystem : BaseProducingBuilding<BuildDefenseSystemEvent>
    {
        public override GrowthRate BuildTimeRate
            => _buildTimeRate
            ??= new(
                    Constants.BASEVALUE_VERY_MUCH,
                    Constants.FACTOR_BUILD_TIME,
                    _planet.PlanetDefenseSystem + 1);

        public override GrowthRate BuildCostOreRate
            => _buildCostOreRate
            ??= new(
                    Constants.BASEVALUE_NORMAL,
                    Constants.FACTOR_BUILD,
                    _planet.PlanetDefenseSystem + 1);

        public override GrowthRate BuildCostManaRate
            => _buildCostManaRate
            ??= new(
                    Constants.BASEVALUE_NORMAL,
                    Constants.FACTOR_BUILD,
                    _planet.PlanetDefenseSystem + 1);

        public override GrowthRate BuildCostCrystalsRate
            => _buildCostCrystalsRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW,
                    Constants.FACTOR_BUILD,
                    _planet.PlanetDefenseSystem + 1);

        public override GrowthRate BuildCostElementiumRate
            => _buildCostElementiumRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW,
                    Constants.FACTOR_BUILD,
                    _planet.PlanetDefenseSystem + 1);

        public override bool IsProducing
            => !EventProvider
                .GetEvents(_planet.PlanetId, EventLocationEnum.Planet, EventTypeEnum.Produce)
                .Any();

        public override GrowthRate ConsumptionEnergyRate
            => _consumptionEnergyRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetDefenseSystem);

        public override GrowthRate ConsumptionManaRate
            => _consumptionManaRate
            ??= new GrowthRate(
                    Constants.BASEVALUE_MUCH,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetDefenseSystem);

        public override GrowthRate ConsupmtionAntimatter
            => _consupmtionAntimatter
            ??= new GrowthRate(
                    Constants.BASEVALUE_FEW,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetDefenseSystem);

        public override GrowthRate ConsumptionElementium
            => _consumptionElementium
            ??= new GrowthRate(
                    Constants.BASEVALUE_NORMAL,
                    Constants.FACTOR_SUPPLY,
                    _planet.PlanetDefenseSystem);

        public bool ProduceDefenseLightManualCannon(out string info)
        {
            if (EventProvider
                .GetEvents(_planet.PlanetId, EventLocationEnum.Planet, EventTypeEnum.Produce)
                .Any())
            {
                info = "Producing is still in progress!";
                return false;
            }

            // init new light manual cannon object on defense system's planet
            LightManualCannon lightManualCannon = new(_planet);

            // check costs
            if (lightManualCannon.CostTools > _planet.PlanetTools)
            {
                info = "Not enough tools!";
                return false;
            }
            if (lightManualCannon.CostCrystals > _planet.PlanetCrystals)
            {
                info = "Not enough crystals!";
                return false;
            }
            if (lightManualCannon.CostAntimatter > _planet.PlanetAntimatter)
            {
                info = "Not enough antimatter!";
                return false;
            }
            if (lightManualCannon.CostElementium > _planet.PlanetElementium)
            {
                info = "Not enough elementium!";
                return false;
            }

            // init produce event type data
            ProduceEventTypeData data = new ProduceEventTypeData(
                lightManualCannon.CostTools,
                lightManualCannon.CostCrystals,
                lightManualCannon.CostAntimatter,
                lightManualCannon.CostElementium);

            // init produce light manual cannon event
            ProduceLightManualCannonEvent @event = new ProduceLightManualCannonEvent(
                EventLocationEnum.Planet,
                _planet.PlanetId,
                100,
                data);

            // enqueue event
            Processor.Instance.EventQueue.Enqueue(@event);

            // set out variable and exit
            info = "Producing of a new light manual cannon defense has been started.";
            return true;
        }

        public DefenseSystem(Planet planet) : base(planet)
        {
            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.ToString(Static.Constants.DEBUG_TIME_FORMAT)}: "
                + "A new DefenseSystem object has been created.");
        }
    }
}
