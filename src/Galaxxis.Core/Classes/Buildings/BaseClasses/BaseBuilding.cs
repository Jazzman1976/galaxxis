using Galaxxis.Core.Classes.Buildings.Interfaces;
using Galaxxis.Core.Extensions.PlanetExtensions;
using Galaxxis.Core.Provider;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Events.BuildEvents;
using Galaxxis.Data.Models;
using System.Diagnostics;
using System.Linq;

namespace Galaxxis.Core.Classes.Buildings.BaseClasses
{
    public abstract class BaseBuilding<TBuildEvent>(Planet planet) : IBuilding
        where TBuildEvent : BaseBuildEvent
    {
        protected Planet _planet = planet;
        protected User _owner = planet.PlanetUser ?? UserProvider.GetUser(planet.PlanetUserId);

        public abstract GrowthRate BuildCostOreRate { get; }
        public abstract GrowthRate BuildCostManaRate { get; }
        public abstract GrowthRate BuildCostCrystalsRate { get; }
        public abstract GrowthRate BuildCostElementiumRate { get; }
        public abstract GrowthRate BuildTimeRate { get; }
        
        public double[] BuildCosts => [
            BuildCostOreRate.Value,
            BuildCostManaRate.Value,
            BuildCostCrystalsRate.Value,
            BuildCostElementiumRate.Value,
            BuildTimeRate.Value
        ];
        
        public bool CanUpgradeToNextLevel
        {
            get 
            {
                // if build is already in progress, cannot start another one
                if (this.IsUnderConstruction)
                {
                    return false;
                }

                // check resources
                _planet.UpdateRessources();
                return
                    BuildCosts[0] <= _planet.PlanetOre
                    && BuildCosts[1] <= _planet.PlanetMana
                    && BuildCosts[2] <= _planet.PlanetCrystals
                    && BuildCosts[3] <= _planet.PlanetElementium;
            }
            set { }
        }   

        public bool IsUnderConstruction
        {
            get
            {
                // in database
                if (EventProvider
                    .GetEvents(_planet.PlanetId, EventLocationEnum.Planet, EventTypeEnum.Build)
                    .Any()

                // in memory
                || Engine.Processor.Instance.EventQueue.Any(e =>
                    e.EventLocationId == _planet.PlanetId
                    && e.EventLocation == EventLocationEnum.Planet.ToString()
                    && e.EventType == EventTypeEnum.Build.ToString()))
                {
                    return true;
                } 
                
                return false;
            }
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
