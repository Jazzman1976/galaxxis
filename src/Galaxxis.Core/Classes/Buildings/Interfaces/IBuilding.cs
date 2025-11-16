using Galaxxis.Data.Events.BuildEvents;

namespace Galaxxis.Core.Classes.Buildings.Interfaces
{
    public interface IBuilding<TBuildEvent> where TBuildEvent : BaseBuildEvent
    {
        public GrowthRate BuildTimeRate { get; }
        public GrowthRate BuildCostOreRate { get; }
        public GrowthRate BuildCostManaRate { get; }
        public GrowthRate BuildCostCrystalsRate { get; }
        public GrowthRate BuildCostElementiumRate { get; }

        /// <summary>
        /// Costs of the next building level. 
        /// 0: ore, 1: mana, 2: crystals, 3: elementium, 4: ticks
        /// </summary>
        public double[] BuildCosts { get; }

        /// <summary>
        /// Checks if all conditions to build the next level are matched.
        /// </summary>
        public bool CanUpgradeToNextLevel { get; }

        /// <summary>
        /// Checks if this building is currently under construction.
        /// </summary>
        public bool IsUnderConstruction { get; }

        /// <summary>
        /// Start construction of the building's next level.
        /// </summary>
        public bool Build(out string info);
    }
}
