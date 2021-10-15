using Galaxxis.Data.Enums;

namespace Galaxxis.Core.Classes.Ships
{
    public class Explorer : BaseShip
    {
        public override int CostTools => 450;

        public override int CostCrystals => Tactical == TacticalEnum.Offensive ? 450 : 900;

        public override int CostAntimatter => Tactical == TacticalEnum.Offensive ? 900 : 450;

        public override int CostElementium => 150;

        public Explorer(int planetId, ShipTypeEnum shipType, TacticalEnum tactical)
            : base(planetId, shipType, tactical)
        { }
    }
}
