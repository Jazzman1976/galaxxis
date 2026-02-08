using Galaxxis.Core.Classes.Buildings.Interfaces;
using Galaxxis.Data.Events.BuildEvents;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Classes.Buildings.BaseClasses
{
    public abstract class BaseProducingBuilding<TBuildEvent> 
        : BaseBuilding<TBuildEvent>, IProducingBuilding, IConsumingBuilding 
            where TBuildEvent : BaseBuildEvent
    {
        protected bool? _canBuild;
        protected GrowthRate _ressourceGrowthRate;
        protected GrowthRate _buildTimeRate;
        protected GrowthRate _buildCostOreRate;
        protected GrowthRate _buildCostManaRate;
        protected GrowthRate _buildCostCrystalsRate;
        protected GrowthRate _buildCostElementiumRate;
        protected double _incomePerDay = -1;
        protected double _incomePerTick = -1;
        protected double? _costsPerTick;
        protected GrowthRate _consumptionEnergyRate;
        protected GrowthRate _consumptionManaRate;
        protected GrowthRate _consupmtionAntimatter;
        protected GrowthRate _consumptionElementium;

        protected BaseProducingBuilding(Planet planet) : base(planet)
        {
        }

        public abstract bool IsProducing { get; }

        public abstract GrowthRate ConsumptionEnergyRate { get; }

        public abstract GrowthRate ConsumptionManaRate { get; }

        public abstract GrowthRate ConsupmtionAntimatter { get; }

        public abstract GrowthRate ConsumptionElementium { get; }

        
    }
}
