using Galaxxis.Core.Classes.Buildings.BaseClasses;
using Galaxxis.Core.Provider;
using Galaxxis.Core.Static;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Events.BuildEvents;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxxis.Core.Classes.Buildings.ProducingBuildings
{
    public class DefenseSystem : BaseProducingBuilding
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

        public override bool Build(out string info)
        {
            return base.Build<BuildDefenseSystemEvent>(out info);
        }

        public DefenseSystem(Planet planet) : base(planet)
        {
            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.ToString(_timeFormat)}: "
                + "A new DefenseSystem object has been created.");
        }
    }
}
