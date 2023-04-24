using Galaxxis.Data.Enums;

namespace Galaxxis.Core.Classes.Ships
{
    public abstract class BaseShip : IShip
    {
        /// <summary>
        /// location of the ship's home planet
        /// </summary>
        public int PlanetId { get; }
        public ShipTypeEnum ShipType { get; }
        public TacticalEnum Tactical { get; }

        public abstract int CostTools { get; }
        public abstract int CostCrystals { get; }
        public abstract int CostAntimatter { get; }
        public abstract int CostElementium { get; }

        public BaseShip(int planetId, ShipTypeEnum shipType, TacticalEnum tactical)
        {
            this.PlanetId = planetId;
            this.ShipType = shipType;
            this.Tactical = tactical;
        }
    }
}
