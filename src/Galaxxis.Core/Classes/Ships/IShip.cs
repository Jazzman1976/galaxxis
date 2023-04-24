using Galaxxis.Data.Enums;

namespace Galaxxis.Core.Classes.Ships
{
    public interface IShip
    {
        /// <summary>
        /// Id of the ship's home planet.
        /// </summary>
        public int PlanetId { get; }
        public ShipTypeEnum ShipType { get; }
        public TacticalEnum Tactical { get; }

        public int CostTools { get; }
        public int CostCrystals { get; }
        public int CostAntimatter { get; }
        public int CostElementium { get; }
    }
}
