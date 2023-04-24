using Galaxxis.Core.Extensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Classes.Ships
{
    public class Explorer : BaseShip
    {
        public override int CostTools => 225;

        public override int CostCrystals => Tactical == TacticalEnum.Offensive ? 225 : 450;

        public override int CostAntimatter => Tactical == TacticalEnum.Offensive ? 450 : 225;

        public override int CostElementium => 75;

        public Explorer(Planet planet) 
            : base(
                  planet.PlanetId, 
                  ShipTypeEnum.Explorer, 
                  planet.PlanetUser.GetTacticalEnum()) 
        { }
    }
}
