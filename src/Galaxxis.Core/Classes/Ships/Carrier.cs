using Galaxxis.Core.Extensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Classes.Ships
{
    public class Carrier : BaseShip
    {
        public override int CostTools => 450;

        public override int CostCrystals => Tactical == TacticalEnum.Offensive ? 450 : 900;

        public override int CostAntimatter => Tactical == TacticalEnum.Offensive ? 900 : 450;

        public override int CostElementium => 150;

        public Carrier(Planet planet)
            : base(
                  planet.PlanetId,
                  ShipTypeEnum.Carrier,
                  planet.PlanetUser.GetTacticalEnum())
        { }
    }
}
