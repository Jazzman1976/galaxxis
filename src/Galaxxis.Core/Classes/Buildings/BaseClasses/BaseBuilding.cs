using Galaxxis.Core.Classes.Buildings.Interfaces;
using Galaxxis.Core.Provider;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Events.BuildEvents;
using Galaxxis.Data.Models;
using System.Linq;

namespace Galaxxis.Core.Classes.Buildings.BaseClasses
{
    public abstract class BaseBuilding<TBuildEvent>(Planet planet) : IBuilding
        where TBuildEvent : BaseBuildEvent
    {
        protected Planet _planet = planet;
        protected User _owner = planet.PlanetUser ?? UserProvider.GetUser(planet.PlanetUserId);

        private double[] _buildCosts;

        public abstract GrowthRate BuildCostOreRate { get; }
        public abstract GrowthRate BuildCostManaRate { get; }
        public abstract GrowthRate BuildCostCrystalsRate { get; }
        public abstract GrowthRate BuildCostElementiumRate { get; }
        public abstract GrowthRate BuildTimeRate { get; }
        
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
        
        public bool CanUpgradeToNextLevel
            => BuildCosts[0] <= _planet.PlanetOre
            && BuildCosts[1] <= _planet.PlanetMana
            && BuildCosts[2] <= _planet.PlanetCrystals
            && BuildCosts[3] <= _planet.PlanetElementium
            && EventProvider
                .GetEvents(_planet.PlanetId, EventLocationEnum.Planet, EventTypeEnum.Build)
                .Count == 0;

        public bool IsUnderConstruction
        {
            get =>
                EventProvider
                    .GetEvents(_planet.PlanetId, EventLocationEnum.Planet, EventTypeEnum.Build)
                    .Any(e => e is TBuildEvent);
            set { }
        }

        /// <summary>
        /// Use this in the Build method of derived classes to build the next building level 
        /// using the correct event name.
        /// </summary>
        public bool Build(out string info)
        {
            return BuildingsHelper.BuildOnPlanet<TBuildEvent>(
                _planet, BuildCosts, BuildTimeRate.ValueLong, out info);
        }
    }
}
