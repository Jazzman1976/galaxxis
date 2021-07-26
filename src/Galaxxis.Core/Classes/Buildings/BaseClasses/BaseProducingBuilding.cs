using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Classes.Buildings.Interfaces;
using Galaxxis.Core.Engine;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Classes.Buildings.BaseClasses
{
    public abstract class BaseProducingBuilding : IBuilding, IProducingBuilding
    {
        protected Planet _planet;
        protected User _owner;
        protected double[] _buildCosts;
        protected double _costsPerTick = -1;

        public abstract GrowthRate BuildTimeRate { get; }

        public abstract GrowthRate BuildCostOreRate { get; }

        public abstract GrowthRate BuildCostManaRate { get; }

        public abstract GrowthRate BuildCostCrystalsRate { get; }

        public abstract GrowthRate BuildCostElementiumRate { get; }

        public double[] BuildCosts
            => _buildCosts
            ??= new double[]
                {
                    BuildCostOreRate.Value,
                    BuildCostManaRate.Value,
                    BuildCostCrystalsRate.Value,
                    BuildCostElementiumRate.Value,
                    BuildTimeRate.Value
                };

        public bool CanBuild
            => BuildCosts[0] <= _planet.PlanetOre
            && BuildCosts[1] <= _planet.PlanetMana
            && BuildCosts[2] <= _planet.PlanetCrystals
            && BuildCosts[3] <= _planet.PlanetElementium
            && !EventProvider
                .GetEvents(_planet.PlanetId, EventLocationEnum.Planet, EventTypeEnum.Build)
                .Any();

        public bool IsProducing => throw new System.NotImplementedException();

        public abstract bool Build(out string info);
    }
}
