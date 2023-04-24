using Galaxxis.Core.Extensions;
using Galaxxis.Data.Enums;
using Galaxxis.Data.Models;

namespace Galaxxis.Core.Classes.Defense
{
    public class LightManualCannon : BaseDefense
    {
        public override int CostTools => 100;
        public override int CostCrystals => Tactical == TacticalEnum.Offensive ? 100 : 200;
        public override int CostAntimatter => Tactical == TacticalEnum.Offensive ? 200 : 100;
        public override int CostElementium => 50;

        public LightManualCannon(Planet planet)
            : base(
                  planet.PlanetId,
                  DefenseTypeEnum.LightManualCannon,
                  planet.PlanetUser.GetTacticalEnum())
        { }
    }
}
