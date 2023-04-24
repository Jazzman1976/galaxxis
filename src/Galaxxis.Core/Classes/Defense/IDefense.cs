using Galaxxis.Data.Enums;

namespace Galaxxis.Core.Classes.Defense
{
    public interface IDefense
    {
        public int PlanetId { get; }
        public DefenseTypeEnum DefenseType { get; }
        public TacticalEnum Tactical { get; }

        public int CostTools { get; }
        public int CostCrystals { get; }
        public int CostAntimatter { get; }
        public int CostElementium { get; }
    }
}
