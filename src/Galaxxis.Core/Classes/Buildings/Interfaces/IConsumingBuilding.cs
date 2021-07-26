using Galaxxis.Core.Classes.Buildings.Enums;
using Galaxxis.Core.Engine;

namespace Galaxxis.Core.Classes.Buildings.Interfaces
{
    public interface IConsumingBuilding
    {
        public GrowthRate ConsumptionEnergyRate { get; }
        public GrowthRate ConsumptionManaRate { get; }
        public GrowthRate ConsupmtionAntimatter { get; }
        public GrowthRate ConsumptionElementium { get; }
        public EnumConsumingType ConsumingType {get;}
    }
}
