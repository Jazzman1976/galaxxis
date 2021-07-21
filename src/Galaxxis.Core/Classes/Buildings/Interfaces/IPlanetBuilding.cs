using Galaxxis.Core.Engine;

namespace Galaxxis.Core.Classes.Buildings.Interfaces
{
    public interface IPlanetBuilding
    {
        public GrowthRate RessourceGrowthRate { get; }
        public GrowthRate BuildTimeRate { get; }
        public GrowthRate BuildCostOreRate { get; }
        public GrowthRate BuildCostManaRate { get; }
        public GrowthRate BuildCostCrystalsRate { get; }
        public double IncomePerDay { get; }
        public double IncomePerTick { get; }
        public double AmountPerTick { get; }

        /// <summary>
        /// Amount of ressource decreased each tick due to consuption in other building's 
        /// production
        /// </summary>
        public double CostsPerTick { get; }

        /// <summary>
        /// Costs of the next building level. 
        /// 0: ore, 1: mana, 2: crystals, 3: ticks
        /// </summary>
        public double[] BuildCosts { get; }

        /// <summary>
        /// Checks if all conditions to build the next level are matched.
        /// </summary>
        public bool CanBuild { get; }

        /// <summary>
        /// Start construction of the building's next level.
        /// </summary>
        public bool Build(out string info);
    }
}
