using Galaxxis.Core.Engine;

namespace Galaxxis.Core.Classes.Buildings.Interfaces
{
    public interface IRessourceBuilding
    {
        public GrowthRate RessourceGrowthRate { get; }
        public double IncomePerDay { get; }
        public double IncomePerTick { get; }
        public double AmountPerTick { get; }

        /// <summary>
        /// Amount of ressource decreased each tick due to consuption in other building's 
        /// production
        /// </summary>
        public double RessourceCostsPerTick { get; }

        /// <summary>
        /// Any change caused by planet owner race. Is used to calculate incomes.
        /// </summary>
        public double ApplyRaceFactor(double value);
    }
}
