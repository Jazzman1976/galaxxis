using Galaxxis.Data.Enums;

namespace Galaxxis.Core.Classes.Defense
{
    public abstract class BaseDefense : IDefense
    {
        public int PlanetId { get; }
        public DefenseTypeEnum DefenseType { get; }
        public TacticalEnum Tactical { get; }

        public abstract int CostTools { get; }
        public abstract int CostCrystals { get; }
        public abstract int CostAntimatter { get; }
        public abstract int CostElementium { get; }

        public BaseDefense(int planetId, DefenseTypeEnum defenseType, TacticalEnum tactical)
        {
            this.PlanetId = planetId;
            this.DefenseType = defenseType;
            this.Tactical = tactical;
        }
    }
}
